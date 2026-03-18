import { callApi } from "@/lib/services/core/api-client";

import type {
  ReportQueryPayload,
  ReportResult,
  ReportWorkspace,
  SavedReportFilter,
} from "./contracts";

const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:5131/api/v1";

// Reune as operacoes HTTP do modulo 15.
export async function getReportsWorkspace(token: string) {
  return callApi<ReportWorkspace>("/reports/workspace", { method: "GET" }, token);
}

export async function runReport(token: string, payload: ReportQueryPayload) {
  return callApi<ReportResult>(
    "/reports/run",
    {
      method: "POST",
      body: JSON.stringify(payload),
    },
    token,
  );
}

export async function saveReportFilter(
  token: string,
  payload: {
    nome: string;
    filtros: ReportQueryPayload;
  },
) {
  return callApi<SavedReportFilter>(
    "/reports/saved-filters",
    {
      method: "POST",
      body: JSON.stringify(payload),
    },
    token,
  );
}

export async function deleteReportFilter(token: string, filterId: string) {
  const response = await fetch(`${API_BASE_URL}/reports/saved-filters/${filterId}`, {
    method: "DELETE",
    headers: {
      Authorization: `Bearer ${token}`,
    },
    cache: "no-store",
  });

  if (!response.ok) {
    let message = "Falha ao remover o filtro salvo.";

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
}

export async function downloadReportExport(
  token: string,
  format: "pdf" | "excel",
  payload: ReportQueryPayload,
) {
  const response = await fetch(`${API_BASE_URL}/reports/export/${format}`, {
    method: "POST",
    headers: {
      Authorization: `Bearer ${token}`,
      "Content-Type": "application/json",
    },
    body: JSON.stringify(payload),
    cache: "no-store",
  });

  if (!response.ok) {
    let message = "Falha ao exportar o relatorio.";

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
    fileNameMatch?.[1] ?? `relatorio.${format === "pdf" ? "html" : "csv"}`;

  return {
    blob: await response.blob(),
    fileName,
  };
}
