using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Renova.Api.Common.Controllers;
using Renova.Api.Common.Responses;
using Renova.Api.Infrastructure.Security;
using Renova.Services.Features.Access;
using Renova.Services.Features.CommercialRules.Abstractions;
using Renova.Services.Features.CommercialRules.Contracts;

namespace Renova.Api.Features.CommercialRules.V1;

// Controller HTTP do modulo 05 com regras comerciais e meios de pagamento.
[ApiVersion("1.0")]
[Authorize]
[RequirePermission(AccessPermissionCodes.RegrasGerenciar)]
[Route("api/v{version:apiVersion}/commercial-rules")]
public sealed class CommercialRulesController : RenovaControllerBase
{
    private readonly ICommercialRuleService _commercialRuleService;

    /// <summary>
    /// Inicializa o controller com o service do modulo.
    /// </summary>
    public CommercialRulesController(ICommercialRuleService commercialRuleService)
    {
        _commercialRuleService = commercialRuleService;
    }

    [HttpGet("workspace")]
    /// <summary>
    /// Carrega a configuracao comercial completa da loja ativa.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<CommercialRulesWorkspaceResponse>>> GetWorkspace(CancellationToken cancellationToken)
    {
        var response = await _commercialRuleService.ObterWorkspaceAsync(cancellationToken);
        return OkEnvelope(response);
    }

    [HttpPut("store-rule")]
    /// <summary>
    /// Cria ou atualiza a regra comercial padrao da loja ativa.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<StoreCommercialRuleResponse>>> SaveStoreRule(
        [FromBody] UpsertStoreCommercialRuleRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _commercialRuleService.SalvarRegraLojaAsync(request, cancellationToken);
        return OkEnvelope(response);
    }

    [HttpPost("supplier-rules")]
    /// <summary>
    /// Cria uma regra comercial especifica para um fornecedor da loja ativa.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<SupplierCommercialRuleResponse>>> CreateSupplierRule(
        [FromBody] CreateSupplierCommercialRuleRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _commercialRuleService.CriarRegraFornecedorAsync(request, cancellationToken);
        return OkEnvelope(response);
    }

    [HttpPut("supplier-rules/{regraFornecedorId:guid}")]
    /// <summary>
    /// Atualiza uma regra comercial de fornecedor ja existente.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<SupplierCommercialRuleResponse>>> UpdateSupplierRule(
        Guid regraFornecedorId,
        [FromBody] UpdateSupplierCommercialRuleRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _commercialRuleService.AtualizarRegraFornecedorAsync(regraFornecedorId, request, cancellationToken);
        return OkEnvelope(response);
    }

    [HttpPost("payment-methods")]
    /// <summary>
    /// Cria um meio de pagamento para a loja ativa.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<PaymentMethodResponse>>> CreatePaymentMethod(
        [FromBody] CreatePaymentMethodRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _commercialRuleService.CriarMeioPagamentoAsync(request, cancellationToken);
        return OkEnvelope(response);
    }

    [HttpPut("payment-methods/{meioPagamentoId:guid}")]
    /// <summary>
    /// Atualiza um meio de pagamento existente da loja ativa.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<PaymentMethodResponse>>> UpdatePaymentMethod(
        Guid meioPagamentoId,
        [FromBody] UpdatePaymentMethodRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _commercialRuleService.AtualizarMeioPagamentoAsync(meioPagamentoId, request, cancellationToken);
        return OkEnvelope(response);
    }
}
