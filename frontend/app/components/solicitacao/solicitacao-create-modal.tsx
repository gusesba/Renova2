"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { startTransition, useEffect, useMemo, useState } from "react";
import { toast } from "sonner";

import { ClientCreateModal } from "@/app/components/client/client-create-modal";
import { ProductAuxiliaryCreateModal } from "@/app/components/product/product-auxiliary-create-modal";
import { SearchableSelect } from "@/app/components/ui/searchable-select";
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
  getProductApiMessage,
  normalizeDecimalValue,
  type ProductLookupOption,
} from "@/lib/product";
import {
  asSolicitacaoResponse,
  extractSolicitacaoFieldErrors,
  getSolicitacaoApiMessage,
  initialSolicitacaoFormValues,
  type SolicitacaoCreateResponse,
  type SolicitacaoFieldErrors,
  type SolicitacaoFormValues,
} from "@/lib/solicitacao";
import { getAuthToken } from "@/lib/store";
import { createClient, getClients } from "@/services/client-service";
import {
  createProductBrand,
  createProductColor,
  createProductReference,
  createProductSize,
  getProductBrandOptions,
  getProductColorOptions,
  getProductReferenceOptions,
  getProductSizeOptions,
} from "@/services/product-service";
import { createSolicitacao } from "@/services/solicitacao-service";
import { clientSchema, mapClientZodErrors } from "@/validations/client";
import { mapSolicitacaoZodErrors, solicitacaoSchema } from "@/validations/solicitacao";

type SolicitacaoCreateModalProps = {
  isOpen: boolean;
  storeId: number | null;
  storeName: string | null;
  onClose: () => void;
  onSolicitacaoCreated: (solicitacao: SolicitacaoCreateResponse) => void;
};

type LookupSearchState = {
  produto: string;
  marca: string;
  tamanho: string;
  cor: string;
  cliente: string;
};

type AuxiliaryField = "produto" | "marca" | "tamanho" | "cor";

type AuxiliaryModalState = {
  error?: string;
  field: AuxiliaryField;
  isOpen: boolean;
  value: string;
};

