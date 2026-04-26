"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { startTransition, useEffect, useMemo, useRef, useState } from "react";
import { toast } from "sonner";

import { useStoreContext } from "@/app/dashboard/store-context";
import { permissions } from "@/lib/access";
import { getAuthToken } from "@/lib/store";
import {
  asProductListResponse,
  defaultProductTableSettings,
  getProductApiMessage,
  getStoredProductTableSettings,
  initialProductFilters,
  persistProductTableSettings,
  type ProductFilters,
  type ProductRequestMatchItem,
  type ProductListItem,
  type ProductTableSettings,
} from "@/lib/product";
import { deleteProduct, getProducts } from "@/services/product-service";

import { ProductCreateModal } from "./product-create-modal";
import { ProductDeleteModal } from "./product-delete-modal";
import { ProductEditModal } from "./product-edit-modal";
import { ProductEmptyState } from "./product-empty-state";
import { ProductFiltersBar } from "./product-filters-bar";
import { ProductPagination } from "./product-pagination";
import { ProductRequestMatchModal } from "./product-request-match-modal";
import { ProductSettingsModal } from "./product-settings-modal";
import { ProductsTable } from "./products-table";
import { PrintConfirmationModal } from "@/app/components/printing/print-confirmation-modal";
import { openLabelPdfPreview, printLabels } from "@/lib/printing/actions";
import { getStoredPrintSettings } from "@/lib/printing/settings";
import type { PrintSettings } from "@/lib/printing/types";

