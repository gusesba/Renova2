import type { Dispatch, FormEvent, SetStateAction } from "react";

import { Button } from "@/components/ui/button";
import { Card, CardBody, CardHeading } from "@/components/ui/card";
import { TextArea } from "@/components/ui/field";
import { StatusBadge } from "@/components/ui/status-badge";
import {
  formatCurrency,
  formatDateTime,
  formatStatus,
} from "@/lib/helpers/formatters";
import type { SaleDetail } from "@/lib/services/sales";

import type { CancelSaleFormState } from "./types";

type SaleDetailPanelProps = {
  busy: boolean;
  canCancel: boolean;
  cancelForm: CancelSaleFormState;
  detail?: SaleDetail;
  setCancelForm: Dispatch<SetStateAction<CancelSaleFormState>>;
  onCancelSale: (event: FormEvent<HTMLFormElement>) => void;
};

// Mostra o detalhe da venda selecionada e o fluxo resumido de cancelamento.
export function SaleDetailPanel({
  busy,
  canCancel,
  cancelForm,
  detail,
  setCancelForm,
  onCancelSale,
}: SaleDetailPanelProps) {
  return (
    <Card>
      <CardBody>
        <CardHeading
          subtitle="Confira itens, pagamentos e recibo. O cancelamento ja dispara os estornos operacionais no backend."
          title="Detalhe da venda"
        />

        {!detail ? (
          <div className="empty-state">
            Selecione uma venda na lista para consultar o detalhe completo.
          </div>
        ) : (
          <div className="section-stack">
            <div className="stock-record-header">
              <div>
                <div className="selection-item-title">{detail.numeroVenda}</div>
                <div className="record-item-copy">
                  {detail.compradorNome ?? "Sem comprador"} • {detail.vendedorNome} •{" "}
                  {formatDateTime(detail.dataHoraVenda)}
                </div>
              </div>

              <StatusBadge value={detail.statusVenda} />
            </div>

            <div className="record-tags">
              <span className="record-tag">
                Subtotal {formatCurrency(detail.subtotal)}
              </span>
              <span className="record-tag">
                Desconto {formatCurrency(detail.descontoTotal)}
              </span>
              <span className="record-tag">
                Taxa {formatCurrency(detail.taxaTotal)}
              </span>
              <span className="record-tag">
                Liquido {formatCurrency(detail.totalLiquido)}
              </span>
            </div>

            <div className="section-stack">
              <div className="selection-item-title">Itens</div>
              <div className="record-list">
                {detail.itens.map((item) => (
                  <div className="record-item" key={item.id}>
                    <div className="stock-record-header">
                      <div>
                        <div className="selection-item-title">
                          {item.codigoInterno} • {item.produtoNome}
                        </div>
                        <div className="record-item-copy">
                          {item.marca} • {item.cor} • {item.tamanho} •{" "}
                          {formatStatus(item.tipoPecaSnapshot)}
                        </div>
                      </div>

                      <span className="record-tag">
                        {formatCurrency(item.precoFinalUnitario)}
                      </span>
                    </div>

                    <div className="record-tags">
                      <span className="record-tag">
                        Quantidade {item.quantidade}
                      </span>
                      <span className="record-tag">
                        Desconto {formatCurrency(item.descontoUnitario)}
                      </span>
                      <span className="record-tag">
                        Repasse {formatCurrency(item.valorRepassePrevisto)}
                      </span>
                    </div>
                  </div>
                ))}
              </div>
            </div>

            <div className="section-stack">
              <div className="selection-item-title">Pagamentos</div>
              <div className="record-list">
                {detail.pagamentos.map((payment) => (
                  <div className="record-item" key={payment.id}>
                    <div className="stock-record-header">
                      <div>
                        <div className="selection-item-title">
                          {formatStatus(payment.tipoPagamento)}
                        </div>
                        <div className="record-item-copy">
                          {payment.meioPagamentoNome ?? "Credito da loja"} •{" "}
                          {formatDateTime(payment.recebidoEm)}
                        </div>
                      </div>

                      <span className="record-tag">
                        {formatCurrency(payment.valor)}
                      </span>
                    </div>

                    <div className="record-tags">
                      <span className="record-tag">
                        Taxa {payment.taxaPercentualAplicada}%
                      </span>
                      <span className="record-tag">
                        Liquido {formatCurrency(payment.valorLiquido)}
                      </span>
                    </div>
                  </div>
                ))}
              </div>
            </div>

            <TextArea
              label="Recibo"
              readOnly
              rows={14}
              value={detail.reciboTexto}
            />

            {detail.statusVenda === "cancelada" ? (
              <div className="empty-state">
                Venda cancelada em {formatDateTime(detail.canceladaEm)}.
                <br />
                Motivo: {detail.motivoCancelamento ?? "Nao informado"}.
              </div>
            ) : (
              <form className="section-stack" onSubmit={onCancelSale}>
                <TextArea
                  label="Motivo do cancelamento"
                  onChange={(event) =>
                    setCancelForm((current) => ({
                      ...current,
                      motivoCancelamento: event.target.value,
                    }))
                  }
                  rows={3}
                  value={cancelForm.motivoCancelamento}
                />

                <Button disabled={busy || !canCancel} type="submit" variant="secondary">
                  Cancelar venda
                </Button>
              </form>
            )}
          </div>
        )}
      </CardBody>
    </Card>
  );
}
