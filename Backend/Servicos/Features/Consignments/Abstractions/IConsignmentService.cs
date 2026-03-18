using Renova.Services.Features.Consignments.Contracts;

namespace Renova.Services.Features.Consignments.Abstractions;

// Define o contrato de aplicacao do modulo 07.
public interface IConsignmentService
{
    /// <summary>
    /// Carrega o resumo, filtros e opcoes auxiliares do modulo.
    /// </summary>
    Task<ConsignmentWorkspaceResponse> ObterWorkspaceAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista as pecas consignadas da loja ativa com indicadores do ciclo de vida.
    /// </summary>
    Task<IReadOnlyList<ConsignmentPieceSummaryResponse>> ListarAsync(
        ConsignmentListQueryRequest query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Carrega o detalhe operacional de uma peca consignada.
    /// </summary>
    Task<ConsignmentDetailResponse> ObterDetalheAsync(Guid pecaId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Aplica o desconto automatico devido pela permanencia da peca na loja.
    /// </summary>
    Task<ApplyConsignmentDiscountResponse> AplicarDescontoAsync(Guid pecaId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Encerra a consignacao com devolucao, doacao, perda ou descarte.
    /// </summary>
    Task<CloseConsignmentResponse> EncerrarAsync(
        Guid pecaId,
        CloseConsignmentRequest request,
        CancellationToken cancellationToken = default);
}
