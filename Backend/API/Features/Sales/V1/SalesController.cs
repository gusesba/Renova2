using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Renova.Api.Common.Controllers;
using Renova.Api.Common.Responses;
using Renova.Api.Infrastructure.Security;
using Renova.Services.Features.Access;
using Renova.Services.Features.Sales.Abstractions;
using Renova.Services.Features.Sales.Contracts;

namespace Renova.Api.Features.Sales.V1;

// Controller HTTP do modulo 09 com workspace, venda, consulta e cancelamento.
[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/sales")]
public sealed class SalesController : RenovaControllerBase
{
    private readonly ISaleService _saleService;

    /// <summary>
    /// Inicializa o controller com o service principal do modulo.
    /// </summary>
    public SalesController(ISaleService saleService)
    {
        _saleService = saleService;
    }

    [HttpGet("workspace")]
    /// <summary>
    /// Carrega compradores, pecas vendaveis e meios de pagamento da loja ativa.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<SalesWorkspaceResponse>>> GetWorkspace(CancellationToken cancellationToken)
    {
        var response = await _saleService.ObterWorkspaceAsync(cancellationToken);
        return OkEnvelope(response);
    }

    [HttpGet]
    /// <summary>
    /// Lista as vendas da loja ativa com filtros simples de periodo, comprador e status.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<IReadOnlyList<SaleSummaryResponse>>>> List(
        [FromQuery] SaleListQueryRequest query,
        CancellationToken cancellationToken)
    {
        var response = await _saleService.ListarAsync(query, cancellationToken);
        return OkEnvelope(response);
    }

    [HttpGet("{vendaId:guid}")]
    /// <summary>
    /// Carrega o detalhe completo da venda, incluindo itens, pagamentos e recibo textual.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<SaleDetailResponse>>> GetById(Guid vendaId, CancellationToken cancellationToken)
    {
        var response = await _saleService.ObterDetalheAsync(vendaId, cancellationToken);
        return OkEnvelope(response);
    }

    [HttpPost]
    [RequirePermission(AccessPermissionCodes.VendasRegistrar)]
    /// <summary>
    /// Conclui uma venda registrando estoque, pagamentos e efeitos financeiros.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<SaleDetailResponse>>> Create(
        [FromBody] CreateSaleRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _saleService.CriarAsync(request, cancellationToken);
        return OkEnvelope(response);
    }

    [HttpPost("{vendaId:guid}/cancel")]
    [RequirePermission(AccessPermissionCodes.VendasCancelar)]
    /// <summary>
    /// Cancela uma venda concluida e gera os estornos operacionais necessarios.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<SaleDetailResponse>>> Cancel(
        Guid vendaId,
        [FromBody] CancelSaleRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _saleService.CancelarAsync(vendaId, request, cancellationToken);
        return OkEnvelope(response);
    }
}
