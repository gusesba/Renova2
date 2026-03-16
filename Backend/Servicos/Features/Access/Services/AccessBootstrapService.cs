using Microsoft.EntityFrameworkCore;
using Renova.Domain.Models;
using Renova.Persistence;
using Renova.Services.Features.Access.Abstractions;

namespace Renova.Services.Features.Access.Services;

// Representa o servico que garante a carga inicial do modulo de acesso.
public sealed class AccessBootstrapService : IAccessBootstrapService
{
    private readonly RenovaDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;

    /// <summary>
    /// Inicializa o servico com persistencia e suporte criptografico.
    /// </summary>
    public AccessBootstrapService(RenovaDbContext dbContext, IPasswordHasher passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    /// <summary>
    /// Executa a inicializacao de permissoes, cargos base e bootstrap de desenvolvimento.
    /// </summary>
    public async Task InicializarAsync(bool isDevelopment, CancellationToken cancellationToken = default)
    {
        await GarantirPermissoesAsync(cancellationToken);

        if (isDevelopment)
        {
            await GarantirEstruturaBootstrapDesenvolvimentoAsync(cancellationToken);
        }

        await GarantirPerfisBaseAsync(cancellationToken);

        if (isDevelopment)
        {
            await GarantirAdministradorBootstrapDesenvolvimentoAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Sincroniza o catalogo base de permissoes no banco.
    /// </summary>
    private async Task GarantirPermissoesAsync(CancellationToken cancellationToken)
    {
        var existentes = await _dbContext.Permissoes
            .ToDictionaryAsync(x => x.Codigo, StringComparer.OrdinalIgnoreCase, cancellationToken);

        foreach (var definition in AccessPermissionCodes.Catalog)
        {
            if (existentes.TryGetValue(definition.Codigo, out var permissao))
            {
                permissao.Nome = definition.Nome;
                permissao.Descricao = definition.Descricao;
                permissao.Modulo = definition.Modulo;
                permissao.Ativo = true;
                permissao.InativadoEm = null;
                continue;
            }

            _dbContext.Permissoes.Add(new Permissao
            {
                Id = Guid.NewGuid(),
                Codigo = definition.Codigo,
                Nome = definition.Nome,
                Descricao = definition.Descricao,
                Modulo = definition.Modulo,
                Ativo = true,
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Garante a existencia dos cargos padrao para cada loja ativa.
    /// </summary>
    private async Task GarantirPerfisBaseAsync(CancellationToken cancellationToken)
    {
        var lojas = await _dbContext.Lojas
            .Where(x => x.Ativo)
            .ToListAsync(cancellationToken);

        if (lojas.Count == 0)
        {
            return;
        }

        var permissoes = await _dbContext.Permissoes
            .Where(x => x.Ativo)
            .ToDictionaryAsync(x => x.Codigo, cancellationToken);

        foreach (var loja in lojas)
        {
            foreach (var template in AccessPermissionCodes.BaseRoleTemplates)
            {
                var cargo = await _dbContext.Cargos.FirstOrDefaultAsync(
                    x => x.LojaId == loja.Id && x.Nome == template.Nome,
                    cancellationToken);

                if (cargo is null)
                {
                    cargo = new Cargo
                    {
                        Id = Guid.NewGuid(),
                        LojaId = loja.Id,
                        Nome = template.Nome,
                        Descricao = template.Descricao,
                        Ativo = true,
                    };

                    _dbContext.Cargos.Add(cargo);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }

                var permissionIds = template.PermissionCodes
                    .Where(permissoes.ContainsKey)
                    .Select(code => permissoes[code].Id)
                    .ToHashSet();

                var existentes = await _dbContext.CargoPermissoes
                    .Where(x => x.CargoId == cargo.Id)
                    .ToListAsync(cancellationToken);

                var removidos = existentes
                    .Where(x => !permissionIds.Contains(x.PermissaoId))
                    .ToList();

                if (removidos.Count > 0)
                {
                    _dbContext.CargoPermissoes.RemoveRange(removidos);
                }

                var permissionIdsExistentes = existentes
                    .Select(x => x.PermissaoId)
                    .ToHashSet();

                foreach (var permissionId in permissionIds.Where(id => !permissionIdsExistentes.Contains(id)))
                {
                    _dbContext.CargoPermissoes.Add(new CargoPermissao
                    {
                        Id = Guid.NewGuid(),
                        CargoId = cargo.Id,
                        PermissaoId = permissionId,
                    });
                }
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Cria a estrutura minima de loja e catalogo para ambiente de desenvolvimento.
    /// </summary>
    private async Task GarantirEstruturaBootstrapDesenvolvimentoAsync(CancellationToken cancellationToken)
    {
        if (await _dbContext.Lojas.AnyAsync(cancellationToken))
        {
            return;
        }

        var conjunto = new ConjuntoCatalogo
        {
            Id = Guid.NewGuid(),
            Nome = "Catálogo Padrão Renova",
            Descricao = "Conjunto inicial usado no ambiente de desenvolvimento.",
            Ativo = true,
        };

        var loja = new Loja
        {
            Id = Guid.NewGuid(),
            NomeFantasia = "Renova Centro",
            RazaoSocial = "Renova Centro LTDA",
            Documento = "00000000000191",
            Telefone = "(00) 0000-0000",
            Email = "contato@renova.local",
            Logradouro = "Rua Principal",
            Numero = "100",
            Bairro = "Centro",
            Cidade = "Curitiba",
            Uf = "PR",
            Cep = "80000-000",
            StatusLoja = "ativa",
            ConjuntoCatalogoId = conjunto.Id,
            Ativo = true,
        };

        var configuracao = new LojaConfiguracao
        {
            Id = Guid.NewGuid(),
            LojaId = loja.Id,
            NomeExibicao = "Renova Centro",
            CabecalhoImpressao = "Renova Centro",
            RodapeImpressao = "Obrigada pela preferência.",
            UsaModeloUnicoEtiqueta = true,
            UsaModeloUnicoRecibo = true,
            FusoHorario = "America/Sao_Paulo",
            Moeda = "BRL",
        };

        _dbContext.ConjuntoCatalogos.Add(conjunto);
        _dbContext.Lojas.Add(loja);
        _dbContext.LojaConfiguracoes.Add(configuracao);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Garante um usuario administrador padrao para desenvolvimento.
    /// </summary>
    private async Task GarantirAdministradorBootstrapDesenvolvimentoAsync(CancellationToken cancellationToken)
    {
        var loja = await _dbContext.Lojas
            .Where(x => x.Ativo)
            .OrderBy(x => x.NomeFantasia)
            .FirstOrDefaultAsync(cancellationToken);

        if (loja is null)
        {
            return;
        }

        var cargoDono = await _dbContext.Cargos.FirstOrDefaultAsync(
            x => x.LojaId == loja.Id && x.Nome == "Dono da Loja",
            cancellationToken);

        if (cargoDono is null)
        {
            return;
        }

        var admin = await _dbContext.Usuarios.FirstOrDefaultAsync(
            x => x.Email == "admin@renova.local",
            cancellationToken);

        if (admin is null)
        {
            var senha = _passwordHasher.Hash("Renova123!");
            admin = new Usuario
            {
                Id = Guid.NewGuid(),
                Nome = "Administrador Renova",
                Email = "admin@renova.local",
                Telefone = "(00) 00000-0000",
                SenhaHash = senha.Hash,
                SenhaSalt = senha.Salt,
                StatusUsuario = AccessStatusValues.Usuario.Ativo,
            };

            _dbContext.Usuarios.Add(admin);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var usuarioLoja = await _dbContext.UsuarioLojas.FirstOrDefaultAsync(
            x => x.UsuarioId == admin.Id && x.LojaId == loja.Id,
            cancellationToken);

        if (usuarioLoja is null)
        {
            usuarioLoja = new UsuarioLoja
            {
                Id = Guid.NewGuid(),
                UsuarioId = admin.Id,
                LojaId = loja.Id,
                StatusVinculo = AccessStatusValues.VinculoLoja.Ativo,
                EhResponsavel = true,
                DataInicio = DateTimeOffset.UtcNow,
            };

            _dbContext.UsuarioLojas.Add(usuarioLoja);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var possuiCargoDono = await _dbContext.UsuarioLojaCargos.AnyAsync(
            x => x.UsuarioLojaId == usuarioLoja.Id && x.CargoId == cargoDono.Id,
            cancellationToken);

        if (possuiCargoDono)
        {
            return;
        }

        var usuarioLojaCargo = new UsuarioLojaCargo
        {
            Id = Guid.NewGuid(),
            UsuarioLojaId = usuarioLoja.Id,
            CargoId = cargoDono.Id,
        };

        _dbContext.UsuarioLojaCargos.Add(usuarioLojaCargo);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
