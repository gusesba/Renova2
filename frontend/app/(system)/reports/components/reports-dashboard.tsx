"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useEffect, useMemo, useState, type FormEvent } from "react";
import { toast } from "sonner";

import { useSystemSession } from "@/app/(system)/components/system-session-provider";
import { ReportFiltersPanel } from "@/app/(system)/reports/components/report-filters-panel";
import { ReportResultsPanel } from "@/app/(system)/reports/components/report-results-panel";
import { ReportsOverview } from "@/app/(system)/reports/components/reports-overview";
import { SavedReportFiltersPanel } from "@/app/(system)/reports/components/saved-report-filters-panel";
import {
  createDefaultReportQuery,
  toReportPayload,
  type ReportQueryState,
} from "@/app/(system)/reports/components/types";
import { AccessStateCard } from "@/components/ui/access-state-card";
import { canAccessReportsModule } from "@/lib/helpers/access-control";
import { getErrorMessage } from "@/lib/helpers/formatters";
import { queryKeys } from "@/lib/helpers/query-keys";
import { getZodErrorMessage } from "@/lib/schemas/access";
import { reportQuerySchema, saveReportFilterSchema } from "@/lib/schemas/reports";
import {
  deleteReportFilter,
  downloadReportExport,
  getReportsWorkspace,
  runReport,
  saveReportFilter,
  type ReportWorkspace,
  type SavedReportFilter,
} from "@/lib/services/reports";

