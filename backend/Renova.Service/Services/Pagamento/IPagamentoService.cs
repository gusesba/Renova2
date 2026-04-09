using Renova.Domain.Model.Dto;
using Renova.Service.Commands.Pagamento;

namespace Renova.Service.Services.Pagamento
{
    public interface IPagamentoService
    {
        Task<IReadOnlyList<PagamentoDto>> CreateAsync(CriarPagamentoCommand request, CancellationToken cancellationToken = default);
    }
}
