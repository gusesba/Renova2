namespace Renova.Services.Features.Documents.Contracts;

// Reune os contratos HTTP e de aplicacao do modulo 16.
public sealed record DocumentTypeOptionResponse(string Codigo, string Nome, string Descricao);

public sealed record DocumentWorkspaceResponse(
    Guid LojaId,
    string LojaNome,
    IReadOnlyList<DocumentTypeOptionResponse> TiposDocumento);

public sealed record DocumentSearchQueryRequest(string? Search);

public sealed record DocumentSearchItemResponse(
    Guid Id,
    string Titulo,
    string Subtitulo,
    string Meta);

public sealed record PrintableDocumentFileResponse(
    string FileName,
    string ContentType,
    byte[] Content);