// Orquestra o modulo 15 com filtros, grid de resultado, exportacao e filtros salvos.
export function ReportsDashboard() {
  const { token, session } = useSystemSession();
  const queryClient = useQueryClient();
  const canViewModule = canAccessReportsModule(session);
  const [filters, setFilters] = useState<ReportQueryState>(createDefaultReportQuery());
  const [appliedFilters, setAppliedFilters] = useState<ReportQueryState>(
    createDefaultReportQuery(),
  );
  const [filterName, setFilterName] = useState("");

  const workspaceQuery = useQuery({
    enabled: Boolean(session.lojaAtivaId && canViewModule),
    queryFn: () => getReportsWorkspace(token),
    queryKey: queryKeys.reportsWorkspace(token, session.lojaAtivaId),
    staleTime: 1000 * 60 * 5,
  });

  const resolvedFilters = useMemo(
    () => resolveReportState(filters, workspaceQuery.data),
    [filters, workspaceQuery.data],
  );
  const resolvedAppliedFilters = useMemo(
    () => resolveReportState(appliedFilters, workspaceQuery.data),
    [appliedFilters, workspaceQuery.data],
  );
  const filtersKey = useMemo(
    () => JSON.stringify(toReportPayload(resolvedAppliedFilters)),
    [resolvedAppliedFilters],
  );

  const resultQuery = useQuery({
    enabled: Boolean(session.lojaAtivaId && canViewModule),
    queryFn: () => runReport(token, toReportPayload(resolvedAppliedFilters)),
    queryKey: queryKeys.reportResult(token, session.lojaAtivaId, filtersKey),
  });

  const saveFilterMutation = useMutation({
    mutationFn: async () => {
      const parsedName = saveReportFilterSchema.safeParse({ nome: filterName });
      if (!parsedName.success) {
        throw new Error(getZodErrorMessage(parsedName.error));
      }

      return saveReportFilter(token, {
        nome: parsedName.data.nome,
        filtros: toReportPayload(resolvedAppliedFilters),
      });
    },
    onError: (error) => {
      toast.error(getErrorMessage(error));
    },
    onSuccess: async () => {
      setFilterName("");
      toast.success("Filtro salvo com sucesso.");
      await queryClient.invalidateQueries({
        queryKey: queryKeys.reportsWorkspace(token, session.lojaAtivaId),
      });
    },
  });

  const deleteFilterMutation = useMutation({
    mutationFn: (filterId: string) => deleteReportFilter(token, filterId),
    onError: (error) => {
      toast.error(getErrorMessage(error));
    },
    onSuccess: async () => {
      toast.success("Filtro removido com sucesso.");
      await queryClient.invalidateQueries({
        queryKey: queryKeys.reportsWorkspace(token, session.lojaAtivaId),
      });
    },
  });

  const exportMutation = useMutation({
    mutationFn: async (format: "pdf" | "excel") =>
      downloadReportExport(token, format, toReportPayload(resolvedAppliedFilters)),
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
    if (resultQuery.isError) {
      toast.error(getErrorMessage(resultQuery.error));
    }
  }, [resultQuery.error, resultQuery.isError]);

  if (!canViewModule) {
    return (
      <AccessStateCard
        message="Solicite a permissao de exportar relatorios para consultar este modulo."
        subtitle="Sua conta nao possui acesso ao modulo de relatorios e exportacoes."
        title="Modulo sem permissao"
      />
    );
  }

  const busy =
    workspaceQuery.isLoading ||
    resultQuery.isLoading ||
    saveFilterMutation.isPending ||
    deleteFilterMutation.isPending ||
    exportMutation.isPending;

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    const parsed = reportQuerySchema.safeParse(resolvedFilters);
    if (!parsed.success) {
      toast.error(getZodErrorMessage(parsed.error));
      return;
    }

    setAppliedFilters(parsed.data);
  }

  function handleApplyFilter(filter: SavedReportFilter) {
    const nextFilters: ReportQueryState = {
      tipoRelatorio: filter.filtros.tipoRelatorio,
      lojaId: filter.filtros.lojaId ?? "",
      dataInicial: filter.filtros.dataInicial ?? "",
      dataFinal: filter.filtros.dataFinal ?? "",
      fornecedorPessoaId: filter.filtros.fornecedorPessoaId ?? "",
      pessoaId: filter.filtros.pessoaId ?? "",
      marcaId: filter.filtros.marcaId ?? "",
      vendedorUsuarioId: filter.filtros.vendedorUsuarioId ?? "",
      statusPeca: filter.filtros.statusPeca ?? "",
      motivoMovimentacao: filter.filtros.motivoMovimentacao ?? "",
      search: filter.filtros.search ?? "",
    };

    setFilters(nextFilters);
    setAppliedFilters(nextFilters);
    toast.success("Filtro aplicado.");
  }

  return (
    <div className="dashboard-grid">
      <div className="dashboard-column" style={{ gridColumn: "1 / -1" }}>
        <ReportsOverview result={resultQuery.data} workspace={workspaceQuery.data} />
      </div>

      <div className="dashboard-column">
        <ReportFiltersPanel
          busy={busy}
          filters={resolvedFilters}
          onSubmit={handleSubmit}
          setFilters={setFilters}
          workspace={workspaceQuery.data}
        />
        <SavedReportFiltersPanel
          busy={busy}
          filterName={filterName}
          filters={resolvedFilters}
          onApplyFilter={handleApplyFilter}
          onDeleteFilter={async (filterId) => {
            await deleteFilterMutation.mutateAsync(filterId);
          }}
          onSaveFilter={async () => {
            await saveFilterMutation.mutateAsync();
          }}
          savedFilters={workspaceQuery.data?.filtrosSalvos ?? []}
          setFilterName={setFilterName}
        />
      </div>

      <div className="dashboard-column">
        <ReportResultsPanel
          busy={busy}
          onExport={async (format) => {
            await exportMutation.mutateAsync(format);
          }}
          result={resultQuery.data}
        />
      </div>
    </div>
  );
}

function resolveReportState(
  state: ReportQueryState,
  workspace?: ReportWorkspace,
) {
  const fallback = createDefaultReportQuery(workspace);

  return {
    ...state,
    tipoRelatorio: state.tipoRelatorio || fallback.tipoRelatorio,
    lojaId: state.lojaId || fallback.lojaId,
  };
}
