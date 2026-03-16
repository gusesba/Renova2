namespace Renova.Services.Features.Access.Contracts;

// Representa os contratos usados na manutencao de usuarios.
public sealed record CreateUserRequest(
    string Nome,
    string Email,
    string Telefone,
    string Senha,
    Guid? PessoaId);

public sealed record UpdateUserRequest(
    string Nome,
    string Email,
    string Telefone,
    Guid? PessoaId);

public sealed record ChangeUserStatusRequest(string StatusUsuario);

public sealed record UserSummaryResponse(
    Guid Id,
    string Nome,
    string Email,
    string Telefone,
    string StatusUsuario,
    Guid? PessoaId,
    StoreMembershipSummaryResponse? VinculoLojaAtiva);

public sealed record StoreMembershipSummaryResponse(
    Guid Id,
    string StatusVinculo,
    bool EhResponsavel,
    IReadOnlyList<RoleReferenceResponse> Cargos);
