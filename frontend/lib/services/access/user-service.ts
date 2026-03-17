import { callApi } from "@/lib/services/core/api-client";

import type { AccessUser } from "./contracts";

// Reune as operacoes HTTP ligadas a usuarios.
export async function listUsers(token: string) {
  return callApi<AccessUser[]>("/access/users", { method: "GET" }, token);
}

export async function createUser(
  token: string,
  payload: {
    nome: string;
    email: string;
    telefone: string;
    senha: string;
    pessoaId: null;
  },
) {
  return callApi<AccessUser>(
    "/access/users",
    {
      method: "POST",
      body: JSON.stringify(payload),
    },
    token,
  );
}

export async function updateUser(
  token: string,
  usuarioId: string,
  payload: {
    nome: string;
    email: string;
    telefone: string;
    pessoaId: null;
  },
) {
  return callApi<AccessUser>(
    `/access/users/${usuarioId}`,
    {
      method: "PUT",
      body: JSON.stringify(payload),
    },
    token,
  );
}

export async function changeUserStatus(
  token: string,
  usuarioId: string,
  statusUsuario: string,
) {
  return callApi<AccessUser>(
    `/access/users/${usuarioId}/status`,
    {
      method: "POST",
      body: JSON.stringify({ statusUsuario }),
    },
    token,
  );
}
