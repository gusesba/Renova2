namespace Renova.Services.Features.Closings.Contracts;

// Reune os contratos HTTP e de aplicacao do modulo 13.
public sealed record ClosingOptionResponse(string Codigo, string Nome);

public sealed record ClosingPersonOptionResponse(
    Guid PessoaId,
    string Nome,
    string Documento,
    bool EhCliente,
    bool EhFornecedor,
    bool AceitaCreditoLoja);

public sealed record ClosingWorkspaceResponse(
    Guid LojaId,
    string LojaNome,
    IReadOnlyList<ClosingPersonOptionResponse> Pessoas,
    IReadOnlyList<ClosingOptionResponse> StatusFechamento,
    IReadOnlyList<ClosingOptionResponse> TiposMovimento);

public sealed record ClosingListQueryRequest(
    string? Search,
    Guid? PessoaId,
    string? StatusFechamento,
    DateOnly? DataInicial,
    DateOnly? DataFinal);

public sealed record GenerateClosingRequest(
    Guid PessoaId,
    DateOnly PeriodoInicio,
    DateOnly PeriodoFim);

public sealed record ClosingSummaryResponse(
    Guid Id,
    Guid PessoaId,
    string PessoaNome,
    string PessoaDocumento,
    bool EhCliente,
    bool EhFornecedor,
    DateTimeOffset PeriodoInicio,
    DateTimeOffset PeriodoFim,
    string StatusFechamento,
    decimal ValorVendido,
    decimal ValorAReceber,
    decimal ValorPago,
    decimal ValorCompradoNaLoja,
    decimal SaldoCreditoAtual,
    decimal SaldoFinal,
    int QuantidadePecasAtuais,
    int QuantidadePecasVendidas,
    DateTimeOffset GeradoEm,
    Guid GeradoPorUsuarioId,
    string GeradoPorUsuarioNome,
    DateTimeOffset? ConferidoEm,
    Guid? ConferidoPorUsuarioId,
    string? ConferidoPorUsuarioNome,
    string ResumoTexto,
    string? PdfUrl,
    string? ExcelUrl);

public sealed record ClosingItemResponse(
    Guid Id,
    Guid PecaId,
    string GrupoItem,
    string CodigoInternoPeca,
    string ProdutoNomePeca,
    string StatusPecaSnapshot,
    decimal? ValorVendaSnapshot,
    decimal? ValorRepasseSnapshot,
    DateTimeOffset DataEvento);

public sealed record ClosingMovementResponse(
    Guid Id,
    string TipoMovimento,
    string OrigemTipo,
    Guid? OrigemId,
    DateTimeOffset DataMovimento,
    string Descricao,
    decimal Valor);

public sealed record ClosingDetailResponse(
    ClosingSummaryResponse Fechamento,
    IReadOnlyList<ClosingItemResponse> Itens,
    IReadOnlyList<ClosingMovementResponse> Movimentos,
    string ResumoWhatsapp);

public sealed record ClosingExportResponse(
    string FileName,
    string ContentType,
    byte[] Content);
