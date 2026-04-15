using Renova.Domain.Model.Dto;
using Renova.Service.Commands.Cliente;
using Renova.Service.Parameters.Cliente;
using Renova.Service.Queries.Cliente;

namespace Renova.Service.Services.Cliente
{
    public interface IClienteService
    {
        Task<ClienteDto> CreateAsync(CriarClienteCommand request, CriarClienteParametros parametros, CancellationToken cancellationToken = default);
        Task DeleteAsync(ExcluirClienteParametros parametros, CancellationToken cancellationToken = default);
        Task<ClienteDto> EditAsync(EditarClienteCommand request, EditarClienteParametros parametros, CancellationToken cancellationToken = default);
        Task<byte[]> ExportClosingAsync(ExportarFechamentoClientesQuery request, ObterClientesParametros parametros, CancellationToken cancellationToken = default);
        Task<PaginacaoDto<ClienteDto>> GetAllAsync(ObterClientesQuery request, ObterClientesParametros parametros, CancellationToken cancellationToken = default);
        Task<ClienteDetalheDto> GetDetailAsync(ObterClienteDetalheQuery request, ObterClienteDetalheParametros parametros, CancellationToken cancellationToken = default);
    }
}
