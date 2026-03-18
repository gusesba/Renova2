import { Card, CardBody, CardHeading } from "@/components/ui/card";
import { formatCurrency, formatDateTime } from "@/lib/helpers/formatters";
import type { ConsignmentDetail } from "@/lib/services/consignments";

// Exibe o detalhe operacional da peca consignada selecionada.
type ConsignmentDetailPanelProps = {
  detail?: ConsignmentDetail;
};

export function ConsignmentDetailPanel({ detail }: ConsignmentDetailPanelProps) {
  if (!detail) {
    return (
      <Card>
        <CardBody className="empty-state">
          Selecione uma peca consignada para visualizar prazo, politica e historico.
        </CardBody>
      </Card>
    );
  }

  const summary = detail.resumo;

  return (
    <Card>
      <CardBody className="section-stack">
        <CardHeading
          subtitle={`${summary.codigoInterno} | ${summary.produtoNome}`}
          title="Detalhe da consignacao"
        />

        <div className="split-fields">
          <div className="ui-banner">
            <strong>Preco atual:</strong> {formatCurrency(summary.precoVendaAtual)}
          </div>
          <div className="ui-banner">
            <strong>Preco base:</strong> {formatCurrency(summary.precoBase)}
          </div>
        </div>

        <div className="split-fields">
          <div className="selection-item">
            <div>
              <div className="selection-item-title">Prazo operacional</div>
              <div className="selection-item-copy">
                Inicio: {formatDateTime(summary.dataInicioConsignacao)}
              </div>
              <div className="selection-item-copy">
                Fim: {formatDateTime(summary.dataFimConsignacao)}
              </div>
              <div className="selection-item-copy">
                Dias em loja: {summary.diasEmLoja}
              </div>
              <div className="selection-item-copy">
                Dias restantes: {summary.diasRestantes ?? "Encerrada"}
              </div>
            </div>
          </div>

          <div className="selection-item">
            <div>
              <div className="selection-item-title">Regra aplicada</div>
              <div className="selection-item-copy">
                Desconto esperado: {summary.percentualDescontoEsperado}%
              </div>
              <div className="selection-item-copy">
                Desconto aplicado: {summary.percentualDescontoAplicado}%
              </div>
              <div className="selection-item-copy">
                Destino padrao no fim: {summary.destinoPadraoFimConsignacao ?? "Nao informado"}
              </div>
              <div className="selection-item-copy">
                Alerta aberto: {summary.alertaAberto ? "Sim" : "Nao"}
              </div>
            </div>
          </div>
        </div>

        <div className="section-stack">
          <div className="selection-item-title">Politica de desconto</div>
          <div className="record-list">
            {detail.politicaDesconto.length === 0 ? (
              <div className="empty-state">Nao existe politica de desconto cadastrada.</div>
            ) : (
              detail.politicaDesconto.map((band) => (
                <div className="record-item" key={`${band.diasMinimos}-${band.percentualDesconto}`}>
                  <div className="selection-item-title">
                    A partir de {band.diasMinimos} dia(s)
                  </div>
                  <div className="record-item-copy">
                    Desconto previsto de {band.percentualDesconto}% sobre o preco base.
                  </div>
                </div>
              ))
            )}
          </div>
        </div>

        <div className="section-stack">
          <div className="selection-item-title">Historico de preco</div>
          <div className="record-list">
            {detail.historicoPreco.length === 0 ? (
              <div className="empty-state">Nenhum ajuste de preco registrado para esta peca.</div>
            ) : (
              detail.historicoPreco.map((item) => (
                <div className="record-item" key={item.id}>
                  <div className="selection-item-title">
                    {formatCurrency(item.precoAnterior)} → {formatCurrency(item.precoNovo)}
                  </div>
                  <div className="record-item-copy">{item.motivo}</div>
                  <div className="record-item-copy">
                    {formatDateTime(item.alteradoEm)}
                  </div>
                </div>
              ))
            )}
          </div>
        </div>
      </CardBody>
    </Card>
  );
}
