// Agrupa os contratos usados pelo modulo de regras comerciais no frontend.
export type CommercialDiscountBand = {
  diasMinimos: number;
  percentualDesconto: number;
};

export type StoreCommercialRule = {
  id: string;
  lojaId: string;
  percentualRepasseDinheiro: number;
  percentualRepasseCredito: number;
  permitePagamentoMisto: boolean;
  tempoMaximoExposicaoDias: number;
  politicaDesconto: CommercialDiscountBand[];
  ativo: boolean;
};

export type SupplierCommercialRule = {
  id: string;
  pessoaLojaId: string;
  pessoaId: string;
  fornecedorNome: string;
  fornecedorDocumento: string;
  percentualRepasseDinheiro: number;
  percentualRepasseCredito: number;
  permitePagamentoMisto: boolean;
  tempoMaximoExposicaoDias: number;
  politicaDesconto: CommercialDiscountBand[];
  ativo: boolean;
};

export type SupplierRuleOption = {
  pessoaLojaId: string;
  pessoaId: string;
  nome: string;
  documento: string;
  statusRelacao: string;
};

export type PaymentMethod = {
  id: string;
  lojaId: string;
  nome: string;
  tipoMeioPagamento: string;
  taxaPercentual: number;
  prazoRecebimentoDias: number;
  ativo: boolean;
};

export type PaymentMethodTypeOption = {
  codigo: string;
  nome: string;
};

export type CommercialRulesWorkspace = {
  lojaId: string;
  lojaNome: string;
  regraLoja?: StoreCommercialRule | null;
  regrasFornecedor: SupplierCommercialRule[];
  fornecedoresDisponiveis: SupplierRuleOption[];
  meiosPagamento: PaymentMethod[];
  tiposMeioPagamento: PaymentMethodTypeOption[];
};
