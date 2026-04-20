import type { ClientUserOption } from "@/lib/client";
import { extractApiMessage, type Usuario } from "@/lib/auth";

const apiBaseUrl = process.env.NEXT_PUBLIC_API_URL?.replace(/\/$/, "") ?? "http://localhost:5268";

type LookupResponse<TItem> = {
  itens: TItem[];
};

type UserResponse = {
  id: number;
  nome: string;
  email: string;
};

type UpdateUserPayload = {
  nome: string;
};

export async function getUserOptions(
  token: string,
  search: string,
): Promise<{ body: ClientUserOption[]; ok: boolean; status: number }> {
  const params = new URLSearchParams({
    pagina: "1",
    tamanhoPagina: "5",
    ordenarPor: "nome",
    direcao: "asc",
  });

  if (search.trim()) {
    params.set("busca", search.trim());
  }

  const response = await fetch(`${apiBaseUrl}/api/usuario?${params.toString()}`, {
    method: "GET",
    headers: {
      Authorization: `Bearer ${token}`,
    },
  });

  const contentType = response.headers.get("content-type") ?? "";
  const body = contentType.includes("application/json")
    ? ((await response.json()) as unknown)
    : null;

  if (!response.ok) {
    return {
      body: [],
      ok: false,
      status: response.status,
    };
  }

  const data = body as LookupResponse<UserResponse>;

  return {
    body: data.itens.map((item) => ({
      id: item.id,
      nome: item.nome,
      email: item.email,
    })),
    ok: true,
    status: response.status,
  };
}

export async function updateUser(
  userId: number,
  payload: UpdateUserPayload,
  token: string,
): Promise<{ body: Usuario; message: string | null; ok: boolean; status: number }> {
  const response = await fetch(`${apiBaseUrl}/api/usuario/${userId}`, {
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
    body: body as Usuario,
    message: extractApiMessage(body),
    ok: response.ok,
    status: response.status,
  };
}
