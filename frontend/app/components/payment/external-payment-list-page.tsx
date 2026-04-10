"use client";

import { useQuery } from "@tanstack/react-query";
import { startTransition, useEffect, useMemo, useState } from "react";
import { toast } from "sonner";

import { MovementEmptyState } from "@/app/components/movement/movement-empty-state";
import { useStoreContext } from "@/app/dashboard/store-context";
import {
  asExternalPaymentListResponse,
  defaultExternalPaymentTableSettings,
  getPaymentApiMessage,
  getStoredExternalPaymentTableSettings,
  initialExternalPaymentFilters,
  persistExternalPaymentTableSettings,
  type ExternalPaymentFilters,
  type ExternalPaymentTableSettings,
} from "@/lib/payment";
import { getAuthToken } from "@/lib/store";
import { getExternalPayments } from "@/services/payment-service";

import { ExternalPaymentFiltersBar } from "./external-payment-filters-bar";
import { ExternalPaymentSettingsModal } from "./external-payment-settings-modal";
import { ExternalPaymentsTable } from "./external-payments-table";
import { PaymentPagination } from "./payment-pagination";

export function ExternalPaymentListPage() {
  const { isLoadingStores, selectedStoreId } = useStoreContext();
  const [isSettingsModalOpen, setIsSettingsModalOpen] = useState(false);
  const [tableSettings, setTableSettings] = useState<ExternalPaymentTableSettings>(() =>
    getStoredExternalPaymentTableSettings(),
  );
  const [filters, setFilters] = useState<ExternalPaymentFilters>(() => ({
    ...initialExternalPaymentFilters,
    tamanhoPagina: getStoredExternalPaymentTableSettings().tamanhoPagina,
  }));
  const [debouncedCliente, setDebouncedCliente] = useState(initialExternalPaymentFilters.cliente);
  const token = useMemo(() => (typeof window === "undefined" ? null : getAuthToken()), []);

  useEffect(() => {
    const timeoutId = window.setTimeout(() => {
      setDebouncedCliente(filters.cliente);
    }, 300);

    return () => {
      window.clearTimeout(timeoutId);
    };
  }, [filters.cliente]);

  const queryFilters = useMemo<ExternalPaymentFilters>(
    () => ({
      ...filters,
      cliente: debouncedCliente,
    }),
    [debouncedCliente, filters],
  );

  const paymentsQuery = useQuery({
    queryKey: ["external-payments", token, selectedStoreId, queryFilters],
    queryFn: async () => {
      if (!token || !selectedStoreId) {
        return null;
      }

      const response = await getExternalPayments(token, selectedStoreId, queryFilters);

      if (!response.ok) {
        throw new Error(
          getPaymentApiMessage(response.body) ??
            "Nao foi possivel carregar os pagamentos externos.",
        );
      }

      return asExternalPaymentListResponse(response.body);
    },
    enabled: Boolean(token && selectedStoreId),
  });

  function handleFilterChange(next: Partial<ExternalPaymentFilters>) {
    setFilters((current) => ({
      ...current,
      ...next,
      pagina: next.pagina ?? 1,
    }));
  }

  const listResponse = paymentsQuery.data;
  const hasStore = Boolean(selectedStoreId);

  return (
    <section className="space-y-6">
      <div className="rounded-[28px] border border-[var(--border)] bg-white p-6 shadow-[0_18px_45px_rgba(15,23,42,0.05)]">
        <ExternalPaymentFiltersBar
          filters={filters}
          isLoading={paymentsQuery.isLoading || isLoadingStores}
          onOpenSettings={() => setIsSettingsModalOpen(true)}
          onChange={handleFilterChange}
        />

        {!hasStore ? (
          <MovementEmptyState
            title="Selecione uma loja"
            description="A busca de pagamentos externos depende da loja ativa no topo da pagina."
          />
        ) : paymentsQuery.isLoading ? (
          <MovementEmptyState
            title="Carregando pagamentos externos"
            description="Buscando os lancamentos externos mais recentes da loja selecionada."
          />
        ) : paymentsQuery.isError ? (
          <MovementEmptyState
            title="Falha ao carregar pagamentos externos"
            description={
              paymentsQuery.error instanceof Error
                ? paymentsQuery.error.message
                : "Tente novamente em instantes."
            }
          />
        ) : listResponse && listResponse.itens.length > 0 ? (
          <>
            <ExternalPaymentsTable
              payments={listResponse.itens}
              visibleFields={tableSettings.visibleFields}
            />
            <PaymentPagination
              currentPage={filters.pagina}
              hasNextPage={filters.pagina < listResponse.totalPaginas}
              hasPreviousPage={filters.pagina > 1}
              totalItems={listResponse.totalItens}
              totalPages={listResponse.totalPaginas}
              itemLabel="pagamento(s) externo(s) encontrado(s)"
              onPageChange={(pagina) => handleFilterChange({ pagina })}
            />
          </>
        ) : (
          <MovementEmptyState
            title="Nenhum pagamento externo encontrado"
            description="Ajuste os filtros para localizar os lancamentos externos da loja."
          />
        )}
      </div>

      <ExternalPaymentSettingsModal
        isOpen={isSettingsModalOpen}
        settings={tableSettings}
        onClose={() => setIsSettingsModalOpen(false)}
        onSave={(nextSettings) => {
          const normalizedSettings = {
            ...defaultExternalPaymentTableSettings,
            ...nextSettings,
          };

          startTransition(() => {
            setTableSettings(normalizedSettings);
            persistExternalPaymentTableSettings(normalizedSettings);
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
