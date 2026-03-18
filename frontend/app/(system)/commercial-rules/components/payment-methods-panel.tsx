import type { Dispatch, FormEvent, SetStateAction } from "react";

import { Button } from "@/components/ui/button";
import { Card, CardBody, CardHeading } from "@/components/ui/card";
import { SelectField, TextInput } from "@/components/ui/field";
import { cx } from "@/lib/helpers/classnames";
import type {
  PaymentMethod,
  PaymentMethodTypeOption,
} from "@/lib/services/commercial-rules";

import type { PaymentMethodFormState } from "./types";

// Mantem os meios de pagamento da loja ativa no modulo comercial.
type PaymentMethodsPanelProps = {
  busy: boolean;
  form: PaymentMethodFormState;
  onNewMethod: () => void;
  onSelectMethod: (paymentMethodId: string) => void;
  onSubmit: (event: FormEvent<HTMLFormElement>) => void;
  paymentMethodTypes: PaymentMethodTypeOption[];
  paymentMethods: PaymentMethod[];
  selectedMethodId: string;
  setForm: Dispatch<SetStateAction<PaymentMethodFormState>>;
};

export function PaymentMethodsPanel({
  busy,
  form,
  onNewMethod,
  onSelectMethod,
  onSubmit,
  paymentMethodTypes,
  paymentMethods,
  selectedMethodId,
  setForm,
}: PaymentMethodsPanelProps) {
  return (
    <Card>
      <CardBody className="section-stack">
        <CardHeading
          subtitle="Define tipos, taxas e prazos que serao reaproveitados nas vendas e na conciliacao."
          title="Meios de pagamento"
        />

        <div className="rule-inline-header">
          <div>
            <div className="ui-card-title">Lista da loja ativa</div>
            <p className="ui-card-subtitle">
              Selecione um cadastro para editar ou abra um novo meio de pagamento.
            </p>
          </div>
          <Button disabled={busy} onClick={onNewMethod} type="button" variant="ghost">
            Novo meio
          </Button>
        </div>

        <div className="record-list">
          {paymentMethods.length === 0 ? (
            <div className="empty-state">
              Nenhum meio de pagamento cadastrado. Crie ao menos um para preparar o
              fluxo de vendas.
            </div>
          ) : (
            paymentMethods.map((method) => (
              <button
                className={cx(
                  "record-item",
                  selectedMethodId === method.id && "catalogs-entry-row-active",
                )}
                key={method.id}
                onClick={() => onSelectMethod(method.id)}
                type="button"
              >
                <div className="selection-item-title">{method.nome}</div>
                <div className="record-item-copy">
                  {method.tipoMeioPagamento} | taxa {method.taxaPercentual}% | prazo{" "}
                  {method.prazoRecebimentoDias} dias
                </div>
                <div className="record-tags">
                  <span className="record-tag">
                    {method.ativo ? "Ativo" : "Inativo"}
                  </span>
                </div>
              </button>
            ))
          )}
        </div>

        <form className="form-grid" onSubmit={onSubmit}>
          <TextInput
            disabled={busy}
            label="Nome"
            onChange={(event) =>
              setForm((current) => ({ ...current, nome: event.target.value }))
            }
            value={form.nome}
          />

          <div className="split-fields">
            <SelectField
              disabled={busy}
              label="Tipo"
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  tipoMeioPagamento: event.target.value,
                }))
              }
              value={form.tipoMeioPagamento}
            >
              <option value="">Selecione</option>
              {paymentMethodTypes.map((type) => (
                <option key={type.codigo} value={type.codigo}>
                  {type.nome}
                </option>
              ))}
            </SelectField>
            <TextInput
              disabled={busy}
              label="Taxa percentual"
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  taxaPercentual: event.target.value,
                }))
              }
              step="0.01"
              type="number"
              value={form.taxaPercentual}
            />
          </div>

          <div className="split-fields">
            <TextInput
              disabled={busy}
              label="Prazo recebimento (dias)"
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  prazoRecebimentoDias: event.target.value,
                }))
              }
              type="number"
              value={form.prazoRecebimentoDias}
            />

            <label className="rule-toggle-card">
              <input
                checked={form.ativo}
                disabled={busy}
                onChange={(event) =>
                  setForm((current) => ({
                    ...current,
                    ativo: event.target.checked,
                  }))
                }
                type="checkbox"
              />
              <div>
                <div className="selection-item-title">Cadastro ativo</div>
                <div className="selection-item-copy">
                  Permite uso futuro do meio de pagamento nas operacoes da loja.
                </div>
              </div>
            </label>
          </div>

          <Button disabled={busy} type="submit">
            {busy
              ? "Salvando..."
              : form.id
                ? "Salvar meio de pagamento"
                : "Criar meio de pagamento"}
          </Button>
        </form>
      </CardBody>
    </Card>
  );
}
