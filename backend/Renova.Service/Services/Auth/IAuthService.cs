using Renova.Domain.Model.Dto;
using Renova.Service.Commands.Auth;

namespace Renova.Service.Services.Auth;

public interface IAuthService
{
    Task<UsuarioTokenDto> CreateAsync(CadastroCommand request, CancellationToken cancellationToken = default);
}
