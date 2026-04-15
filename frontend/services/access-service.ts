const apiBaseUrl = process.env.NEXT_PUBLIC_API_URL?.replace(/\/$/, "") ?? "http://localhost:5268";

async function parseResponseBody(response: Response) {
  const contentType = response.headers.get("content-type") ?? "";
  return contentType.includes("application/json") ? ((await response.json()) as unknown) : null;
}

export async function getEmployees(
  storeId: number,
  token: string,
): Promise<{ body: unknown; ok: boolean; status: number }> {
  const response = await fetch(`${apiBaseUrl}/api/funcionario?lojaId=${storeId}`, {
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

export async function createEmployee(
  storeId: number,
  payload: { usuarioId: number },
  token: string,
): Promise<{ body: unknown; ok: boolean; status: number }> {
  const response = await fetch(`${apiBaseUrl}/api/funcionario?lojaId=${storeId}`, {
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

export async function deleteEmployee(
  storeId: number,
  userId: number,
  token: string,
): Promise<{ body: unknown; ok: boolean; status: number }> {
  const response = await fetch(`${apiBaseUrl}/api/funcionario?lojaId=${storeId}&usuarioId=${userId}`, {
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
