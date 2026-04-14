"use client";

import { useQuery } from "@tanstack/react-query";
import { startTransition, useEffect, useMemo, useState } from "react";
import { toast } from "sonner";

import { MovementEmptyState } from "@/app/components/movement/movement-empty-state";
import { useStoreContext } from "@/app/dashboard/store-context";
import {
  asPaymentListResponse,
  defaultPaymentTableSettings,
  getPaymentApiMessage,
  getStoredPaymentTableSettings,
  initialPaymentFilters,
  persistPaymentTableSettings,
  type PaymentFilters,
  type PaymentTableSettings,
} from "@/lib/payment";
import { getAuthToken } from "@/lib/store";
import { getPayments } from "@/services/payment-service";

import { PaymentFiltersBar } from "./payment-filters-bar";
import { PaymentCreateModal } from "./payment-create-modal";
import { PaymentPagination } from "./payment-pagination";
import { PaymentSettingsModal } from "./payment-settings-modal";
import { PaymentsTable } from "./payments-table";

export function PaymentListPage() {
  const { isLoadingStores, selectedStore, selectedStoreId } = useStoreContext();
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [isSettingsModalOpen, setIsSettingsModalOpen] = useState(false);
  const [expandedIds, setExpandedIds] = useState<number[]>([]);
  const [tableSettings, setTableSettings] = useState<PaymentTableSettings>(() =>
    getStoredPaymentTableSettings(),
  );
  const [filters, setFilters] = useState<PaymentFilters>(() => ({
    ...initialPaymentFilters,
    tamanhoPagina: getStoredPaymentTableSettings().tamanhoPagina,
  }));
  const [debouncedCliente, setDebouncedCliente] = useState(initialPaymentFilters.cliente);
  const token = useMemo(() => (typeof window === "undefined" ? null : getAuthToken()), []);

  useEffect(() => {
    const timeoutId = window.setTimeout(() => {
      setDebouncedCliente(filters.cliente);
    }, 300);

    return () => {
      window.clearTimeout(timeoutId);
    };
  }, [filters.cliente]);

  const queryFilters = useMemo<PaymentFilters>(
    () => ({
      ...filters,
      cliente: debouncedCliente,
    }),
    [debouncedCliente, filters],
  );

  const paymentsQuery = useQuery({
    queryKey: ["payments", token, selectedStoreId, queryFilters],
    queryFn: async () => {
      if (!token || !selectedStoreId) {
        return null;
      }

      const response = await getPayments(token, selectedStoreId, queryFilters);

      if (!response.ok) {
        throw new Error(
          getPaymentApiMessage(response.body) ?? "Nao foi possivel carregar os pagamentos.",
        );
      }

      return asPaymentListResponse(response.body);
    },
    enabled: Boolean(token && selectedStoreId),
  });

  function handleFilterChange(next: Partial<PaymentFilters>) {
    setFilters((current) => ({
      ...current,
      ...next,
      pagina: next.pagina ?? 1,
    }));
  }

  function handleToggleExpanded(paymentId: number) {
    setExpandedIds((current) =>
      current.includes(paymentId)
        ? current.filter((id) => id !== paymentId)
        : [...current, paymentId],
    );
  }

  const listResponse = paymentsQuery.data;
  const hasStore = Boolean(selectedStoreId);

  return (
    <section className="space-y-6">
      <div className="rounded-[28px] border border-[var(--border)] bg-white p-6 shadow-[0_18px_45px_rgba(15,23,42,0.05)]">
        <PaymentFiltersBar
          filters={filters}
          isLoading={paymentsQuery.isLoading || isLoadingStores}
          onOpenCreateModal={() => setIsCreateModalOpen(true)}
          onOpenSettings={() => setIsSettingsModalOpen(true)}
          onChange={handleFilterChange}
        />

        {!hasStore ? (
          <MovementEmptyState
            title="Selecione uma loja"
            description="A busca de pagamentos depende da loja ativa no topo da pagina."
          />
        ) : paymentsQuery.isLoading ? (
          <MovementEmptyState
            title="Carregando pagamentos"
            description="Buscando os registros mais recentes da loja selecionada."
          />
        ) : paymentsQuery.isError ? (
          <MovementEmptyState
            title="Falha ao carregar pagamentos"
            description={
              paymentsQuery.error instanceof Error
                ? paymentsQuery.error.message
                : "Tente novamente em instantes."
            }
          />
        ) : listResponse && listResponse.itens.length > 0 ? (
          <>
            <PaymentsTable
              expandedIds={expandedIds}
              payments={listResponse.itens}
              visibleFields={tableSettings.visibleFields}
              onToggleExpanded={handleToggleExpanded}
            />
            <PaymentPagination
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
            title="Nenhum pagamento encontrado"
            description="Ajuste os filtros para localizar pagamentos e suas movimentacoes relacionadas."
          />
        )}
      </div>

      <PaymentSettingsModal
        isOpen={isSettingsModalOpen}
        settings={tableSettings}
        onClose={() => setIsSettingsModalOpen(false)}
        onSave={(nextSettings) => {
          const normalizedSettings = {
            ...defaultPaymentTableSettings,
            ...nextSettings,
          };

          startTransition(() => {
            setTableSettings(normalizedSettings);
            persistPaymentTableSettings(normalizedSettings);
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

      <PaymentCreateModal
        isOpen={isCreateModalOpen}
        storeId={selectedStoreId}
        storeName={selectedStore?.nome ?? null}
        onClose={() => setIsCreateModalOpen(false)}
        onSuccess={async () => {
          await paymentsQuery.refetch();
        }}
      />
    </section>
  );
}
