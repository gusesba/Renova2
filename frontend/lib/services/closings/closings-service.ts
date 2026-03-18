import { callApi } from "@/lib/services/core/api-client";

import type { ClosingDetail, ClosingSummary, ClosingWorkspace } from "./contracts";

type ClosingFilters = {
  search?: string;
  pessoaId?: string;
  statusFechamento?: string;
  dataInicial?: string;
  dataFinal?: string;
};

type ClosingExportType = "pdf" | "excel";

const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:5131/api/v1";

// Reune as operacoes HTTP do modulo 13.
export async function getClosingsWorkspace(token: string) {
  return callApi<ClosingWorkspace>("/closings/workspace", { method: "GET" }, token);
}

export async function listClosings(token: string, filters: ClosingFilters) {
  const queryString = buildQueryString(filters);
  return callApi<ClosingSummary[]>(
    `/closings${queryString ? `?${queryString}` : ""}`,
    { method: "GET" },
    token,
  );
}

export async function getClosingById(token: string, closingId: string) {
  return callApi<ClosingDetail>(`/closings/${closingId}`, { method: "GET" }, token);
}

export async function generateClosing(
  token: string,
  payload: {
    pessoaId: string;
    periodoInicio: string;
    periodoFim: string;
  },
) {
  return callApi<ClosingDetail>(
    "/closings",
    {
      method: "POST",
      body: JSON.stringify(payload),
    },
    token,
  );
}

export async function reviewClosing(token: string, closingId: string) {
  return callApi<ClosingDetail>(
    `/closings/${closingId}/review`,
    { method: "POST" },
    token,
  );
}

export async function settleClosing(token: string, closingId: string) {
  return callApi<ClosingDetail>(
    `/closings/${closingId}/settle`,
    { method: "POST" },
    token,
  );
}

export async function downloadClosingExport(
  token: string,
  closingId: string,
  exportType: ClosingExportType,
) {
  const response = await fetch(`${API_BASE_URL}/closings/${closingId}/export/${exportType}`, {
    method: "GET",
    headers: {
      Authorization: `Bearer ${token}`,
    },
    cache: "no-store",
  });

  if (!response.ok) {
    let message = "Falha ao exportar o fechamento.";

    try {
      const body = (await response.json()) as { detail?: string; title?: string };
      message = body.detail ?? body.title ?? message;
    } catch {
      if (response.status === 401 || response.status === 403) {
        message = "Voce nao tem acesso a esta funcionalidade.";
      }
    }

    throw new Error(message);
  }

  const contentDisposition = response.headers.get("content-disposition") ?? "";
  const fileNameMatch = /filename=\"?([^\";]+)\"?/i.exec(contentDisposition);
  const fileName =
    fileNameMatch?.[1] ?? `fechamento-${closingId}.${exportType === "pdf" ? "html" : "csv"}`;

  return {
    blob: await response.blob(),
    fileName,
  };
}

function buildQueryString(filters: ClosingFilters) {
  const params = new URLSearchParams();
  if (filters.search) {
    params.set("search", filters.search);
  }
  if (filters.pessoaId) {
    params.set("pessoaId", filters.pessoaId);
  }
  if (filters.statusFechamento) {
    params.set("statusFechamento", filters.statusFechamento);
  }
  if (filters.dataInicial) {
    params.set("dataInicial", filters.dataInicial);
  }
  if (filters.dataFinal) {
    params.set("dataFinal", filters.dataFinal);
  }

  return params.toString();
}
