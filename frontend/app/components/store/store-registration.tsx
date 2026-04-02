"use client";

import { useMutation } from "@tanstack/react-query";
import { startTransition, useState } from "react";
import { toast } from "sonner";

import {
  extractStoreApiMessage,
  extractStoreFieldErrors,
  getAuthToken,
  initialStoreValues,
  type LojaResponse,
  type StoreFieldErrors,
  type StoreFormValues,
} from "@/lib/store";
import { asStoreResponse, createStore } from "@/services/store-service";
import { mapStoreZodErrors, storeSchema } from "@/validations/store";

import { StoreRegistrationForm } from "./store-registration-form";
import { StoreRegistrationHeader } from "./store-registration-header";
import { StoreRegistrationResponse } from "./store-registration-response";

export function StoreRegistration() {
  const [values, setValues] = useState<StoreFormValues>(initialStoreValues);
  const [errors, setErrors] = useState<StoreFieldErrors>({});
  const [latestStore, setLatestStore] = useState<LojaResponse | null>(null);

  const createStoreMutation = useMutation({
    mutationFn: async (payload: { nome: string; token: string }) =>
      createStore({ nome: payload.nome }, payload.token),
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

      toast.success(`Loja ${result.nome} cadastrada com sucesso.`);
    } catch {
      toast.error("Nao foi possivel conectar ao backend. Verifique se a API esta em execucao.");
    }
  }

  return (
    <section className="mx-auto max-w-3xl space-y-6">
      <StoreRegistrationHeader />
      <div className="grid gap-5 xl:grid-cols-[minmax(0,1fr)_320px]">
        <StoreRegistrationForm
          errors={errors}
          isSubmitting={createStoreMutation.isPending}
          values={values}
          onChange={updateNome}
          onReset={resetForm}
          onSubmit={handleSubmit}
        />
        <StoreRegistrationResponse latestStore={latestStore} />
      </div>
    </section>
  );
}
