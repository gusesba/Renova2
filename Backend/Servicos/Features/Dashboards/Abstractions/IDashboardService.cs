using Renova.Services.Features.Dashboards.Contracts;

namespace Renova.Services.Features.Dashboards.Abstractions;

// Define as operacoes de workspace e consulta consolidada do modulo 14.
public interface IDashboardService
{
    Task<DashboardWorkspaceResponse> ObterWorkspaceAsync(CancellationToken cancellationToken = default);

    Task<DashboardOverviewResponse> ObterVisaoGeralAsync(
        DashboardQueryRequest query,
        CancellationToken cancellationToken = default);
}
