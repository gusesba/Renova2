import { type FormEvent } from "react";

import { Button } from "@/components/ui/button";
import { Card, CardBody, CardHeading } from "@/components/ui/card";
import { SelectField, TextArea, TextInput } from "@/components/ui/field";
import { formatCurrency } from "@/lib/helpers/formatters";
import type {
  SupplierObligationDetail,
  SupplierPaymentWorkspace,
} from "@/lib/services/supplier-payments";

import type { SupplierSettlementFormState } from "@/app/(system)/supplier-payments/components/types";

type SupplierLiquidationPanelProps = {
  busy: boolean;
  canManage: boolean;
  detail?: SupplierObligationDetail;
  form: SupplierSettlementFormState;
  workspace?: SupplierPaymentWorkspace;
  setForm: (
    updater:
      | SupplierSettlementFormState
      | ((current: SupplierSettlementFormState) => SupplierSettlementFormState),
  ) => void;
  onAddLine: () => void;
  onRemoveLine: (index: number) => void;
  onSubmit: (event: FormEvent<HTMLFormElement>) => Promise<void>;
};

// Reune o formulario de liquidacao parcial ou total da obrigacao selecionada.
export function SupplierLiquidationPanel({
  busy,
  canManage,
  detail,
  form,
  workspace,
  setForm,
  onAddLine,
  onRemoveLine,
  onSubmit,
}: SupplierLiquidationPanelProps) {
  return (
    <Card>
      <CardBody className="section-stack">
        <CardHeading
          subtitle="Permite pagamento financeiro, credito da loja ou combinacao entre os dois."
          title="Liquidar obrigacao"
        />

        {!detail ? (
          <div className="empty-state">
            Selecione uma obrigacao para registrar a liquidacao.
          </div>
        ) : !canManage ? (
          <div className="empty-state">
            Seu cargo pode consultar pendencias e comprovantes, mas nao pode liquidar repasses.
          </div>
        ) : (
          <form className="section-stack" onSubmit={onSubmit}>
            <div className="ui-banner">
              Saldo atual da obrigacao:{" "}
              <strong>{formatCurrency(detail.obrigacao.valorEmAberto)}</strong>
            </div>

            <div className="section-stack stack-scroll">
              {form.pagamentos.map((payment, index) => (
                <div className="selection-item" key={`payment-${index}`}>
                  <div className="form-grid" style={{ flex: 1 }}>
                    <div className="split-fields">
                      <SelectField
                        disabled={busy}
                        label="Tipo"
                        onChange={(event) =>
                          setForm((current) => ({
                            ...current,
                            pagamentos: current.pagamentos.map((item, itemIndex) =>
                              itemIndex === index
                                ? {
                                    ...item,
                                    tipoLiquidacao: event.target.value,
                                    meioPagamentoId:
                                      event.target.value === "meio_pagamento"
                                        ? item.meioPagamentoId ||
                                          workspace?.meiosPagamento[0]?.id ||
                                          ""
                                        : "",
                                  }
                                : item,
                            ),
                          }))
                        }
                        value={payment.tipoLiquidacao}
                      >
                        {workspace?.tiposLiquidacao.map((type) => (
                          <option key={type.codigo} value={type.codigo}>
                            {type.nome}
                          </option>
                        ))}
                      </SelectField>

                      <TextInput
                        disabled={busy}
                        label="Valor"
                        onChange={(event) =>
                          setForm((current) => ({
                            ...current,
                            pagamentos: current.pagamentos.map((item, itemIndex) =>
                              itemIndex === index
                                ? { ...item, valor: event.target.value }
                                : item,
                            ),
                          }))
                        }
                        placeholder="0,00"
                        type="number"
                        value={payment.valor}
                      />
                    </div>

                    {payment.tipoLiquidacao === "meio_pagamento" ? (
                      <SelectField
                        disabled={busy}
                        label="Meio de pagamento"
                        onChange={(event) =>
                          setForm((current) => ({
                            ...current,
                            pagamentos: current.pagamentos.map((item, itemIndex) =>
                              itemIndex === index
                                ? { ...item, meioPagamentoId: event.target.value }
                                : item,
                            ),
                          }))
                        }
                        value={payment.meioPagamentoId}
                      >
                        <option value="">Selecione</option>
                        {workspace?.meiosPagamento.map((method) => (
                          <option key={method.id} value={method.id}>
                            {method.nome}
                          </option>
                        ))}
                      </SelectField>
                    ) : null}
                  </div>

                  <div style={{ alignSelf: "end" }}>
                    <Button
                      disabled={busy || form.pagamentos.length === 1}
                      onClick={() => onRemoveLine(index)}
                      type="button"
                      variant="ghost"
                    >
                      Remover
                    </Button>
                  </div>
                </div>
              ))}
            </div>

            <div className="split-fields">
              <TextInput
                disabled={busy}
                label="URL do comprovante"
                onChange={(event) =>
                  setForm((current) => ({
                    ...current,
                    comprovanteUrl: event.target.value,
                  }))
                }
                placeholder="Opcional"
                value={form.comprovanteUrl}
              />

              <div style={{ alignSelf: "end" }}>
                <Button disabled={busy} onClick={onAddLine} type="button" variant="ghost">
                  Adicionar forma
                </Button>
              </div>
            </div>

            <TextArea
              disabled={busy}
              label="Observacoes"
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  observacoes: event.target.value,
                }))
              }
              placeholder="Descreva a liquidacao."
              value={form.observacoes}
            />

            <Button disabled={busy} type="submit">
              Registrar liquidacao
            </Button>
          </form>
        )}
      </CardBody>
    </Card>
  );
}
