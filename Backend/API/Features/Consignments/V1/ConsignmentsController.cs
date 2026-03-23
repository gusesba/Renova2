using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Renova.Api.Common.Controllers;
using Renova.Api.Common.Responses;
using Renova.Api.Infrastructure.Security;
using Renova.Services.Features.Access;
using Renova.Services.Features.Consignments.Abstractions;
using Renova.Services.Features.Consignments.Contracts;

namespace Renova.Api.Features.Consignments.V1;

// Controller HTTP do modulo 07 com consulta e operacoes do ciclo de vida da consignacao.
[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/consignments")]
public sealed class ConsignmentsController : RenovaControllerBase
{
    private readonly IConsignmentService _consignmentService;

    /// <summary>
    /// Inicializa o controller com o service do modulo.
    /// </summary>
    public ConsignmentsController(IConsignmentService consignmentService)
    {
        _consignmentService = consignmentService;
    }

    [HttpGet("workspace")]
    /// <summary>
    /// Carrega resumo, filtros e acoes disponiveis da consignacao.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<ConsignmentWorkspaceResponse>>> GetWorkspace(CancellationToken cancellationToken)
    {
        var response = await _consignmentService.ObterWorkspaceAsync(cancellationToken);
        return OkEnvelope(response);
    }

    [HttpGet]
    /// <summary>
    /// Lista as pecas consignadas da loja ativa com filtros e indicadores do ciclo.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<IReadOnlyList<ConsignmentPieceSummaryResponse>>>> List(
        [FromQuery] ConsignmentListQueryRequest query,
        CancellationToken cancellationToken)
    {
        var response = await _consignmentService.ListarAsync(query, cancellationToken);
        return OkEnvelope(response);
    }

    [HttpGet("{pecaId:guid}")]
    /// <summary>
    /// Carrega o detalhe operacional de uma peca consignada.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<ConsignmentDetailResponse>>> GetById(
        Guid pecaId,
        CancellationToken cancellationToken)
    {
        var response = await _consignmentService.ObterDetalheAsync(pecaId, cancellationToken);
        return OkEnvelope(response);
    }

    [HttpPost("{pecaId:guid}/close")]
    [RequirePermission(AccessPermissionCodes.PecasAjustar)]
    /// <summary>
    /// Encerra a consignacao com devolucao, doacao, perda ou descarte.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<CloseConsignmentResponse>>> Close(
        Guid pecaId,
        [FromBody] CloseConsignmentRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _consignmentService.EncerrarAsync(pecaId, request, cancellationToken);
        return OkEnvelope(response);
    }
}
