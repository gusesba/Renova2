namespace Renova.Services.Features.People.Contracts;

// Reune os contratos HTTP e de aplicacao do modulo de pessoas.
public sealed record CreatePersonRequest(
    string TipoPessoa,
    string Nome,
    string NomeSocial,
    string Documento,
    string Telefone,
    string Email,
    string Logradouro,
    string Numero,
    string Complemento,
    string Bairro,
    string Cidade,
    string Uf,
    string Cep,
    string Observacoes,
    bool Ativo,
    Guid? UsuarioId,
    PersonStoreRelationRequest RelacaoLoja,
    IReadOnlyList<PersonBankAccountRequest> ContasBancarias);

public sealed record UpdatePersonRequest(
    string TipoPessoa,
    string Nome,
    string NomeSocial,
    string Documento,
    string Telefone,
    string Email,
    string Logradouro,
    string Numero,
    string Complemento,
    string Bairro,
    string Cidade,
    string Uf,
    string Cep,
    string Observacoes,
    bool Ativo,
    Guid? UsuarioId,
    PersonStoreRelationRequest RelacaoLoja,
    IReadOnlyList<PersonBankAccountRequest> ContasBancarias);

public sealed record PersonStoreRelationRequest(
    bool EhCliente,
    bool EhFornecedor,
    bool AceitaCreditoLoja,
    string PoliticaPadraoFimConsignacao,
    string ObservacoesInternas,
    string StatusRelacao);

public sealed record PersonBankAccountRequest(
    Guid? Id,
    string Banco,
    string Agencia,
    string Conta,
    string TipoConta,
    string PixTipo,
    string PixChave,
    string FavorecidoNome,
    string FavorecidoDocumento,
    bool Principal);

public sealed record PersonSummaryResponse(
    Guid Id,
    string TipoPessoa,
    string Nome,
    string NomeSocial,
    string Documento,
    string Telefone,
    string Email,
    bool Ativo,
    PersonStoreRelationResponse RelacaoLoja,
    PersonLinkedUserResponse? UsuarioVinculado,
    PersonFinancialSummaryResponse Financeiro);

public sealed record PersonDetailResponse(
    Guid Id,
    string TipoPessoa,
    string Nome,
    string NomeSocial,
    string Documento,
    string Telefone,
    string Email,
    string Logradouro,
    string Numero,
    string Complemento,
    string Bairro,
    string Cidade,
    string Uf,
    string Cep,
    string Observacoes,
    bool Ativo,
    PersonStoreRelationResponse RelacaoLoja,
    PersonLinkedUserResponse? UsuarioVinculado,
    IReadOnlyList<PersonBankAccountResponse> ContasBancarias,
    PersonFinancialSummaryResponse Financeiro,
    IReadOnlyList<PersonFinancialEntryResponse> HistoricoFinanceiro);

public sealed record PersonStoreRelationResponse(
    Guid Id,
    Guid LojaId,
    bool EhCliente,
    bool EhFornecedor,
    bool AceitaCreditoLoja,
    string PoliticaPadraoFimConsignacao,
    string ObservacoesInternas,
    string StatusRelacao);

public sealed record PersonBankAccountResponse(
    Guid Id,
    string Banco,
    string Agencia,
    string Conta,
    string TipoConta,
    string PixTipo,
    string PixChave,
    string FavorecidoNome,
    string FavorecidoDocumento,
    bool Principal);

public sealed record PersonLinkedUserResponse(
    Guid Id,
    string Nome,
    string Email,
    string StatusUsuario);

public sealed record PersonUserOptionResponse(
    Guid Id,
    string Nome,
    string Email,
    string StatusUsuario,
    Guid? PessoaId,
    Guid? PessoaIdLojaAtiva);

public sealed record PersonReuseDraftResponse(
    Guid PessoaId,
    string TipoPessoa,
    string Nome,
    string NomeSocial,
    string Documento,
    string Telefone,
    string Email,
    string Logradouro,
    string Numero,
    string Complemento,
    string Bairro,
    string Cidade,
    string Uf,
    string Cep,
    string Observacoes,
    bool Ativo,
    IReadOnlyList<PersonBankAccountResponse> ContasBancarias,
    bool JaVinculadaNaLojaAtiva);

public sealed record PersonFinancialSummaryResponse(
    decimal SaldoCreditoAtual,
    decimal SaldoCreditoComprometido,
    decimal TotalPendencias,
    int QuantidadePendencias,
    DateTimeOffset? UltimaMovimentacaoEm);

public sealed record PersonFinancialEntryResponse(
    Guid? Id,
    string Tipo,
    string Descricao,
    decimal Valor,
    string Direcao,
    string Referencia,
    DateTimeOffset OcorridoEm);
