using Renova.Services.Features.Access.Contracts;

namespace Renova.Services.Features.Access.Abstractions;

// Representa o contrato de manutencao de usuarios do modulo de acesso.
public interface IAccessUserService
{
    /// <summary>
    /// Lista os usuarios com o resumo do vinculo atual.
    /// </summary>
    Task<IReadOnlyList<UserSummaryResponse>> ListarAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Cria um novo usuario do sistema.
    /// </summary>
    Task<UserSummaryResponse> CriarAsync(CreateUserRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza os dados cadastrais de um usuario.
    /// </summary>
    Task<UserSummaryResponse> AtualizarAsync(Guid usuarioId, UpdateUserRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza o status operacional de um usuario.
    /// </summary>
    Task<UserSummaryResponse> AlterarStatusAsync(Guid usuarioId, ChangeUserStatusRequest request, CancellationToken cancellationToken = default);
}
