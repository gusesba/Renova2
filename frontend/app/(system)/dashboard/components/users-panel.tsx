import type { Dispatch, SubmitEvent, SetStateAction } from "react";

import { Button } from "@/components/ui/button";
import { Card, CardBody, CardHeading } from "@/components/ui/card";
import { SelectField, TextInput } from "@/components/ui/field";
import { StatusBadge } from "@/components/ui/status-badge";
import type { AccessUser } from "@/lib/services/access";

// Painel de usuarios: formulario e lista de selecao usando apenas props do container.
export type UserFormState = {
  id: string;
  nome: string;
  email: string;
  telefone: string;
  senha: string;
  statusUsuario: string;
};

type UsersPanelProps = {
  busy: boolean;
  form: UserFormState;
  users: AccessUser[];
  setForm: Dispatch<SetStateAction<UserFormState>>;
  onSubmit: (event: SubmitEvent<HTMLFormElement>) => void;
  onStatusChange: () => void;
};

export function UsersPanel({
  busy,
  form,
  users,
  setForm,
  onSubmit,
  onStatusChange,
}: UsersPanelProps) {
  return (
    <Card>
      <CardBody className="section-stack">
        <CardHeading
          subtitle="Cadastro e manutencao de usuarios com selecao rapida por registro."
          title="Usuarios da plataforma"
        />

        <form className="form-grid" onSubmit={onSubmit}>
          <div className="split-fields">
            <TextInput
              label="Nome"
              onChange={(event) =>
                setForm((current) => ({ ...current, nome: event.target.value }))
              }
              value={form.nome}
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
          </div>
          <TextInput
            label="Email"
            onChange={(event) =>
              setForm((current) => ({ ...current, email: event.target.value }))
            }
            value={form.email}
          />
          {!form.id ? (
            <TextInput
              label="Senha inicial"
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  senha: event.target.value,
                }))
              }
              type="password"
              value={form.senha}
            />
          ) : null}
          <div className="split-fields">
            <SelectField
              label="Status"
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  statusUsuario: event.target.value,
                }))
              }
              value={form.statusUsuario}
            >
              <option value="ativo">Ativo</option>
              <option value="inativo">Inativo</option>
              <option value="bloqueado">Bloqueado</option>
            </SelectField>
            <div style={{ display: "grid", alignItems: "end" }}>
              <Button
                disabled={busy || !form.id}
                onClick={onStatusChange}
                variant="soft"
              >
                Atualizar status
              </Button>
            </div>
          </div>
          <Button disabled={busy} type="submit">
            {busy
              ? "Salvando..."
              : form.id
                ? "Salvar usuario"
                : "Criar usuario"}
          </Button>
        </form>

        <div className="record-list">
          {users.length === 0 ? (
            <div className="empty-state">
              Nenhum usuario encontrado na plataforma.
            </div>
          ) : (
            users.map((user) => (
              <button
                className="record-item"
                key={user.id}
                onClick={() =>
                  setForm({
                    id: user.id,
                    nome: user.nome,
                    email: user.email,
                    telefone: user.telefone,
                    senha: "",
                    statusUsuario: user.statusUsuario,
                  })
                }
                type="button"
              >
                <div
                  style={{
                    display: "flex",
                    justifyContent: "space-between",
                    gap: "1rem",
                  }}
                >
                  <div>
                    <div className="selection-item-title">{user.nome}</div>
                    <div className="record-item-copy">{user.email}</div>
                  </div>
                  <StatusBadge value={user.statusUsuario} />
                </div>
                <div className="record-tags">
                  {(user.vinculoLojaAtiva?.cargos ?? []).map((role) => (
                    <span className="record-tag" key={role.id}>
                      {role.nome}
                    </span>
                  ))}
                </div>
              </button>
            ))
          )}
        </div>
      </CardBody>
    </Card>
  );
}
