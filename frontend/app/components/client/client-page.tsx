"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { startTransition, useMemo, useState } from "react";
import { toast } from "sonner";

import {
  asClientListResponse,
  asClientResponse,
  extractClientFieldErrors,
  getClientApiMessage,
  initialClientFilters,
  initialClientFormValues,
  type ClientFieldErrors,
  type ClientFilters,
  type ClientFormValues,
} from "@/lib/client";
import { getAuthToken } from "@/lib/store";
import { createClient, getClients } from "@/services/client-service";
import { clientSchema, mapClientZodErrors } from "@/validations/client";

import { ClientCreateModal } from "./client-create-modal";
import { ClientEmptyState } from "./client-empty-state";
import { ClientFiltersBar } from "./client-filters-bar";
import { ClientPagination } from "./client-pagination";
import { ClientsTable } from "./clients-table";

import { useStoreContext } from "@/app/dashboard/store-context";

export function ClientPage() {
  const queryClient = useQueryClient();
  const { isLoadingStores, selectedStore, selectedStoreId } = useStoreContext();
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [formValues, setFormValues] = useState<ClientFormValues>(initialClientFormValues);
  const [formErrors, setFormErrors] = useState<ClientFieldErrors>({});
  const [filters, setFilters] = useState<ClientFilters>(initialClientFilters);
  const token = useMemo(() => (typeof window === "undefined" ? null : getAuthToken()), []);

  const clientsQuery = useQuery({
    queryKey: ["clients", token, selectedStoreId, filters],
    queryFn: async () => {
      if (!token || !selectedStoreId) {
        return null;
      }

      const response = await getClients(token, selectedStoreId, filters);

      if (!response.ok) {
        throw new Error(
          getClientApiMessage(response.body) ?? "Nao foi possivel carregar os clientes.",
        );
      }

      return asClientListResponse(response.body);
    },
    enabled: Boolean(token && selectedStoreId),
  });

  const createClientMutation = useMutation({
    mutationFn: async (payload: {
      nome: string;
      contato: string;
      lojaId: number;
      userId?: number;
    }) => {
      if (!token) {
        throw new Error("Voce precisa estar autenticado para cadastrar um cliente.");
      }

      return createClient(payload, token);
    },
  });

  function handleOpenModal() {
    setIsCreateModalOpen(true);
  }

  function handleCloseModal() {
    if (createClientMutation.isPending) {
      return;
    }

    setIsCreateModalOpen(false);
    setFormValues(initialClientFormValues);
    setFormErrors({});
  }

  function handleFilterChange(next: Partial<ClientFilters>) {
    setFilters((current) => ({
      ...current,
      ...next,
      pagina: next.pagina ?? 1,
    }));
  }

  function updateFormField<K extends keyof ClientFormValues>(field: K, value: ClientFormValues[K]) {
    setFormValues((current) => ({
      ...current,
      [field]: value,
    }));
    setFormErrors((current) => ({
      ...current,
      [field]: undefined,
    }));
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
        contato: validation.data.contato.trim(),
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
            <ClientsTable clients={listResponse.itens} />
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
        storeName={selectedStore?.nome ?? null}
        values={formValues}
        onChange={updateFormField}
        onClose={handleCloseModal}
        onSubmit={handleSubmit}
      />
    </section>
  );
}
