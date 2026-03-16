using System.Security.Cryptography;
using System.Text;
using Renova.Services.Features.Access.Abstractions;

namespace Renova.Services.Features.Access.Services;

// Representa o servico que gera e resume tokens opacos.
public sealed class TokenService : ITokenService
{
    /// <summary>
    /// Gera um token aleatorio e retorna seu hash persistivel.
    /// </summary>
    public TokenGenerationResult Generate()
    {
        var rawToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        return new TokenGenerationResult(rawToken, Hash(rawToken));
    }

    /// <summary>
    /// Calcula o hash SHA256 do token bruto.
    /// </summary>
    public string Hash(string rawToken)
    {
        var tokenBytes = Encoding.UTF8.GetBytes(rawToken);
        var hashBytes = SHA256.HashData(tokenBytes);
        return Convert.ToHexString(hashBytes);
    }
}
