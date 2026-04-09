using Renova.Domain.Model.Dto;
using Renova.Service.Commands.Movimentacao;
using Renova.Service.Parameters.Movimentacao;
using Renova.Service.Queries.Movimentacao;

namespace Renova.Service.Services.Movimentacao
{
    public interface IMovimentacaoService
    {
        Task<PaginacaoDto<MovimentacaoBuscaDto>> GetAllAsync(ObterMovimentacoesQuery request, ObterMovimentacoesParametros parametros, CancellationToken cancellationToken = default);
        Task<MovimentacaoDto> CreateAsync(CriarMovimentacaoCommand request, CriarMovimentacaoParametros parametros, CancellationToken cancellationToken = default);
        Task<MovimentacaoDestinacaoSugestaoDto> GetDestinacaoAsync(int lojaId, ObterMovimentacoesParametros parametros, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<MovimentacaoDto>> CreateDestinacaoAsync(CriarMovimentacaoDestinacaoCommand request, CriarMovimentacaoParametros parametros, CancellationToken cancellationToken = default);
    }
}
