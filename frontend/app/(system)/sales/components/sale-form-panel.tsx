import type { Dispatch, FormEvent, SetStateAction } from "react";

import { Button } from "@/components/ui/button";
import { Card, CardBody, CardHeading } from "@/components/ui/card";
import { SelectField, TextArea, TextInput } from "@/components/ui/field";
import { StatusBadge } from "@/components/ui/status-badge";
import { formatCurrency } from "@/lib/helpers/formatters";
import type {
  SaleBuyerOption,
  SaleOption,
  SalePaymentMethodOption,
  SalePieceOption,
} from "@/lib/services/sales";

import {
  calculateSaleDraftTotals,
  getSelectedPaymentMethod,
  resolveSalePieceReference,
  type SaleFormState,
} from "./types";

type SaleFormPanelProps = {
  busy: boolean;
  canCreate: boolean;
  buyers: SaleBuyerOption[];
  form: SaleFormState;
  paymentMethods: SalePaymentMethodOption[];
  paymentTypes: SaleOption[];
  pieces: SalePieceOption[];
  onAddItem: () => void;
  onAddPayment: () => void;
  onRemoveItem: (itemId: string) => void;
  onRemovePayment: (paymentId: string) => void;
  onSubmit: (event: FormEvent<HTMLFormElement>) => void;
  setForm: Dispatch<SetStateAction<SaleFormState>>;
};

