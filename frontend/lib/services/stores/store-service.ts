import { callApi } from "@/lib/services/core/api-client";

import type { StoreSummary } from "./contracts";

// Reune as operacoes HTTP do modulo de lojas.
export async function listAccessibleStores(token: string) {
  return callApi<StoreSummary[]>("/stores/accessible", { method: "GET" }, token);
}

export async function createStore(
  token: string,
  payload: {
    nomeFantasia: string;
    razaoSocial: string;
    documento: string;
    telefone: string;
    email: string;
    logradouro: string;
    numero: string;
    complemento: string;
    bairro: string;
    cidade: string;
    uf: string;
    cep: string;
  },
) {
  return callApi<StoreSummary>(
    "/stores",
    {
      method: "POST",
      body: JSON.stringify(payload),
    },
    token,
  );
}

export async function updateStore(
  token: string,
  lojaId: string,
  payload: {
    nomeFantasia: string;
    razaoSocial: string;
    documento: string;
    telefone: string;
    email: string;
    logradouro: string;
    numero: string;
    complemento: string;
    bairro: string;
    cidade: string;
    uf: string;
    cep: string;
    statusLoja: string;
  },
) {
  return callApi<StoreSummary>(
    `/stores/${lojaId}`,
    {
      method: "PUT",
      body: JSON.stringify(payload),
    },
    token,
  );
}
