using Renova.Domain.Model;

namespace Renova.Service.Services;

public interface IJwtTokenService
{
    string GenerateToken(UsuarioModel usuario);
}