// Concentra o formulario de conclusao da venda com itens e pagamentos.
export function SaleFormPanel({
  busy,
  canCreate,
  buyers,
  form,
  paymentMethods,
  paymentTypes,
  pieces,
  onAddItem,
  onAddPayment,
  onRemoveItem,
  onRemovePayment,
  onSubmit,
  setForm,
}: SaleFormPanelProps) {
  const totals = calculateSaleDraftTotals(form, pieces, paymentMethods);

  return (
    <Card>
      <CardBody>
        <CardHeading
          subtitle="Monte a venda com itens, pagamentos e observacoes. A API valida saldo, credito e efeitos transacionais."
          title="Registrar venda"
        />

        <form className="section-stack" onSubmit={onSubmit}>
          <div className="split-fields">
            <SelectField
              label="Comprador"
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  compradorPessoaId: event.target.value,
                }))
              }
              value={form.compradorPessoaId}
            >
              <option value="">Selecione</option>
              {buyers.map((buyer) => (
                <option key={buyer.pessoaId} value={buyer.pessoaId}>
                  {buyer.nome}
                </option>
              ))}
            </SelectField>

            <TextArea
              label="Observacoes"
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  observacoes: event.target.value,
                }))
              }
              rows={3}
              value={form.observacoes}
            />
          </div>

          <div className="section-stack">
            <div className="stock-record-header">
              <div>
                <div className="selection-item-title">Itens da venda</div>
                <div className="record-item-copy">
                  Leia o codigo de barras ou digite o codigo interno no formato
                  ` PEC-xxxxxx ` para adicionar a peca.
                </div>
              </div>

              {canCreate ? (
                <Button disabled={busy} onClick={onAddItem} variant="soft">
                  Adicionar item
                </Button>
              ) : null}
            </div>

            <div className="stack-scroll">
              {form.itens.map((item, index) => {
                const piece = resolveSalePieceReference(
                  pieces,
                  item.identificadorPeca,
                );

                return (
                  <div className="selection-item" key={item.id}>
                    <div className="section-stack" style={{ width: "100%" }}>
                      <div className="split-fields">
                        <TextInput
                          label={`Peca ${index + 1}`}
                          onChange={(event) =>
                            setForm((current) => ({
                              ...current,
                              itens: current.itens.map((currentItem) =>
                                currentItem.id === item.id
                                  ? {
                                      ...currentItem,
                                      identificadorPeca: event.target.value,
                                    }
                                  : currentItem,
                              ),
                            }))
                          }
                          placeholder="Codigo de barras ou PEC-xxxxxx"
                          value={item.identificadorPeca}
                        />

                        <TextInput
                          label="Quantidade"
                          onChange={(event) =>
                            setForm((current) => ({
                              ...current,
                              itens: current.itens.map((currentItem) =>
                                currentItem.id === item.id
                                  ? {
                                      ...currentItem,
                                      quantidade: event.target.value,
                                    }
                                  : currentItem,
                              ),
                            }))
                          }
                          type="number"
                          value={item.quantidade}
                        />
                      </div>

                      <TextInput
                        label="Desconto unitario"
                        onChange={(event) =>
                          setForm((current) => ({
                            ...current,
                            itens: current.itens.map((currentItem) =>
                              currentItem.id === item.id
                                ? {
                                    ...currentItem,
                                    descontoUnitario: event.target.value,
                                  }
                                : currentItem,
                            ),
                          }))
                        }
                        step="0.01"
                        type="number"
                        value={item.descontoUnitario}
                      />

                      {piece ? (
                        <div className="record-tags">
                          <span className="record-tag">{piece.codigoInterno}</span>
                          <span className="record-tag">
                            {piece.codigoBarras || "Sem codigo de barras"}
                          </span>
                          <span className="record-tag">
                            {piece.produtoNome} • {piece.marca}
                          </span>
                          <span className="record-tag">
                            Estoque {piece.quantidadeAtual}
                          </span>
                          <span className="record-tag">
                            {formatCurrency(piece.precoVendaAtual)}
                          </span>
                          <StatusBadge value={piece.tipoPeca} />
                        </div>
                      ) : item.identificadorPeca.trim() ? (
                        <div className="empty-state">
                          Nenhuma peca encontrada para o identificador informado.
                        </div>
                      ) : null}

                      {form.itens.length > 1 ? (
                        <Button
                          disabled={busy || !canCreate}
                          onClick={() => onRemoveItem(item.id)}
                          variant="ghost"
                        >
                          Remover item
                        </Button>
                      ) : null}
                    </div>
                  </div>
                );
              })}
            </div>
          </div>

          <div className="section-stack">
            <div className="stock-record-header">
              <div>
                <div className="selection-item-title">Pagamentos</div>
                <div className="record-item-copy">
                  Misture meios financeiros e credito da loja quando permitido pela regra da peca.
                </div>
              </div>

              {canCreate ? (
                <Button disabled={busy} onClick={onAddPayment} variant="soft">
                  Adicionar pagamento
                </Button>
              ) : null}
            </div>

            <div className="stack-scroll">
              {form.pagamentos.map((payment, index) => {
                const paymentMethod = getSelectedPaymentMethod(
                  paymentMethods,
                  payment.meioPagamentoId,
                );

                return (
                  <div className="selection-item" key={payment.id}>
                    <div className="section-stack" style={{ width: "100%" }}>
                      <div className="split-fields">
                        <SelectField
                          label={`Pagamento ${index + 1}`}
                          onChange={(event) =>
                            setForm((current) => ({
                              ...current,
                              pagamentos: current.pagamentos.map((currentPayment) =>
                                currentPayment.id === payment.id
                                  ? {
                                      ...currentPayment,
                                      tipoPagamento: event.target.value,
                                      meioPagamentoId:
                                        event.target.value === "credito_loja"
                                          ? ""
                                          : currentPayment.meioPagamentoId,
                                    }
                                  : currentPayment,
                              ),
                            }))
                          }
                          value={payment.tipoPagamento}
                        >
                          {paymentTypes.map((paymentType) => (
                            <option
                              key={paymentType.codigo}
                              value={paymentType.codigo}
                            >
                              {paymentType.nome}
                            </option>
                          ))}
                        </SelectField>

                        <TextInput
                          label="Valor"
                          onChange={(event) =>
                            setForm((current) => ({
                              ...current,
                              pagamentos: current.pagamentos.map((currentPayment) =>
                                currentPayment.id === payment.id
                                  ? { ...currentPayment, valor: event.target.value }
                                  : currentPayment,
                              ),
                            }))
                          }
                          step="0.01"
                          type="number"
                          value={payment.valor}
                        />
                      </div>

                      {payment.tipoPagamento === "meio_pagamento" ? (
                        <SelectField
                          label="Meio de pagamento"
                          onChange={(event) =>
                            setForm((current) => ({
                              ...current,
                              pagamentos: current.pagamentos.map((currentPayment) =>
                                currentPayment.id === payment.id
                                  ? {
                                      ...currentPayment,
                                      meioPagamentoId: event.target.value,
                                    }
                                  : currentPayment,
                              ),
                            }))
                          }
                          value={payment.meioPagamentoId}
                        >
                          <option value="">Selecione</option>
                          {paymentMethods.map((method) => (
                            <option key={method.id} value={method.id}>
                              {method.nome}
                            </option>
                          ))}
                        </SelectField>
                      ) : null}

                      {paymentMethod ? (
                        <div className="record-tags">
                          <span className="record-tag">
                            {paymentMethod.tipoMeioPagamentoNome}
                          </span>
                          <span className="record-tag">
                            Taxa {paymentMethod.taxaPercentual}%
                          </span>
                          <span className="record-tag">
                            Prazo {paymentMethod.prazoRecebimentoDias} dias
                          </span>
                        </div>
                      ) : null}

                      {form.pagamentos.length > 1 ? (
                        <Button
                          disabled={busy || !canCreate}
                          onClick={() => onRemovePayment(payment.id)}
                          variant="ghost"
                        >
                          Remover pagamento
                        </Button>
                      ) : null}
                    </div>
                  </div>
                );
              })}
            </div>
          </div>

          <div className="record-tags">
            <span className="record-tag">
              Subtotal {formatCurrency(totals.subtotal)}
            </span>
            <span className="record-tag">
              Desconto {formatCurrency(totals.desconto)}
            </span>
            <span className="record-tag">
              Total venda {formatCurrency(totals.totalVenda)}
            </span>
            <span className="record-tag">
              Pagamentos {formatCurrency(totals.totalPagamentos)}
            </span>
            <span className="record-tag">Taxa {formatCurrency(totals.taxa)}</span>
            <span className="record-tag">
              Liquido {formatCurrency(totals.liquido)}
            </span>
          </div>

          <Button disabled={busy || !canCreate} type="submit">
            Concluir venda
          </Button>
        </form>
      </CardBody>
    </Card>
  );
}
