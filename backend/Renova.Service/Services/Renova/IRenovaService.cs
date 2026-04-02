using Renova.Domain.Model;
using Renova.Service.Commands;
using Renova.Service.Queries;

namespace Renova.Service.Services;

public interface IRenovaService
{
    Task<RenovaModel?> GetAsync(RenovaQuery request, CancellationToken cancellationToken = default);
    Task<RenovaModel> CreateAsync(RenovaCommand request, CancellationToken cancellationToken = default);
}
