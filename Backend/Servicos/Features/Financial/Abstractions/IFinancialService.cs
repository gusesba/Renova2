using Renova.Services.Features.Financial.Contracts;

namespace Renova.Services.Features.Financial.Abstractions;

// Define as operacoes de livro razao, conciliacao e lancamentos financeiros.
public interface IFinancialService
{
    Task<FinancialWorkspaceResponse> ObterWorkspaceAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FinancialLedgerEntryResponse>> ListarAsync(
        FinancialListQueryRequest query,
        CancellationToken cancellationToken = default);

    Task<FinancialReconciliationResponse> ObterConciliacaoAsync(
        FinancialListQueryRequest query,
        CancellationToken cancellationToken = default);

    Task<FinancialLedgerEntryResponse> RegistrarLancamentoAsync(
        RegisterFinancialEntryRequest request,
        CancellationToken cancellationToken = default);
}
