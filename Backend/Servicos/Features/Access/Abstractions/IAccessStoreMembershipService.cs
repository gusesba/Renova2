using Renova.Services.Features.Access.Contracts;

namespace Renova.Services.Features.Access.Abstractions;

// Representa o contrato de vinculacao de usuarios com lojas.
public interface IAccessStoreMembershipService
{
    /// <summary>
    /// Lista os vinculos da loja ativa.
    /// </summary>
    Task<IReadOnlyList<StoreMembershipResponse>> ListarAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Cria um vinculo de usuario com a loja ativa.
    /// </summary>
    Task<StoreMembershipResponse> CriarAsync(CreateStoreMembershipRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza status e dados operacionais do vinculo.
    /// </summary>
    Task<StoreMembershipResponse> AtualizarAsync(Guid usuarioLojaId, UpdateStoreMembershipRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza os cargos associados a um vinculo existente.
    /// </summary>
    Task<StoreMembershipResponse> AtualizarCargosAsync(Guid usuarioLojaId, UpdateStoreMembershipRolesRequest request, CancellationToken cancellationToken = default);
}
