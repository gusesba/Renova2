"use client";

import { useQuery } from "@tanstack/react-query";
import { useEffect, useMemo, useState, type FormEvent } from "react";
import { toast } from "sonner";

import { useSystemSession } from "@/app/(system)/components/system-session-provider";
import { ConsignmentInsightsPanel } from "@/app/(system)/indicators/components/consignment-insights-panel";
import { DashboardFiltersPanel } from "@/app/(system)/indicators/components/dashboard-filters-panel";
import { FinancialInsightsPanel } from "@/app/(system)/indicators/components/financial-insights-panel";
import { IndicatorRankingsPanel } from "@/app/(system)/indicators/components/indicator-rankings-panel";
import { IndicatorsOverview } from "@/app/(system)/indicators/components/indicators-overview";
import { PendingInsightsPanel } from "@/app/(system)/indicators/components/pending-insights-panel";
import { SalesInsightsPanel } from "@/app/(system)/indicators/components/sales-insights-panel";
import {
  createDefaultDashboardFilters,
  type DashboardFiltersState,
} from "@/app/(system)/indicators/components/types";
import { AccessStateCard } from "@/components/ui/access-state-card";
import { canAccessIndicatorsModule } from "@/lib/helpers/access-control";
import { getErrorMessage } from "@/lib/helpers/formatters";
import { queryKeys } from "@/lib/helpers/query-keys";
import { getZodErrorMessage } from "@/lib/schemas/access";
import { dashboardFiltersSchema } from "@/lib/schemas/dashboards";
import {
  getDashboardOverview,
  getDashboardsWorkspace,
} from "@/lib/services/dashboards";

// Orquestra o modulo 14 com filtros, leitura consolidada e paineis de indicadores.
export function IndicatorsDashboard() {
  const { token, session } = useSystemSession();
  const canViewModule = canAccessIndicatorsModule(session);
  const [filters, setFilters] = useState<DashboardFiltersState>(
    createDefaultDashboardFilters(),
  );
  const [appliedFilters, setAppliedFilters] = useState<DashboardFiltersState>(
    createDefaultDashboardFilters(),
  );
  const filtersKey = useMemo(() => JSON.stringify(appliedFilters), [appliedFilters]);

  const workspaceQuery = useQuery({
    enabled: Boolean(session.lojaAtivaId && canViewModule),
    queryFn: () => getDashboardsWorkspace(token),
    queryKey: queryKeys.dashboardsWorkspace(token, session.lojaAtivaId),
    staleTime: 1000 * 60 * 5,
  });

  const overviewQuery = useQuery({
    enabled: Boolean(session.lojaAtivaId && canViewModule),
    queryFn: () => getDashboardOverview(token, appliedFilters),
    queryKey: queryKeys.dashboardOverview(
      token,
      session.lojaAtivaId,
      filtersKey,
    ),
  });

  useEffect(() => {
    if (workspaceQuery.isError) {
      toast.error(getErrorMessage(workspaceQuery.error));
    }
  }, [workspaceQuery.error, workspaceQuery.isError]);

  useEffect(() => {
    if (overviewQuery.isError) {
      toast.error(getErrorMessage(overviewQuery.error));
    }
  }, [overviewQuery.error, overviewQuery.isError]);

  if (session.lojas.length === 0 || !session.lojaAtivaId) {
    return (
      <AccessStateCard
        message="Crie a primeira loja ou selecione uma loja ativa para consultar os indicadores do sistema."
        subtitle="Os dashboards dependem do contexto operacional de uma loja."
        title="Loja ativa obrigatoria"
      />
    );
  }

  if (!canViewModule) {
    return (
      <AccessStateCard
        message="Solicite permissao comercial, financeira, de estoque ou de fechamento para visualizar os indicadores da loja."
        subtitle="Sua conta nao possui acesso ao modulo de dashboards e indicadores."
        title="Modulo sem permissao"
      />
    );
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    const parsed = dashboardFiltersSchema.safeParse(filters);
    if (!parsed.success) {
      toast.error(getZodErrorMessage(parsed.error));
      return;
    }

    setAppliedFilters(parsed.data);
  }

  return (
    <div className="dashboard-grid">
      <div className="dashboard-column" style={{ gridColumn: "1 / -1" }}>
        <IndicatorsOverview
          overview={overviewQuery.data}
          workspace={workspaceQuery.data}
        />
      </div>

      <div className="dashboard-column">
        <DashboardFiltersPanel
          busy={workspaceQuery.isLoading || overviewQuery.isLoading}
          filters={filters}
          onSubmit={handleSubmit}
          setFilters={setFilters}
          workspace={workspaceQuery.data}
        />
        <SalesInsightsPanel overview={overviewQuery.data} />
        <FinancialInsightsPanel overview={overviewQuery.data} />
      </div>

      <div className="dashboard-column">
        <ConsignmentInsightsPanel overview={overviewQuery.data} />
        <PendingInsightsPanel overview={overviewQuery.data} />
        <IndicatorRankingsPanel overview={overviewQuery.data} />
      </div>
    </div>
  );
}
