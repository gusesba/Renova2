using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.Cliente;
using Renova.Service.Parameters.Cliente;

namespace Renova.Service.Services.Cliente
{
    public class ClienteService(RenovaDbContext context) : IClienteService
    {
        private readonly RenovaDbContext _context = context;

        public Task<ClienteDto> CreateAsync(CriarClienteCommand request, CriarClienteParametros parametros, CancellationToken cancellationToken = default)
        {
            _ = _context;
            _ = request;
            _ = parametros;
            _ = cancellationToken;

            throw new NotImplementedException("Criacao de cliente ainda nao foi implementada.");
        }
    }
}
