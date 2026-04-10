using Renova.Domain.Model.Dto;
using Renova.Service.Commands.Loja;
using Renova.Service.Parameters.Loja;

namespace Renova.Service.Services.Loja
{
    public interface ILojaService
    {
        Task<LojaDto> CreateAsync(CriarLojaCommand request, CriarLojaParametros parametros, CancellationToken cancellationToken = default);
        Task DeleteAsync(ExcluirLojaParametros parametros, CancellationToken cancellationToken = default);
        Task<LojaDto> EditAsync(EditarLojaCommand request, EditarLojaParametros parametros, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<LojaDto>> GetAllAsync(ObterLojasParametros parametros, CancellationToken cancellationToken = default);
    }
}
