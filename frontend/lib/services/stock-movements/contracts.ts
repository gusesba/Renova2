// Agrupa os contratos usados pelo modulo de movimentacoes de estoque no frontend.
export type StockOption = {
  codigo: string;
  nome: string;
};

export type StockSupplierOption = {
  pessoaId: string;
  nome: string;
  documento: string;
};

export type StockMovementSummary = {
  totalMovimentacoes: number;
  ajustesManuais: number;
  pecasComSaldo: number;
  pecasSemSaldo: number;
};

export type StockMovementWorkspace = {
  lojaId: string;
  lojaNome: string;
  resumo: StockMovementSummary;
  fornecedores: StockSupplierOption[];
  statusPeca: StockOption[];
  tiposMovimentacao: StockOption[];
};

export type StockMovementItem = {
  id: string;
  pecaId: string;
  codigoInterno: string;
  codigoBarras: string;
  produtoNome: string;
  marca: string;
  tamanho: string;
  cor: string;
  fornecedorPessoaId?: string | null;
  fornecedorNome?: string | null;
  statusPeca: string;
  tipoMovimentacao: string;
  quantidade: number;
  saldoAnterior: number;
  saldoPosterior: number;
  origemTipo: string;
  origemId?: string | null;
  motivo: string;
  movimentadoEm: string;
  movimentadoPorUsuarioId: string;
  quantidadeAtualPeca: number;
  diasEmLoja: number;
};

export type StockPieceLookup = {
  id: string;
  codigoInterno: string;
  codigoBarras: string;
  tipoPeca: string;
  statusPeca: string;
  produtoNome: string;
  marca: string;
  tamanho: string;
  cor: string;
  fornecedorPessoaId?: string | null;
  fornecedorNome?: string | null;
  dataEntrada: string;
  diasEmLoja: number;
  quantidadeAtual: number;
  localizacaoFisica: string;
  disponivelParaVenda: boolean;
  ultimaMovimentacaoEm?: string | null;
};

export type AdjustStockResponse = {
  movimentacaoId: string;
  pecaId: string;
  codigoInterno: string;
  quantidadeAnterior: number;
  quantidadeNova: number;
  statusAnterior: string;
  statusNovo: string;
  movimentadoEm: string;
  motivo: string;
};
