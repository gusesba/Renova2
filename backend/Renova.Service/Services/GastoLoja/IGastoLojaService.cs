using Renova.Domain.Model.Dto;
using Renova.Service.Commands.GastoLoja;
using Renova.Service.Parameters.GastoLoja;
using Renova.Service.Queries.GastoLoja;

namespace Renova.Service.Services.GastoLoja
{
    public interface IGastoLojaService
    {
        Task<PaginacaoDto<GastoLojaBuscaDto>> GetAllAsync(
            ObterGastosLojaQuery request,
            OperacaoGastoLojaParametros parametros,
            CancellationToken cancellationToken = default);

        Task<GastoLojaDto> CreateAsync(
            CriarGastoLojaCommand request,
            OperacaoGastoLojaParametros parametros,
            CancellationToken cancellationToken = default);
    }
}
