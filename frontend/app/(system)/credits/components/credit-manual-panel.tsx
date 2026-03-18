import { type FormEvent } from "react";

import { Button } from "@/components/ui/button";
import { Card, CardBody, CardHeading } from "@/components/ui/card";
import { SelectField, TextArea, TextInput } from "@/components/ui/field";
import type { CreditPersonOption } from "@/lib/services/credits";

import type { ManualCreditFormState } from "@/app/(system)/credits/components/types";

type CreditManualPanelProps = {
  busy: boolean;
  canManage: boolean;
  form: ManualCreditFormState;
  people: CreditPersonOption[];
  setForm: (updater: ManualCreditFormState | ((current: ManualCreditFormState) => ManualCreditFormState)) => void;
  onEnsureAccount: () => Promise<void>;
  onSubmit: (event: FormEvent<HTMLFormElement>) => Promise<void>;
};

// Reune as acoes de criacao de conta e credito manual do modulo.
export function CreditManualPanel({
  busy,
  canManage,
  form,
  people,
  setForm,
  onEnsureAccount,
  onSubmit,
}: CreditManualPanelProps) {
  return (
    <Card>
      <CardBody className="section-stack">
        <CardHeading
          subtitle="Garanta a conta unica da pessoa e lance creditos manuais com justificativa obrigatoria."
          title="Lancamentos manuais"
        />

        {!canManage ? (
          <div className="empty-state">
            Seu cargo pode consultar saldos e extratos, mas nao pode criar contas nem lancar creditos.
          </div>
        ) : (
          <form className="section-stack" onSubmit={onSubmit}>
            <SelectField
              disabled={busy}
              label="Pessoa"
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  pessoaId: event.target.value,
                }))
              }
              value={form.pessoaId}
            >
              <option value="">Selecione</option>
              {people.map((person) => (
                <option key={person.pessoaId} value={person.pessoaId}>
                  {person.nome}
                </option>
              ))}
            </SelectField>

            <div className="split-fields">
              <TextInput
                disabled={busy}
                label="Valor do credito"
                onChange={(event) =>
                  setForm((current) => ({
                    ...current,
                    valor: event.target.value,
                  }))
                }
                placeholder="0,00"
                type="number"
                value={form.valor}
              />

              <div style={{ alignSelf: "end" }}>
                <Button disabled={busy} onClick={() => void onEnsureAccount()} type="button" variant="ghost">
                  Garantir conta
                </Button>
              </div>
            </div>

            <TextArea
              disabled={busy}
              label="Justificativa"
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  justificativa: event.target.value,
                }))
              }
              placeholder="Explique a origem do credito manual."
              value={form.justificativa}
            />

            <Button disabled={busy} type="submit">
              Registrar credito manual
            </Button>
          </form>
        )}
      </CardBody>
    </Card>
  );
}
