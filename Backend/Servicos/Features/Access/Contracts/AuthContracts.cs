namespace Renova.Services.Features.Access.Contracts;

// Representa os contratos de request e response do fluxo de autenticacao.
public sealed record LoginRequest(string Email, string Senha);

public sealed record RegisterRequest(
    string Nome,
    string Email,
    string Telefone,
    string Senha);

public sealed record LoginResponse(
    string Token,
    DateTimeOffset ExpiraEm,
    LoginContextResponse Contexto);

public sealed record RegisterResponse(
    string Mensagem,
    AuthenticatedUserResponse Usuario);

public sealed record LoginContextResponse(
    AuthenticatedUserResponse Usuario,
    Guid? LojaAtivaId,
    IReadOnlyList<AccessibleStoreResponse> Lojas,
    IReadOnlyList<string> Permissoes);

public sealed record AuthenticatedUserResponse(
    Guid Id,
    string Nome,
    string Email,
    string Telefone,
    string StatusUsuario,
    Guid? PessoaId);

public sealed record AccessibleStoreResponse(
    Guid Id,
    string Nome,
    string StatusVinculo,
    bool EhResponsavel,
    IReadOnlyList<RoleReferenceResponse> Cargos);

public sealed record SwitchActiveStoreRequest(Guid LojaId);

public sealed record PasswordResetRequest(string Email);

public sealed record PasswordResetRequestResponse(
    string Mensagem,
    string? TokenRecuperacao,
    DateTimeOffset? ExpiraEm);

public sealed record ConfirmPasswordResetRequest(string Token, string NovaSenha);

public sealed record ChangePasswordRequest(string SenhaAtual, string NovaSenha);
