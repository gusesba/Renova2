namespace Renova.Services.Features.Access.Abstractions;

// Representa o contrato de hash e validacao de senha.
public interface IPasswordHasher
{
    /// <summary>
    /// Gera hash e salt para uma senha em texto puro.
    /// </summary>
    PasswordHashResult Hash(string password);

    /// <summary>
    /// Compara uma senha em texto puro com o hash persistido.
    /// </summary>
    bool Verify(string password, string passwordHash, string salt);
}

// Representa o resultado do processamento criptografico da senha.
public sealed record PasswordHashResult(string Hash, string Salt);
