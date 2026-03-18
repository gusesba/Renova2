// Agrupa os contratos usados pelo modulo 14 no frontend.
export type DashboardOption = {
  codigo: string;
  nome: string;
};

export type DashboardFilterOption = {
  id: string;
  nome: string;
  documento?: string | null;
};

export type DashboardWorkspace = {
  lojaId: string;
  lojaNome: string;
  vendedores: DashboardFilterOption[];
  fornecedores: DashboardFilterOption[];
  marcas: DashboardFilterOption[];
  tiposPeca: DashboardOption[];
};

export type DashboardBucket = {
  chave: string;
  nome: string;
  quantidade: number;
  valor: number;
};

export type DashboardSales = {
  quantidadeVendas: number;
  quantidadePecasVendidas: number;
  totalVendido: number;
  ticketMedio: number;
  porDia: DashboardBucket[];
  porMes: DashboardBucket[];
  porLoja: DashboardBucket[];
  porVendedor: DashboardBucket[];
};

export type DashboardFinancial = {
  quantidadeEntradas: number;
  quantidadeSaidas: number;
  entradasBrutas: number;
  saidasBrutas: number;
  saldoBruto: number;
  entradasLiquidas: number;
  saidasLiquidas: number;
  saldoLiquido: number;
};

export type DashboardConsignmentItem = {
  pecaId: string;
  codigoInterno: string;
  produtoNome: string;
  marcaNome: string;
  fornecedorNome?: string | null;
  dataEntrada: string;
  diasEmEstoque: number;
  dataLimite?: string | null;
  diasParaVencer?: number | null;
};

export type DashboardConsignment = {
  proximasVencer: DashboardConsignmentItem[];
  paradasEmEstoque: DashboardConsignmentItem[];
};

export type DashboardPendingItem = {
  tipo: string;
  titulo: string;
  descricao: string;
  valor?: number | null;
};

export type DashboardPending = {
  valorPagarFornecedores: number;
  valorPendenteRecebimento: number;
  quantidadeInconsistencias: number;
  inconsistencias: DashboardPendingItem[];
};

export type DashboardIndicatorRow = {
  chave: string;
  nome: string;
  totalPecas: number;
  pecasAtuais: number;
  pecasVendidasPeriodo: number;
  valorVendidoPeriodo: number;
};

export type DashboardIndicators = {
  porTipoPeca: DashboardIndicatorRow[];
  porMarca: DashboardIndicatorRow[];
  porFornecedor: DashboardIndicatorRow[];
};

export type DashboardOverview = {
  periodoInicio: string;
  periodoFim: string;
  vendas: DashboardSales;
  financeiro: DashboardFinancial;
  consignacao: DashboardConsignment;
  pendencias: DashboardPending;
  indicadores: DashboardIndicators;
};
