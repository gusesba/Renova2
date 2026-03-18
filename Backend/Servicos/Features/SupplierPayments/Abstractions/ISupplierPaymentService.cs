using Renova.Services.Features.SupplierPayments.Contracts;

namespace Renova.Services.Features.SupplierPayments.Abstractions;

// Define os casos de uso do modulo 11 para API e clientes futuros.
public interface ISupplierPaymentService
{
    /// <summary>
    /// Carrega filtros e listas auxiliares do modulo na loja ativa.
    /// </summary>
    Task<SupplierPaymentWorkspaceResponse> ObterWorkspaceAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista as obrigacoes da loja ativa com filtros por fornecedor, tipo e status.
    /// </summary>
    Task<IReadOnlyList<SupplierObligationSummaryResponse>> ListarAsync(
        SupplierPaymentListQueryRequest query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Carrega o detalhe completo da obrigacao com historico de liquidacoes.
    /// </summary>
    Task<SupplierObligationDetailResponse> ObterDetalheAsync(Guid obrigacaoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Liquida total ou parcialmente a obrigacao com meio financeiro, credito ou ambos.
    /// </summary>
    Task<SupplierObligationDetailResponse> LiquidarAsync(
        Guid obrigacaoId,
        SettleSupplierObligationRequest request,
        CancellationToken cancellationToken = default);
}
