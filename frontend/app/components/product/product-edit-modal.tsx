"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useEffect, useMemo, useState } from "react";
import { toast } from "sonner";

import { SearchableSelect } from "@/app/components/ui/searchable-select";
import { Select } from "@/app/components/ui/select";
import { ThemedCheckbox } from "@/app/components/ui/themed-checkbox";
import {
  asProductResponse,
  extractProductFieldErrors,
  getProductApiMessage,
  normalizeDecimalValue,
  productSituacaoOptions,
  type ProductFieldErrors,
  type ProductFormValues,
  type ProductListItem,
  type ProductLookupOption,
} from "@/lib/product";
import { getAuthToken } from "@/lib/store";
import {
  getProductBrandOptions,
  getProductColorOptions,
  getProductReferenceOptions,
  getProductSizeOptions,
  getProductSupplierOptions,
  updateProduct,
} from "@/services/product-service";
import { mapProductZodErrors, productSchema } from "@/validations/product";

type ProductEditModalProps = {
  isOpen: boolean;
  onClose: () => void;
  product: ProductListItem | null;
  storeId: number | null;
};

type LookupSearchState = {
  produto: string;
  marca: string;
  tamanho: string;
  cor: string;
  fornecedor: string;
};

function getInitialProductFormValues(product: ProductListItem): ProductFormValues {
  return {
    descricao: product.descricao,
    preco: String(product.preco),
    quantidade: "1",
    entrada: toDateInputValue(product.entrada),
    situacao: String(product.situacao),
    consignado: product.consignado,
    produtoId: String(product.produtoId),
    produtoLabel: product.produto,
    marcaId: String(product.marcaId),
    marcaLabel: product.marca,
    tamanhoId: String(product.tamanhoId),
    tamanhoLabel: product.tamanho,
    corId: String(product.corId),
    corLabel: product.cor,
    fornecedorId: String(product.fornecedorId),
    fornecedorLabel: product.fornecedor,
  };
}

function getInitialLookupSearchState(product: ProductListItem): LookupSearchState {
  return {
    produto: product.produto,
    marca: product.marca,
    tamanho: product.tamanho,
    cor: product.cor,
    fornecedor: "",
  };
}

function toUtcStartOfDay(value: string) {
  return new Date(`${value}T00:00:00.000Z`).toISOString();
}

function toDateInputValue(value: string) {
  const parsed = new Date(value);

  if (Number.isNaN(parsed.getTime())) {
    return value.slice(0, 10);
  }

  return parsed.toISOString().slice(0, 10);
}

function FormField({
  label,
  placeholder,
  type = "text",
  value,
  error,
  inputMode,
  step,
  min,
  onChange,
}: {
  label: string;
  placeholder?: string;
  type?: "text" | "number" | "date";
  value: string;
  error?: string;
  inputMode?: React.HTMLAttributes<HTMLInputElement>["inputMode"];
  step?: string;
  min?: string;
  onChange: (value: string) => void;
}) {
  return (
    <label className="block space-y-2">
      <span className="text-sm font-semibold text-[var(--foreground)]">{label}</span>
      <input
        type={type}
        value={value}
        placeholder={placeholder}
        inputMode={inputMode}
        step={step}
        min={min}
        onChange={(event) => onChange(event.target.value)}
        className={`h-12 w-full rounded-2xl border bg-white px-4 text-sm text-[var(--foreground)] outline-none transition ${
          error
            ? "border-red-300 shadow-[0_0_0_4px_rgba(248,113,113,0.12)]"
            : "border-[var(--border)] focus:border-[var(--primary)] focus:shadow-[0_0_0_4px_rgba(106,92,255,0.12)]"
        }`}
      />
      {error ? <p className="text-sm text-red-500">{error}</p> : null}
    </label>
  );
}

function SelectField({
  label,
  value,
  error,
  options,
  onChange,
}: {
  label: string;
  value: string;
  error?: string;
  options: ReadonlyArray<{ value: number; label: string }>;
  onChange: (value: string) => void;
}) {
  return (
    <div className="space-y-2">
      <span className="text-sm font-semibold text-[var(--foreground)]">{label}</span>
      <div
        className={`rounded-2xl border bg-white px-4 py-3 text-sm text-[var(--foreground)] transition ${
          error
            ? "border-red-300 shadow-[0_0_0_4px_rgba(248,113,113,0.12)]"
            : "border-[var(--border)] focus:border-[var(--primary)] focus:shadow-[0_0_0_4px_rgba(106,92,255,0.12)]"
        }`}
      >
        <Select
          ariaLabel={label}
          value={value}
          options={options.map((option) => ({
            label: option.label,
            value: String(option.value),
          }))}
          placeholder="Selecionar"
          onChange={onChange}
        />
      </div>
      {error ? <p className="text-sm text-red-500">{error}</p> : null}
    </div>
  );
}

