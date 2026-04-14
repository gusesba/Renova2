import { buildStoreExpenseQuery, type StoreExpenseFilters } from "@/lib/store-expense";

const apiBaseUrl = process.env.NEXT_PUBLIC_API_URL?.replace(/\/$/, "") ?? "http://localhost:5268";

export async function getStoreExpenses(
  token: string,
  storeId: number,
  filters: StoreExpenseFilters,
): Promise<{ body: unknown; ok: boolean; status: number }> {
  const query = buildStoreExpenseQuery(storeId, filters);
  const response = await fetch(`${apiBaseUrl}/api/gasto-loja?${query}`, {
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

export async function createStoreExpense(
  payload: {
    lojaId: number;
    natureza: number;
    valor: number;
    data: string;
    descricao?: string;
  },
  token: string,
): Promise<{ body: unknown; ok: boolean; status: number }> {
  const response = await fetch(`${apiBaseUrl}/api/gasto-loja`, {
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
