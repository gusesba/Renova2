import { callApi } from "@/lib/services/core/api-client";

import type {
  AdjustStockResponse,
  StockMovementItem,
  StockMovementWorkspace,
  StockPieceLookup,
} from "@/lib/services/stock-movements/contracts";

type StockMovementListQuery = {
  search?: string;
  pecaId?: string;
  fornecedorPessoaId?: string;
  statusPeca?: string;
  tipoMovimentacao?: string;
  dataInicial?: string;
  dataFinal?: string;
};

type StockPieceSearchQuery = {
  search?: string;
  codigoBarras?: string;
  fornecedorPessoaId?: string;
  statusPeca?: string;
  tempoMinimoLojaDias?: string;
};

// Carrega o resumo e as opcoes auxiliares do modulo 08.
export async function getStockMovementsWorkspace(token: string) {
  return callApi<StockMovementWorkspace>(
    "/stock-movements/workspace",
    { method: "GET" },
    token,
  );
}

// Lista as movimentacoes de estoque da loja ativa com filtros operacionais.
export async function listStockMovements(
  token: string,
  query: StockMovementListQuery,
) {
  const params = new URLSearchParams();
  if (query.search) params.set("search", query.search);
  if (query.pecaId) params.set("pecaId", query.pecaId);
  if (query.fornecedorPessoaId) {
    params.set("fornecedorPessoaId", query.fornecedorPessoaId);
  }
  if (query.statusPeca) params.set("statusPeca", query.statusPeca);
  if (query.tipoMovimentacao) {
    params.set("tipoMovimentacao", query.tipoMovimentacao);
  }
  if (query.dataInicial) params.set("dataInicial", query.dataInicial);
  if (query.dataFinal) params.set("dataFinal", query.dataFinal);

  const queryString = params.toString();
  return callApi<StockMovementItem[]>(
    `/stock-movements${queryString ? `?${queryString}` : ""}`,
    { method: "GET" },
    token,
  );
}

// Busca pecas da loja ativa para consulta operacional e ajuste manual.
export async function searchStockPieces(
  token: string,
  query: StockPieceSearchQuery,
) {
  const params = new URLSearchParams();
  if (query.search) params.set("search", query.search);
  if (query.codigoBarras) params.set("codigoBarras", query.codigoBarras);
  if (query.fornecedorPessoaId) {
    params.set("fornecedorPessoaId", query.fornecedorPessoaId);
  }
  if (query.statusPeca) params.set("statusPeca", query.statusPeca);
  if (query.tempoMinimoLojaDias) {
    params.set("tempoMinimoLojaDias", query.tempoMinimoLojaDias);
  }

  const queryString = params.toString();
  return callApi<StockPieceLookup[]>(
    `/stock-movements/pieces${queryString ? `?${queryString}` : ""}`,
    { method: "GET" },
    token,
  );
}

// Registra um ajuste manual de estoque para a peca selecionada.
export async function adjustStock(
  token: string,
  payload: {
    pecaId: string;
    quantidadeNova: number;
    statusPeca?: string | null;
    motivo: string;
  },
) {
  return callApi<AdjustStockResponse>(
    "/stock-movements/adjustments",
    {
      body: JSON.stringify(payload),
      method: "POST",
    },
    token,
  );
}
