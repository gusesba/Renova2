// Agrupa os contratos usados pelo modulo 12 no frontend.
export type FinancialOption = {
  codigo: string;
  nome: string;
};

export type FinancialPaymentMethodOption = {
  id: string;
  nome: string;
  tipoMeioPagamento: string;
  tipoMeioPagamentoNome: string;
  taxaPercentual: number;
  prazoRecebimentoDias: number;
};

export type FinancialWorkspace = {
  lojaId: string;
  lojaNome: string;
  meiosPagamento: FinancialPaymentMethodOption[];
  tiposMovimentacao: FinancialOption[];
  tiposLancamentoManual: FinancialOption[];
  direcoes: FinancialOption[];
};

export type FinancialLedgerEntry = {
  id: string;
  tipoMovimentacao: string;
  direcao: string;
  origemTipo: string;
  meioPagamentoId?: string | null;
  meioPagamentoNome?: string | null;
  vendaId?: string | null;
  numeroVenda?: string | null;
  liquidacaoObrigacaoFornecedorId?: string | null;
  obrigacaoFornecedorId?: string | null;
  fornecedorNome?: string | null;
  valorBruto: number;
  taxa: number;
  valorLiquido: number;
  descricao: string;
  competenciaEm?: string | null;
  movimentadoEm: string;
  movimentadoPorUsuarioId: string;
  movimentadoPorUsuarioNome: string;
};

export type FinancialAggregate = {
  quantidadeLancamentos: number;
  totalEntradasBrutas: number;
  totalSaidasBrutas: number;
  saldoBruto: number;
  totalEntradasLiquidas: number;
  totalSaidasLiquidas: number;
  saldoLiquido: number;
  totalTaxas: number;
};

export type FinancialBreakdown = {
  codigo: string;
  nome: string;
  quantidadeLancamentos: number;
  totalEntradasBrutas: number;
  totalSaidasBrutas: number;
  saldoBruto: number;
  totalEntradasLiquidas: number;
  totalSaidasLiquidas: number;
  saldoLiquido: number;
  totalTaxas: number;
};

export type FinancialDailySummary = {
  data: string;
  quantidadeLancamentos: number;
  totalEntradasBrutas: number;
  totalSaidasBrutas: number;
  saldoBruto: number;
  totalEntradasLiquidas: number;
  totalSaidasLiquidas: number;
  saldoLiquido: number;
  totalTaxas: number;
};

export type FinancialReconciliation = {
  totais: FinancialAggregate;
  porMeioPagamento: FinancialBreakdown[];
  porTipoMovimentacao: FinancialBreakdown[];
  resumoDiario: FinancialDailySummary[];
};
