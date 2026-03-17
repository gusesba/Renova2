import type { SessionContext } from "@/lib/services/access";

// Centraliza os codigos de permissao usados pelo frontend para montar guardas visuais.
export const accessPermissionCodes = {
  usersView: "usuarios.visualizar",
  usersManage: "usuarios.gerenciar",
  rolesManage: "cargos.gerenciar",
  storesManage: "lojas.gerenciar",
} as const;

// Verifica uma permissao unica dentro da sessao autenticada.
export function hasPermission(
  session: Pick<SessionContext, "permissoes">,
  permissionCode: string,
) {
  return session.permissoes.includes(permissionCode);
}

// Verifica se a sessao possui ao menos uma permissao da lista informada.
export function hasAnyPermission(
  session: Pick<SessionContext, "permissoes">,
  permissionCodes: readonly string[],
) {
  return permissionCodes.some((permissionCode) =>
    hasPermission(session, permissionCode),
  );
}

// Libera o modulo de acesso para quem ja tem alguma permissao administrativa ou ainda nao possui loja.
export function canAccessDashboardModule(session: SessionContext) {
  return (
    session.lojas.length === 0 ||
    hasAnyPermission(session, [
      accessPermissionCodes.usersView,
      accessPermissionCodes.usersManage,
      accessPermissionCodes.rolesManage,
    ])
  );
}

// Libera o modulo de lojas para o primeiro cadastro ou para quem gerencia lojas.
export function canAccessStoresModule(session: SessionContext) {
  return (
    session.lojas.length === 0 ||
    hasPermission(session, accessPermissionCodes.storesManage)
  );
}
