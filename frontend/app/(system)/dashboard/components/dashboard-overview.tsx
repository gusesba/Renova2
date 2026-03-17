import { MetricCard } from "@/components/ui/metric-card";
import type { SessionContext } from "@/lib/services/access";

// Resumo superior da tela para dar contexto rapido da loja e do modulo.
type DashboardOverviewProps = {
  session: SessionContext;
  usersCount: number;
  rolesCount: number;
  membershipsCount: number;
};

export function DashboardOverview({
  session,
  usersCount,
  rolesCount,
  membershipsCount,
}: DashboardOverviewProps) {
  return (
    <div className="metrics-grid">
      <MetricCard
        meta="usuarios disponiveis na plataforma"
        title="Usuarios"
        value={String(usersCount)}
      />
      <MetricCard
        meta="cargos prontos para associacao"
        title="Cargos"
        value={String(rolesCount)}
      />
      <MetricCard
        meta={`${session.lojas.length} lojas acessiveis na sessao`}
        title="Vinculos"
        value={String(membershipsCount)}
      />
    </div>
  );
}
