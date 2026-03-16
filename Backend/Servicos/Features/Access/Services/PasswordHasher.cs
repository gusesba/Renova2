using System.Security.Cryptography;
using Renova.Services.Features.Access.Abstractions;

namespace Renova.Services.Features.Access.Services;

// Representa o servico de hash de senha com PBKDF2.
public sealed class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100_000;

    /// <summary>
    /// Gera hash e salt seguros para armazenamento de senha.
    /// </summary>
    public PasswordHashResult Hash(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException("A senha é obrigatória.");
        }

        var saltBytes = RandomNumberGenerator.GetBytes(SaltSize);
        var hashBytes = Rfc2898DeriveBytes.Pbkdf2(
            password,
            saltBytes,
            Iterations,
            HashAlgorithmName.SHA512,
            HashSize);

        return new PasswordHashResult(
            Convert.ToBase64String(hashBytes),
            Convert.ToBase64String(saltBytes));
    }

    /// <summary>
    /// Verifica se a senha informada corresponde ao hash armazenado.
    /// </summary>
    public bool Verify(string password, string passwordHash, string salt)
    {
        if (string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(passwordHash) ||
            string.IsNullOrWhiteSpace(salt))
        {
            return false;
        }

        var expectedHash = Convert.FromBase64String(passwordHash);
        var saltBytes = Convert.FromBase64String(salt);
        var actualHash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            saltBytes,
            Iterations,
            HashAlgorithmName.SHA512,
            expectedHash.Length);

        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }
}
