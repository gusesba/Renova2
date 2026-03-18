using Renova.Services.Features.StockMovements.Contracts;

namespace Renova.Services.Features.StockMovements.Abstractions;

// Expone a regra reutilizavel que bloqueia vendas sem saldo disponivel.
public interface IStockAvailabilityService
{
    /// <summary>
    /// Garante que a peca pode ser vendida na quantidade solicitada.
    /// </summary>
    Task EnsureSaleAvailabilityAsync(
        Guid lojaId,
        StockSaleAvailabilityRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Garante que todas as pecas informadas podem ser vendidas.
    /// </summary>
    Task EnsureSaleAvailabilityAsync(
        Guid lojaId,
        IReadOnlyCollection<StockSaleAvailabilityRequest> requests,
        CancellationToken cancellationToken = default);
}
