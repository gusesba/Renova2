import { callApi } from "@/lib/services/core/api-client";

import type { StoreMembership } from "./contracts";

// Reune as operacoes HTTP dos vinculos de usuario com loja.
export async function listMemberships(token: string) {
  return callApi<StoreMembership[]>(
    "/access/store-memberships",
    { method: "GET" },
    token,
  );
}

export async function createMembership(
  token: string,
  payload: {
    usuarioId: string;
    statusVinculo: string;
    ehResponsavel: boolean;
    dataFim: null;
    cargoIds: string[];
  },
) {
  return callApi<StoreMembership>(
    "/access/store-memberships",
    {
      method: "POST",
      body: JSON.stringify(payload),
    },
    token,
  );
}

export async function updateMembershipRoles(
  token: string,
  usuarioLojaId: string,
  cargoIds: string[],
) {
  return callApi<StoreMembership>(
    `/access/store-memberships/${usuarioLojaId}/roles`,
    {
      method: "PUT",
      body: JSON.stringify({ cargoIds }),
    },
    token,
  );
}
