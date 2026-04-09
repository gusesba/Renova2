using Renova.Domain.Model.Dto;
using Renova.Service.Commands.Pagamento;
using Renova.Service.Parameters.Pagamento;

namespace Renova.Service.Services.Pagamento
{
    public interface IPagamentoService
    {
        Task<IReadOnlyList<PagamentoDto>> CreateAsync(CriarPagamentoCommand request, CancellationToken cancellationToken = default);
        Task<PagamentoCreditoDto> CreateCreditoAsync(CriarPagamentoCreditoCommand request, CriarPagamentoCreditoParametros parametros, CancellationToken cancellationToken = default);
    }
}
