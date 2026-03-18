"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  useDeferredValue,
  useEffect,
  useState,
  type FormEvent,
  type SetStateAction,
} from "react";
import { toast } from "sonner";

import { useSystemSession } from "@/app/(system)/components/system-session-provider";
import { FinancialLedgerPanel } from "@/app/(system)/financial/components/financial-ledger-panel";
import { FinancialManualEntryPanel } from "@/app/(system)/financial/components/financial-manual-entry-panel";
import { FinancialOverview } from "@/app/(system)/financial/components/financial-overview";
import { FinancialReconciliationPanel } from "@/app/(system)/financial/components/financial-reconciliation-panel";
import {
  createFinancialEntryForm,
  emptyFinancialFilters,
  type FinancialEntryFormState,
  type FinancialFiltersState,
} from "@/app/(system)/financial/components/types";
import { AccessStateCard } from "@/components/ui/access-state-card";
import {
  accessPermissionCodes,
  hasAnyPermission,
  hasPermission,
} from "@/lib/helpers/access-control";
import { getErrorMessage } from "@/lib/helpers/formatters";
import { queryKeys } from "@/lib/helpers/query-keys";
import { financialEntrySchema } from "@/lib/schemas/financial";
import {
  getFinancialReconciliation,
  getFinancialWorkspace,
  listFinancialEntries,
  registerFinancialEntry,
} from "@/lib/services/financial";

