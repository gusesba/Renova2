"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { startTransition, useEffect, useState, type SubmitEvent } from "react";
import { toast } from "sonner";

import { useSystemSession } from "@/app/(system)/components/system-session-provider";
import { AccessibleStoresPanel } from "@/app/(system)/stores/components/accessible-stores-panel";
import {
  StoreFormPanel,
  type StoreFormState,
} from "@/app/(system)/stores/components/store-form-panel";
import { StoresOverview } from "@/app/(system)/stores/components/stores-overview";
import { Card, CardBody, CardHeading } from "@/components/ui/card";
import { getZodErrorMessage } from "@/lib/helpers/access-schemas";
import { getErrorMessage } from "@/lib/helpers/formatters";
import { queryKeys } from "@/lib/helpers/query-keys";
import { createStoreSchema, updateStoreSchema } from "@/lib/helpers/store-schemas";
import { createStore, listAccessibleStores, updateStore } from "@/lib/services/stores";

const storeManagerPermission = "lojas.gerenciar";

const emptyStoreForm: StoreFormState = {
  id: "",
  nomeFantasia: "",
  razaoSocial: "",
  documento: "",
  telefone: "",
  email: "",
  logradouro: "",
  numero: "",
  complemento: "",
  bairro: "",
  cidade: "",
  uf: "",
  cep: "",
  statusLoja: "ativa",
};

// Coordena o modulo 02 com cadastro e listagem de lojas acessiveis.
export function StoresDashboard() {
  const { token, session } = useSystemSession();
  const queryClient = useQueryClient();
  const [selectedStoreId, setSelectedStoreId] = useState("");
  const [storeForm, setStoreForm] = useState<StoreFormState>(emptyStoreForm);

  const storesQuery = useQuery({
    queryFn: () => listAccessibleStores(token),
    queryKey: queryKeys.accessibleStores(token),
  });

  const canManageStores =
    session.lojas.length === 0 ||
    session.permissoes.includes(storeManagerPermission);

  useEffect(() => {
    if (storesQuery.isError) {
      toast.error(getErrorMessage(storesQuery.error));
    }
  }, [storesQuery.error, storesQuery.isError]);

  useEffect(() => {
    const stores = storesQuery.data ?? [];
    if (stores.length === 0) {
      startTransition(() => {
        setSelectedStoreId("");
        setStoreForm(emptyStoreForm);
      });
      return;
    }

    const nextSelectedStoreId =
      selectedStoreId && stores.some((store) => store.id === selectedStoreId)
        ? selectedStoreId
        : (stores.find((store) => store.ehLojaAtiva)?.id ?? stores[0]?.id ?? "");

    const selectedStore = stores.find((store) => store.id === nextSelectedStoreId);
    if (!selectedStore) {
      return;
    }

    startTransition(() => {
      setSelectedStoreId(nextSelectedStoreId);
      setStoreForm({
        id: selectedStore.id,
        nomeFantasia: selectedStore.nomeFantasia,
        razaoSocial: selectedStore.razaoSocial,
        documento: selectedStore.documento,
        telefone: selectedStore.telefone,
        email: selectedStore.email,
        logradouro: selectedStore.logradouro,
        numero: selectedStore.numero,
        complemento: selectedStore.complemento,
        bairro: selectedStore.bairro,
        cidade: selectedStore.cidade,
        uf: selectedStore.uf,
        cep: selectedStore.cep,
        statusLoja: selectedStore.statusLoja as "ativa" | "inativa",
      });
    });
  }, [selectedStoreId, storesQuery.data]);

  const refreshStores = async () => {
    await Promise.all([
      queryClient.invalidateQueries({ queryKey: queryKeys.accessibleStores(token) }),
      queryClient.invalidateQueries({ queryKey: queryKeys.session(token) }),
    ]);
  };

  const storeMutation = useMutation({
    mutationFn: async () => {
      if (storeForm.id) {
        const parsed = updateStoreSchema.safeParse(storeForm);
        if (!parsed.success) {
          throw new Error(getZodErrorMessage(parsed.error));
        }

        return updateStore(token, storeForm.id, parsed.data);
      }

      const parsed = createStoreSchema.safeParse(storeForm);
      if (!parsed.success) {
        throw new Error(getZodErrorMessage(parsed.error));
      }

      return createStore(token, parsed.data);
    },
    onError: (error) => {
      toast.error(getErrorMessage(error));
    },
    onSuccess: async (response) => {
      setSelectedStoreId(response.id);
      toast.success(
        storeForm.id
          ? "Loja atualizada com sucesso."
          : "Loja criada com sucesso.",
      );
      await refreshStores();
    },
  });

  const stores = storesQuery.data ?? [];
  const busy = storesQuery.isLoading || storeMutation.isPending;

  async function handleStoreSubmit(event: SubmitEvent<HTMLFormElement>) {
    event.preventDefault();
    await storeMutation.mutateAsync();
  }

  function handleNewStore() {
    setSelectedStoreId("");
    setStoreForm(emptyStoreForm);
  }

  return (
    <div className="dashboard-grid">
      <div className="dashboard-column" style={{ gridColumn: "1 / -1" }}>
        <StoresOverview stores={stores} />
      </div>

      <div className="dashboard-column">
        <StoreFormPanel
          busy={busy}
          canManage={canManageStores}
          form={storeForm}
          onNewStore={handleNewStore}
          onSubmit={handleStoreSubmit}
          setForm={setStoreForm}
        />
      </div>

      <div className="dashboard-column">
        <AccessibleStoresPanel
          selectedStoreId={selectedStoreId}
          setSelectedStoreId={setSelectedStoreId}
          stores={stores}
        />

        {!canManageStores ? (
          <Card>
            <CardBody className="section-stack">
              <CardHeading
                subtitle="A visao consolidada continua disponivel, mas a gestao da loja exige permissao especifica."
                title="Gestao restrita"
              />
              <div className="empty-state">
                Solicite a permissao de gerenciamento de lojas para editar o
                cadastro da loja.
              </div>
            </CardBody>
          </Card>
        ) : null}
      </div>
    </div>
  );
}
