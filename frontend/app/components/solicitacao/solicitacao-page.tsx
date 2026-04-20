"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { startTransition, useEffect, useMemo, useState } from "react";
import { toast } from "sonner";

import { useStoreContext } from "@/app/dashboard/store-context";
import { permissions } from "@/lib/access";
import { getAuthToken } from "@/lib/store";
import {
  asSolicitacaoListResponse,
  defaultSolicitacaoTableSettings,
  getSolicitacaoApiMessage,
  getStoredSolicitacaoTableSettings,
  initialSolicitacaoFilters,
  persistSolicitacaoTableSettings,
  type SolicitacaoListItem,
  type SolicitacaoFilters,
  type SolicitacaoTableSettings,
} from "@/lib/solicitacao";
import { deleteSolicitacao, getSolicitacoes } from "@/services/solicitacao-service";

import { SolicitacaoCreateModal } from "./solicitacao-create-modal";
import { SolicitacaoDeleteModal } from "./solicitacao-delete-modal";
import { SolicitacaoEmptyState } from "./solicitacao-empty-state";
import { SolicitacaoFiltersBar } from "./solicitacao-filters-bar";
import { SolicitacaoPagination } from "./solicitacao-pagination";
import { SolicitacaoSettingsModal } from "./solicitacao-settings-modal";
import { SolicitacoesTable } from "./solicitacoes-table";

