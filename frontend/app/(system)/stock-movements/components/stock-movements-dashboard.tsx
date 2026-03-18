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
import { StockAdjustmentPanel } from "@/app/(system)/stock-movements/components/stock-adjustment-panel";
import { StockMovementsListPanel } from "@/app/(system)/stock-movements/components/stock-movements-list-panel";
import { StockMovementsOverview } from "@/app/(system)/stock-movements/components/stock-movements-overview";
import { StockPieceSearchPanel } from "@/app/(system)/stock-movements/components/stock-piece-search-panel";
import {
  emptyStockAdjustmentForm,
  emptyStockMovementFilters,
  emptyStockPieceSearchFilters,
  mapPieceToAdjustmentForm,
  type StockAdjustmentFormState,
  type StockMovementFiltersState,
  type StockPieceSearchFiltersState,
} from "@/app/(system)/stock-movements/components/types";
import { AccessStateCard } from "@/components/ui/access-state-card";
import {
  accessPermissionCodes,
  hasAnyPermission,
  hasPermission,
} from "@/lib/helpers/access-control";
import { getErrorMessage } from "@/lib/helpers/formatters";
import { queryKeys } from "@/lib/helpers/query-keys";
import { getZodErrorMessage } from "@/lib/schemas/access";
import { adjustStockSchema } from "@/lib/schemas/stock-movements";
import {
  adjustStock,
  getStockMovementsWorkspace,
  listStockMovements,
  searchStockPieces,
} from "@/lib/services/stock-movements";

// Converte a data do input para o inicio do dia em UTC.
function toStartOfDayIso(value: string) {
  return value ? `${value}T00:00:00Z` : "";
}

// Converte a data do input para o fim do dia em UTC.
function toEndOfDayIso(value: string) {
  return value ? `${value}T23:59:59Z` : "";
}

