using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Renova.Api.Common.Controllers;
using Renova.Api.Common.Responses;
using Renova.Api.Infrastructure.Security;
using Renova.Services.Features.Access;
using Renova.Services.Features.Access.Abstractions;
using Renova.Services.Features.Access.Contracts;

namespace Renova.Api.Features.Access.V1;

// Representa o controlador HTTP de vinculos de usuarios com a loja ativa.
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/access/store-memberships")]
public sealed class StoreMembershipsController : RenovaControllerBase
{
    private readonly IAccessStoreMembershipService _accessStoreMembershipService;

    /// <summary>
    /// Inicializa o controller com os servicos de vinculo por loja.
    /// </summary>
    public StoreMembershipsController(IAccessStoreMembershipService accessStoreMembershipService)
    {
        _accessStoreMembershipService = accessStoreMembershipService;
    }

    [HttpGet]
    [RequirePermission(AccessPermissionCodes.UsuariosGerenciar)]
    /// <summary>
    /// Lista os vinculos de usuarios da loja ativa.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<IReadOnlyList<StoreMembershipResponse>>>> List(CancellationToken cancellationToken)
    {
        var response = await _accessStoreMembershipService.ListarAsync(cancellationToken);
        return OkEnvelope(response);
    }

    [HttpPost]
    [RequirePermission(AccessPermissionCodes.UsuariosGerenciar)]
    /// <summary>
    /// Cria um novo vinculo de usuario com a loja ativa.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<StoreMembershipResponse>>> Create(
        [FromBody] CreateStoreMembershipRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _accessStoreMembershipService.CriarAsync(request, cancellationToken);
        return OkEnvelope(response);
    }

    [HttpPut("{usuarioLojaId:guid}")]
    [RequirePermission(AccessPermissionCodes.UsuariosGerenciar)]
    /// <summary>
    /// Atualiza os dados principais de um vinculo de loja.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<StoreMembershipResponse>>> Update(
        Guid usuarioLojaId,
        [FromBody] UpdateStoreMembershipRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _accessStoreMembershipService.AtualizarAsync(usuarioLojaId, request, cancellationToken);
        return OkEnvelope(response);
    }

    [HttpPut("{usuarioLojaId:guid}/roles")]
    [RequirePermission(AccessPermissionCodes.UsuariosGerenciar)]
    /// <summary>
    /// Atualiza os cargos associados ao vinculo da loja.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<StoreMembershipResponse>>> UpdateRoles(
        Guid usuarioLojaId,
        [FromBody] UpdateStoreMembershipRolesRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _accessStoreMembershipService.AtualizarCargosAsync(usuarioLojaId, request, cancellationToken);
        return OkEnvelope(response);
    }
}
