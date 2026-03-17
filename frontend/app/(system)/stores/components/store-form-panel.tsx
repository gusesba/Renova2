import type { Dispatch, SetStateAction, SubmitEvent } from "react";

import { Button } from "@/components/ui/button";
import { Card, CardBody, CardHeading } from "@/components/ui/card";
import { SelectField, TextInput } from "@/components/ui/field";

// Formulario principal de cadastro e edicao da loja.
export type StoreFormState = {
  id: string;
  nomeFantasia: string;
  razaoSocial: string;
  documento: string;
  telefone: string;
  email: string;
  logradouro: string;
  numero: string;
  complemento: string;
  bairro: string;
  cidade: string;
  uf: string;
  cep: string;
  statusLoja: "ativa" | "inativa";
};

type StoreFormPanelProps = {
  busy: boolean;
  canManage: boolean;
  form: StoreFormState;
  onNewStore: () => void;
  onSubmit: (event: SubmitEvent<HTMLFormElement>) => void;
  setForm: Dispatch<SetStateAction<StoreFormState>>;
};

export function StoreFormPanel({
  busy,
  canManage,
  form,
  onNewStore,
  onSubmit,
  setForm,
}: StoreFormPanelProps) {
  return (
    <Card>
      <CardBody className="section-stack">
        <CardHeading
          subtitle="Mantem os dados cadastrais da loja e controla sua ativacao logica."
          title="Cadastro da loja"
        />

        <form className="form-grid" onSubmit={onSubmit}>
          <div className="split-fields">
            <TextInput
              disabled={!canManage || busy}
              label="Nome fantasia"
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  nomeFantasia: event.target.value,
                }))
              }
              value={form.nomeFantasia}
            />
            <TextInput
              disabled={!canManage || busy}
              label="Razao social"
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  razaoSocial: event.target.value,
                }))
              }
              value={form.razaoSocial}
            />
          </div>

          <div className="split-fields">
            <TextInput
              disabled={!canManage || busy}
              label="Documento"
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  documento: event.target.value,
                }))
              }
              value={form.documento}
            />
            <TextInput
              disabled={!canManage || busy}
              label="Telefone"
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  telefone: event.target.value,
                }))
              }
              value={form.telefone}
            />
          </div>

          <TextInput
            disabled={!canManage || busy}
            label="Email"
            onChange={(event) =>
              setForm((current) => ({
                ...current,
                email: event.target.value,
              }))
            }
            value={form.email}
          />

          <div className="split-fields">
            <TextInput
              disabled={!canManage || busy}
              label="Logradouro"
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  logradouro: event.target.value,
                }))
              }
              value={form.logradouro}
            />
            <TextInput
              disabled={!canManage || busy}
              label="Numero"
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  numero: event.target.value,
                }))
              }
              value={form.numero}
            />
          </div>

          <TextInput
            disabled={!canManage || busy}
            label="Complemento"
            onChange={(event) =>
              setForm((current) => ({
                ...current,
                complemento: event.target.value,
              }))
            }
            value={form.complemento}
          />

          <div className="split-fields">
            <TextInput
              disabled={!canManage || busy}
              label="Bairro"
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  bairro: event.target.value,
                }))
              }
              value={form.bairro}
            />
            <TextInput
              disabled={!canManage || busy}
              label="Cidade"
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  cidade: event.target.value,
                }))
              }
              value={form.cidade}
            />
          </div>

          <div className="split-fields">
            <TextInput
              disabled={!canManage || busy}
              label="UF"
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  uf: event.target.value,
                }))
              }
              value={form.uf}
            />
            <TextInput
              disabled={!canManage || busy}
              label="CEP"
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  cep: event.target.value,
                }))
              }
              value={form.cep}
            />
          </div>

          {form.id ? (
            <SelectField
              label="Status da loja"
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  statusLoja: event.target.value as "ativa" | "inativa",
                }))
              }
              value={form.statusLoja}
            >
              <option value="ativa">Ativa</option>
              <option value="inativa">Inativa</option>
            </SelectField>
          ) : null}

          <div className="split-fields">
            <Button disabled={!canManage || busy} type="submit">
              {busy
                ? "Salvando..."
                : form.id
                  ? "Salvar loja"
                  : "Criar loja"}
            </Button>
            <Button
              disabled={busy}
              onClick={onNewStore}
              type="button"
              variant="ghost"
            >
              Nova loja
            </Button>
          </div>
        </form>
      </CardBody>
    </Card>
  );
}
