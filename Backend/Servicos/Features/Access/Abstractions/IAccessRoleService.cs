using Renova.Services.Features.Access.Contracts;

namespace Renova.Services.Features.Access.Abstractions;

// Representa o contrato de manutencao de cargos e permissoes da loja.
public interface IAccessRoleService
{
    /// <summary>
    /// Lista os cargos da loja ativa.
    /// </summary>
    Task<IReadOnlyList<RoleResponse>> ListarAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista o catalogo de permissoes disponivel para uso em cargos.
    /// </summary>
    Task<IReadOnlyList<PermissionResponse>> ListarPermissoesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Cria um novo cargo na loja ativa.
    /// </summary>
    Task<RoleResponse> CriarAsync(CreateRoleRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza os dados basicos de um cargo existente.
    /// </summary>
    Task<RoleResponse> AtualizarAsync(Guid cargoId, UpdateRoleRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza a lista de permissoes de um cargo.
    /// </summary>
    Task<RoleResponse> AtualizarPermissoesAsync(Guid cargoId, UpdateRolePermissionsRequest request, CancellationToken cancellationToken = default);
}
