"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { startTransition, useEffect, useMemo, useState } from "react";
import { toast } from "sonner";

import { ClientCreateModal } from "@/app/components/client/client-create-modal";
import { ProductAuxiliaryCreateModal } from "@/app/components/product/product-auxiliary-create-modal";
import { ProductAuxiliaryDeleteModal } from "@/app/components/product/product-auxiliary-delete-modal";
import { SearchableSelect } from "@/app/components/ui/searchable-select";
import { ThemedCheckbox } from "@/app/components/ui/themed-checkbox";
import {
  asClientResponse,
  extractClientFieldErrors,
  formatPhoneValue,
  getClientApiMessage,
  initialClientFormValues,
  normalizeNumericValue,
  type ClientFieldErrors,
  type ClientFormValues,
} from "@/lib/client";
import {
  asProductResponse,
  extractProductFieldErrors,
  getProductApiMessage,
  initialProductFormValues,
  normalizeDecimalValue,
  type ProductCreateResponse,
  type ProductFieldErrors,
  type ProductFormValues,
  type ProductLookupOption,
} from "@/lib/product";
import { getAuthToken } from "@/lib/store";
import { createClient } from "@/services/client-service";
import {
  createProductBrand,
  createProductColor,
  createProduct,
  createProductReference,
  createProductSize,
  deleteProductBrand,
  deleteProductColor,
  deleteProductReference,
  deleteProductSize,
  getProductBrandOptions,
  getProductColorOptions,
  getProductReferenceOptions,
  getProductSizeOptions,
  getProductSupplierOptions,
} from "@/services/product-service";
import { clientSchema, mapClientZodErrors } from "@/validations/client";
import { mapProductZodErrors, productSchema } from "@/validations/product";

type ProductCreateModalProps = {
  isOpen: boolean;
  onClose: () => void;
  storeId: number | null;
  storeName: string | null;
  onProductCreated?: (product: ProductCreateResponse) => void;
};

type LookupSearchState = {
  produto: string;
  marca: string;
  tamanho: string;
  cor: string;
  fornecedor: string;
};

type AuxiliaryField = "produto" | "marca" | "tamanho" | "cor";

type AuxiliaryModalState = {
  error?: string;
  field: AuxiliaryField;
  isOpen: boolean;
  value: string;
};

type AuxiliaryLookupOption = ProductLookupOption & {
  onSecondaryAction?: () => void;
  secondaryActionAriaLabel?: string;
};

type AuxiliaryDeleteModalState = {
  field: AuxiliaryField;
  id: number | null;
  isOpen: boolean;
  value: string;
};

function toUtcStartOfDay(value: string) {
  return new Date(`${value}T00:00:00.000Z`).toISOString();
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
  actionLabel,
  onSearchChange,
  onAction,
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
  options: AuxiliaryLookupOption[];
  emptyLabel: string;
  actionLabel?: string;
  onSearchChange: (value: string) => void;
  onAction?: () => void;
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
          onSecondaryAction: option.onSecondaryAction,
          secondaryActionAriaLabel: option.secondaryActionAriaLabel,
          value: String(option.id),
        }))}
        placeholder={placeholder}
        searchPlaceholder={searchPlaceholder}
        searchValue={searchValue}
        selectedLabel={selectedLabel}
        value={value || null}
        actionLabel={actionLabel}
        onSearchChange={onSearchChange}
        onAction={onAction}
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

