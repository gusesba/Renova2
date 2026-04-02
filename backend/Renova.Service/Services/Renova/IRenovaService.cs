using Renova.Domain.Model;
using Renova.Service.Commands.Renova;
using Renova.Service.Queries.Renova;

namespace Renova.Service.Services.Renova
{
    public interface IRenovaService
    {
        Task<RenovaModel?> GetAsync(RenovaQuery request, CancellationToken cancellationToken = default);
        Task<RenovaModel> CreateAsync(RenovaCommand request, CancellationToken cancellationToken = default);
    }
}