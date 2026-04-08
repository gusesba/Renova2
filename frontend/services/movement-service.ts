import { buildMovementQuery, type MovementFilters } from "@/lib/movement";

const apiBaseUrl = process.env.NEXT_PUBLIC_API_URL?.replace(/\/$/, "") ?? "http://localhost:5268";

export async function getMovements(
  token: string,
  storeId: number,
  filters: MovementFilters,
): Promise<{ body: unknown; ok: boolean; status: number }> {
  const response = await fetch(
    `${apiBaseUrl}/api/movimentacao?${buildMovementQuery(storeId, filters)}`,
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

export async function createMovement(
  payload: {
    tipo: number;
    data: string;
    clienteId: number;
    lojaId: number;
    produtoIds: number[];
  },
  token: string,
): Promise<{ body: unknown; ok: boolean; status: number }> {
  const response = await fetch(`${apiBaseUrl}/api/movimentacao`, {
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
