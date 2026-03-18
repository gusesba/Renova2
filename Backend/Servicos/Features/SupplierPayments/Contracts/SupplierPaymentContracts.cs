namespace Renova.Services.Features.SupplierPayments.Contracts;

// Reune os contratos HTTP e de aplicacao do modulo 11.
public sealed record SupplierPaymentOptionResponse(string Codigo, string Nome);

public sealed record SupplierPaymentMethodOptionResponse(
    Guid Id,
    string Nome,
    string TipoMeioPagamento,
    string TipoMeioPagamentoNome);

public sealed record SupplierPaymentSupplierOptionResponse(
    Guid PessoaId,
    string Nome,
    string Documento);

public sealed record SupplierPaymentWorkspaceResponse(
    Guid LojaId,
    string LojaNome,
    IReadOnlyList<SupplierPaymentMethodOptionResponse> MeiosPagamento,
    IReadOnlyList<SupplierPaymentSupplierOptionResponse> Fornecedores,
    IReadOnlyList<SupplierPaymentOptionResponse> StatusObrigacao,
    IReadOnlyList<SupplierPaymentOptionResponse> TiposObrigacao,
    IReadOnlyList<SupplierPaymentOptionResponse> TiposLiquidacao);

public sealed record SupplierPaymentListQueryRequest(
    string? Search,
    Guid? PessoaId,
    string? StatusObrigacao,
    string? TipoObrigacao);

public sealed record SupplierObligationSummaryResponse(
    Guid Id,
    Guid PessoaId,
    string FornecedorNome,
    string FornecedorDocumento,
    Guid? PecaId,
    string? CodigoInternoPeca,
    string? ProdutoNomePeca,
    Guid? VendaId,
    string? NumeroVenda,
    string TipoObrigacao,
    string StatusObrigacao,
    decimal ValorOriginal,
    decimal ValorEmAberto,
    decimal ValorLiquidado,
    int QuantidadeLiquidacoes,
    DateTimeOffset DataGeracao,
    DateTimeOffset? DataVencimento,
    string Observacoes);

public sealed record SupplierPaymentLiquidationResponse(
    Guid Id,
    string TipoLiquidacao,
    Guid? MeioPagamentoId,
    string? MeioPagamentoNome,
    Guid? ContaCreditoLojaId,
    decimal Valor,
    string? ComprovanteUrl,
    DateTimeOffset LiquidadoEm,
    Guid LiquidadoPorUsuarioId,
    string LiquidadoPorUsuarioNome,
    string Observacoes);

public sealed record SupplierObligationDetailResponse(
    SupplierObligationSummaryResponse Obrigacao,
    IReadOnlyList<SupplierPaymentLiquidationResponse> Liquidacoes,
    string ComprovanteTexto);

public sealed record SettleSupplierObligationPaymentItemRequest(
    string TipoLiquidacao,
    Guid? MeioPagamentoId,
    decimal Valor);

public sealed record SettleSupplierObligationRequest(
    IReadOnlyList<SettleSupplierObligationPaymentItemRequest> Pagamentos,
    string? ComprovanteUrl,
    string Observacoes);
