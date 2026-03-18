using Renova.Services.Features.StockMovements.Contracts;

namespace Renova.Services.Features.StockMovements.Abstractions;

// Define o contrato principal do modulo 08 para consulta e ajuste de estoque.
public interface IStockMovementService
{
    /// <summary>
    /// Carrega o resumo, filtros e opcoes auxiliares do modulo.
    /// </summary>
    Task<StockMovementWorkspaceResponse> ObterWorkspaceAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista as movimentacoes de estoque da loja ativa com filtros operacionais.
    /// </summary>
    Task<IReadOnlyList<StockMovementItemResponse>> ListarAsync(
        StockMovementListQueryRequest query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca pecas operacionais por codigo, fornecedor, status e tempo em loja.
    /// </summary>
    Task<IReadOnlyList<StockPieceLookupResponse>> BuscarPecasAsync(
        StockPieceSearchQueryRequest query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Registra um ajuste manual de estoque para a peca informada.
    /// </summary>
    Task<AdjustStockResponse> AjustarAsync(
        AdjustStockRequest request,
        CancellationToken cancellationToken = default);
}
