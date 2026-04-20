"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useDeferredValue, useMemo, useState } from "react";
import { toast } from "sonner";

import { SearchableSelect } from "@/app/components/ui/searchable-select";
import { Select } from "@/app/components/ui/select";
import { useStoreContext } from "@/app/dashboard/store-context";
import {
  asEmployeeListResponse,
  asRoleFunctionalityList,
  asRoleListResponse,
  extractAccessApiMessage,
  getAuthToken,
  permissions,
  requiredPermissionDependencies,
  type EmployeeListItem,
  type PermissionKey,
  type RoleFunctionality,
  type RoleItem,
} from "@/lib/access";
import {
  createEmployee,
  createRole,
  deleteEmployee,
  deleteRole,
  getEmployees,
  getRoleFunctionalities,
  getRoles,
  updateEmployeeRole,
  updateRole,
} from "@/services/access-service";
import { getUserOptions } from "@/services/user-service";

type UserOption = { id: number; nome: string; email: string };
type RoleDraft = { nome: string; funcionalidadeIds: number[] };

function EmptyState({ message }: { message: string }) {
  return (
    <div className="rounded-[28px] border border-dashed border-[var(--border-strong)] bg-white/80 px-6 py-10 text-center">
      <p className="text-base font-semibold text-[var(--foreground)]">Controle de acesso</p>
      <p className="mt-2 text-sm text-[var(--muted)]">{message}</p>
    </div>
  );
}

function SectionBlocked({ title, description }: { title: string; description: string }) {
  return (
    <div className="rounded-[24px] border border-dashed border-[var(--border)] bg-[var(--surface-muted)]/60 px-5 py-6">
      <h3 className="text-base font-semibold text-[var(--foreground)]">{title}</h3>
      <p className="mt-2 text-sm text-[var(--muted)]">{description}</p>
    </div>
  );
}

function getGroupedFunctionalities(items: RoleFunctionality[]) {
  return Object.entries(
    items.reduce<Record<string, RoleFunctionality[]>>((acc, item) => {
      acc[item.grupo] ??= [];
      acc[item.grupo].push(item);
      return acc;
    }, {}),
  ).sort((a, b) => a[0].localeCompare(b[0]));
}

function collectDependencyIds(
  functionalityKey: PermissionKey,
  functionalityIdByKey: Map<PermissionKey, number>,
  visited = new Set<PermissionKey>(),
): number[] {
  if (visited.has(functionalityKey)) return [];
  visited.add(functionalityKey);

  const dependencyKeys = requiredPermissionDependencies[functionalityKey] ?? [];

  return dependencyKeys.flatMap((dependencyKey) => {
    const dependencyId = functionalityIdByKey.get(dependencyKey);
    if (!dependencyId) return [];
    return [dependencyId, ...collectDependencyIds(dependencyKey, functionalityIdByKey, visited)];
  });
}

function collectDependentIds(
  functionalityKey: PermissionKey,
  reverseDependencyKeys: Map<PermissionKey, PermissionKey[]>,
  functionalityIdByKey: Map<PermissionKey, number>,
  visited = new Set<PermissionKey>(),
): number[] {
  if (visited.has(functionalityKey)) return [];
  visited.add(functionalityKey);

  const dependentKeys = reverseDependencyKeys.get(functionalityKey) ?? [];

  return dependentKeys.flatMap((dependentKey) => {
    const dependentId = functionalityIdByKey.get(dependentKey);
    if (!dependentId) return [];
    return [dependentId, ...collectDependentIds(dependentKey, reverseDependencyKeys, functionalityIdByKey, visited)];
  });
}

export function AccessControl() {
  const { selectedStore } = useStoreContext();
  return <AccessControlContent key={selectedStore?.id ?? "no-store"} selectedStore={selectedStore} />;
}

