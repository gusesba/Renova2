import type {
  PieceCommercialCondition,
  PieceDetail,
  PieceImage,
} from "@/lib/services/pieces";

// Estado editavel de uma faixa de desconto dentro da regra manual da peca.
export type PieceDiscountBandFormState = {
  id: string;
  diasMinimos: string;
  percentualDesconto: string;
};

// Estado editavel da regra manual da peca.
export type PieceManualRuleFormState = {
  percentualRepasseDinheiro: string;
  percentualRepasseCredito: string;
  permitePagamentoMisto: boolean;
  tempoMaximoExposicaoDias: string;
  politicaDesconto: PieceDiscountBandFormState[];
};

// Estado editavel do formulario principal de peca.
export type PieceFormState = {
  id: string;
  codigoInterno: string;
  tipoPeca: "consignada" | "fixa" | "lote";
  codigoBarras: string;
  produtoNomeId: string;
  marcaId: string;
  tamanhoId: string;
  corId: string;
  fornecedorPessoaId: string;
  descricao: string;
  observacoes: string;
  dataEntrada: string;
  quantidadeInicial: string;
  quantidadeAtual: string;
  precoVendaAtual: string;
  custoUnitario: string;
  localizacaoFisica: string;
  statusPeca: string;
  usarRegraManual: boolean;
  regraManual: PieceManualRuleFormState;
};

// Estado local dos filtros rapidos da listagem.
export type PieceFiltersState = {
  search: string;
  codigoBarras: string;
  statusPeca: string;
  produtoNomeId: string;
  marcaId: string;
  fornecedorPessoaId: string;
};

// Gera uma chave local para listas editaveis da pagina.
function createLocalId() {
  return globalThis.crypto?.randomUUID?.() ?? Math.random().toString(36).slice(2);
}

// Cria uma faixa de desconto vazia para a regra manual da peca.
export function createEmptyPieceDiscountBand(): PieceDiscountBandFormState {
  return {
    id: createLocalId(),
    diasMinimos: "",
    percentualDesconto: "",
  };
}

// Converte o snapshot comercial persistido para o formulario manual da tela.
export function mapConditionToManualRule(
  condition: PieceCommercialCondition,
): PieceManualRuleFormState {
  return {
    percentualRepasseDinheiro: String(condition.percentualRepasseDinheiro),
    percentualRepasseCredito: String(condition.percentualRepasseCredito),
    permitePagamentoMisto: condition.permitePagamentoMisto,
    tempoMaximoExposicaoDias: String(condition.tempoMaximoExposicaoDias),
    politicaDesconto: condition.politicaDesconto.map((band) => ({
      id: createLocalId(),
      diasMinimos: String(band.diasMinimos),
      percentualDesconto: String(band.percentualDesconto),
    })),
  };
}

// Define o formulario inicial vazio da peca.
export function emptyPieceForm(today: string): PieceFormState {
  return {
    id: "",
    codigoInterno: "",
    tipoPeca: "consignada",
    codigoBarras: "",
    produtoNomeId: "",
    marcaId: "",
    tamanhoId: "",
    corId: "",
    fornecedorPessoaId: "",
    descricao: "",
    observacoes: "",
    dataEntrada: today,
    quantidadeInicial: "1",
    quantidadeAtual: "1",
    precoVendaAtual: "",
    custoUnitario: "",
    localizacaoFisica: "",
    statusPeca: "disponivel",
    usarRegraManual: false,
    regraManual: {
      percentualRepasseDinheiro: "",
      percentualRepasseCredito: "",
      permitePagamentoMisto: false,
      tempoMaximoExposicaoDias: "",
      politicaDesconto: [],
    },
  };
}

// Converte o detalhe da API para o estado editavel da pagina.
export function mapPieceDetailToForm(detail: PieceDetail): PieceFormState {
  return {
    id: detail.id,
    codigoInterno: detail.codigoInterno,
    tipoPeca: detail.tipoPeca as "consignada" | "fixa" | "lote",
    codigoBarras: detail.codigoBarras,
    produtoNomeId: detail.produtoNomeId,
    marcaId: detail.marcaId,
    tamanhoId: detail.tamanhoId,
    corId: detail.corId,
    fornecedorPessoaId: detail.fornecedorPessoaId ?? "",
    descricao: detail.descricao,
    observacoes: detail.observacoes,
    dataEntrada: detail.dataEntrada.slice(0, 10),
    quantidadeInicial: String(detail.quantidadeInicial),
    quantidadeAtual: String(detail.quantidadeAtual),
    precoVendaAtual: String(detail.precoVendaAtual),
    custoUnitario:
      detail.custoUnitario === null || detail.custoUnitario === undefined
        ? ""
        : String(detail.custoUnitario),
    localizacaoFisica: detail.localizacaoFisica,
    statusPeca: detail.statusPeca,
    usarRegraManual: detail.condicaoComercial.origemRegra === "manual",
    regraManual: mapConditionToManualRule(detail.condicaoComercial),
  };
}

// Estado inicial dos filtros da listagem.
export const emptyPieceFilters: PieceFiltersState = {
  search: "",
  codigoBarras: "",
  statusPeca: "",
  produtoNomeId: "",
  marcaId: "",
  fornecedorPessoaId: "",
};

// Ordena as imagens para apresentacao e edicao no painel lateral.
export function sortPieceImages(images: PieceImage[]) {
  return [...images].sort((left, right) => {
    if (left.ordem !== right.ordem) {
      return left.ordem - right.ordem;
    }

    return left.id.localeCompare(right.id);
  });
}
