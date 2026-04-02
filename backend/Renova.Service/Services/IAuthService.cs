using Renova.Domain.Model.Dto;
using Renova.Service.Commands;

namespace Renova.Service.Services;

public interface IAuthService
{
    Task<UsuarioTokenDto> CreateAsync(CadastroCommand request, CancellationToken cancellationToken = default);
}
