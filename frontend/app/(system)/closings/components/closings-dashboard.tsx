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
import { ClosingDetailPanel } from "@/app/(system)/closings/components/closing-detail-panel";
import { ClosingFormPanel } from "@/app/(system)/closings/components/closing-form-panel";
import { ClosingsListPanel } from "@/app/(system)/closings/components/closings-list-panel";
import { ClosingsOverview } from "@/app/(system)/closings/components/closings-overview";
import {
  createGenerateClosingForm,
  emptyClosingFilters,
  type ClosingFiltersState,
  type GenerateClosingFormState,
} from "@/app/(system)/closings/components/types";
import { AccessStateCard } from "@/components/ui/access-state-card";
import {
  accessPermissionCodes,
  hasAnyPermission,
  hasPermission,
} from "@/lib/helpers/access-control";
import { getErrorMessage } from "@/lib/helpers/formatters";
import { queryKeys } from "@/lib/helpers/query-keys";
import { generateClosingSchema } from "@/lib/schemas/closings";
import {
  downloadClosingExport,
  generateClosing,
  getClosingById,
  getClosingsWorkspace,
  listClosings,
  reviewClosing,
  settleClosing,
} from "@/lib/services/closings";

// Coordena o modulo 13 com geracao, historico, conferencia e exportacao.
export function ClosingsDashboard() {
  const { token, session } = useSystemSession();
  const queryClient = useQueryClient();
  const [filters, setFilters] = useState<ClosingFiltersState>(emptyClosingFilters);
  const deferredFilters = useDeferredValue(filters);
  const filtersKey = useMemo(() => JSON.stringify(deferredFilters), [deferredFilters]);
  const [selectedClosingId, setSelectedClosingId] = useState("");
  const [formDraft, setFormDraft] = useState<GenerateClosingFormState | null>(null);
  const canViewModule = hasAnyPermission(session, [
    accessPermissionCodes.closingGenerate,
    accessPermissionCodes.closingReview,
  ]);
  const canGenerate = hasPermission(session, accessPermissionCodes.closingGenerate);
  const canReview = hasPermission(session, accessPermissionCodes.closingReview);

  const workspaceQuery = useQuery({
    enabled: Boolean(session.lojaAtivaId && canViewModule),
    queryFn: () => getClosingsWorkspace(token),
    queryKey: queryKeys.closingsWorkspace(token, session.lojaAtivaId),
  });

  const closingsQuery = useQuery({
    enabled: Boolean(session.lojaAtivaId && canViewModule),
    queryFn: () =>
      listClosings(token, {
        search: deferredFilters.search || undefined,
        pessoaId: deferredFilters.pessoaId || undefined,
        statusFechamento: deferredFilters.statusFechamento || undefined,
        dataInicial: deferredFilters.dataInicial || undefined,
        dataFinal: deferredFilters.dataFinal || undefined,
      }),
    queryKey: queryKeys.closings(token, session.lojaAtivaId, filtersKey),
  });

  const detailQuery = useQuery({
    enabled: Boolean(selectedClosingId && canViewModule),
    queryFn: () => getClosingById(token, selectedClosingId),
    queryKey: queryKeys.closingDetail(token, session.lojaAtivaId, selectedClosingId),
  });

  const generateMutation = useMutation({
    mutationFn: async () => {
      const parsed = generateClosingSchema.safeParse(formValue);
      if (!parsed.success) {
        throw new Error(parsed.error.issues[0]?.message ?? "Fechamento invalido.");
      }

      return generateClosing(token, parsed.data);
    },
    onError: (error) => {
      toast.error(getErrorMessage(error));
    },
    onSuccess: async (response) => {
      setSelectedClosingId(response.fechamento.id);
      setFormDraft(createGenerateClosingForm(workspaceQuery.data, response));
      toast.success("Fechamento gerado com sucesso.");
      await refreshModuleData(response.fechamento.id);
    },
  });

  const reviewMutation = useMutation({
    mutationFn: async () => {
      if (!selectedClosingId) {
        throw new Error("Selecione um fechamento para conferir.");
      }

      return reviewClosing(token, selectedClosingId);
    },
    onError: (error) => {
      toast.error(getErrorMessage(error));
    },
    onSuccess: async (response) => {
      toast.success("Fechamento marcado como conferido.");
      await refreshModuleData(response.fechamento.id);
    },
  });

  const settleMutation = useMutation({
    mutationFn: async () => {
      if (!selectedClosingId) {
        throw new Error("Selecione um fechamento para liquidar.");
      }

      return settleClosing(token, selectedClosingId);
    },
    onError: (error) => {
      toast.error(getErrorMessage(error));
    },
    onSuccess: async (response) => {
      toast.success("Fechamento marcado como liquidado.");
      await refreshModuleData(response.fechamento.id);
    },
  });

  const exportMutation = useMutation({
    mutationFn: async (exportType: "pdf" | "excel") => {
      if (!selectedClosingId) {
        throw new Error("Selecione um fechamento para exportar.");
      }

      return downloadClosingExport(token, selectedClosingId, exportType);
    },
    onError: (error) => {
      toast.error(getErrorMessage(error));
    },
    onSuccess: ({ blob, fileName }) => {
      const objectUrl = URL.createObjectURL(blob);
      const link = document.createElement("a");
      link.href = objectUrl;
      link.download = fileName;
      link.click();
      URL.revokeObjectURL(objectUrl);
      toast.success("Exportacao iniciada.");
    },
  });

  useEffect(() => {
    if (workspaceQuery.isError) {
      toast.error(getErrorMessage(workspaceQuery.error));
    }
  }, [workspaceQuery.error, workspaceQuery.isError]);

  useEffect(() => {
    if (closingsQuery.isError) {
      toast.error(getErrorMessage(closingsQuery.error));
    }
  }, [closingsQuery.error, closingsQuery.isError]);

  useEffect(() => {
    if (detailQuery.isError) {
      toast.error(getErrorMessage(detailQuery.error));
    }
  }, [detailQuery.error, detailQuery.isError]);

  useEffect(() => {
    const closings = closingsQuery.data ?? [];
    if (closings.length === 0) {
      startTransition(() => {
        setSelectedClosingId("");
      });
      return;
    }

    if (!selectedClosingId || !closings.some((item) => item.id === selectedClosingId)) {
      startTransition(() => {
        setSelectedClosingId(closings[0]?.id ?? "");
      });
    }
  }, [closingsQuery.data, selectedClosingId]);

  if (!canViewModule) {
    return (
      <AccessStateCard
        message="Solicite permissao para gerar ou conferir fechamentos na loja ativa."
        subtitle="Sua conta nao possui acesso ao modulo de fechamento do cliente e fornecedor."
        title="Modulo sem permissao"
      />
    );
  }

  const busy =
    workspaceQuery.isLoading ||
    closingsQuery.isLoading ||
    detailQuery.isLoading ||
    generateMutation.isPending ||
    reviewMutation.isPending ||
    settleMutation.isPending ||
    exportMutation.isPending;
  const formValue =
    formDraft ?? createGenerateClosingForm(workspaceQuery.data, detailQuery.data);

  function setForm(value: SetStateAction<GenerateClosingFormState>) {
    setFormDraft((current) => {
      const baseValue = current ?? formValue;
      return typeof value === "function"
        ? (value as (current: GenerateClosingFormState) => GenerateClosingFormState)(
            baseValue,
          )
        : value;
    });
  }

  async function refreshModuleData(closingId = selectedClosingId) {
    await Promise.all([
      queryClient.invalidateQueries({
        queryKey: queryKeys.closingsWorkspace(token, session.lojaAtivaId),
      }),
      queryClient.invalidateQueries({
        queryKey: queryKeys.closings(token, session.lojaAtivaId, filtersKey),
      }),
      closingId
        ? queryClient.invalidateQueries({
            queryKey: queryKeys.closingDetail(token, session.lojaAtivaId, closingId),
          })
        : Promise.resolve(),
    ]);
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    await generateMutation.mutateAsync();
  }

  async function handleCopySummary() {
    const summary = detailQuery.data?.resumoWhatsapp;
    if (!summary) {
      toast.error("Selecione um fechamento para copiar o resumo.");
      return;
    }

    try {
      await navigator.clipboard.writeText(summary);
      toast.success("Resumo copiado para a area de transferencia.");
    } catch {
      toast.error("Nao foi possivel copiar o resumo.");
    }
  }

  return (
    <div className="dashboard-grid">
      <div className="dashboard-column" style={{ gridColumn: "1 / -1" }}>
        <ClosingsOverview
          closings={closingsQuery.data ?? []}
          workspace={workspaceQuery.data}
        />
      </div>

      <div className="dashboard-column">
        <ClosingsListPanel
          closings={closingsQuery.data ?? []}
          filters={filters}
          onSelectClosing={setSelectedClosingId}
          people={workspaceQuery.data?.pessoas ?? []}
          selectedClosingId={selectedClosingId}
          setFilters={setFilters}
          statuses={workspaceQuery.data?.statusFechamento ?? []}
        />
      </div>

      <div className="dashboard-column">
        <ClosingFormPanel
          busy={busy}
          canGenerate={canGenerate}
          canReview={canReview}
          detail={detailQuery.data}
          form={formValue}
          onCopySummary={handleCopySummary}
          onExport={async (exportType) => {
            await exportMutation.mutateAsync(exportType);
          }}
          onReview={async () => {
            await reviewMutation.mutateAsync();
          }}
          onSettle={async () => {
            await settleMutation.mutateAsync();
          }}
          onSubmit={handleSubmit}
          setForm={setForm}
          workspace={workspaceQuery.data}
        />
        <ClosingDetailPanel detail={detailQuery.data} />
      </div>
    </div>
  );
}
