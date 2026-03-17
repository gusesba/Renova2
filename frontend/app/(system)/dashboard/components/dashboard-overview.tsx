import { MetricCard } from "@/components/ui/metric-card";
import type { SessionContext } from "@/lib/services/access";

// Resumo superior da tela para dar contexto rapido da loja e do modulo.
type DashboardOverviewProps = {
  showMemberships: boolean;
  showRoles: boolean;
  showUsers: boolean;
  session: SessionContext;
  usersCount: number;
  rolesCount: number;
  membershipsCount: number;
};

export function DashboardOverview({
  showMemberships,
  showRoles,
  showUsers,
  session,
  usersCount,
  rolesCount,
  membershipsCount,
}: DashboardOverviewProps) {
  if (!showUsers && !showRoles && !showMemberships) {
    return null;
  }

  return (
    <div className="metrics-grid">
      {showUsers ? (
        <MetricCard
          meta="usuarios disponiveis na plataforma"
          title="Usuarios"
          value={String(usersCount)}
        />
      ) : null}
      {showRoles ? (
        <MetricCard
          meta="cargos prontos para associacao"
          title="Cargos"
          value={String(rolesCount)}
        />
      ) : null}
      {showMemberships ? (
        <MetricCard
          meta={`${session.lojas.length} lojas acessiveis na sessao`}
          title="Vinculos"
          value={String(membershipsCount)}
        />
      ) : null}
    </div>
  );
}
