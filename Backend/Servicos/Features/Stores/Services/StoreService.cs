using Microsoft.EntityFrameworkCore;
using Renova.Domain.Models;
using Renova.Persistence;
using Renova.Services.Features.Access;
using Renova.Services.Features.Access.Abstractions;
using Renova.Services.Features.Stores.Abstractions;
using Renova.Services.Features.Stores.Contracts;

namespace Renova.Services.Features.Stores.Services;

// Servico principal do modulo de lojas e visao consolidada.
public sealed class StoreService : IStoreService
{
    private static readonly string[] AllowedStatuses = ["ativa", "inativa"];

    private readonly RenovaDbContext _dbContext;
    private readonly IAccessAuditService _auditService;
    private readonly ICurrentRequestContext _currentRequestContext;

    /// <summary>
    /// Inicializa o servico com persistencia, auditoria e contexto autenticado.
    /// </summary>
    public StoreService(
        RenovaDbContext dbContext,
        IAccessAuditService auditService,
        ICurrentRequestContext currentRequestContext)
    {
        _dbContext = dbContext;
        _auditService = auditService;
        _currentRequestContext = currentRequestContext;
    }

    /// <summary>
    /// Lista as lojas acessiveis ao usuario autenticado.
    /// </summary>
    public async Task<IReadOnlyList<StoreResponse>> ListarAcessiveisAsync(CancellationToken cancellationToken = default)
    {
        var usuarioId = EnsureAuthenticatedUser();

        var vinculos = await (
                from usuarioLoja in _dbContext.UsuarioLojas
                join loja in _dbContext.Lojas on usuarioLoja.LojaId equals loja.Id
                where usuarioLoja.UsuarioId == usuarioId
                where usuarioLoja.StatusVinculo == AccessStatusValues.VinculoLoja.Ativo
                where usuarioLoja.DataFim == null || usuarioLoja.DataFim >= DateTimeOffset.UtcNow
                orderby loja.NomeFantasia
                select new
                {
                    UsuarioLoja = usuarioLoja,
                    Loja = loja,
                })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        if (vinculos.Count == 0)
        {
            return Array.Empty<StoreResponse>();
        }

        var lojaIds = vinculos.Select(x => x.Loja.Id).ToHashSet();
        var lojasGerenciaveis = await CarregarLojasGerenciaveisAsync(usuarioId, lojaIds, cancellationToken);

        return vinculos
            .Select(item => MapStore(
                item.Loja,
                item.UsuarioLoja.EhResponsavel,
                lojasGerenciaveis.Contains(item.Loja.Id),
                _currentRequestContext.LojaAtivaId == item.Loja.Id))
            .ToArray();
    }

    /// <summary>
    /// Cria uma nova loja e vincula o criador como responsavel.
    /// </summary>
    public async Task<StoreResponse> CriarAsync(CreateStoreRequest request, CancellationToken cancellationToken = default)
    {
        var usuarioId = EnsureAuthenticatedUser();
        await EnsureCanCreateStoreAsync(usuarioId, cancellationToken);
        ValidateStoreInput(
            request.NomeFantasia,
            request.RazaoSocial,
            request.Documento,
            request.Telefone,
            request.Email,
            request.Logradouro,
            request.Numero,
            request.Bairro,
            request.Cidade,
            request.Uf,
            request.Cep);

        var documento = NormalizeDocument(request.Documento);
        var documentoEmUso = await _dbContext.Lojas.AnyAsync(x => x.Documento == documento, cancellationToken);
        if (documentoEmUso)
        {
            throw new InvalidOperationException("Ja existe uma loja com o documento informado.");
        }

        var loja = new Loja
        {
            Id = Guid.NewGuid(),
            NomeFantasia = request.NomeFantasia.Trim(),
            RazaoSocial = request.RazaoSocial.Trim(),
            Documento = documento,
            Telefone = request.Telefone.Trim(),
            Email = NormalizeEmail(request.Email),
            Logradouro = request.Logradouro.Trim(),
            Numero = request.Numero.Trim(),
            Complemento = request.Complemento.Trim(),
            Bairro = request.Bairro.Trim(),
            Cidade = request.Cidade.Trim(),
            Uf = request.Uf.Trim().ToUpperInvariant(),
            Cep = request.Cep.Trim(),
            StatusLoja = "ativa",
            Ativo = true,
            CriadoPorUsuarioId = usuarioId,
        };

        _dbContext.Lojas.Add(loja);

        var cargoDonoId = await CriarCargosBaseAsync(loja.Id, usuarioId, cancellationToken);

        var usuarioLoja = new UsuarioLoja
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            LojaId = loja.Id,
            StatusVinculo = AccessStatusValues.VinculoLoja.Ativo,
            EhResponsavel = true,
            DataInicio = DateTimeOffset.UtcNow,
            CriadoPorUsuarioId = usuarioId,
        };

