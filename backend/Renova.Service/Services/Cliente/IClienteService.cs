using Renova.Domain.Model.Dto;
using Renova.Service.Commands.Cliente;
using Renova.Service.Parameters.Cliente;

namespace Renova.Service.Services.Cliente
{
    public interface IClienteService
    {
        Task<ClienteDto> CreateAsync(CriarClienteCommand request, CriarClienteParametros parametros, CancellationToken cancellationToken = default);
    }
}
