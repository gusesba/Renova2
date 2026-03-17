import type { SessionContext } from "@/lib/services/access";

// Centraliza os codigos de permissao usados pelo frontend para montar guardas visuais.
export const accessPermissionCodes = {
  usersView: "usuarios.visualizar",
  usersManage: "usuarios.gerenciar",
  rolesManage: "cargos.gerenciar",
  storesManage: "lojas.gerenciar",
  peopleView: "pessoas.visualizar",
  peopleManage: "pessoas.gerenciar",
  catalogManage: "catalogo.gerenciar",
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

// Libera o modulo de pessoas para quem possui leitura ou gestao no contexto da loja ativa.
export function canAccessPeopleModule(session: SessionContext) {
  return hasAnyPermission(session, [
    accessPermissionCodes.peopleView,
    accessPermissionCodes.peopleManage,
  ]);
}

// Libera o modulo de catalogos para quem possui a permissao especifica do cadastro base.
export function canAccessCatalogsModule(session: SessionContext) {
  return hasPermission(session, accessPermissionCodes.catalogManage);
}
