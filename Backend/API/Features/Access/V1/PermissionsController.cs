using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Renova.Api.Common.Controllers;
using Renova.Api.Common.Responses;
using Renova.Api.Infrastructure.Security;
using Renova.Services.Features.Access;
using Renova.Services.Features.Access.Abstractions;

namespace Renova.Api.Features.Access.V1;

// Representa o controlador HTTP de consulta ao catalogo de permissoes.
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/access/permissions")]
public sealed class PermissionsController : RenovaControllerBase
{
    private readonly IAccessRoleService _accessRoleService;

    /// <summary>
    /// Inicializa o controller com os servicos de cargos e permissoes.
    /// </summary>
    public PermissionsController(IAccessRoleService accessRoleService)
    {
        _accessRoleService = accessRoleService;
    }

    [HttpGet]
    [RequirePermission(AccessPermissionCodes.CargosGerenciar)]
    /// <summary>
    /// Lista as permissoes disponiveis para associacao em cargos.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<IReadOnlyList<Renova.Services.Features.Access.Contracts.PermissionResponse>>>> List(CancellationToken cancellationToken)
    {
        var response = await _accessRoleService.ListarPermissoesAsync(cancellationToken);
        return OkEnvelope(response);
    }
}
