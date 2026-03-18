import { callApi } from "@/lib/services/core/api-client";

import type { SaleDetail, SaleSummary, SalesWorkspace } from "./contracts";

// Reune as operacoes HTTP do modulo 09.
export async function getSalesWorkspace(token: string) {
  return callApi<SalesWorkspace>("/sales/workspace", { method: "GET" }, token);
}

export async function listSales(
  token: string,
  filters: {
    search?: string;
    statusVenda?: string;
    compradorPessoaId?: string;
    dataInicial?: string;
    dataFinal?: string;
  },
) {
  const params = new URLSearchParams();
  if (filters.search) {
    params.set("search", filters.search);
  }
  if (filters.statusVenda) {
    params.set("statusVenda", filters.statusVenda);
  }
  if (filters.compradorPessoaId) {
    params.set("compradorPessoaId", filters.compradorPessoaId);
  }
  if (filters.dataInicial) {
    params.set("dataInicial", filters.dataInicial);
  }
  if (filters.dataFinal) {
    params.set("dataFinal", filters.dataFinal);
  }

  const queryString = params.toString();
  return callApi<SaleSummary[]>(
    `/sales${queryString ? `?${queryString}` : ""}`,
    { method: "GET" },
    token,
  );
}

export async function getSaleById(token: string, saleId: string) {
  return callApi<SaleDetail>(`/sales/${saleId}`, { method: "GET" }, token);
}

export async function createSale(
  token: string,
  payload: {
    compradorPessoaId?: string | null;
    observacoes: string;
    itens: Array<{
      pecaId: string;
      quantidade: number;
      descontoUnitario: number;
    }>;
    pagamentos: Array<{
      tipoPagamento: string;
      meioPagamentoId?: string | null;
      valor: number;
    }>;
  },
) {
  return callApi<SaleDetail>(
    "/sales",
    {
      method: "POST",
      body: JSON.stringify(payload),
    },
    token,
  );
}

export async function cancelSale(
  token: string,
  saleId: string,
  payload: {
    motivoCancelamento: string;
  },
) {
  return callApi<SaleDetail>(
    `/sales/${saleId}/cancel`,
    {
      method: "POST",
      body: JSON.stringify(payload),
    },
    token,
  );
}
