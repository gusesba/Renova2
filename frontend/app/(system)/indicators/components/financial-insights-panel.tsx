import { Card, CardBody, CardHeading } from "@/components/ui/card";
import { formatCurrency } from "@/lib/helpers/formatters";
import type { DashboardOverview } from "@/lib/services/dashboards";

type FinancialInsightsPanelProps = {
  overview?: DashboardOverview;
};

// Exibe os totais do painel financeiro do modulo.
export function FinancialInsightsPanel({
  overview,
}: FinancialInsightsPanelProps) {
  const financial = overview?.financeiro;

  return (
    <Card>
      <CardBody className="section-stack">
        <CardHeading
          title="Dashboard financeiro"
          subtitle="Entradas, saidas, saldo bruto e saldo liquido do periodo."
        />

        <div className="catalogs-summary-grid">
          <div className="ui-banner">
            <div className="ui-field-label">Entradas brutas</div>
            <strong>{formatCurrency(financial?.entradasBrutas ?? 0)}</strong>
          </div>
          <div className="ui-banner">
            <div className="ui-field-label">Saidas brutas</div>
            <strong>{formatCurrency(financial?.saidasBrutas ?? 0)}</strong>
          </div>
          <div className="ui-banner">
            <div className="ui-field-label">Saldo bruto</div>
            <strong>{formatCurrency(financial?.saldoBruto ?? 0)}</strong>
          </div>
          <div className="ui-banner">
            <div className="ui-field-label">Saldo liquido</div>
            <strong>{formatCurrency(financial?.saldoLiquido ?? 0)}</strong>
          </div>
        </div>

        <div className="record-tags">
          <span className="record-tag">
            Entradas: {financial?.quantidadeEntradas ?? 0}
          </span>
          <span className="record-tag">Saidas: {financial?.quantidadeSaidas ?? 0}</span>
          <span className="record-tag">
            Liquido de entrada: {formatCurrency(financial?.entradasLiquidas ?? 0)}
          </span>
          <span className="record-tag">
            Liquido de saida: {formatCurrency(financial?.saidasLiquidas ?? 0)}
          </span>
        </div>
      </CardBody>
    </Card>
  );
}
