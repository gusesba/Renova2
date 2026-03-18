namespace Renova.Services.Features.Financial.Contracts;

// Reune os contratos HTTP e de aplicacao do modulo 12.
public sealed record FinancialOptionResponse(string Codigo, string Nome);

public sealed record FinancialPaymentMethodOptionResponse(
    Guid Id,
    string Nome,
    string TipoMeioPagamento,
    string TipoMeioPagamentoNome,
    decimal TaxaPercentual,
    int PrazoRecebimentoDias);

public sealed record FinancialWorkspaceResponse(
    Guid LojaId,
    string LojaNome,
    IReadOnlyList<FinancialPaymentMethodOptionResponse> MeiosPagamento,
    IReadOnlyList<FinancialOptionResponse> TiposMovimentacao,
    IReadOnlyList<FinancialOptionResponse> TiposLancamentoManual,
    IReadOnlyList<FinancialOptionResponse> Direcoes);

public sealed record FinancialListQueryRequest(
    string? Search,
    Guid? MeioPagamentoId,
    string? TipoMovimentacao,
    string? Direcao,
    DateOnly? DataInicial,
    DateOnly? DataFinal);

public sealed record FinancialLedgerEntryResponse(
    Guid Id,
    string TipoMovimentacao,
    string Direcao,
    string OrigemTipo,
    Guid? MeioPagamentoId,
    string? MeioPagamentoNome,
    Guid? VendaId,
    string? NumeroVenda,
    Guid? LiquidacaoObrigacaoFornecedorId,
    Guid? ObrigacaoFornecedorId,
    string? FornecedorNome,
    decimal ValorBruto,
    decimal Taxa,
    decimal ValorLiquido,
    string Descricao,
    DateTimeOffset? CompetenciaEm,
    DateTimeOffset MovimentadoEm,
    Guid MovimentadoPorUsuarioId,
    string MovimentadoPorUsuarioNome);

public sealed record RegisterFinancialEntryRequest(
    string TipoMovimentacao,
    string Direcao,
    Guid? MeioPagamentoId,
    decimal ValorBruto,
    decimal Taxa,
    string Descricao,
    DateOnly? CompetenciaEm,
    DateOnly? MovimentadoEm);

public sealed record FinancialAggregateResponse(
    int QuantidadeLancamentos,
    decimal TotalEntradasBrutas,
    decimal TotalSaidasBrutas,
    decimal SaldoBruto,
    decimal TotalEntradasLiquidas,
    decimal TotalSaidasLiquidas,
    decimal SaldoLiquido,
    decimal TotalTaxas);

public sealed record FinancialBreakdownResponse(
    string Codigo,
    string Nome,
    int QuantidadeLancamentos,
    decimal TotalEntradasBrutas,
    decimal TotalSaidasBrutas,
    decimal SaldoBruto,
    decimal TotalEntradasLiquidas,
    decimal TotalSaidasLiquidas,
    decimal SaldoLiquido,
    decimal TotalTaxas);

public sealed record FinancialDailySummaryResponse(
    string Data,
    int QuantidadeLancamentos,
    decimal TotalEntradasBrutas,
    decimal TotalSaidasBrutas,
    decimal SaldoBruto,
    decimal TotalEntradasLiquidas,
    decimal TotalSaidasLiquidas,
    decimal SaldoLiquido,
    decimal TotalTaxas);

public sealed record FinancialReconciliationResponse(
    FinancialAggregateResponse Totais,
    IReadOnlyList<FinancialBreakdownResponse> PorMeioPagamento,
    IReadOnlyList<FinancialBreakdownResponse> PorTipoMovimentacao,
    IReadOnlyList<FinancialDailySummaryResponse> ResumoDiario);
