// Agrupa os contratos usados pelo modulo de pessoas no frontend.
export type PersonStoreRelation = {
  id: string;
  lojaId: string;
  ehCliente: boolean;
  ehFornecedor: boolean;
  aceitaCreditoLoja: boolean;
  politicaPadraoFimConsignacao: string;
  observacoesInternas: string;
  statusRelacao: string;
};

export type PersonLinkedUser = {
  id: string;
  nome: string;
  email: string;
  statusUsuario: string;
};

export type PersonBankAccount = {
  id: string;
  banco: string;
  agencia: string;
  conta: string;
  tipoConta: string;
  pixTipo: string;
  pixChave: string;
  favorecidoNome: string;
  favorecidoDocumento: string;
  principal: boolean;
};

export type PersonFinancialSummary = {
  saldoCreditoAtual: number;
  saldoCreditoComprometido: number;
  totalPendencias: number;
  quantidadePendencias: number;
  ultimaMovimentacaoEm?: string | null;
};

export type PersonFinancialEntry = {
  id?: string | null;
  tipo: string;
  descricao: string;
  valor: number;
  direcao: string;
  referencia: string;
  ocorridoEm: string;
};

export type PersonSummary = {
  id: string;
  tipoPessoa: string;
  nome: string;
  nomeSocial: string;
  documento: string;
  telefone: string;
  email: string;
  ativo: boolean;
  relacaoLoja: PersonStoreRelation;
  usuarioVinculado?: PersonLinkedUser | null;
  financeiro: PersonFinancialSummary;
};

export type PersonDetail = {
  id: string;
  tipoPessoa: string;
  nome: string;
  nomeSocial: string;
  documento: string;
  telefone: string;
  email: string;
  logradouro: string;
  numero: string;
  complemento: string;
  bairro: string;
  cidade: string;
  uf: string;
  cep: string;
  observacoes: string;
  ativo: boolean;
  relacaoLoja: PersonStoreRelation;
  usuarioVinculado?: PersonLinkedUser | null;
  contasBancarias: PersonBankAccount[];
  financeiro: PersonFinancialSummary;
  historicoFinanceiro: PersonFinancialEntry[];
};

export type PersonUserOption = {
  id: string;
  nome: string;
  email: string;
  statusUsuario: string;
  pessoaId?: string | null;
  pessoaIdLojaAtiva?: string | null;
};

export type PersonReuseDraft = {
  pessoaId: string;
  tipoPessoa: string;
  nome: string;
  nomeSocial: string;
  documento: string;
  telefone: string;
  email: string;
  logradouro: string;
  numero: string;
  complemento: string;
  bairro: string;
  cidade: string;
  uf: string;
  cep: string;
  observacoes: string;
  ativo: boolean;
  contasBancarias: PersonBankAccount[];
  jaVinculadaNaLojaAtiva: boolean;
};
