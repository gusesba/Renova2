"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  startTransition,
  useDeferredValue,
  useEffect,
  useMemo,
  useState,
  type FormEvent,
  type SetStateAction,
} from "react";
import { toast } from "sonner";

import { SupplierLiquidationPanel } from "@/app/(system)/supplier-payments/components/supplier-liquidation-panel";
import { SupplierObligationsListPanel } from "@/app/(system)/supplier-payments/components/supplier-obligations-list-panel";
import { SupplierPaymentDetailPanel } from "@/app/(system)/supplier-payments/components/supplier-payment-detail-panel";
import { SupplierPaymentsOverview } from "@/app/(system)/supplier-payments/components/supplier-payments-overview";
import {
  createEmptySettlementLine,
  createSettlementFormFromDetail,
  emptySupplierPaymentFilters,
  type SupplierPaymentFiltersState,
  type SupplierSettlementFormState,
} from "@/app/(system)/supplier-payments/components/types";
import { useSystemSession } from "@/app/(system)/components/system-session-provider";
import { AccessStateCard } from "@/components/ui/access-state-card";
import {
  accessPermissionCodes,
  hasAnyPermission,
  hasPermission,
} from "@/lib/helpers/access-control";
import { getErrorMessage } from "@/lib/helpers/formatters";
import { queryKeys } from "@/lib/helpers/query-keys";
import { supplierSettlementSchema } from "@/lib/schemas/supplier-payments";
import {
  getSupplierObligationById,
  getSupplierPaymentsWorkspace,
  listSupplierObligations,
  settleSupplierObligation,
} from "@/lib/services/supplier-payments";

