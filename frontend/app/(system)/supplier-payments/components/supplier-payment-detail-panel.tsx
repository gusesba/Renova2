import { Card, CardBody, CardHeading } from "@/components/ui/card";
import { StatusBadge } from "@/components/ui/status-badge";
import {
  formatCurrency,
  formatDateTime,
  formatStatus,
} from "@/lib/helpers/formatters";
import type { SupplierObligationDetail } from "@/lib/services/supplier-payments";

type SupplierPaymentDetailPanelProps = {
  detail?: SupplierObligationDetail;
};

// Exibe o detalhe da obrigacao, historico e comprovante textual.
export function SupplierPaymentDetailPanel({
  detail,
}: SupplierPaymentDetailPanelProps) {
  if (!detail) {
    return (
      <Card>
        <CardBody className="section-stack">
          <CardHeading
            subtitle="Selecione uma obrigacao na lateral para consultar detalhes e comprovante."
            title="Detalhe da obrigacao"
          />
          <div className="empty-state">Nenhuma obrigacao selecionada.</div>
        </CardBody>
      </Card>
    );
  }

  return (
    <Card>
      <CardBody className="section-stack">
        <CardHeading
          subtitle={`${detail.obrigacao.fornecedorNome} • ${detail.obrigacao.fornecedorDocumento}`}
          title="Detalhe da obrigacao"
        />

        <div className="catalogs-summary-grid">
          <div className="ui-banner">
            <div className="ui-field-label">Original</div>
            <strong>{formatCurrency(detail.obrigacao.valorOriginal)}</strong>
          </div>
          <div className="ui-banner">
            <div className="ui-field-label">Liquidado</div>
            <strong>{formatCurrency(detail.obrigacao.valorLiquidado)}</strong>
          </div>
          <div className="ui-banner">
            <div className="ui-field-label">Em aberto</div>
            <strong>{formatCurrency(detail.obrigacao.valorEmAberto)}</strong>
          </div>
          <div className="ui-banner">
            <div className="ui-field-label">Status</div>
            <StatusBadge value={detail.obrigacao.statusObrigacao} />
          </div>
        </div>

        <div className="record-tags">
          <span className="record-tag">
            Tipo {formatStatus(detail.obrigacao.tipoObrigacao)}
          </span>
          {detail.obrigacao.codigoInternoPeca ? (
            <span className="record-tag">
              Peca {detail.obrigacao.codigoInternoPeca}
            </span>
          ) : null}
          {detail.obrigacao.numeroVenda ? (
            <span className="record-tag">Venda {detail.obrigacao.numeroVenda}</span>
          ) : null}
          <span className="record-tag">
            Gerada {formatDateTime(detail.obrigacao.dataGeracao)}
          </span>
        </div>

        <div className="record-list">
          {detail.liquidacoes.length === 0 ? (
            <div className="empty-state">
              Nenhuma liquidacao registrada para esta obrigacao.
            </div>
          ) : (
            detail.liquidacoes.map((liquidation) => (
              <div className="record-item" key={liquidation.id}>
                <div className="stock-record-header">
                  <div>
                    <div className="selection-item-title">
                      {formatStatus(liquidation.tipoLiquidacao)}
                    </div>
                    <div className="record-item-copy">
                      {liquidation.meioPagamentoNome ?? "Credito da loja"} •{" "}
                      {formatDateTime(liquidation.liquidadoEm)}
                    </div>
                  </div>
                  <div className="record-tag">
                    {formatCurrency(liquidation.valor)}
                  </div>
                </div>

                <div className="record-item-copy">
                  {liquidation.liquidadoPorUsuarioNome}
                </div>
                <div className="record-item-copy">{liquidation.observacoes}</div>
              </div>
            ))
          )}
        </div>

        <CardHeading
          subtitle="Modelo unico gerado a partir da obrigacao e das liquidacoes registradas."
          title="Comprovante"
        />
        <pre className="ui-banner" style={{ overflowX: "auto", whiteSpace: "pre-wrap" }}>
          {detail.comprovanteTexto}
        </pre>
      </CardBody>
    </Card>
  );
}
