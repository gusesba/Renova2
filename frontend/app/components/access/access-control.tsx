"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useDeferredValue, useMemo, useState } from "react";
import { toast } from "sonner";

import { useStoreContext } from "@/app/dashboard/store-context";
import { SearchableSelect } from "@/app/components/ui/searchable-select";
import {
  asEmployeeListResponse,
  getAuthToken,
  extractAccessApiMessage,
  type EmployeeListItem,
} from "@/lib/access";
import { getUserOptions } from "@/services/user-service";
import { createEmployee, deleteEmployee, getEmployees } from "@/services/access-service";

type UserOption = {
  id: number;
  nome: string;
  email: string;
};

function EmptyState({ message }: { message: string }) {
  return (
    <div className="rounded-[28px] border border-dashed border-[var(--border-strong)] bg-white/80 px-6 py-10 text-center shadow-[0_18px_40px_rgba(15,23,42,0.05)]">
      <p className="text-base font-semibold text-[var(--foreground)]">Controle de acesso</p>
      <p className="mt-2 text-sm text-[var(--muted)]">{message}</p>
    </div>
  );
}

export function AccessControl() {
  const { selectedStore } = useStoreContext();

  return (
    <AccessControlContent
      key={selectedStore?.id ?? "no-store"}
      selectedStore={selectedStore}
    />
  );
}

