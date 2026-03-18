import { callApi } from "@/lib/services/core/api-client";

import type { CreditAccountDetail, CreditsWorkspace } from "./contracts";

// Reune as operacoes HTTP do modulo 10.
export async function getCreditsWorkspace(token: string) {
  return callApi<CreditsWorkspace>("/credits/workspace", { method: "GET" }, token);
}

export async function getCreditAccountByPerson(token: string, pessoaId: string) {
  return callApi<CreditAccountDetail>(`/credits/person/${pessoaId}`, { method: "GET" }, token);
}

export async function getMyCreditAccount(token: string) {
  return callApi<CreditAccountDetail>("/credits/me", { method: "GET" }, token);
}

export async function ensureCreditAccount(
  token: string,
  payload: {
    pessoaId: string;
  },
) {
  return callApi<CreditAccountDetail>(
    "/credits/accounts",
    {
      method: "POST",
      body: JSON.stringify(payload),
    },
    token,
  );
}

export async function registerManualCredit(
  token: string,
  payload: {
    pessoaId: string;
    valor: number;
    justificativa: string;
  },
) {
  return callApi<CreditAccountDetail>(
    "/credits/manual",
    {
      method: "POST",
      body: JSON.stringify(payload),
    },
    token,
  );
}

export async function updateCreditAccountStatus(
  token: string,
  contaId: string,
  payload: {
    statusConta: string;
  },
) {
  return callApi<CreditAccountDetail>(
    `/credits/accounts/${contaId}/status`,
    {
      method: "PUT",
      body: JSON.stringify(payload),
    },
    token,
  );
}
