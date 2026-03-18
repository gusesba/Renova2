using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Renova.Api.Common.Controllers;
using Renova.Api.Common.Responses;
using Renova.Api.Infrastructure.Security;
using Renova.Services.Features.Access;
using Renova.Services.Features.Closings.Abstractions;
using Renova.Services.Features.Closings.Contracts;

namespace Renova.Api.Features.Closings.V1;

// Controller HTTP do modulo 13 com geracao, historico, conferencia e exportacao.
[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/closings")]
public sealed class ClosingsController : RenovaControllerBase
{
    private readonly IClosingService _closingService;

    /// <summary>
    /// Inicializa o controller com o service principal do modulo.
    /// </summary>
    public ClosingsController(IClosingService closingService)
    {
        _closingService = closingService;
    }

    [HttpGet("workspace")]
    /// <summary>
    /// Carrega filtros e pessoas elegiveis para o fechamento na loja ativa.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<ClosingWorkspaceResponse>>> GetWorkspace(CancellationToken cancellationToken)
    {
        var response = await _closingService.ObterWorkspaceAsync(cancellationToken);
        return OkEnvelope(response);
    }

    [HttpGet]
    /// <summary>
    /// Lista o historico de fechamentos com filtros por pessoa, status e periodo.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<IReadOnlyList<ClosingSummaryResponse>>>> List(
        [FromQuery] ClosingListQueryRequest query,
        CancellationToken cancellationToken)
    {
        var response = await _closingService.ListarAsync(query, cancellationToken);
        return OkEnvelope(response);
    }

    [HttpGet("{fechamentoId:guid}")]
    /// <summary>
    /// Carrega o detalhe completo do fechamento selecionado.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<ClosingDetailResponse>>> GetById(
        Guid fechamentoId,
        CancellationToken cancellationToken)
    {
        var response = await _closingService.ObterDetalheAsync(fechamentoId, cancellationToken);
        return OkEnvelope(response);
    }

    [HttpPost]
    [RequirePermission(AccessPermissionCodes.FechamentoGerar)]
    /// <summary>
    /// Gera ou regenera o fechamento para a pessoa e periodo informados.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<ClosingDetailResponse>>> Generate(
        [FromBody] GenerateClosingRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _closingService.GerarAsync(request, cancellationToken);
        return OkEnvelope(response);
    }

    [HttpPost("{fechamentoId:guid}/review")]
    [RequirePermission(AccessPermissionCodes.FechamentoConferir)]
    /// <summary>
    /// Marca o fechamento como conferido.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<ClosingDetailResponse>>> Review(
        Guid fechamentoId,
        CancellationToken cancellationToken)
    {
        var response = await _closingService.ConferirAsync(fechamentoId, cancellationToken);
        return OkEnvelope(response);
    }

    [HttpPost("{fechamentoId:guid}/settle")]
    [RequirePermission(AccessPermissionCodes.FechamentoConferir)]
    /// <summary>
    /// Marca o fechamento como liquidado quando nao restam pendencias.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<ClosingDetailResponse>>> Settle(
        Guid fechamentoId,
        CancellationToken cancellationToken)
    {
        var response = await _closingService.LiquidarAsync(fechamentoId, cancellationToken);
        return OkEnvelope(response);
    }

    [HttpGet("{fechamentoId:guid}/export/pdf")]
    /// <summary>
    /// Retorna o arquivo imprimivel do fechamento para download.
    /// </summary>
    public async Task<IActionResult> ExportPdf(Guid fechamentoId, CancellationToken cancellationToken)
    {
        var response = await _closingService.ExportarPdfAsync(fechamentoId, cancellationToken);
        return File(response.Content, response.ContentType, response.FileName);
    }

    [HttpGet("{fechamentoId:guid}/export/excel")]
    /// <summary>
    /// Retorna o CSV compativel com Excel para download.
    /// </summary>
    public async Task<IActionResult> ExportExcel(Guid fechamentoId, CancellationToken cancellationToken)
    {
        var response = await _closingService.ExportarExcelAsync(fechamentoId, cancellationToken);
        return File(response.Content, response.ContentType, response.FileName);
    }
}
