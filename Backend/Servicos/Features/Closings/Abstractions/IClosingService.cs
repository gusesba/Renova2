using Renova.Services.Features.Closings.Contracts;

namespace Renova.Services.Features.Closings.Abstractions;

// Define as operacoes de geracao, consulta, conferencia e exportacao do modulo 13.
public interface IClosingService
{
    Task<ClosingWorkspaceResponse> ObterWorkspaceAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ClosingSummaryResponse>> ListarAsync(
        ClosingListQueryRequest query,
        CancellationToken cancellationToken = default);

    Task<ClosingDetailResponse> ObterDetalheAsync(
        Guid fechamentoId,
        CancellationToken cancellationToken = default);

    Task<ClosingDetailResponse> GerarAsync(
        GenerateClosingRequest request,
        CancellationToken cancellationToken = default);

    Task<ClosingDetailResponse> ConferirAsync(
        Guid fechamentoId,
        CancellationToken cancellationToken = default);

    Task<ClosingDetailResponse> LiquidarAsync(
        Guid fechamentoId,
        CancellationToken cancellationToken = default);

    Task<ClosingExportResponse> ExportarPdfAsync(
        Guid fechamentoId,
        CancellationToken cancellationToken = default);

    Task<ClosingExportResponse> ExportarExcelAsync(
        Guid fechamentoId,
        CancellationToken cancellationToken = default);
}
