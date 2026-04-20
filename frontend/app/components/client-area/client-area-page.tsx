"use client";

import { useQuery } from "@tanstack/react-query";
import { startTransition, useEffect, useMemo, useState } from "react";
import { toast } from "sonner";

import {
  defaultClientAreaTableSettings,
  getClientAreaInventory,
  getStoredClientAreaTableSettings,
  initialClientAreaFilters,
  persistClientAreaTableSettings,
  type ClientAreaFilters,
  type ClientAreaTableSettings,
} from "@/lib/client-area";
import { getAuthToken } from "@/lib/store";
import { ClientAreaFiltersBar } from "./client-area-filters-bar";
import { ClientAreaPagination } from "./client-area-pagination";
import { ClientAreaSettingsModal } from "./client-area-settings-modal";
import { ClientAreaTable } from "./client-area-table";

function ClientAreaEmptyState({
  title,
  description,
}: {
  title: string;
  description: string;
}) {
  return (
    <div className="rounded-[24px] border border-dashed border-[var(--border-strong)] bg-[var(--surface-muted)]/55 px-6 py-12 text-center">
      <h2 className="text-lg font-semibold text-[var(--foreground)]">{title}</h2>
      <p className="mx-auto mt-2 max-w-2xl text-sm text-[var(--muted)]">{description}</p>
    </div>
  );
}

export function ClientAreaPage() {
  const token = useMemo(() => (typeof window === "undefined" ? null : getAuthToken()), []);
  const [isSettingsModalOpen, setIsSettingsModalOpen] = useState(false);
  const [tableSettings, setTableSettings] = useState<ClientAreaTableSettings>(() =>
    getStoredClientAreaTableSettings(),
  );
  const [filters, setFilters] = useState<ClientAreaFilters>(() => ({
    ...initialClientAreaFilters,
    tamanhoPagina: getStoredClientAreaTableSettings().tamanhoPagina,
  }));
  const [debouncedTextFilters, setDebouncedTextFilters] = useState(() => ({
    loja: initialClientAreaFilters.loja,
    produto: initialClientAreaFilters.produto,
    descricao: initialClientAreaFilters.descricao,
    marca: initialClientAreaFilters.marca,
    tamanho: initialClientAreaFilters.tamanho,
    cor: initialClientAreaFilters.cor,
  }));

  useEffect(() => {
    const timeoutId = window.setTimeout(() => {
      setDebouncedTextFilters({
        loja: filters.loja,
        produto: filters.produto,
        descricao: filters.descricao,
        marca: filters.marca,
        tamanho: filters.tamanho,
        cor: filters.cor,
      });
    }, 300);

    return () => {
      window.clearTimeout(timeoutId);
    };
  }, [
    filters.cor,
    filters.descricao,
    filters.loja,
    filters.marca,
    filters.produto,
    filters.tamanho,
  ]);

  const queryFilters = useMemo<ClientAreaFilters>(
    () => ({
      ...filters,
      ...debouncedTextFilters,
    }),
    [debouncedTextFilters, filters],
  );

  function handleFilterChange(next: Partial<ClientAreaFilters>) {
    setFilters((current) => ({
      ...current,
      ...next,
      pagina: next.pagina ?? 1,
    }));
  }

  const inventoryQuery = useQuery({
    queryKey: ["client-area-products", token, queryFilters],
    queryFn: async () => {
      if (!token) {
        return null;
      }

      return getClientAreaInventory(token, queryFilters);
    },
    enabled: Boolean(token),
  });

  const listResponse = inventoryQuery.data;
  const products = listResponse?.itens ?? [];

  return (
    <section className="space-y-6">
      <div className="rounded-[30px] border border-[var(--border)] bg-[linear-gradient(135deg,_#fffef9,_#f4f7ff_50%,_#eef6f1)] p-6">
        <div className="flex flex-col gap-4 lg:flex-row lg:items-end lg:justify-between">
          <div className="space-y-2">
            <span className="inline-flex rounded-full bg-[#eef4ea] px-3 py-1 text-xs font-semibold uppercase tracking-[0.22em] text-[#52624d]">
              Area do cliente
            </span>
            <div>
              <h1 className="text-3xl font-semibold tracking-tight text-[var(--foreground)]">
                Pecas em todas as lojas
              </h1>
              <p className="mt-2 max-w-3xl text-sm text-[var(--muted)]">
                Visualize as pecas em que voce esta vinculado como fornecedor nas lojas acessiveis.
              </p>
            </div>
          </div>
          <div className="rounded-3xl border border-white/70 bg-white/75 px-5 py-4">
            <p className="text-xs font-semibold uppercase tracking-[0.2em] text-[var(--muted)]">
              Pecas localizadas
            </p>
            <p className="mt-1 text-3xl font-semibold text-[var(--foreground)]">
              {products.length}
            </p>
          </div>
        </div>
      </div>

      <div className="rounded-[28px] border border-[var(--border)] bg-white p-6 shadow-[0_18px_45px_rgba(15,23,42,0.05)]">
        <ClientAreaFiltersBar
          filters={filters}
          isLoading={inventoryQuery.isLoading}
          onOpenSettings={() => setIsSettingsModalOpen(true)}
          onChange={handleFilterChange}
        />

        {inventoryQuery.isLoading ? (
          <ClientAreaEmptyState
            title="Carregando suas pecas"
            description="Buscando os vinculos do cliente logado em todas as lojas acessiveis."
          />
        ) : inventoryQuery.isError ? (
          <ClientAreaEmptyState
            title="Falha ao carregar pecas"
            description={
              inventoryQuery.error instanceof Error
                ? inventoryQuery.error.message
                : "Tente novamente em instantes."
            }
          />
        ) : products.length === 0 ? (
          <ClientAreaEmptyState
            title="Nenhuma peca encontrada"
            description="Seu usuario ainda nao esta vinculado como fornecedor com pecas cadastradas nas lojas acessiveis."
          />
        ) : (
          <>
            <ClientAreaTable
              products={products}
              visibleFields={tableSettings.visibleFields}
            />
            <ClientAreaPagination
              currentPage={filters.pagina}
              hasNextPage={Boolean(listResponse && filters.pagina < listResponse.totalPaginas)}
              hasPreviousPage={filters.pagina > 1}
              totalItems={listResponse?.totalItens ?? 0}
              totalPages={listResponse?.totalPaginas ?? 0}
              onPageChange={(pagina) => handleFilterChange({ pagina })}
            />
          </>
        )}
      </div>

      <ClientAreaSettingsModal
        isOpen={isSettingsModalOpen}
        settings={tableSettings}
        onClose={() => setIsSettingsModalOpen(false)}
        onSave={(nextSettings) => {
          const normalizedSettings = {
            ...defaultClientAreaTableSettings,
            ...nextSettings,
          };

          startTransition(() => {
            setTableSettings(normalizedSettings);
            persistClientAreaTableSettings(normalizedSettings);
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
