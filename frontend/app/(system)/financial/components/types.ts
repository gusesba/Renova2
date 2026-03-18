import type {
  FinancialLedgerEntry,
  FinancialWorkspace,
} from "@/lib/services/financial";

export type FinancialFiltersState = {
  search: string;
  meioPagamentoId: string;
  tipoMovimentacao: string;
  direcao: string;
  dataInicial: string;
  dataFinal: string;
};

export type FinancialEntryFormState = {
  tipoMovimentacao: string;
  direcao: string;
  meioPagamentoId: string;
  valorBruto: string;
  taxa: string;
  descricao: string;
  competenciaEm: string;
  movimentadoEm: string;
};

export const emptyFinancialFilters: FinancialFiltersState = {
  search: "",
  meioPagamentoId: "",
  tipoMovimentacao: "",
  direcao: "",
  dataInicial: "",
  dataFinal: "",
};

// Cria o rascunho padrao do formulario de lancamento manual.
export function createFinancialEntryForm(workspace?: FinancialWorkspace) {
  const defaultType = workspace?.tiposLancamentoManual[0]?.codigo ?? "despesa";
  const today = new Date().toISOString().slice(0, 10);

  return {
    tipoMovimentacao: defaultType,
    direcao: resolveDirection(defaultType),
    meioPagamentoId: "",
    valorBruto: "",
    taxa: "0",
    descricao: "",
    competenciaEm: today,
    movimentadoEm: today,
  } satisfies FinancialEntryFormState;
}

// Ajusta a direcao automatica dos tipos que ja sao definidos pela regra.
export function resolveDirection(movementType: string) {
  if (movementType === "despesa") {
    return "saida";
  }

  if (movementType === "receita_avulsa") {
    return "entrada";
  }

  return "entrada";
}

// Resume a linha do livro razao para uso em cards e listas.
export function describeLedgerEntry(entry: FinancialLedgerEntry) {
  if (entry.numeroVenda) {
    return `Venda ${entry.numeroVenda}`;
  }

  if (entry.fornecedorNome) {
    return `Fornecedor ${entry.fornecedorNome}`;
  }

  return "Lancamento avulso";
}
