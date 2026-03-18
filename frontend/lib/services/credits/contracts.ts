// Agrupa os contratos usados pelo modulo 10 no frontend.
export type CreditOption = {
  codigo: string;
  nome: string;
};

export type CreditPersonOption = {
  pessoaId: string;
  nome: string;
  documento: string;
  tipoPessoa: string;
  ehCliente: boolean;
  ehFornecedor: boolean;
  aceitaCreditoLoja: boolean;
  statusRelacao: string;
  possuiConta: boolean;
};

export type CreditAccountSummary = {
  contaId: string;
  pessoaId: string;
  nome: string;
  documento: string;
  tipoPessoa: string;
  ehCliente: boolean;
  ehFornecedor: boolean;
  aceitaCreditoLoja: boolean;
  statusConta: string;
  saldoAtual: number;
  saldoComprometido: number;
  saldoDisponivel: number;
  ultimaMovimentacaoEm?: string | null;
};

export type CreditMovement = {
  id: string;
  tipoMovimentacao: string;
  origemTipo: string;
  origemId?: string | null;
  valor: number;
  saldoAnterior: number;
  saldoPosterior: number;
  direcao: string;
  observacoes: string;
  movimentadoEm: string;
  movimentadoPorUsuarioId: string;
  movimentadoPorUsuarioNome: string;
};

export type CreditAccountDetail = {
  conta: CreditAccountSummary;
  movimentacoes: CreditMovement[];
};

export type CreditsWorkspace = {
  lojaId: string;
  lojaNome: string;
  contas: CreditAccountSummary[];
  pessoas: CreditPersonOption[];
  statusConta: CreditOption[];
  tiposMovimentacao: CreditOption[];
};
