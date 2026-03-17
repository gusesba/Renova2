const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:5131/api/v1";

type Envelope<T> = {
  data: T;
};

type ErrorEnvelope = {
  detail?: string;
  title?: string;
};

// Centraliza o transporte HTTP compartilhado pelos services do frontend.
export async function callApi<T>(
  path: string,
  init: RequestInit,
  token?: string | null,
) {
  const headers = new Headers(init.headers);
  headers.set("Content-Type", "application/json");

  if (token) {
    headers.set("Authorization", `Bearer ${token}`);
  }

  let response: Response;

  try {
    response = await fetch(`${API_BASE_URL}${path}`, {
      ...init,
      headers,
      cache: "no-store",
    });
  } catch {
    throw new Error("Servidor indisponivel. Tente novamente em instantes.");
  }

  if (response.status === 204) {
    return undefined as T;
  }

  const rawBody = await response.text();
  const body = rawBody
    ? ((JSON.parse(rawBody) as Envelope<T> & ErrorEnvelope))
    : null;

  if (!response.ok) {
    throw new Error(
      body?.detail ??
        body?.title ??
        (response.status === 401 || response.status === 403
          ? "Voce nao tem acesso a esta funcionalidade."
          : "Falha ao consultar a API."),
    );
  }

  if (!body) {
    throw new Error("Resposta invalida do servidor.");
  }

  return body.data;
}
