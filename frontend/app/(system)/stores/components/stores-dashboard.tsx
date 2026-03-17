"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { startTransition, useEffect, useState, type SubmitEvent } from "react";
import { toast } from "sonner";

import { useSystemSession } from "@/app/(system)/components/system-session-provider";
import {
  AccessibleStoresPanel,
} from "@/app/(system)/stores/components/accessible-stores-panel";
import {
  StoreConfigurationPanel,
  type StoreConfigurationFormState,
} from "@/app/(system)/stores/components/store-configuration-panel";
import {
  StoreFormPanel,
  type StoreFormState,
} from "@/app/(system)/stores/components/store-form-panel";
import { StoresOverview } from "@/app/(system)/stores/components/stores-overview";
import { Card, CardBody, CardHeading } from "@/components/ui/card";
import { getZodErrorMessage } from "@/lib/helpers/access-schemas";
import { getErrorMessage } from "@/lib/helpers/formatters";
import { queryKeys } from "@/lib/helpers/query-keys";
import {
  createStoreSchema,
  storeConfigurationSchema,
  updateStoreSchema,
} from "@/lib/helpers/store-schemas";
import {
  createStore,
  listAccessibleStores,
  updateStore,
  updateStoreConfiguration,
} from "@/lib/services/renova-api";

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

const emptyStoreConfigurationForm: StoreConfigurationFormState = {
  nomeExibicao: "",
  cabecalhoImpressao: "",
  rodapeImpressao: "",
  usaModeloUnicoEtiqueta: true,
  usaModeloUnicoRecibo: true,
  fusoHorario: "America/Sao_Paulo",
  moeda: "BRL",
};

// Coordena o modulo 02: lista consolidada, cadastro da loja e configuracao operacional.
export function StoresDashboard() {
  const { token, session } = useSystemSession();
  const queryClient = useQueryClient();
  const [selectedStoreId, setSelectedStoreId] = useState("");
  const [storeForm, setStoreForm] = useState<StoreFormState>(emptyStoreForm);
  const [configurationForm, setConfigurationForm] =
    useState<StoreConfigurationFormState>(emptyStoreConfigurationForm);

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
        setConfigurationForm(emptyStoreConfigurationForm);
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
      setConfigurationForm({
        nomeExibicao: selectedStore.configuracao.nomeExibicao,
        cabecalhoImpressao: selectedStore.configuracao.cabecalhoImpressao,
        rodapeImpressao: selectedStore.configuracao.rodapeImpressao,
        usaModeloUnicoEtiqueta: selectedStore.configuracao.usaModeloUnicoEtiqueta,
        usaModeloUnicoRecibo: selectedStore.configuracao.usaModeloUnicoRecibo,
        fusoHorario: selectedStore.configuracao.fusoHorario,
        moeda: selectedStore.configuracao.moeda,
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

      const initialConfiguration = {
        ...configurationForm,
        nomeExibicao:
          configurationForm.nomeExibicao.trim() || storeForm.nomeFantasia.trim(),
        cabecalhoImpressao:
          configurationForm.cabecalhoImpressao.trim() ||
          storeForm.nomeFantasia.trim(),
      };

      const parsed = createStoreSchema.safeParse({
        ...storeForm,
        configuracao: initialConfiguration,
      });
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

  const configurationMutation = useMutation({
    mutationFn: async () => {
      if (!storeForm.id) {
        throw new Error("Crie ou selecione uma loja antes de salvar a configuracao.");
      }

      const parsed = storeConfigurationSchema.safeParse(configurationForm);
      if (!parsed.success) {
        throw new Error(getZodErrorMessage(parsed.error));
      }

      return updateStoreConfiguration(token, storeForm.id, parsed.data);
    },
    onError: (error) => {
      toast.error(getErrorMessage(error));
    },
    onSuccess: async () => {
      toast.success("Configuracao da loja atualizada com sucesso.");
      await refreshStores();
    },
  });

  const stores = storesQuery.data ?? [];
  const busy =
    storesQuery.isLoading ||
    storeMutation.isPending ||
    configurationMutation.isPending;

  async function handleStoreSubmit(event: SubmitEvent<HTMLFormElement>) {
    event.preventDefault();
    await storeMutation.mutateAsync();
  }

  async function handleConfigurationSubmit(event: SubmitEvent<HTMLFormElement>) {
    event.preventDefault();
    await configurationMutation.mutateAsync();
  }

  function handleNewStore() {
    setSelectedStoreId("");
    setStoreForm(emptyStoreForm);
    setConfigurationForm(emptyStoreConfigurationForm);
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
        <StoreConfigurationPanel
          busy={busy}
          canManage={canManageStores}
          form={configurationForm}
          hasSelectedStore={Boolean(storeForm.id)}
          onSubmit={handleConfigurationSubmit}
          setForm={setConfigurationForm}
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
                cadastro ou a configuracao operacional.
              </div>
            </CardBody>
          </Card>
        ) : null}
      </div>
    </div>
  );
}