        _dbContext.UsuarioLojas.Add(usuarioLoja);
        _dbContext.UsuarioLojaCargos.Add(new UsuarioLojaCargo
        {
            Id = Guid.NewGuid(),
            UsuarioLojaId = usuarioLoja.Id,
            CargoId = cargoDonoId,
            CriadoPorUsuarioId = usuarioId,
        });

        await VincularLojaNaSessaoAtualAsync(loja.Id, usuarioId, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.RegistrarAuditoriaAsync(
            loja.Id,
            "loja",
            loja.Id,
            "criada",
            null,
            new { loja.NomeFantasia, loja.Documento, loja.StatusLoja },
            cancellationToken);

        return MapStore(loja, true, true, _currentRequestContext.LojaAtivaId == loja.Id || _currentRequestContext.LojaAtivaId is null);
    }

    /// <summary>
    /// Atualiza o cadastro principal de uma loja gerenciavel pelo usuario.
    /// </summary>
    public async Task<StoreResponse> AtualizarAsync(Guid lojaId, UpdateStoreRequest request, CancellationToken cancellationToken = default)
    {
        var usuarioId = EnsureAuthenticatedUser();
        await EnsureCanManageStoreAsync(usuarioId, lojaId, cancellationToken);
        ValidateStoreInput(
            request.NomeFantasia,
            request.RazaoSocial,
            request.Documento,
            request.Telefone,
            request.Email,
            request.Logradouro,
            request.Numero,
            request.Bairro,
            request.Cidade,
            request.Uf,
            request.Cep);

        var loja = await _dbContext.Lojas.FirstOrDefaultAsync(x => x.Id == lojaId, cancellationToken)
            ?? throw new InvalidOperationException("Loja nao encontrada.");

        var documento = NormalizeDocument(request.Documento);
        var documentoEmUso = await _dbContext.Lojas.AnyAsync(
            x => x.Id != lojaId && x.Documento == documento,
            cancellationToken);

        if (documentoEmUso)
        {
            throw new InvalidOperationException("Ja existe uma loja com o documento informado.");
        }

        var statusLoja = NormalizeStatus(request.StatusLoja);
        var antes = new
        {
            loja.NomeFantasia,
            loja.RazaoSocial,
            loja.Documento,
            loja.Telefone,
            loja.Email,
            loja.StatusLoja,
            loja.Ativo,
        };

        loja.NomeFantasia = request.NomeFantasia.Trim();
        loja.RazaoSocial = request.RazaoSocial.Trim();
        loja.Documento = documento;
        loja.Telefone = request.Telefone.Trim();
        loja.Email = NormalizeEmail(request.Email);
        loja.Logradouro = request.Logradouro.Trim();
        loja.Numero = request.Numero.Trim();
        loja.Complemento = request.Complemento.Trim();
        loja.Bairro = request.Bairro.Trim();
        loja.Cidade = request.Cidade.Trim();
        loja.Uf = request.Uf.Trim().ToUpperInvariant();
        loja.Cep = request.Cep.Trim();
        loja.StatusLoja = statusLoja;
        loja.Ativo = string.Equals(statusLoja, "ativa", StringComparison.OrdinalIgnoreCase);
        loja.InativadoEm = loja.Ativo ? null : DateTimeOffset.UtcNow;
        loja.AtualizadoEm = DateTimeOffset.UtcNow;
        loja.AtualizadoPorUsuarioId = usuarioId;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.RegistrarAuditoriaAsync(
            loja.Id,
            "loja",
            loja.Id,
            "atualizada",
            antes,
            new
            {
                loja.NomeFantasia,
                loja.RazaoSocial,
                loja.Documento,
                loja.Telefone,
                loja.Email,
                loja.StatusLoja,
                loja.Ativo,
            },
            cancellationToken);

        var responsavel = await IsResponsibleAsync(usuarioId, loja.Id, cancellationToken);
        return MapStore(loja, responsavel, true, _currentRequestContext.LojaAtivaId == loja.Id);
    }

    /// <summary>
    /// Garante que a requisicao possui usuario autenticado.
    /// </summary>
    private Guid EnsureAuthenticatedUser()
    {
        return _currentRequestContext.UsuarioId
            ?? throw new InvalidOperationException("Usuario autenticado nao encontrado.");
    }