// Coordena o modulo 11 com pendencias, detalhe e liquidacao operacional.
export function SupplierPaymentsDashboard() {
  const { token, session } = useSystemSession();
  const queryClient = useQueryClient();
  const [filters, setFilters] =
    useState<SupplierPaymentFiltersState>(emptySupplierPaymentFilters);
  const deferredFilters = useDeferredValue(filters);
  const filtersKey = useMemo(() => JSON.stringify(deferredFilters), [deferredFilters]);
  const [selectedObligationId, setSelectedObligationId] = useState("");
  const [settlementDraft, setSettlementDraft] =
    useState<SupplierSettlementFormState | null>(null);
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
    queryFn: () => getSupplierPaymentsWorkspace(token),
    queryKey: queryKeys.supplierPaymentsWorkspace(token, session.lojaAtivaId),
  });

  const obligationsQuery = useQuery({
    enabled: Boolean(session.lojaAtivaId && canViewModule),
    queryFn: () =>
      listSupplierObligations(token, {
        pessoaId: deferredFilters.pessoaId || undefined,
        search: deferredFilters.search || undefined,
        statusObrigacao: deferredFilters.statusObrigacao || undefined,
        tipoObrigacao: deferredFilters.tipoObrigacao || undefined,
      }),
    queryKey: queryKeys.supplierPayments(token, session.lojaAtivaId, filtersKey),
  });

  const detailQuery = useQuery({
    enabled: Boolean(selectedObligationId && canViewModule),
    queryFn: () => getSupplierObligationById(token, selectedObligationId),
    queryKey: queryKeys.supplierPaymentDetail(
      token,
      session.lojaAtivaId,
      selectedObligationId,
    ),
  });

  const settleMutation = useMutation({
    mutationFn: async () => {
      if (!selectedObligationId) {
        throw new Error("Selecione a obrigacao a liquidar.");
      }

      const parsed = supplierSettlementSchema.safeParse(settlementForm);
      if (!parsed.success) {
        throw new Error(parsed.error.issues[0]?.message ?? "Liquidacao invalida.");
      }

      return settleSupplierObligation(token, selectedObligationId, {
        pagamentos: parsed.data.pagamentos.map((payment) => ({
          tipoLiquidacao: payment.tipoLiquidacao,
          meioPagamentoId: payment.meioPagamentoId || null,
          valor: payment.valor,
        })),
        comprovanteUrl: parsed.data.comprovanteUrl || null,
        observacoes: parsed.data.observacoes,
      });
    },
    onError: (error) => {
      toast.error(getErrorMessage(error));
    },
    onSuccess: async () => {
      toast.success("Liquidacao registrada com sucesso.");
      setSettlementDraft(
        createSettlementFormFromDetail(undefined, workspaceQuery.data),
      );
      await refreshModuleData();
    },
  });

  useEffect(() => {
    if (workspaceQuery.isError) {
      toast.error(getErrorMessage(workspaceQuery.error));
    }
  }, [workspaceQuery.error, workspaceQuery.isError]);

  useEffect(() => {
    if (obligationsQuery.isError) {
      toast.error(getErrorMessage(obligationsQuery.error));
    }
  }, [obligationsQuery.error, obligationsQuery.isError]);

  useEffect(() => {
    if (detailQuery.isError) {
      toast.error(getErrorMessage(detailQuery.error));
    }
  }, [detailQuery.error, detailQuery.isError]);

  useEffect(() => {
    const obligations = obligationsQuery.data ?? [];
    if (obligations.length === 0) {
      startTransition(() => {
        setSelectedObligationId("");
      });
      return;
    }

    if (
      !selectedObligationId ||
      !obligations.some((obligation) => obligation.id === selectedObligationId)
    ) {
      startTransition(() => {
        setSelectedObligationId(obligations[0]?.id ?? "");
      });
    }
  }, [obligationsQuery.data, selectedObligationId]);

  if (!canViewModule) {
    return (
      <AccessStateCard
        message="Solicite permissao financeira para consultar pagamentos e repasses."
        subtitle="Sua conta nao possui acesso ao modulo de obrigacoes do fornecedor."
        title="Modulo sem permissao"
      />
    );
  }

  const busy =
    workspaceQuery.isLoading ||
    obligationsQuery.isLoading ||
    detailQuery.isLoading ||
    settleMutation.isPending;
  const settlementForm =
    settlementDraft ??
    createSettlementFormFromDetail(detailQuery.data, workspaceQuery.data);

  function setForm(value: SetStateAction<SupplierSettlementFormState>) {
    setSettlementDraft((current) => {
      const baseValue = current ?? settlementForm;
      return typeof value === "function"
        ? (value as (current: SupplierSettlementFormState) => SupplierSettlementFormState)(
            baseValue,
          )
        : value;
    });
  }

  async function refreshModuleData() {
    await Promise.all([
      queryClient.invalidateQueries({
        queryKey: queryKeys.supplierPaymentsWorkspace(token, session.lojaAtivaId),
      }),
      queryClient.invalidateQueries({
        queryKey: queryKeys.supplierPayments(token, session.lojaAtivaId, filtersKey),
      }),
      selectedObligationId
        ? queryClient.invalidateQueries({
            queryKey: queryKeys.supplierPaymentDetail(
              token,
              session.lojaAtivaId,
              selectedObligationId,
            ),
          })
        : Promise.resolve(),
    ]);
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    await settleMutation.mutateAsync();
  }

  return (
    <div className="dashboard-grid">
      <div className="dashboard-column" style={{ gridColumn: "1 / -1" }}>
        <SupplierPaymentsOverview obligations={obligationsQuery.data ?? []} />
      </div>

      <div className="dashboard-column">
        <SupplierObligationsListPanel
          busy={busy}
          filters={filters}
          obligations={obligationsQuery.data ?? []}
          onSelectObligation={setSelectedObligationId}
          selectedObligationId={selectedObligationId}
          setFilters={setFilters}
          statuses={workspaceQuery.data?.statusObrigacao ?? []}
          suppliers={workspaceQuery.data?.fornecedores ?? []}
          types={workspaceQuery.data?.tiposObrigacao ?? []}
        />
      </div>

      <div className="dashboard-column">
        <SupplierPaymentDetailPanel detail={detailQuery.data} />
        <SupplierLiquidationPanel
          busy={busy}
          canManage={canManageModule}
          detail={detailQuery.data}
          form={settlementForm}
          onAddLine={() =>
            setForm((current) => ({
              ...current,
              pagamentos: [
                ...current.pagamentos,
                createEmptySettlementLine(workspaceQuery.data),
              ],
            }))
          }
          onRemoveLine={(index) =>
            setForm((current) => ({
              ...current,
              pagamentos: current.pagamentos.filter((_, itemIndex) => itemIndex !== index),
            }))
          }
          onSubmit={handleSubmit}
          setForm={setForm}
          workspace={workspaceQuery.data}
        />
      </div>
    </div>
  );
}
