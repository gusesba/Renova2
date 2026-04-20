import type { ClientDetailFilters, ClientFilters } from "@/lib/client";
import type { ClientAreaFilters } from "@/lib/client-area";
import { buildClientDetailQuery, buildClientQuery } from "@/lib/client";
import { buildClientAreaQuery } from "@/lib/client-area";

const apiBaseUrl = process.env.NEXT_PUBLIC_API_URL?.replace(/\/$/, "") ?? "http://localhost:5268";

export async function getClients(
  token: string,
  storeId: number,
  filters: ClientFilters,
): Promise<{ body: unknown; ok: boolean; status: number }> {
  const response = await fetch(`${apiBaseUrl}/api/cliente?${buildClientQuery(storeId, filters)}`, {
    method: "GET",
    headers: {
      Authorization: `Bearer ${token}`,
    },
  });

  const contentType = response.headers.get("content-type") ?? "";
  const body = contentType.includes("application/json")
    ? ((await response.json()) as unknown)
    : null;

  return {
    body,
    ok: response.ok,
    status: response.status,
  };
}

export async function createClient(
  payload: {
    nome: string;
    contato: string;
    obs?: string;
    doacao: boolean;
    lojaId: number;
    userId?: number;
  },
  token: string,
): Promise<{ body: unknown; ok: boolean; status: number }> {
  const response = await fetch(`${apiBaseUrl}/api/cliente`, {
    method: "POST",
    headers: {
      Authorization: `Bearer ${token}`,
      "Content-Type": "application/json",
    },
    body: JSON.stringify(payload),
  });

  const contentType = response.headers.get("content-type") ?? "";
  const body = contentType.includes("application/json")
    ? ((await response.json()) as unknown)
    : null;

  return {
    body,
    ok: response.ok,
    status: response.status,
  };
}

export async function updateClient(
  clientId: number,
  payload: { nome: string; contato: string; obs?: string; doacao: boolean; userId?: number },
  token: string,
): Promise<{ body: unknown; ok: boolean; status: number }> {
  const response = await fetch(`${apiBaseUrl}/api/cliente/${clientId}`, {
    method: "PUT",
    headers: {
      Authorization: `Bearer ${token}`,
      "Content-Type": "application/json",
    },
    body: JSON.stringify(payload),
  });

  const contentType = response.headers.get("content-type") ?? "";
  const body = contentType.includes("application/json")
    ? ((await response.json()) as unknown)
    : null;

  return {
    body,
    ok: response.ok,
    status: response.status,
  };
}

export async function deleteClient(
  clientId: number,
  token: string,
): Promise<{ body: unknown; ok: boolean; status: number }> {
  const response = await fetch(`${apiBaseUrl}/api/cliente/${clientId}`, {
    method: "DELETE",
    headers: {
      Authorization: `Bearer ${token}`,
    },
  });

  const contentType = response.headers.get("content-type") ?? "";
  const body = contentType.includes("application/json")
    ? ((await response.json()) as unknown)
    : null;

  return {
    body,
    ok: response.ok,
    status: response.status,
  };
}

export async function getClientDetail(
  token: string,
  storeId: number,
  clientId: number,
  filters: ClientDetailFilters,
): Promise<{ body: unknown; ok: boolean; status: number }> {
  const response = await fetch(
    `${apiBaseUrl}/api/cliente/${clientId}/detalhe?${buildClientDetailQuery(storeId, filters)}`,
    {
      method: "GET",
      headers: {
        Authorization: `Bearer ${token}`,
      },
    },
  );

  const contentType = response.headers.get("content-type") ?? "";
  const body = contentType.includes("application/json")
    ? ((await response.json()) as unknown)
    : null;

  return {
    body,
    ok: response.ok,
    status: response.status,
  };
}

export async function getMyClientProducts(
  token: string,
  filters: ClientAreaFilters,
): Promise<{ body: unknown; ok: boolean; status: number }> {
  const response = await fetch(`${apiBaseUrl}/api/cliente/minhas-pecas?${buildClientAreaQuery(filters)}`, {
    method: "GET",
    headers: {
      Authorization: `Bearer ${token}`,
    },
  });

  const contentType = response.headers.get("content-type") ?? "";
  const body = contentType.includes("application/json")
    ? ((await response.json()) as unknown)
    : null;

  return {
    body,
    ok: response.ok,
    status: response.status,
  };
}

export async function getMyCustomerProducts(
  token: string,
  filters: ClientAreaFilters,
): Promise<{ body: unknown; ok: boolean; status: number }> {
  const response = await fetch(`${apiBaseUrl}/api/cliente/meus-produtos?${buildClientAreaQuery(filters)}`, {
    method: "GET",
    headers: {
      Authorization: `Bearer ${token}`,
    },
  });

  const contentType = response.headers.get("content-type") ?? "";
  const body = contentType.includes("application/json")
    ? ((await response.json()) as unknown)
    : null;

  return {
    body,
    ok: response.ok,
    status: response.status,
  };
}

export async function exportClientClosing(
  token: string,
  storeId: number,
  filters: { dataInicial: string; dataFinal: string },
): Promise<{ blob: Blob | null; fileName: string | null; ok: boolean; status: number }> {
  const params = new URLSearchParams({
    lojaId: String(storeId),
    dataInicial: `${filters.dataInicial}T00:00:00`,
    dataFinal: `${filters.dataFinal}T23:59:59.999`,
  });

  const response = await fetch(`${apiBaseUrl}/api/cliente/fechamento/exportar?${params.toString()}`, {
    method: "GET",
    headers: {
      Authorization: `Bearer ${token}`,
    },
  });

  const contentDisposition = response.headers.get("content-disposition");
  const fileNameMatch = contentDisposition?.match(/filename=\"?([^\";]+)\"?/i);
  const fileName = fileNameMatch?.[1] ?? null;

  return {
    blob: response.ok ? await response.blob() : null,
    fileName,
    ok: response.ok,
    status: response.status,
  };
}
