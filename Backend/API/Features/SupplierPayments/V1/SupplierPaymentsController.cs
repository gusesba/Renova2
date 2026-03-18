using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Renova.Api.Common.Controllers;
using Renova.Api.Common.Responses;
using Renova.Api.Infrastructure.Security;
using Renova.Services.Features.Access;
using Renova.Services.Features.SupplierPayments.Abstractions;
using Renova.Services.Features.SupplierPayments.Contracts;

namespace Renova.Api.Features.SupplierPayments.V1;

// Controller HTTP do modulo 11 com consulta de obrigacoes e liquidacao de repasses.
[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/supplier-payments")]
public sealed class SupplierPaymentsController : RenovaControllerBase
{
    private readonly ISupplierPaymentService _supplierPaymentService;

    /// <summary>
    /// Inicializa o controller com o service principal do modulo.
    /// </summary>
    public SupplierPaymentsController(ISupplierPaymentService supplierPaymentService)
    {
        _supplierPaymentService = supplierPaymentService;
    }

    [HttpGet("workspace")]
    /// <summary>
    /// Carrega filtros, fornecedores e meios de pagamento da loja ativa.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<SupplierPaymentWorkspaceResponse>>> GetWorkspace(CancellationToken cancellationToken)
    {
        var response = await _supplierPaymentService.ObterWorkspaceAsync(cancellationToken);
        return OkEnvelope(response);
    }

    [HttpGet]
    /// <summary>
    /// Lista as obrigacoes da loja ativa com filtros simples.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<IReadOnlyList<SupplierObligationSummaryResponse>>>> List(
        [FromQuery] SupplierPaymentListQueryRequest query,
        CancellationToken cancellationToken)
    {
        var response = await _supplierPaymentService.ListarAsync(query, cancellationToken);
        return OkEnvelope(response);
    }

    [HttpGet("{obrigacaoId:guid}")]
    /// <summary>
    /// Carrega o detalhe da obrigacao com historico de pagamentos.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<SupplierObligationDetailResponse>>> GetById(Guid obrigacaoId, CancellationToken cancellationToken)
    {
        var response = await _supplierPaymentService.ObterDetalheAsync(obrigacaoId, cancellationToken);
        return OkEnvelope(response);
    }

    [HttpPost("{obrigacaoId:guid}/settle")]
    [RequirePermission(AccessPermissionCodes.FinanceiroConciliar)]
    /// <summary>
    /// Liquida total ou parcialmente a obrigacao do fornecedor.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<SupplierObligationDetailResponse>>> Settle(
        Guid obrigacaoId,
        [FromBody] SettleSupplierObligationRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _supplierPaymentService.LiquidarAsync(obrigacaoId, request, cancellationToken);
        return OkEnvelope(response);
    }
}
