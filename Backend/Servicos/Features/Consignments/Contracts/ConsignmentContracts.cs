using Renova.Services.Features.CommercialRules.Contracts;

namespace Renova.Services.Features.Consignments.Contracts;

// Agrupa os contratos HTTP e de aplicacao do modulo 07.
public sealed record ConsignmentListQueryRequest(
    string? Search,
    Guid? FornecedorPessoaId,
    string? StatusConsignacao,
    bool SomenteProximasDoFim,
    bool SomenteDescontoPendente);

public sealed record CloseConsignmentRequest(
    string Acao,
    string Motivo);

public sealed record ConsignmentSummaryResponse(
    int TotalAtivas,
    int ProximasDoFim,
    int Vencidas,
    int ComDescontoPendente);

public sealed record ConsignmentSupplierOptionResponse(
    Guid PessoaId,
    string Nome,
    string Documento);

public sealed record ConsignmentStatusOptionResponse(
    string Codigo,
    string Nome);

public sealed record ConsignmentActionOptionResponse(
    string Codigo,
    string Nome);

public sealed record ConsignmentWorkspaceResponse(
    Guid LojaId,
    string LojaNome,
    ConsignmentSummaryResponse Resumo,
    IReadOnlyList<ConsignmentSupplierOptionResponse> Fornecedores,
    IReadOnlyList<ConsignmentStatusOptionResponse> Statuses,
    IReadOnlyList<ConsignmentActionOptionResponse> AcoesEncerramento);

public sealed record ConsignmentPieceSummaryResponse(
    Guid Id,
    string CodigoInterno,
    string ProdutoNome,
    string Marca,
    string Tamanho,
    string Cor,
    Guid? FornecedorPessoaId,
    string? FornecedorNome,
    string StatusPeca,
    string StatusConsignacao,
    decimal PrecoBase,
    decimal PrecoVendaAtual,
    decimal PercentualDescontoAplicado,
    decimal PercentualDescontoEsperado,
    bool DescontoPendente,
    DateTimeOffset DataEntrada,
    DateTimeOffset? DataInicioConsignacao,
    DateTimeOffset? DataFimConsignacao,
    int DiasEmLoja,
    int? DiasRestantes,
    bool ProximaDoFim,
    bool Vencida,
    string? DestinoPadraoFimConsignacao,
    bool AlertaAberto);

public sealed record ConsignmentPriceHistoryResponse(
    Guid Id,
    decimal PrecoAnterior,
    decimal PrecoNovo,
    string Motivo,
    DateTimeOffset AlteradoEm,
    Guid AlteradoPorUsuarioId);

public sealed record ConsignmentDetailResponse(
    ConsignmentPieceSummaryResponse Resumo,
    IReadOnlyList<CommercialDiscountBandResponse> PoliticaDesconto,
    IReadOnlyList<ConsignmentPriceHistoryResponse> HistoricoPreco);

public sealed record CloseConsignmentResponse(
    Guid PecaId,
    string CodigoInterno,
    string StatusPeca,
    string TipoMovimentacao,
    int QuantidadeMovimentada,
    DateTimeOffset EncerradoEm,
    string ComprovanteTexto);
