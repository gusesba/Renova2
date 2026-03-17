import type { Dispatch, SetStateAction, SubmitEvent } from "react";

import { Button } from "@/components/ui/button";
import { Card, CardBody, CardHeading } from "@/components/ui/card";
import { TextArea, TextInput } from "@/components/ui/field";

// Formulario da configuracao operacional e de impressao da loja.
export type StoreConfigurationFormState = {
  nomeExibicao: string;
  cabecalhoImpressao: string;
  rodapeImpressao: string;
  usaModeloUnicoEtiqueta: boolean;
  usaModeloUnicoRecibo: boolean;
  fusoHorario: string;
  moeda: string;
};

type StoreConfigurationPanelProps = {
  busy: boolean;
  canManage: boolean;
  form: StoreConfigurationFormState;
  hasSelectedStore: boolean;
  onSubmit: (event: SubmitEvent<HTMLFormElement>) => void;
  setForm: Dispatch<SetStateAction<StoreConfigurationFormState>>;
};

export function StoreConfigurationPanel({
  busy,
  canManage,
  form,
  hasSelectedStore,
  onSubmit,
  setForm,
}: StoreConfigurationPanelProps) {
  return (
    <Card>
      <CardBody className="section-stack">
        <CardHeading
          subtitle="Controla identificacao, impressao e parametros operacionais da loja."
          title="Configuracao operacional"
        />

        {!hasSelectedStore ? (
          <div className="empty-state">
            Crie ou selecione uma loja para editar a configuracao operacional.
          </div>
        ) : (
          <form className="form-grid" onSubmit={onSubmit}>
            <TextInput
              disabled={!canManage || busy}
              label="Nome de exibicao"
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  nomeExibicao: event.target.value,
                }))
              }
              value={form.nomeExibicao}
            />

            <TextArea
              className="stack-scroll"
              label="Cabecalho de impressao"
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  cabecalhoImpressao: event.target.value,
                }))
              }
              value={form.cabecalhoImpressao}
            />

            <TextArea
              className="stack-scroll"
              label="Rodape de impressao"
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  rodapeImpressao: event.target.value,
                }))
              }
              value={form.rodapeImpressao}
            />

            <div className="split-fields">
              <TextInput
                disabled={!canManage || busy}
                label="Fuso horario"
                onChange={(event) =>
                  setForm((current) => ({
                    ...current,
                    fusoHorario: event.target.value,
                  }))
                }
                value={form.fusoHorario}
              />
              <TextInput
                disabled={!canManage || busy}
                label="Moeda"
                onChange={(event) =>
                  setForm((current) => ({
                    ...current,
                    moeda: event.target.value,
                  }))
                }
                value={form.moeda}
              />
            </div>

            <div className="selection-grid">
              <label className="selection-item">
                <input
                  checked={form.usaModeloUnicoEtiqueta}
                  disabled={!canManage || busy}
                  onChange={(event) =>
                    setForm((current) => ({
                      ...current,
                      usaModeloUnicoEtiqueta: event.target.checked,
                    }))
                  }
                  type="checkbox"
                />
                <div>
                  <div className="selection-item-title">
                    Modelo unico de etiqueta
                  </div>
                  <div className="selection-item-copy">
                    Mantem o padrao unico definido para impressao de etiqueta.
                  </div>
                </div>
              </label>

              <label className="selection-item">
                <input
                  checked={form.usaModeloUnicoRecibo}
                  disabled={!canManage || busy}
                  onChange={(event) =>
                    setForm((current) => ({
                      ...current,
                      usaModeloUnicoRecibo: event.target.checked,
                    }))
                  }
                  type="checkbox"
                />
                <div>
                  <div className="selection-item-title">
                    Modelo unico de recibo
                  </div>
                  <div className="selection-item-copy">
                    Mantem o padrao unico definido para recibos e comprovantes.
                  </div>
                </div>
              </label>
            </div>

            <Button disabled={!canManage || busy} type="submit">
              {busy ? "Salvando..." : "Salvar configuracao"}
            </Button>
          </form>
        )}
      </CardBody>
    </Card>
  );
}
