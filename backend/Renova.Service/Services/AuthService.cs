using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands;

namespace Renova.Service.Services;

public class AuthService(RenovaDbContext context) : IAuthService
{
    private readonly RenovaDbContext _context = context;

    public Task<UsuarioTokenDto> CreateAsync(CadastroCommand request, CancellationToken cancellationToken = default)
    {
        _ = _context;
        _ = request;
        _ = cancellationToken;

        throw new NotImplementedException();
    }
}
