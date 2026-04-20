import { buildSolicitacaoQuery, type SolicitacaoFilters } from "@/lib/solicitacao";

const apiBaseUrl = process.env.NEXT_PUBLIC_API_URL?.replace(/\/$/, "") ?? "http://localhost:5268";

export async function getSolicitacoes(
  token: string,
  storeId: number,
  filters: SolicitacaoFilters,
): Promise<{ body: unknown; ok: boolean; status: number }> {
  const response = await fetch(
    `${apiBaseUrl}/api/solicitacao?${buildSolicitacaoQuery(storeId, filters)}`,
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

export async function createSolicitacao(
  payload: {
    produtoId: number | null;
    marcaId: number | null;
    tamanhoId: number | null;
    corId: number | null;
    clienteId: number | null;
    descricao: string;
    precoMaximo: number | null;
    lojaId: number;
  },
  token: string,
): Promise<{ body: unknown; ok: boolean; status: number }> {
  const response = await fetch(`${apiBaseUrl}/api/solicitacao`, {
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

export async function deleteSolicitacao(
  solicitacaoId: number,
  token: string,
): Promise<{ body: unknown; ok: boolean; status: number }> {
  const response = await fetch(`${apiBaseUrl}/api/solicitacao/${solicitacaoId}`, {
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
