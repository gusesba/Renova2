import type { Dispatch, SetStateAction, SubmitEvent } from "react";

import { Button } from "@/components/ui/button";
import { Card, CardBody, CardHeading } from "@/components/ui/card";
import { TextInput } from "@/components/ui/field";

// Formulario de edicao dos dados do proprio usuario.
export type ProfileFormState = {
  nome: string;
  email: string;
  telefone: string;
};

type ProfileFormPanelProps = {
  busy: boolean;
  form: ProfileFormState;
  onSubmit: (event: SubmitEvent<HTMLFormElement>) => void;
  setForm: Dispatch<SetStateAction<ProfileFormState>>;
};

export function ProfileFormPanel({
  busy,
  form,
  onSubmit,
  setForm,
}: ProfileFormPanelProps) {
  return (
    <Card>
      <CardBody className="section-stack">
        <CardHeading
          subtitle="Atualize seus dados cadastrais. Esta tela edita apenas o usuario autenticado."
          title="Dados do perfil"
        />

        <form className="form-grid" onSubmit={onSubmit}>
          <TextInput
            label="Nome"
            onChange={(event) =>
              setForm((current) => ({ ...current, nome: event.target.value }))
            }
            value={form.nome}
          />
          <TextInput
            label="Email"
            onChange={(event) =>
              setForm((current) => ({ ...current, email: event.target.value }))
            }
            value={form.email}
          />
          <TextInput
            label="Telefone"
            onChange={(event) =>
              setForm((current) => ({
                ...current,
                telefone: event.target.value,
              }))
            }
            value={form.telefone}
          />

          <Button disabled={busy} type="submit">
            {busy ? "Salvando..." : "Salvar perfil"}
          </Button>
        </form>
      </CardBody>
    </Card>
  );
}
