using Microsoft.EntityFrameworkCore;
using Renova.Domain.Models;
using Renova.Persistence;
using Renova.Services.Features.Access.Abstractions;

namespace Renova.Services.Features.Access.Services;

// Garante apenas a carga estrutural do modulo de acesso.
public sealed class AccessBootstrapService : IAccessBootstrapService
{
    private readonly RenovaDbContext _dbContext;

    /// <summary>
    /// Inicializa o servico com acesso a persistencia.
    /// </summary>
    public AccessBootstrapService(RenovaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Sincroniza permissoes e perfis base ja existentes no banco.
    /// </summary>
    public async Task InicializarAsync(bool isDevelopment, CancellationToken cancellationToken = default)
    {
        await GarantirPermissoesAsync(cancellationToken);
        await GarantirPerfisBaseAsync(cancellationToken);
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
}
