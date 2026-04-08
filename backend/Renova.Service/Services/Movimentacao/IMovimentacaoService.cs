using Renova.Domain.Model.Dto;
using Renova.Service.Commands.Movimentacao;
using Renova.Service.Parameters.Movimentacao;

namespace Renova.Service.Services.Movimentacao
{
    public interface IMovimentacaoService
    {
        Task<MovimentacaoDto> CreateAsync(CriarMovimentacaoCommand request, CriarMovimentacaoParametros parametros, CancellationToken cancellationToken = default);
    }
}
