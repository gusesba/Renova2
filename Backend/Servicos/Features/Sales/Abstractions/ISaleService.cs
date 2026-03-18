using Renova.Services.Features.Sales.Contracts;

namespace Renova.Services.Features.Sales.Abstractions;

// Define o contrato principal do modulo 09 para consulta, venda e cancelamento.
public interface ISaleService
{
    /// <summary>
    /// Carrega compradores, pecas e meios de pagamento da loja ativa.
    /// </summary>
    Task<SalesWorkspaceResponse> ObterWorkspaceAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista as vendas da loja ativa com filtros basicos.
    /// </summary>
    Task<IReadOnlyList<SaleSummaryResponse>> ListarAsync(
        SaleListQueryRequest query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Carrega o detalhe completo de uma venda da loja ativa.
    /// </summary>
    Task<SaleDetailResponse> ObterDetalheAsync(Guid vendaId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registra uma nova venda com itens, pagamentos e efeitos colaterais transacionais.
    /// </summary>
    Task<SaleDetailResponse> CriarAsync(CreateSaleRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancela uma venda concluida e executa os estornos necessarios.
    /// </summary>
    Task<SaleDetailResponse> CancelarAsync(
        Guid vendaId,
        CancelSaleRequest request,
        CancellationToken cancellationToken = default);
}
