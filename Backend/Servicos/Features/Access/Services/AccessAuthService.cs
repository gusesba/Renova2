using Microsoft.EntityFrameworkCore;
using Renova.Domain.Models;
using Renova.Persistence;
using Renova.Services.Features.Access.Abstractions;
using Renova.Services.Features.Access.Contracts;
using System.Text.Json;

namespace Renova.Services.Features.Access.Services;

// Representa o servico principal de login, sessao e recuperacao de senha.
public sealed class AccessAuthService : IAccessAuthService
{
    private readonly RenovaDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IAccessAuditService _auditService;
    private readonly ICurrentRequestContext _currentRequestContext;

    /// <summary>
    /// Inicializa o servico com persistencia, seguranca e auditoria.
    /// </summary>
    public AccessAuthService(
        RenovaDbContext dbContext,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IAccessAuditService auditService,
        ICurrentRequestContext currentRequestContext)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _auditService = auditService;
        _currentRequestContext = currentRequestContext;
    }

    /// <summary>
    /// Valida as credenciais e cria uma nova sessao autenticada.
    /// </summary>
    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        ValidatePassword(request.Senha);

        var email = NormalizeEmail(request.Email);
        var usuario = await _dbContext.Usuarios
            .AsTracking()
            .FirstOrDefaultAsync(x => x.Email.ToLower() == email, cancellationToken);

        if (usuario is null || !_passwordHasher.Verify(request.Senha, usuario.SenhaHash, usuario.SenhaSalt))
        {
            if (usuario is not null)
            {
                await _auditService.RegistrarEventoAcessoAsync(
                    usuario.Id,
                    "login_falha",
                    new { motivo = "credenciais_invalidas" },
                    cancellationToken);
            }

            throw new InvalidOperationException("Credenciais inválidas.");
        }

        if (!string.Equals(usuario.StatusUsuario, AccessStatusValues.Usuario.Ativo, StringComparison.OrdinalIgnoreCase))
        {
            await _auditService.RegistrarEventoAcessoAsync(
                usuario.Id,
                "login_negado",
                new { motivo = usuario.StatusUsuario },
                cancellationToken);

            throw new InvalidOperationException("O usuário não está ativo para acesso.");
        }

        var lojas = await CarregarLojasAcessiveisAsync(usuario.Id, cancellationToken);
        var lojaAtivaId = lojas.FirstOrDefault()?.Id;
        var token = _tokenService.Generate();
        var expiraEm = DateTimeOffset.UtcNow.AddHours(8);

        _dbContext.UsuarioSessoes.Add(new UsuarioSessao
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuario.Id,
            LojaAtivaId = lojaAtivaId,
            TokenHash = token.TokenHash,
            ExpiraEm = expiraEm,
            Ip = _currentRequestContext.Ip,
            UserAgent = _currentRequestContext.UserAgent,
            CriadoPorUsuarioId = usuario.Id,
        });

        usuario.UltimoLoginEm = DateTimeOffset.UtcNow;
        usuario.AtualizadoEm = DateTimeOffset.UtcNow;
        usuario.AtualizadoPorUsuarioId = usuario.Id;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.RegistrarEventoAcessoAsync(usuario.Id, "login_sucesso", new { lojaAtivaId }, cancellationToken);

        var contexto = await ConstruirContextoAsync(usuario, lojaAtivaId, cancellationToken);
        return new LoginResponse(token.RawToken, expiraEm, contexto);
    }

    /// <summary>
    /// Revoga a sessao autenticada corrente.
    /// </summary>
    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        if (_currentRequestContext.SessaoId is null || _currentRequestContext.UsuarioId is null)
        {
            throw new InvalidOperationException("Sessão autenticada não encontrada.");
        }

        var sessao = await _dbContext.UsuarioSessoes.FirstOrDefaultAsync(
            x => x.Id == _currentRequestContext.SessaoId.Value,
            cancellationToken);

        if (sessao is null || sessao.RevogadoEm is not null)
        {
            return;
        }

        sessao.RevogadoEm = DateTimeOffset.UtcNow;
        sessao.AtualizadoEm = DateTimeOffset.UtcNow;
        sessao.AtualizadoPorUsuarioId = _currentRequestContext.UsuarioId;

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _auditService.RegistrarEventoAcessoAsync(_currentRequestContext.UsuarioId.Value, "logout", null, cancellationToken);
    }

    /// <summary>
    /// Retorna o contexto completo do usuario autenticado.
    /// </summary>
    public async Task<LoginContextResponse> ObterContextoAtualAsync(CancellationToken cancellationToken = default)
    {
        var usuarioId = EnsureAuthenticatedUser();
        var usuario = await _dbContext.Usuarios.FirstOrDefaultAsync(x => x.Id == usuarioId, cancellationToken)
            ?? throw new InvalidOperationException("Usuário autenticado não encontrado.");

        return await ConstruirContextoAsync(usuario, _currentRequestContext.LojaAtivaId, cancellationToken);
    }

    /// <summary>
    /// Atualiza a loja ativa gravada na sessao.
    /// </summary>
    public async Task<LoginContextResponse> AlterarLojaAtivaAsync(
        SwitchActiveStoreRequest request,
        CancellationToken cancellationToken = default)
    {
        var usuarioId = EnsureAuthenticatedUser();
        var sessaoId = _currentRequestContext.SessaoId
            ?? throw new InvalidOperationException("Sessão autenticada não encontrada.");

        var vinculoExiste = await _dbContext.UsuarioLojas.AnyAsync(
            x => x.UsuarioId == usuarioId &&
                 x.LojaId == request.LojaId &&
                 x.StatusVinculo == AccessStatusValues.VinculoLoja.Ativo &&
                 (x.DataFim == null || x.DataFim >= DateTimeOffset.UtcNow),
            cancellationToken);

        if (!vinculoExiste)
        {
            throw new InvalidOperationException("O usuário não possui acesso ativo à loja informada.");
        }

        var sessao = await _dbContext.UsuarioSessoes.FirstAsync(x => x.Id == sessaoId, cancellationToken);
        sessao.LojaAtivaId = request.LojaId;
        sessao.AtualizadoEm = DateTimeOffset.UtcNow;
        sessao.AtualizadoPorUsuarioId = usuarioId;

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _auditService.RegistrarEventoAcessoAsync(usuarioId, "troca_loja_ativa", new { request.LojaId }, cancellationToken);

        var usuario = await _dbContext.Usuarios.FirstAsync(x => x.Id == usuarioId, cancellationToken);
        return await ConstruirContextoAsync(usuario, request.LojaId, cancellationToken);
    }

    /// <summary>
    /// Emite um token temporario para redefinicao de senha.
    /// </summary>
    public async Task<PasswordResetRequestResponse> SolicitarRecuperacaoAsync(
        PasswordResetRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = NormalizeEmail(request.Email);
        var usuario = await _dbContext.Usuarios.FirstOrDefaultAsync(x => x.Email.ToLower() == email, cancellationToken);

        if (usuario is null)
        {
            return new PasswordResetRequestResponse(
                "Se o e-mail estiver cadastrado, um token de recuperação foi emitido.",
                null,
                null);
        }

        var token = _tokenService.Generate();
        var expiraEm = DateTimeOffset.UtcNow.AddMinutes(30);

        _dbContext.UsuarioRecuperacoesAcesso.Add(new UsuarioRecuperacaoAcesso
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuario.Id,
            TokenHash = token.TokenHash,
            SolicitadoEm = DateTimeOffset.UtcNow,
            ExpiraEm = expiraEm,
            Ip = _currentRequestContext.Ip,
            UserAgent = _currentRequestContext.UserAgent,
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _auditService.RegistrarEventoAcessoAsync(usuario.Id, "recuperacao_solicitada", new { expiraEm }, cancellationToken);
        await RegistrarAuditoriaRecuperacaoAsync(
            usuario,
            "recuperacao_solicitada",
            null,
            new { expiraEm },
            cancellationToken);

        return new PasswordResetRequestResponse(
            "Token de recuperação gerado com sucesso.",
            token.RawToken,
            expiraEm);
    }

    /// <summary>
    /// Consome um token de recuperacao e redefine a senha do usuario.
    /// </summary>
    public async Task RedefinirSenhaAsync(ConfirmPasswordResetRequest request, CancellationToken cancellationToken = default)
    {
        ValidatePassword(request.NovaSenha);

        var tokenHash = _tokenService.Hash(request.Token);
        var recuperacao = await _dbContext.UsuarioRecuperacoesAcesso
            .AsTracking()
            .FirstOrDefaultAsync(
                x => x.TokenHash == tokenHash &&
                     x.UtilizadoEm == null &&
                     x.ExpiraEm >= DateTimeOffset.UtcNow,
                cancellationToken)
            ?? throw new InvalidOperationException("O token de recuperação é inválido ou expirou.");

        var usuario = await _dbContext.Usuarios.FirstAsync(x => x.Id == recuperacao.UsuarioId, cancellationToken);
        var senha = _passwordHasher.Hash(request.NovaSenha);

        usuario.SenhaHash = senha.Hash;
        usuario.SenhaSalt = senha.Salt;
        usuario.AtualizadoEm = DateTimeOffset.UtcNow;
        usuario.AtualizadoPorUsuarioId = usuario.Id;

        recuperacao.UtilizadoEm = DateTimeOffset.UtcNow;
        recuperacao.AtualizadoEm = DateTimeOffset.UtcNow;
        recuperacao.AtualizadoPorUsuarioId = usuario.Id;

        var sessoes = await _dbContext.UsuarioSessoes
            .Where(x => x.UsuarioId == usuario.Id && x.RevogadoEm == null)
            .ToListAsync(cancellationToken);

        foreach (var sessao in sessoes)
        {
            sessao.RevogadoEm = DateTimeOffset.UtcNow;
            sessao.AtualizadoEm = DateTimeOffset.UtcNow;
            sessao.AtualizadoPorUsuarioId = usuario.Id;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _auditService.RegistrarEventoAcessoAsync(usuario.Id, "senha_redefinida", null, cancellationToken);
        await RegistrarAuditoriaRecuperacaoAsync(
            usuario,
            "senha_redefinida",
            null,
            new { redefinidoEm = recuperacao.UtilizadoEm },
            cancellationToken);
    }

    /// <summary>
    /// Garante que existe um usuario autenticado no contexto.
    /// </summary>
    private Guid EnsureAuthenticatedUser()
    {
        return _currentRequestContext.UsuarioId
            ?? throw new InvalidOperationException("Usuário autenticado não encontrado.");
    }

    /// <summary>
    /// Normaliza e valida o e-mail informado.
    /// </summary>
    private static string NormalizeEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new InvalidOperationException("O e-mail é obrigatório.");
        }

        return email.Trim().ToLowerInvariant();
    }

    /// <summary>
    /// Valida a senha minima exigida pelo modulo.
    /// </summary>
    private static void ValidatePassword(string senha)
    {
        if (string.IsNullOrWhiteSpace(senha) || senha.Trim().Length < 8)
        {
            throw new InvalidOperationException("A senha deve ter ao menos 8 caracteres.");
        }
    }

    /// <summary>
    /// Monta o contexto de resposta com lojas acessiveis e permissoes ativas.
    /// </summary>
    private async Task<LoginContextResponse> ConstruirContextoAsync(
        Usuario usuario,
        Guid? lojaAtivaId,
        CancellationToken cancellationToken)
    {
        var lojas = await CarregarLojasAcessiveisAsync(usuario.Id, cancellationToken);
        var activeStoreId = lojaAtivaId ?? lojas.FirstOrDefault()?.Id;
        var permissoes = activeStoreId is null
            ? Array.Empty<string>()
            : await CarregarPermissoesAsync(usuario.Id, activeStoreId.Value, cancellationToken);

        return new LoginContextResponse(
            new AuthenticatedUserResponse(
                usuario.Id,
                usuario.Nome,
                usuario.Email,
                usuario.Telefone,
                usuario.StatusUsuario,
                usuario.PessoaId),
            activeStoreId,
            lojas,
            permissoes);
    }

    /// <summary>
    /// Carrega as lojas as quais o usuario possui vinculo ativo.
    /// </summary>
    private async Task<IReadOnlyList<AccessibleStoreResponse>> CarregarLojasAcessiveisAsync(Guid usuarioId, CancellationToken cancellationToken)
    {
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
            return Array.Empty<AccessibleStoreResponse>();
        }

        var usuarioLojaIds = vinculos.Select(x => x.UsuarioLoja.Id).ToList();
        var cargos = await (
                from usuarioLojaCargo in _dbContext.UsuarioLojaCargos
                join cargo in _dbContext.Cargos on usuarioLojaCargo.CargoId equals cargo.Id
                where usuarioLojaIds.Contains(usuarioLojaCargo.UsuarioLojaId)
                select new
                {
                    usuarioLojaCargo.UsuarioLojaId,
                    cargo.Id,
                    cargo.Nome,
                })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return vinculos.Select(vinculo => new AccessibleStoreResponse(
                vinculo.Loja.Id,
                vinculo.Loja.NomeFantasia,
                vinculo.UsuarioLoja.StatusVinculo,
                vinculo.UsuarioLoja.EhResponsavel,
                cargos
                    .Where(x => x.UsuarioLojaId == vinculo.UsuarioLoja.Id)
                    .Select(x => new RoleReferenceResponse(x.Id, x.Nome))
                    .ToArray()))
            .ToArray();
    }

    /// <summary>
    /// Carrega as permissoes efetivas do usuario para a loja informada.
    /// </summary>
    private async Task<IReadOnlyList<string>> CarregarPermissoesAsync(Guid usuarioId, Guid lojaId, CancellationToken cancellationToken)
    {
        var usuarioLoja = await _dbContext.UsuarioLojas
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.UsuarioId == usuarioId &&
                     x.LojaId == lojaId &&
                     x.StatusVinculo == AccessStatusValues.VinculoLoja.Ativo &&
                     (x.DataFim == null || x.DataFim >= DateTimeOffset.UtcNow),
                cancellationToken);

        if (usuarioLoja is null)
        {
            return Array.Empty<string>();
        }

        return await (
                from usuarioLojaCargo in _dbContext.UsuarioLojaCargos
                join cargo in _dbContext.Cargos on usuarioLojaCargo.CargoId equals cargo.Id
                join cargoPermissao in _dbContext.CargoPermissoes on cargo.Id equals cargoPermissao.CargoId
                join permissao in _dbContext.Permissoes on cargoPermissao.PermissaoId equals permissao.Id
                where usuarioLojaCargo.UsuarioLojaId == usuarioLoja.Id
                where cargo.LojaId == lojaId && cargo.Ativo && permissao.Ativo
                select permissao.Codigo)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Registra a trilha administrativa do fluxo de recuperacao de senha.
    /// </summary>
    private async Task RegistrarAuditoriaRecuperacaoAsync(
        Usuario usuario,
        string acao,
        object? antes,
        object? depois,
        CancellationToken cancellationToken)
    {
        _dbContext.AuditoriaEventos.Add(new AuditoriaEvento
        {
            Id = Guid.NewGuid(),
            LojaId = null,
            UsuarioId = usuario.Id,
            Entidade = "usuario",
            EntidadeId = usuario.Id,
            Acao = acao,
            AntesJson = antes is null ? null : JsonSerializer.Serialize(antes),
            DepoisJson = depois is null ? null : JsonSerializer.Serialize(depois),
            OcorridoEm = DateTimeOffset.UtcNow,
            CriadoPorUsuarioId = usuario.Id,
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
