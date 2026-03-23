import { callApi } from "@/lib/services/core/api-client";

import type {
  CloseConsignmentResult,
  ConsignmentDetail,
  ConsignmentPieceSummary,
  ConsignmentWorkspace,
} from "./contracts";

// Reune as operacoes HTTP do modulo 07.
export async function getConsignmentsWorkspace(token: string) {
  return callApi<ConsignmentWorkspace>(
    "/consignments/workspace",
    { method: "GET" },
    token,
  );
}

export async function listConsignments(
  token: string,
  filters: {
    search?: string;
    fornecedorPessoaId?: string;
    statusConsignacao?: string;
    somenteProximasDoFim?: boolean;
    somenteDescontoPendente?: boolean;
  },
) {
  const params = new URLSearchParams();
  if (filters.search) {
    params.set("search", filters.search);
  }
  if (filters.fornecedorPessoaId) {
    params.set("fornecedorPessoaId", filters.fornecedorPessoaId);
  }
  if (filters.statusConsignacao) {
    params.set("statusConsignacao", filters.statusConsignacao);
  }
  if (filters.somenteProximasDoFim) {
    params.set("somenteProximasDoFim", "true");
  }
  if (filters.somenteDescontoPendente) {
    params.set("somenteDescontoPendente", "true");
  }

  const queryString = params.toString();
  return callApi<ConsignmentPieceSummary[]>(
    `/consignments${queryString ? `?${queryString}` : ""}`,
    { method: "GET" },
    token,
  );
}

export async function getConsignmentById(token: string, pecaId: string) {
  return callApi<ConsignmentDetail>(`/consignments/${pecaId}`, { method: "GET" }, token);
}

export async function closeConsignment(
  token: string,
  pecaId: string,
  payload: {
    acao: string;
    motivo: string;
  },
) {
  return callApi<CloseConsignmentResult>(
    `/consignments/${pecaId}/close`,
    {
      method: "POST",
      body: JSON.stringify(payload),
    },
    token,
  );
}
