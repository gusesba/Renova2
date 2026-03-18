namespace Renova.Services.Features.Reports.Contracts;

// Reune os contratos HTTP e de aplicacao do modulo 15.
public sealed record ReportOptionResponse(string Codigo, string Nome);

public sealed record ReportFilterOptionResponse(Guid Id, string Nome, string? Descricao);

public sealed record SavedReportFilterResponse(
    Guid Id,
    string Nome,
    string TipoRelatorio,
    ReportQueryRequest Filtros,
    DateTimeOffset CriadoEm);

public sealed record ReportWorkspaceResponse(
    Guid LojaAtivaId,
    string LojaAtivaNome,
    IReadOnlyList<ReportFilterOptionResponse> Lojas,
    IReadOnlyList<ReportFilterOptionResponse> Fornecedores,
    IReadOnlyList<ReportFilterOptionResponse> PessoasFinanceiras,
    IReadOnlyList<ReportFilterOptionResponse> Marcas,
    IReadOnlyList<ReportFilterOptionResponse> Vendedores,
    IReadOnlyList<ReportOptionResponse> StatusPeca,
    IReadOnlyList<ReportOptionResponse> MotivosBaixa,
    IReadOnlyList<ReportOptionResponse> TiposRelatorio,
    IReadOnlyList<SavedReportFilterResponse> FiltrosSalvos);

public sealed record ReportQueryRequest(
    string TipoRelatorio,
    Guid? LojaId,
    DateOnly? DataInicial,
    DateOnly? DataFinal,
    Guid? FornecedorPessoaId,
    Guid? PessoaId,
    Guid? MarcaId,
    Guid? VendedorUsuarioId,
    string? StatusPeca,
    string? MotivoMovimentacao,
    string? Search);

public sealed record ReportMetricResponse(string Nome, string Valor);

public sealed record ReportColumnResponse(string Chave, string Titulo);

public sealed record ReportCellResponse(string Chave, string Valor);

public sealed record ReportRowResponse(string Id, IReadOnlyList<ReportCellResponse> Celulas);

public sealed record ReportResultResponse(
    string TipoRelatorio,
    string Titulo,
    string Subtitulo,
    IReadOnlyList<ReportMetricResponse> Metricas,
    IReadOnlyList<ReportColumnResponse> Colunas,
    IReadOnlyList<ReportRowResponse> Linhas,
    int QuantidadeRegistros);

public sealed record SaveReportFilterRequest(string Nome, ReportQueryRequest Filtros);

public sealed record ReportExportFileResponse(string FileName, string ContentType, byte[] Content);
