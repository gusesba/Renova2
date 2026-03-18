import { MetricCard } from "@/components/ui/metric-card";
import { formatCurrency } from "@/lib/helpers/formatters";
import type { SupplierObligationSummary } from "@/lib/services/supplier-payments";

type SupplierPaymentsOverviewProps = {
  obligations: SupplierObligationSummary[];
};

// Resume o modulo com totais de pendencias e pagamentos realizados.
export function SupplierPaymentsOverview({
  obligations,
}: SupplierPaymentsOverviewProps) {
  const totalOpen = obligations.reduce(
    (sum, obligation) => sum + obligation.valorEmAberto,
    0,
  );
  const totalPaid = obligations.reduce(
    (sum, obligation) => sum + obligation.valorLiquidado,
    0,
  );
  const pendingCount = obligations.filter(
    (obligation) =>
      obligation.statusObrigacao === "pendente" ||
      obligation.statusObrigacao === "parcial",
  ).length;
  const paidCount = obligations.filter(
    (obligation) => obligation.statusObrigacao === "paga",
  ).length;

  return (
    <div className="catalogs-summary-grid">
      <MetricCard
        meta={`${pendingCount} com saldo pendente`}
        title="Obrigacoes"
        value={String(obligations.length)}
      />
      <MetricCard
        meta={`${paidCount} quitadas`}
        title="Saldo em aberto"
        value={formatCurrency(totalOpen)}
      />
      <MetricCard
        meta="Somatorio das liquidacoes registradas"
        title="Valor liquidado"
        value={formatCurrency(totalPaid)}
      />
      <MetricCard
        meta="Loja ativa como contexto da consulta"
        title="Pendencias"
        value={String(pendingCount)}
      />
    </div>
  );
}
