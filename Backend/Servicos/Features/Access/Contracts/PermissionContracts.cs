namespace Renova.Services.Features.Access.Contracts;

// Representa o contrato de leitura de uma permissao disponivel.
public sealed record PermissionResponse(
    Guid Id,
    string Codigo,
    string Nome,
    string Descricao,
    string Modulo,
    bool Ativo);
