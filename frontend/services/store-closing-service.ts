import { buildStoreClosingQuery } from "@/lib/store-closing";

const apiBaseUrl = process.env.NEXT_PUBLIC_API_URL?.replace(/\/$/, "") ?? "http://localhost:5268";

export async function getStoreClosing(
  token: string,
  storeId: number,
  referenceMonth: string,
): Promise<{ body: unknown; ok: boolean; status: number }> {
  const response = await fetch(
    `${apiBaseUrl}/api/pagamento/fechamento?${buildStoreClosingQuery(storeId, referenceMonth)}`,
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
