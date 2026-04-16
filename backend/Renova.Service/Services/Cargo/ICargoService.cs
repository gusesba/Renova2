using Renova.Domain.Model.Dto;
using Renova.Service.Commands.Cargo;
using Renova.Service.Parameters.Cargo;

namespace Renova.Service.Services.Cargo
{
    public interface ICargoService
    {
        Task<IReadOnlyList<CargoDto>> GetAllAsync(OperacaoCargoParametros parametros, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<FuncionalidadeDto>> GetFuncionalidadesAsync(OperacaoCargoParametros parametros, CancellationToken cancellationToken = default);
        Task<CargoDto> CreateAsync(CriarCargoCommand request, OperacaoCargoParametros parametros, CancellationToken cancellationToken = default);
        Task<CargoDto> EditAsync(EditarCargoCommand request, OperacaoCargoParametros parametros, CancellationToken cancellationToken = default);
        Task DeleteAsync(OperacaoCargoParametros parametros, CancellationToken cancellationToken = default);
    }
}
