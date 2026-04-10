"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { startTransition, useEffect, useMemo, useState } from "react";
import { toast } from "sonner";

import {
  asClientListResponse,
  asClientResponse,
  defaultClientTableSettings,
  extractClientFieldErrors,
  getStoredClientTableSettings,
  getClientApiMessage,
  initialClientFilters,
  initialClientFormValues,
  formatPhoneValue,
  normalizeNumericValue,
  persistClientTableSettings,
  type ClientTableSettings,
  type ClientFieldErrors,
  type ClientFilters,
  type ClientListItem,
  type ClientFormValues,
  type ClientUserOption,
} from "@/lib/client";
import { getAuthToken } from "@/lib/store";
import {
  createClient,
  deleteClient,
  getClients,
  updateClient,
} from "@/services/client-service";
import { getUserOptions } from "@/services/user-service";
import { clientSchema, mapClientZodErrors } from "@/validations/client";

import { ClientCreateModal } from "./client-create-modal";
import { ClientDeleteModal } from "./client-delete-modal";
import { ClientEditModal } from "./client-edit-modal";
import { ClientEmptyState } from "./client-empty-state";
import { ClientFiltersBar } from "./client-filters-bar";
import { ClientPagination } from "./client-pagination";
import { ClientSettingsModal } from "./client-settings-modal";
import { ClientsTable } from "./clients-table";

import { useStoreContext } from "@/app/dashboard/store-context";

