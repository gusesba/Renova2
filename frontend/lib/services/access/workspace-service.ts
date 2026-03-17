import { listMemberships } from "./membership-service";
import { listPermissions, listRoles } from "./role-service";
import type { AccessWorkspace } from "./contracts";
import { listUsers } from "./user-service";

type AccessWorkspaceScope = {
  includeMemberships?: boolean;
  includePermissions?: boolean;
  includeRoles?: boolean;
  includeUsers?: boolean;
};

// Monta o workspace administrativo do modulo de acesso.
export async function loadAccessWorkspace(
  token: string,
  scope: AccessWorkspaceScope,
): Promise<AccessWorkspace> {
  const {
    includeMemberships = false,
    includePermissions = false,
    includeRoles = false,
    includeUsers = false,
  } = scope;

  const [users, permissions, roles, memberships] = await Promise.all([
    includeUsers ? listUsers(token) : Promise.resolve([]),
    includePermissions ? listPermissions(token) : Promise.resolve([]),
    includeRoles ? listRoles(token) : Promise.resolve([]),
    includeMemberships ? listMemberships(token) : Promise.resolve([]),
  ]);

  return { users, permissions, roles, memberships };
}
