"use client";

import { startTransition, useEffect, useEffectEvent, useState, type FormEvent } from "react";

import { useSystemSession } from "@/app/(system)/components/system-session-provider";
import { DashboardOverview } from "@/app/(system)/dashboard/components/dashboard-overview";
import {
  MembershipsPanel,
  type MembershipFormState,
} from "@/app/(system)/dashboard/components/memberships-panel";
import { RolesPanel, type RoleFormState } from "@/app/(system)/dashboard/components/roles-panel";
import { UsersPanel, type UserFormState } from "@/app/(system)/dashboard/components/users-panel";
import { FeedbackBanner } from "@/components/ui/feedback-banner";
import { getErrorMessage } from "@/lib/helpers/formatters";
import {
  changeUserStatus,
  createMembership,
  createRole,
  createUser,
  loadAccessWorkspace,
  updateMembershipRoles,
  updateRole,
  updateRolePermissions,
  updateUser,
  type AccessPermission,
  type AccessRole,
  type AccessUser,
  type StoreMembership,
} from "@/lib/services/renova-api";

const emptyUserForm: UserFormState = {
  id: "",
  nome: "",
  email: "",
  telefone: "",
  senha: "",
  statusUsuario: "ativo",
};

const emptyRoleForm: RoleFormState = {
  id: "",
  nome: "",
  descricao: "",
  permissaoIds: [],
};

const emptyMembershipForm: MembershipFormState = {
  id: "",
  usuarioId: "",
  cargoIds: [],
};

export function AccessDashboard() {
  const { token, session } = useSystemSession();
  const [busy, setBusy] = useState(false);
  const [feedback, setFeedback] = useState<string | null>(null);
  const [users, setUsers] = useState<AccessUser[]>([]);
  const [permissions, setPermissions] = useState<AccessPermission[]>([]);
  const [roles, setRoles] = useState<AccessRole[]>([]);
  const [memberships, setMemberships] = useState<StoreMembership[]>([]);
  const [userForm, setUserForm] = useState<UserFormState>(emptyUserForm);
  const [roleForm, setRoleForm] = useState<RoleFormState>(emptyRoleForm);
  const [membershipForm, setMembershipForm] = useState<MembershipFormState>(emptyMembershipForm);

  async function reloadWorkspace() {
    setBusy(true);

    try {
      const workspace = await loadAccessWorkspace(token);
      startTransition(() => {
        setUsers(workspace.users);
        setPermissions(workspace.permissions);
        setRoles(workspace.roles);
        setMemberships(workspace.memberships);
      });
    } catch (error) {
      startTransition(() => {
        setFeedback(getErrorMessage(error));
      });
    } finally {
      setBusy(false);
    }
  }

  const hydrateWorkspace = useEffectEvent(async () => {
    await reloadWorkspace();
  });

  useEffect(() => {
    void hydrateWorkspace();
  }, [session.lojaAtivaId, token]);

  async function handleUserSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setBusy(true);

    try {
      if (userForm.id) {
        await updateUser(token, userForm.id, {
          nome: userForm.nome,
          email: userForm.email,
          telefone: userForm.telefone,
          pessoaId: null,
        });
      } else {
        await createUser(token, {
          nome: userForm.nome,
          email: userForm.email,
          telefone: userForm.telefone,
          senha: userForm.senha,
          pessoaId: null,
        });
      }

      startTransition(() => {
        setFeedback("Usuario salvo com sucesso.");
        setUserForm(emptyUserForm);
      });
      await reloadWorkspace();
    } catch (error) {
      startTransition(() => {
        setFeedback(getErrorMessage(error));
      });
    } finally {
      setBusy(false);
    }
  }

  async function handleUserStatusChange() {
    if (!userForm.id) {
      return;
    }

    setBusy(true);

    try {
      await changeUserStatus(token, userForm.id, userForm.statusUsuario);
      startTransition(() => {
        setFeedback("Status do usuario atualizado.");
      });
      await reloadWorkspace();
    } catch (error) {
      startTransition(() => {
        setFeedback(getErrorMessage(error));
      });
    } finally {
      setBusy(false);
    }
  }

  async function handleRoleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setBusy(true);

    try {
      if (roleForm.id) {
        await updateRole(token, roleForm.id, {
          nome: roleForm.nome,
          descricao: roleForm.descricao,
          ativo: true,
        });
        await updateRolePermissions(token, roleForm.id, roleForm.permissaoIds);
      } else {
        await createRole(token, {
          nome: roleForm.nome,
          descricao: roleForm.descricao,
          permissaoIds: roleForm.permissaoIds,
        });
      }

      startTransition(() => {
        setFeedback("Cargo salvo com sucesso.");
        setRoleForm(emptyRoleForm);
      });
      await reloadWorkspace();
    } catch (error) {
      startTransition(() => {
        setFeedback(getErrorMessage(error));
      });
    } finally {
      setBusy(false);
    }
  }

  async function handleMembershipSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setBusy(true);

    try {
      if (membershipForm.id) {
        await updateMembershipRoles(token, membershipForm.id, membershipForm.cargoIds);
      } else {
        await createMembership(token, {
          usuarioId: membershipForm.usuarioId,
          statusVinculo: "ativo",
          ehResponsavel: false,
          dataFim: null,
          cargoIds: membershipForm.cargoIds,
        });
      }

      startTransition(() => {
        setFeedback("Vinculo salvo com sucesso.");
        setMembershipForm(emptyMembershipForm);
      });
      await reloadWorkspace();
    } catch (error) {
      startTransition(() => {
        setFeedback(getErrorMessage(error));
      });
    } finally {
      setBusy(false);
    }
  }

  return (
    <div className="dashboard-grid">
      <div className="dashboard-column" style={{ gridColumn: "1 / -1" }}>
        {feedback ? <FeedbackBanner message={feedback} /> : null}
        <DashboardOverview
          membershipsCount={memberships.length}
          rolesCount={roles.length}
          session={session}
          usersCount={users.length}
        />
      </div>

      <div className="dashboard-column">
        <UsersPanel
          busy={busy}
          form={userForm}
          onStatusChange={handleUserStatusChange}
          onSubmit={handleUserSubmit}
          setForm={setUserForm}
          users={users}
        />
        <MembershipsPanel
          busy={busy}
          form={membershipForm}
          memberships={memberships}
          onSubmit={handleMembershipSubmit}
          roles={roles}
          setForm={setMembershipForm}
          users={users}
        />
      </div>

      <div className="dashboard-column">
        <RolesPanel
          busy={busy}
          form={roleForm}
          onSubmit={handleRoleSubmit}
          permissions={permissions}
          roles={roles}
          setForm={setRoleForm}
        />
      </div>
    </div>
  );
}