export function ProductPage() {
  const queryClient = useQueryClient();
  const { hasPermission, isLoadingStores, selectedStore, selectedStoreId } = useStoreContext();
  const canAddProduct = hasPermission(permissions.produtosAdicionar);
  const canEditProduct = hasPermission(permissions.produtosEditar);
  const canDeleteProduct = hasPermission(permissions.produtosExcluir);
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
  const [isPrintConfirmationOpen, setIsPrintConfirmationOpen] = useState(false);
  const [isPrintingLabels, setIsPrintingLabels] = useState(false);
  const [isSettingsModalOpen, setIsSettingsModalOpen] = useState(false);
  const [printSettings, setPrintSettings] = useState<PrintSettings>(() => getStoredPrintSettings());
  const [isRequestMatchModalOpen, setIsRequestMatchModalOpen] = useState(false);
  const [requestMatches, setRequestMatches] = useState<ProductRequestMatchItem[]>([]);
  const [recentlyCreatedProductDescription, setRecentlyCreatedProductDescription] = useState<string | null>(null);
  const [selectedProduct, setSelectedProduct] = useState<ProductListItem | null>(null);
  const [selectedProductIds, setSelectedProductIds] = useState<number[]>([]);
  const [selectedProductName, setSelectedProductName] = useState<string | null>(null);
  const editModalCleanupTimeoutRef = useRef<number | null>(null);
  const [tableSettings, setTableSettings] = useState<ProductTableSettings>(() =>
    getStoredProductTableSettings(),
  );
  const [filters, setFilters] = useState<ProductFilters>(() => ({
    ...initialProductFilters,
    tamanhoPagina: getStoredProductTableSettings().tamanhoPagina,
  }));
  const [debouncedTextFilters, setDebouncedTextFilters] = useState(() => ({
    descricao: initialProductFilters.descricao,
    produto: initialProductFilters.produto,
    marca: initialProductFilters.marca,
    tamanho: initialProductFilters.tamanho,
    cor: initialProductFilters.cor,
    fornecedor: initialProductFilters.fornecedor,
  }));
  const token = useMemo(() => (typeof window === "undefined" ? null : getAuthToken()), []);

  useEffect(() => {
    const timeoutId = window.setTimeout(() => {
      setDebouncedTextFilters({
        descricao: filters.descricao,
        produto: filters.produto,
        marca: filters.marca,
        tamanho: filters.tamanho,
        cor: filters.cor,
        fornecedor: filters.fornecedor,
      });
    }, 300);

    return () => {
      window.clearTimeout(timeoutId);
    };
  }, [
    filters.cor,
    filters.descricao,
    filters.fornecedor,
    filters.marca,
    filters.produto,
    filters.tamanho,
  ]);

  const queryFilters = useMemo<ProductFilters>(
    () => ({
      ...filters,
      ...debouncedTextFilters,
    }),
    [debouncedTextFilters, filters],
  );

  const productsQuery = useQuery({
    queryKey: ["products", token, selectedStoreId, queryFilters],
    queryFn: async () => {
      if (!token || !selectedStoreId) {
        return null;
      }

      const response = await getProducts(token, selectedStoreId, queryFilters);

      if (!response.ok) {
        throw new Error(
          getProductApiMessage(response.body) ?? "Nao foi possivel carregar os produtos.",
        );
      }

      return asProductListResponse(response.body);
    },
    enabled: Boolean(token && selectedStoreId),
  });

  const deleteProductMutation = useMutation({
    mutationFn: async (productId: number) => {
      if (!token) {
        throw new Error("Voce precisa estar autenticado para excluir um produto.");
      }

      return deleteProduct(productId, token);
    },
  });

  function handleFilterChange(next: Partial<ProductFilters>) {
    setFilters((current) => ({
      ...current,
      ...next,
      pagina: next.pagina ?? 1,
    }));
  }

  function handleCloseEditModal() {
    setIsEditModalOpen(false);

    if (editModalCleanupTimeoutRef.current) {
      window.clearTimeout(editModalCleanupTimeoutRef.current);
    }

    editModalCleanupTimeoutRef.current = window.setTimeout(() => {
      setSelectedProduct(null);
      editModalCleanupTimeoutRef.current = null;
    }, 240);
  }

  function handleOpenDeleteModal(product: ProductListItem) {
    setSelectedProduct(product);
    setSelectedProductName(product.id.toString());
    setIsDeleteModalOpen(true);
  }

  function handleCloseDeleteModal() {
    if (deleteProductMutation.isPending) {
      return;
    }

    setIsDeleteModalOpen(false);
    setSelectedProduct(null);
    setSelectedProductName(null);
  }

  useEffect(() => {
    return () => {
      if (editModalCleanupTimeoutRef.current) {
        window.clearTimeout(editModalCleanupTimeoutRef.current);
      }
    };
  }, []);

  async function handleDeleteConfirm() {
    if (!selectedProduct) {
      toast.error("Selecione um produto valido para excluir.");
      return;
    }

    try {
      const response = await deleteProductMutation.mutateAsync(selectedProduct.id);

      if (!response.ok) {
        toast.error(getProductApiMessage(response.body) ?? "Nao foi possivel excluir o produto.");
        return;
      }

      const deletedProductName = selectedProductName;

      startTransition(() => {
        setIsDeleteModalOpen(false);
        setSelectedProduct(null);
        setSelectedProductName(null);
      });

      await queryClient.invalidateQueries({ queryKey: ["products"] });
      toast.success(
        deletedProductName
          ? `Produto ${deletedProductName} excluido com sucesso.`
          : "Produto excluido com sucesso.",
      );
    } catch (error) {
      toast.error(
        error instanceof Error
          ? error.message
          : "Nao foi possivel conectar ao backend. Verifique se a API esta em execucao.",
      );
    }
  }

  function getSelectedProducts({ showError = true } = {}) {
    const products = listResponse?.itens.filter((product) => selectedProductIds.includes(product.id)) ?? [];

    if (products.length === 0 && showError) {
      toast.error("Selecione ao menos um produto visivel para imprimir etiquetas.");
    }

    return products;
  }

  async function handlePrintLabels() {
    const products = getSelectedProducts();

    if (products.length === 0) {
      return;
    }

    setIsPrintingLabels(true);

    try {
      await printLabels(products);
      toast.success("Etiquetas enviadas para impressao.");
      setIsPrintConfirmationOpen(false);
    } catch (error) {
      toast.error(
        error instanceof Error
          ? error.message
          : "Nao foi possivel imprimir as etiquetas.",
      );
    } finally {
      setIsPrintingLabels(false);
    }
  }

  async function handleOpenLabelsPdf() {
    const products = getSelectedProducts();

    if (products.length > 0) {
      await openLabelPdfPreview(products);
    }
  }

  function handleOpenPrintConfirmation() {
    if (getSelectedProducts().length === 0) {
      return;
    }

    setPrintSettings(getStoredPrintSettings());
    setIsPrintConfirmationOpen(true);
  }

  const listResponse = productsQuery.data;
  const hasStore = Boolean(selectedStoreId);

  return (
    <section className="space-y-6">
      <div className="rounded-[28px] border border-[var(--border)] bg-white p-6 shadow-[0_18px_45px_rgba(15,23,42,0.05)]">
        <ProductFiltersBar
          canAddProduct={canAddProduct}
          filters={filters}
          hasStore={hasStore}
          isLoading={productsQuery.isLoading || isLoadingStores}
          onAddProduct={() => setIsCreateModalOpen(true)}
          onOpenPrint={handleOpenPrintConfirmation}
          onOpenSettings={() => setIsSettingsModalOpen(true)}
          onChange={handleFilterChange}
        />

        {!hasStore ? (
          <ProductEmptyState
            title="Selecione uma loja"
            description="A busca e o cadastro de produtos dependem da loja ativa no topo da pagina."
          />
        ) : productsQuery.isLoading ? (
          <ProductEmptyState
            title="Carregando produtos"
            description="Buscando os registros mais recentes da loja selecionada."
          />
        ) : productsQuery.isError ? (
          <ProductEmptyState
            title="Falha ao carregar produtos"
            description={
              productsQuery.error instanceof Error
                ? productsQuery.error.message
                : "Tente novamente em instantes."
            }
          />
        ) : listResponse && listResponse.itens.length > 0 ? (
          <>
            <ProductsTable
              canDeleteProduct={canDeleteProduct}
              canEditProduct={canEditProduct}
              products={listResponse.itens}
              selectedProductIds={selectedProductIds}
              visibleFields={tableSettings.visibleFields}
              onEditProduct={(product) => {
                if (editModalCleanupTimeoutRef.current) {
                  window.clearTimeout(editModalCleanupTimeoutRef.current);
                  editModalCleanupTimeoutRef.current = null;
                }
                setSelectedProduct(product);
                setIsEditModalOpen(true);
              }}
              onDeleteProduct={handleOpenDeleteModal}
              onToggleProductSelection={(productId) =>
                setSelectedProductIds((current) =>
                  current.includes(productId)
                    ? current.filter((item) => item !== productId)
                    : [...current, productId],
                )
              }
              onToggleAllProducts={() => {
                const visibleIds = listResponse.itens.map((product) => product.id);
                const allSelected = visibleIds.every((id) => selectedProductIds.includes(id));

                setSelectedProductIds((current) =>
                  allSelected
                    ? current.filter((id) => !visibleIds.includes(id))
                    : [...new Set([...current, ...visibleIds])],
                );
              }}
            />
            <ProductPagination
              currentPage={filters.pagina}
              hasNextPage={filters.pagina < listResponse.totalPaginas}
              hasPreviousPage={filters.pagina > 1}
              totalItems={listResponse.totalItens}
              totalPages={listResponse.totalPaginas}
              onPageChange={(pagina) => handleFilterChange({ pagina })}
            />
          </>
        ) : (
          <ProductEmptyState
            title="Nenhum produto encontrado"
            description="Ajuste os filtros ou cadastre um novo produto para comecar a popular esta tabela."
          />
        )}
      </div>

      <ProductCreateModal
        isOpen={isCreateModalOpen}
        storeId={selectedStoreId}
        storeName={selectedStore?.nome ?? null}
        onClose={() => setIsCreateModalOpen(false)}
        onProductCreated={(product) => {
          const matches = product.solicitacoesCompativeis ?? [];

          if (matches.length === 0) {
            return;
          }

          setRequestMatches(matches);
          setRecentlyCreatedProductDescription(product.descricao);
          setIsRequestMatchModalOpen(true);
        }}
      />
      <ProductEditModal
        isOpen={isEditModalOpen}
        product={selectedProduct}
        storeId={selectedStoreId}
        onClose={handleCloseEditModal}
      />
      <ProductDeleteModal
        productName={selectedProductName}
        isOpen={isDeleteModalOpen}
        isSubmitting={deleteProductMutation.isPending}
        onClose={handleCloseDeleteModal}
        onConfirm={handleDeleteConfirm}
      />
      <ProductSettingsModal
        isOpen={isSettingsModalOpen}
        settings={tableSettings}
        onClose={() => setIsSettingsModalOpen(false)}
        onSave={(nextSettings) => {
          const normalizedSettings = {
            ...defaultProductTableSettings,
            ...nextSettings,
          };

          startTransition(() => {
            setTableSettings(normalizedSettings);
            persistProductTableSettings(normalizedSettings);
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
      <PrintConfirmationModal
        description={`${getSelectedProducts({ showError: false }).length} produto(s) selecionado(s) para etiqueta.`}
        isOpen={isPrintConfirmationOpen}
        isPrinting={isPrintingLabels}
        onClose={() => setIsPrintConfirmationOpen(false)}
        onPreviewPdf={() => void handleOpenLabelsPdf()}
        onPrint={() => void handlePrintLabels()}
        settings={printSettings}
        target="label"
        title="Imprimir etiquetas"
      />
      <ProductRequestMatchModal
        isOpen={isRequestMatchModalOpen}
        matches={requestMatches}
        productDescription={recentlyCreatedProductDescription}
        onClose={() => {
          setIsRequestMatchModalOpen(false);
          setRequestMatches([]);
          setRecentlyCreatedProductDescription(null);
        }}
      />
    </section>
  );
}
