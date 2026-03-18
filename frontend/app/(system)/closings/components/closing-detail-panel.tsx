import { Card, CardBody, CardHeading } from "@/components/ui/card";
import { StatusBadge } from "@/components/ui/status-badge";
import {
  formatCurrency,
  formatDateTime,
  formatStatus,
} from "@/lib/helpers/formatters";
import type { ClosingDetail } from "@/lib/services/closings";

type ClosingDetailPanelProps = {
  detail?: ClosingDetail;
};

// Exibe os snapshots de itens, movimentos e o resumo textual do fechamento.
export function ClosingDetailPanel({ detail }: ClosingDetailPanelProps) {
  if (!detail) {
    return (
      <Card>
        <CardBody className="section-stack">
          <CardHeading
            title="Detalhe do fechamento"
            subtitle="Selecione um fechamento na lateral para consultar o snapshot completo."
          />
          <div className="empty-state">Nenhum fechamento selecionado.</div>
        </CardBody>
      </Card>
    );
  }

  return (
    <Card>
      <CardBody className="section-stack">
        <CardHeading
          title="Detalhe do fechamento"
          subtitle={`${detail.fechamento.pessoaNome} • ${detail.fechamento.pessoaDocumento}`}
        />

        <div className="catalogs-summary-grid">
          <div className="ui-banner">
            <div className="ui-field-label">Valor vendido</div>
            <strong>{formatCurrency(detail.fechamento.valorVendido)}</strong>
          </div>
          <div className="ui-banner">
            <div className="ui-field-label">Valor a receber</div>
            <strong>{formatCurrency(detail.fechamento.valorAReceber)}</strong>
          </div>
          <div className="ui-banner">
            <div className="ui-field-label">Valor pago</div>
            <strong>{formatCurrency(detail.fechamento.valorPago)}</strong>
          </div>
          <div className="ui-banner">
            <div className="ui-field-label">Saldo final</div>
            <strong>{formatCurrency(detail.fechamento.saldoFinal)}</strong>
          </div>
        </div>

        <div className="record-tags">
          <span className="record-tag">
            Compras na loja {formatCurrency(detail.fechamento.valorCompradoNaLoja)}
          </span>
          <span className="record-tag">
            Credito atual {formatCurrency(detail.fechamento.saldoCreditoAtual)}
          </span>
          <span className="record-tag">
            Gerado por {detail.fechamento.geradoPorUsuarioNome}
          </span>
          <span className="record-tag">
            Gerado em {formatDateTime(detail.fechamento.geradoEm)}
          </span>
          <StatusBadge value={detail.fechamento.statusFechamento} />
        </div>

        <CardHeading
          title="Itens consolidados"
          subtitle="Pecas atuais e pecas vendidas congeladas no snapshot."
        />
        <div className="record-list">
          {detail.itens.length === 0 ? (
            <div className="empty-state">Nenhum item consolidado neste fechamento.</div>
          ) : (
            detail.itens.map((item) => (
              <div className="record-item" key={item.id}>
                <div className="stock-record-header">
                  <div>
                    <div className="selection-item-title">{item.codigoInternoPeca}</div>
                    <div className="record-item-copy">{item.produtoNomePeca}</div>
                  </div>
                  <StatusBadge value={item.statusPecaSnapshot} />
                </div>

                <div className="record-tags">
                  <span className="record-tag">Grupo {formatStatus(item.grupoItem)}</span>
                  <span className="record-tag">
                    Evento {formatDateTime(item.dataEvento)}
                  </span>
                  {item.valorVendaSnapshot != null ? (
                    <span className="record-tag">
                      Venda {formatCurrency(item.valorVendaSnapshot)}
                    </span>
                  ) : null}
                  {item.valorRepasseSnapshot != null ? (
                    <span className="record-tag">
                      Repasse {formatCurrency(item.valorRepasseSnapshot)}
                    </span>
                  ) : null}
                </div>
              </div>
            ))
          )}
        </div>

        <CardHeading
          title="Movimentos consolidados"
          subtitle="Vendas, pagamentos, credito e compras do periodo."
        />
        <div className="record-list">
          {detail.movimentos.length === 0 ? (
            <div className="empty-state">Nenhum movimento consolidado neste fechamento.</div>
          ) : (
            detail.movimentos.map((movement) => (
              <div className="record-item" key={movement.id}>
                <div className="stock-record-header">
                  <div>
                    <div className="selection-item-title">
                      {formatStatus(movement.tipoMovimento)}
                    </div>
                    <div className="record-item-copy">
                      {formatDateTime(movement.dataMovimento)}
                    </div>
                  </div>
                  <div className="record-tag">{formatCurrency(movement.valor)}</div>
                </div>
                <div className="record-item-copy">{movement.descricao}</div>
              </div>
            ))
          )}
        </div>

        <CardHeading
          title="Resumo para WhatsApp"
          subtitle="Texto pronto para conferencia e compartilhamento."
        />
        <pre className="ui-banner" style={{ overflowX: "auto", whiteSpace: "pre-wrap" }}>
          {detail.resumoWhatsapp}
        </pre>
      </CardBody>
    </Card>
  );
}
