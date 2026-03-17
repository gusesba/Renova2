import { Card, CardBody, CardHeading } from "@/components/ui/card";
import { StatusBadge } from "@/components/ui/status-badge";
import { formatCurrency, formatDateTime } from "@/lib/helpers/formatters";
import type { PersonDetail, PersonSummary } from "@/lib/services/people";

// Exibe o resumo financeiro atual e o historico resumido da pessoa selecionada.
type PersonFinancialPanelProps = {
  detail?: PersonDetail | null;
  selectedSummary?: PersonSummary | null;
};

export function PersonFinancialPanel({
  detail,
  selectedSummary,
}: PersonFinancialPanelProps) {
  const financeiro = detail?.financeiro ?? selectedSummary?.financeiro;

  return (
    <Card>
      <CardBody className="section-stack">
        <CardHeading
          subtitle="Credito, pendencias e historico resumido no contexto da loja ativa."
          title="Visao financeira"
        />

        {!financeiro ? (
          <div className="empty-state">
            Selecione uma pessoa para visualizar o resumo financeiro.
          </div>
        ) : (
          <>
            <div className="metrics-grid">
              <div className="metric-card">
                <div className="metric-card-meta">Credito atual</div>
                <div className="metric-card-value">
                  {formatCurrency(financeiro.saldoCreditoAtual)}
                </div>
              </div>
              <div className="metric-card">
                <div className="metric-card-meta">Credito comprometido</div>
                <div className="metric-card-value">
                  {formatCurrency(financeiro.saldoCreditoComprometido)}
                </div>
              </div>
              <div className="metric-card">
                <div className="metric-card-meta">Pendencias abertas</div>
                <div className="metric-card-value">
                  {formatCurrency(financeiro.totalPendencias)}
                </div>
              </div>
              <div className="metric-card">
                <div className="metric-card-meta">Ultima movimentacao</div>
                <div className="metric-card-value" style={{ fontSize: "0.92rem" }}>
                  {formatDateTime(financeiro.ultimaMovimentacaoEm)}
                </div>
              </div>
            </div>

            <div className="record-list">
              {!detail || detail.historicoFinanceiro.length === 0 ? (
                <div className="empty-state">
                  Nenhum movimento financeiro encontrado para a pessoa selecionada.
                </div>
              ) : (
                detail.historicoFinanceiro.map((entry, index) => (
                  <div className="record-item" key={`${entry.id ?? entry.tipo}-${index}`}>
                    <div
                      style={{
                        display: "flex",
                        justifyContent: "space-between",
                        gap: "1rem",
                      }}
                    >
                      <div>
                        <div className="selection-item-title">{entry.descricao}</div>
                        <div className="record-item-copy">
                          {entry.referencia} • {formatDateTime(entry.ocorridoEm)}
                        </div>
                      </div>
                      <StatusBadge value={entry.direcao} />
                    </div>
                    <div className="record-tags">
                      <span className="record-tag">{entry.tipo}</span>
                      <span className="record-tag">
                        {formatCurrency(entry.valor)}
                      </span>
                    </div>
                  </div>
                ))
              )}
            </div>
          </>
        )}
      </CardBody>
    </Card>
  );
}
