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
  rulesManage: "regras.gerenciar",
  piecesView: "pecas.visualizar",
  piecesCreate: "pecas.cadastrar",
  piecesAdjust: "pecas.ajustar",
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

// Libera o modulo de regras comerciais para quem possui a permissao especifica.
export function canAccessCommercialRulesModule(session: SessionContext) {
  return hasPermission(session, accessPermissionCodes.rulesManage);
}

// Libera o modulo de pecas para quem possui visualizacao ou cadastro.
export function canAccessPiecesModule(session: SessionContext) {
  return hasAnyPermission(session, [
    accessPermissionCodes.piecesView,
    accessPermissionCodes.piecesCreate,
    accessPermissionCodes.piecesAdjust,
  ]);
}

// Libera o modulo de consignacao para quem consulta ou ajusta pecas.
export function canAccessConsignmentsModule(session: SessionContext) {
  return hasAnyPermission(session, [
    accessPermissionCodes.piecesView,
    accessPermissionCodes.piecesCreate,
    accessPermissionCodes.piecesAdjust,
  ]);
}

// Libera o modulo de movimentacoes para quem consulta ou ajusta pecas.
export function canAccessStockMovementsModule(session: SessionContext) {
  return hasAnyPermission(session, [
    accessPermissionCodes.piecesView,
    accessPermissionCodes.piecesCreate,
    accessPermissionCodes.piecesAdjust,
  ]);
}