export function ProductCreateModal({
  isOpen,
  onClose,
  storeId,
  storeName,
  onProductCreated,
}: ProductCreateModalProps) {
  const queryClient = useQueryClient();
  const [shouldRender, setShouldRender] = useState(isOpen);
  const [isVisible, setIsVisible] = useState(false);
  const [values, setValues] = useState<ProductFormValues>(initialProductFormValues);
  const [errors, setErrors] = useState<ProductFieldErrors>({});
  const [lookupSearch, setLookupSearch] = useState<LookupSearchState>({
    produto: "",
    marca: "",
    tamanho: "",
    cor: "",
    fornecedor: "",
  });
  const [debouncedLookupSearch, setDebouncedLookupSearch] = useState<LookupSearchState>({
    produto: "",
    marca: "",
    tamanho: "",
    cor: "",
    fornecedor: "",
  });
  const [auxiliaryModal, setAuxiliaryModal] = useState<AuxiliaryModalState>({
    field: "produto",
    isOpen: false,
    value: "",
  });
  const [auxiliaryDeleteModal, setAuxiliaryDeleteModal] = useState<AuxiliaryDeleteModalState>({
    field: "produto",
    id: null,
    isOpen: false,
    value: "",
  });
  const [supplierModalOpen, setSupplierModalOpen] = useState(false);
  const [supplierFormValues, setSupplierFormValues] =
    useState<ClientFormValues>(initialClientFormValues);
  const [supplierFormErrors, setSupplierFormErrors] = useState<ClientFieldErrors>({});
  const token = useMemo(() => (typeof window === "undefined" ? null : getAuthToken()), []);

  const createProductMutation = useMutation({
    mutationFn: async (payload: {
      preco: number;
      quantidade: number;
      produtoId: number;
      marcaId: number;
      tamanhoId: number;
      corId: number;
      fornecedorId: number;
      descricao: string;
      entrada: string;
      lojaId: number;
      situacao: number;
      consignado: boolean;
    }) => {
      if (!token) {
        throw new Error("Voce precisa estar autenticado para cadastrar um produto.");
      }

      return createProduct(payload, token);
    },
  });

  const createAuxiliaryMutation = useMutation({
    mutationFn: async (payload: { field: AuxiliaryField; value: string; storeId: number }) => {
      if (!token) {
        throw new Error("Voce precisa estar autenticado para cadastrar um auxiliar.");
      }

      const requestPayload = {
        valor: payload.value,
        lojaId: payload.storeId,
      };

      if (payload.field === "produto") {
        return createProductReference(requestPayload, token);
      }

      if (payload.field === "marca") {
        return createProductBrand(requestPayload, token);
      }

      if (payload.field === "tamanho") {
        return createProductSize(requestPayload, token);
      }

      return createProductColor(requestPayload, token);
    },
  });

  const createSupplierMutation = useMutation({
    mutationFn: async (payload: {
      nome: string;
      contato: string;
      doacao: boolean;
      lojaId: number;
      userId?: number;
    }) => {
      if (!token) {
        throw new Error("Voce precisa estar autenticado para cadastrar um fornecedor.");
      }

      return createClient(payload, token);
    },
  });

  const deleteAuxiliaryMutation = useMutation({
    mutationFn: async (payload: { field: AuxiliaryField; id: number }) => {
      if (!token) {
        throw new Error("Voce precisa estar autenticado para excluir um auxiliar.");
      }

      if (payload.field === "produto") {
        return deleteProductReference(payload.id, token);
      }

      if (payload.field === "marca") {
        return deleteProductBrand(payload.id, token);
      }

      if (payload.field === "tamanho") {
        return deleteProductSize(payload.id, token);
      }

      return deleteProductColor(payload.id, token);
    },
  });

  function resetForm() {
    setValues(initialProductFormValues);
    setErrors({});
    setLookupSearch({
      produto: "",
      marca: "",
      tamanho: "",
      cor: "",
      fornecedor: "",
    });
    setDebouncedLookupSearch({
      produto: "",
      marca: "",
      tamanho: "",
      cor: "",
      fornecedor: "",
    });
    setAuxiliaryModal({
      field: "produto",
      isOpen: false,
      value: "",
    });
    setAuxiliaryDeleteModal({
      field: "produto",
      id: null,
      isOpen: false,
      value: "",
    });
    setSupplierModalOpen(false);
    setSupplierFormValues(initialClientFormValues);
    setSupplierFormErrors({});
  }

  function handleClose() {
    if (createProductMutation.isPending) {
      return;
    }

    resetForm();
    onClose();
  }

  useEffect(() => {
    const timeoutId = window.setTimeout(() => {
      setDebouncedLookupSearch(lookupSearch);
    }, 300);

    return () => {
      window.clearTimeout(timeoutId);
    };
  }, [lookupSearch]);

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
      if (
        event.key === "Escape" &&
        !createProductMutation.isPending &&
        !auxiliaryModal.isOpen &&
        !auxiliaryDeleteModal.isOpen &&
        !supplierModalOpen
      ) {
        resetForm();
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
  }, [
    auxiliaryModal.isOpen,
    auxiliaryDeleteModal.isOpen,
    isOpen,
    onClose,
    shouldRender,
    createProductMutation.isPending,
    supplierModalOpen,
  ]);

  const productOptionsQuery = useQuery({
    queryKey: ["product-create-options", "produto", token, storeId, debouncedLookupSearch.produto],
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
    enabled: Boolean(isOpen && token && storeId),
  });

  const brandOptionsQuery = useQuery({
    queryKey: ["product-create-options", "marca", token, storeId, debouncedLookupSearch.marca],
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
    enabled: Boolean(isOpen && token && storeId),
  });

  const sizeOptionsQuery = useQuery({
    queryKey: ["product-create-options", "tamanho", token, storeId, debouncedLookupSearch.tamanho],
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
    enabled: Boolean(isOpen && token && storeId),
  });

  const colorOptionsQuery = useQuery({
    queryKey: ["product-create-options", "cor", token, storeId, debouncedLookupSearch.cor],
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
    enabled: Boolean(isOpen && token && storeId),
  });

  const supplierOptionsQuery = useQuery({
    queryKey: [
      "product-create-options",
      "fornecedor",
      token,
      storeId,
      debouncedLookupSearch.fornecedor,
    ],
    queryFn: async () => {
      if (!token || !storeId) {
        return [];
      }

      const response = await getProductSupplierOptions(
        token,
        storeId,
        debouncedLookupSearch.fornecedor,
      );

      if (!response.ok) {
        throw new Error("Nao foi possivel carregar os fornecedores.");
      }

      return response.body;
    },
    enabled: Boolean(isOpen && token && storeId),
  });

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
    field:
      | "produto"
      | "marca"
      | "tamanho"
      | "cor"
      | "fornecedor",
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

  function getAuxiliaryLabel(field: AuxiliaryField) {
    if (field === "produto") {
      return "Produto";
    }

    if (field === "marca") {
      return "Marca";
    }

    if (field === "tamanho") {
      return "Tamanho";
    }

    return "Cor";
  }

  function openAuxiliaryModal(field: AuxiliaryField) {
    setAuxiliaryModal({
      field,
      isOpen: true,
      value: "",
    });
  }

  function closeAuxiliaryModal() {
    if (createAuxiliaryMutation.isPending) {
      return;
    }

    setAuxiliaryModal((current) => ({
      ...current,
      error: undefined,
      isOpen: false,
      value: "",
    }));
  }

  function openAuxiliaryDeleteModal(field: AuxiliaryField, option: ProductLookupOption) {
    setAuxiliaryDeleteModal({
      field,
      id: option.id,
      isOpen: true,
      value: option.label,
    });
  }

  function closeAuxiliaryDeleteModal() {
    if (deleteAuxiliaryMutation.isPending) {
      return;
    }

    setAuxiliaryDeleteModal((current) => ({
      ...current,
      id: null,
      isOpen: false,
      value: "",
    }));
  }

  function closeSupplierModal() {
    if (createSupplierMutation.isPending) {
      return;
    }

    setSupplierModalOpen(false);
    setSupplierFormValues(initialClientFormValues);
    setSupplierFormErrors({});
  }

  function updateSupplierField<K extends keyof ClientFormValues>(field: K, value: ClientFormValues[K]) {
    const normalizedValue = field === "contato" ? formatPhoneValue(String(value)) : value;

    setSupplierFormValues((current) => ({
      ...current,
      [field]: normalizedValue,
    }));
    setSupplierFormErrors((current) => ({
      ...current,
      [field]: undefined,
    }));
  }

  async function handleAuxiliarySubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (!storeId) {
      toast.error("Selecione uma loja antes de criar opcoes.");
      return;
    }

    const value = auxiliaryModal.value.trim();

    if (!value) {
      setAuxiliaryModal((current) => ({
        ...current,
        error: "Informe um valor.",
      }));
      return;
    }

    setAuxiliaryModal((current) => ({
      ...current,
      error: undefined,
    }));

    try {
      const response = await createAuxiliaryMutation.mutateAsync({
        field: auxiliaryModal.field,
        value,
        storeId,
      });

      if (!response.ok) {
        const message =
          getProductApiMessage(response.body) ??
          `Nao foi possivel cadastrar ${getAuxiliaryLabel(auxiliaryModal.field).toLowerCase()}.`;

        setAuxiliaryModal((current) => ({
          ...current,
          error: message,
        }));
        toast.error(message);
        return;
      }

      const created = response.body as { id: number; valor: string };
      const field = auxiliaryModal.field;

      updateRelation(field, {
        id: created.id,
        label: created.valor,
      });
      updateLookupSearch(field, created.valor);
      setDebouncedLookupSearch((current) => ({
        ...current,
        [field]: created.valor,
      }));
      setAuxiliaryModal({
        field,
        isOpen: false,
        value: "",
      });

      await queryClient.invalidateQueries({ queryKey: ["product-create-options"] });
      toast.success(`${getAuxiliaryLabel(field)} criado com sucesso.`);
    } catch (error) {
      toast.error(
        error instanceof Error
          ? error.message
          : "Nao foi possivel conectar ao backend. Verifique se a API esta em execucao.",
      );
    }
  }

  async function handleSupplierSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (!storeId) {
      toast.error("Selecione uma loja antes de cadastrar fornecedores.");
      return;
    }

    const validation = clientSchema.safeParse(supplierFormValues);

    if (!validation.success) {
      setSupplierFormErrors(mapClientZodErrors(validation.error));
      return;
    }

    setSupplierFormErrors({});

    try {
      const response = await createSupplierMutation.mutateAsync({
        nome: validation.data.nome.trim(),
        contato: normalizeNumericValue(validation.data.contato),
        doacao: validation.data.doacao,
        lojaId: storeId,
        ...(validation.data.userId ? { userId: Number(validation.data.userId) } : {}),
      });

      if (!response.ok) {
        const apiErrors = extractClientFieldErrors(response.body);

        if (Object.keys(apiErrors).length > 0) {
          setSupplierFormErrors(apiErrors);
        }

        toast.error(
          getClientApiMessage(response.body) ?? "Nao foi possivel cadastrar o fornecedor.",
        );
        return;
      }

      const createdSupplier = asClientResponse(response.body);

      updateRelation("fornecedor", {
        id: createdSupplier.id,
        label: createdSupplier.nome,
      });
      updateLookupSearch("fornecedor", createdSupplier.nome);
      setDebouncedLookupSearch((current) => ({
        ...current,
        fornecedor: createdSupplier.nome,
      }));
      setSupplierModalOpen(false);
      setSupplierFormValues(initialClientFormValues);
      setSupplierFormErrors({});

      await queryClient.invalidateQueries({ queryKey: ["product-create-options"] });
      await queryClient.invalidateQueries({ queryKey: ["clients"] });
      toast.success(`Fornecedor ${createdSupplier.nome} cadastrado com sucesso.`);
    } catch (error) {
      toast.error(
        error instanceof Error
          ? error.message
          : "Nao foi possivel conectar ao backend. Verifique se a API esta em execucao.",
      );
    }
  }

  async function handleAuxiliaryDeleteConfirm() {
    if (!auxiliaryDeleteModal.id) {
      toast.error("Selecione um item auxiliar valido para excluir.");
      return;
    }

    try {
      const response = await deleteAuxiliaryMutation.mutateAsync({
        field: auxiliaryDeleteModal.field,
        id: auxiliaryDeleteModal.id,
      });

      if (!response.ok) {
        toast.error(
          getProductApiMessage(response.body) ??
            `Nao foi possivel excluir ${getAuxiliaryLabel(auxiliaryDeleteModal.field).toLowerCase()}.`,
        );
        return;
      }

      const field = auxiliaryDeleteModal.field;
      const deletedId = String(auxiliaryDeleteModal.id);

      const idField = `${field}Id` as const;
      const labelField = `${field}Label` as const;

      setValues((current) =>
        current[idField] === deletedId
          ? {
              ...current,
              [idField]: "",
              [labelField]: "",
            }
          : current,
      );

      if (lookupSearch[field] === auxiliaryDeleteModal.value) {
        updateLookupSearch(field, "");
        setDebouncedLookupSearch((current) => ({
          ...current,
          [field]: "",
        }));
      }

      closeAuxiliaryDeleteModal();
      await queryClient.invalidateQueries({ queryKey: ["product-create-options"] });
      toast.success(`${getAuxiliaryLabel(field)} excluido com sucesso.`);
    } catch (error) {
      toast.error(
        error instanceof Error
          ? error.message
          : "Nao foi possivel conectar ao backend. Verifique se a API esta em execucao.",
      );
    }
  }

  function withDeleteAction(field: AuxiliaryField, options: ProductLookupOption[]): AuxiliaryLookupOption[] {
    return options.map((option) => ({
      ...option,
      onSecondaryAction: () => openAuxiliaryDeleteModal(field, option),
      secondaryActionAriaLabel: `Excluir ${getAuxiliaryLabel(field).toLowerCase()} ${option.label}`,
    }));
  }

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (!storeId) {
      toast.error("Selecione uma loja antes de cadastrar produtos.");
      return;
    }

    const validation = productSchema.safeParse(values);

    if (!validation.success) {
      setErrors(mapProductZodErrors(validation.error));
      return;
    }

    setErrors({});

    try {
      const payload = {
        preco: Number(normalizeDecimalValue(validation.data.preco)),
        quantidade: Number(validation.data.quantidade),
        produtoId: Number(validation.data.produtoId),
        marcaId: Number(validation.data.marcaId),
        tamanhoId: Number(validation.data.tamanhoId),
        corId: Number(validation.data.corId),
        fornecedorId: Number(validation.data.fornecedorId),
        descricao: validation.data.descricao.trim(),
        entrada: toUtcStartOfDay(validation.data.entrada),
        lojaId: storeId,
        situacao: Number(validation.data.situacao),
        consignado: values.consignado,
      };

      const response = await createProductMutation.mutateAsync(payload);

      if (!response.ok) {
        const apiFieldErrors = extractProductFieldErrors(response.body);

        if (Object.keys(apiFieldErrors).length > 0) {
          setErrors(apiFieldErrors);
        }

        toast.error(getProductApiMessage(response.body) ?? "Nao foi possivel cadastrar o produto.");
        return;
      }

      const createdProduct = asProductResponse(response.body);

      startTransition(() => {
        resetForm();
        onClose();
      });

      await queryClient.invalidateQueries({ queryKey: ["products"] });
      toast.success(
        payload.quantidade > 1
          ? `${payload.quantidade} produtos ${createdProduct.descricao} cadastrados com sucesso.`
          : `Produto ${createdProduct.descricao} cadastrado com sucesso.`,
      );
      onProductCreated?.(createdProduct);
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
      className={`fixed inset-0 z-50 flex items-center justify-center bg-[rgba(15,23,42,0.45)] p-4 transition-opacity duration-200 ease-out ${
        isVisible ? "opacity-100" : "opacity-0"
      }`}
    >
      <div
        className={`max-h-[92vh] w-full max-w-4xl overflow-y-auto rounded-[28px] border border-[var(--border)] bg-white p-6 shadow-[0_30px_90px_rgba(15,23,42,0.22)] transition duration-250 ease-out ${
          isVisible ? "translate-y-0 scale-100 opacity-100" : "translate-y-4 scale-[0.98] opacity-0"
        }`}
      >
        <div className="flex items-start justify-between gap-4">
          <div>
            <p className="text-sm font-semibold uppercase tracking-[0.16em] text-[var(--muted)]">
              Novo produto
            </p>
            <h2 className="mt-2 text-2xl font-semibold tracking-tight text-[var(--foreground)]">
              Cadastro rapido na loja ativa
            </h2>
            <p className="mt-2 text-sm leading-7 text-[var(--muted)]">
              {storeName
                ? `Os novos produtos serao vinculados a ${storeName}.`
                : "Selecione uma loja no topo antes de continuar."}
            </p>
          </div>

          <button
            type="button"
            onClick={handleClose}
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
              options={withDeleteAction("produto", productOptionsQuery.data ?? [])}
              emptyLabel={
                productOptionsQuery.isError
                  ? "Falha ao carregar produtos."
                  : "Nenhum produto auxiliar encontrado."
              }
              actionLabel="Criar novo produto"
              onSearchChange={(value) => updateLookupSearch("produto", value)}
              onAction={() => openAuxiliaryModal("produto")}
              onSelect={(option) => updateRelation("produto", option)}
            />
            <FormField
              label="Descricao"
              placeholder="Ex.: Vestido curto estampado"
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
              options={withDeleteAction("marca", brandOptionsQuery.data ?? [])}
              emptyLabel={
                brandOptionsQuery.isError
                  ? "Falha ao carregar marcas."
                  : "Nenhuma marca encontrada."
              }
              actionLabel="Criar nova marca"
              onSearchChange={(value) => updateLookupSearch("marca", value)}
              onAction={() => openAuxiliaryModal("marca")}
              onSelect={(option) => updateRelation("marca", option)}
            />
            <SearchableField
              label="Fornecedor"
              error={errors.fornecedorId}
              disabled={!storeId}
              loading={supplierOptionsQuery.isLoading}
              value={values.fornecedorId}
              selectedLabel={values.fornecedorLabel}
              searchValue={lookupSearch.fornecedor}
              placeholder="Selecione o fornecedor"
              searchPlaceholder="Pesquisar por nome"
              options={supplierOptionsQuery.data ?? []}
              emptyLabel={
                supplierOptionsQuery.isError
                  ? "Falha ao carregar fornecedores."
                  : "Nenhum fornecedor encontrado."
              }
              actionLabel="Criar novo fornecedor"
              onSearchChange={(value) => updateLookupSearch("fornecedor", value)}
              onAction={() => {
                setSupplierFormValues(initialClientFormValues);
                setSupplierFormErrors({});
                setSupplierModalOpen(true);
              }}
              onSelect={(option) => updateRelation("fornecedor", option)}
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
              options={withDeleteAction("tamanho", sizeOptionsQuery.data ?? [])}
              emptyLabel={
                sizeOptionsQuery.isError
                  ? "Falha ao carregar tamanhos."
                  : "Nenhum tamanho encontrado."
              }
              actionLabel="Criar novo tamanho"
              onSearchChange={(value) => updateLookupSearch("tamanho", value)}
              onAction={() => openAuxiliaryModal("tamanho")}
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
              options={withDeleteAction("cor", colorOptionsQuery.data ?? [])}
              emptyLabel={
                colorOptionsQuery.isError ? "Falha ao carregar cores." : "Nenhuma cor encontrada."
              }
              actionLabel="Criar nova cor"
              onSearchChange={(value) => updateLookupSearch("cor", value)}
              onAction={() => openAuxiliaryModal("cor")}
              onSelect={(option) => updateRelation("cor", option)}
            />
            <FormField
              label="Preco"
              type="number"
              inputMode="decimal"
              step="0.01"
              min="0"
              placeholder="0,00"
              value={values.preco}
              error={errors.preco}
              onChange={(value) => {
                updateField("preco", value);
                setErrors((current) => ({ ...current, preco: undefined }));
              }}
            />
            <FormField
              label="Quantidade"
              type="number"
              inputMode="numeric"
              step="1"
              min="1"
              placeholder="1"
              value={values.quantidade}
              error={errors.quantidade}
              onChange={(value) => {
                updateField("quantidade", value);
                setErrors((current) => ({ ...current, quantidade: undefined }));
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
              onClick={handleClose}
              className="flex h-12 cursor-pointer items-center justify-center rounded-2xl border border-[var(--border)] bg-[var(--surface-muted)] px-5 text-sm font-semibold text-[var(--foreground)] transition hover:border-[var(--border-strong)] hover:bg-white"
            >
              Cancelar
            </button>
            <button
              type="submit"
              disabled={createProductMutation.isPending || !storeId}
              className="flex h-12 cursor-pointer items-center justify-center rounded-2xl bg-[linear-gradient(90deg,_#ff8a3d,_#ff6b3d)] px-5 text-sm font-semibold text-white shadow-[0_16px_30px_rgba(255,107,61,0.28)] transition hover:brightness-105 disabled:cursor-not-allowed disabled:opacity-60"
            >
              {createProductMutation.isPending ? "Salvando produto..." : "Salvar produto"}
            </button>
          </div>
        </form>
      </div>

      <ProductAuxiliaryCreateModal
        error={auxiliaryModal.error}
        isOpen={auxiliaryModal.isOpen}
        isSubmitting={createAuxiliaryMutation.isPending}
        label={getAuxiliaryLabel(auxiliaryModal.field)}
        storeName={storeName}
        value={auxiliaryModal.value}
        onChange={(value) =>
          setAuxiliaryModal((current) => ({
            ...current,
            error: undefined,
            value,
          }))
        }
        onClose={closeAuxiliaryModal}
        onSubmit={handleAuxiliarySubmit}
      />
      <ProductAuxiliaryDeleteModal
        auxiliaryLabel={getAuxiliaryLabel(auxiliaryDeleteModal.field)}
        auxiliaryName={auxiliaryDeleteModal.value || null}
        isOpen={auxiliaryDeleteModal.isOpen}
        isSubmitting={deleteAuxiliaryMutation.isPending}
        onClose={closeAuxiliaryDeleteModal}
        onConfirm={handleAuxiliaryDeleteConfirm}
      />
      <ClientCreateModal
        errors={supplierFormErrors}
        isOpen={supplierModalOpen}
        isSubmitting={createSupplierMutation.isPending}
        storeName={storeName}
        values={supplierFormValues}
        onChange={updateSupplierField}
        onClose={closeSupplierModal}
        onSubmit={handleSupplierSubmit}
      />
    </div>
  );
}
