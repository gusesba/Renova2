namespace Renova.Services.Features.StockMovements.Contracts;

// Agrupa os contratos HTTP e de aplicacao do modulo 08.
public sealed record StockMovementListQueryRequest(
    string? Search,
    Guid? PecaId,
    Guid? FornecedorPessoaId,
    string? StatusPeca,
    string? TipoMovimentacao,
    DateTimeOffset? DataInicial,
    DateTimeOffset? DataFinal);

public sealed record StockPieceSearchQueryRequest(
    string? Search,
    string? CodigoBarras,
    Guid? FornecedorPessoaId,
    string? StatusPeca,
    int? TempoMinimoLojaDias);

public sealed record AdjustStockRequest(
    Guid PecaId,
    int QuantidadeNova,
    string? StatusPeca,
    string Motivo);

public sealed record StockSaleAvailabilityRequest(
    Guid PecaId,
    int QuantidadeSolicitada);

public sealed record StockOptionResponse(
    string Codigo,
    string Nome);

public sealed record StockSupplierOptionResponse(
    Guid PessoaId,
    string Nome,
    string Documento);

public sealed record StockMovementSummaryResponse(
    int TotalMovimentacoes,
    int AjustesManuais,
    int PecasComSaldo,
    int PecasSemSaldo);

public sealed record StockMovementWorkspaceResponse(
    Guid LojaId,
    string LojaNome,
    StockMovementSummaryResponse Resumo,
    IReadOnlyList<StockSupplierOptionResponse> Fornecedores,
    IReadOnlyList<StockOptionResponse> StatusPeca,
    IReadOnlyList<StockOptionResponse> TiposMovimentacao);

public sealed record StockMovementItemResponse(
    Guid Id,
    Guid PecaId,
    string CodigoInterno,
    string CodigoBarras,
    string ProdutoNome,
    string Marca,
    string Tamanho,
    string Cor,
    Guid? FornecedorPessoaId,
    string? FornecedorNome,
    string StatusPeca,
    string TipoMovimentacao,
    int Quantidade,
    int SaldoAnterior,
    int SaldoPosterior,
    string OrigemTipo,
    Guid? OrigemId,
    string Motivo,
    DateTimeOffset MovimentadoEm,
    Guid MovimentadoPorUsuarioId,
    int QuantidadeAtualPeca,
    int DiasEmLoja);

public sealed record StockPieceLookupResponse(
    Guid Id,
    string CodigoInterno,
    string CodigoBarras,
    string TipoPeca,
    string StatusPeca,
    string ProdutoNome,
    string Marca,
    string Tamanho,
    string Cor,
    Guid? FornecedorPessoaId,
    string? FornecedorNome,
    DateTimeOffset DataEntrada,
    int DiasEmLoja,
    int QuantidadeAtual,
    string LocalizacaoFisica,
    bool DisponivelParaVenda,
    DateTimeOffset? UltimaMovimentacaoEm);

public sealed record AdjustStockResponse(
    Guid MovimentacaoId,
    Guid PecaId,
    string CodigoInterno,
    int QuantidadeAnterior,
    int QuantidadeNova,
    string StatusAnterior,
    string StatusNovo,
    DateTimeOffset MovimentadoEm,
    string Motivo);
