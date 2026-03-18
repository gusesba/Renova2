using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Renova.Api.Common.Controllers;
using Renova.Api.Common.Responses;
using Renova.Api.Infrastructure.Security;
using Renova.Services.Features.Access;
using Renova.Services.Features.Reports.Abstractions;
using Renova.Services.Features.Reports.Contracts;

namespace Renova.Api.Features.Reports.V1;

// Controller HTTP do modulo 15 com consulta, exportacao e filtros salvos.
[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/reports")]
public sealed class ReportsController : RenovaControllerBase
{
    private readonly IReportService _reportService;

    /// <summary>
    /// Inicializa o controller com o service principal do modulo.
    /// </summary>
    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("workspace")]
    /// <summary>
    /// Carrega listas auxiliares e filtros salvos do modulo.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<ReportWorkspaceResponse>>> GetWorkspace(CancellationToken cancellationToken)
    {
        var response = await _reportService.ObterWorkspaceAsync(cancellationToken);
        return OkEnvelope(response);
    }

    [HttpPost("run")]
    [RequirePermission(AccessPermissionCodes.RelatoriosExportar)]
    /// <summary>
    /// Executa o relatorio solicitado e devolve o grid consolidado.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<ReportResultResponse>>> Run(
        [FromBody] ReportQueryRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _reportService.ExecutarAsync(request, cancellationToken);
        return OkEnvelope(response);
    }

    [HttpPost("export/{format}")]
    [RequirePermission(AccessPermissionCodes.RelatoriosExportar)]
    /// <summary>
    /// Exporta o relatorio em HTML imprimivel ou CSV compativel com Excel.
    /// </summary>
    public async Task<IActionResult> Export(
        string format,
        [FromBody] ReportQueryRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _reportService.ExportarAsync(format, request, cancellationToken);
        return File(response.Content, response.ContentType, response.FileName);
    }

    [HttpPost("saved-filters")]
    [RequirePermission(AccessPermissionCodes.RelatoriosExportar)]
    /// <summary>
    /// Persiste um filtro frequente para o usuario autenticado.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<SavedReportFilterResponse>>> SaveFilter(
        [FromBody] SaveReportFilterRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _reportService.SalvarFiltroAsync(request, cancellationToken);
        return OkEnvelope(response);
    }

    [HttpDelete("saved-filters/{filtroId:guid}")]
    [RequirePermission(AccessPermissionCodes.RelatoriosExportar)]
    /// <summary>
    /// Inativa um filtro salvo do usuario autenticado.
    /// </summary>
    public async Task<IActionResult> DeleteFilter(Guid filtroId, CancellationToken cancellationToken)
    {
        await _reportService.RemoverFiltroAsync(filtroId, cancellationToken);
        return NoContent();
    }
}
