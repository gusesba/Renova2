"use client";

import { useQuery } from "@tanstack/react-query";
import { useRouter } from "next/navigation";
import { startTransition, useEffect, useMemo, useState } from "react";
import { toast } from "sonner";

import { useStoreContext } from "@/app/dashboard/store-context";
import { getAuthToken } from "@/lib/store";
import {
  asMovementListResponse,
  defaultMovementTableSettings,
  getMovementApiMessage,
  getStoredMovementTableSettings,
  initialMovementFilters,
  persistMovementTableSettings,
  type MovementFilters,
  type MovementTableSettings,
} from "@/lib/movement";
import { getMovements } from "@/services/movement-service";

import { MovementEmptyState } from "./movement-empty-state";
import { MovementFiltersBar } from "./movement-filters-bar";
import { MovementPagination } from "./movement-pagination";
import { MovementSettingsModal } from "./movement-settings-modal";
import { MovementsTable } from "./movements-table";

export function MovementListPage() {
  const { isLoadingStores, selectedStoreId } = useStoreContext();
  const router = useRouter();
  const [isSettingsModalOpen, setIsSettingsModalOpen] = useState(false);
  const [expandedIds, setExpandedIds] = useState<number[]>([]);
  const [tableSettings, setTableSettings] = useState<MovementTableSettings>(() =>
    getStoredMovementTableSettings(),
  );
  const [filters, setFilters] = useState<MovementFilters>(() => ({
    ...initialMovementFilters,
    tamanhoPagina: getStoredMovementTableSettings().tamanhoPagina,
  }));
  const [debouncedCliente, setDebouncedCliente] = useState(initialMovementFilters.cliente);
  const token = useMemo(() => (typeof window === "undefined" ? null : getAuthToken()), []);

  useEffect(() => {
    const timeoutId = window.setTimeout(() => {
      setDebouncedCliente(filters.cliente);
    }, 300);

    return () => {
      window.clearTimeout(timeoutId);
    };
  }, [filters.cliente]);

  const queryFilters = useMemo<MovementFilters>(
    () => ({
      ...filters,
      cliente: debouncedCliente,
    }),
    [debouncedCliente, filters],
  );

  const movementsQuery = useQuery({
    queryKey: ["movements", token, selectedStoreId, queryFilters],
    queryFn: async () => {
      if (!token || !selectedStoreId) {
        return null;
      }

      const response = await getMovements(token, selectedStoreId, queryFilters);

      if (!response.ok) {
        throw new Error(
          getMovementApiMessage(response.body) ?? "Nao foi possivel carregar as movimentacoes.",
        );
      }

      return asMovementListResponse(response.body);
    },
    enabled: Boolean(token && selectedStoreId),
  });

  function handleFilterChange(next: Partial<MovementFilters>) {
    setFilters((current) => ({
      ...current,
      ...next,
      pagina: next.pagina ?? 1,
    }));
  }

  function handleAddMovement() {
    if (!selectedStoreId) {
      toast.error("Selecione uma loja antes de criar movimentacoes.");
      return;
    }

    router.push("/dashboard/movimentacao/nova");
  }

  function handleOpenDestination() {
    if (!selectedStoreId) {
      toast.error("Selecione uma loja antes de abrir a destinacao por permanencia.");
      return;
    }

    router.push("/dashboard/movimentacao/doacao-devolucao");
  }

  function handleToggleExpanded(movementId: number) {
    setExpandedIds((current) =>
      current.includes(movementId)
        ? current.filter((id) => id !== movementId)
        : [...current, movementId],
    );
  }

  const listResponse = movementsQuery.data;
  const hasStore = Boolean(selectedStoreId);

  return (
    <section className="space-y-6">
      <div className="rounded-[28px] border border-[var(--border)] bg-white p-6 shadow-[0_18px_45px_rgba(15,23,42,0.05)]">
        <MovementFiltersBar
          filters={filters}
          hasStore={hasStore}
          isLoading={movementsQuery.isLoading || isLoadingStores}
          onAddMovement={handleAddMovement}
          onOpenDestination={handleOpenDestination}
          onOpenSettings={() => setIsSettingsModalOpen(true)}
          onChange={handleFilterChange}
        />

        {!hasStore ? (
          <MovementEmptyState
            title="Selecione uma loja"
            description="A busca e o cadastro de movimentacoes dependem da loja ativa no topo da pagina."
          />
        ) : movementsQuery.isLoading ? (
          <MovementEmptyState
            title="Carregando movimentacoes"
            description="Buscando os registros mais recentes da loja selecionada."
          />
        ) : movementsQuery.isError ? (
          <MovementEmptyState
            title="Falha ao carregar movimentacoes"
            description={
              movementsQuery.error instanceof Error
                ? movementsQuery.error.message
                : "Tente novamente em instantes."
            }
          />
        ) : listResponse && listResponse.itens.length > 0 ? (
          <>
            <MovementsTable
              expandedIds={expandedIds}
              movements={listResponse.itens}
              visibleFields={tableSettings.visibleFields}
              onToggleExpanded={handleToggleExpanded}
            />
            <MovementPagination
              currentPage={filters.pagina}
              hasNextPage={filters.pagina < listResponse.totalPaginas}
              hasPreviousPage={filters.pagina > 1}
              totalItems={listResponse.totalItens}
              totalPages={listResponse.totalPaginas}
              onPageChange={(pagina) => handleFilterChange({ pagina })}
            />
          </>
        ) : (
          <MovementEmptyState
            title="Nenhuma movimentacao encontrada"
            description="Ajuste os filtros ou abra uma nova movimentacao para popular esta tabela."
          />
        )}
      </div>

      <MovementSettingsModal
        isOpen={isSettingsModalOpen}
        settings={tableSettings}
        onClose={() => setIsSettingsModalOpen(false)}
        onSave={(nextSettings) => {
          const normalizedSettings = {
            ...defaultMovementTableSettings,
            ...nextSettings,
          };

          startTransition(() => {
            setTableSettings(normalizedSettings);
            persistMovementTableSettings(normalizedSettings);
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
