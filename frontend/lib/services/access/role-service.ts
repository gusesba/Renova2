import { callApi } from "@/lib/services/core/api-client";

import type { AccessPermission, AccessRole } from "./contracts";

// Reune as operacoes HTTP de cargos e permissoes.
export async function listPermissions(token: string) {
  return callApi<AccessPermission[]>(
    "/access/permissions",
    { method: "GET" },
    token,
  );
}

export async function listRoles(token: string) {
  return callApi<AccessRole[]>("/access/roles", { method: "GET" }, token);
}

export async function createRole(
  token: string,
  payload: { nome: string; descricao: string; permissaoIds: string[] },
) {
  return callApi<AccessRole>(
    "/access/roles",
    {
      method: "POST",
      body: JSON.stringify(payload),
    },
    token,
  );
}

export async function updateRole(
  token: string,
  cargoId: string,
  payload: { nome: string; descricao: string; ativo: boolean },
) {
  return callApi<AccessRole>(
    `/access/roles/${cargoId}`,
    {
      method: "PUT",
      body: JSON.stringify(payload),
    },
    token,
  );
}

export async function updateRolePermissions(
  token: string,
  cargoId: string,
  permissaoIds: string[],
) {
  return callApi<AccessRole>(
    `/access/roles/${cargoId}/permissions`,
    {
      method: "PUT",
      body: JSON.stringify({ permissaoIds }),
    },
    token,
  );
}
