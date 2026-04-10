using Renova.Domain.Model.Dto;
using Renova.Service.Commands.Pagamento;
using Renova.Service.Parameters.Pagamento;
using Renova.Service.Queries.Pagamento;

namespace Renova.Service.Services.Pagamento
{
    public interface IPagamentoService
    {
        Task<PaginacaoDto<PagamentoBuscaDto>> GetAllAsync(ObterPagamentosQuery request, ObterPagamentosParametros parametros, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<PagamentoDto>> CreateAsync(CriarPagamentoCommand request, CancellationToken cancellationToken = default);
        Task<PagamentoCreditoDto> CreateCreditoAsync(CriarPagamentoCreditoCommand request, CriarPagamentoCreditoParametros parametros, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ClientePendenciaDto>> GetPendenciasAsync(int lojaId, int usuarioId, CancellationToken cancellationToken = default);
        Task<AtualizarPendenciasDto> UpdatePendenciasAsync(AtualizarPendenciasCommand request, AtualizarPendenciasParametros parametros, CancellationToken cancellationToken = default);
    }
}
