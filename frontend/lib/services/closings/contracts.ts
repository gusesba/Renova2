// Agrupa os contratos usados pelo modulo 13 no frontend.
export type ClosingOption = {
  codigo: string;
  nome: string;
};

export type ClosingPersonOption = {
  pessoaId: string;
  nome: string;
  documento: string;
  ehCliente: boolean;
  ehFornecedor: boolean;
  aceitaCreditoLoja: boolean;
};

export type ClosingWorkspace = {
  lojaId: string;
  lojaNome: string;
  pessoas: ClosingPersonOption[];
  statusFechamento: ClosingOption[];
  tiposMovimento: ClosingOption[];
};

export type ClosingSummary = {
  id: string;
  pessoaId: string;
  pessoaNome: string;
  pessoaDocumento: string;
  ehCliente: boolean;
  ehFornecedor: boolean;
  periodoInicio: string;
  periodoFim: string;
  statusFechamento: string;
  valorVendido: number;
  valorAReceber: number;
  valorPago: number;
  valorCompradoNaLoja: number;
  saldoCreditoAtual: number;
  saldoFinal: number;
  quantidadePecasAtuais: number;
  quantidadePecasVendidas: number;
  geradoEm: string;
  geradoPorUsuarioId: string;
  geradoPorUsuarioNome: string;
  conferidoEm?: string | null;
  conferidoPorUsuarioId?: string | null;
  conferidoPorUsuarioNome?: string | null;
  resumoTexto: string;
  pdfUrl?: string | null;
  excelUrl?: string | null;
};

export type ClosingItem = {
  id: string;
  pecaId: string;
  grupoItem: string;
  codigoInternoPeca: string;
  produtoNomePeca: string;
  statusPecaSnapshot: string;
  valorVendaSnapshot?: number | null;
  valorRepasseSnapshot?: number | null;
  dataEvento: string;
};

export type ClosingMovement = {
  id: string;
  tipoMovimento: string;
  origemTipo: string;
  origemId?: string | null;
  dataMovimento: string;
  descricao: string;
  valor: number;
};

export type ClosingDetail = {
  fechamento: ClosingSummary;
  itens: ClosingItem[];
  movimentos: ClosingMovement[];
  resumoWhatsapp: string;
};
