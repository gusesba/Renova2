import { Card, CardBody, CardHeading } from "@/components/ui/card";
import { formatDateTime } from "@/lib/helpers/formatters";
import type { DashboardOverview } from "@/lib/services/dashboards";

type ConsignmentInsightsPanelProps = {
  overview?: DashboardOverview;
};

// Exibe pecas proximas do vencimento e paradas em estoque.
export function ConsignmentInsightsPanel({
  overview,
}: ConsignmentInsightsPanelProps) {
  const consignment = overview?.consignacao;

  return (
    <Card>
      <CardBody className="section-stack">
        <CardHeading
          title="Dashboard de consignacao"
          subtitle="Pecas proximas do vencimento e pecas paradas por tempo em estoque."
        />

        <div className="split-panels">
          <div className="section-stack">
            <div className="ui-field-label">Proximas do vencimento</div>
            <div className="record-list">
              {(consignment?.proximasVencer ?? []).map((item) => (
                <div className="record-item" key={item.pecaId}>
                  <div className="selection-item-title">{item.codigoInterno}</div>
                  <div className="record-item-copy">
                    {item.produtoNome} • {item.fornecedorNome ?? "Sem fornecedor"}
                  </div>
                  <div className="record-tags">
                    <span className="record-tag">
                      Limite {formatDateTime(item.dataLimite)}
                    </span>
                    <span className="record-tag">
                      {item.diasParaVencer ?? 0} dias para vencer
                    </span>
                  </div>
                </div>
              ))}
            </div>
          </div>

          <div className="section-stack">
            <div className="ui-field-label">Paradas em estoque</div>
            <div className="record-list">
              {(consignment?.paradasEmEstoque ?? []).map((item) => (
                <div className="record-item" key={item.pecaId}>
                  <div className="selection-item-title">{item.codigoInterno}</div>
                  <div className="record-item-copy">
                    {item.produtoNome} • {item.marcaNome}
                  </div>
                  <div className="record-tags">
                    <span className="record-tag">{item.diasEmEstoque} dias em estoque</span>
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>
      </CardBody>
    </Card>
  );
}
