namespace Renova.Services.Features.Dashboards.Contracts;

// Reune os contratos HTTP e de aplicacao do modulo 14.
public sealed record DashboardOptionResponse(string Codigo, string Nome);

public sealed record DashboardFilterOptionResponse(
    Guid Id,
    string Nome,
    string? Documento);

public sealed record DashboardWorkspaceResponse(
    Guid LojaId,
    string LojaNome,
    IReadOnlyList<DashboardFilterOptionResponse> Vendedores,
    IReadOnlyList<DashboardFilterOptionResponse> Fornecedores,
    IReadOnlyList<DashboardFilterOptionResponse> Marcas,
    IReadOnlyList<DashboardOptionResponse> TiposPeca);

public sealed record DashboardQueryRequest(
    DateOnly? DataInicial,
    DateOnly? DataFinal,
    Guid? VendedorUsuarioId,
    Guid? FornecedorPessoaId,
    Guid? MarcaId,
    string? TipoPeca);

public sealed record DashboardBucketResponse(
    string Chave,
    string Nome,
    int Quantidade,
    decimal Valor);

public sealed record DashboardSalesResponse(
    int QuantidadeVendas,
    int QuantidadePecasVendidas,
    decimal TotalVendido,
    decimal TicketMedio,
    IReadOnlyList<DashboardBucketResponse> PorDia,
    IReadOnlyList<DashboardBucketResponse> PorMes,
    IReadOnlyList<DashboardBucketResponse> PorLoja,
    IReadOnlyList<DashboardBucketResponse> PorVendedor);

public sealed record DashboardFinancialResponse(
    int QuantidadeEntradas,
    int QuantidadeSaidas,
    decimal EntradasBrutas,
    decimal SaidasBrutas,
    decimal SaldoBruto,
    decimal EntradasLiquidas,
    decimal SaidasLiquidas,
    decimal SaldoLiquido);

public sealed record DashboardConsignmentItemResponse(
    Guid PecaId,
    string CodigoInterno,
    string ProdutoNome,
    string MarcaNome,
    string? FornecedorNome,
    DateTimeOffset DataEntrada,
    int DiasEmEstoque,
    DateTimeOffset? DataLimite,
    int? DiasParaVencer);

public sealed record DashboardConsignmentResponse(
    IReadOnlyList<DashboardConsignmentItemResponse> ProximasVencer,
    IReadOnlyList<DashboardConsignmentItemResponse> ParadasEmEstoque);

public sealed record DashboardPendingItemResponse(
    string Tipo,
    string Titulo,
    string Descricao,
    decimal? Valor);

public sealed record DashboardPendingResponse(
    decimal ValorPagarFornecedores,
    decimal ValorPendenteRecebimento,
    int QuantidadeInconsistencias,
    IReadOnlyList<DashboardPendingItemResponse> Inconsistencias);

public sealed record DashboardIndicatorRowResponse(
    string Chave,
    string Nome,
    int TotalPecas,
    int PecasAtuais,
    int PecasVendidasPeriodo,
    decimal ValorVendidoPeriodo);

public sealed record DashboardIndicatorsResponse(
    IReadOnlyList<DashboardIndicatorRowResponse> PorTipoPeca,
    IReadOnlyList<DashboardIndicatorRowResponse> PorMarca,
    IReadOnlyList<DashboardIndicatorRowResponse> PorFornecedor);

public sealed record DashboardOverviewResponse(
    DateTimeOffset PeriodoInicio,
    DateTimeOffset PeriodoFim,
    DashboardSalesResponse Vendas,
    DashboardFinancialResponse Financeiro,
    DashboardConsignmentResponse Consignacao,
    DashboardPendingResponse Pendencias,
    DashboardIndicatorsResponse Indicadores);
