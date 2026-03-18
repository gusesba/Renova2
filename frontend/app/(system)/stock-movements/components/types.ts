import type { StockPieceLookup } from "@/lib/services/stock-movements";

// Estado local dos filtros da listagem principal de movimentacoes.
export type StockMovementFiltersState = {
  search: string;
  pecaId: string;
  fornecedorPessoaId: string;
  statusPeca: string;
  tipoMovimentacao: string;
  dataInicial: string;
  dataFinal: string;
};

// Estado local da busca operacional de pecas.
export type StockPieceSearchFiltersState = {
  search: string;
  codigoBarras: string;
  fornecedorPessoaId: string;
  statusPeca: string;
  tempoMinimoLojaDias: string;
};

// Estado editavel do formulario de ajuste manual.
export type StockAdjustmentFormState = {
  pecaId: string;
  quantidadeNova: string;
  statusPeca: string;
  motivo: string;
};

// Filtros iniciais da listagem de movimentos.
export const emptyStockMovementFilters: StockMovementFiltersState = {
  search: "",
  pecaId: "",
  fornecedorPessoaId: "",
  statusPeca: "",
  tipoMovimentacao: "",
  dataInicial: "",
  dataFinal: "",
};

// Filtros iniciais da busca operacional de pecas.
export const emptyStockPieceSearchFilters: StockPieceSearchFiltersState = {
  search: "",
  codigoBarras: "",
  fornecedorPessoaId: "",
  statusPeca: "",
  tempoMinimoLojaDias: "",
};

// Formulario inicial de ajuste manual sem peca selecionada.
export function emptyStockAdjustmentForm(): StockAdjustmentFormState {
  return {
    pecaId: "",
    quantidadeNova: "",
    statusPeca: "",
    motivo: "",
  };
}

// Converte a peca selecionada para o formulario de ajuste da lateral.
export function mapPieceToAdjustmentForm(
  piece: StockPieceLookup,
): StockAdjustmentFormState {
  return {
    pecaId: piece.id,
    quantidadeNova: String(piece.quantidadeAtual),
    statusPeca: piece.statusPeca,
    motivo: "",
  };
}
