using Renova.Services.Features.CommercialRules.Contracts;

namespace Renova.Services.Features.CommercialRules.Abstractions;

// Define as operacoes administrativas do modulo 05.
public interface ICommercialRuleService
{
    /// <summary>
    /// Carrega a configuracao comercial completa da loja ativa.
    /// </summary>
    Task<CommercialRulesWorkspaceResponse> ObterWorkspaceAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Cria ou atualiza a regra comercial padrao da loja ativa.
    /// </summary>
    Task<StoreCommercialRuleResponse> SalvarRegraLojaAsync(
        UpsertStoreCommercialRuleRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cria uma regra comercial especifica para um fornecedor da loja ativa.
    /// </summary>
    Task<SupplierCommercialRuleResponse> CriarRegraFornecedorAsync(
        CreateSupplierCommercialRuleRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza uma regra comercial ja cadastrada para fornecedor.
    /// </summary>
    Task<SupplierCommercialRuleResponse> AtualizarRegraFornecedorAsync(
        Guid regraFornecedorId,
        UpdateSupplierCommercialRuleRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cria um meio de pagamento para a loja ativa.
    /// </summary>
    Task<PaymentMethodResponse> CriarMeioPagamentoAsync(
        CreatePaymentMethodRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza um meio de pagamento existente da loja ativa.
    /// </summary>
    Task<PaymentMethodResponse> AtualizarMeioPagamentoAsync(
        Guid meioPagamentoId,
        UpdatePaymentMethodRequest request,
        CancellationToken cancellationToken = default);
}
