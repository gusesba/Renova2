"use client";

import { useMutation, useQueryClient } from "@tanstack/react-query";
import { startTransition, useState } from "react";
import { toast } from "sonner";

import { useStoreContext } from "@/app/dashboard/store-context";
import {
  extractStoreApiMessage,
  extractStoreFieldErrors,
  getAuthToken,
  initialStoreValues,
  type LojaResponse,
  type StoreFieldErrors,
  type StoreFormValues,
} from "@/lib/store";
import { asStoreResponse, createStore, deleteStore, updateStore } from "@/services/store-service";
import { mapStoreZodErrors, storeSchema } from "@/validations/store";

import { StoreEditModal } from "./store-edit-modal";
import { StoreRegistrationForm } from "./store-registration-form";
import { StoreRegistrationHeader } from "./store-registration-header";
import { StoreRegistrationResponse } from "./store-registration-response";

export function StoreRegistration() {
  const queryClient = useQueryClient();
  const { selectedStore, setSelectedStoreId, stores } = useStoreContext();
  const [values, setValues] = useState<StoreFormValues>(initialStoreValues);
  const [errors, setErrors] = useState<StoreFieldErrors>({});
  const [latestStore, setLatestStore] = useState<LojaResponse | null>(null);
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [editValues, setEditValues] = useState<StoreFormValues>(initialStoreValues);
  const [editErrors, setEditErrors] = useState<StoreFieldErrors>({});

  const createStoreMutation = useMutation({
    mutationFn: async (payload: { nome: string; token: string }) =>
      createStore({ nome: payload.nome }, payload.token),
  });

  const updateStoreMutation = useMutation({
    mutationFn: async (payload: { nome: string; storeId: number; token: string }) =>
      updateStore(payload.storeId, { nome: payload.nome }, payload.token),
  });

  const deleteStoreMutation = useMutation({
    mutationFn: async (payload: { storeId: number; token: string }) =>
      deleteStore(payload.storeId, payload.token),
  });

  function updateNome(nome: string) {
    setValues({ nome });
    setErrors((current) => ({
      ...current,
      nome: undefined,
    }));
  }

  function resetForm() {
    setValues(initialStoreValues);
    setErrors({});
  }

  function openEditModal() {
    if (!selectedStore) {
      toast.error("Selecione uma loja antes de editar.");
      return;
    }

    setEditValues({ nome: selectedStore.nome });
    setEditErrors({});
    setIsEditModalOpen(true);
  }

  function closeEditModal() {
    if (updateStoreMutation.isPending || deleteStoreMutation.isPending) {
      return;
    }

    setIsEditModalOpen(false);
    setEditErrors({});
  }

  function updateEditNome(nome: string) {
    setEditValues({ nome });
    setEditErrors((current) => ({
      ...current,
      nome: undefined,
    }));
  }

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (createStoreMutation.isPending) {
      return;
    }

    const token = getAuthToken();

    if (!token) {
      toast.error("Voce precisa estar autenticado para cadastrar uma loja.");
      return;
    }

    const payload = {
      nome: values.nome.trim(),
    };

    const validation = storeSchema.safeParse(payload);

    if (!validation.success) {
      setErrors(mapStoreZodErrors(validation.error));
      toast.error("Corrija o nome da loja antes de continuar.");
      return;
    }

    setErrors({});

    try {
      const response = await createStoreMutation.mutateAsync({
        nome: validation.data.nome,
        token,
      });

      if (!response.ok) {
        const apiFieldErrors = extractStoreFieldErrors(response.body);

        if (Object.keys(apiFieldErrors).length > 0) {
          setErrors(apiFieldErrors);
        }

        toast.error(extractStoreApiMessage(response.body) ?? "Nao foi possivel cadastrar a loja.");
        return;
      }

      const result = asStoreResponse(response.body);

      startTransition(() => {
        setLatestStore(result);
        setValues(initialStoreValues);
      });

      await queryClient.invalidateQueries({ queryKey: ["stores"] });
      toast.success(`Loja ${result.nome} cadastrada com sucesso.`);
    } catch {
      toast.error("Nao foi possivel conectar ao backend. Verifique se a API esta em execucao.");
    }
  }

  async function handleEditSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (updateStoreMutation.isPending || !selectedStore) {
      return;
    }

    const token = getAuthToken();

    if (!token) {
      toast.error("Voce precisa estar autenticado para editar uma loja.");
      return;
    }

    const payload = {
      nome: editValues.nome.trim(),
    };

    const validation = storeSchema.safeParse(payload);

    if (!validation.success) {
      setEditErrors(mapStoreZodErrors(validation.error));
      toast.error("Corrija o nome da loja antes de continuar.");
      return;
    }

    setEditErrors({});

    try {
      const response = await updateStoreMutation.mutateAsync({
        nome: validation.data.nome,
        storeId: selectedStore.id,
        token,
      });

      if (!response.ok) {
        const apiFieldErrors = extractStoreFieldErrors(response.body);

        if (Object.keys(apiFieldErrors).length > 0) {
          setEditErrors(apiFieldErrors);
        }

        toast.error(extractStoreApiMessage(response.body) ?? "Nao foi possivel atualizar a loja.");
        return;
      }

      const result = asStoreResponse(response.body);

      startTransition(() => {
        setLatestStore(result);
        setIsEditModalOpen(false);
      });

      await queryClient.invalidateQueries({ queryKey: ["stores"] });
      toast.success(`Loja ${result.nome} atualizada com sucesso.`);
    } catch {
      toast.error("Nao foi possivel conectar ao backend. Verifique se a API esta em execucao.");
    }
  }

  async function handleDeleteStore() {
    if (deleteStoreMutation.isPending || !selectedStore) {
      return;
    }

    const token = getAuthToken();

    if (!token) {
      toast.error("Voce precisa estar autenticado para excluir uma loja.");
      return;
    }

    try {
      const response = await deleteStoreMutation.mutateAsync({
        storeId: selectedStore.id,
        token,
      });

      if (!response.ok) {
        toast.error(extractStoreApiMessage(response.body) ?? "Nao foi possivel excluir a loja.");
        return;
      }

      const deletedStoreId = selectedStore.id;
      const fallbackStore = stores.find((store) => store.id !== deletedStoreId) ?? null;

      startTransition(() => {
        setIsEditModalOpen(false);
        setLatestStore(null);
        setSelectedStoreId(fallbackStore?.id ?? null);
      });

      await queryClient.invalidateQueries({ queryKey: ["stores"] });
      toast.success("Loja excluida com sucesso.");
    } catch {
      toast.error("Nao foi possivel conectar ao backend. Verifique se a API esta em execucao.");
    }
  }

  return (
    <section className="mx-auto max-w-3xl space-y-6">
      <StoreRegistrationHeader
        canEditCurrentStore={Boolean(selectedStore)}
        currentStoreName={selectedStore?.nome ?? null}
        onEditCurrentStore={openEditModal}
      />
      <div className="grid gap-5 xl:grid-cols-[minmax(0,1fr)_320px]">
        <StoreRegistrationForm
          errors={errors}
          isSubmitting={createStoreMutation.isPending}
          values={values}
          onChange={updateNome}
          onReset={resetForm}
          onSubmit={handleSubmit}
        />
        <StoreRegistrationResponse currentStore={selectedStore} latestStore={latestStore} />
      </div>
      <StoreEditModal
        errors={editErrors}
        isDeleting={deleteStoreMutation.isPending}
        isOpen={isEditModalOpen}
        isSubmitting={updateStoreMutation.isPending}
        storeId={selectedStore?.id ?? null}
        values={editValues}
        onChange={updateEditNome}
        onClose={closeEditModal}
        onDelete={handleDeleteStore}
        onSubmit={handleEditSubmit}
      />
    </section>
  );
}
