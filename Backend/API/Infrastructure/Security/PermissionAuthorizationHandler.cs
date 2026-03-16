using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Renova.Persistence;
using Renova.Services.Features.Access;

namespace Renova.Api.Infrastructure.Security;

// Representa o handler que valida permissoes por loja e por cargo na sessao autenticada.
public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly RenovaDbContext _dbContext;

    /// <summary>
    /// Inicializa o handler com acesso ao banco de autorizacao.
    /// </summary>
    public PermissionAuthorizationHandler(RenovaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Confere se o usuario autenticado possui a permissao exigida na loja ativa.
    /// </summary>
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var userIdValue = context.User.FindFirstValue(SessionTokenAuthenticationHandler.UserIdClaimType);
        var activeStoreValue = context.User.FindFirstValue(SessionTokenAuthenticationHandler.ActiveStoreIdClaimType);

        if (!Guid.TryParse(userIdValue, out var userId) || !Guid.TryParse(activeStoreValue, out var activeStoreId))
        {
            return;
        }

        var usuarioLoja = await _dbContext.UsuarioLojas
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.UsuarioId == userId &&
                     x.LojaId == activeStoreId &&
                     x.StatusVinculo == AccessStatusValues.VinculoLoja.Ativo &&
                     (x.DataFim == null || x.DataFim >= DateTimeOffset.UtcNow),
                CancellationToken.None);

        if (usuarioLoja is null)
        {
            return;
        }

        var possuiPermissao = await (
                from usuarioLojaCargo in _dbContext.UsuarioLojaCargos
                join cargo in _dbContext.Cargos on usuarioLojaCargo.CargoId equals cargo.Id
                join cargoPermissao in _dbContext.CargoPermissoes on cargo.Id equals cargoPermissao.CargoId
                join permissao in _dbContext.Permissoes on cargoPermissao.PermissaoId equals permissao.Id
                where usuarioLojaCargo.UsuarioLojaId == usuarioLoja.Id
                where cargo.LojaId == activeStoreId && cargo.Ativo && permissao.Ativo
                where permissao.Codigo == requirement.PermissionCode
                select permissao.Id)
            .AnyAsync(CancellationToken.None);

        if (possuiPermissao)
        {
            context.Succeed(requirement);
        }
    }
}
