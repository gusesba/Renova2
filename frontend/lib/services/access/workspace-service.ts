import { listMemberships } from "./membership-service";
import { listPermissions, listRoles } from "./role-service";
import type { AccessWorkspace } from "./contracts";
import { listUsers } from "./user-service";

// Monta o workspace administrativo do modulo de acesso.
export async function loadAccessWorkspace(
  token: string,
): Promise<AccessWorkspace> {
  const [users, permissions, roles, memberships] = await Promise.all([
    listUsers(token),
    listPermissions(token),
    listRoles(token),
    listMemberships(token),
  ]);

  return { users, permissions, roles, memberships };
}
