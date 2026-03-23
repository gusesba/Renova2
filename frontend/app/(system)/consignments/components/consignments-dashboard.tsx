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

import { useSystemSession } from "@/app/(system)/components/system-session-provider";
import { ConsignmentActionsPanel } from "@/app/(system)/consignments/components/consignment-actions-panel";
import { ConsignmentDetailPanel } from "@/app/(system)/consignments/components/consignment-detail-panel";
import { ConsignmentsListPanel } from "@/app/(system)/consignments/components/consignments-list-panel";
import { ConsignmentsOverview } from "@/app/(system)/consignments/components/consignments-overview";
import {
  createEmptyConsignmentCloseForm,
  emptyConsignmentFilters,
  type ConsignmentCloseFormState,
  type ConsignmentFiltersState,
} from "@/app/(system)/consignments/components/types";
import { AccessStateCard } from "@/components/ui/access-state-card";
import {
  accessPermissionCodes,
  hasAnyPermission,
  hasPermission,
} from "@/lib/helpers/access-control";
import { getErrorMessage } from "@/lib/helpers/formatters";
import { queryKeys } from "@/lib/helpers/query-keys";
import { getZodErrorMessage } from "@/lib/schemas/access";
import { consignmentCloseFormSchema } from "@/lib/schemas/consignments";
import {
  closeConsignment,
  getConsignmentById,
  getConsignmentsWorkspace,
  listConsignments,
} from "@/lib/services/consignments";

