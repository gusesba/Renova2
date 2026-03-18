import { MetricCard } from "@/components/ui/metric-card";
import { formatCurrency } from "@/lib/helpers/formatters";
import type { SaleSummary, SalesWorkspace } from "@/lib/services/sales";

type SalesOverviewProps = {
  sales: SaleSummary[];
  workspace?: SalesWorkspace;
};

// Resume o modulo com indicadores rapidos no topo da pagina.
export function SalesOverview({ sales, workspace }: SalesOverviewProps) {
  const concludedSales = sales.filter((sale) => sale.statusVenda === "concluida");
  const cancelledSales = sales.filter((sale) => sale.statusVenda === "cancelada");
  const grossValue = concludedSales.reduce(
    (sum, sale) => sum + (sale.subtotal - sale.descontoTotal),
    0,
  );
  const netValue = concludedSales.reduce((sum, sale) => sum + sale.totalLiquido, 0);

  return (
    <div className="catalogs-summary-grid">
      <MetricCard
        meta={`${workspace?.pecasDisponiveis.length ?? 0} pecas prontas para venda`}
        title="Vendas concluidas"
        value={String(concludedSales.length)}
      />
      <MetricCard
        meta={`${cancelledSales.length} canceladas no filtro atual`}
        title="Valor bruto"
        value={formatCurrency(grossValue)}
      />
      <MetricCard
        meta={`${workspace?.meiosPagamento.length ?? 0} meios ativos`}
        title="Valor liquido"
        value={formatCurrency(netValue)}
      />
      <MetricCard
        meta={`${workspace?.compradores.length ?? 0} compradores disponiveis`}
        title="Recebimentos"
        value={formatCurrency(
          concludedSales.reduce((sum, sale) => sum + sale.totalLiquido + sale.taxaTotal, 0),
        )}
      />
    </div>
  );
}
