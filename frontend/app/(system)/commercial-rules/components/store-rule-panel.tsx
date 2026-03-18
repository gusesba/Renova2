import type { Dispatch, FormEvent, SetStateAction } from "react";

import { DiscountPolicyEditor } from "@/app/(system)/commercial-rules/components/discount-policy-editor";
import type {
  DiscountBandFormState,
  StoreRuleFormState,
} from "@/app/(system)/commercial-rules/components/types";
import { Button } from "@/components/ui/button";
import { Card, CardBody, CardHeading } from "@/components/ui/card";
import { TextInput } from "@/components/ui/field";

// Mantem a regra comercial padrao da loja ativa.
type StoreRulePanelProps = {
  busy: boolean;
  form: StoreRuleFormState;
  setForm: Dispatch<SetStateAction<StoreRuleFormState>>;
  onAddBand: () => void;
  onSubmit: (event: FormEvent<HTMLFormElement>) => void;
};

export function StoreRulePanel({
  busy,
  form,
  setForm,
  onAddBand,
  onSubmit,
}: StoreRulePanelProps) {
  function setBands(value: SetStateAction<DiscountBandFormState[]>) {
    setForm((current) => ({
      ...current,
      politicaDesconto:
        typeof value === "function" ? value(current.politicaDesconto) : value,
    }));
  }

  return (
    <Card>
      <CardBody className="section-stack">
        <CardHeading
          subtitle="Define a base de repasse, pagamento misto e prazo maximo para a loja ativa."
          title="Regra comercial da loja"
        />

        <form className="form-grid" onSubmit={onSubmit}>
          <div className="split-fields">
            <TextInput
              disabled={busy}
              label="Percentual repasse dinheiro"
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  percentualRepasseDinheiro: event.target.value,
                }))
              }
              step="0.01"
              type="number"
              value={form.percentualRepasseDinheiro}
            />
            <TextInput
              disabled={busy}
              label="Percentual repasse credito"
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  percentualRepasseCredito: event.target.value,
                }))
              }
              step="0.01"
              type="number"
              value={form.percentualRepasseCredito}
            />
          </div>

          <TextInput
            disabled={busy}
            label="Prazo maximo exposicao (dias)"
            onChange={(event) =>
              setForm((current) => ({
                ...current,
                tempoMaximoExposicaoDias: event.target.value,
              }))
            }
            type="number"
            value={form.tempoMaximoExposicaoDias}
          />

          <div className="rule-toggle-grid">
            <label className="rule-toggle-card">
              <input
                checked={form.permitePagamentoMisto}
                disabled={busy}
                onChange={(event) =>
                  setForm((current) => ({
                    ...current,
                    permitePagamentoMisto: event.target.checked,
                  }))
                }
                type="checkbox"
              />
              <div>
                <div className="selection-item-title">Permite pagamento misto</div>
                <div className="selection-item-copy">
                  Autoriza combinacao entre pagamento financeiro e credito.
                </div>
              </div>
            </label>

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
                <div className="selection-item-title">Regra ativa</div>
                <div className="selection-item-copy">
                  Mantem a regra disponivel para uso nas proximas pecas.
                </div>
              </div>
            </label>
          </div>

          <DiscountPolicyEditor
            bands={form.politicaDesconto}
            disabled={busy}
            onAddBand={onAddBand}
            setBands={setBands}
          />

          <Button disabled={busy} type="submit">
            {busy ? "Salvando..." : form.id ? "Salvar regra da loja" : "Criar regra da loja"}
          </Button>
        </form>
      </CardBody>
    </Card>
  );
}
