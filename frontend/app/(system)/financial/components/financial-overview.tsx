import { MetricCard } from "@/components/ui/metric-card";
import { formatCurrency } from "@/lib/helpers/formatters";
import type {
  FinancialReconciliation,
  FinancialWorkspace,
} from "@/lib/services/financial";

type FinancialOverviewProps = {
  reconciliation?: FinancialReconciliation;
  workspace?: FinancialWorkspace;
};

// Resume o financeiro consolidado da loja ativa em quatro indicadores principais.
export function FinancialOverview({
  reconciliation,
  workspace,
}: FinancialOverviewProps) {
  const totals = reconciliation?.totais;

  return (
    <div className="catalogs-summary-grid">
      <MetricCard
        meta={`${totals?.quantidadeLancamentos ?? 0} movimentos filtrados`}
        title="Entradas brutas"
        value={formatCurrency(totals?.totalEntradasBrutas ?? 0)}
      />
      <MetricCard
        meta={`${workspace?.meiosPagamento.length ?? 0} meios ativos na loja`}
        title="Saidas brutas"
        value={formatCurrency(totals?.totalSaidasBrutas ?? 0)}
      />
      <MetricCard
        meta={`Taxas acumuladas ${formatCurrency(totals?.totalTaxas ?? 0)}`}
        title="Saldo bruto"
        value={formatCurrency(totals?.saldoBruto ?? 0)}
      />
      <MetricCard
        meta={`${workspace?.lojaNome ?? "Loja ativa"} como contexto financeiro`}
        title="Saldo liquido"
        value={formatCurrency(totals?.saldoLiquido ?? 0)}
      />
    </div>
  );
}
