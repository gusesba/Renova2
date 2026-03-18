import { callApi } from "@/lib/services/core/api-client";

import type {
  DocumentSearchItem,
  DocumentTypeCode,
  DocumentWorkspace,
} from "./contracts";

const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:5131/api/v1";

// Reune as operacoes HTTP do modulo 16.
export async function getDocumentsWorkspace(token: string) {
  return callApi<DocumentWorkspace>("/documents/workspace", { method: "GET" }, token);
}

export async function searchDocuments(
  token: string,
  type: DocumentTypeCode,
  search: string,
) {
  const query = new URLSearchParams();
  if (search.trim()) {
    query.set("search", search.trim());
  }

  return callApi<DocumentSearchItem[]>(
    `${resolveSearchPath(type)}${query.size ? `?${query.toString()}` : ""}`,
    { method: "GET" },
    token,
  );
}

export async function printDocument(
  token: string,
  type: DocumentTypeCode,
  itemId: string,
) {
  const response = await fetch(`${API_BASE_URL}${resolvePrintPath(type, itemId)}`, {
    method: "GET",
    headers: {
      Authorization: `Bearer ${token}`,
    },
    cache: "no-store",
  });

  if (!response.ok) {
    let message = "Falha ao gerar o documento.";

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

  return {
    blob: await response.blob(),
    fileName: fileNameMatch?.[1] ?? "documento.html",
  };
}

function resolveSearchPath(type: DocumentTypeCode) {
  switch (type) {
    case "etiqueta":
      return "/documents/labels";
    case "recibo_venda":
      return "/documents/sales";
    case "comprovante_fornecedor":
      return "/documents/supplier-payments";
    case "comprovante_consignacao":
      return "/documents/consignments";
  }
}

function resolvePrintPath(type: DocumentTypeCode, itemId: string) {
  switch (type) {
    case "etiqueta":
      return `/documents/labels/${itemId}`;
    case "recibo_venda":
      return `/documents/sales/${itemId}`;
    case "comprovante_fornecedor":
      return `/documents/supplier-payments/${itemId}`;
    case "comprovante_consignacao":
      return `/documents/consignments/${itemId}`;
  }
}
