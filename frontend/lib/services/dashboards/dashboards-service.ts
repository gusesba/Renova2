import { callApi } from "@/lib/services/core/api-client";

import type { DashboardOverview, DashboardWorkspace } from "./contracts";

type DashboardFilters = {
  dataInicial?: string;
  dataFinal?: string;
  vendedorUsuarioId?: string;
  fornecedorPessoaId?: string;
  marcaId?: string;
  tipoPeca?: string;
};

// Reune as operacoes HTTP do modulo 14.
export async function getDashboardsWorkspace(token: string) {
  return callApi<DashboardWorkspace>("/dashboards/workspace", { method: "GET" }, token);
}

export async function getDashboardOverview(
  token: string,
  filters: DashboardFilters,
) {
  const queryString = buildQueryString(filters);
  return callApi<DashboardOverview>(
    `/dashboards/overview${queryString ? `?${queryString}` : ""}`,
    { method: "GET" },
    token,
  );
}

function buildQueryString(filters: DashboardFilters) {
  const params = new URLSearchParams();
  if (filters.dataInicial) {
    params.set("dataInicial", filters.dataInicial);
  }
  if (filters.dataFinal) {
    params.set("dataFinal", filters.dataFinal);
  }
  if (filters.vendedorUsuarioId) {
    params.set("vendedorUsuarioId", filters.vendedorUsuarioId);
  }
  if (filters.fornecedorPessoaId) {
    params.set("fornecedorPessoaId", filters.fornecedorPessoaId);
  }
  if (filters.marcaId) {
    params.set("marcaId", filters.marcaId);
  }
  if (filters.tipoPeca) {
    params.set("tipoPeca", filters.tipoPeca);
  }

  return params.toString();
}
