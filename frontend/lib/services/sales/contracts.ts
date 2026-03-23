// Agrupa os contratos usados pelo modulo de vendas no frontend.
export type SaleOption = {
  codigo: string;
  nome: string;
};

export type SaleBuyerOption = {
  pessoaId: string;
  nome: string;
  documento: string;
  aceitaCreditoLoja: boolean;
  saldoCreditoDisponivel: number;
};

export type SalePieceOption = {
  pecaId: string;
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
  quantidadeAtual: number;
  precoBase: number;
  precoVendaAtual: number;
  percentualDescontoAutomatico: number;
  descontoAutomaticoAtivo: boolean;
  percentualRepasseDinheiro: number;
  percentualRepasseCredito: number;
  permitePagamentoMisto: boolean;
};

export type SalePaymentMethodOption = {
  id: string;
  nome: string;
  tipoMeioPagamento: string;
  tipoMeioPagamentoNome: string;
  taxaPercentual: number;
  prazoRecebimentoDias: number;
};

export type SalesWorkspace = {
  lojaId: string;
  lojaNome: string;
  compradores: SaleBuyerOption[];
  pecasDisponiveis: SalePieceOption[];
  meiosPagamento: SalePaymentMethodOption[];
  tiposPagamento: SaleOption[];
  statusVenda: SaleOption[];
};

export type SaleSummary = {
  id: string;
  numeroVenda: string;
  statusVenda: string;
  dataHoraVenda: string;
  compradorPessoaId?: string | null;
  compradorNome?: string | null;
  vendedorUsuarioId: string;
  vendedorNome: string;
  subtotal: number;
  descontoTotal: number;
  taxaTotal: number;
  totalLiquido: number;
  quantidadeItens: number;
  quantidadePagamentos: number;
};

export type SaleItem = {
  id: string;
  pecaId: string;
  codigoInterno: string;
  produtoNome: string;
  marca: string;
  tamanho: string;
  cor: string;
  quantidade: number;
  precoTabelaUnitario: number;
  descontoUnitario: number;
  precoFinalUnitario: number;
  tipoPecaSnapshot: string;
  fornecedorPessoaIdSnapshot?: string | null;
  fornecedorNome?: string | null;
  percentualRepasseDinheiroSnapshot?: number | null;
  percentualRepasseCreditoSnapshot?: number | null;
  valorRepassePrevisto: number;
};

export type SalePayment = {
  id: string;
  sequencia: number;
  meioPagamentoId?: string | null;
  meioPagamentoNome?: string | null;
  tipoPagamento: string;
  valor: number;
  taxaPercentualAplicada: number;
  valorLiquido: number;
  recebidoEm: string;
};

export type SaleDetail = {
  id: string;
  numeroVenda: string;
  statusVenda: string;
  dataHoraVenda: string;
  compradorPessoaId?: string | null;
  compradorNome?: string | null;
  vendedorUsuarioId: string;
  vendedorNome: string;
  subtotal: number;
  descontoTotal: number;
  taxaTotal: number;
  totalLiquido: number;
  observacoes: string;
  canceladaEm?: string | null;
  canceladaPorUsuarioId?: string | null;
  motivoCancelamento?: string | null;
  itens: SaleItem[];
  pagamentos: SalePayment[];
  reciboTexto: string;
};
