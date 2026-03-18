using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Renova.Api.Common.Controllers;
using Renova.Api.Common.Responses;
using Renova.Services.Features.Dashboards.Abstractions;
using Renova.Services.Features.Dashboards.Contracts;

namespace Renova.Api.Features.Dashboards.V1;

// Controller HTTP do modulo 14 com filtros e consultas consolidadas.
[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/dashboards")]
public sealed class DashboardsController : RenovaControllerBase
{
    private readonly IDashboardService _dashboardService;

    /// <summary>
    /// Inicializa o controller com o service principal do modulo.
    /// </summary>
    public DashboardsController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("workspace")]
    /// <summary>
    /// Carrega os filtros disponiveis para a tela de indicadores.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<DashboardWorkspaceResponse>>> GetWorkspace(CancellationToken cancellationToken)
    {
        var response = await _dashboardService.ObterWorkspaceAsync(cancellationToken);
        return OkEnvelope(response);
    }

    [HttpGet("overview")]
    /// <summary>
    /// Consolida os paineis do modulo com os filtros informados.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<DashboardOverviewResponse>>> GetOverview(
        [FromQuery] DashboardQueryRequest query,
        CancellationToken cancellationToken)
    {
        var response = await _dashboardService.ObterVisaoGeralAsync(query, cancellationToken);
        return OkEnvelope(response);
    }
}
