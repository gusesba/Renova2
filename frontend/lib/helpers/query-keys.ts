// Chaves compartilhadas do React Query para evitar strings soltas pelo app.
export const queryKeys = {
  session: (token: string | null) => ["session", token] as const,
  accessWorkspace: (token: string, lojaAtivaId?: string | null) =>
    ["access-workspace", token, lojaAtivaId ?? null] as const,
};
