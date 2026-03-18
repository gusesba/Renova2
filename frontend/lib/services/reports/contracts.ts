// Contratos consumidos pelo modulo 15 no frontend.
export type ReportOption = {
  codigo: string;
  nome: string;
};

export type ReportFilterOption = {
  id: string;
  nome: string;
  descricao: string | null;
};

export type ReportQueryPayload = {
  tipoRelatorio: string;
  lojaId: string | null;
  dataInicial: string | null;
  dataFinal: string | null;
  fornecedorPessoaId: string | null;
  pessoaId: string | null;
  marcaId: string | null;
  vendedorUsuarioId: string | null;
  statusPeca: string | null;
  motivoMovimentacao: string | null;
  search: string | null;
};

export type SavedReportFilter = {
  id: string;
  nome: string;
  tipoRelatorio: string;
  filtros: ReportQueryPayload;
  criadoEm: string;
};

export type ReportWorkspace = {
  lojaAtivaId: string;
  lojaAtivaNome: string;
  lojas: ReportFilterOption[];
  fornecedores: ReportFilterOption[];
  pessoasFinanceiras: ReportFilterOption[];
  marcas: ReportFilterOption[];
  vendedores: ReportFilterOption[];
  statusPeca: ReportOption[];
  motivosBaixa: ReportOption[];
  tiposRelatorio: ReportOption[];
  filtrosSalvos: SavedReportFilter[];
};

export type ReportMetric = {
  nome: string;
  valor: string;
};

export type ReportColumn = {
  chave: string;
  titulo: string;
};

export type ReportCell = {
  chave: string;
  valor: string;
};

export type ReportRow = {
  id: string;
  celulas: ReportCell[];
};

export type ReportResult = {
  tipoRelatorio: string;
  titulo: string;
  subtitulo: string;
  metricas: ReportMetric[];
  colunas: ReportColumn[];
  linhas: ReportRow[];
  quantidadeRegistros: number;
};
