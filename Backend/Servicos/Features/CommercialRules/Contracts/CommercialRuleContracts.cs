namespace Renova.Services.Features.CommercialRules.Contracts;

// Agrupa os contratos HTTP e de aplicacao do modulo 05.
public sealed record CommercialDiscountBandRequest(
    int DiasMinimos,
    decimal PercentualDesconto);

public sealed record UpsertStoreCommercialRuleRequest(
    decimal PercentualRepasseDinheiro,
    decimal PercentualRepasseCredito,
    bool PermitePagamentoMisto,
    int TempoMaximoExposicaoDias,
    IReadOnlyList<CommercialDiscountBandRequest> PoliticaDesconto,
    bool Ativo);

public sealed record CreateSupplierCommercialRuleRequest(
    Guid PessoaLojaId,
    decimal PercentualRepasseDinheiro,
    decimal PercentualRepasseCredito,
    bool PermitePagamentoMisto,
    int TempoMaximoExposicaoDias,
    IReadOnlyList<CommercialDiscountBandRequest> PoliticaDesconto,
    bool Ativo);

public sealed record UpdateSupplierCommercialRuleRequest(
    decimal PercentualRepasseDinheiro,
    decimal PercentualRepasseCredito,
    bool PermitePagamentoMisto,
    int TempoMaximoExposicaoDias,
    IReadOnlyList<CommercialDiscountBandRequest> PoliticaDesconto,
    bool Ativo);

public sealed record CreatePaymentMethodRequest(
    string Nome,
    string TipoMeioPagamento,
    decimal TaxaPercentual,
    int PrazoRecebimentoDias,
    bool Ativo);

public sealed record UpdatePaymentMethodRequest(
    string Nome,
    string TipoMeioPagamento,
    decimal TaxaPercentual,
    int PrazoRecebimentoDias,
    bool Ativo);

public sealed record CommercialDiscountBandResponse(
    int DiasMinimos,
    decimal PercentualDesconto);

public sealed record StoreCommercialRuleResponse(
    Guid Id,
    Guid LojaId,
    decimal PercentualRepasseDinheiro,
    decimal PercentualRepasseCredito,
    bool PermitePagamentoMisto,
    int TempoMaximoExposicaoDias,
    IReadOnlyList<CommercialDiscountBandResponse> PoliticaDesconto,
    bool Ativo);

public sealed record SupplierCommercialRuleResponse(
    Guid Id,
    Guid PessoaLojaId,
    Guid PessoaId,
    string FornecedorNome,
    string FornecedorDocumento,
    decimal PercentualRepasseDinheiro,
    decimal PercentualRepasseCredito,
    bool PermitePagamentoMisto,
    int TempoMaximoExposicaoDias,
    IReadOnlyList<CommercialDiscountBandResponse> PoliticaDesconto,
    bool Ativo);

public sealed record PaymentMethodResponse(
    Guid Id,
    Guid LojaId,
    string Nome,
    string TipoMeioPagamento,
    decimal TaxaPercentual,
    int PrazoRecebimentoDias,
    bool Ativo);

public sealed record SupplierRuleOptionResponse(
    Guid PessoaLojaId,
    Guid PessoaId,
    string Nome,
    string Documento,
    string StatusRelacao);

public sealed record PaymentMethodTypeOptionResponse(
    string Codigo,
    string Nome);

public sealed record CommercialRulesWorkspaceResponse(
    Guid LojaId,
    string LojaNome,
    StoreCommercialRuleResponse? RegraLoja,
    IReadOnlyList<SupplierCommercialRuleResponse> RegrasFornecedor,
    IReadOnlyList<SupplierRuleOptionResponse> FornecedoresDisponiveis,
    IReadOnlyList<PaymentMethodResponse> MeiosPagamento,
    IReadOnlyList<PaymentMethodTypeOptionResponse> TiposMeioPagamento);

public sealed record ManualCommercialRuleInput(
    decimal PercentualRepasseDinheiro,
    decimal PercentualRepasseCredito,
    bool PermitePagamentoMisto,
    int TempoMaximoExposicaoDias,
    IReadOnlyList<CommercialDiscountBandResponse> PoliticaDesconto);

public sealed record EffectiveCommercialRuleResponse(
    string OrigemRegra,
    decimal PercentualRepasseDinheiro,
    decimal PercentualRepasseCredito,
    bool PermitePagamentoMisto,
    int TempoMaximoExposicaoDias,
    IReadOnlyList<CommercialDiscountBandResponse> PoliticaDesconto);
