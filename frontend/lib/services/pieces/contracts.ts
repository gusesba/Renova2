// Agrupa os contratos usados pelo modulo de pecas e estoque no frontend.
export type PieceOption = {
  codigo: string;
  nome: string;
};

export type PieceCatalogOption = {
  id: string;
  nome: string;
};

export type PieceSupplierOption = {
  pessoaId: string;
  pessoaLojaId: string;
  nome: string;
  documento: string;
  politicaPadraoFimConsignacao: string;
  statusRelacao: string;
};

export type PieceWorkspace = {
  lojaId: string;
  lojaNome: string;
  produtoNomes: PieceCatalogOption[];
  marcas: PieceCatalogOption[];
  tamanhos: PieceCatalogOption[];
  cores: PieceCatalogOption[];
  fornecedores: PieceSupplierOption[];
  tiposPeca: PieceOption[];
  statusPeca: PieceOption[];
  visibilidadesImagem: PieceOption[];
};

export type PieceCommercialCondition = {
  id: string;
  origemRegra: string;
  percentualRepasseDinheiro: number;
  percentualRepasseCredito: number;
  permitePagamentoMisto: boolean;
  tempoMaximoExposicaoDias: number;
  politicaDesconto: Array<{
    diasMinimos: number;
    percentualDesconto: number;
  }>;
  dataInicioConsignacao?: string | null;
  dataFimConsignacao?: string | null;
  destinoPadraoFimConsignacao?: string | null;
};

export type PieceImage = {
  id: string;
  urlArquivo: string;
  ordem: number;
  tipoVisibilidade: string;
};

export type PieceSummary = {
  id: string;
  codigoInterno: string;
  codigoBarras: string;
  tipoPeca: string;
  statusPeca: string;
  produtoNomeId: string;
  produtoNome: string;
  marcaId: string;
  marca: string;
  tamanhoId: string;
  tamanho: string;
  corId: string;
  cor: string;
  fornecedorPessoaId?: string | null;
  fornecedorNome?: string | null;
  dataEntrada: string;
  quantidadeAtual: number;
  precoBase: number;
  precoVendaAtual: number;
  precoEfetivoVenda: number;
  percentualDescontoAutomatico: number;
  descontoAutomaticoAtivo: boolean;
  localizacaoFisica: string;
  dataFimConsignacao?: string | null;
};

export type PieceDetail = {
  id: string;
  lojaId: string;
  codigoInterno: string;
  codigoBarras: string;
  tipoPeca: string;
  statusPeca: string;
  produtoNomeId: string;
  produtoNome: string;
  marcaId: string;
  marca: string;
  tamanhoId: string;
  tamanho: string;
  corId: string;
  cor: string;
  fornecedorPessoaId?: string | null;
  fornecedorNome?: string | null;
  descricao: string;
  observacoes: string;
  dataEntrada: string;
  quantidadeInicial: number;
  quantidadeAtual: number;
  precoBase: number;
  precoVendaAtual: number;
  precoEfetivoVenda: number;
  percentualDescontoAutomatico: number;
  descontoAutomaticoAtivo: boolean;
  custoUnitario?: number | null;
  localizacaoFisica: string;
  responsavelCadastroUsuarioId: string;
  condicaoComercial: PieceCommercialCondition;
  imagens: PieceImage[];
};
