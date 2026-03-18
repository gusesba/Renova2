import { callApi } from "@/lib/services/core/api-client";

import type {
  FinancialLedgerEntry,
  FinancialReconciliation,
  FinancialWorkspace,
} from "./contracts";

type FinancialFilters = {
  search?: string;
  meioPagamentoId?: string;
  tipoMovimentacao?: string;
  direcao?: string;
  dataInicial?: string;
  dataFinal?: string;
};

// Reune as operacoes HTTP do modulo 12.
export async function getFinancialWorkspace(token: string) {
  return callApi<FinancialWorkspace>("/financial/workspace", { method: "GET" }, token);
}

export async function listFinancialEntries(
  token: string,
  filters: FinancialFilters,
) {
  const queryString = buildQueryString(filters);
  return callApi<FinancialLedgerEntry[]>(
    `/financial${queryString ? `?${queryString}` : ""}`,
    { method: "GET" },
    token,
  );
}

export async function getFinancialReconciliation(
  token: string,
  filters: FinancialFilters,
) {
  const queryString = buildQueryString(filters);
  return callApi<FinancialReconciliation>(
    `/financial/reconciliation${queryString ? `?${queryString}` : ""}`,
    { method: "GET" },
    token,
  );
}

export async function registerFinancialEntry(
  token: string,
  payload: {
    tipoMovimentacao: string;
    direcao: string;
    meioPagamentoId?: string | null;
    valorBruto: number;
    taxa: number;
    descricao: string;
    competenciaEm?: string | null;
    movimentadoEm?: string | null;
  },
) {
  return callApi<FinancialLedgerEntry>(
    "/financial/entries",
    {
      method: "POST",
      body: JSON.stringify(payload),
    },
    token,
  );
}

function buildQueryString(filters: FinancialFilters) {
  const params = new URLSearchParams();
  if (filters.search) {
    params.set("search", filters.search);
  }
  if (filters.meioPagamentoId) {
    params.set("meioPagamentoId", filters.meioPagamentoId);
  }
  if (filters.tipoMovimentacao) {
    params.set("tipoMovimentacao", filters.tipoMovimentacao);
  }
  if (filters.direcao) {
    params.set("direcao", filters.direcao);
  }
  if (filters.dataInicial) {
    params.set("dataInicial", filters.dataInicial);
  }
  if (filters.dataFinal) {
    params.set("dataFinal", filters.dataFinal);
  }

  return params.toString();
}
