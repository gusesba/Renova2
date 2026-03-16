import type { AccessPermission } from "@/lib/services/renova-api";

export function groupPermissionsByModule(permissions: AccessPermission[]) {
  const groups = new Map<string, AccessPermission[]>();

  for (const permission of permissions) {
    const current = groups.get(permission.modulo) ?? [];
    current.push(permission);
    groups.set(permission.modulo, current);
  }

  return Array.from(groups.entries())
    .sort(([left], [right]) => left.localeCompare(right, "pt-BR"))
    .map(([modulo, items]) => ({
      modulo,
      items: items.sort((left, right) => left.nome.localeCompare(right.nome, "pt-BR")),
    }));
}
