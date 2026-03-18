using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Renova.Api.Common.Controllers;
using Renova.Api.Common.Responses;
using Renova.Api.Infrastructure.Security;
using Renova.Services.Features.Access;
using Renova.Services.Features.StockMovements.Abstractions;
using Renova.Services.Features.StockMovements.Contracts;

namespace Renova.Api.Features.StockMovements.V1;

// Controller HTTP do modulo 08 com consulta e ajustes manuais de estoque.
[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/stock-movements")]
public sealed class StockMovementsController : RenovaControllerBase
{
    private readonly IStockMovementService _stockMovementService;

    /// <summary>
    /// Inicializa o controller com o service principal do modulo.
    /// </summary>
    public StockMovementsController(IStockMovementService stockMovementService)
    {
        _stockMovementService = stockMovementService;
    }

    [HttpGet("workspace")]
    /// <summary>
    /// Carrega resumo e opcoes auxiliares da loja ativa para o modulo.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<StockMovementWorkspaceResponse>>> GetWorkspace(CancellationToken cancellationToken)
    {
        var response = await _stockMovementService.ObterWorkspaceAsync(cancellationToken);
        return OkEnvelope(response);
    }

    [HttpGet]
    /// <summary>
    /// Lista as movimentacoes de estoque da loja ativa com filtros operacionais.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<IReadOnlyList<StockMovementItemResponse>>>> List(
        [FromQuery] StockMovementListQueryRequest query,
        CancellationToken cancellationToken)
    {
        var response = await _stockMovementService.ListarAsync(query, cancellationToken);
        return OkEnvelope(response);
    }

    [HttpGet("pieces")]
    /// <summary>
    /// Busca pecas da loja ativa por codigo, fornecedor, status e tempo em loja.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<IReadOnlyList<StockPieceLookupResponse>>>> SearchPieces(
        [FromQuery] StockPieceSearchQueryRequest query,
        CancellationToken cancellationToken)
    {
        var response = await _stockMovementService.BuscarPecasAsync(query, cancellationToken);
        return OkEnvelope(response);
    }

    [HttpPost("adjustments")]
    [RequirePermission(AccessPermissionCodes.PecasAjustar)]
    /// <summary>
    /// Registra um ajuste manual alterando saldo e opcionalmente o status da peca.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<AdjustStockResponse>>> Adjust(
        [FromBody] AdjustStockRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _stockMovementService.AjustarAsync(request, cancellationToken);
        return OkEnvelope(response);
    }
}