// Coordena o modulo 12 com livro razao, resumo e lancamentos manuais.
export function FinancialDashboard() {
  const { token, session } = useSystemSession();
  const queryClient = useQueryClient();
  const [filters, setFilters] =
    useState<FinancialFiltersState>(emptyFinancialFilters);
  const [entryDraft, setEntryDraft] = useState<FinancialEntryFormState | null>(null);
  const deferredFilters = useDeferredValue(filters);
  const filtersKey = JSON.stringify(deferredFilters);
  const canViewModule = hasAnyPermission(session, [
    accessPermissionCodes.financeView,
    accessPermissionCodes.financeManage,
  ]);
  const canManageModule = hasPermission(
    session,
    accessPermissionCodes.financeManage,
  );

  const workspaceQuery = useQuery({
    enabled: Boolean(session.lojaAtivaId && canViewModule),
    queryFn: () => getFinancialWorkspace(token),
    queryKey: queryKeys.financialWorkspace(token, session.lojaAtivaId),
  });

  const entriesQuery = useQuery({
    enabled: Boolean(session.lojaAtivaId && canViewModule),
    queryFn: () =>
      listFinancialEntries(token, {
        search: deferredFilters.search || undefined,
        meioPagamentoId: deferredFilters.meioPagamentoId || undefined,
        tipoMovimentacao: deferredFilters.tipoMovimentacao || undefined,
        direcao: deferredFilters.direcao || undefined,
        dataInicial: deferredFilters.dataInicial || undefined,
        dataFinal: deferredFilters.dataFinal || undefined,
      }),
    queryKey: queryKeys.financialLedger(token, session.lojaAtivaId, filtersKey),
  });

  const reconciliationQuery = useQuery({
    enabled: Boolean(session.lojaAtivaId && canViewModule),
    queryFn: () =>
      getFinancialReconciliation(token, {
        search: deferredFilters.search || undefined,
        meioPagamentoId: deferredFilters.meioPagamentoId || undefined,
        tipoMovimentacao: deferredFilters.tipoMovimentacao || undefined,
        direcao: deferredFilters.direcao || undefined,
        dataInicial: deferredFilters.dataInicial || undefined,
        dataFinal: deferredFilters.dataFinal || undefined,
      }),
    queryKey: queryKeys.financialReconciliation(
      token,
      session.lojaAtivaId,
      filtersKey,
    ),
  });

  const createEntryMutation = useMutation({
    mutationFn: async () => {
      const parsed = financialEntrySchema.safeParse(entryForm);
      if (!parsed.success) {
        throw new Error(parsed.error.issues[0]?.message ?? "Lancamento invalido.");
      }

      return registerFinancialEntry(token, {
        tipoMovimentacao: parsed.data.tipoMovimentacao,
        direcao: parsed.data.direcao,
        meioPagamentoId: parsed.data.meioPagamentoId || null,
        valorBruto: parsed.data.valorBruto,
        taxa: parsed.data.taxa,
        descricao: parsed.data.descricao,
        competenciaEm: parsed.data.competenciaEm || null,
        movimentadoEm: parsed.data.movimentadoEm || null,
      });
    },
    onError: (error) => {
      toast.error(getErrorMessage(error));
    },
    onSuccess: async () => {
      toast.success("Lancamento financeiro registrado com sucesso.");
      setEntryDraft(createFinancialEntryForm(workspaceQuery.data));
      await refreshModuleData();
    },
  });

  useEffect(() => {
    if (workspaceQuery.isError) {
      toast.error(getErrorMessage(workspaceQuery.error));
    }
  }, [workspaceQuery.error, workspaceQuery.isError]);

  useEffect(() => {
    if (entriesQuery.isError) {
      toast.error(getErrorMessage(entriesQuery.error));
    }
  }, [entriesQuery.error, entriesQuery.isError]);

  useEffect(() => {
    if (reconciliationQuery.isError) {
      toast.error(getErrorMessage(reconciliationQuery.error));
    }
  }, [reconciliationQuery.error, reconciliationQuery.isError]);

  if (!canViewModule) {
    return (
      <AccessStateCard
        message="Solicite permissao financeira para consultar o livro razao da loja."
        subtitle="Sua conta nao possui acesso ao modulo de meios de pagamento e conciliacao financeira."
        title="Modulo sem permissao"
      />
    );
  }

  const busy =
    workspaceQuery.isLoading ||
    entriesQuery.isLoading ||
    reconciliationQuery.isLoading ||
    createEntryMutation.isPending;
  const entryForm = entryDraft ?? createFinancialEntryForm(workspaceQuery.data);

  function setForm(value: SetStateAction<FinancialEntryFormState>) {
    setEntryDraft((current) => {
      const baseValue = current ?? entryForm;
      return typeof value === "function"
        ? (value as (current: FinancialEntryFormState) => FinancialEntryFormState)(
            baseValue,
          )
        : value;
    });
  }

  async function refreshModuleData() {
    await Promise.all([
      queryClient.invalidateQueries({
        queryKey: queryKeys.financialWorkspace(token, session.lojaAtivaId),
      }),
      queryClient.invalidateQueries({
        queryKey: queryKeys.financialLedger(token, session.lojaAtivaId, filtersKey),
      }),
      queryClient.invalidateQueries({
        queryKey: queryKeys.financialReconciliation(
          token,
          session.lojaAtivaId,
          filtersKey,
        ),
      }),
    ]);
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    await createEntryMutation.mutateAsync();
  }

  return (
    <div className="dashboard-grid">
      <div className="dashboard-column" style={{ gridColumn: "1 / -1" }}>
        <FinancialOverview
          reconciliation={reconciliationQuery.data}
          workspace={workspaceQuery.data}
        />
      </div>

      <div className="dashboard-column">
        <FinancialLedgerPanel
          entries={entriesQuery.data ?? []}
          filters={filters}
          setFilters={setFilters}
          workspace={workspaceQuery.data}
        />
      </div>

      <div className="dashboard-column">
        <FinancialManualEntryPanel
          busy={busy}
          canManage={canManageModule}
          form={entryForm}
          onSubmit={handleSubmit}
          setForm={setForm}
          workspace={workspaceQuery.data}
        />
        <FinancialReconciliationPanel reconciliation={reconciliationQuery.data} />
      </div>
    </div>
  );
}
