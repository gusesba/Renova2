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
  consignmentsWorkspace: (token: string, lojaAtivaId?: string | null) =>
    ["consignments-workspace", token, lojaAtivaId ?? null] as const,
  consignments: (
    token: string,
    lojaAtivaId: string | null | undefined,
    filters: string,
  ) => ["consignments", token, lojaAtivaId ?? null, filters] as const,
  consignmentDetail: (
    token: string,
    lojaAtivaId: string | null | undefined,
    pecaId: string | null,
  ) => ["consignment-detail", token, lojaAtivaId ?? null, pecaId ?? null] as const,
  piecesWorkspace: (token: string, lojaAtivaId?: string | null) =>
    ["pieces-workspace", token, lojaAtivaId ?? null] as const,
  pieces: (
    token: string,
    lojaAtivaId: string | null | undefined,
    filters: string,
  ) => ["pieces", token, lojaAtivaId ?? null, filters] as const,
  pieceDetail: (
    token: string,
    lojaAtivaId: string | null | undefined,
    pecaId: string | null,
  ) => ["piece-detail", token, lojaAtivaId ?? null, pecaId ?? null] as const,
  stockMovementsWorkspace: (token: string, lojaAtivaId?: string | null) =>
    ["stock-movements-workspace", token, lojaAtivaId ?? null] as const,
  stockMovements: (
    token: string,
    lojaAtivaId: string | null | undefined,
    filters: string,
  ) => ["stock-movements", token, lojaAtivaId ?? null, filters] as const,
  stockMovementPieces: (
    token: string,
    lojaAtivaId: string | null | undefined,
    filters: string,
  ) => ["stock-movement-pieces", token, lojaAtivaId ?? null, filters] as const,
  salesWorkspace: (token: string, lojaAtivaId?: string | null) =>
    ["sales-workspace", token, lojaAtivaId ?? null] as const,
  creditsWorkspace: (token: string, lojaAtivaId?: string | null) =>
    ["credits-workspace", token, lojaAtivaId ?? null] as const,
  financialWorkspace: (token: string, lojaAtivaId?: string | null) =>
    ["financial-workspace", token, lojaAtivaId ?? null] as const,
  closingsWorkspace: (token: string, lojaAtivaId?: string | null) =>
    ["closings-workspace", token, lojaAtivaId ?? null] as const,
  supplierPaymentsWorkspace: (token: string, lojaAtivaId?: string | null) =>
    ["supplier-payments-workspace", token, lojaAtivaId ?? null] as const,
  sales: (
    token: string,
    lojaAtivaId: string | null | undefined,
    filters: string,
  ) => ["sales", token, lojaAtivaId ?? null, filters] as const,
  saleDetail: (
    token: string,
    lojaAtivaId: string | null | undefined,
    saleId: string | null,
  ) => ["sale-detail", token, lojaAtivaId ?? null, saleId ?? null] as const,
  creditDetail: (
    token: string,
    lojaAtivaId: string | null | undefined,
    pessoaId: string | null,
  ) => ["credit-detail", token, lojaAtivaId ?? null, pessoaId ?? null] as const,
  financialLedger: (
    token: string,
    lojaAtivaId: string | null | undefined,
    filters: string,
  ) => ["financial-ledger", token, lojaAtivaId ?? null, filters] as const,
  financialReconciliation: (
    token: string,
    lojaAtivaId: string | null | undefined,
    filters: string,
  ) => ["financial-reconciliation", token, lojaAtivaId ?? null, filters] as const,
  closings: (
    token: string,
    lojaAtivaId: string | null | undefined,
    filters: string,
  ) => ["closings", token, lojaAtivaId ?? null, filters] as const,
  closingDetail: (
    token: string,
    lojaAtivaId: string | null | undefined,
    closingId: string | null,
  ) => ["closing-detail", token, lojaAtivaId ?? null, closingId ?? null] as const,
  supplierPayments: (
    token: string,
    lojaAtivaId: string | null | undefined,
    filters: string,
  ) => ["supplier-payments", token, lojaAtivaId ?? null, filters] as const,
  supplierPaymentDetail: (
    token: string,
    lojaAtivaId: string | null | undefined,
    obligationId: string | null,
  ) => ["supplier-payment-detail", token, lojaAtivaId ?? null, obligationId ?? null] as const,
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
