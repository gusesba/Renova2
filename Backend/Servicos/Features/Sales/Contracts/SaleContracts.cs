namespace Renova.Services.Features.Sales.Contracts;

// Agrupa os contratos HTTP e de aplicacao do modulo 09.
public sealed record SaleListQueryRequest(
    string? Search,
    string? StatusVenda,
    Guid? CompradorPessoaId,
    DateTimeOffset? DataInicial,
    DateTimeOffset? DataFinal);

public sealed record CreateSaleItemRequest(
    Guid PecaId,
    int Quantidade,
    decimal DescontoUnitario);

public sealed record CreateSalePaymentRequest(
    string TipoPagamento,
    Guid? MeioPagamentoId,
    decimal Valor);

public sealed record CreateSaleRequest(
    Guid? CompradorPessoaId,
    string Observacoes,
    IReadOnlyList<CreateSaleItemRequest> Itens,
    IReadOnlyList<CreateSalePaymentRequest> Pagamentos);

public sealed record CancelSaleRequest(
    string MotivoCancelamento);

public sealed record SaleOptionResponse(
    string Codigo,
    string Nome);

public sealed record SalePaymentMethodOptionResponse(
    Guid Id,
    string Nome,
    string TipoMeioPagamento,
    string TipoMeioPagamentoNome,
    decimal TaxaPercentual,
    int PrazoRecebimentoDias);

public sealed record SaleBuyerOptionResponse(
    Guid PessoaId,
    string Nome,
    string Documento,
    bool AceitaCreditoLoja,
    decimal SaldoCreditoDisponivel);

public sealed record SalePieceOptionResponse(
    Guid PecaId,
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
    int QuantidadeAtual,
    decimal PrecoVendaAtual,
    decimal PercentualRepasseDinheiro,
    decimal PercentualRepasseCredito,
    bool PermitePagamentoMisto);

public sealed record SalesWorkspaceResponse(
    Guid LojaId,
    string LojaNome,
    IReadOnlyList<SaleBuyerOptionResponse> Compradores,
    IReadOnlyList<SalePieceOptionResponse> PecasDisponiveis,
    IReadOnlyList<SalePaymentMethodOptionResponse> MeiosPagamento,
    IReadOnlyList<SaleOptionResponse> TiposPagamento,
    IReadOnlyList<SaleOptionResponse> StatusVenda);

public sealed record SaleSummaryResponse(
    Guid Id,
    string NumeroVenda,
    string StatusVenda,
    DateTimeOffset DataHoraVenda,
    Guid? CompradorPessoaId,
    string? CompradorNome,
    Guid VendedorUsuarioId,
    string VendedorNome,
    decimal Subtotal,
    decimal DescontoTotal,
    decimal TaxaTotal,
    decimal TotalLiquido,
    int QuantidadeItens,
    int QuantidadePagamentos);

public sealed record SaleItemResponse(
    Guid Id,
    Guid PecaId,
    string CodigoInterno,
    string ProdutoNome,
    string Marca,
    string Tamanho,
    string Cor,
    int Quantidade,
    decimal PrecoTabelaUnitario,
    decimal DescontoUnitario,
    decimal PrecoFinalUnitario,
    string TipoPecaSnapshot,
    Guid? FornecedorPessoaIdSnapshot,
    string? FornecedorNome,
    decimal? PercentualRepasseDinheiroSnapshot,
    decimal? PercentualRepasseCreditoSnapshot,
    decimal ValorRepassePrevisto);

public sealed record SalePaymentResponse(
    Guid Id,
    int Sequencia,
    Guid? MeioPagamentoId,
    string? MeioPagamentoNome,
    string TipoPagamento,
    decimal Valor,
    decimal TaxaPercentualAplicada,
    decimal ValorLiquido,
    DateTimeOffset RecebidoEm);

public sealed record SaleDetailResponse(
    Guid Id,
    string NumeroVenda,
    string StatusVenda,
    DateTimeOffset DataHoraVenda,
    Guid? CompradorPessoaId,
    string? CompradorNome,
    Guid VendedorUsuarioId,
    string VendedorNome,
    decimal Subtotal,
    decimal DescontoTotal,
    decimal TaxaTotal,
    decimal TotalLiquido,
    string Observacoes,
    DateTimeOffset? CanceladaEm,
    Guid? CanceladaPorUsuarioId,
    string? MotivoCancelamento,
    IReadOnlyList<SaleItemResponse> Itens,
    IReadOnlyList<SalePaymentResponse> Pagamentos,
    string ReciboTexto);
