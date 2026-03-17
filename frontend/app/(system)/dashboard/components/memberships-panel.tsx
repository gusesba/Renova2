import type { Dispatch, SubmitEvent, SetStateAction } from "react";

import { Button } from "@/components/ui/button";
import { Card, CardBody, CardHeading } from "@/components/ui/card";
import { SelectField } from "@/components/ui/field";
import { StatusBadge } from "@/components/ui/status-badge";
import type {
  AccessRole,
  AccessUser,
  StoreMembership,
} from "@/lib/services/renova-api";

// Painel de vinculos entre usuario, loja ativa e cargos atribuidos.
export type MembershipFormState = {
  id: string;
  usuarioId: string;
  cargoIds: string[];
};

type MembershipsPanelProps = {
  busy: boolean;
  form: MembershipFormState;
  users: AccessUser[];
  roles: AccessRole[];
  memberships: StoreMembership[];
  setForm: Dispatch<SetStateAction<MembershipFormState>>;
  onSubmit: (event: SubmitEvent<HTMLFormElement>) => void;
};

export function MembershipsPanel({
  busy,
  form,
  users,
  roles,
  memberships,
  setForm,
  onSubmit,
}: MembershipsPanelProps) {
  return (
    <Card>
      <CardBody className="section-stack">
        <CardHeading
          subtitle="Associe usuarios a loja ativa e mantenha a atribuicao de cargos por vinculo."
          title="Vinculos da loja"
        />

        <form className="form-grid" onSubmit={onSubmit}>
          <SelectField
            label="Usuario"
            onChange={(event) =>
              setForm((current) => ({
                ...current,
                usuarioId: event.target.value,
              }))
            }
            value={form.usuarioId}
          >
            <option value="">Selecione um usuario</option>
            {users.map((user) => (
              <option key={user.id} value={user.id}>
                {user.nome}
              </option>
            ))}
          </SelectField>

          <div className="selection-grid stack-scroll">
            {roles.map((role) => (
              <label className="selection-item" key={role.id}>
                <input
                  checked={form.cargoIds.includes(role.id)}
                  onChange={(event) =>
                    setForm((current) => ({
                      ...current,
                      cargoIds: event.target.checked
                        ? [...current.cargoIds, role.id]
                        : current.cargoIds.filter((id) => id !== role.id),
                    }))
                  }
                  type="checkbox"
                />
                <div>
                  <div className="selection-item-title">{role.nome}</div>
                  <div className="selection-item-copy">{role.descricao}</div>
                </div>
              </label>
            ))}
          </div>

          <Button disabled={busy} type="submit">
            {busy
              ? "Salvando..."
              : form.id
                ? "Atualizar vinculo"
                : "Criar vinculo"}
          </Button>
        </form>

        <div className="record-list">
          {memberships.length === 0 ? (
            <div className="empty-state">
              Nenhum vinculo encontrado para a loja ativa.
            </div>
          ) : (
            memberships.map((membership) => (
              <button
                className="record-item"
                key={membership.id}
                onClick={() =>
                  setForm({
                    id: membership.id,
                    usuarioId: membership.usuarioId,
                    cargoIds: membership.cargos.map((role) => role.id),
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
                    <div className="selection-item-title">
                      {membership.usuarioNome}
                    </div>
                    <div className="record-item-copy">
                      {membership.usuarioEmail}
                    </div>
                  </div>
                  <StatusBadge value={membership.statusVinculo} />
                </div>
                <div className="record-tags">
                  {membership.cargos.map((role) => (
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
