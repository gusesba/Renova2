import { Card, CardBody, CardHeading } from "@/components/ui/card";
import { formatCurrency } from "@/lib/helpers/formatters";
import type { ClosingSummary, ClosingWorkspace } from "@/lib/services/closings";

type ClosingsOverviewProps = {
  closings: ClosingSummary[];
  workspace?: ClosingWorkspace;
};

// Resume o historico do modulo em cards numericos de apoio.
export function ClosingsOverview({ closings, workspace }: ClosingsOverviewProps) {
  const openCount = closings.filter((item) => item.statusFechamento === "aberto").length;
  const reviewedCount = closings.filter(
    (item) => item.statusFechamento === "conferido",
  ).length;
  const settledCount = closings.filter(
    (item) => item.statusFechamento === "liquidado",
  ).length;
  const totalBalance = closings.reduce((total, item) => total + item.saldoFinal, 0);

  return (
    <Card>
      <CardBody className="section-stack">
        <CardHeading
          title="Visao geral do fechamento"
          subtitle={`Loja ativa: ${workspace?.lojaNome ?? "Carregando..."}`}
        />

        <div className="catalogs-summary-grid">
          <div className="ui-banner">
            <div className="ui-field-label">Em aberto</div>
            <strong>{openCount}</strong>
          </div>
          <div className="ui-banner">
            <div className="ui-field-label">Conferidos</div>
            <strong>{reviewedCount}</strong>
          </div>
          <div className="ui-banner">
            <div className="ui-field-label">Liquidados</div>
            <strong>{settledCount}</strong>
          </div>
          <div className="ui-banner">
            <div className="ui-field-label">Saldo acumulado</div>
            <strong>{formatCurrency(totalBalance)}</strong>
          </div>
        </div>
      </CardBody>
    </Card>
  );
}