    /// <summary>
    /// Permite criar a primeira loja sem permissao previa.
    /// </summary>
    private async Task EnsureCanCreateStoreAsync(Guid usuarioId, CancellationToken cancellationToken)
    {
        var possuiLojas = await _dbContext.UsuarioLojas.AnyAsync(
            x => x.UsuarioId == usuarioId &&
                 x.StatusVinculo == AccessStatusValues.VinculoLoja.Ativo &&
                 (x.DataFim == null || x.DataFim >= DateTimeOffset.UtcNow),
            cancellationToken);

        if (!possuiLojas)
        {
            return;
        }

        var lojaAtivaId = _currentRequestContext.LojaAtivaId
            ?? throw new InvalidOperationException("Selecione uma loja ativa para criar outra loja.");

        await EnsureCanManageStoreAsync(usuarioId, lojaAtivaId, cancellationToken);
    }

    /// <summary>
    /// Garante que o usuario autenticado possui permissao para gerir a loja alvo.
    /// </summary>
    private async Task EnsureCanManageStoreAsync(Guid usuarioId, Guid lojaId, CancellationToken cancellationToken)
    {
        var lojasGerenciaveis = await CarregarLojasGerenciaveisAsync(usuarioId, [lojaId], cancellationToken);
        if (!lojasGerenciaveis.Contains(lojaId))
        {
            throw new InvalidOperationException("Voce nao tem acesso para gerenciar a loja informada.");
        }
    }

    /// <summary>
    /// Carrega as lojas em que o usuario possui a permissao de gestao.
    /// </summary>
    private async Task<HashSet<Guid>> CarregarLojasGerenciaveisAsync(
        Guid usuarioId,
        IReadOnlyCollection<Guid> lojaIds,
        CancellationToken cancellationToken)
    {
        return await (
                from usuarioLoja in _dbContext.UsuarioLojas
                join usuarioLojaCargo in _dbContext.UsuarioLojaCargos on usuarioLoja.Id equals usuarioLojaCargo.UsuarioLojaId
                join cargo in _dbContext.Cargos on usuarioLojaCargo.CargoId equals cargo.Id
                join cargoPermissao in _dbContext.CargoPermissoes on cargo.Id equals cargoPermissao.CargoId
                join permissao in _dbContext.Permissoes on cargoPermissao.PermissaoId equals permissao.Id
                where usuarioLoja.UsuarioId == usuarioId
                where lojaIds.Contains(usuarioLoja.LojaId)
                where usuarioLoja.StatusVinculo == AccessStatusValues.VinculoLoja.Ativo
                where usuarioLoja.DataFim == null || usuarioLoja.DataFim >= DateTimeOffset.UtcNow
                where cargo.Ativo && permissao.Ativo
                where permissao.Codigo == AccessPermissionCodes.LojasGerenciar
                select usuarioLoja.LojaId)
            .Distinct()
            .ToHashSetAsync(cancellationToken);
    }

    /// <summary>
    /// Cria os cargos base da nova loja e retorna o cargo do dono.
    /// </summary>
    private async Task<Guid> CriarCargosBaseAsync(Guid lojaId, Guid usuarioId, CancellationToken cancellationToken)
    {
        var permissionCodes = AccessPermissionCodes.BaseRoleTemplates
            .SelectMany(template => template.PermissionCodes)
            .Distinct()
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var permissions = await _dbContext.Permissoes
            .Where(x => permissionCodes.Contains(x.Codigo) && x.Ativo)
            .ToDictionaryAsync(x => x.Codigo, x => x.Id, cancellationToken);

        Guid cargoDonoId = Guid.Empty;

        foreach (var template in AccessPermissionCodes.BaseRoleTemplates)
        {
            var cargo = new Cargo
            {
                Id = Guid.NewGuid(),
                LojaId = lojaId,
                Nome = template.Nome,
                Descricao = template.Descricao,
                Ativo = true,
                CriadoPorUsuarioId = usuarioId,
            };

            _dbContext.Cargos.Add(cargo);

            foreach (var permissionCode in template.PermissionCodes.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (!permissions.TryGetValue(permissionCode, out var permissionId))
                {
                    continue;
                }

                _dbContext.CargoPermissoes.Add(new CargoPermissao
                {
                    Id = Guid.NewGuid(),
                    CargoId = cargo.Id,
                    PermissaoId = permissionId,
                    CriadoPorUsuarioId = usuarioId,
                });
            }

            if (string.Equals(template.CodigoInterno, "dono_loja", StringComparison.OrdinalIgnoreCase))
            {
                cargoDonoId = cargo.Id;
            }
        }

        if (cargoDonoId == Guid.Empty)
        {
            throw new InvalidOperationException("Nao foi possivel criar o cargo base de dono da loja.");
        }

        return cargoDonoId;
    }