function SearchableField({
  label,
  error,
  disabled,
  loading,
  value,
  selectedLabel,
  searchValue,
  placeholder,
  searchPlaceholder,
  options,
  emptyLabel,
  onSearchChange,
  onSelect,
}: {
  label: string;
  error?: string;
  disabled?: boolean;
  loading: boolean;
  value: string;
  selectedLabel: string;
  searchValue: string;
  placeholder: string;
  searchPlaceholder: string;
  options: ProductLookupOption[];
  emptyLabel: string;
  onSearchChange: (value: string) => void;
  onSelect: (option: ProductLookupOption) => void;
}) {
  return (
    <div className="space-y-2">
      <span className="text-sm font-semibold text-[var(--foreground)]">{label}</span>
      <SearchableSelect
        ariaLabel={label}
        disabled={disabled}
        emptyLabel={emptyLabel}
        error={error}
        loading={loading}
        options={options.map((option) => ({
          label: option.label,
          value: String(option.id),
        }))}
        placeholder={placeholder}
        searchPlaceholder={searchPlaceholder}
        searchValue={searchValue}
        selectedLabel={selectedLabel}
        value={value || null}
        onSearchChange={onSearchChange}
        onChange={(option) =>
          onSelect({
            id: Number(option.value),
            label: option.label,
          })
        }
      />
      {error ? <p className="text-sm text-red-500">{error}</p> : null}
    </div>
  );
}

export function ProductEditModal({
  isOpen,
  onClose,
  product,
  storeId,
}: ProductEditModalProps) {
  if (!product) {
    return null;
  }

  return (
    <ProductEditModalContent
      key={product.id}
      isOpen={isOpen}
      onClose={onClose}
      product={product}
      storeId={storeId}
    />
  );
}

type ProductEditModalContentProps = {
  isOpen: boolean;
  onClose: () => void;
  product: ProductListItem;
  storeId: number | null;
};