function FormField({
  label,
  placeholder,
  value,
  error,
  type = "text",
  onChange,
}: {
  label: string;
  placeholder: string;
  value: string;
  error?: string;
  type?: "text" | "number";
  onChange: (value: string) => void;
}) {
  return (
    <label className="block space-y-2">
      <span className="text-sm font-semibold text-[var(--foreground)]">{label}</span>
      <input
        type={type}
        step={type === "number" ? "0.01" : undefined}
        min={type === "number" ? "0" : undefined}
        value={value}
        placeholder={placeholder}
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
  onClear,
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
  actionLabel?: string;
  onSearchChange: (value: string) => void;
  onAction?: () => void;
  onClear: () => void;
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
        clearAriaLabel={`Limpar ${label}`}
        onClear={onClear}
        placeholder={placeholder}
        searchPlaceholder={searchPlaceholder}
        searchValue={searchValue}
        selectedLabel={selectedLabel}
        value={value || null}
        actionLabel={actionLabel}
        onAction={onAction}
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

export function SolicitacaoCreateModal({
  isOpen,
  storeId,
  storeName,
  onClose,
  onSolicitacaoCreated,
}: SolicitacaoCreateModalProps) {
  const queryClient = useQueryClient();
  const [shouldRender, setShouldRender] = useState(isOpen);
  const [isVisible, setIsVisible] = useState(false);
  const [values, setValues] = useState<SolicitacaoFormValues>(initialSolicitacaoFormValues);
  const [errors, setErrors] = useState<SolicitacaoFieldErrors>({});
  const [lookupSearch, setLookupSearch] = useState<LookupSearchState>({
    produto: "",
    marca: "",
    tamanho: "",
    cor: "",
    cliente: "",
  });
  const [debouncedLookupSearch, setDebouncedLookupSearch] = useState<LookupSearchState>({
    produto: "",
    marca: "",
    tamanho: "",
    cor: "",
    cliente: "",
  });
  const [auxiliaryModal, setAuxiliaryModal] = useState<AuxiliaryModalState>({
    field: "produto",
    isOpen: false,
    value: "",
  });
  const [clientModalOpen, setClientModalOpen] = useState(false);
  const [clientFormValues, setClientFormValues] = useState<ClientFormValues>(initialClientFormValues);
  const [clientFormErrors, setClientFormErrors] = useState<ClientFieldErrors>({});
  const token = useMemo(() => (typeof window === "undefined" ? null : getAuthToken()), []);

  const createSolicitacaoMutation = useMutation({
    mutationFn: async (payload: {
      produtoId: number | null;
      marcaId: number | null;
      tamanhoId: number | null;
      corId: number | null;
      clienteId: number | null;
      descricao: string;
      precoMaximo: number | null;
      lojaId: number;
    }) => {
      if (!token) {
        throw new Error("Voce precisa estar autenticado para cadastrar uma solicitacao.");
      }

      return createSolicitacao(payload, token);
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

  const createClientMutation = useMutation({
    mutationFn: async (payload: {
      nome: string;
      contato: string;
      doacao: boolean;
      lojaId: number;
      userId?: number;
    }) => {
      if (!token) {
        throw new Error("Voce precisa estar autenticado para cadastrar um cliente.");
      }

      return createClient(payload, token);
    },
  });

  function resetForm() {
    setValues(initialSolicitacaoFormValues);
    setErrors({});
    setLookupSearch({
      produto: "",
      marca: "",
      tamanho: "",
      cor: "",
      cliente: "",
    });
    setDebouncedLookupSearch({
      produto: "",
      marca: "",
      tamanho: "",
      cor: "",
      cliente: "",
    });
    setAuxiliaryModal({
      field: "produto",
      isOpen: false,
      value: "",
    });
    setClientModalOpen(false);
    setClientFormValues(initialClientFormValues);
    setClientFormErrors({});
  }

  function handleClose() {
    if (createSolicitacaoMutation.isPending) {
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
      if (event.key === "Escape" && !createSolicitacaoMutation.isPending) {
        if (auxiliaryModal.isOpen || clientModalOpen) {
          return;
        }

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
    clientModalOpen,
    createSolicitacaoMutation.isPending,
    isOpen,
    onClose,
    shouldRender,
  ]);

  const productOptionsQuery = useQuery({
    queryKey: ["solicitacao-create-options", "produto", token, storeId, debouncedLookupSearch.produto],
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
    queryKey: ["solicitacao-create-options", "marca", token, storeId, debouncedLookupSearch.marca],
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
    queryKey: ["solicitacao-create-options", "tamanho", token, storeId, debouncedLookupSearch.tamanho],
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
    queryKey: ["solicitacao-create-options", "cor", token, storeId, debouncedLookupSearch.cor],
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

  const clientOptionsQuery = useQuery({
    queryKey: ["solicitacao-create-options", "cliente", token, storeId, debouncedLookupSearch.cliente],
    queryFn: async () => {
      if (!token || !storeId) {
        return [];
      }

      const response = await getClients(token, storeId, {
        nome: debouncedLookupSearch.cliente,
        contato: "",
        ordenarPor: "nome",
        direcao: "asc",
        pagina: 1,
        tamanhoPagina: 20,
      });

      if (!response.ok) {
        throw new Error("Nao foi possivel carregar os clientes.");
      }

      const body = response.body as { itens: Array<{ id: number; nome: string }> };
      return body.itens.map((item) => ({
        id: item.id,
        label: item.nome,
      }));
    },
    enabled: Boolean(isOpen && token && storeId),
  });

  function updateField<K extends keyof SolicitacaoFormValues>(
    field: K,
    value: SolicitacaoFormValues[K],
  ) {
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
    field: "produto" | "marca" | "tamanho" | "cor" | "cliente",
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

  function closeClientModal() {
    if (createClientMutation.isPending) {
      return;
    }

    setClientModalOpen(false);
    setClientFormValues(initialClientFormValues);
    setClientFormErrors({});
  }

  function updateClientField<K extends keyof ClientFormValues>(field: K, value: ClientFormValues[K]) {
    const normalizedValue = field === "contato" ? formatPhoneValue(String(value)) : value;

    setClientFormValues((current) => ({
      ...current,
      [field]: normalizedValue,
    }));
    setClientFormErrors((current) => ({
      ...current,
      [field]: undefined,
    }));
  }

  function clearRelation(field: "produto" | "marca" | "tamanho" | "cor" | "cliente") {
    const idField = `${field}Id` as const;
    const labelField = `${field}Label` as const;

    setValues((current) => ({
      ...current,
      [idField]: "",
      [labelField]: "",
    }));

    setErrors((current) => ({
      ...current,
      [idField]: undefined,
    }));

    updateLookupSearch(field, "");
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

      await queryClient.invalidateQueries({ queryKey: ["solicitacao-create-options"] });
      toast.success(`${getAuxiliaryLabel(field)} criado com sucesso.`);
    } catch (error) {
      toast.error(
        error instanceof Error
          ? error.message
          : "Nao foi possivel conectar ao backend. Verifique se a API esta em execucao.",
      );
    }
  }

  async function handleClientSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (!storeId) {
      toast.error("Selecione uma loja antes de cadastrar clientes.");
      return;
    }

    const validation = clientSchema.safeParse(clientFormValues);

    if (!validation.success) {
      setClientFormErrors(mapClientZodErrors(validation.error));
      return;
    }

    setClientFormErrors({});

    try {
      const response = await createClientMutation.mutateAsync({
        nome: validation.data.nome.trim(),
        contato: normalizeNumericValue(validation.data.contato),
        doacao: validation.data.doacao,
        lojaId: storeId,
        ...(validation.data.userId ? { userId: Number(validation.data.userId) } : {}),
      });

      if (!response.ok) {
        const apiErrors = extractClientFieldErrors(response.body);

        if (Object.keys(apiErrors).length > 0) {
          setClientFormErrors(apiErrors);
        }

        toast.error(getClientApiMessage(response.body) ?? "Nao foi possivel cadastrar o cliente.");
        return;
      }

      const createdClient = asClientResponse(response.body);

      updateRelation("cliente", {
        id: createdClient.id,
        label: createdClient.nome,
      });
      updateLookupSearch("cliente", createdClient.nome);
      setDebouncedLookupSearch((current) => ({
        ...current,
        cliente: createdClient.nome,
      }));
      setClientModalOpen(false);
      setClientFormValues(initialClientFormValues);
      setClientFormErrors({});

      await queryClient.invalidateQueries({ queryKey: ["solicitacao-create-options"] });
      await queryClient.invalidateQueries({ queryKey: ["clients"] });
      toast.success(`Cliente ${createdClient.nome} cadastrado com sucesso.`);
    } catch (error) {
      toast.error(
        error instanceof Error
          ? error.message
          : "Nao foi possivel conectar ao backend. Verifique se a API esta em execucao.",
      );
    }
  }

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (!storeId) {
      toast.error("Selecione uma loja antes de cadastrar solicitacoes.");
      return;
    }

    const validation = solicitacaoSchema.safeParse(values);

    if (!validation.success) {
      setErrors(mapSolicitacaoZodErrors(validation.error));
      return;
    }

    setErrors({});

    try {
      const payload = {
        produtoId: validation.data.produtoId.trim() ? Number(validation.data.produtoId) : null,
        marcaId: validation.data.marcaId.trim() ? Number(validation.data.marcaId) : null,
        tamanhoId: validation.data.tamanhoId.trim() ? Number(validation.data.tamanhoId) : null,
        corId: validation.data.corId.trim() ? Number(validation.data.corId) : null,
        clienteId: validation.data.clienteId.trim() ? Number(validation.data.clienteId) : null,
        descricao: validation.data.descricao.trim(),
        precoMaximo: validation.data.precoMaximo.trim()
          ? Number(normalizeDecimalValue(validation.data.precoMaximo))
          : null,
        lojaId: storeId,
      };

      const response = await createSolicitacaoMutation.mutateAsync(payload);

      if (!response.ok) {
        const apiFieldErrors = extractSolicitacaoFieldErrors(response.body);

        if (Object.keys(apiFieldErrors).length > 0) {
          setErrors(apiFieldErrors);
        }

        toast.error(
          getSolicitacaoApiMessage(response.body) ?? "Nao foi possivel cadastrar a solicitacao.",
        );
        return;
      }

      const createdSolicitacao = asSolicitacaoResponse(response.body);

      startTransition(() => {
        resetForm();
        onClose();
      });

      await queryClient.invalidateQueries({ queryKey: ["solicitacoes"] });
      toast.success("Solicitacao cadastrada com sucesso.");
      onSolicitacaoCreated(createdSolicitacao);
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
      className={`fixed inset-0 z-50 flex items-start justify-center overflow-y-auto bg-[rgba(15,23,42,0.45)] p-4 py-6 transition-opacity duration-200 ease-out sm:items-center sm:py-4 ${
        isVisible ? "opacity-100" : "opacity-0"
      }`}
    >
      <div
        className={`max-h-[calc(100vh-3rem)] w-full max-w-4xl overflow-y-auto rounded-[28px] border border-[var(--border)] bg-white p-6 shadow-[0_30px_90px_rgba(15,23,42,0.22)] transition duration-250 ease-out sm:max-h-[calc(100vh-2rem)] ${
          isVisible ? "translate-y-0 scale-100 opacity-100" : "translate-y-4 scale-[0.98] opacity-0"
        }`}
      >
        <div className="flex items-start justify-between gap-4">
          <div>
            <p className="text-sm font-semibold uppercase tracking-[0.16em] text-[var(--muted)]">
              Nova solicitacao
            </p>
            <h2 className="mt-2 text-2xl font-semibold tracking-tight text-[var(--foreground)]">
              Cadastro rapido na loja ativa
            </h2>
            <p className="mt-2 text-sm leading-7 text-[var(--muted)]">
              {storeName
                ? `As novas solicitacoes serao vinculadas a ${storeName}.`
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
              label="Cliente"
              error={errors.clienteId}
              disabled={!storeId}
              loading={clientOptionsQuery.isLoading}
              value={values.clienteId}
              selectedLabel={values.clienteLabel}
              searchValue={lookupSearch.cliente}
              placeholder="Qualquer cliente"
              searchPlaceholder="Pesquisar por nome"
              options={clientOptionsQuery.data ?? []}
              emptyLabel={
                clientOptionsQuery.isError ? "Falha ao carregar clientes." : "Nenhum cliente encontrado."
              }
              actionLabel="Criar novo cliente"
              onSearchChange={(value) => updateLookupSearch("cliente", value)}
              onAction={() => {
                setClientFormValues(initialClientFormValues);
                setClientFormErrors({});
                setClientModalOpen(true);
              }}
              onClear={() => clearRelation("cliente")}
              onSelect={(option) => updateRelation("cliente", option)}
            />
            <FormField
              label="Descricao"
              placeholder="Opcional"
              value={values.descricao}
              error={errors.descricao}
              onChange={(value) => {
                updateField("descricao", value);
                setErrors((current) => ({ ...current, descricao: undefined }));
              }}
            />
            <SearchableField
              label="Produto"
              error={errors.produtoId}
              disabled={!storeId}
              loading={productOptionsQuery.isLoading}
              value={values.produtoId}
              selectedLabel={values.produtoLabel}
              searchValue={lookupSearch.produto}
              placeholder="Qualquer produto"
              searchPlaceholder="Pesquisar por valor"
              options={productOptionsQuery.data ?? []}
              emptyLabel={
                productOptionsQuery.isError
                  ? "Falha ao carregar produtos."
                  : "Nenhum produto auxiliar encontrado."
              }
              actionLabel="Criar novo produto"
              onSearchChange={(value) => updateLookupSearch("produto", value)}
              onAction={() => openAuxiliaryModal("produto")}
              onClear={() => clearRelation("produto")}
              onSelect={(option) => updateRelation("produto", option)}
            />
            <SearchableField
              label="Marca"
              error={errors.marcaId}
              disabled={!storeId}
              loading={brandOptionsQuery.isLoading}
              value={values.marcaId}
              selectedLabel={values.marcaLabel}
              searchValue={lookupSearch.marca}
              placeholder="Qualquer marca"
              searchPlaceholder="Pesquisar por valor"
              options={brandOptionsQuery.data ?? []}
              emptyLabel={
                brandOptionsQuery.isError ? "Falha ao carregar marcas." : "Nenhuma marca encontrada."
              }
              actionLabel="Criar nova marca"
              onSearchChange={(value) => updateLookupSearch("marca", value)}
              onAction={() => openAuxiliaryModal("marca")}
              onClear={() => clearRelation("marca")}
              onSelect={(option) => updateRelation("marca", option)}
            />
            <SearchableField
              label="Tamanho"
              error={errors.tamanhoId}
              disabled={!storeId}
              loading={sizeOptionsQuery.isLoading}
              value={values.tamanhoId}
              selectedLabel={values.tamanhoLabel}
              searchValue={lookupSearch.tamanho}
              placeholder="Qualquer tamanho"
              searchPlaceholder="Pesquisar por valor"
              options={sizeOptionsQuery.data ?? []}
              emptyLabel={
                sizeOptionsQuery.isError
                  ? "Falha ao carregar tamanhos."
                  : "Nenhum tamanho encontrado."
              }
              actionLabel="Criar novo tamanho"
              onSearchChange={(value) => updateLookupSearch("tamanho", value)}
              onAction={() => openAuxiliaryModal("tamanho")}
              onClear={() => clearRelation("tamanho")}
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
              placeholder="Qualquer cor"
              searchPlaceholder="Pesquisar por valor"
              options={colorOptionsQuery.data ?? []}
              emptyLabel={
                colorOptionsQuery.isError ? "Falha ao carregar cores." : "Nenhuma cor encontrada."
              }
              actionLabel="Criar nova cor"
              onSearchChange={(value) => updateLookupSearch("cor", value)}
              onAction={() => openAuxiliaryModal("cor")}
              onClear={() => clearRelation("cor")}
              onSelect={(option) => updateRelation("cor", option)}
            />
            <FormField
              label="Preco maximo"
              type="number"
              placeholder="Opcional"
              value={values.precoMaximo}
              error={errors.precoMaximo}
              onChange={(value) => {
                updateField("precoMaximo", value);
                setErrors((current) => ({ ...current, precoMaximo: undefined }));
              }}
            />
          </div>

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
              disabled={createSolicitacaoMutation.isPending || !storeId}
              className="flex h-12 cursor-pointer items-center justify-center rounded-2xl bg-[linear-gradient(90deg,_#ff8a3d,_#ff6b3d)] px-5 text-sm font-semibold text-white shadow-[0_16px_30px_rgba(255,107,61,0.28)] transition hover:brightness-105 disabled:cursor-not-allowed disabled:opacity-60"
            >
              {createSolicitacaoMutation.isPending ? "Salvando solicitacao..." : "Salvar solicitacao"}
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
      <ClientCreateModal
        errors={clientFormErrors}
        isOpen={clientModalOpen}
        isSubmitting={createClientMutation.isPending}
        storeName={storeName}
        values={clientFormValues}
        onChange={updateClientField}
        onClose={closeClientModal}
        onSubmit={handleClientSubmit}
      />
    </div>
  );
}