    /// <summary>
    /// Se a sessao atual ainda nao possui loja ativa, aponta para a loja criada.
    /// </summary>
    private async Task VincularLojaNaSessaoAtualAsync(Guid lojaId, Guid usuarioId, CancellationToken cancellationToken)
    {
        if (_currentRequestContext.SessaoId is null || _currentRequestContext.LojaAtivaId is not null)
        {
            return;
        }

        var sessao = await _dbContext.UsuarioSessoes.FirstOrDefaultAsync(
            x => x.Id == _currentRequestContext.SessaoId.Value,
            cancellationToken);

        if (sessao is null)
        {
            return;
        }

        sessao.LojaAtivaId = lojaId;
        sessao.AtualizadoEm = DateTimeOffset.UtcNow;
        sessao.AtualizadoPorUsuarioId = usuarioId;
    }

    /// <summary>
    /// Verifica se o usuario autenticado e responsavel pela loja.
    /// </summary>
    private async Task<bool> IsResponsibleAsync(Guid usuarioId, Guid lojaId, CancellationToken cancellationToken)
    {
        return await _dbContext.UsuarioLojas.AnyAsync(
            x => x.UsuarioId == usuarioId &&
                 x.LojaId == lojaId &&
                 x.EhResponsavel &&
                 x.StatusVinculo == AccessStatusValues.VinculoLoja.Ativo &&
                 (x.DataFim == null || x.DataFim >= DateTimeOffset.UtcNow),
            cancellationToken);
    }

    /// <summary>
    /// Mapeia a loja para o contrato retornado ao frontend.
    /// </summary>
    private static StoreResponse MapStore(
        Loja loja,
        bool ehResponsavel,
        bool podeGerenciar,
        bool ehLojaAtiva)
    {
        return new StoreResponse(
            loja.Id,
            loja.NomeFantasia,
            loja.RazaoSocial,
            loja.Documento,
            loja.Telefone,
            loja.Email,
            loja.Logradouro,
            loja.Numero,
            loja.Complemento,
            loja.Bairro,
            loja.Cidade,
            loja.Uf,
            loja.Cep,
            loja.StatusLoja,
            loja.Ativo,
            ehLojaAtiva,
            ehResponsavel,
            podeGerenciar);
    }

    /// <summary>
    /// Valida o bloco principal do cadastro da loja.
    /// </summary>
    private static void ValidateStoreInput(
        string nomeFantasia,
        string razaoSocial,
        string documento,
        string telefone,
        string email,
        string logradouro,
        string numero,
        string bairro,
        string cidade,
        string uf,
        string cep)
    {
        if (string.IsNullOrWhiteSpace(nomeFantasia))
        {
            throw new InvalidOperationException("O nome fantasia da loja e obrigatorio.");
        }

        if (string.IsNullOrWhiteSpace(razaoSocial))
        {
            throw new InvalidOperationException("A razao social da loja e obrigatoria.");
        }

        if (string.IsNullOrWhiteSpace(telefone))
        {
            throw new InvalidOperationException("O telefone da loja e obrigatorio.");
        }

        if (string.IsNullOrWhiteSpace(logradouro) ||
            string.IsNullOrWhiteSpace(numero) ||
            string.IsNullOrWhiteSpace(bairro) ||
            string.IsNullOrWhiteSpace(cidade) ||
            string.IsNullOrWhiteSpace(uf) ||
            string.IsNullOrWhiteSpace(cep))
        {
            throw new InvalidOperationException("Endereco da loja incompleto.");
        }

        _ = NormalizeDocument(documento);
        _ = NormalizeEmail(email);
    }

    /// <summary>
    /// Normaliza e valida o status aceito para a loja.
    /// </summary>
    private static string NormalizeStatus(string statusLoja)
    {
        var normalized = statusLoja.Trim().ToLowerInvariant();
        if (!AllowedStatuses.Contains(normalized))
        {
            throw new InvalidOperationException("Status da loja invalido.");
        }

        return normalized;
    }

    /// <summary>
    /// Remove formatacao do documento para validar unicidade real.
    /// </summary>
    private static string NormalizeDocument(string documento)
    {
        if (string.IsNullOrWhiteSpace(documento))
        {
            throw new InvalidOperationException("O documento da loja e obrigatorio.");
        }

        var normalized = new string(documento.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new InvalidOperationException("O documento da loja e obrigatorio.");
        }

        return normalized;
    }

    /// <summary>
    /// Normaliza e valida o email da loja.
    /// </summary>
    private static string NormalizeEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new InvalidOperationException("O email da loja e obrigatorio.");
        }

        return email.Trim().ToLowerInvariant();
    }
}
