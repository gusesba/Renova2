import { Card, CardBody, CardHeading } from "@/components/ui/card";
import { formatCurrency } from "@/lib/helpers/formatters";
import type { DashboardOverview } from "@/lib/services/dashboards";

type SalesInsightsPanelProps = {
  overview?: DashboardOverview;
};

// Exibe os agrupamentos do dashboard de vendas.
export function SalesInsightsPanel({ overview }: SalesInsightsPanelProps) {
  const sales = overview?.vendas;

  return (
    <Card>
      <CardBody className="section-stack">
        <CardHeading
          title="Dashboard de vendas"
          subtitle="Totais por periodo, dia, mes e vendedor."
        />

        <div className="catalogs-summary-grid">
          <div className="ui-banner">
            <div className="ui-field-label">Quantidade de vendas</div>
            <strong>{sales?.quantidadeVendas ?? 0}</strong>
          </div>
          <div className="ui-banner">
            <div className="ui-field-label">Pecas vendidas</div>
            <strong>{sales?.quantidadePecasVendidas ?? 0}</strong>
          </div>
          <div className="ui-banner">
            <div className="ui-field-label">Total vendido</div>
            <strong>{formatCurrency(sales?.totalVendido ?? 0)}</strong>
          </div>
          <div className="ui-banner">
            <div className="ui-field-label">Ticket medio</div>
            <strong>{formatCurrency(sales?.ticketMedio ?? 0)}</strong>
          </div>
        </div>

        <div className="split-panels">
          <div className="section-stack">
            <div className="ui-field-label">Por dia</div>
            <div className="record-list">
              {(sales?.porDia ?? []).map((item) => (
                <div className="record-item" key={item.chave}>
                  <div className="stock-record-header">
                    <span className="selection-item-title">{item.nome}</span>
                    <span className="record-tag">{formatCurrency(item.valor)}</span>
                  </div>
                  <div className="record-item-copy">{item.quantidade} vendas</div>
                </div>
              ))}
            </div>
          </div>

          <div className="section-stack">
            <div className="ui-field-label">Por vendedor</div>
            <div className="record-list">
              {(sales?.porVendedor ?? []).map((item) => (
                <div className="record-item" key={item.chave}>
                  <div className="stock-record-header">
                    <span className="selection-item-title">{item.nome}</span>
                    <span className="record-tag">{formatCurrency(item.valor)}</span>
                  </div>
                  <div className="record-item-copy">{item.quantidade} vendas</div>
                </div>
              ))}
            </div>
          </div>
        </div>
      </CardBody>
    </Card>
  );
}
