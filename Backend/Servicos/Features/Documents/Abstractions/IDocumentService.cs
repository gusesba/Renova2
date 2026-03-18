using Renova.Services.Features.Documents.Contracts;

namespace Renova.Services.Features.Documents.Abstractions;

// Define as operacoes de busca e impressao do modulo 16.
public interface IDocumentService
{
    Task<DocumentWorkspaceResponse> ObterWorkspaceAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DocumentSearchItemResponse>> BuscarEtiquetasAsync(
        DocumentSearchQueryRequest query,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DocumentSearchItemResponse>> BuscarRecibosVendaAsync(
        DocumentSearchQueryRequest query,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DocumentSearchItemResponse>> BuscarComprovantesFornecedorAsync(
        DocumentSearchQueryRequest query,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DocumentSearchItemResponse>> BuscarComprovantesConsignacaoAsync(
        DocumentSearchQueryRequest query,
        CancellationToken cancellationToken = default);

    Task<PrintableDocumentFileResponse> ImprimirEtiquetaAsync(Guid pecaId, CancellationToken cancellationToken = default);

    Task<PrintableDocumentFileResponse> ImprimirReciboVendaAsync(Guid vendaId, CancellationToken cancellationToken = default);

    Task<PrintableDocumentFileResponse> ImprimirComprovanteFornecedorAsync(Guid obrigacaoId, CancellationToken cancellationToken = default);

    Task<PrintableDocumentFileResponse> ImprimirComprovanteConsignacaoAsync(Guid pecaId, CancellationToken cancellationToken = default);
}
