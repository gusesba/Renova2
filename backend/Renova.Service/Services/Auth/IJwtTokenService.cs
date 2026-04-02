using Renova.Domain.Model;

namespace Renova.Service.Services.Auth;

public interface IJwtTokenService
{
    string GenerateToken(UsuarioModel usuario);
}
