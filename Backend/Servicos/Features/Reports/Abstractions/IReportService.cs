using Renova.Services.Features.Reports.Contracts;

namespace Renova.Services.Features.Reports.Abstractions;

// Define as operacoes de consulta, exportacao e persistencia de filtros do modulo 15.
public interface IReportService
{
    Task<ReportWorkspaceResponse> ObterWorkspaceAsync(CancellationToken cancellationToken = default);

    Task<ReportResultResponse> ExecutarAsync(ReportQueryRequest query, CancellationToken cancellationToken = default);

    Task<ReportExportFileResponse> ExportarAsync(
        string format,
        ReportQueryRequest query,
        CancellationToken cancellationToken = default);

    Task<SavedReportFilterResponse> SalvarFiltroAsync(
        SaveReportFilterRequest request,
        CancellationToken cancellationToken = default);

    Task RemoverFiltroAsync(Guid filtroId, CancellationToken cancellationToken = default);
}
