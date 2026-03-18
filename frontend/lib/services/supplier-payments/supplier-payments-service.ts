import { callApi } from "@/lib/services/core/api-client";

import type {
  SupplierObligationDetail,
  SupplierObligationSummary,
  SupplierPaymentWorkspace,
} from "./contracts";

// Reune as operacoes HTTP do modulo 11.
export async function getSupplierPaymentsWorkspace(token: string) {
  return callApi<SupplierPaymentWorkspace>(
    "/supplier-payments/workspace",
    { method: "GET" },
    token,
  );
}

export async function listSupplierObligations(
  token: string,
  filters: {
    search?: string;
    pessoaId?: string;
    statusObrigacao?: string;
    tipoObrigacao?: string;
  },
) {
  const params = new URLSearchParams();
  if (filters.search) {
    params.set("search", filters.search);
  }
  if (filters.pessoaId) {
    params.set("pessoaId", filters.pessoaId);
  }
  if (filters.statusObrigacao) {
    params.set("statusObrigacao", filters.statusObrigacao);
  }
  if (filters.tipoObrigacao) {
    params.set("tipoObrigacao", filters.tipoObrigacao);
  }

  const queryString = params.toString();
  return callApi<SupplierObligationSummary[]>(
    `/supplier-payments${queryString ? `?${queryString}` : ""}`,
    { method: "GET" },
    token,
  );
}

export async function getSupplierObligationById(token: string, obligationId: string) {
  return callApi<SupplierObligationDetail>(
    `/supplier-payments/${obligationId}`,
    { method: "GET" },
    token,
  );
}

export async function settleSupplierObligation(
  token: string,
  obligationId: string,
  payload: {
    pagamentos: Array<{
      tipoLiquidacao: string;
      meioPagamentoId?: string | null;
      valor: number;
    }>;
    comprovanteUrl?: string | null;
    observacoes: string;
  },
) {
  return callApi<SupplierObligationDetail>(
    `/supplier-payments/${obligationId}/settle`,
    {
      method: "POST",
      body: JSON.stringify(payload),
    },
    token,
  );
}
