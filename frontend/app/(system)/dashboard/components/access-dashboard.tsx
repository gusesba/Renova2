"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useEffect, useState, type SubmitEvent } from "react";
import { toast } from "sonner";

import { useSystemSession } from "@/app/(system)/components/system-session-provider";
import { DashboardOverview } from "@/app/(system)/dashboard/components/dashboard-overview";
import {
  MembershipsPanel,
  type MembershipFormState,
} from "@/app/(system)/dashboard/components/memberships-panel";
import {
  RolesPanel,
  type RoleFormState,
} from "@/app/(system)/dashboard/components/roles-panel";
import {
  UsersPanel,
  type UserFormState,
} from "@/app/(system)/dashboard/components/users-panel";
import { Card, CardBody, CardHeading } from "@/components/ui/card";
import {
  createUserFormSchema,
  getZodErrorMessage,
  membershipFormSchema,
  roleFormSchema,
  userFormSchema,
} from "@/lib/helpers/access-schemas";
import { getErrorMessage } from "@/lib/helpers/formatters";
import { queryKeys } from "@/lib/helpers/query-keys";
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
} from "@/lib/services/access";

// Valores iniciais dos formularios para evitar estado parcial espalhado pela tela.
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

// Coordena a dashboard de acesso: leitura do workspace e mutacoes dos tres paineis.
export function AccessDashboard() {
  const { token, session } = useSystemSession();
  const queryClient = useQueryClient();
  const [userForm, setUserForm] = useState<UserFormState>(emptyUserForm);
  const [roleForm, setRoleForm] = useState<RoleFormState>(emptyRoleForm);
  const [membershipForm, setMembershipForm] =
    useState<MembershipFormState>(emptyMembershipForm);

  const workspaceQuery = useQuery({
    enabled: Boolean(session.lojaAtivaId && session.permissoes.length > 0),
    queryFn: () => loadAccessWorkspace(token),
    queryKey: queryKeys.accessWorkspace(token, session.lojaAtivaId),
  });

  // Sempre invalida pelo token + loja ativa para manter o cache coerente com a sessao.
  const refreshWorkspace = () =>
    queryClient.invalidateQueries({
      queryKey: queryKeys.accessWorkspace(token, session.lojaAtivaId),
    });

  const userMutation = useMutation({
    mutationFn: async () => {
      // Atualizacao e criacao compartilham o mesmo formulario, mas com schemas distintos.
      if (userForm.id) {
        const parsed = userFormSchema.safeParse(userForm);
        if (!parsed.success) {
          throw new Error(getZodErrorMessage(parsed.error));
        }

        return updateUser(token, userForm.id, {
          nome: parsed.data.nome,
          email: parsed.data.email,
          telefone: parsed.data.telefone,
          pessoaId: null,
        });
      }

      const parsed = createUserFormSchema.safeParse(userForm);
      if (!parsed.success) {
        throw new Error(getZodErrorMessage(parsed.error));
      }

      return createUser(token, {
        nome: parsed.data.nome,
        email: parsed.data.email,
        telefone: parsed.data.telefone,
        senha: parsed.data.senha,
        pessoaId: null,
      });
    },
    onError: (error) => {
      toast.error(getErrorMessage(error));
    },
    onSuccess: async () => {
      setUserForm(emptyUserForm);
      toast.success("Usuario salvo com sucesso.");
      await refreshWorkspace();
    },
  });

  const userStatusMutation = useMutation({
    mutationFn: async () => {
      const parsed = userFormSchema.safeParse(userForm);
      if (!parsed.success) {
        throw new Error(getZodErrorMessage(parsed.error));
      }

      if (!parsed.data.id) {
        throw new Error("Selecione um usuario antes de alterar o status.");
      }

      return changeUserStatus(token, parsed.data.id, parsed.data.statusUsuario);
    },
    onError: (error) => {
      toast.error(getErrorMessage(error));
    },
    onSuccess: async () => {
      toast.success("Status do usuario atualizado.");
      await refreshWorkspace();
    },
  });

  const roleMutation = useMutation({
    mutationFn: async () => {
      const parsed = roleFormSchema.safeParse(roleForm);
      if (!parsed.success) {
        throw new Error(getZodErrorMessage(parsed.error));
      }

      if (parsed.data.id) {
        await updateRole(token, parsed.data.id, {
          nome: parsed.data.nome,
          descricao: parsed.data.descricao,
          ativo: true,
        });

        return updateRolePermissions(
          token,
          parsed.data.id,
          parsed.data.permissaoIds,
        );
      }

      return createRole(token, {
        nome: parsed.data.nome,
        descricao: parsed.data.descricao,
        permissaoIds: parsed.data.permissaoIds,
      });
    },
    onError: (error) => {
      toast.error(getErrorMessage(error));
    },
    onSuccess: async () => {
      setRoleForm(emptyRoleForm);
      toast.success("Cargo salvo com sucesso.");
      await refreshWorkspace();
    },
  });

  const membershipMutation = useMutation({
    mutationFn: async () => {
      const parsed = membershipFormSchema.safeParse(membershipForm);
      if (!parsed.success) {
        throw new Error(getZodErrorMessage(parsed.error));
      }

      if (parsed.data.id) {
        return updateMembershipRoles(
          token,
          parsed.data.id,
          parsed.data.cargoIds,
        );
      }

      return createMembership(token, {
        usuarioId: parsed.data.usuarioId,
        statusVinculo: "ativo",
        ehResponsavel: false,
        dataFim: null,
        cargoIds: parsed.data.cargoIds,
      });
    },
    onError: (error) => {
      toast.error(getErrorMessage(error));
    },
    onSuccess: async () => {
      setMembershipForm(emptyMembershipForm);
      toast.success("Vinculo salvo com sucesso.");
      await refreshWorkspace();
    },
  });

  const workspace = workspaceQuery.data;
  const users = workspace?.users ?? [];
  const permissions = workspace?.permissions ?? [];
  const roles = workspace?.roles ?? [];
  const memberships = workspace?.memberships ?? [];

  const busy =
    workspaceQuery.isLoading ||
    userMutation.isPending ||
    userStatusMutation.isPending ||
    roleMutation.isPending ||
    membershipMutation.isPending;

  useEffect(() => {
    if (workspaceQuery.isError) {
      toast.error(getErrorMessage(workspaceQuery.error));
    }
  }, [workspaceQuery.error, workspaceQuery.isError]);

  if (session.lojas.length === 0) {
    return (
      <Card>
        <CardBody className="section-stack">
          <CardHeading
            subtitle="Sua conta ja foi criada, mas ainda nao existe vinculo com nenhuma loja."
            title="Acesso aguardando liberacao"
          />
          <div className="empty-state">
            Um responsavel precisa vincular sua conta a uma loja e atribuir os
            cargos necessarios antes de liberar o uso do sistema.
          </div>
        </CardBody>
      </Card>
    );
  }

  if (session.permissoes.length === 0) {
    return (
      <Card>
        <CardBody className="section-stack">
          <CardHeading
            subtitle="Sua conta esta autenticada, mas nao possui permissao para este modulo."
            title="Modulo sem permissao"
          />
          <div className="empty-state">
            Solicite a atribuicao de um cargo na loja ativa para acessar as
            funcionalidades administrativas.
          </div>
        </CardBody>
      </Card>
    );
  }

  async function handleUserSubmit(event: SubmitEvent<HTMLFormElement>) {
    event.preventDefault();
    await userMutation.mutateAsync();
  }

  async function handleUserStatusChange() {
    await userStatusMutation.mutateAsync();
  }

  async function handleRoleSubmit(event: SubmitEvent<HTMLFormElement>) {
    event.preventDefault();
    await roleMutation.mutateAsync();
  }

  async function handleMembershipSubmit(event: SubmitEvent<HTMLFormElement>) {
    event.preventDefault();
    await membershipMutation.mutateAsync();
  }

  return (
    <div className="dashboard-grid">
      <div className="dashboard-column" style={{ gridColumn: "1 / -1" }}>
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