function AccessControlContent({
  selectedStore,
}: {
  selectedStore: { id: number; nome: string } | null;
}) {
  const queryClient = useQueryClient();
  const token = useMemo(() => (typeof window === "undefined" ? null : getAuthToken()), []);
  const [userSearch, setUserSearch] = useState("");
  const [selectedUser, setSelectedUser] = useState<UserOption | null>(null);
  const deferredUserSearch = useDeferredValue(userSearch);

  const employeesQuery = useQuery({
    queryKey: ["employees", selectedStore?.id, token],
    queryFn: async () => {
      if (!selectedStore || !token) {
        return [];
      }

      const response = await getEmployees(selectedStore.id, token);

      if (!response.ok) {
        throw new Error(extractAccessApiMessage(response.body) ?? "Nao foi possivel carregar os funcionarios.");
      }

      return asEmployeeListResponse(response.body);
    },
    enabled: Boolean(selectedStore && token),
  });

  const userOptionsQuery = useQuery({
    queryKey: ["employee-user-options", deferredUserSearch, token],
    queryFn: async () => {
      if (!token) {
        return [];
      }

      const response = await getUserOptions(token, deferredUserSearch);

      if (!response.ok) {
        throw new Error("Nao foi possivel carregar os usuarios.");
      }

      return response.body;
    },
    enabled: Boolean(token),
  });

  const createEmployeeMutation = useMutation({
    mutationFn: async (payload: { storeId: number; userId: number; token: string }) =>
      createEmployee(payload.storeId, { usuarioId: payload.userId }, payload.token),
  });

  const deleteEmployeeMutation = useMutation({
    mutationFn: async (payload: { storeId: number; userId: number; token: string }) =>
      deleteEmployee(payload.storeId, payload.userId, payload.token),
  });

  const employeeIds = useMemo(
    () => new Set((employeesQuery.data ?? []).map((employee) => employee.usuarioId)),
    [employeesQuery.data],
  );

  const availableOptions = useMemo(
    () =>
      (userOptionsQuery.data ?? [])
        .filter((user) => !employeeIds.has(user.id))
        .map((user) => ({
          label: `${user.nome} • ${user.email}`,
          value: String(user.id),
        })),
    [employeeIds, userOptionsQuery.data],
  );

  async function handleAddEmployee() {
    if (!selectedStore) {
      toast.error("Selecione uma loja antes de adicionar funcionarios.");
      return;
    }

    if (createEmployeeMutation.isPending) {
      return;
    }

    if (!token) {
      toast.error("Voce precisa estar autenticado para gerenciar funcionarios.");
      return;
    }

    if (!selectedUser) {
      toast.error("Selecione um usuario para adicionar como funcionario.");
      return;
    }

    try {
      const response = await createEmployeeMutation.mutateAsync({
        storeId: selectedStore.id,
        userId: selectedUser.id,
        token,
      });

      if (!response.ok) {
        toast.error(extractAccessApiMessage(response.body) ?? "Nao foi possivel adicionar o funcionario.");
        return;
      }

      setSelectedUser(null);
      setUserSearch("");
      await queryClient.invalidateQueries({ queryKey: ["employees", selectedStore.id, token] });
      toast.success(`Usuario ${selectedUser.nome} adicionado como funcionario.`);
    } catch {
      toast.error("Nao foi possivel conectar ao backend. Verifique se a API esta em execucao.");
    }
  }

  async function handleDeleteEmployee(employee: EmployeeListItem) {
    if (!selectedStore) {
      return;
    }

    if (!token) {
      toast.error("Voce precisa estar autenticado para gerenciar funcionarios.");
      return;
    }

    try {
      const response = await deleteEmployeeMutation.mutateAsync({
        storeId: selectedStore.id,
        userId: employee.usuarioId,
        token,
      });

      if (!response.ok) {
        toast.error(extractAccessApiMessage(response.body) ?? "Nao foi possivel remover o funcionario.");
        return;
      }

      await queryClient.invalidateQueries({ queryKey: ["employees", selectedStore.id, token] });
      toast.success(`Usuario ${employee.nome} removido da loja.`);
    } catch {
      toast.error("Nao foi possivel conectar ao backend. Verifique se a API esta em execucao.");
    }
  }

  if (!selectedStore) {
    return <EmptyState message="Selecione uma loja no topo da pagina para gerenciar os funcionarios." />;
  }

  return (
    <section className="space-y-6">
      <div className="rounded-[30px] border border-[var(--border)] bg-[linear-gradient(135deg,_#fffef9,_#f4f7ff_50%,_#eef6f1)] p-6 shadow-[0_28px_70px_rgba(15,23,42,0.08)]">
        <div className="flex flex-col gap-4 lg:flex-row lg:items-end lg:justify-between">
          <div className="space-y-2">
            <span className="inline-flex rounded-full bg-[#eef4ea] px-3 py-1 text-xs font-semibold uppercase tracking-[0.22em] text-[#52624d]">
              Loja ativa
            </span>
            <div>
              <h1 className="text-3xl font-semibold tracking-tight text-[var(--foreground)]">
                Controle de acesso
              </h1>
              <p className="mt-2 max-w-2xl text-sm text-[var(--muted)]">
                Gerencie quais usuarios estao vinculados como funcionarios da loja{" "}
                <span className="font-semibold text-[var(--foreground)]">{selectedStore.nome}</span>.
              </p>
            </div>
          </div>

          <div className="rounded-3xl border border-white/70 bg-white/75 px-5 py-4 shadow-[0_18px_42px_rgba(15,23,42,0.06)] backdrop-blur">
            <p className="text-xs font-semibold uppercase tracking-[0.2em] text-[var(--muted)]">
              Funcionarios vinculados
            </p>
            <p className="mt-1 text-3xl font-semibold text-[var(--foreground)]">
              {employeesQuery.data?.length ?? 0}
            </p>
          </div>
        </div>
      </div>

      <div className="grid gap-6 xl:grid-cols-[minmax(0,420px)_minmax(0,1fr)]">
        <section className="rounded-[28px] border border-[var(--border)] bg-white p-6 shadow-[0_18px_44px_rgba(15,23,42,0.06)]">
          <div className="space-y-1">
            <h2 className="text-lg font-semibold text-[var(--foreground)]">Adicionar funcionario</h2>
            <p className="text-sm text-[var(--muted)]">
              Selecione um usuario cadastrado para criar o vinculo com a loja atual.
            </p>
          </div>

          <div className="mt-6 space-y-4">
            <div className="space-y-2">
              <label className="text-sm font-medium text-[var(--foreground)]">Usuario</label>
              <SearchableSelect
                ariaLabel="Selecionar usuario"
                value={selectedUser ? String(selectedUser.id) : null}
                selectedLabel={selectedUser ? `${selectedUser.nome} • ${selectedUser.email}` : undefined}
                searchValue={userSearch}
                searchPlaceholder="Buscar por nome ou e-mail"
                placeholder="Escolha um usuario"
                loading={userOptionsQuery.isLoading}
                emptyLabel="Nenhum usuario disponivel para vinculo"
                options={availableOptions}
                onSearchChange={setUserSearch}
                onChange={(option) => {
                  const user = (userOptionsQuery.data ?? []).find((item) => item.id === Number(option.value));

                  if (!user) {
                    return;
                  }

                  setSelectedUser(user);
                }}
              />
            </div>

            <button
              type="button"
              onClick={handleAddEmployee}
              disabled={createEmployeeMutation.isPending}
              className="inline-flex h-12 w-full items-center justify-center rounded-2xl bg-[#294d44] px-4 text-sm font-semibold text-white transition hover:brightness-105 disabled:cursor-not-allowed disabled:opacity-60"
            >
              {createEmployeeMutation.isPending ? "Adicionando..." : "Adicionar funcionario"}
            </button>
          </div>
        </section>

        <section className="rounded-[28px] border border-[var(--border)] bg-white p-6 shadow-[0_18px_44px_rgba(15,23,42,0.06)]">
          <div className="flex flex-col gap-2 sm:flex-row sm:items-end sm:justify-between">
            <div>
              <h2 className="text-lg font-semibold text-[var(--foreground)]">Funcionarios da loja</h2>
              <p className="text-sm text-[var(--muted)]">
                Esta tabela representa somente os vinculos salvos em <code>Funcionario</code>.
              </p>
            </div>
          </div>

          <div className="mt-6 overflow-hidden rounded-[24px] border border-[var(--border)]">
            <table className="min-w-full border-collapse">
              <thead className="bg-[var(--surface-muted)]">
                <tr className="text-left text-xs uppercase tracking-[0.18em] text-[var(--muted)]">
                  <th className="px-4 py-4 font-semibold">Usuario</th>
                  <th className="px-4 py-4 font-semibold">E-mail</th>
                  <th className="px-4 py-4 font-semibold text-right">Acao</th>
                </tr>
              </thead>
              <tbody>
                {employeesQuery.isLoading ? (
                  <tr>
                    <td colSpan={3} className="px-4 py-8 text-center text-sm text-[var(--muted)]">
                      Carregando funcionarios...
                    </td>
                  </tr>
                ) : employeesQuery.isError ? (
                  <tr>
                    <td colSpan={3} className="px-4 py-8 text-center text-sm text-red-500">
                      {(employeesQuery.error as Error).message}
                    </td>
                  </tr>
                ) : (employeesQuery.data?.length ?? 0) === 0 ? (
                  <tr>
                    <td colSpan={3} className="px-4 py-8 text-center text-sm text-[var(--muted)]">
                      Nenhum funcionario vinculado a esta loja.
                    </td>
                  </tr>
                ) : (
                  employeesQuery.data?.map((employee) => (
                    <tr key={`${employee.lojaId}-${employee.usuarioId}`} className="border-t border-[var(--border)]">
                      <td className="px-4 py-4 text-sm font-medium text-[var(--foreground)]">{employee.nome}</td>
                      <td className="px-4 py-4 text-sm text-[var(--muted)]">{employee.email}</td>
                      <td className="px-4 py-4 text-right">
                        <button
                          type="button"
                          onClick={() => void handleDeleteEmployee(employee)}
                          disabled={deleteEmployeeMutation.isPending}
                          className="inline-flex h-10 items-center justify-center rounded-2xl border border-[#efdfdb] bg-[#fff7f5] px-4 text-sm font-semibold text-[#b14a37] transition hover:bg-[#fff0eb] disabled:cursor-not-allowed disabled:opacity-60"
                        >
                          Remover
                        </button>
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        </section>
      </div>
    </section>
  );
}
