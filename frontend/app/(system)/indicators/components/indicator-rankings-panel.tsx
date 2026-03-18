import { Card, CardBody, CardHeading } from "@/components/ui/card";
import { formatCurrency } from "@/lib/helpers/formatters";
import type { DashboardIndicatorRow, DashboardOverview } from "@/lib/services/dashboards";

type IndicatorRankingsPanelProps = {
  overview?: DashboardOverview;
};

function RankingList({
  items,
  title,
}: {
  items: DashboardIndicatorRow[];
  title: string;
}) {
  return (
    <div className="section-stack">
      <div className="ui-field-label">{title}</div>
      <div className="record-list">
        {items.map((item) => (
          <div className="record-item" key={item.chave}>
            <div className="stock-record-header">
              <span className="selection-item-title">{item.nome}</span>
              <span className="record-tag">
                {formatCurrency(item.valorVendidoPeriodo)}
              </span>
            </div>
            <div className="record-tags">
              <span className="record-tag">Total {item.totalPecas}</span>
              <span className="record-tag">Atuais {item.pecasAtuais}</span>
              <span className="record-tag">Vendidas {item.pecasVendidasPeriodo}</span>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}

// Exibe os rankings por tipo, marca e fornecedor.
export function IndicatorRankingsPanel({
  overview,
}: IndicatorRankingsPanelProps) {
  const indicators = overview?.indicadores;

  return (
    <Card>
      <CardBody className="section-stack">
        <CardHeading
          title="Indicadores por base"
          subtitle="Leitura por tipo de peca, marca e fornecedor."
        />

        <div className="split-panels">
          <RankingList items={indicators?.porTipoPeca ?? []} title="Por tipo" />
          <RankingList items={indicators?.porMarca ?? []} title="Por marca" />
          <RankingList items={indicators?.porFornecedor ?? []} title="Por fornecedor" />
        </div>
      </CardBody>
    </Card>
  );
}
