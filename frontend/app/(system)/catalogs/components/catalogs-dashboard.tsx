"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useEffect, useState, type SetStateAction, type SubmitEvent } from "react";
import { toast } from "sonner";

import { useSystemSession } from "@/app/(system)/components/system-session-provider";
import { CatalogEntryEditor } from "@/app/(system)/catalogs/components/catalog-entry-editor";
import { CatalogsOverview } from "@/app/(system)/catalogs/components/catalogs-overview";
import {
  emptyCatalogEntryForm,
  findCatalogEntry,
  getCatalogEntries,
  mapCatalogEntryToForm,
  type CatalogEntryFormState,
  type CatalogEntryType,
} from "@/app/(system)/catalogs/components/types";
import { AccessStateCard } from "@/components/ui/access-state-card";
import { accessPermissionCodes, hasPermission } from "@/lib/helpers/access-control";
import { getErrorMessage } from "@/lib/helpers/formatters";
import { queryKeys } from "@/lib/helpers/query-keys";
import { getZodErrorMessage } from "@/lib/schemas/access";
import { catalogNameFormSchema } from "@/lib/schemas/catalogs";
import {
  createBrand,
  createColor,
  createProductName,
  createSize,
  getCatalogWorkspace,
  updateBrand,
  updateColor,
  updateProductName,
  updateSize,
} from "@/lib/services/catalogs";

// Coordena o modulo 04 operando sempre em cima da loja ativa.
export function CatalogsDashboard() {
  const { token, session } = useSystemSession();
  const queryClient = useQueryClient();
  const canManageCatalog = hasPermission(session, accessPermissionCodes.catalogManage);
  const [selectedEntryType, setSelectedEntryType] =
    useState<CatalogEntryType>("produtoNome");
  const [selectedEntryId, setSelectedEntryId] = useState("");
  const [entryDraft, setEntryDraft] = useState<CatalogEntryFormState | null>(null);

  const workspaceQuery = useQuery({
    enabled: canManageCatalog && Boolean(session.lojaAtivaId),
    queryFn: () => getCatalogWorkspace(token),
    queryKey: queryKeys.catalogsWorkspace(token, session.lojaAtivaId),
  });

  useEffect(() => {
    if (workspaceQuery.isError) {
      toast.error(getErrorMessage(workspaceQuery.error));
    }
  }, [workspaceQuery.error, workspaceQuery.isError]);

  const currentEntries = getCatalogEntries(workspaceQuery.data, selectedEntryType);
  const resolvedSelectedEntryId = selectedEntryId || currentEntries[0]?.id || "";
  const currentEntry = findCatalogEntry(
    workspaceQuery.data,
    selectedEntryType,
    resolvedSelectedEntryId,
  );
  const entryForm = entryDraft ?? mapCatalogEntryToForm(currentEntry);

  function setEntryForm(value: SetStateAction<CatalogEntryFormState>) {
    setEntryDraft((current) => {
      const baseValue = current ?? entryForm;
      return typeof value === "function"
        ? (value as (current: CatalogEntryFormState) => CatalogEntryFormState)(baseValue)
        : value;
    });
  }

  const refreshWorkspace = async () => {
    await queryClient.invalidateQueries({
      queryKey: queryKeys.catalogsWorkspace(token, session.lojaAtivaId),
    });
  };

  const entryMutation = useMutation({
    mutationFn: async () => {
      const currentForm = entryDraft ?? entryForm;
      const parsed = catalogNameFormSchema.safeParse(currentForm);
      if (!parsed.success) {
        throw new Error(getZodErrorMessage(parsed.error));
      }

      switch (selectedEntryType) {
        case "produtoNome": {
          return parsed.data.id
            ? updateProductName(token, parsed.data.id, { nome: parsed.data.nome })
            : createProductName(token, {
              nome: parsed.data.nome,
            });
        }
        case "marca": {
          return parsed.data.id
            ? updateBrand(token, parsed.data.id, { nome: parsed.data.nome })
            : createBrand(token, {
              nome: parsed.data.nome,
            });
        }
        case "tamanho": {
          return parsed.data.id
            ? updateSize(token, parsed.data.id, { nome: parsed.data.nome })
            : createSize(token, {
              nome: parsed.data.nome,
            });
        }
        case "cor": {
          return parsed.data.id
            ? updateColor(token, parsed.data.id, { nome: parsed.data.nome })
            : createColor(token, {
              nome: parsed.data.nome,
            });
        }
      }
    },
    onError: (error) => {
      toast.error(getErrorMessage(error));
    },
    onSuccess: async (response) => {
      setSelectedEntryId(response.id);
      setEntryDraft(mapCatalogEntryToForm(response));
      toast.success(
        entryForm.id ? "Cadastro atualizado com sucesso." : "Cadastro criado com sucesso.",
      );
      await refreshWorkspace();
    },
  });

  if (!canManageCatalog) {
    return (
      <AccessStateCard
        message="Solicite a permissao adequada para manter os cadastros auxiliares do sistema."
        subtitle="Sua conta nao possui acesso ao modulo de catalogos."
        title="Modulo sem permissao"
      />
    );
  }

  async function handleEntrySubmit(event: SubmitEvent<HTMLFormElement>) {
    event.preventDefault();
    await entryMutation.mutateAsync();
  }

  function handleSelectEntryType(type: CatalogEntryType) {
    setSelectedEntryType(type);
    setSelectedEntryId("");
    setEntryDraft(null);
  }

  function handleSelectEntry(entryId: string) {
    setSelectedEntryId(entryId);
    setEntryDraft(null);
  }

  function handleNewEntry() {
    setSelectedEntryId("");
    setEntryDraft(emptyCatalogEntryForm);
  }

  return (
    <div className="catalogs-page-shell">
      <CatalogsOverview
        onSelectType={handleSelectEntryType}
        selectedType={selectedEntryType}
        workspace={workspaceQuery.data}
      />

      <CatalogEntryEditor
        busy={workspaceQuery.isLoading || entryMutation.isPending}
        entries={currentEntries}
        form={entryForm}
        onNewEntry={handleNewEntry}
        onSelectEntry={handleSelectEntry}
        onSelectType={handleSelectEntryType}
        onSubmit={handleEntrySubmit}
        selectedEntryId={resolvedSelectedEntryId}
        selectedType={selectedEntryType}
        setForm={setEntryForm}
      />
    </div>
  );
}
