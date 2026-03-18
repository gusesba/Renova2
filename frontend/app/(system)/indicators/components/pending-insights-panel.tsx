import { Card, CardBody, CardHeading } from "@/components/ui/card";
import { formatCurrency } from "@/lib/helpers/formatters";
import type { DashboardOverview } from "@/lib/services/dashboards";

type PendingInsightsPanelProps = {
  overview?: DashboardOverview;
};

// Exibe valores pendentes e inconsistencias operacionais.
export function PendingInsightsPanel({ overview }: PendingInsightsPanelProps) {
  const pending = overview?.pendencias;

  return (
    <Card>
      <CardBody className="section-stack">
        <CardHeading
          title="Pendencias e inconsistencias"
          subtitle="Valores a pagar, valores pendentes de recebimento e alertas operacionais."
        />

        <div className="catalogs-summary-grid">
          <div className="ui-banner">
            <div className="ui-field-label">A pagar</div>
            <strong>{formatCurrency(pending?.valorPagarFornecedores ?? 0)}</strong>
          </div>
          <div className="ui-banner">
            <div className="ui-field-label">A receber</div>
            <strong>{formatCurrency(pending?.valorPendenteRecebimento ?? 0)}</strong>
          </div>
          <div className="ui-banner">
            <div className="ui-field-label">Inconsistencias</div>
            <strong>{pending?.quantidadeInconsistencias ?? 0}</strong>
          </div>
        </div>

        <div className="record-list">
          {(pending?.inconsistencias ?? []).map((item, index) => (
            <div className="record-item" key={`${item.tipo}-${index}`}>
              <div className="selection-item-title">{item.titulo}</div>
              <div className="record-item-copy">{item.descricao}</div>
              {item.valor != null ? (
                <div className="record-tags">
                  <span className="record-tag">{formatCurrency(item.valor)}</span>
                </div>
              ) : null}
            </div>
          ))}
        </div>
      </CardBody>
    </Card>
  );
}
