using Renova.Domain.Model.Dto;
using Renova.Service.Commands.ConfigLoja;
using Renova.Service.Parameters.ConfigLoja;

namespace Renova.Service.Services.ConfigLoja
{
    public interface IConfigLojaService
    {
        Task<ConfigLojaDto> GetAsync(int lojaId, ObterConfigLojaParametros parametros, CancellationToken cancellationToken = default);
        Task<ConfigLojaDto> SaveAsync(SalvarConfigLojaCommand request, SalvarConfigLojaParametros parametros, CancellationToken cancellationToken = default);
    }
}