export function SolicitacaoPage() {
  const queryClient = useQueryClient();
  const { hasPermission, isLoadingStores, selectedStore, selectedStoreId } = useStoreContext();
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
  const [isSettingsModalOpen, setIsSettingsModalOpen] = useState(false);
  const [selectedSolicitacaoId, setSelectedSolicitacaoId] = useState<number | null>(null);
  const [selectedSolicitacaoDescription, setSelectedSolicitacaoDescription] = useState<string | null>(null);
  const [tableSettings, setTableSettings] = useState<SolicitacaoTableSettings>(() =>
    getStoredSolicitacaoTableSettings(),
  );
  const [filters, setFilters] = useState<SolicitacaoFilters>(() => ({
    ...initialSolicitacaoFilters,
    tamanhoPagina: getStoredSolicitacaoTableSettings().tamanhoPagina,
  }));
  const [debouncedTextFilters, setDebouncedTextFilters] = useState(() => ({
    descricao: initialSolicitacaoFilters.descricao,
    produto: initialSolicitacaoFilters.produto,
    marca: initialSolicitacaoFilters.marca,
    tamanho: initialSolicitacaoFilters.tamanho,
    cor: initialSolicitacaoFilters.cor,
    cliente: initialSolicitacaoFilters.cliente,
  }));
  const token = useMemo(() => (typeof window === "undefined" ? null : getAuthToken()), []);
  const canAddSolicitacao = hasPermission(permissions.solicitacoesAdicionar);
  const canDeleteSolicitacao = hasPermission(permissions.solicitacoesExcluir);

  useEffect(() => {
    const timeoutId = window.setTimeout(() => {
      setDebouncedTextFilters({
        descricao: filters.descricao,
        produto: filters.produto,
        marca: filters.marca,
        tamanho: filters.tamanho,
        cor: filters.cor,
        cliente: filters.cliente,
      });
    }, 300);

    return () => {
      window.clearTimeout(timeoutId);
    };
  }, [filters.cliente, filters.cor, filters.descricao, filters.marca, filters.produto, filters.tamanho]);

  const queryFilters = useMemo<SolicitacaoFilters>(
    () => ({
      ...filters,
      ...debouncedTextFilters,
    }),
    [debouncedTextFilters, filters],
  );

  const solicitacoesQuery = useQuery({
    queryKey: ["solicitacoes", token, selectedStoreId, queryFilters],
    queryFn: async () => {
      if (!token || !selectedStoreId) {
        return null;
      }

      const response = await getSolicitacoes(token, selectedStoreId, queryFilters);

      if (!response.ok) {
        throw new Error(
          getSolicitacaoApiMessage(response.body) ??
            "Nao foi possivel carregar as solicitacoes.",
        );
      }

      return asSolicitacaoListResponse(response.body);
    },
    enabled: Boolean(token && selectedStoreId),
  });

  const deleteSolicitacaoMutation = useMutation({
    mutationFn: async (solicitacaoId: number) => {
      if (!token) {
        throw new Error("Voce precisa estar autenticado para excluir uma solicitacao.");
      }

      return deleteSolicitacao(solicitacaoId, token);
    },
  });

  function handleFilterChange(next: Partial<SolicitacaoFilters>) {
    setFilters((current) => ({
      ...current,
      ...next,
      pagina: next.pagina ?? 1,
    }));
  }

  function handleOpenDeleteModal(solicitacao: SolicitacaoListItem) {
    setSelectedSolicitacaoId(solicitacao.id);
    setSelectedSolicitacaoDescription(
      `${solicitacao.produto} para ${solicitacao.cliente}`.trim(),
    );
    setIsDeleteModalOpen(true);
  }

  function handleCloseDeleteModal() {
    if (deleteSolicitacaoMutation.isPending) {
      return;
    }

    setIsDeleteModalOpen(false);
    setSelectedSolicitacaoId(null);
    setSelectedSolicitacaoDescription(null);
  }

  async function handleDeleteConfirm() {
    if (!selectedSolicitacaoId) {
      toast.error("Selecione uma solicitacao valida para excluir.");
      return;
    }

    try {
      const response = await deleteSolicitacaoMutation.mutateAsync(selectedSolicitacaoId);

      if (!response.ok) {
        toast.error(
          getSolicitacaoApiMessage(response.body) ?? "Nao foi possivel excluir a solicitacao.",
        );
        return;
      }

      const deletedSolicitacaoDescription = selectedSolicitacaoDescription;

      startTransition(() => {
        setIsDeleteModalOpen(false);
        setSelectedSolicitacaoId(null);
        setSelectedSolicitacaoDescription(null);
      });

      await queryClient.invalidateQueries({ queryKey: ["solicitacoes"] });
      toast.success(
        deletedSolicitacaoDescription
          ? `Solicitacao ${deletedSolicitacaoDescription} excluida com sucesso.`
          : "Solicitacao excluida com sucesso.",
      );
    } catch (error) {
      toast.error(
        error instanceof Error
          ? error.message
          : "Nao foi possivel conectar ao backend. Verifique se a API esta em execucao.",
      );
    }
  }

  const listResponse = solicitacoesQuery.data;
  const hasStore = Boolean(selectedStoreId);

  return (
    <section className="space-y-6">
      <div className="rounded-[28px] border border-[var(--border)] bg-white p-6 shadow-[0_18px_45px_rgba(15,23,42,0.05)]">
        <SolicitacaoFiltersBar
          canAddSolicitacao={canAddSolicitacao}
          filters={filters}
          hasStore={hasStore}
          isLoading={solicitacoesQuery.isLoading || isLoadingStores}
          onAddSolicitacao={() => setIsCreateModalOpen(true)}
          onOpenSettings={() => setIsSettingsModalOpen(true)}
          onChange={handleFilterChange}
        />

        {!hasStore ? (
          <SolicitacaoEmptyState
            title="Selecione uma loja"
            description="A busca e o cadastro de solicitacoes dependem da loja ativa no topo da pagina."
          />
        ) : solicitacoesQuery.isLoading ? (
          <SolicitacaoEmptyState
            title="Carregando solicitacoes"
            description="Buscando os registros mais recentes da loja selecionada."
          />
        ) : solicitacoesQuery.isError ? (
          <SolicitacaoEmptyState
            title="Falha ao carregar solicitacoes"
            description={
              solicitacoesQuery.error instanceof Error
                ? solicitacoesQuery.error.message
                : "Tente novamente em instantes."
            }
          />
        ) : listResponse && listResponse.itens.length > 0 ? (
          <>
            <SolicitacoesTable
              canDeleteSolicitacao={canDeleteSolicitacao}
              onDeleteSolicitacao={handleOpenDeleteModal}
              solicitacoes={listResponse.itens}
              visibleFields={tableSettings.visibleFields}
            />
            <SolicitacaoPagination
              currentPage={filters.pagina}
              hasNextPage={filters.pagina < listResponse.totalPaginas}
              hasPreviousPage={filters.pagina > 1}
              totalItems={listResponse.totalItens}
              totalPages={listResponse.totalPaginas}
              onPageChange={(pagina) => handleFilterChange({ pagina })}
            />
          </>
        ) : (
          <SolicitacaoEmptyState
            title="Nenhuma solicitacao encontrada"
            description="Ajuste os filtros ou cadastre uma nova solicitacao para comecar a popular esta tabela."
          />
        )}
      </div>

      {canAddSolicitacao ? (
        <SolicitacaoCreateModal
          isOpen={isCreateModalOpen}
          storeId={selectedStoreId}
          storeName={selectedStore?.nome ?? null}
          onClose={() => setIsCreateModalOpen(false)}
          onSolicitacaoCreated={(solicitacao) => {
            if (solicitacao.produtosCompativeis.length > 0) {
              toast.success(
                `${solicitacao.produtosCompativeis.length} produto(s) compativel(is) encontrado(s).`,
              );
            }
          }}
        />
      ) : null}
      <SolicitacaoDeleteModal
        description={selectedSolicitacaoDescription}
        isOpen={isDeleteModalOpen}
        isSubmitting={deleteSolicitacaoMutation.isPending}
        onClose={handleCloseDeleteModal}
        onConfirm={handleDeleteConfirm}
      />
      <SolicitacaoSettingsModal
        isOpen={isSettingsModalOpen}
        settings={tableSettings}
        onClose={() => setIsSettingsModalOpen(false)}
        onSave={(nextSettings) => {
          const normalizedSettings = {
            ...defaultSolicitacaoTableSettings,
            ...nextSettings,
          };

          startTransition(() => {
            setTableSettings(normalizedSettings);
            persistSolicitacaoTableSettings(normalizedSettings);
            setFilters((current) => ({
              ...current,
              pagina: 1,
              tamanhoPagina: normalizedSettings.tamanhoPagina,
            }));
            setIsSettingsModalOpen(false);
          });

          toast.success("Configuracoes da tabela atualizadas.");
        }}
      />
    </section>
  );
}
