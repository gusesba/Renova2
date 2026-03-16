namespace Renova.Services.Features.Access.Abstractions;

// Representa o contrato de emissao e hash de tokens opacos.
public interface ITokenService
{
    /// <summary>
    /// Gera um novo token bruto e o hash correspondente.
    /// </summary>
    TokenGenerationResult Generate();

    /// <summary>
    /// Gera o hash persistivel de um token bruto.
    /// </summary>
    string Hash(string rawToken);
}

// Representa o par de token bruto e hash usado na autenticacao.
public sealed record TokenGenerationResult(string RawToken, string TokenHash);
