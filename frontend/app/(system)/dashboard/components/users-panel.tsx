import type { Dispatch, SubmitEvent, SetStateAction } from "react";

import { Button } from "@/components/ui/button";
import { Card, CardBody, CardHeading } from "@/components/ui/card";
import { TextInput } from "@/components/ui/field";
import { StatusBadge } from "@/components/ui/status-badge";
import type { AccessUser } from "@/lib/services/access";

// Painel de usuarios: formulario e lista de selecao usando apenas props do container.
export type UserFormState = {
  nome: string;
  email: string;
  telefone: string;
  senha: string;
};

type UsersPanelProps = {
  busy: boolean;
  form: UserFormState;
  users: AccessUser[];
  setForm: Dispatch<SetStateAction<UserFormState>>;
  onSubmit: (event: SubmitEvent<HTMLFormElement>) => void;
};

export function UsersPanel({
  busy,
  form,
  users,
  setForm,
  onSubmit,
}: UsersPanelProps) {
  return (
    <Card>
      <CardBody className="section-stack">
        <CardHeading
          subtitle="Crie novos usuarios da plataforma. A edicao do cadastro e feita apenas pelo proprio usuario."
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
          <Button disabled={busy} type="submit">
            {busy ? "Salvando..." : "Criar usuario"}
          </Button>
        </form>

        <div className="record-list">
          {users.length === 0 ? (
            <div className="empty-state">
              Nenhum usuario encontrado na plataforma.
            </div>
          ) : (
            users.map((user) => (
              <div className="record-item" key={user.id}>
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
              </div>
            ))
          )}
        </div>
      </CardBody>
    </Card>
  );
}