// Coordena a pagina do modulo 07 com resumo, filtros, detalhe e acoes.
export function ConsignmentsDashboard() {
  const { token, session } = useSystemSession();
  const queryClient = useQueryClient();
  const [filters, setFilters] = useState<ConsignmentFiltersState>(
    emptyConsignmentFilters,
  );
  const deferredFilters = useDeferredValue(filters);
  const [selectedPieceId, setSelectedPieceId] = useState("");
  const [closeDraft, setCloseDraft] = useState<ConsignmentCloseFormState | null>(
    null,
  );
  const [receiptText, setReceiptText] = useState("");
  const canViewConsignments = hasAnyPermission(session, [
    accessPermissionCodes.piecesView,
    accessPermissionCodes.piecesCreate,
    accessPermissionCodes.piecesAdjust,
  ]);
  const canManageConsignments = hasPermission(
    session,
    accessPermissionCodes.piecesAdjust,
  );

  const workspaceQuery = useQuery({
    enabled: Boolean(session.lojaAtivaId && canViewConsignments),
    queryFn: () => getConsignmentsWorkspace(token),
    queryKey: queryKeys.consignmentsWorkspace(token, session.lojaAtivaId),
  });

  const filtersKey = useMemo(
    () => JSON.stringify(deferredFilters),
    [deferredFilters],
  );

  const listQuery = useQuery({
    enabled: Boolean(session.lojaAtivaId && canViewConsignments),
    queryFn: () => listConsignments(token, deferredFilters),
    queryKey: queryKeys.consignments(token, session.lojaAtivaId, filtersKey),
  });

  const detailQuery = useQuery({
    enabled: Boolean(selectedPieceId && canViewConsignments),
    queryFn: () => getConsignmentById(token, selectedPieceId),
    queryKey: queryKeys.consignmentDetail(token, session.lojaAtivaId, selectedPieceId),
  });

  const closeMutation = useMutation({
    mutationFn: async () => {
      if (!selectedPieceId) {
        throw new Error("Selecione uma peca para encerrar a consignacao.");
      }

      const parsed = consignmentCloseFormSchema.safeParse(closeForm);
      if (!parsed.success) {
        throw new Error(getZodErrorMessage(parsed.error));
      }

      return closeConsignment(token, selectedPieceId, parsed.data);
    },
    onError: (error) => {
      toast.error(getErrorMessage(error));
    },
    onSuccess: async (response) => {
      setReceiptText(response.comprovanteTexto);
      toast.success("Consignacao encerrada com sucesso.");
      await refreshModuleData();
    },
  });

  useEffect(() => {
    if (workspaceQuery.isError) {
      toast.error(getErrorMessage(workspaceQuery.error));
    }
  }, [workspaceQuery.error, workspaceQuery.isError]);

  useEffect(() => {
    if (listQuery.isError) {
      toast.error(getErrorMessage(listQuery.error));
    }
  }, [listQuery.error, listQuery.isError]);

  useEffect(() => {
    if (detailQuery.isError) {
      toast.error(getErrorMessage(detailQuery.error));
    }
  }, [detailQuery.error, detailQuery.isError]);

  useEffect(() => {
    const items = listQuery.data ?? [];
    if (items.length === 0) {
      startTransition(() => {
        setSelectedPieceId("");
      });
      return;
    }

    if (!selectedPieceId || !items.some((item) => item.id === selectedPieceId)) {
      startTransition(() => {
        setSelectedPieceId(items[0]?.id ?? "");
      });
    }
  }, [listQuery.data, selectedPieceId]);

  const closeForm =
    closeDraft ??
    createEmptyConsignmentCloseForm(
      workspaceQuery.data?.acoesEncerramento[0]?.codigo ?? "",
    );
  const busy =
    workspaceQuery.isLoading ||
    listQuery.isLoading ||
    detailQuery.isLoading ||
    closeMutation.isPending;

  if (!canViewConsignments) {
    return (
      <AccessStateCard
        message="Solicite permissao de visualizacao ou ajuste de pecas para consultar este modulo."
        subtitle="Sua conta nao possui acesso ao ciclo de vida da consignacao."
        title="Modulo sem permissao"
      />
    );
  }

  async function refreshModuleData() {
    await Promise.all([
      queryClient.invalidateQueries({
        queryKey: queryKeys.consignmentsWorkspace(token, session.lojaAtivaId),
      }),
      queryClient.invalidateQueries({
        queryKey: queryKeys.consignments(token, session.lojaAtivaId, filtersKey),
      }),
      queryClient.invalidateQueries({
        queryKey: queryKeys.consignmentDetail(token, session.lojaAtivaId, selectedPieceId),
      }),
    ]);
  }

  function setCloseForm(value: SetStateAction<ConsignmentCloseFormState>) {
    setCloseDraft((current) => {
      const baseValue = current ?? closeForm;
      return typeof value === "function"
        ? (value as (current: ConsignmentCloseFormState) => ConsignmentCloseFormState)(
            baseValue,
          )
        : value;
    });
  }

  async function handleCloseSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    await closeMutation.mutateAsync();
  }

  return (
    <div className="dashboard-grid">
      <div className="dashboard-column" style={{ gridColumn: "1 / -1" }}>
        <ConsignmentsOverview workspace={workspaceQuery.data} />
      </div>

      <div className="dashboard-column">
        <ConsignmentsListPanel
          busy={busy}
          filters={filters}
          items={listQuery.data ?? []}
          onSelectPiece={(pieceId) => {
            setSelectedPieceId(pieceId);
            setReceiptText("");
          }}
          selectedPieceId={selectedPieceId}
          setFilters={setFilters}
          statuses={workspaceQuery.data?.statuses ?? []}
          suppliers={workspaceQuery.data?.fornecedores ?? []}
        />
      </div>

      <div className="dashboard-column">
        <ConsignmentDetailPanel detail={detailQuery.data} />
        <ConsignmentActionsPanel
          actions={workspaceQuery.data?.acoesEncerramento ?? []}
          busy={busy}
          canManage={canManageConsignments}
          closeForm={closeForm}
          detail={detailQuery.data}
          onClose={handleCloseSubmit}
          receiptText={receiptText}
          setCloseForm={setCloseForm}
        />
      </div>
    </div>
  );
}
