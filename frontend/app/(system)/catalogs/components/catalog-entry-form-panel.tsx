import type { Dispatch, SetStateAction, SubmitEvent } from "react";

import { Button } from "@/components/ui/button";
import { Card, CardBody, CardHeading } from "@/components/ui/card";
import { TextInput } from "@/components/ui/field";

import type { CatalogEntryFormState, CatalogEntryType } from "./types";

// Isola o formulario de criacao e edicao do cadastro selecionado.
type CatalogEntryFormPanelProps = {
  busy: boolean;
  form: CatalogEntryFormState;
  onSubmit: (event: SubmitEvent<HTMLFormElement>) => void;
  selectedType: CatalogEntryType;
  setForm: Dispatch<SetStateAction<CatalogEntryFormState>>;
};

const titlesByType: Record<CatalogEntryType, string> = {
  produtoNome: "Cadastro de produto",
  marca: "Cadastro de marca",
  tamanho: "Cadastro de tamanho",
  cor: "Cadastro de cor",
};

export function CatalogEntryFormPanel({
  busy,
  form,
  onSubmit,
  selectedType,
  setForm,
}: CatalogEntryFormPanelProps) {
  return (
    <Card className="catalogs-form-card">
      <CardBody className="catalogs-form-body">
        <CardHeading
          subtitle="Edite o nome base do grupo selecionado."
          title={titlesByType[selectedType]}
        />

        <form className="form-grid" onSubmit={onSubmit}>
          <TextInput
            disabled={busy}
            label="Nome"
            onChange={(event) =>
              setForm((current) => ({ ...current, nome: event.target.value }))
            }
            value={form.nome}
          />

          <Button disabled={busy} type="submit">
            {busy ? "Salvando..." : form.id ? "Salvar cadastro" : "Criar cadastro"}
          </Button>
        </form>
      </CardBody>
    </Card>
  );
}
