import type { Dispatch, SubmitEvent, SetStateAction } from "react";

import { Button } from "@/components/ui/button";
import { Card, CardBody, CardHeading } from "@/components/ui/card";
import { TextArea, TextInput } from "@/components/ui/field";
import { groupPermissionsByModule } from "@/lib/helpers/group-permissions";
import type { AccessPermission, AccessRole } from "@/lib/services/renova-api";

// Painel de cargos com agrupamento visual de permissoes por modulo.
export type RoleFormState = {
  id: string;
  nome: string;
  descricao: string;
  permissaoIds: string[];
};

type RolesPanelProps = {
  busy: boolean;
  form: RoleFormState;
  permissions: AccessPermission[];
  roles: AccessRole[];
  setForm: Dispatch<SetStateAction<RoleFormState>>;
  onSubmit: (event: SubmitEvent<HTMLFormElement>) => void;
};

export function RolesPanel({
  busy,
  form,
  permissions,
  roles,
  setForm,
  onSubmit,
}: RolesPanelProps) {
  // O agrupamento evita repetir logica de ordenacao dentro do JSX.
  const permissionGroups = groupPermissionsByModule(permissions);

  return (
    <Card>
      <CardBody className="section-stack">
        <CardHeading
          subtitle="Crie cargos por loja e monte a matriz de permissoes sem misturar a logica da pagina."
          title="Cargos e permissoes"
        />

        <form className="form-grid" onSubmit={onSubmit}>
          <TextInput
            label="Nome do cargo"
            onChange={(event) =>
              setForm((current) => ({ ...current, nome: event.target.value }))
            }
            value={form.nome}
          />
          <TextArea
            label="Descricao"
            onChange={(event) =>
              setForm((current) => ({
                ...current,
                descricao: event.target.value,
              }))
            }
            value={form.descricao}
          />

          <div className="permissions-groups-scroll">
            {permissionGroups.map((group) => (
              <div key={group.modulo}>
                <div
                  className="ui-field-label"
                  style={{ marginBottom: "0.55rem" }}
                >
                  Modulo {group.modulo}
                </div>
                <div className="selection-grid">
                  {group.items.map((permission) => (
                    <label className="selection-item" key={permission.id}>
                      <input
                        checked={form.permissaoIds.includes(permission.id)}
                        onChange={(event) =>
                          setForm((current) => ({
                            ...current,
                            permissaoIds: event.target.checked
                              ? [...current.permissaoIds, permission.id]
                              : current.permissaoIds.filter(
                                  (id) => id !== permission.id,
                                ),
                          }))
                        }
                        type="checkbox"
                      />
                      <div>
                        <div className="selection-item-title">
                          {permission.nome}
                        </div>
                        <div className="selection-item-copy">
                          {permission.codigo}
                        </div>
                      </div>
                    </label>
                  ))}
                </div>
              </div>
            ))}
          </div>

          <Button disabled={busy} type="submit">
            {busy ? "Salvando..." : form.id ? "Salvar cargo" : "Criar cargo"}
          </Button>
        </form>

        <div className="record-list">
          {roles.length === 0 ? (
            <div className="empty-state">
              Nenhum cargo cadastrado para a loja ativa.
            </div>
          ) : (
            roles.map((role) => (
              <button
                className="record-item"
                key={role.id}
                onClick={() =>
                  setForm({
                    id: role.id,
                    nome: role.nome,
                    descricao: role.descricao,
                    permissaoIds: role.permissoes.map(
                      (permission) => permission.id,
                    ),
                  })
                }
                type="button"
              >
                <div className="selection-item-title">{role.nome}</div>
                <div className="record-item-copy">{role.descricao}</div>
                <div className="record-tags">
                  {role.permissoes.slice(0, 5).map((permission) => (
                    <span className="record-tag" key={permission.id}>
                      {permission.codigo}
                    </span>
                  ))}
                  {role.permissoes.length > 5 ? (
                    <span className="record-tag">
                      +{role.permissoes.length - 5}
                    </span>
                  ) : null}
                </div>
              </button>
            ))
          )}
        </div>
      </CardBody>
    </Card>
  );
}
