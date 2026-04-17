"use client";

import { useQuery } from "@tanstack/react-query";
import { useMemo, useState } from "react";

import { MovementEmptyState } from "@/app/components/movement/movement-empty-state";
import { PaymentPagination } from "@/app/components/payment/payment-pagination";
import { useStoreContext } from "@/app/dashboard/store-context";
import { permissions } from "@/lib/access";
import { getAuthToken } from "@/lib/store";
import {
  asStoreExpenseListResponse,
  getStoreExpenseApiMessage,
  initialStoreExpenseFilters,
} from "@/lib/store-expense";
import { getStoreExpenses } from "@/services/store-expense-service";

import { StoreExpenseCreateModal } from "./store-expense-create-modal";
import { StoreExpensesTable } from "./store-expenses-table";

export function StoreExpenseListPage() {
  const { hasPermission, isLoadingStores, selectedStore, selectedStoreId } = useStoreContext();
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [page, setPage] = useState(initialStoreExpenseFilters.pagina);
  const token = useMemo(() => (typeof window === "undefined" ? null : getAuthToken()), []);
  const canCreateExpense = hasPermission(permissions.gastosLojaAdicionar);

  const expensesQuery = useQuery({
    queryKey: ["store-expenses", token, selectedStoreId, page],
    queryFn: async () => {
      if (!token || !selectedStoreId) {
        return null;
      }

      const response = await getStoreExpenses(token, selectedStoreId, {
        ...initialStoreExpenseFilters,
        pagina: page,
      });

      if (!response.ok) {
        throw new Error(
          getStoreExpenseApiMessage(response.body) ??
            "Nao foi possivel carregar os gastos da loja.",
        );
      }

      return asStoreExpenseListResponse(response.body);
    },
    enabled: Boolean(token && selectedStoreId),
  });

  const listResponse = expensesQuery.data;
  const hasStore = Boolean(selectedStoreId);

  return (
    <section className="space-y-6">
      <div className="rounded-[28px] border border-[var(--border)] bg-white p-6 shadow-[0_18px_45px_rgba(15,23,42,0.05)]">
        <div className="flex flex-col gap-4 lg:flex-row lg:items-end lg:justify-between">
          <div>
            <h2 className="text-xl font-semibold text-[var(--foreground)]">
              Gastos da loja
            </h2>
            <p className="mt-1 text-sm text-[var(--muted)]">
              Lancamentos administrativos sem relacao com clientes.
            </p>
          </div>

          {canCreateExpense ? (
            <button
              type="button"
              onClick={() => setIsCreateModalOpen(true)}
              disabled={expensesQuery.isLoading || isLoadingStores}
              className="flex h-12 cursor-pointer items-center justify-center rounded-2xl bg-[linear-gradient(90deg,_#ff8a3d,_#ff6b3d)] px-5 text-sm font-semibold text-white shadow-[0_16px_30px_rgba(255,107,61,0.28)] transition hover:brightness-105 disabled:cursor-not-allowed disabled:opacity-60"
            >
              Novo lancamento
            </button>
          ) : null}
        </div>

        {!hasStore ? (
          <MovementEmptyState
            title="Selecione uma loja"
            description="A listagem de gastos depende da loja ativa no topo da pagina."
          />
        ) : expensesQuery.isLoading ? (
          <MovementEmptyState
            title="Carregando gastos da loja"
            description="Buscando os lancamentos administrativos mais recentes da loja selecionada."
          />
        ) : expensesQuery.isError ? (
          <MovementEmptyState
            title="Falha ao carregar gastos da loja"
            description={
              expensesQuery.error instanceof Error
                ? expensesQuery.error.message
                : "Tente novamente em instantes."
            }
          />
        ) : listResponse && listResponse.itens.length > 0 ? (
          <>
            <StoreExpensesTable expenses={listResponse.itens} />
            <PaymentPagination
              currentPage={page}
              hasNextPage={page < listResponse.totalPaginas}
              hasPreviousPage={page > 1}
              totalItems={listResponse.totalItens}
              totalPages={listResponse.totalPaginas}
              itemLabel="lancamento(s) encontrado(s)"
              onPageChange={setPage}
            />
          </>
        ) : (
          <MovementEmptyState
            title="Nenhum gasto encontrado"
            description="Cadastre lancamentos como compra de cabide, conta de luz ou reformas da loja."
          />
        )}
      </div>

      {canCreateExpense ? (
        <StoreExpenseCreateModal
          isOpen={isCreateModalOpen}
          storeId={selectedStoreId}
          storeName={selectedStore?.nome ?? null}
          onClose={() => setIsCreateModalOpen(false)}
          onSuccess={async () => {
            setPage(1);
            await expensesQuery.refetch();
          }}
        />
      ) : null}
    </section>
  );
}
