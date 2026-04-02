using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.Loja;
using Renova.Service.Parameters.Loja;

namespace Renova.Service.Services.Loja
{
    public class LojaService(RenovaDbContext context) : ILojaService
    {
        private readonly RenovaDbContext _context = context;

        public Task<LojaDto> CreateAsync(CriarLojaCommand request, CriarLojaParametros parametros, CancellationToken cancellationToken = default)
        {
            _ = _context;
            _ = request;
            _ = parametros;
            _ = cancellationToken;

            throw new NotImplementedException();
        }
    }
}
