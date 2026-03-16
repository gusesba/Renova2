namespace Renova.Services.Features.Access.Contracts;

// Representa os contratos usados na vinculacao de usuarios com lojas.
public sealed record CreateStoreMembershipRequest(
    Guid UsuarioId,
    string StatusVinculo,
    bool EhResponsavel,
    DateTimeOffset? DataFim,
    IReadOnlyList<Guid> CargoIds);

public sealed record UpdateStoreMembershipRequest(
    string StatusVinculo,
    bool EhResponsavel,
    DateTimeOffset? DataFim);

public sealed record UpdateStoreMembershipRolesRequest(IReadOnlyList<Guid> CargoIds);

public sealed record StoreMembershipResponse(
    Guid Id,
    Guid UsuarioId,
    string UsuarioNome,
    string UsuarioEmail,
    string StatusVinculo,
    bool EhResponsavel,
    DateTimeOffset DataInicio,
    DateTimeOffset? DataFim,
    IReadOnlyList<RoleReferenceResponse> Cargos);
