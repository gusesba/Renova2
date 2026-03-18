import type { Dispatch, SetStateAction } from "react";

import { Button } from "@/components/ui/button";
import { TextInput } from "@/components/ui/field";

import type {
  PieceDiscountBandFormState,
  PieceManualRuleFormState,
} from "./types";

// Edita a regra manual opcional da peca quando houver sobrescrita do cadastro padrao.
type PieceManualRuleEditorProps = {
  busy: boolean;
  onAddBand: () => void;
  rule: PieceManualRuleFormState;
  setRule: Dispatch<SetStateAction<PieceManualRuleFormState>>;
};

export function PieceManualRuleEditor({
  busy,
  onAddBand,
  rule,
  setRule,
}: PieceManualRuleEditorProps) {
  function setBands(value: SetStateAction<PieceDiscountBandFormState[]>) {
    setRule((current) => ({
      ...current,
      politicaDesconto:
        typeof value === "function" ? value(current.politicaDesconto) : value,
    }));
  }

  return (
    <div className="section-stack">
      <div className="rule-inline-header">
        <div>
          <div className="ui-card-title">Regra manual da peca</div>
          <p className="ui-card-subtitle">
            Use apenas quando a condicao desta entrada for diferente da loja e do
            fornecedor.
          </p>
        </div>
        <Button disabled={busy} onClick={onAddBand} type="button" variant="ghost">
          Adicionar faixa
        </Button>
      </div>

      <div className="split-fields">
        <TextInput
          disabled={busy}
          label="Repasse dinheiro"
          onChange={(event) =>
            setRule((current) => ({
              ...current,
              percentualRepasseDinheiro: event.target.value,
            }))
          }
          step="0.01"
          type="number"
          value={rule.percentualRepasseDinheiro}
        />
        <TextInput
          disabled={busy}
          label="Repasse credito"
          onChange={(event) =>
            setRule((current) => ({
              ...current,
              percentualRepasseCredito: event.target.value,
            }))
          }
          step="0.01"
          type="number"
          value={rule.percentualRepasseCredito}
        />
      </div>

      <div className="split-fields">
        <TextInput
          disabled={busy}
          label="Prazo maximo exposicao"
          onChange={(event) =>
            setRule((current) => ({
              ...current,
              tempoMaximoExposicaoDias: event.target.value,
            }))
          }
          type="number"
          value={rule.tempoMaximoExposicaoDias}
        />

        <label className="rule-toggle-card">
          <input
            checked={rule.permitePagamentoMisto}
            disabled={busy}
            onChange={(event) =>
              setRule((current) => ({
                ...current,
                permitePagamentoMisto: event.target.checked,
              }))
            }
            type="checkbox"
          />
          <div>
            <div className="selection-item-title">Permite pagamento misto</div>
            <div className="selection-item-copy">
              Mantem a excecao comercial aplicada so a esta peca.
            </div>
          </div>
        </label>
      </div>

      {rule.politicaDesconto.length === 0 ? (
        <div className="empty-state">
          Nenhuma faixa de desconto configurada para a regra manual.
        </div>
      ) : (
        <div className="rule-policy-list">
          {rule.politicaDesconto.map((band) => (
            <div className="rule-policy-row" key={band.id}>
              <TextInput
                disabled={busy}
                label="Dias minimos"
                onChange={(event) =>
                  setBands((current) =>
                    current.map((item) =>
                      item.id === band.id
                        ? { ...item, diasMinimos: event.target.value }
                        : item,
                    ),
                  )
                }
                type="number"
                value={band.diasMinimos}
              />
              <TextInput
                disabled={busy}
                label="Percentual desconto"
                onChange={(event) =>
                  setBands((current) =>
                    current.map((item) =>
                      item.id === band.id
                        ? { ...item, percentualDesconto: event.target.value }
                        : item,
                    ),
                  )
                }
                step="0.01"
                type="number"
                value={band.percentualDesconto}
              />
              <Button
                disabled={busy}
                onClick={() =>
                  setBands((current) => current.filter((item) => item.id !== band.id))
                }
                type="button"
                variant="soft"
              >
                Remover
              </Button>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
