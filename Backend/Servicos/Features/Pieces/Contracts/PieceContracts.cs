using Renova.Services.Features.CommercialRules.Contracts;

namespace Renova.Services.Features.Pieces.Contracts;

// Agrupa os contratos HTTP e de aplicacao do modulo 06.
public sealed record PieceListQueryRequest(
    string? Search,
    string? CodigoBarras,
    string? StatusPeca,
    Guid? ProdutoNomeId,
    Guid? MarcaId,
    Guid? FornecedorPessoaId);

public sealed record ManualPieceCommercialRuleRequest(
    decimal PercentualRepasseDinheiro,
    decimal PercentualRepasseCredito,
    bool PermitePagamentoMisto,
    int TempoMaximoExposicaoDias,
    IReadOnlyList<CommercialDiscountBandRequest> PoliticaDesconto);

public sealed record CreatePieceRequest(
    string TipoPeca,
    string CodigoBarras,
    Guid ProdutoNomeId,
    Guid MarcaId,
    Guid TamanhoId,
    Guid CorId,
    Guid? FornecedorPessoaId,
    string Descricao,
    string Observacoes,
    DateTimeOffset? DataEntrada,
    int QuantidadeInicial,
    decimal PrecoVendaAtual,
    decimal? CustoUnitario,
    string LocalizacaoFisica,
    ManualPieceCommercialRuleRequest? RegraManual);

public sealed record UpdatePieceRequest(
    string TipoPeca,
    string CodigoBarras,
    Guid ProdutoNomeId,
    Guid MarcaId,
    Guid TamanhoId,
    Guid CorId,
    Guid? FornecedorPessoaId,
    string Descricao,
    string Observacoes,
    DateTimeOffset? DataEntrada,
    decimal PrecoVendaAtual,
    decimal? CustoUnitario,
    string LocalizacaoFisica,
    ManualPieceCommercialRuleRequest? RegraManual);

public sealed record RegisterPieceImageRequest(
    string UrlArquivo,
    int Ordem,
    string TipoVisibilidade);

public sealed record UpdatePieceImageRequest(
    int Ordem,
    string TipoVisibilidade);

public sealed record PieceOptionResponse(
    string Codigo,
    string Nome);

public sealed record PieceCatalogOptionResponse(
    Guid Id,
    string Nome);

public sealed record PieceSupplierOptionResponse(
    Guid PessoaId,
    Guid PessoaLojaId,
    string Nome,
    string Documento,
    string PoliticaPadraoFimConsignacao,
    string StatusRelacao);

public sealed record PieceWorkspaceResponse(
    Guid LojaId,
    string LojaNome,
    IReadOnlyList<PieceCatalogOptionResponse> ProdutoNomes,
    IReadOnlyList<PieceCatalogOptionResponse> Marcas,
    IReadOnlyList<PieceCatalogOptionResponse> Tamanhos,
    IReadOnlyList<PieceCatalogOptionResponse> Cores,
    IReadOnlyList<PieceSupplierOptionResponse> Fornecedores,
    IReadOnlyList<PieceOptionResponse> TiposPeca,
    IReadOnlyList<PieceOptionResponse> StatusPeca,
    IReadOnlyList<PieceOptionResponse> VisibilidadesImagem);

public sealed record PieceCommercialConditionResponse(
    Guid Id,
    string OrigemRegra,
    decimal PercentualRepasseDinheiro,
    decimal PercentualRepasseCredito,
    bool PermitePagamentoMisto,
    int TempoMaximoExposicaoDias,
    IReadOnlyList<CommercialDiscountBandResponse> PoliticaDesconto,
    DateTimeOffset? DataInicioConsignacao,
    DateTimeOffset? DataFimConsignacao,
    string? DestinoPadraoFimConsignacao);

public sealed record PieceImageResponse(
    Guid Id,
    string UrlArquivo,
    int Ordem,
    string TipoVisibilidade);

public sealed record PieceSummaryResponse(
    Guid Id,
    string CodigoInterno,
    string CodigoBarras,
    string TipoPeca,
    string StatusPeca,
    Guid ProdutoNomeId,
    string ProdutoNome,
    Guid MarcaId,
    string Marca,
    Guid TamanhoId,
    string Tamanho,
    Guid CorId,
    string Cor,
    Guid? FornecedorPessoaId,
    string? FornecedorNome,
    DateTimeOffset DataEntrada,
    int QuantidadeAtual,
    decimal PrecoVendaAtual,
    string LocalizacaoFisica,
    DateTimeOffset? DataFimConsignacao);

public sealed record PieceDetailResponse(
    Guid Id,
    Guid LojaId,
    string CodigoInterno,
    string CodigoBarras,
    string TipoPeca,
    string StatusPeca,
    Guid ProdutoNomeId,
    string ProdutoNome,
    Guid MarcaId,
    string Marca,
    Guid TamanhoId,
    string Tamanho,
    Guid CorId,
    string Cor,
    Guid? FornecedorPessoaId,
    string? FornecedorNome,
    string Descricao,
    string Observacoes,
    DateTimeOffset DataEntrada,
    int QuantidadeInicial,
    int QuantidadeAtual,
    decimal PrecoVendaAtual,
    decimal? CustoUnitario,
    string LocalizacaoFisica,
    Guid ResponsavelCadastroUsuarioId,
    PieceCommercialConditionResponse CondicaoComercial,
    IReadOnlyList<PieceImageResponse> Imagens);
