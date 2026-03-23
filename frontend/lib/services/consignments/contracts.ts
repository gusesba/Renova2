// Agrupa os contratos usados pelo modulo 07 no frontend.
export type ConsignmentSummary = {
  totalAtivas: number;
  proximasDoFim: number;
  vencidas: number;
  comDescontoPendente: number;
};

export type ConsignmentSupplierOption = {
  pessoaId: string;
  nome: string;
  documento: string;
};

export type ConsignmentStatusOption = {
  codigo: string;
  nome: string;
};

export type ConsignmentActionOption = {
  codigo: string;
  nome: string;
};

export type ConsignmentWorkspace = {
  lojaId: string;
  lojaNome: string;
  resumo: ConsignmentSummary;
  fornecedores: ConsignmentSupplierOption[];
  statuses: ConsignmentStatusOption[];
  acoesEncerramento: ConsignmentActionOption[];
};

export type ConsignmentPieceSummary = {
  id: string;
  codigoInterno: string;
  produtoNome: string;
  marca: string;
  tamanho: string;
  cor: string;
  fornecedorPessoaId?: string | null;
  fornecedorNome?: string | null;
  statusPeca: string;
  statusConsignacao: string;
  precoBase: number;
  precoVendaAtual: number;
  percentualDescontoAplicado: number;
  percentualDescontoEsperado: number;
  descontoPendente: boolean;
  dataEntrada: string;
  dataInicioConsignacao?: string | null;
  dataFimConsignacao?: string | null;
  diasEmLoja: number;
  diasRestantes?: number | null;
  proximaDoFim: boolean;
  vencida: boolean;
  destinoPadraoFimConsignacao?: string | null;
  alertaAberto: boolean;
};

export type ConsignmentPriceHistory = {
  id: string;
  precoAnterior: number;
  precoNovo: number;
  motivo: string;
  alteradoEm: string;
  alteradoPorUsuarioId: string;
};

export type CommercialDiscountBand = {
  diasMinimos: number;
  percentualDesconto: number;
};

export type ConsignmentDetail = {
  resumo: ConsignmentPieceSummary;
  politicaDesconto: CommercialDiscountBand[];
  historicoPreco: ConsignmentPriceHistory[];
};

export type CloseConsignmentResult = {
  pecaId: string;
  codigoInterno: string;
  statusPeca: string;
  tipoMovimentacao: string;
  quantidadeMovimentada: number;
  encerradoEm: string;
  comprovanteTexto: string;
};
