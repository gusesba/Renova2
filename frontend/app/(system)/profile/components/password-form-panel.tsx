import type { Dispatch, SetStateAction, SubmitEvent } from "react";

import { Button } from "@/components/ui/button";
import { Card, CardBody, CardHeading } from "@/components/ui/card";
import { TextInput } from "@/components/ui/field";

// Formulario isolado para troca da senha do usuario autenticado.
export type PasswordFormState = {
  senhaAtual: string;
  novaSenha: string;
  confirmacaoNovaSenha: string;
};

type PasswordFormPanelProps = {
  busy: boolean;
  form: PasswordFormState;
  onSubmit: (event: SubmitEvent<HTMLFormElement>) => void;
  setForm: Dispatch<SetStateAction<PasswordFormState>>;
};

export function PasswordFormPanel({
  busy,
  form,
  onSubmit,
  setForm,
}: PasswordFormPanelProps) {
  return (
    <Card>
      <CardBody className="section-stack">
        <CardHeading
          subtitle="Informe a senha atual para definir uma nova senha de acesso."
          title="Trocar senha"
        />

        <form className="form-grid" onSubmit={onSubmit}>
          <TextInput
            label="Senha atual"
            onChange={(event) =>
              setForm((current) => ({
                ...current,
                senhaAtual: event.target.value,
              }))
            }
            type="password"
            value={form.senhaAtual}
          />
          <TextInput
            label="Nova senha"
            onChange={(event) =>
              setForm((current) => ({
                ...current,
                novaSenha: event.target.value,
              }))
            }
            type="password"
            value={form.novaSenha}
          />
          <TextInput
            label="Confirmar nova senha"
            onChange={(event) =>
              setForm((current) => ({
                ...current,
                confirmacaoNovaSenha: event.target.value,
              }))
            }
            type="password"
            value={form.confirmacaoNovaSenha}
          />

          <Button disabled={busy} type="submit">
            {busy ? "Alterando..." : "Alterar senha"}
          </Button>
        </form>
      </CardBody>
    </Card>
  );
}
