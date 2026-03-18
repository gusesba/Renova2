import { Card, CardBody, CardHeading } from "@/components/ui/card";
import { formatCurrency } from "@/lib/helpers/formatters";
import type { FinancialReconciliation } from "@/lib/services/financial";

type FinancialReconciliationPanelProps = {
  reconciliation?: FinancialReconciliation;
};

// Exibe a conciliacao por meio, por tipo e o resumo diario do periodo filtrado.
export function FinancialReconciliationPanel({
  reconciliation,
}: FinancialReconciliationPanelProps) {
  return (
    <Card>
      <CardBody className="section-stack">
        <CardHeading
          subtitle="Acompanhe liquido, taxas e saldo por meio de pagamento, tipo e dia."
          title="Conciliacao"
        />

        <div className="section-stack">
          <div>
            <div className="ui-field-label">Por meio de pagamento</div>
            <div className="record-list">
              {reconciliation?.porMeioPagamento.length ? (
                reconciliation.porMeioPagamento.map((item) => (
                  <div className="record-item" key={item.codigo}>
                    <div className="selection-item-title">{item.nome}</div>
                    <div className="record-tags">
                      <span className="record-tag">
                        Liquido {formatCurrency(item.saldoLiquido)}
                      </span>
                      <span className="record-tag">
                        Taxas {formatCurrency(item.totalTaxas)}
                      </span>
                      <span className="record-tag">
                        Itens {item.quantidadeLancamentos}
                      </span>
                    </div>
                  </div>
                ))
              ) : (
                <div className="empty-state">
                  Nenhuma consolidacao por meio de pagamento para o filtro atual.
                </div>
              )}
            </div>
          </div>

          <div>
            <div className="ui-field-label">Por tipo de movimentacao</div>
            <div className="record-list">
              {reconciliation?.porTipoMovimentacao.length ? (
                reconciliation.porTipoMovimentacao.map((item) => (
                  <div className="record-item" key={item.codigo}>
                    <div className="selection-item-title">{item.nome}</div>
                    <div className="record-tags">
                      <span className="record-tag">
                        Bruto {formatCurrency(item.saldoBruto)}
                      </span>
                      <span className="record-tag">
                        Liquido {formatCurrency(item.saldoLiquido)}
                      </span>
                      <span className="record-tag">
                        Taxas {formatCurrency(item.totalTaxas)}
                      </span>
                    </div>
                  </div>
                ))
              ) : (
                <div className="empty-state">
                  Nenhuma consolidacao por tipo para o filtro atual.
                </div>
              )}
            </div>
          </div>

          <div>
            <div className="ui-field-label">Resumo diario</div>
            <div className="record-list">
              {reconciliation?.resumoDiario.length ? (
                reconciliation.resumoDiario.map((item) => (
                  <div className="record-item" key={item.data}>
                    <div className="selection-item-title">{item.data}</div>
                    <div className="record-tags">
                      <span className="record-tag">
                        Entradas {formatCurrency(item.totalEntradasBrutas)}
                      </span>
                      <span className="record-tag">
                        Saidas {formatCurrency(item.totalSaidasBrutas)}
                      </span>
                      <span className="record-tag">
                        Liquido {formatCurrency(item.saldoLiquido)}
                      </span>
                    </div>
                  </div>
                ))
              ) : (
                <div className="empty-state">
                  Nenhum resumo diario encontrado para o periodo atual.
                </div>
              )}
            </div>
          </div>
        </div>
      </CardBody>
    </Card>
  );
}
