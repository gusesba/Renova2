"use client";

import { useMutation, useQuery } from "@tanstack/react-query";
import {
  startTransition,
  useDeferredValue,
  useEffect,
  useMemo,
  useState,
  type FormEvent,
} from "react";
import { toast } from "sonner";

import { DocumentPreviewPanel } from "@/app/(system)/documents/components/document-preview-panel";
import { DocumentSearchPanel } from "@/app/(system)/documents/components/document-search-panel";
import { DocumentsOverview } from "@/app/(system)/documents/components/documents-overview";
import {
  createDefaultDocumentQuery,
  getSelectedDocumentType,
  resolveSelectedDocument,
  type DocumentQueryState,
} from "@/app/(system)/documents/components/types";
import { useSystemSession } from "@/app/(system)/components/system-session-provider";
import { AccessStateCard } from "@/components/ui/access-state-card";
import { canAccessDocumentsModule } from "@/lib/helpers/access-control";
import { getErrorMessage } from "@/lib/helpers/formatters";
import { queryKeys } from "@/lib/helpers/query-keys";
import { getZodErrorMessage } from "@/lib/schemas/access";
import { documentQuerySchema } from "@/lib/schemas/documents";
import {
  getDocumentsWorkspace,
  printDocument,
  searchDocuments,
} from "@/lib/services/documents";

// Orquestra o modulo 16 com busca por tipo e impressao dos HTMLs padronizados.
export function DocumentsDashboard() {
  const { token, session } = useSystemSession();
  const canViewModule = canAccessDocumentsModule(session);
  const [filters, setFilters] = useState<DocumentQueryState>(
    createDefaultDocumentQuery(),
  );
  const [appliedFilters, setAppliedFilters] = useState<DocumentQueryState>(
    createDefaultDocumentQuery(),
  );
  const [selectedItemId, setSelectedItemId] = useState<string | null>(null);
  const deferredSearch = useDeferredValue(appliedFilters.search);

  const workspaceQuery = useQuery({
    enabled: Boolean(session.lojaAtivaId && canViewModule),
    queryFn: () => getDocumentsWorkspace(token),
    queryKey: queryKeys.documentsWorkspace(token, session.lojaAtivaId),
    staleTime: 1000 * 60 * 5,
  });

  const selectedType = useMemo(
    () =>
      getSelectedDocumentType(
        workspaceQuery.data?.tiposDocumento,
        appliedFilters.tipoDocumento,
      ),
    [appliedFilters.tipoDocumento, workspaceQuery.data?.tiposDocumento],
  );

  const searchQuery = useQuery({
    enabled: Boolean(session.lojaAtivaId && canViewModule && appliedFilters.tipoDocumento),
    queryFn: () => {
      if (!appliedFilters.tipoDocumento) {
        throw new Error("Selecione um tipo de documento.");
      }

      return searchDocuments(token, appliedFilters.tipoDocumento, deferredSearch);
    },
    queryKey: queryKeys.documentsSearch(
      token,
      session.lojaAtivaId,
      appliedFilters.tipoDocumento || null,
      deferredSearch,
    ),
  });

  const selectedItem = useMemo(
    () => resolveSelectedDocument(selectedItemId, searchQuery.data),
    [searchQuery.data, selectedItemId],
  );

  const printMutation = useMutation({
    mutationFn: async () => {
      if (!selectedItem || !selectedType) {
        throw new Error("Selecione um documento para imprimir.");
      }

      return printDocument(token, selectedType.codigo, selectedItem.id);
    },
    onError: (error) => {
      toast.error(getErrorMessage(error));
    },
    onSuccess: ({ blob, fileName }) => {
      const objectUrl = URL.createObjectURL(blob);
      const popup = window.open(objectUrl, "_blank", "noopener,noreferrer");

      if (!popup) {
        const link = document.createElement("a");
        link.href = objectUrl;
        link.download = fileName;
        link.click();
      }

      window.setTimeout(() => URL.revokeObjectURL(objectUrl), 15000);
      toast.success("Documento gerado com sucesso.");
    },
  });

  useEffect(() => {
    if (workspaceQuery.isError) {
      toast.error(getErrorMessage(workspaceQuery.error));
    }
  }, [workspaceQuery.error, workspaceQuery.isError]);

  useEffect(() => {
    if (searchQuery.isError) {
      toast.error(getErrorMessage(searchQuery.error));
    }
  }, [searchQuery.error, searchQuery.isError]);

  useEffect(() => {
    const firstType = workspaceQuery.data?.tiposDocumento[0]?.codigo;
    if (!firstType) {
      return;
    }

    if (!filters.tipoDocumento || !appliedFilters.tipoDocumento) {
      startTransition(() => {
        setFilters((current) => ({
          ...current,
          tipoDocumento: (current.tipoDocumento || firstType) as DocumentQueryState["tipoDocumento"],
        }));
        setAppliedFilters((current) => ({
          ...current,
          tipoDocumento:
            (current.tipoDocumento || firstType) as DocumentQueryState["tipoDocumento"],
        }));
      });
    }
  }, [appliedFilters.tipoDocumento, filters.tipoDocumento, workspaceQuery.data]);

  if (session.lojas.length === 0 || !session.lojaAtivaId) {
    return (
      <AccessStateCard
        message="Crie a primeira loja ou selecione uma loja ativa para usar o modulo de impressoes."
        subtitle="Os documentos seguem o contexto operacional da loja ativa."
        title="Loja ativa obrigatoria"
      />
    );
  }

  if (!canViewModule) {
    return (
      <AccessStateCard
        message="Solicite permissao de pecas, vendas ou financeiro para acessar este modulo."
        subtitle="Sua conta nao possui acesso ao modulo de impressoes e documentos."
        title="Modulo sem permissao"
      />
    );
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    const parsed = documentQuerySchema.safeParse(filters);
    if (!parsed.success) {
      toast.error(getZodErrorMessage(parsed.error));
      return;
    }

    setAppliedFilters({
      search: parsed.data.search,
      tipoDocumento: parsed.data.tipoDocumento as DocumentQueryState["tipoDocumento"],
    });
  }

  const busy =
    workspaceQuery.isLoading || searchQuery.isLoading || printMutation.isPending;

  return (
    <div className="documents-page-shell">
      <div className="documents-page-top">
        <DocumentsOverview
          onSelectType={(type) => {
            setSelectedItemId(null);
            setFilters((current) => ({
              ...current,
              tipoDocumento: type as DocumentQueryState["tipoDocumento"],
            }));
            setAppliedFilters((current) => ({
              ...current,
              tipoDocumento: type as DocumentQueryState["tipoDocumento"],
            }));
          }}
          selectedType={appliedFilters.tipoDocumento}
          types={workspaceQuery.data?.tiposDocumento ?? []}
        />
      </div>

      <div className="documents-workspace-grid">
        <DocumentSearchPanel
          busy={busy}
          filters={filters}
          onSubmit={handleSubmit}
          results={searchQuery.data ?? []}
          selectedItemId={selectedItem?.id ?? null}
          setFilters={setFilters}
          setSelectedItemId={setSelectedItemId}
          types={workspaceQuery.data?.tiposDocumento ?? []}
        />
        <DocumentPreviewPanel
          busy={busy}
          item={selectedItem}
          onPrint={async () => {
            await printMutation.mutateAsync();
          }}
          selectedType={selectedType}
        />
      </div>
    </div>
  );
}
