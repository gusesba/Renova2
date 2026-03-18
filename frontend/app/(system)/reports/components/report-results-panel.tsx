import { Button } from "@/components/ui/button";
import { Card, CardBody, CardHeading } from "@/components/ui/card";
import type { ReportResult } from "@/lib/services/reports";

type ReportResultsPanelProps = {
  busy: boolean;
  result?: ReportResult;
  onExport: (format: "pdf" | "excel") => Promise<void>;
};

// Exibe o grid, metricas e exportacoes do relatorio executado.
export function ReportResultsPanel({
  busy,
  result,
  onExport,
}: ReportResultsPanelProps) {
  return (
    <Card>
      <CardBody className="section-stack">
        <CardHeading
          title={result?.titulo ?? "Resultado do relatorio"}
          subtitle={result?.subtitulo ?? "Execute um relatorio para visualizar os dados."}
        />

        <div className="catalogs-summary-grid">
          {(result?.metricas ?? []).map((metric) => (
            <div className="ui-banner" key={metric.nome}>
              <div className="ui-field-label">{metric.nome}</div>
              <strong>{metric.valor}</strong>
            </div>
          ))}
        </div>

        <div className="record-tags">
          <Button disabled={busy || !result} onClick={() => void onExport("pdf")}>
            Exportar PDF
          </Button>
          <Button
            disabled={busy || !result}
            onClick={() => void onExport("excel")}
            variant="soft"
          >
            Exportar Excel
          </Button>
        </div>

        {result ? (
          <div className="report-table-shell">
            <table className="report-table">
              <thead>
                <tr>
                  {result.colunas.map((column) => (
                    <th key={column.chave}>{column.titulo}</th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {result.linhas.map((row) => (
                  <tr key={row.id}>
                    {result.colunas.map((column) => (
                      <td key={`${row.id}-${column.chave}`}>
                        {row.celulas.find((cell) => cell.chave === column.chave)?.valor ?? ""}
                      </td>
                    ))}
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : (
          <div className="empty-state">
            Nenhum relatorio executado ainda.
          </div>
        )}
      </CardBody>
    </Card>
  );
}
