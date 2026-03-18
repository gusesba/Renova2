import type { Dispatch, SetStateAction } from "react";

import { Button } from "@/components/ui/button";
import { TextInput } from "@/components/ui/field";

import type { DiscountBandFormState } from "./types";

// Edita a politica de desconto em faixas de dias e percentual.
type DiscountPolicyEditorProps = {
  bands: DiscountBandFormState[];
  disabled?: boolean;
  setBands: Dispatch<SetStateAction<DiscountBandFormState[]>>;
  onAddBand: () => void;
};

export function DiscountPolicyEditor({
  bands,
  disabled = false,
  setBands,
  onAddBand,
}: DiscountPolicyEditorProps) {
  return (
    <div className="section-stack">
      <div className="rule-inline-header">
        <div>
          <div className="ui-card-title">Politica de desconto</div>
          <p className="ui-card-subtitle">
            Configure as faixas de desconto por tempo de exposicao.
          </p>
        </div>
        <Button disabled={disabled} onClick={onAddBand} type="button" variant="ghost">
          Adicionar faixa
        </Button>
      </div>

      {bands.length === 0 ? (
        <div className="empty-state">
          Nenhuma faixa configurada. Adicione apenas se a loja usar desconto por
          tempo de exposicao.
        </div>
      ) : (
        <div className="rule-policy-list">
          {bands.map((band) => (
            <div className="rule-policy-row" key={band.id}>
              <TextInput
                disabled={disabled}
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
                disabled={disabled}
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
                disabled={disabled}
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