export function ClientPage() {
  const queryClient = useQueryClient();
  const { isLoadingStores, selectedStore, selectedStoreId } = useStoreContext();
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
  const [isSettingsModalOpen, setIsSettingsModalOpen] = useState(false);
  const [formValues, setFormValues] = useState<ClientFormValues>(initialClientFormValues);
  const [formErrors, setFormErrors] = useState<ClientFieldErrors>({});
  const [editFormValues, setEditFormValues] = useState<ClientFormValues>(initialClientFormValues);
  const [editFormErrors, setEditFormErrors] = useState<ClientFieldErrors>({});
  const [selectedClientId, setSelectedClientId] = useState<number | null>(null);
  const [selectedClientName, setSelectedClientName] = useState<string | null>(null);
  const [tableSettings, setTableSettings] = useState<ClientTableSettings>(() =>
    getStoredClientTableSettings(),
  );
  const [userSearch, setUserSearch] = useState("");
  const [editUserSearch, setEditUserSearch] = useState("");
  const [debouncedUserSearch, setDebouncedUserSearch] = useState("");
  const [debouncedEditUserSearch, setDebouncedEditUserSearch] = useState("");
  const [selectedUserOption, setSelectedUserOption] = useState<ClientUserOption | null>(null);
  const [selectedEditUserOption, setSelectedEditUserOption] = useState<ClientUserOption | null>(null);
  const [filters, setFilters] = useState<ClientFilters>(() => ({
    ...initialClientFilters,
    tamanhoPagina: getStoredClientTableSettings().tamanhoPagina,
  }));
  const [debouncedTextFilters, setDebouncedTextFilters] = useState(() => ({
    nome: initialClientFilters.nome,
    contato: initialClientFilters.contato,
  }));
  const token = useMemo(() => (typeof window === "undefined" ? null : getAuthToken()), []);

  useEffect(() => {
    const timeoutId = window.setTimeout(() => {
      setDebouncedTextFilters({
        nome: filters.nome,
        contato: filters.contato,
      });
    }, 300);

    return () => {
      window.clearTimeout(timeoutId);
    };
  }, [filters.nome, filters.contato]);

  useEffect(() => {
    const timeoutId = window.setTimeout(() => {
      setDebouncedUserSearch(userSearch);
    }, 300);

    return () => {
      window.clearTimeout(timeoutId);
    };
  }, [userSearch]);

  useEffect(() => {
    const timeoutId = window.setTimeout(() => {
      setDebouncedEditUserSearch(editUserSearch);
    }, 300);

    return () => {
      window.clearTimeout(timeoutId);
    };
  }, [editUserSearch]);

  const queryFilters = useMemo<ClientFilters>(
    () => ({
      ...filters,
      nome: debouncedTextFilters.nome,
      contato: normalizeNumericValue(debouncedTextFilters.contato),
    }),
    [debouncedTextFilters.contato, debouncedTextFilters.nome, filters],
  );

  const clientsQuery = useQuery({
    queryKey: ["clients", token, selectedStoreId, queryFilters],
    queryFn: async () => {
      if (!token || !selectedStoreId) {
        return null;
      }

      const response = await getClients(token, selectedStoreId, queryFilters);

      if (!response.ok) {
        throw new Error(
          getClientApiMessage(response.body) ?? "Nao foi possivel carregar os clientes.",
        );
      }

      return asClientListResponse(response.body);
    },
    enabled: Boolean(token && selectedStoreId),
  });

  const createUserOptionsQuery = useQuery({
    queryKey: ["client-users", token, debouncedUserSearch],
    queryFn: async () => {
      if (!token) {
        return [];
      }

      const response = await getUserOptions(token, debouncedUserSearch);

      if (!response.ok) {
        throw new Error("Nao foi possivel carregar os usuarios.");
      }

      return response.body;
    },
    enabled: Boolean(token && isCreateModalOpen),
  });

  const editUserOptionsQuery = useQuery({
    queryKey: ["client-users", token, debouncedEditUserSearch],
    queryFn: async () => {
      if (!token) {
        return [];
      }

      const response = await getUserOptions(token, debouncedEditUserSearch);

      if (!response.ok) {
        throw new Error("Nao foi possivel carregar os usuarios.");
      }

      return response.body;
    },
    enabled: Boolean(token && isEditModalOpen),
  });

  const createClientMutation = useMutation({
    mutationFn: async (payload: {
      nome: string;
      contato: string;
      doacao: boolean;
      lojaId: number;
      userId?: number;
    }) => {
      if (!token) {
        throw new Error("Voce precisa estar autenticado para cadastrar um cliente.");
      }

      return createClient(payload, token);
    },
  });

  const updateClientMutation = useMutation({
    mutationFn: async (payload: {
      clientId: number;
      nome: string;
      contato: string;
      doacao: boolean;
      userId?: number;
    }) => {
      if (!token) {
        throw new Error("Voce precisa estar autenticado para editar um cliente.");
      }

      return updateClient(
        payload.clientId,
        {
          nome: payload.nome,
          contato: payload.contato,
          doacao: payload.doacao,
          ...(payload.userId ? { userId: payload.userId } : {}),
        },
        token,
      );
    },
  });

  const deleteClientMutation = useMutation({
    mutationFn: async (clientId: number) => {
      if (!token) {
        throw new Error("Voce precisa estar autenticado para excluir um cliente.");
      }

      return deleteClient(clientId, token);
    },
  });

  function handleOpenModal() {
    setUserSearch("");
    setDebouncedUserSearch("");
    setSelectedUserOption(null);
    setIsCreateModalOpen(true);
  }

  function handleOpenEditModal(client: ClientListItem) {
    setSelectedClientId(client.id);
    setEditFormValues({
      nome: client.nome,
      contato: formatPhoneValue(client.contato),
      doacao: client.doacao,
      userId: client.userId ? String(client.userId) : "",
    });
    setEditUserSearch(client.userNome ?? "");
    setDebouncedEditUserSearch(client.userNome ?? "");
    setSelectedEditUserOption(
      client.userId && client.userNome && client.userEmail
        ? {
            id: client.userId,
            nome: client.userNome,
            email: client.userEmail,
          }
        : null,
    );
    setEditFormErrors({});
    setIsEditModalOpen(true);
  }

  function handleOpenSettingsModal() {
    setIsSettingsModalOpen(true);
  }

  function handleOpenDeleteModal(client: ClientListItem) {
    setSelectedClientId(client.id);
    setSelectedClientName(client.nome);
    setIsDeleteModalOpen(true);
  }

  function handleCloseModal() {
    if (createClientMutation.isPending) {
      return;
    }

    setIsCreateModalOpen(false);
    setFormValues(initialClientFormValues);
    setFormErrors({});
    setUserSearch("");
    setDebouncedUserSearch("");
    setSelectedUserOption(null);
  }

  function handleCloseEditModal() {
    if (updateClientMutation.isPending) {
      return;
    }

    setIsEditModalOpen(false);
    setSelectedClientId(null);
    setEditFormValues(initialClientFormValues);
    setEditFormErrors({});
    setEditUserSearch("");
    setDebouncedEditUserSearch("");
    setSelectedEditUserOption(null);
  }

  function handleCloseDeleteModal() {
    if (deleteClientMutation.isPending) {
      return;
    }

    setIsDeleteModalOpen(false);
    setSelectedClientId(null);
    setSelectedClientName(null);
  }

  function handleCloseSettingsModal() {
    setIsSettingsModalOpen(false);
  }

  function handleFilterChange(next: Partial<ClientFilters>) {
    setFilters((current) => ({
      ...current,
      ...next,
      pagina: next.pagina ?? 1,
    }));
  }

  function updateFormField<K extends keyof ClientFormValues>(field: K, value: ClientFormValues[K]) {
    const normalizedValue = field === "contato" ? formatPhoneValue(String(value)) : value;

    setFormValues((current) => ({
      ...current,
      [field]: normalizedValue,
    }));
    setFormErrors((current) => ({
      ...current,
      [field]: undefined,
    }));

    if (field === "userId" && !value) {
      setSelectedUserOption(null);
    }
  }

  function updateEditFormField<K extends keyof ClientFormValues>(
    field: K,
    value: ClientFormValues[K],
  ) {
    const normalizedValue = field === "contato" ? formatPhoneValue(String(value)) : value;

    setEditFormValues((current) => ({
      ...current,
      [field]: normalizedValue,
    }));
    setEditFormErrors((current) => ({
      ...current,
      [field]: undefined,
    }));

    if (field === "userId" && !value) {
      setSelectedEditUserOption(null);
    }
  }

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (!selectedStoreId) {
      toast.error("Selecione uma loja antes de cadastrar clientes.");
      return;
    }

    const validation = clientSchema.safeParse(formValues);

    if (!validation.success) {
      setFormErrors(mapClientZodErrors(validation.error));
      return;
    }

    setFormErrors({});

    try {
      const payload = {
        nome: validation.data.nome.trim(),
        contato: normalizeNumericValue(validation.data.contato),
        doacao: validation.data.doacao,
        lojaId: selectedStoreId,
        ...(validation.data.userId ? { userId: Number(validation.data.userId) } : {}),
      };

      const response = await createClientMutation.mutateAsync(payload);

      if (!response.ok) {
        const apiErrors = extractClientFieldErrors(response.body);

        if (Object.keys(apiErrors).length > 0) {
          setFormErrors(apiErrors);
        }

        toast.error(getClientApiMessage(response.body) ?? "Nao foi possivel cadastrar o cliente.");
        return;
      }

      const createdClient = asClientResponse(response.body);

      startTransition(() => {
        setFormValues(initialClientFormValues);
        setFormErrors({});
        setIsCreateModalOpen(false);
      });

      await queryClient.invalidateQueries({ queryKey: ["clients"] });
      toast.success(`Cliente ${createdClient.nome} cadastrado com sucesso.`);
    } catch (error) {
      toast.error(
        error instanceof Error
          ? error.message
          : "Nao foi possivel conectar ao backend. Verifique se a API esta em execucao.",
      );
    }
  }

  async function handleEditSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (!selectedClientId) {
      toast.error("Selecione um cliente valido para editar.");
      return;
    }

    const validation = clientSchema.safeParse(editFormValues);

    if (!validation.success) {
      setEditFormErrors(mapClientZodErrors(validation.error));
      return;
    }

    setEditFormErrors({});

    try {
      const payload = {
        clientId: selectedClientId,
        nome: validation.data.nome.trim(),
        contato: normalizeNumericValue(validation.data.contato),
        doacao: validation.data.doacao,
        ...(validation.data.userId ? { userId: Number(validation.data.userId) } : {}),
      };

      const response = await updateClientMutation.mutateAsync(payload);

      if (!response.ok) {
        const apiErrors = extractClientFieldErrors(response.body);

        if (Object.keys(apiErrors).length > 0) {
          setEditFormErrors(apiErrors);
        }

        toast.error(getClientApiMessage(response.body) ?? "Nao foi possivel editar o cliente.");
        return;
      }

      const updatedClient = asClientResponse(response.body);

      startTransition(() => {
        setSelectedClientId(null);
        setEditFormValues(initialClientFormValues);
        setEditFormErrors({});
        setIsEditModalOpen(false);
      });

      await queryClient.invalidateQueries({ queryKey: ["clients"] });
      toast.success(`Cliente ${updatedClient.nome} atualizado com sucesso.`);
    } catch (error) {
      toast.error(
        error instanceof Error
          ? error.message
          : "Nao foi possivel conectar ao backend. Verifique se a API esta em execucao.",
      );
    }
  }

  async function handleDeleteConfirm() {
    if (!selectedClientId) {
      toast.error("Selecione um cliente valido para excluir.");
      return;
    }

    try {
      const response = await deleteClientMutation.mutateAsync(selectedClientId);

      if (!response.ok) {
        toast.error(getClientApiMessage(response.body) ?? "Nao foi possivel excluir o cliente.");
        return;
      }

      const deletedClientName = selectedClientName;

      startTransition(() => {
        setIsDeleteModalOpen(false);
        setSelectedClientId(null);
        setSelectedClientName(null);
      });

      await queryClient.invalidateQueries({ queryKey: ["clients"] });
      toast.success(
        deletedClientName
          ? `Cliente ${deletedClientName} excluido com sucesso.`
          : "Cliente excluido com sucesso.",
      );
    } catch (error) {
      toast.error(
        error instanceof Error
          ? error.message
          : "Nao foi possivel conectar ao backend. Verifique se a API esta em execucao.",
      );
    }
  }

  const listResponse = clientsQuery.data;
  const hasStore = Boolean(selectedStoreId);

  return (
    <section className="space-y-6">
      <div className="rounded-[28px] border border-[var(--border)] bg-white p-6 shadow-[0_18px_45px_rgba(15,23,42,0.05)]">
        <ClientFiltersBar
          filters={filters}
          hasStore={hasStore}
          isLoading={clientsQuery.isLoading || isLoadingStores}
          onAddClient={handleOpenModal}
          onOpenSettings={handleOpenSettingsModal}
          onChange={handleFilterChange}
        />

        {!hasStore ? (
          <ClientEmptyState
            title="Selecione uma loja"
            description="A busca e o cadastro de clientes dependem da loja ativa no topo da pagina."
          />
        ) : clientsQuery.isLoading ? (
          <ClientEmptyState
            title="Carregando clientes"
            description="Buscando os registros mais recentes da loja selecionada."
          />
        ) : clientsQuery.isError ? (
          <ClientEmptyState
            title="Falha ao carregar clientes"
            description={
              clientsQuery.error instanceof Error
                ? clientsQuery.error.message
                : "Tente novamente em instantes."
            }
          />
        ) : listResponse && listResponse.itens.length > 0 ? (
          <>
            <ClientsTable
              clients={listResponse.itens}
              visibleFields={tableSettings.visibleFields}
              getClientDetailsHref={(client) => `/dashboard/cliente/${client.id}`}
              onEditClient={handleOpenEditModal}
              onDeleteClient={handleOpenDeleteModal}
            />
            <ClientPagination
              currentPage={filters.pagina}
              hasNextPage={filters.pagina < listResponse.totalPaginas}
              hasPreviousPage={filters.pagina > 1}
              totalItems={listResponse.totalItens}
              totalPages={listResponse.totalPaginas}
              onPageChange={(pagina) => handleFilterChange({ pagina })}
            />
          </>
        ) : (
          <ClientEmptyState
            title="Nenhum cliente encontrado"
            description="Ajuste os filtros ou cadastre um novo cliente para começar a popular esta tabela."
          />
        )}
      </div>

      <ClientCreateModal
        errors={formErrors}
        isOpen={isCreateModalOpen}
        isSubmitting={createClientMutation.isPending}
        isUserLoading={createUserOptionsQuery.isLoading}
        storeName={selectedStore?.nome ?? null}
        userEmptyLabel={
          createUserOptionsQuery.isError ? "Falha ao carregar usuarios." : "Nenhum usuario encontrado."
        }
        userOptions={createUserOptionsQuery.data ?? []}
        userSearchValue={userSearch}
        userSelectedLabel={
          selectedUserOption ? `${selectedUserOption.nome} - ${selectedUserOption.email}` : undefined
        }
        values={formValues}
        onChange={(field, value) => {
          updateFormField(field, value);

          if (field === "userId") {
            const selected = (createUserOptionsQuery.data ?? []).find((user) => user.id === Number(value));
            setSelectedUserOption(selected ?? null);
            setUserSearch(selected ? `${selected.nome} - ${selected.email}` : "");
          }
        }}
        onClose={handleCloseModal}
        onUserSearchChange={(value) => {
          setUserSearch(value);
          setSelectedUserOption(null);
          updateFormField("userId", "");
        }}
        onSubmit={handleSubmit}
      />
      <ClientEditModal
        clientId={selectedClientId}
        errors={editFormErrors}
        isOpen={isEditModalOpen}
        isSubmitting={updateClientMutation.isPending}
        isUserLoading={editUserOptionsQuery.isLoading}
        userEmptyLabel={
          editUserOptionsQuery.isError ? "Falha ao carregar usuarios." : "Nenhum usuario encontrado."
        }
        userOptions={editUserOptionsQuery.data ?? []}
        userSearchValue={editUserSearch}
        userSelectedLabel={
          selectedEditUserOption
            ? `${selectedEditUserOption.nome} - ${selectedEditUserOption.email}`
            : undefined
        }
        values={editFormValues}
        onChange={(field, value) => {
          updateEditFormField(field, value);

          if (field === "userId") {
            const selected = (editUserOptionsQuery.data ?? []).find((user) => user.id === Number(value));
            setSelectedEditUserOption(selected ?? null);
            setEditUserSearch(selected ? `${selected.nome} - ${selected.email}` : "");
          }
        }}
        onClose={handleCloseEditModal}
        onUserSearchChange={(value) => {
          setEditUserSearch(value);
          setSelectedEditUserOption(null);
          updateEditFormField("userId", "");
        }}
        onSubmit={handleEditSubmit}
      />
      <ClientDeleteModal
        clientName={selectedClientName}
        isOpen={isDeleteModalOpen}
        isSubmitting={deleteClientMutation.isPending}
        onClose={handleCloseDeleteModal}
        onConfirm={handleDeleteConfirm}
      />
      <ClientSettingsModal
        isOpen={isSettingsModalOpen}
        settings={tableSettings}
        onClose={handleCloseSettingsModal}
        onSave={(nextSettings) => {
          const normalizedSettings = {
            ...defaultClientTableSettings,
            ...nextSettings,
          };

          setTableSettings(normalizedSettings);
          persistClientTableSettings(normalizedSettings);
          setFilters((current) => ({
            ...current,
            pagina: 1,
            tamanhoPagina: normalizedSettings.tamanhoPagina,
          }));
          setIsSettingsModalOpen(false);
          toast.success("Configuracoes da tabela atualizadas.");
        }}
      />
    </section>
  );
}
