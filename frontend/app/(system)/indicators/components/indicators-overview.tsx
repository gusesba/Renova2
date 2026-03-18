import { Card, CardBody, CardHeading } from "@/components/ui/card";
import { formatCurrency } from "@/lib/helpers/formatters";
import type { DashboardOverview, DashboardWorkspace } from "@/lib/services/dashboards";

type IndicatorsOverviewProps = {
  overview?: DashboardOverview;
  workspace?: DashboardWorkspace;
};

// Resume os principais numeros do modulo em cards de leitura rapida.
export function IndicatorsOverview({
  overview,
  workspace,
}: IndicatorsOverviewProps) {
  return (
    <Card>
      <CardBody className="section-stack">
        <CardHeading
          title="Dashboards e indicadores"
          subtitle={`Loja ativa: ${workspace?.lojaNome ?? "Carregando..."}`}
        />

        <div className="catalogs-summary-grid">
          <div className="ui-banner">
            <div className="ui-field-label">Total vendido</div>
            <strong>{formatCurrency(overview?.vendas.totalVendido ?? 0)}</strong>
          </div>
          <div className="ui-banner">
            <div className="ui-field-label">Saldo liquido</div>
            <strong>{formatCurrency(overview?.financeiro.saldoLiquido ?? 0)}</strong>
          </div>
          <div className="ui-banner">
            <div className="ui-field-label">A pagar</div>
            <strong>
              {formatCurrency(overview?.pendencias.valorPagarFornecedores ?? 0)}
            </strong>
          </div>
          <div className="ui-banner">
            <div className="ui-field-label">A receber</div>
            <strong>
              {formatCurrency(overview?.pendencias.valorPendenteRecebimento ?? 0)}
            </strong>
          </div>
        </div>
      </CardBody>
    </Card>
  );
}
