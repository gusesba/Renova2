namespace Renova.Services.Features.Credits.Contracts;

// Reune os contratos HTTP e de aplicacao do modulo 10.
public sealed record CreditOptionResponse(string Codigo, string Nome);

public sealed record CreditPersonOptionResponse(
    Guid PessoaId,
    string Nome,
    string Documento,
    string TipoPessoa,
    bool EhCliente,
    bool EhFornecedor,
    bool AceitaCreditoLoja,
    string StatusRelacao,
    bool PossuiConta);

public sealed record CreditAccountSummaryResponse(
    Guid ContaId,
    Guid PessoaId,
    string Nome,
    string Documento,
    string TipoPessoa,
    bool EhCliente,
    bool EhFornecedor,
    bool AceitaCreditoLoja,
    string StatusConta,
    decimal SaldoAtual,
    decimal SaldoComprometido,
    decimal SaldoDisponivel,
    DateTimeOffset? UltimaMovimentacaoEm);

public sealed record CreditMovementResponse(
    Guid Id,
    string TipoMovimentacao,
    string OrigemTipo,
    Guid? OrigemId,
    decimal Valor,
    decimal SaldoAnterior,
    decimal SaldoPosterior,
    string Direcao,
    string Observacoes,
    DateTimeOffset MovimentadoEm,
    Guid MovimentadoPorUsuarioId,
    string MovimentadoPorUsuarioNome);

public sealed record CreditAccountDetailResponse(
    CreditAccountSummaryResponse Conta,
    IReadOnlyList<CreditMovementResponse> Movimentacoes);

public sealed record CreditsWorkspaceResponse(
    Guid LojaId,
    string LojaNome,
    IReadOnlyList<CreditAccountSummaryResponse> Contas,
    IReadOnlyList<CreditPersonOptionResponse> Pessoas,
    IReadOnlyList<CreditOptionResponse> StatusConta,
    IReadOnlyList<CreditOptionResponse> TiposMovimentacao);

public sealed record EnsureCreditAccountRequest(Guid PessoaId);

public sealed record ManualCreditRequest(Guid PessoaId, decimal Valor, string Justificativa);

public sealed record SupplierPassThroughCreditRequest(
    Guid PessoaId,
    decimal Valor,
    Guid? ObrigacaoFornecedorId,
    string Referencia,
    string Observacoes);

public sealed record UpdateCreditAccountStatusRequest(string StatusConta);
