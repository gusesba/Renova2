namespace Renova.Services.Features.Access.Contracts;

// Representa os contratos usados na manutencao de cargos e permissoes.
public sealed record CreateRoleRequest(string Nome, string Descricao, IReadOnlyList<Guid> PermissaoIds);

public sealed record UpdateRoleRequest(string Nome, string Descricao, bool Ativo);

public sealed record UpdateRolePermissionsRequest(IReadOnlyList<Guid> PermissaoIds);

public sealed record RoleResponse(
    Guid Id,
    string Nome,
    string Descricao,
    bool Ativo,
    IReadOnlyList<PermissionResponse> Permissoes);

public sealed record RoleReferenceResponse(Guid Id, string Nome);