function AccessControlContent({
  selectedStore,
}: {
  selectedStore: { id: number; nome: string } | null;
}) {
  const queryClient = useQueryClient();
  const { hasPermission } = useStoreContext();
  const token = useMemo(() => (typeof window === "undefined" ? null : getAuthToken()), []);
  const [userSearch, setUserSearch] = useState("");
  const [selectedUser, setSelectedUser] = useState<UserOption | null>(null);
  const [selectedCargoId, setSelectedCargoId] = useState("");
  const [editingRoleId, setEditingRoleId] = useState<number | null>(null);
  const [roleDraft, setRoleDraft] = useState<RoleDraft>({ nome: "", funcionalidadeIds: [] });
  const deferredUserSearch = useDeferredValue(userSearch);
  const trimmedUserSearch = deferredUserSearch.trim();

  const canViewEmployees = hasPermission(permissions.funcionariosVisualizar);
  const canAddEmployees = hasPermission(permissions.funcionariosAdicionar);
  const canEditEmployees = hasPermission(permissions.funcionariosEditar);
  const canRemoveEmployees = hasPermission(permissions.funcionariosRemover);
  const canViewRoles = hasPermission(permissions.cargosVisualizar);
  const canAddRoles = hasPermission(permissions.cargosAdicionar);
  const canEditRoles = hasPermission(permissions.cargosEditar);
  const canDeleteRoles = hasPermission(permissions.cargosExcluir);
  const canManageRoles = canViewRoles || canAddRoles || canEditRoles || canDeleteRoles;
  const canOpenPage =
    canViewEmployees || canAddEmployees || canEditEmployees || canRemoveEmployees || canManageRoles;

  const employeesQuery = useQuery({
    queryKey: ["employees", selectedStore?.id, token],
    queryFn: async () => {
      const response = await getEmployees(selectedStore!.id, token!);
      if (!response.ok) throw new Error(extractAccessApiMessage(response.body) ?? "Nao foi possivel carregar os funcionarios.");
      return asEmployeeListResponse(response.body);
    },
    enabled: Boolean(selectedStore && token && canViewEmployees),
  });

  const rolesQuery = useQuery({
    queryKey: ["roles", selectedStore?.id, token],
    queryFn: async () => {
      const response = await getRoles(selectedStore!.id, token!);
      if (!response.ok) throw new Error(extractAccessApiMessage(response.body) ?? "Nao foi possivel carregar os cargos.");
      return asRoleListResponse(response.body);
    },
    enabled: Boolean(selectedStore && token && (canViewRoles || canAddEmployees || canEditEmployees)),
  });

  const functionalitiesQuery = useQuery({
    queryKey: ["role-functionalities", selectedStore?.id, token],
    queryFn: async () => {
      const response = await getRoleFunctionalities(selectedStore!.id, token!);
      if (!response.ok) throw new Error(extractAccessApiMessage(response.body) ?? "Nao foi possivel carregar as funcionalidades.");
      return asRoleFunctionalityList(response.body);
    },
    enabled: Boolean(selectedStore && token && (canViewRoles || canAddRoles || canEditRoles)),
  });

  const userOptionsQuery = useQuery({
    queryKey: ["employee-user-options", selectedStore?.id, trimmedUserSearch, token],
    queryFn: async () => {
      const response = await getUserOptions(token!, trimmedUserSearch, selectedStore?.id);
      if (!response.ok) throw new Error("Nao foi possivel carregar os usuarios.");
      return response.body as UserOption[];
    },
    enabled: Boolean(token && selectedStore && canAddEmployees && trimmedUserSearch),
  });

  const createEmployeeMutation = useMutation({
    mutationFn: ({ storeId, userId, cargoId, tokenValue }: { storeId: number; userId: number; cargoId: number; tokenValue: string }) =>
      createEmployee(storeId, { usuarioId: userId, cargoId }, tokenValue),
  });

  const updateEmployeeRoleMutation = useMutation({
    mutationFn: ({ storeId, userId, cargoId, tokenValue }: { storeId: number; userId: number; cargoId: number; tokenValue: string }) =>
      updateEmployeeRole(storeId, userId, { cargoId }, tokenValue),
  });

  const deleteEmployeeMutation = useMutation({
    mutationFn: ({ storeId, userId, tokenValue }: { storeId: number; userId: number; tokenValue: string }) =>
      deleteEmployee(storeId, userId, tokenValue),
  });

  const createRoleMutation = useMutation({
    mutationFn: ({ storeId, nome, funcionalidadeIds, tokenValue }: { storeId: number; nome: string; funcionalidadeIds: number[]; tokenValue: string }) =>
      createRole(storeId, { nome, funcionalidadeIds }, tokenValue),
  });

  const updateRoleMutation = useMutation({
    mutationFn: ({ storeId, roleId, nome, funcionalidadeIds, tokenValue }: { storeId: number; roleId: number; nome: string; funcionalidadeIds: number[]; tokenValue: string }) =>
      updateRole(storeId, roleId, { nome, funcionalidadeIds }, tokenValue),
  });

  const deleteRoleMutation = useMutation({
    mutationFn: ({ storeId, roleId, tokenValue }: { storeId: number; roleId: number; tokenValue: string }) =>
      deleteRole(storeId, roleId, tokenValue),
  });

  const employeeIds = useMemo(() => new Set((employeesQuery.data ?? []).map((item) => item.usuarioId)), [employeesQuery.data]);
  const availableUsers = useMemo(
    () =>
      (userOptionsQuery.data ?? [])
        .filter((user) => !employeeIds.has(user.id))
        .map((user) => ({ label: `${user.nome} - ${user.email}`, value: String(user.id) })),
    [employeeIds, userOptionsQuery.data],
  );
  const roleOptions = useMemo(
    () => (rolesQuery.data ?? []).map((role) => ({ label: `${role.nome} (${role.quantidadeFuncionarios})`, value: String(role.id) })),
    [rolesQuery.data],
  );
  const groupedFunctionalities = useMemo(() => getGroupedFunctionalities(functionalitiesQuery.data ?? []), [functionalitiesQuery.data]);
  const functionalityIdByKey = useMemo(
    () =>
      new Map(
        (functionalitiesQuery.data ?? []).map((item) => [item.chave, item.id] as const),
      ),
    [functionalitiesQuery.data],
  );
  const functionalityKeyById = useMemo(
    () =>
      new Map(
        (functionalitiesQuery.data ?? []).map((item) => [item.id, item.chave] as const),
      ),
    [functionalitiesQuery.data],
  );
  const reverseDependencyKeys = useMemo(() => {
    const entries = Object.entries(requiredPermissionDependencies) as [PermissionKey, PermissionKey[]][];
    const reverseMap = new Map<PermissionKey, PermissionKey[]>();

    entries.forEach(([functionalityKey, dependencyKeys]) => {
      dependencyKeys.forEach((dependencyKey) => {
        reverseMap.set(dependencyKey, [...(reverseMap.get(dependencyKey) ?? []), functionalityKey]);
      });
    });

    return reverseMap;
  }, []);
  const selectedFunctionalityIds = useMemo(() => new Set(roleDraft.funcionalidadeIds), [roleDraft.funcionalidadeIds]);

  async function invalidateAccess() {
    if (!selectedStore || !token) return;
    await Promise.all([
      queryClient.invalidateQueries({ queryKey: ["employees", selectedStore.id, token] }),
      queryClient.invalidateQueries({ queryKey: ["roles", selectedStore.id, token] }),
      queryClient.invalidateQueries({ queryKey: ["store-access-profile", token, selectedStore.id] }),
    ]);
  }

  function resetRoleDraft() {
    setEditingRoleId(null);
    setRoleDraft({ nome: "", funcionalidadeIds: [] });
  }

  function startEditingRole(role: RoleItem) {
    setEditingRoleId(role.id);
    setRoleDraft({ nome: role.nome, funcionalidadeIds: role.funcionalidades.map((item) => item.id) });
  }

  function toggleFunctionality(id: number) {
    const functionalityKey = functionalityKeyById.get(id);
    if (!functionalityKey) return;

    setRoleDraft((current) => {
      const nextIds = new Set(current.funcionalidadeIds);

      if (nextIds.has(id)) {
        nextIds.delete(id);
        collectDependentIds(functionalityKey, reverseDependencyKeys, functionalityIdByKey).forEach((dependentId) => {
          nextIds.delete(dependentId);
        });
      } else {
        nextIds.add(id);
        collectDependencyIds(functionalityKey, functionalityIdByKey).forEach((dependencyId) => {
          nextIds.add(dependencyId);
        });
      }

      return {
        ...current,
        funcionalidadeIds: [...nextIds],
      };
    });
  }

  async function handleAddEmployee() {
    if (!selectedStore || !token || !selectedUser || !selectedCargoId) {
      toast.error("Selecione usuario e cargo antes de adicionar o funcionario.");
      return;
    }

    try {
      const response = await createEmployeeMutation.mutateAsync({
        storeId: selectedStore.id,
        userId: selectedUser.id,
        cargoId: Number(selectedCargoId),
        tokenValue: token,
      });
      if (!response.ok) {
        toast.error(extractAccessApiMessage(response.body) ?? "Nao foi possivel adicionar o funcionario.");
        return;
      }
      setSelectedUser(null);
      setSelectedCargoId("");
      setUserSearch("");
      await invalidateAccess();
      toast.success(`Usuario ${selectedUser.nome} adicionado com sucesso.`);
    } catch {
      toast.error("Nao foi possivel conectar ao backend.");
    }
  }

  async function handleChangeEmployeeRole(employee: EmployeeListItem, cargoId: string) {
    if (!selectedStore || !token || !cargoId || Number(cargoId) === employee.cargoId) return;
    try {
      const response = await updateEmployeeRoleMutation.mutateAsync({
        storeId: selectedStore.id,
        userId: employee.usuarioId,
        cargoId: Number(cargoId),
        tokenValue: token,
      });
      if (!response.ok) {
        toast.error(extractAccessApiMessage(response.body) ?? "Nao foi possivel atualizar o cargo.");
        return;
      }
      await invalidateAccess();
      toast.success(`Cargo de ${employee.nome} atualizado.`);
    } catch {
      toast.error("Nao foi possivel conectar ao backend.");
    }
  }

  async function handleDeleteEmployee(employee: EmployeeListItem) {
    if (!selectedStore || !token) return;
    try {
      const response = await deleteEmployeeMutation.mutateAsync({
        storeId: selectedStore.id,
        userId: employee.usuarioId,
        tokenValue: token,
      });
      if (!response.ok) {
        toast.error(extractAccessApiMessage(response.body) ?? "Nao foi possivel remover o funcionario.");
        return;
      }
      await invalidateAccess();
      toast.success(`Usuario ${employee.nome} removido da loja.`);
    } catch {
      toast.error("Nao foi possivel conectar ao backend.");
    }
  }

  async function handleSaveRole() {
    if (!selectedStore || !token) return;
    if (!roleDraft.nome.trim()) {
      toast.error("Informe o nome do cargo.");
      return;
    }
    if (roleDraft.funcionalidadeIds.length === 0) {
      toast.error("Selecione ao menos uma funcionalidade.");
      return;
    }

    try {
      const response = editingRoleId === null
        ? await createRoleMutation.mutateAsync({
            storeId: selectedStore.id,
            nome: roleDraft.nome.trim(),
            funcionalidadeIds: roleDraft.funcionalidadeIds,
            tokenValue: token,
          })
        : await updateRoleMutation.mutateAsync({
            storeId: selectedStore.id,
            roleId: editingRoleId,
            nome: roleDraft.nome.trim(),
            funcionalidadeIds: roleDraft.funcionalidadeIds,
            tokenValue: token,
          });

      if (!response.ok) {
        toast.error(extractAccessApiMessage(response.body) ?? "Nao foi possivel salvar o cargo.");
        return;
      }

      resetRoleDraft();
      await invalidateAccess();
      toast.success(editingRoleId === null ? "Cargo criado com sucesso." : "Cargo atualizado com sucesso.");
    } catch {
      toast.error("Nao foi possivel conectar ao backend.");
    }
  }

  async function handleDeleteRole(role: RoleItem) {
    if (!selectedStore || !token) return;
    try {
      const response = await deleteRoleMutation.mutateAsync({
        storeId: selectedStore.id,
        roleId: role.id,
        tokenValue: token,
      });
      if (!response.ok) {
        toast.error(extractAccessApiMessage(response.body) ?? "Nao foi possivel excluir o cargo.");
        return;
      }
      if (editingRoleId === role.id) resetRoleDraft();
      await invalidateAccess();
      toast.success(`Cargo ${role.nome} removido.`);
    } catch {
      toast.error("Nao foi possivel conectar ao backend.");
    }
  }

  if (!selectedStore) return <EmptyState message="Selecione uma loja no topo da pagina para gerenciar cargos e funcionarios." />;
  if (!canOpenPage) return <EmptyState message="Seu usuario nao possui permissao para gerenciar controle de acesso nesta loja." />;

  return (
    <section className="space-y-6">
      <div className="rounded-[30px] border border-[var(--border)] bg-[linear-gradient(135deg,_#fffef9,_#f4f7ff_50%,_#eef6f1)] p-6">
        <div className="flex flex-col gap-4 lg:flex-row lg:items-end lg:justify-between">
          <div className="space-y-2">
            <span className="inline-flex rounded-full bg-[#eef4ea] px-3 py-1 text-xs font-semibold uppercase tracking-[0.22em] text-[#52624d]">Loja ativa</span>
            <div>
              <h1 className="text-3xl font-semibold tracking-tight text-[var(--foreground)]">Controle de acesso</h1>
              <p className="mt-2 max-w-3xl text-sm text-[var(--muted)]">
                Configure cargos e funcionalidades para a loja <span className="font-semibold text-[var(--foreground)]">{selectedStore.nome}</span>.
              </p>
            </div>
          </div>
          <div className="grid gap-4 sm:grid-cols-2">
            <div className="rounded-3xl border border-white/70 bg-white/75 px-5 py-4">
              <p className="text-xs font-semibold uppercase tracking-[0.2em] text-[var(--muted)]">Funcionarios</p>
              <p className="mt-1 text-3xl font-semibold text-[var(--foreground)]">{employeesQuery.data?.length ?? 0}</p>
            </div>
            <div className="rounded-3xl border border-white/70 bg-white/75 px-5 py-4">
              <p className="text-xs font-semibold uppercase tracking-[0.2em] text-[var(--muted)]">Cargos</p>
              <p className="mt-1 text-3xl font-semibold text-[var(--foreground)]">{rolesQuery.data?.length ?? 0}</p>
            </div>
          </div>
        </div>
      </div>

      <div className="grid gap-6 xl:grid-cols-[minmax(0,420px)_minmax(0,1fr)]">
        <section className="rounded-[28px] border border-[var(--border)] bg-white p-6">
          {canAddEmployees ? (
            <div className="space-y-4">
              <div>
                <h2 className="text-lg font-semibold text-[var(--foreground)]">Adicionar funcionario</h2>
                <p className="text-sm text-[var(--muted)]">Selecione um usuario e vincule um cargo.</p>
              </div>
              <div className="space-y-2">
                <label className="text-sm font-medium text-[var(--foreground)]">Usuario</label>
                <SearchableSelect
                  ariaLabel="Selecionar usuario"
                  value={selectedUser ? String(selectedUser.id) : null}
                  selectedLabel={selectedUser ? `${selectedUser.nome} - ${selectedUser.email}` : undefined}
                  searchValue={userSearch}
                  searchPlaceholder="Buscar por nome ou e-mail"
                  placeholder="Escolha um usuario"
                  loading={Boolean(trimmedUserSearch) && userOptionsQuery.isLoading}
                  emptyLabel={
                    !trimmedUserSearch
                      ? "Digite para buscar usuarios."
                      : "Nenhum usuario disponivel"
                  }
                  options={trimmedUserSearch ? availableUsers : []}
                  onSearchChange={setUserSearch}
                  onChange={(option) => {
                    const user = (userOptionsQuery.data ?? []).find((item) => item.id === Number(option.value));
                    if (user) {
                      setSelectedUser(user);
                      setUserSearch("");
                    }
                  }}
                />
              </div>
              <label className="space-y-2">
                <span className="text-sm font-medium text-[var(--foreground)]">Cargo</span>
                <div className="rounded-2xl border border-[var(--border)] bg-white px-4 py-3 text-sm text-[var(--foreground)]">
                  <Select
                    ariaLabel="Selecionar cargo para novo funcionario"
                    value={selectedCargoId}
                    options={roleOptions}
                    placeholder="Selecione um cargo"
                    onChange={setSelectedCargoId}
                  />
                </div>
              </label>
              <button
                type="button"
                onClick={handleAddEmployee}
                disabled={createEmployeeMutation.isPending || roleOptions.length === 0}
                className="mt-2 inline-flex h-12 w-full items-center justify-center rounded-2xl bg-[#294d44] px-4 text-sm font-semibold text-white disabled:opacity-60"
              >
                {createEmployeeMutation.isPending ? "Adicionando..." : "Adicionar funcionario"}
              </button>
            </div>
          ) : (
            <SectionBlocked title="Adicionar funcionario" description="Seu usuario nao possui permissao para vincular novos funcionarios." />
          )}
        </section>

        <section className="rounded-[28px] border border-[var(--border)] bg-white p-6">
          {canViewEmployees ? (
            <div className="space-y-4">
              <div>
                <h2 className="text-lg font-semibold text-[var(--foreground)]">Funcionarios da loja</h2>
                <p className="text-sm text-[var(--muted)]">O acesso de cada funcionario e definido pelo cargo.</p>
              </div>
              <div className="rounded-[24px] border border-[var(--border)]">
                <div className="overflow-x-auto overflow-y-visible rounded-[24px]">
                  <table className="min-w-full border-collapse">
                  <thead className="bg-[var(--surface-muted)]">
                    <tr className="text-left text-xs uppercase tracking-[0.18em] text-[var(--muted)]">
                      <th className="px-4 py-4 font-semibold">Usuario</th>
                      <th className="px-4 py-4 font-semibold">E-mail</th>
                      <th className="px-4 py-4 font-semibold">Cargo</th>
                      <th className="px-4 py-4 font-semibold text-right">Acoes</th>
                    </tr>
                  </thead>
                    <tbody>
                    {employeesQuery.isLoading ? (
                      <tr><td colSpan={4} className="px-4 py-8 text-center text-sm text-[var(--muted)]">Carregando funcionarios...</td></tr>
                    ) : employeesQuery.isError ? (
                      <tr><td colSpan={4} className="px-4 py-8 text-center text-sm text-red-500">{(employeesQuery.error as Error).message}</td></tr>
                    ) : (employeesQuery.data?.length ?? 0) === 0 ? (
                      <tr><td colSpan={4} className="px-4 py-8 text-center text-sm text-[var(--muted)]">Nenhum funcionario vinculado a esta loja.</td></tr>
                    ) : employeesQuery.data?.map((employee) => (
                      <tr key={`${employee.lojaId}-${employee.usuarioId}`} className="border-t border-[var(--border)]">
                        <td className="px-4 py-4 text-sm font-medium text-[var(--foreground)]">{employee.nome}</td>
                        <td className="px-4 py-4 text-sm text-[var(--muted)]">{employee.email}</td>
                        <td className="px-4 py-4">
                          {canEditEmployees ? (
                            <div className="min-w-[180px] rounded-2xl border border-[var(--border)] bg-white px-3 py-2.5 text-sm text-[var(--foreground)]">
                              <Select
                                ariaLabel={`Selecionar cargo para ${employee.nome}`}
                                value={String(employee.cargoId)}
                                options={roleOptions}
                                onChange={(value) => void handleChangeEmployeeRole(employee, value)}
                              />
                            </div>
                          ) : (
                            <span className="inline-flex rounded-full bg-[var(--surface-muted)] px-3 py-2 text-sm text-[var(--foreground)]">{employee.cargoNome}</span>
                          )}
                        </td>
                        <td className="px-4 py-4 text-right">
                          {canRemoveEmployees ? (
                            <button
                              type="button"
                              onClick={() => void handleDeleteEmployee(employee)}
                              disabled={deleteEmployeeMutation.isPending}
                              className="inline-flex h-10 items-center justify-center rounded-2xl border border-[#efdfdb] bg-[#fff7f5] px-4 text-sm font-semibold text-[#b14a37] disabled:opacity-60"
                            >
                              Remover
                            </button>
                          ) : (
                            <span className="text-sm text-[var(--muted)]">Sem acoes</span>
                          )}
                        </td>
                      </tr>
                    ))}
                    </tbody>
                  </table>
                </div>
              </div>
            </div>
          ) : (
            <SectionBlocked title="Funcionarios da loja" description="Seu usuario nao possui permissao para visualizar funcionarios." />
          )}
        </section>
      </div>

      <section className="grid gap-6 xl:grid-cols-[minmax(0,440px)_minmax(0,1fr)]">
        <div className="rounded-[28px] border border-[var(--border)] bg-white p-6">
          {canViewRoles || canAddRoles || canEditRoles ? (
            <div className="space-y-5">
              <div>
                <h2 className="text-lg font-semibold text-[var(--foreground)]">{editingRoleId === null ? "Novo cargo" : "Editar cargo"}</h2>
                <p className="text-sm text-[var(--muted)]">Selecione as funcionalidades liberadas para este cargo.</p>
              </div>
              <label className="space-y-2">
                <span className="text-sm font-medium text-[var(--foreground)]">Nome do cargo</span>
                <input
                  type="text"
                  value={roleDraft.nome}
                  onChange={(event) => setRoleDraft((current) => ({ ...current, nome: event.target.value }))}
                  placeholder="Ex.: Atendente, Caixa, Gerente"
                  disabled={editingRoleId === null ? !canAddRoles : !canEditRoles}
                  className="h-12 w-full rounded-2xl border border-[var(--border)] bg-white px-4 text-sm text-[var(--foreground)] outline-none disabled:opacity-60"
                />
              </label>
              <div className="max-h-[520px] space-y-4 overflow-y-auto pr-1">
                {functionalitiesQuery.isLoading ? (
                  <p className="text-sm text-[var(--muted)]">Carregando funcionalidades...</p>
                ) : functionalitiesQuery.isError ? (
                  <p className="text-sm text-red-500">{(functionalitiesQuery.error as Error).message}</p>
                ) : groupedFunctionalities.map(([groupName, items]) => (
                  <div key={groupName} className="rounded-[24px] border border-[var(--border)] bg-[var(--surface-muted)]/45 p-4">
                    <p className="mb-3 text-sm font-semibold uppercase tracking-[0.16em] text-[var(--foreground)]">{groupName}</p>
                    <div className="space-y-3">
                      {items.map((item) => (
                        <label key={item.id} className="flex items-start gap-3 rounded-2xl bg-white px-4 py-3">
                          <input
                            type="checkbox"
                            checked={selectedFunctionalityIds.has(item.id)}
                            onChange={() => toggleFunctionality(item.id)}
                            disabled={editingRoleId === null ? !canAddRoles : !canEditRoles}
                            className="mt-1 h-4 w-4"
                          />
                          <div>
                            <p className="text-sm font-semibold text-[var(--foreground)]">{item.chave}</p>
                            <p className="text-sm text-[var(--muted)]">{item.descricao}</p>
                          </div>
                        </label>
                      ))}
                    </div>
                  </div>
                ))}
              </div>
              <div className="flex flex-col gap-3 sm:flex-row">
                <button
                  type="button"
                  onClick={handleSaveRole}
                  disabled={createRoleMutation.isPending || updateRoleMutation.isPending || (editingRoleId === null ? !canAddRoles : !canEditRoles)}
                  className="inline-flex h-12 items-center justify-center rounded-2xl bg-[#294d44] px-5 text-sm font-semibold text-white disabled:opacity-60"
                >
                  {editingRoleId === null ? (createRoleMutation.isPending ? "Salvando..." : "Criar cargo") : (updateRoleMutation.isPending ? "Atualizando..." : "Atualizar cargo")}
                </button>
                <button
                  type="button"
                  onClick={resetRoleDraft}
                  className="inline-flex h-12 items-center justify-center rounded-2xl border border-[var(--border)] px-5 text-sm font-semibold text-[var(--foreground)]"
                >
                  Limpar formulario
                </button>
              </div>
            </div>
          ) : (
            <SectionBlocked title="Cadastro de cargos" description="Seu usuario nao possui permissao para criar ou editar cargos." />
          )}
        </div>

        <div className="rounded-[28px] border border-[var(--border)] bg-white p-6">
          {canViewRoles ? (
            <div className="space-y-4">
              <div>
                <h2 className="text-lg font-semibold text-[var(--foreground)]">Cargos cadastrados</h2>
                <p className="text-sm text-[var(--muted)]">Revise e ajuste rapidamente as permissoes de cada cargo.</p>
              </div>
              {rolesQuery.isLoading ? (
                <p className="text-sm text-[var(--muted)]">Carregando cargos...</p>
              ) : rolesQuery.isError ? (
                <p className="text-sm text-red-500">{(rolesQuery.error as Error).message}</p>
              ) : (rolesQuery.data?.length ?? 0) === 0 ? (
                <p className="text-sm text-[var(--muted)]">Nenhum cargo cadastrado para esta loja.</p>
              ) : rolesQuery.data?.map((role) => (
                <div key={role.id} className="rounded-[24px] border border-[var(--border)] bg-[var(--surface-muted)]/45 p-5">
                  <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
                    <div>
                      <div className="flex flex-wrap items-center gap-2">
                        <h3 className="text-lg font-semibold text-[var(--foreground)]">{role.nome}</h3>
                        <span className="inline-flex rounded-full bg-white px-3 py-1 text-xs font-semibold uppercase tracking-[0.12em] text-[var(--muted)]">{role.quantidadeFuncionarios} funcionario(s)</span>
                      </div>
                      <p className="mt-2 text-sm text-[var(--muted)]">{role.funcionalidades.length} funcionalidade(s) liberada(s).</p>
                    </div>
                    <div className="flex flex-wrap gap-3">
                      {canEditRoles ? <button type="button" onClick={() => startEditingRole(role)} className="inline-flex h-10 items-center justify-center rounded-2xl border border-[var(--border)] bg-white px-4 text-sm font-semibold text-[var(--foreground)]">Editar</button> : null}
                      {canDeleteRoles ? <button type="button" onClick={() => void handleDeleteRole(role)} disabled={deleteRoleMutation.isPending} className="inline-flex h-10 items-center justify-center rounded-2xl border border-[#efdfdb] bg-[#fff7f5] px-4 text-sm font-semibold text-[#b14a37] disabled:opacity-60">Excluir</button> : null}
                    </div>
                  </div>
                  <div className="mt-4 flex flex-wrap gap-2">
                    {role.funcionalidades.map((item) => <span key={`${role.id}-${item.id}`} className="inline-flex rounded-full bg-white px-3 py-1.5 text-xs font-medium text-[var(--foreground)]">{item.chave}</span>)}
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <SectionBlocked title="Cargos cadastrados" description="Seu usuario nao possui permissao para visualizar cargos." />
          )}
        </div>
      </section>
    </section>
  );
}
