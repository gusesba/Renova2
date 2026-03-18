// Chaves compartilhadas do React Query para evitar strings soltas pelo app.
export const queryKeys = {
  session: (token: string | null) => ["session", token] as const,
  accessWorkspace: (token: string, lojaAtivaId?: string | null) =>
    ["access-workspace", token, lojaAtivaId ?? null] as const,
  accessibleStores: (token: string) => ["accessible-stores", token] as const,
  catalogsWorkspace: (token: string, lojaAtivaId?: string | null) =>
    ["catalogs-workspace", token, lojaAtivaId ?? null] as const,
  commercialRulesWorkspace: (token: string, lojaAtivaId?: string | null) =>
    ["commercial-rules-workspace", token, lojaAtivaId ?? null] as const,
  people: (token: string, lojaAtivaId?: string | null) =>
    ["people", token, lojaAtivaId ?? null] as const,
  personDetail: (
    token: string,
    lojaAtivaId: string | null | undefined,
    pessoaId: string | null,
  ) => ["people-detail", token, lojaAtivaId ?? null, pessoaId ?? null] as const,
  peopleUsers: (token: string, lojaAtivaId?: string | null) =>
    ["people-users", token, lojaAtivaId ?? null] as const,
};
