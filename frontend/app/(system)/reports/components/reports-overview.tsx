import { Card, CardBody, CardHeading } from "@/components/ui/card";
import type { ReportResult, ReportWorkspace } from "@/lib/services/reports";

type ReportsOverviewProps = {
  result?: ReportResult;
  workspace?: ReportWorkspace;
};

// Resume o contexto da tela e os totais do ultimo relatorio executado.
export function ReportsOverview({ result, workspace }: ReportsOverviewProps) {
  return (
    <Card>
      <CardBody className="section-stack">
        <CardHeading
          title="Relatorios e exportacoes"
          subtitle={`Loja ativa: ${workspace?.lojaAtivaNome ?? "Carregando..."}`}
        />

        <div className="catalogs-summary-grid">
          <div className="ui-banner">
            <div className="ui-field-label">Tipos de relatorio</div>
            <strong>{workspace?.tiposRelatorio.length ?? 0}</strong>
          </div>
          <div className="ui-banner">
            <div className="ui-field-label">Filtros salvos</div>
            <strong>{workspace?.filtrosSalvos.length ?? 0}</strong>
          </div>
          <div className="ui-banner">
            <div className="ui-field-label">Registros retornados</div>
            <strong>{result?.quantidadeRegistros ?? 0}</strong>
          </div>
          <div className="ui-banner">
            <div className="ui-field-label">Relatorio atual</div>
            <strong>{result?.titulo ?? "Selecione os filtros"}</strong>
          </div>
        </div>
      </CardBody>
    </Card>
  );
}
