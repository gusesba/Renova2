using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Renova.Api.Common.Controllers;
using Renova.Api.Common.Responses;
using Renova.Api.Infrastructure.Security;
using Renova.Services.Features.Access;
using Renova.Services.Features.Access.Abstractions;
using Renova.Services.Features.Access.Contracts;

namespace Renova.Api.Features.Access.V1;

// Representa o controlador HTTP de manutencao de cargos e suas permissoes.
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/access/roles")]
public sealed class RolesController : RenovaControllerBase
{
    private readonly IAccessRoleService _accessRoleService;

    /// <summary>
    /// Inicializa o controller com os servicos de cargos.
    /// </summary>
    public RolesController(IAccessRoleService accessRoleService)
    {
        _accessRoleService = accessRoleService;
    }

    [HttpGet]
    [RequirePermission(AccessPermissionCodes.CargosGerenciar)]
    /// <summary>
    /// Lista os cargos da loja ativa.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<IReadOnlyList<RoleResponse>>>> List(CancellationToken cancellationToken)
    {
        var response = await _accessRoleService.ListarAsync(cancellationToken);
        return OkEnvelope(response);
    }

    [HttpPost]
    [RequirePermission(AccessPermissionCodes.CargosGerenciar)]
    /// <summary>
    /// Cria um novo cargo na loja ativa.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<RoleResponse>>> Create([FromBody] CreateRoleRequest request, CancellationToken cancellationToken)
    {
        var response = await _accessRoleService.CriarAsync(request, cancellationToken);
        return OkEnvelope(response);
    }

    [HttpPut("{cargoId:guid}")]
    [RequirePermission(AccessPermissionCodes.CargosGerenciar)]
    /// <summary>
    /// Atualiza os dados cadastrais de um cargo.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<RoleResponse>>> Update(
        Guid cargoId,
        [FromBody] UpdateRoleRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _accessRoleService.AtualizarAsync(cargoId, request, cancellationToken);
        return OkEnvelope(response);
    }

    [HttpPut("{cargoId:guid}/permissions")]
    [RequirePermission(AccessPermissionCodes.CargosGerenciar)]
    /// <summary>
    /// Substitui a matriz de permissoes de um cargo.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<RoleResponse>>> UpdatePermissions(
        Guid cargoId,
        [FromBody] UpdateRolePermissionsRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _accessRoleService.AtualizarPermissoesAsync(cargoId, request, cancellationToken);
        return OkEnvelope(response);
    }
}