function ProductEditModalContent({
  isOpen,
  onClose,
  product,
  storeId,
}: ProductEditModalContentProps) {
  const queryClient = useQueryClient();
  const token = useMemo(() => (typeof window === "undefined" ? null : getAuthToken()), []);
  const [shouldRender, setShouldRender] = useState(isOpen);
  const [isVisible, setIsVisible] = useState(false);
  const [values, setValues] = useState<ProductFormValues>(() => getInitialProductFormValues(product));
  const [errors, setErrors] = useState<ProductFieldErrors>({});
  const [lookupSearch, setLookupSearch] = useState<LookupSearchState>(() =>
    getInitialLookupSearchState(product),
  );
  const [debouncedLookupSearch, setDebouncedLookupSearch] = useState<LookupSearchState>(() =>
    getInitialLookupSearchState(product),
  );
  const trimmedSupplierSearch = debouncedLookupSearch.fornecedor.trim();

  useEffect(() => {
    const timeoutId = window.setTimeout(() => {
      setDebouncedLookupSearch(lookupSearch);
    }, 300);

    return () => {
      window.clearTimeout(timeoutId);
    };
  }, [lookupSearch]);

  const productOptionsQuery = useQuery({
    queryKey: ["product-edit-options", "produto", token, storeId, debouncedLookupSearch.produto],
    queryFn: async () => {
      if (!token || !storeId) {
        return [];
      }

      const response = await getProductReferenceOptions(token, storeId, debouncedLookupSearch.produto);
      if (!response.ok) {
        throw new Error("Nao foi possivel carregar os produtos auxiliares.");
      }

      return response.body;
    },
    enabled: Boolean(token && storeId),
  });

  const brandOptionsQuery = useQuery({
    queryKey: ["product-edit-options", "marca", token, storeId, debouncedLookupSearch.marca],
    queryFn: async () => {
      if (!token || !storeId) {
        return [];
      }

      const response = await getProductBrandOptions(token, storeId, debouncedLookupSearch.marca);
      if (!response.ok) {
        throw new Error("Nao foi possivel carregar as marcas.");
      }

      return response.body;
    },
    enabled: Boolean(token && storeId),
  });

  const sizeOptionsQuery = useQuery({
    queryKey: ["product-edit-options", "tamanho", token, storeId, debouncedLookupSearch.tamanho],
    queryFn: async () => {
      if (!token || !storeId) {
        return [];
      }

      const response = await getProductSizeOptions(token, storeId, debouncedLookupSearch.tamanho);
      if (!response.ok) {
        throw new Error("Nao foi possivel carregar os tamanhos.");
      }

      return response.body;
    },
    enabled: Boolean(token && storeId),
  });

  const colorOptionsQuery = useQuery({
    queryKey: ["product-edit-options", "cor", token, storeId, debouncedLookupSearch.cor],
    queryFn: async () => {
      if (!token || !storeId) {
        return [];
      }

      const response = await getProductColorOptions(token, storeId, debouncedLookupSearch.cor);
      if (!response.ok) {
        throw new Error("Nao foi possivel carregar as cores.");
      }

      return response.body;
    },
    enabled: Boolean(token && storeId),
  });

  const supplierOptionsQuery = useQuery({
    queryKey: ["product-edit-options", "fornecedor", token, storeId, trimmedSupplierSearch],
    queryFn: async () => {
      if (!token || !storeId) {
        return [];
      }

      const response = await getProductSupplierOptions(token, storeId, trimmedSupplierSearch);
      if (!response.ok) {
        throw new Error("Nao foi possivel carregar os fornecedores.");
      }

      return response.body;
    },
    enabled: Boolean(token && storeId && trimmedSupplierSearch),
  });

  const updateProductMutation = useMutation({
    mutationFn: async (payload: {
      productId: number;
      preco: number;
      produtoId: number;
      marcaId: number;
      tamanhoId: number;
      corId: number;
      fornecedorId: number;
      descricao: string;
      entrada: string;
      situacao: number;
      consignado: boolean;
    }) => {
      if (!token) {
        throw new Error("Voce precisa estar autenticado para editar um produto.");
      }

      return updateProduct(
        payload.productId,
        {
          preco: payload.preco,
          produtoId: payload.produtoId,
          marcaId: payload.marcaId,
          tamanhoId: payload.tamanhoId,
          corId: payload.corId,
          fornecedorId: payload.fornecedorId,
          descricao: payload.descricao,
          entrada: payload.entrada,
          situacao: payload.situacao,
          consignado: payload.consignado,
        },
        token,
      );
    },
  });

  useEffect(() => {
    let animationFrame = 0;
    let visibilityFrame = 0;
    let closeTimeout = 0;

    if (isOpen) {
      animationFrame = window.requestAnimationFrame(() => {
        setShouldRender(true);
        visibilityFrame = window.requestAnimationFrame(() => {
          setIsVisible(true);
        });
      });
    } else if (shouldRender) {
      animationFrame = window.requestAnimationFrame(() => {
        setIsVisible(false);
      });

      closeTimeout = window.setTimeout(() => {
        setShouldRender(false);
      }, 220);
    }

    function handleEscape(event: KeyboardEvent) {
      if (event.key === "Escape" && !updateProductMutation.isPending) {
        onClose();
      }
    }

    window.addEventListener("keydown", handleEscape);

    return () => {
      window.cancelAnimationFrame(animationFrame);
      window.cancelAnimationFrame(visibilityFrame);
      window.clearTimeout(closeTimeout);
      window.removeEventListener("keydown", handleEscape);
    };
  }, [isOpen, onClose, shouldRender, updateProductMutation.isPending]);

  function updateField<K extends keyof ProductFormValues>(field: K, value: ProductFormValues[K]) {
    setValues((current) => ({
      ...current,
      [field]: value,
    }));
  }

  function updateLookupSearch(field: keyof LookupSearchState, value: string) {
    setLookupSearch((current) => ({
      ...current,
      [field]: value,
    }));
  }

  function updateRelation(
    field: "produto" | "marca" | "tamanho" | "cor" | "fornecedor",
    option: ProductLookupOption,
  ) {
    const idField = `${field}Id` as const;
    const labelField = `${field}Label` as const;

    setValues((current) => ({
      ...current,
      [idField]: String(option.id),
      [labelField]: option.label,
    }));
    setErrors((current) => ({
      ...current,
      [idField]: undefined,
    }));
  }

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();

    const validation = productSchema.safeParse(values);

    if (!validation.success) {
      setErrors(mapProductZodErrors(validation.error));
      return;
    }

    setErrors({});

    try {
      const response = await updateProductMutation.mutateAsync({
        productId: product.id,
        preco: Number(normalizeDecimalValue(validation.data.preco)),
        produtoId: Number(validation.data.produtoId),
        marcaId: Number(validation.data.marcaId),
        tamanhoId: Number(validation.data.tamanhoId),
        corId: Number(validation.data.corId),
        fornecedorId: Number(validation.data.fornecedorId),
        descricao: validation.data.descricao.trim(),
        entrada: toUtcStartOfDay(validation.data.entrada),
        situacao: Number(validation.data.situacao),
        consignado: values.consignado,
      });

      if (!response.ok) {
        const apiFieldErrors = extractProductFieldErrors(response.body);

        if (Object.keys(apiFieldErrors).length > 0) {
          setErrors(apiFieldErrors);
        }

        toast.error(getProductApiMessage(response.body) ?? "Nao foi possivel editar o produto.");
        return;
      }

      const updatedProduct = asProductResponse(response.body);
      await queryClient.invalidateQueries({ queryKey: ["products"] });
      toast.success(`Produto ${updatedProduct.descricao} atualizado com sucesso.`);
      onClose();
    } catch (error) {
      toast.error(
        error instanceof Error
          ? error.message
          : "Nao foi possivel conectar ao backend. Verifique se a API esta em execucao.",
      );
    }
  }

  if (!shouldRender) {
    return null;
  }

  return (
    <div
      className={`fixed inset-0 z-50 flex items-start justify-center overflow-y-auto bg-[rgba(15,23,42,0.45)] px-4 pt-[calc(env(safe-area-inset-top,0px)+1rem)] pb-[calc(env(safe-area-inset-bottom,0px)+1.5rem)] transition-opacity duration-200 ease-out sm:items-center sm:px-4 sm:pt-4 sm:pb-4 ${
        isVisible ? "opacity-100" : "opacity-0"
      }`}
    >
      <div
        className={`max-h-[calc(100dvh-env(safe-area-inset-top,0px)-env(safe-area-inset-bottom,0px)-2.5rem)] w-full max-w-4xl overflow-y-auto rounded-[28px] border border-[var(--border)] bg-white p-6 shadow-[0_30px_90px_rgba(15,23,42,0.22)] transition duration-250 ease-out sm:max-h-[calc(100vh-2rem)] ${
          isVisible ? "translate-y-0 scale-100 opacity-100" : "translate-y-4 scale-[0.98] opacity-0"
        }`}
      >
        <div className="flex items-start justify-between gap-4">
          <div>
            <p className="text-sm font-semibold uppercase tracking-[0.16em] text-[var(--muted)]">
              Editar produto
            </p>
            <h2 className="mt-2 text-2xl font-semibold tracking-tight text-[var(--foreground)]">
              Atualize os dados do item
            </h2>
          </div>

          <button
            type="button"
            onClick={onClose}
            className="flex h-11 w-11 cursor-pointer items-center justify-center rounded-2xl border border-[var(--border)] bg-[var(--surface-muted)] text-[var(--muted)] transition hover:border-[var(--border-strong)] hover:text-[var(--foreground)]"
            aria-label="Fechar modal"
          >
            x
          </button>
        </div>

        <form className="mt-6 space-y-5" onSubmit={handleSubmit} noValidate>
          <div className="grid gap-4 md:grid-cols-2">
            <SearchableField
              label="Produto"
              error={errors.produtoId}
              disabled={!storeId}
              loading={productOptionsQuery.isLoading}
              value={values.produtoId}
              selectedLabel={values.produtoLabel}
              searchValue={lookupSearch.produto}
              placeholder="Selecione o produto"
              searchPlaceholder="Pesquisar por valor"
              options={productOptionsQuery.data ?? []}
              emptyLabel="Nenhum produto auxiliar encontrado."
              onSearchChange={(value) => updateLookupSearch("produto", value)}
              onSelect={(option) => updateRelation("produto", option)}
            />
            <FormField
              label="Descricao"
              value={values.descricao}
              error={errors.descricao}
              onChange={(value) => {
                updateField("descricao", value);
                setErrors((current) => ({ ...current, descricao: undefined }));
              }}
            />
            <SearchableField
              label="Marca"
              error={errors.marcaId}
              disabled={!storeId}
              loading={brandOptionsQuery.isLoading}
              value={values.marcaId}
              selectedLabel={values.marcaLabel}
              searchValue={lookupSearch.marca}
              placeholder="Selecione a marca"
              searchPlaceholder="Pesquisar por valor"
              options={brandOptionsQuery.data ?? []}
              emptyLabel="Nenhuma marca encontrada."
              onSearchChange={(value) => updateLookupSearch("marca", value)}
              onSelect={(option) => updateRelation("marca", option)}
            />
            <SearchableField
              label="Fornecedor"
              error={errors.fornecedorId}
              disabled={!storeId}
              loading={Boolean(trimmedSupplierSearch) && supplierOptionsQuery.isLoading}
              value={values.fornecedorId}
              selectedLabel={values.fornecedorLabel}
              searchValue={lookupSearch.fornecedor}
              placeholder="Selecione o fornecedor"
              searchPlaceholder="Pesquisar por nome"
              options={trimmedSupplierSearch ? (supplierOptionsQuery.data ?? []) : []}
              emptyLabel={
                !trimmedSupplierSearch
                  ? "Digite para buscar clientes."
                  : "Nenhum fornecedor encontrado."
              }
              onSearchChange={(value) => updateLookupSearch("fornecedor", value)}
              onSelect={(option) => {
                updateRelation("fornecedor", option);
                updateLookupSearch("fornecedor", "");
              }}
            />
            <SearchableField
              label="Tamanho"
              error={errors.tamanhoId}
              disabled={!storeId}
              loading={sizeOptionsQuery.isLoading}
              value={values.tamanhoId}
              selectedLabel={values.tamanhoLabel}
              searchValue={lookupSearch.tamanho}
              placeholder="Selecione o tamanho"
              searchPlaceholder="Pesquisar por valor"
              options={sizeOptionsQuery.data ?? []}
              emptyLabel="Nenhum tamanho encontrado."
              onSearchChange={(value) => updateLookupSearch("tamanho", value)}
              onSelect={(option) => updateRelation("tamanho", option)}
            />
            <SearchableField
              label="Cor"
              error={errors.corId}
              disabled={!storeId}
              loading={colorOptionsQuery.isLoading}
              value={values.corId}
              selectedLabel={values.corLabel}
              searchValue={lookupSearch.cor}
              placeholder="Selecione a cor"
              searchPlaceholder="Pesquisar por valor"
              options={colorOptionsQuery.data ?? []}
              emptyLabel="Nenhuma cor encontrada."
              onSearchChange={(value) => updateLookupSearch("cor", value)}
              onSelect={(option) => updateRelation("cor", option)}
            />
            <FormField
              label="Preco"
              type="number"
              inputMode="decimal"
              step="0.01"
              min="0"
              value={values.preco}
              error={errors.preco}
              onChange={(value) => {
                updateField("preco", value);
                setErrors((current) => ({ ...current, preco: undefined }));
              }}
            />
            <FormField
              label="Data de entrada"
              type="date"
              value={values.entrada}
              error={errors.entrada}
              onChange={(value) => {
                updateField("entrada", value);
                setErrors((current) => ({ ...current, entrada: undefined }));
              }}
            />
            <SelectField
              label="Situacao"
              value={values.situacao}
              error={errors.situacao}
              options={productSituacaoOptions}
              onChange={(value) => {
                updateField("situacao", value);
                setErrors((current) => ({ ...current, situacao: undefined }));
              }}
            />
          </div>

          <ThemedCheckbox
            checked={values.consignado}
            disabled={!storeId}
            label="Produto consignado"
            onChange={(checked) => updateField("consignado", checked)}
          />

          <div className="flex flex-col gap-3 sm:flex-row sm:justify-end">
            <button
              type="button"
              onClick={onClose}
              className="flex h-12 cursor-pointer items-center justify-center rounded-2xl border border-[var(--border)] bg-[var(--surface-muted)] px-5 text-sm font-semibold text-[var(--foreground)] transition hover:border-[var(--border-strong)] hover:bg-white"
            >
              Cancelar
            </button>
            <button
              type="submit"
              disabled={updateProductMutation.isPending || !storeId}
              className="flex h-12 cursor-pointer items-center justify-center rounded-2xl bg-[linear-gradient(90deg,_#22c55e,_#16a34a)] px-5 text-sm font-semibold text-white shadow-[0_16px_30px_rgba(34,197,94,0.25)] transition hover:brightness-105 disabled:cursor-not-allowed disabled:opacity-60"
            >
              {updateProductMutation.isPending ? "Salvando alteracoes..." : "Salvar alteracoes"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
