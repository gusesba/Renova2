import type { LojaResponse } from "@/lib/store";

const apiBaseUrl = process.env.NEXT_PUBLIC_API_URL?.replace(/\/$/, "") ?? "http://localhost:5268";

async function parseResponseBody(response: Response) {
  const contentType = response.headers.get("content-type") ?? "";
  return contentType.includes("application/json") ? ((await response.json()) as unknown) : null;
}

export async function createStore(
  payload: { nome: string },
  token: string,
): Promise<{ body: unknown; ok: boolean; status: number }> {
  const response = await fetch(`${apiBaseUrl}/api/loja`, {
    method: "POST",
    headers: {
      Authorization: `Bearer ${token}`,
      "Content-Type": "application/json",
    },
    body: JSON.stringify(payload),
  });

  const body = await parseResponseBody(response);

  return {
    body,
    ok: response.ok,
    status: response.status,
  };
}

export async function getStores(
  token: string,
): Promise<{ body: unknown; ok: boolean; status: number }> {
  const response = await fetch(`${apiBaseUrl}/api/loja`, {
    method: "GET",
    headers: {
      Authorization: `Bearer ${token}`,
    },
  });

  const body = await parseResponseBody(response);

  return {
    body,
    ok: response.ok,
    status: response.status,
  };
}

export async function updateStore(
  storeId: number,
  payload: { nome: string },
  token: string,
): Promise<{ body: unknown; ok: boolean; status: number }> {
  const response = await fetch(`${apiBaseUrl}/api/loja/${storeId}`, {
    method: "PUT",
    headers: {
      Authorization: `Bearer ${token}`,
      "Content-Type": "application/json",
    },
    body: JSON.stringify(payload),
  });

  const body = await parseResponseBody(response);

  return {
    body,
    ok: response.ok,
    status: response.status,
  };
}

export async function deleteStore(
  storeId: number,
  token: string,
): Promise<{ body: unknown; ok: boolean; status: number }> {
  const response = await fetch(`${apiBaseUrl}/api/loja/${storeId}`, {
    method: "DELETE",
    headers: {
      Authorization: `Bearer ${token}`,
    },
  });

  const body = await parseResponseBody(response);

  return {
    body,
    ok: response.ok,
    status: response.status,
  };
}

export function asStoreResponse(body: unknown) {
  return body as LojaResponse;
}

export function asStoreListResponse(body: unknown) {
  return body as LojaResponse[];
}