// Coordena o modulo 08 com listagem, busca operacional e ajustes manuais.
export function StockMovementsDashboard() {
  const { token, session } = useSystemSession();
  const queryClient = useQueryClient();
  const [movementFilters, setMovementFilters] =
    useState<StockMovementFiltersState>(emptyStockMovementFilters);
  const [pieceFilters, setPieceFilters] = useState<StockPieceSearchFiltersState>(
    emptyStockPieceSearchFilters,
  );
  const deferredMovementFilters = useDeferredValue(movementFilters);
  const deferredPieceFilters = useDeferredValue(pieceFilters);
  const [selectedPieceId, setSelectedPieceId] = useState("");
  const [draftForm, setDraftForm] = useState<StockAdjustmentFormState | null>(
    null,
  );
  const canViewModule = hasAnyPermission(session, [
    accessPermissionCodes.piecesView,
    accessPermissionCodes.piecesCreate,
    accessPermissionCodes.piecesAdjust,
  ]);
  const canAdjustStock = hasPermission(
    session,
    accessPermissionCodes.piecesAdjust,
  );

  const workspaceQuery = useQuery({
    enabled: Boolean(session.lojaAtivaId && canViewModule),
    queryFn: () => getStockMovementsWorkspace(token),
    queryKey: queryKeys.stockMovementsWorkspace(token, session.lojaAtivaId),
  });

  const movementFiltersKey = useMemo(
    () => JSON.stringify(deferredMovementFilters),
    [deferredMovementFilters],
  );

  const pieceFiltersKey = useMemo(
    () => JSON.stringify(deferredPieceFilters),
    [deferredPieceFilters],
  );

  const movementsQuery = useQuery({
    enabled: Boolean(session.lojaAtivaId && canViewModule),
    queryFn: () =>
      listStockMovements(token, {
        dataFinal: toEndOfDayIso(deferredMovementFilters.dataFinal),
        dataInicial: toStartOfDayIso(deferredMovementFilters.dataInicial),
        fornecedorPessoaId: deferredMovementFilters.fornecedorPessoaId || undefined,
        pecaId: deferredMovementFilters.pecaId || undefined,
        search: deferredMovementFilters.search || undefined,
        statusPeca: deferredMovementFilters.statusPeca || undefined,
        tipoMovimentacao:
          deferredMovementFilters.tipoMovimentacao || undefined,
      }),
    queryKey: queryKeys.stockMovements(
      token,
      session.lojaAtivaId,
      movementFiltersKey,
    ),
  });

  const piecesQuery = useQuery({
    enabled: Boolean(session.lojaAtivaId && canViewModule),
    queryFn: () =>
      searchStockPieces(token, {
        codigoBarras: deferredPieceFilters.codigoBarras || undefined,
        fornecedorPessoaId: deferredPieceFilters.fornecedorPessoaId || undefined,
        search: deferredPieceFilters.search || undefined,
        statusPeca: deferredPieceFilters.statusPeca || undefined,
        tempoMinimoLojaDias:
          deferredPieceFilters.tempoMinimoLojaDias || undefined,
      }),
    queryKey: queryKeys.stockMovementPieces(
      token,
      session.lojaAtivaId,
      pieceFiltersKey,
    ),
  });

  const adjustMutation = useMutation({
    mutationFn: async () => {
      const parsed = adjustStockSchema.safeParse(form);
      if (!parsed.success) {
        throw new Error(getZodErrorMessage(parsed.error));
      }

      return adjustStock(token, {
        motivo: parsed.data.motivo,
        pecaId: parsed.data.pecaId,
        quantidadeNova: parsed.data.quantidadeNova,
        statusPeca: parsed.data.statusPeca?.trim() || null,
      });
    },
    onError: (error) => {
      toast.error(getErrorMessage(error));
    },
    onSuccess: async (response) => {
      toast.success("Ajuste manual registrado com sucesso.");
      setDraftForm((current) =>
        current
          ? {
              ...current,
              motivo: "",
              quantidadeNova: String(response.quantidadeNova),
              statusPeca: response.statusNovo,
            }
          : current,
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
    if (movementsQuery.isError) {
      toast.error(getErrorMessage(movementsQuery.error));
    }
  }, [movementsQuery.error, movementsQuery.isError]);

  useEffect(() => {
    if (piecesQuery.isError) {
      toast.error(getErrorMessage(piecesQuery.error));
    }
  }, [piecesQuery.error, piecesQuery.isError]);

  useEffect(() => {
    const items = piecesQuery.data ?? [];
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
  }, [piecesQuery.data, selectedPieceId]);

  if (!canViewModule) {
    return (
      <AccessStateCard
        message="Solicite permissao de visualizacao, cadastro ou ajuste de pecas para acessar este modulo."
        subtitle="Sua conta nao possui acesso ao historico e aos ajustes de estoque."
        title="Modulo sem permissao"
      />
    );
  }

  const busy =
    workspaceQuery.isLoading ||
    movementsQuery.isLoading ||
    piecesQuery.isLoading ||
    adjustMutation.isPending;
  const selectedPiece = (piecesQuery.data ?? []).find(
    (piece) => piece.id === selectedPieceId,
  );
  const form =
    draftForm && selectedPiece && draftForm.pecaId === selectedPiece.id
      ? draftForm
      : selectedPiece
        ? mapPieceToAdjustmentForm(selectedPiece)
        : emptyStockAdjustmentForm();

  function setForm(value: SetStateAction<StockAdjustmentFormState>) {
    setDraftForm((current) => {
      const baseValue = current ?? form;
      return typeof value === "function"
        ? (value as (current: StockAdjustmentFormState) => StockAdjustmentFormState)(
            baseValue,
          )
        : value;
    });
  }

  async function refreshModuleData() {
    await Promise.all([
      queryClient.invalidateQueries({
        queryKey: queryKeys.stockMovementsWorkspace(token, session.lojaAtivaId),
      }),
      queryClient.invalidateQueries({
        queryKey: queryKeys.stockMovements(
          token,
          session.lojaAtivaId,
          movementFiltersKey,
        ),
      }),
      queryClient.invalidateQueries({
        queryKey: queryKeys.stockMovementPieces(
          token,
          session.lojaAtivaId,
          pieceFiltersKey,
        ),
      }),
    ]);
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    await adjustMutation.mutateAsync();
  }

  return (
    <div className="dashboard-grid">
      <div className="dashboard-column" style={{ gridColumn: "1 / -1" }}>
        <StockMovementsOverview workspace={workspaceQuery.data} />
      </div>

      <div className="dashboard-column">
        <StockMovementsListPanel
          busy={busy}
          filters={movementFilters}
          items={movementsQuery.data ?? []}
          movementTypes={workspaceQuery.data?.tiposMovimentacao ?? []}
          setFilters={setMovementFilters}
          statuses={workspaceQuery.data?.statusPeca ?? []}
          suppliers={workspaceQuery.data?.fornecedores ?? []}
        />
      </div>

      <div className="dashboard-column">
        <StockPieceSearchPanel
          busy={busy}
          filters={pieceFilters}
          items={piecesQuery.data ?? []}
          onApplyMovementFilter={(pieceId) =>
            setMovementFilters((current) => ({
              ...current,
              pecaId: pieceId,
            }))
          }
          onSelectPiece={(pieceId) => {
            setSelectedPieceId(pieceId);
          }}
          selectedPieceId={selectedPieceId}
          setFilters={setPieceFilters}
          statuses={workspaceQuery.data?.statusPeca ?? []}
          suppliers={workspaceQuery.data?.fornecedores ?? []}
        />

        <StockAdjustmentPanel
          busy={busy}
          canManage={canAdjustStock}
          form={form}
          onSubmit={handleSubmit}
          piece={selectedPiece}
          setForm={setForm}
          statuses={workspaceQuery.data?.statusPeca ?? []}
        />
      </div>
    </div>
  );
}
