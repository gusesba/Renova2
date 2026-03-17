import { MetricCard } from "@/components/ui/metric-card";
import type { StoreSummary } from "@/lib/services/renova-api";

// Resumo superior do modulo com foco em lojas acessiveis e contexto ativo.
type StoresOverviewProps = {
  stores: StoreSummary[];
};

export function StoresOverview({ stores }: StoresOverviewProps) {
  const activeStore = stores.find((store) => store.ehLojaAtiva);
  const activeCount = stores.filter((store) => store.ativo).length;

  return (
    <div className="metrics-grid">
      <MetricCard
        meta="lojas vinculadas ao usuario autenticado"
        title="Lojas acessiveis"
        value={String(stores.length)}
      />
      <MetricCard
        meta="lojas ativas no contexto consolidado"
        title="Lojas ativas"
        value={String(activeCount)}
      />
      <MetricCard
        meta={activeStore ? `${activeStore.cidade} / ${activeStore.uf}` : "nenhuma loja ativa"}
        title="Loja ativa"
        value={activeStore?.nomeFantasia ?? "Pendente"}
      />
    </div>
  );
}
