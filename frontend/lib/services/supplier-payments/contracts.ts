// Agrupa os contratos usados pelo modulo 11 no frontend.
export type SupplierPaymentOption = {
  codigo: string;
  nome: string;
};

export type SupplierPaymentMethodOption = {
  id: string;
  nome: string;
  tipoMeioPagamento: string;
  tipoMeioPagamentoNome: string;
};

export type SupplierPaymentSupplierOption = {
  pessoaId: string;
  nome: string;
  documento: string;
};

export type SupplierPaymentWorkspace = {
  lojaId: string;
  lojaNome: string;
  meiosPagamento: SupplierPaymentMethodOption[];
  fornecedores: SupplierPaymentSupplierOption[];
  statusObrigacao: SupplierPaymentOption[];
  tiposObrigacao: SupplierPaymentOption[];
  tiposLiquidacao: SupplierPaymentOption[];
};

export type SupplierObligationSummary = {
  id: string;
  pessoaId: string;
  fornecedorNome: string;
  fornecedorDocumento: string;
  pecaId?: string | null;
  codigoInternoPeca?: string | null;
  produtoNomePeca?: string | null;
  vendaId?: string | null;
  numeroVenda?: string | null;
  tipoObrigacao: string;
  statusObrigacao: string;
  valorOriginal: number;
  valorEmAberto: number;
  valorLiquidado: number;
  quantidadeLiquidacoes: number;
  dataGeracao: string;
  dataVencimento?: string | null;
  observacoes: string;
};

export type SupplierPaymentLiquidation = {
  id: string;
  tipoLiquidacao: string;
  meioPagamentoId?: string | null;
  meioPagamentoNome?: string | null;
  contaCreditoLojaId?: string | null;
  valor: number;
  comprovanteUrl?: string | null;
  liquidadoEm: string;
  liquidadoPorUsuarioId: string;
  liquidadoPorUsuarioNome: string;
  observacoes: string;
};

export type SupplierObligationDetail = {
  obrigacao: SupplierObligationSummary;
  liquidacoes: SupplierPaymentLiquidation[];
  comprovanteTexto: string;
};
