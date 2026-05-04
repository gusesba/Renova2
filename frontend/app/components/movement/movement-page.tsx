"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useSearchParams } from "next/navigation";
import { startTransition, useEffect, useEffectEvent, useMemo, useRef, useState } from "react";
import { toast } from "sonner";

import { useStoreContext } from "@/app/dashboard/store-context";
import { permissions } from "@/lib/access";
import { StoreConfigModal } from "@/app/components/layout/store-config-modal";
import { ClientCreateModal } from "@/app/components/client/client-create-modal";
import { MovementCreditAutoPaidModal } from "@/app/components/movement/movement-credit-auto-paid-modal";
import { MovementOverdueOwnerReturnModal } from "@/app/components/movement/movement-overdue-owner-return-modal";
import { PaymentConfigRequiredModal } from "@/app/components/movement/payment-config-required-modal";
import { PaymentCreditModal } from "@/app/components/payment/payment-credit-modal";
import { PrintConfirmationModal } from "@/app/components/printing/print-confirmation-modal";
import { ProductCreateModal } from "@/app/components/product/product-create-modal";
import { Select } from "@/app/components/ui/select";
import { SearchableSelect } from "@/app/components/ui/searchable-select";
import {
  asClientResponse,
  asClientListResponse,
  extractClientFieldErrors,
  formatPhoneValue,
  getClientApiMessage,
  initialClientFilters,
  initialClientFormValues,
  normalizeNumericValue,
  type ClientFieldErrors,
  type ClientFormValues,
  type ClientListItem,
} from "@/lib/client";
import {
  asMovementDestinationSuggestionResponse,
  asMovementResponse,
  buildMovementSuggestion,
  formatMovementType,
  getMovementApiMessage,
  isMissingStorePaymentConfigMessage,
  initialMovementDraftFormValues,
  isProductSituationCompatible,
  movementTypeOptions,
  type MovementDraftProduct,
  type MovementFieldErrors,
  type MovementSuggestion,
} from "@/lib/movement";
import {
  asProductListResponse,
  formatDateValue,
  formatCurrencyValue,
  formatSituacaoValue,
  getProductApiMessage,
  initialProductFilters,
  type ProductListItem,
} from "@/lib/product";
import {
  asStoreConfigResponse,
  getStoreConfig,
} from "@/services/store-config-service";
import { getAuthToken } from "@/lib/store";
import { createMovement, getMovementDestinationSuggestions } from "@/services/movement-service";
import { createClient, getClients } from "@/services/client-service";
import { getBorrowedProductsByClient, getProductById, getProducts } from "@/services/product-service";
import {
  extractStoreConfigApiMessage,
  type ConfigLojaResponse,
} from "@/lib/store-config";
import type { PendingClientItem } from "@/lib/payment";
import { mapMovementZodErrors, movementSchema } from "@/validations/movement";
import { openReceiptPdfPreview, printReceipt } from "@/lib/printing/actions";
import { getStoredPrintSettings } from "@/lib/printing/settings";
import type { PrintSettings, ReceiptPrintData } from "@/lib/printing/types";
import { clientSchema, mapClientZodErrors } from "@/validations/client";

type MovementDraft = {
  autoLinkedBorrowedProductIds: number[];
  clienteContato: string;
  clienteId: string;
  clienteLabel: string;
  clienteSearch: string;
  data: string;
  descontoTotal: string;
  errors: MovementFieldErrors;
  id: string;
  productIdInput: string;
  products: MovementDraftProduct[];
  suggestion: MovementSuggestion | null;
  tipo: string;
};

type OverdueOwnerReturnPrompt = {
  clientName: string;
  draftId: string;
  products: MovementDraftProduct[];
};

type CreditAutoPaidPrompt = {
  creditoAnterior: number;
  creditoAtual: number;
  nome: string;
};

function createDraft(id: string): MovementDraft {
  return {
    id,
    ...initialMovementDraftFormValues,
    autoLinkedBorrowedProductIds: [],
    clienteContato: "",
    clienteLabel: "",
    clienteSearch: "",
    data: initialMovementDraftFormValues.data,
    errors: {},
    productIdInput: "",
    products: [],
    suggestion: null,
    tipo: initialMovementDraftFormValues.tipo,
  };
}

function toUtcStartOfDay(value: string) {
  return new Date(`${value}T00:00:00.000Z`).toISOString();
}

function getDraftTitle(index: number, draft: MovementDraft) {
  return `Mov. ${index + 1} - ${formatMovementType(Number(draft.tipo))}`;
}

function parsePrefilledProductIds(value: string | null) {
  if (!value) {
    return [];
  }

  return [...new Set(
    value
      .split(",")
      .map((item) => Number(item.trim()))
      .filter((item) => Number.isInteger(item) && item > 0),
  )];
}

function isDraftPending(draft: MovementDraft) {
  return (
    draft.tipo !== initialMovementDraftFormValues.tipo ||
    draft.data !== initialMovementDraftFormValues.data ||
    draft.clienteId.trim().length > 0 ||
    draft.clienteLabel.trim().length > 0 ||
    draft.clienteSearch.trim().length > 0 ||
    draft.descontoTotal !== initialMovementDraftFormValues.descontoTotal ||
    draft.productIdInput.trim().length > 0 ||
    draft.products.length > 0 ||
    draft.suggestion !== null
  );
}

function parseDiscountValue(value: string) {
  const normalized = (value || "0").replace(",", ".");
  const parsed = Number(normalized);
  return Number.isFinite(parsed) ? parsed : 0;
}

function getEffectiveProductDiscount(draft: MovementDraft, product: MovementDraftProduct) {
  return parseDiscountValue(product.desconto) > 0
    ? parseDiscountValue(product.desconto)
    : Number(draft.tipo) === 1
      ? parseDiscountValue(draft.descontoTotal)
      : 0;
}

function getMonthsBetweenDates(startDate: string, endDate: string) {
  const start = new Date(`${startDate}T00:00:00.000Z`);
  const end = new Date(`${endDate}T00:00:00.000Z`);

  if (Number.isNaN(start.getTime()) || Number.isNaN(end.getTime()) || end < start) {
    return 0;
  }

  let months =
    (end.getUTCFullYear() - start.getUTCFullYear()) * 12 +
    (end.getUTCMonth() - start.getUTCMonth());

  if (end.getUTCDate() < start.getUTCDate()) {
    months -= 1;
  }

  return Math.max(months, 0);
}

function getAutomaticDiscountForProduct(
  draft: MovementDraft,
  product: ProductListItem,
  storeConfig: ConfigLojaResponse | null,
) {
  if (!storeConfig || !product.consignado || product.situacao !== 1) {
    return null;
  }

  const monthsInStore = getMonthsBetweenDates(product.entrada.slice(0, 10), draft.data);

  if (monthsInStore < storeConfig.tempoPermanenciaProdutoMeses) {
    return null;
  }

  const matchingDiscount = storeConfig.descontosPermanencia
    .filter((discount) => monthsInStore >= discount.aPartirDeMeses)
    .sort((left, right) => right.aPartirDeMeses - left.aPartirDeMeses)[0];

  if (!matchingDiscount || matchingDiscount.percentualDesconto <= 0) {
    return null;
  }

  return {
    monthsInStore,
    percentualDesconto: matchingDiscount.percentualDesconto,
  };
}

function FieldShell({
  children,
  error,
  label,
}: {
  children: React.ReactNode;
  error?: string;
  label: string;
}) {
  return (
    <label className="block space-y-2">
      <span className="text-sm font-semibold text-[var(--foreground)]">{label}</span>
      {children}
      {error ? <p className="text-sm text-red-500">{error}</p> : null}
    </label>
  );
}

function TextField({
  error,
  label,
  onChange,
  placeholder,
  type = "text",
  value,
}: {
  error?: string;
  label: string;
  onChange: (value: string) => void;
  placeholder?: string;
  type?: "date" | "text";
  value: string;
}) {
  return (
    <FieldShell error={error} label={label}>
      <input
        type={type}
        value={value}
        placeholder={placeholder}
        onChange={(event) => onChange(event.target.value)}
        className={`h-12 w-full rounded-2xl border bg-white px-4 text-sm text-[var(--foreground)] outline-none transition ${
          error
            ? "border-red-300 shadow-[0_0_0_4px_rgba(248,113,113,0.12)]"
            : "border-[var(--border)] focus:border-[var(--primary)] focus:shadow-[0_0_0_4px_rgba(106,92,255,0.12)]"
        }`}
      />
    </FieldShell>
  );
}

function TypeSelect({
  error,
  onChange,
  options,
  value,
}: {
  error?: string;
  onChange: (value: string) => void;
  options: Array<{ label: string; value: number }>;
  value: string;
}) {
  return (
    <FieldShell error={error} label="Tipo de movimentacao">
      <div
        className={`rounded-2xl border bg-white px-4 py-3 text-sm text-[var(--foreground)] transition ${
          error
            ? "border-red-300 shadow-[0_0_0_4px_rgba(248,113,113,0.12)]"
            : "border-[var(--border)]"
        }`}
      >
        <Select
          ariaLabel="Tipo de movimentacao"
          value={value}
          options={options.map((option) => ({
            label: option.label,
            value: String(option.value),
          }))}
          placeholder="Selecionar"
          onChange={onChange}
        />
      </div>
    </FieldShell>
  );
}

export function MovementPage() {
  const queryClient = useQueryClient();
  const searchParams = useSearchParams();
  const { hasPermission, isLoadingStores, selectedStore, selectedStoreId } = useStoreContext();
  const draftCounterRef = useRef(2);
  const prefillAppliedRef = useRef(false);
  const [drafts, setDrafts] = useState<MovementDraft[]>(() => [createDraft("draft-1")]);
  const [activeDraftId, setActiveDraftId] = useState("draft-1");
  const [debouncedClientSearch, setDebouncedClientSearch] = useState("");
  const [isCreateClientModalOpen, setIsCreateClientModalOpen] = useState(false);
  const [isCreateProductModalOpen, setIsCreateProductModalOpen] = useState(false);
  const [clientFormValues, setClientFormValues] = useState<ClientFormValues>(
    initialClientFormValues,
  );
  const [clientFormErrors, setClientFormErrors] = useState<ClientFieldErrors>({});
  const [isPrintingReceipt, setIsPrintingReceipt] = useState(false);
  const [isStoreConfigOpen, setIsStoreConfigOpen] = useState(false);
  const [isPaymentConfigRequiredOpen, setIsPaymentConfigRequiredOpen] = useState(false);
  const [paymentClient, setPaymentClient] = useState<PendingClientItem | null>(null);
  const [creditAutoPaidPrompt, setCreditAutoPaidPrompt] = useState<CreditAutoPaidPrompt | null>(null);
  const [autoLinkingDraftId, setAutoLinkingDraftId] = useState<string | null>(null);
  const [overdueOwnerReturnPrompt, setOverdueOwnerReturnPrompt] =
    useState<OverdueOwnerReturnPrompt | null>(null);
  const [receiptPreview, setReceiptPreview] = useState<ReceiptPrintData | null>(null);
  const [printSettings, setPrintSettings] = useState<PrintSettings>(() => getStoredPrintSettings());
  const token = useMemo(() => (typeof window === "undefined" ? null : getAuthToken()), []);
  const canCreateMovement = hasPermission(permissions.movimentacoesAdicionar);
  const canAddClient = hasPermission(permissions.clientesAdicionar);
  const canAddProduct = hasPermission(permissions.produtosAdicionar);
  const canHandleCredit =
    hasPermission(permissions.pagamentosCreditoAdicionar) ||
    hasPermission(permissions.pagamentosCreditoResgatar);
  const canEditStoreConfig = hasPermission(permissions.configLojaEditar);

  const activeDraft = useMemo(
    () => drafts.find((draft) => draft.id === activeDraftId) ?? drafts[0] ?? null,
    [activeDraftId, drafts],
  );
  const hasPendingMovements = useMemo(() => drafts.some(isDraftPending), [drafts]);
  const activeDraftTotalPrice = useMemo(
    () =>
      activeDraft?.products.reduce((total, product) => {
        const effectiveDiscount = getEffectiveProductDiscount(activeDraft, product);

        return total + product.preco * Math.max(0, 1 - effectiveDiscount / 100);
      }, 0) ?? 0,
    [activeDraft],
  );

  useEffect(() => {
    const timeoutId = window.setTimeout(() => {
      setDebouncedClientSearch(activeDraft?.clienteSearch ?? "");
    }, 300);

    return () => {
      window.clearTimeout(timeoutId);
    };
  }, [activeDraft?.clienteSearch]);

  useEffect(() => {
    startTransition(() => {
      draftCounterRef.current = 2;
      setDrafts([createDraft("draft-1")]);
      setActiveDraftId("draft-1");
    });
    prefillAppliedRef.current = false;
  }, [selectedStoreId]);

  useEffect(() => {
    if (!hasPendingMovements) {
      return;
    }

    const handleBeforeUnload = (event: BeforeUnloadEvent) => {
      event.preventDefault();
      event.returnValue = "";
    };

    window.addEventListener("beforeunload", handleBeforeUnload);

    return () => {
      window.removeEventListener("beforeunload", handleBeforeUnload);
    };
  }, [hasPendingMovements]);

  useEffect(() => {
    if (!hasPendingMovements) {
      return;
    }

    const handleInternalNavigation = (event: MouseEvent) => {
      if (
        event.defaultPrevented ||
        event.button !== 0 ||
        event.metaKey ||
        event.ctrlKey ||
        event.shiftKey ||
        event.altKey
      ) {
        return;
      }

      if (!(event.target instanceof Element)) {
        return;
      }

      const anchor = event.target.closest("a[href]");

      if (!(anchor instanceof HTMLAnchorElement) || (anchor.target && anchor.target !== "_self")) {
        return;
      }

      const href = anchor.getAttribute("href");

      if (!href || href.startsWith("#") || href.startsWith("mailto:") || href.startsWith("tel:")) {
        return;
      }

      const currentUrl = new URL(window.location.href);
      const nextUrl = new URL(anchor.href, currentUrl);

      if (
        nextUrl.origin !== currentUrl.origin ||
        (nextUrl.pathname === currentUrl.pathname && nextUrl.search === currentUrl.search)
      ) {
        return;
      }

      const canNavigate = window.confirm(
        "Existe uma movimentacao nao finalizada. Deseja sair desta pagina mesmo assim?",
      );

      if (!canNavigate) {
        event.preventDefault();
        event.stopPropagation();
      }
    };

    document.addEventListener("click", handleInternalNavigation, true);

    return () => {
      document.removeEventListener("click", handleInternalNavigation, true);
    };
  }, [hasPendingMovements]);

  useEffect(() => {
    if (!token || !selectedStoreId || !activeDraft?.clienteId) {
      return;
    }

    let cancelled = false;
    const draftId = activeDraft.id;
    const clienteId = Number(activeDraft.clienteId);

    setAutoLinkingDraftId(draftId);

    void (async () => {
      try {
        const response = await getBorrowedProductsByClient(token, selectedStoreId, clienteId);

        if (cancelled) {
          return;
        }

        if (!response.ok) {
          toast.error(
            getProductApiMessage(response.body) ??
              "Nao foi possivel carregar os produtos emprestados deste cliente.",
          );
          return;
        }

        const borrowedProducts = (response.body as ProductListItem[]) ?? [];

        updateDraft(draftId, (draft) => {
          if (draft.clienteId !== String(clienteId)) {
            return draft;
          }

          const manualProducts = draft.products.filter(
            (product) => !draft.autoLinkedBorrowedProductIds.includes(product.id),
          );

          return {
            ...draft,
            products: [
              ...manualProducts,
              ...borrowedProducts.filter(
                (product) =>
                  !manualProducts.some((manualProduct) => manualProduct.id === product.id),
              ).map((product) => ({ ...product, desconto: "0" })),
            ],
            autoLinkedBorrowedProductIds: borrowedProducts.map((product) => product.id),
            errors: { ...draft.errors, produtos: undefined },
          };
        });
      } catch (error) {
        if (!cancelled) {
          toast.error(
            error instanceof Error
              ? error.message
              : "Nao foi possivel carregar os produtos emprestados deste cliente.",
          );
        }
      } finally {
        if (!cancelled) {
          setAutoLinkingDraftId((current) => (current === draftId ? null : current));
        }
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [activeDraft?.clienteId, activeDraft?.id, selectedStoreId, token]);
  const trimmedClientSearch = debouncedClientSearch.trim();

  const clientOptionsQuery = useQuery({
    queryKey: ["movement-clients", token, selectedStoreId, trimmedClientSearch],
    queryFn: async () => {
      if (!token || !selectedStoreId) {
        return [];
      }

      const response = await getClients(token, selectedStoreId, {
        ...initialClientFilters,
        nome: trimmedClientSearch,
        tamanhoPagina: 5,
      });

      if (!response.ok) {
        throw new Error(
          getClientApiMessage(response.body) ?? "Nao foi possivel carregar os clientes.",
        );
      }

      return asClientListResponse(response.body).itens;
    },
    enabled: Boolean(token && selectedStoreId && activeDraft && trimmedClientSearch),
  });

  const storeConfigQuery = useQuery({
    queryKey: ["store-config", token, selectedStoreId],
    queryFn: async () => {
      if (!token || !selectedStoreId) {
        return null;
      }

      const response = await getStoreConfig(selectedStoreId, token);

      if (response.status === 404) {
        return null;
      }

      if (!response.ok) {
        throw new Error(
          extractStoreConfigApiMessage(response.body) ??
            "Nao foi possivel carregar a configuracao da loja.",
        );
      }

      return asStoreConfigResponse(response.body);
    },
    enabled: Boolean(token && selectedStoreId),
    staleTime: 60_000,
  });

  const fetchProductMutation = useMutation({
    mutationFn: async (etiqueta: string) => {
      if (!token) {
        throw new Error("Voce precisa estar autenticado para buscar produtos.");
      }

      if (!selectedStoreId) {
        throw new Error("Selecione uma loja valida antes de buscar produtos.");
      }

      return getProducts(token, selectedStoreId, {
        ...initialProductFilters,
        etiqueta,
        pagina: 1,
        tamanhoPagina: 1,
      });
    },
  });

  const createClientMutation = useMutation({
    mutationFn: async (payload: {
      nome: string;
      contato: string;
      obs?: string;
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

  const createMovementMutation = useMutation({
    mutationFn: async (draft: MovementDraft) => {
      if (!token || !selectedStoreId) {
        throw new Error("Selecione uma loja valida antes de criar movimentacoes.");
      }

      return createMovement(
        {
          tipo: Number(draft.tipo),
          data: toUtcStartOfDay(draft.data),
          clienteId: Number(draft.clienteId),
          lojaId: selectedStoreId,
          descontoTotal:
            Number(draft.tipo) === 1 ? Number(draft.descontoTotal.replace(",", ".")) || 0 : 0,
          produtos: draft.products.map((product) => ({
            produtoId: product.id,
            desconto:
              Number(draft.tipo) === 1 ? Number(product.desconto.replace(",", ".")) || 0 : 0,
          })),
        },
        token,
      );
    },
  });

  const clientOptions = useMemo(
    () =>
      (clientOptionsQuery.data ?? []).map((client) => ({
        label: `${client.nome} - ${formatPhoneValue(client.contato)}`,
        value: String(client.id),
        raw: client,
      })),
    [clientOptionsQuery.data],
  );

  function updateDraft(draftId: string, updater: (draft: MovementDraft) => MovementDraft) {
    setDrafts((current) => current.map((draft) => (draft.id === draftId ? updater(draft) : draft)));
  }

  const applyPrefilledProducts = useEffectEvent((products: ProductListItem[]) => {
    updateDraft("draft-1", (current) => {
      const existingIds = new Set(current.products.map((product) => product.id));
      const compatibleProducts = products.filter((product) =>
        isProductSituationCompatible(Number(current.tipo), product.situacao),
      );

      if (compatibleProducts.every((product) => existingIds.has(product.id))) {
        return current;
      }

      const nextProducts = [...current.products];

      for (const product of compatibleProducts) {
        if (existingIds.has(product.id)) {
          continue;
        }

        const automaticDiscount = getAutomaticDiscountForProduct(
          current,
          product,
          storeConfigQuery.data ?? null,
        );

        nextProducts.push({
          ...product,
          desconto: automaticDiscount ? String(automaticDiscount.percentualDesconto) : "0",
        });
        existingIds.add(product.id);
      }

      return {
        ...current,
        productIdInput: "",
        products: nextProducts,
        autoLinkedBorrowedProductIds: current.autoLinkedBorrowedProductIds.filter(
          (itemId) => !existingIds.has(itemId),
        ),
        errors: { ...current.errors, produtos: undefined },
      };
    });
  });

  useEffect(() => {
    if (prefillAppliedRef.current || !token || !selectedStoreId) {
      return;
    }

    const clientId = searchParams.get("clienteId");
    const clientName = searchParams.get("clienteNome") ?? "";
    const clientContact = searchParams.get("clienteContato") ?? "";
    const type = searchParams.get("tipo");
    const productIds = parsePrefilledProductIds(searchParams.get("produtoIds"));

    if (!clientId && !type && productIds.length === 0) {
      prefillAppliedRef.current = true;
      return;
    }

    const normalizedType =
      type && ["1", "2", "3", "4", "5", "6"].includes(type) ? type : initialMovementDraftFormValues.tipo;

    prefillAppliedRef.current = true;

    startTransition(() => {
      setDrafts([
        {
          ...createDraft("draft-1"),
          tipo: normalizedType,
          clienteId: clientId ?? "",
          clienteContato: clientContact,
          clienteLabel: clientName,
          clienteSearch: "",
        },
      ]);
      setActiveDraftId("draft-1");
    });

    if (productIds.length === 0) {
      return;
    }

    void (async () => {
      const loadedProducts: ProductListItem[] = [];

      for (const productId of productIds) {
        try {
          const response = await getProductById(productId, token);

          if (!response.ok) {
            toast.error(
              getProductApiMessage(response.body) ??
                `Nao foi possivel carregar o produto ${productId} enviado pelo atalho.`,
            );
            continue;
          }

          loadedProducts.push(response.body as ProductListItem);
        } catch (error) {
          toast.error(
            error instanceof Error
              ? error.message
              : `Nao foi possivel carregar o produto ${productId} enviado pelo atalho.`,
          );
        }
      }

      if (loadedProducts.length === 0) {
        return;
      }

      applyPrefilledProducts(loadedProducts);

      toast.success(`${loadedProducts.length} produto(s) carregado(s) no rascunho.`);
    })();
  }, [searchParams, selectedStoreId, token]);

  function addDraft(partial?: Partial<MovementDraft>) {
    const nextId = `draft-${draftCounterRef.current++}`;
    const nextDraft = {
      ...createDraft(nextId),
      ...partial,
    };

    startTransition(() => {
      setDrafts((current) => [...current, nextDraft]);
      setActiveDraftId(nextId);
    });
  }

  function removeDraft(draftId: string) {
    if (drafts.length === 1) {
      startTransition(() => {
        setDrafts([createDraft("draft-1")]);
        setActiveDraftId("draft-1");
      });
      return;
    }

    const currentIndex = drafts.findIndex((draft) => draft.id === draftId);
    const fallbackDraft = drafts[currentIndex - 1] ?? drafts[currentIndex + 1];

    startTransition(() => {
      setDrafts((current) => current.filter((draft) => draft.id !== draftId));
      setActiveDraftId(fallbackDraft.id);
    });
  }

  function handleTypeChange(draftId: string, tipo: string) {
    updateDraft(draftId, (draft) => ({
      ...draft,
      tipo,
      suggestion: null,
      descontoTotal: tipo === "1" ? draft.descontoTotal : "0",
      errors: { ...draft.errors, tipo: undefined, descontoTotal: undefined, produtos: undefined },
    }));
  }

  function handleClientSelect(draftId: string, client: ClientListItem) {
    updateDraft(draftId, (draft) => ({
      ...draft,
      autoLinkedBorrowedProductIds: [],
      clienteContato: client.contato,
      clienteId: String(client.id),
      clienteLabel: client.nome,
      clienteSearch: "",
      errors: { ...draft.errors, clienteId: undefined },
    }));
  }

  function openClientModal() {
    setClientFormValues(initialClientFormValues);
    setClientFormErrors({});
    setIsCreateClientModalOpen(true);
  }

  function closeClientModal() {
    if (createClientMutation.isPending) {
      return;
    }

    setIsCreateClientModalOpen(false);
    setClientFormValues(initialClientFormValues);
    setClientFormErrors({});
  }

  function updateClientField<K extends keyof ClientFormValues>(
    field: K,
    value: ClientFormValues[K],
  ) {
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

  async function handleClientSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (!selectedStoreId) {
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
        ...(validation.data.obs.trim() ? { obs: validation.data.obs.trim() } : {}),
        doacao: validation.data.doacao,
        lojaId: selectedStoreId,
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

      handleClientSelect(activeDraftId, createdClient);
      void checkOverdueOwnerReturnProducts(activeDraftId, createdClient);
      setDebouncedClientSearch(createdClient.nome);
      setIsCreateClientModalOpen(false);
      setClientFormValues(initialClientFormValues);
      setClientFormErrors({});

      await queryClient.invalidateQueries({ queryKey: ["movement-clients"] });
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

  async function checkOverdueOwnerReturnProducts(draftId: string, client: ClientListItem) {
    if (!token || !selectedStoreId) {
      return;
    }

    try {
      const response = await getMovementDestinationSuggestions(token, selectedStoreId);

      if (!response.ok) {
        return;
      }

      if (client.doacao) {
        return;
      }

      const overdueProducts = asMovementDestinationSuggestionResponse(response.body).produtos
        .filter((product) => product.fornecedorId === client.id)
        .map((product) => ({ ...product, desconto: "0" }));

      if (overdueProducts.length === 0) {
        return;
      }

      setOverdueOwnerReturnPrompt({
        clientName: client.nome,
        draftId,
        products: overdueProducts,
      });
    } catch {
      // Nao bloqueia a selecao do cliente se a consulta auxiliar falhar.
    }
  }

  function buildIncompatibleProductsMessage(draft: MovementDraft) {
    const incompatibleProducts = draft.products.filter(
      (product) => !isProductSituationCompatible(Number(draft.tipo), product.situacao),
    );

    if (incompatibleProducts.length === 0) {
      return null;
    }

    return `Os produtos ${incompatibleProducts.map((product) => product.id).join(", ")} nao respeitam o tipo ${formatMovementType(Number(draft.tipo))}.`;
  }

  async function handleAddProduct(draft: MovementDraft) {
    const rawValue = draft.productIdInput.trim();

    if (!rawValue) {
      updateDraft(draft.id, (current) => ({
        ...current,
        errors: { ...current.errors, produtos: "Informe a etiqueta de um produto." },
      }));
      return;
    }

    const parsedEtiqueta = Number(rawValue);

    if (!Number.isInteger(parsedEtiqueta) || parsedEtiqueta < 0) {
      updateDraft(draft.id, (current) => ({
        ...current,
        errors: { ...current.errors, produtos: "Informe uma etiqueta numerica valida." },
      }));
      return;
    }

    if (draft.products.some((product) => product.etiqueta === parsedEtiqueta)) {
      toast.error(`O produto com etiqueta ${parsedEtiqueta} ja foi adicionado nesta movimentacao.`);
      return;
    }

    try {
      const response = await fetchProductMutation.mutateAsync(rawValue);

      if (!response.ok) {
        toast.error(getProductApiMessage(response.body) ?? "Nao foi possivel buscar o produto.");
        return;
      }

      const product = asProductListResponse(response.body).itens[0];

      if (!product) {
        toast.error(`Nenhum produto encontrado com a etiqueta ${parsedEtiqueta}.`);
        return;
      }

      if (selectedStoreId && product.lojaId !== selectedStoreId) {
        toast.error("O produto buscado pertence a outra loja.");
        return;
      }

      if (!isProductSituationCompatible(Number(draft.tipo), product.situacao)) {
        updateDraft(draft.id, (current) => ({
          ...current,
          productIdInput: "",
          suggestion: buildMovementSuggestion(Number(current.tipo), product),
          errors: { ...current.errors, produtos: undefined },
        }));
        return;
      }

      const automaticDiscount = getAutomaticDiscountForProduct(
        draft,
        product,
        storeConfigQuery.data ?? null,
      );
      const productDiscount = automaticDiscount
        ? String(automaticDiscount.percentualDesconto)
        : "0";

      updateDraft(draft.id, (current) => ({
        ...current,
        productIdInput: "",
        products: [{ ...product, desconto: productDiscount }, ...current.products],
        autoLinkedBorrowedProductIds: current.autoLinkedBorrowedProductIds.filter(
          (itemId) => itemId !== product.id,
        ),
        suggestion: null,
        errors: { ...current.errors, produtos: undefined },
      }));
      toast.success(`Produto ${product.id} adicionado na movimentacao.`);

      if (automaticDiscount) {
        toast.info(
          `Desconto automatico de ${automaticDiscount.percentualDesconto}% aplicado ao produto ${product.id} por estar ha ${automaticDiscount.monthsInStore} mes(es) na loja.`,
        );
      }
    } catch (error) {
      toast.error(
        error instanceof Error
          ? error.message
          : "Nao foi possivel conectar ao backend. Verifique se a API esta em execucao.",
      );
    }
  }

  function appendProductToDraft(draftId: string, product: ProductListItem) {
    const targetDraft = drafts.find((draft) => draft.id === draftId);

    if (!targetDraft) {
      return;
    }

    if (selectedStoreId && product.lojaId !== selectedStoreId) {
      toast.error("O produto cadastrado pertence a outra loja.");
      return;
    }

    if (targetDraft.products.some((item) => item.id === product.id)) {
      toast.error(`O produto ${product.id} ja foi adicionado nesta movimentacao.`);
      return;
    }

    if (!isProductSituationCompatible(Number(targetDraft.tipo), product.situacao)) {
      updateDraft(draftId, (current) => ({
        ...current,
        productIdInput: "",
        suggestion: buildMovementSuggestion(Number(current.tipo), product),
        errors: { ...current.errors, produtos: undefined },
      }));
      return;
    }

    const automaticDiscount = getAutomaticDiscountForProduct(
      targetDraft,
      product,
      storeConfigQuery.data ?? null,
    );
    const productDiscount = automaticDiscount ? String(automaticDiscount.percentualDesconto) : "0";

    updateDraft(draftId, (current) => ({
      ...current,
      productIdInput: "",
      products: [{ ...product, desconto: productDiscount }, ...current.products],
      autoLinkedBorrowedProductIds: current.autoLinkedBorrowedProductIds.filter(
        (itemId) => itemId !== product.id,
      ),
      suggestion: null,
      errors: { ...current.errors, produtos: undefined },
    }));
    toast.success(`Produto ${product.id} adicionado na movimentacao.`);

    if (automaticDiscount) {
      toast.info(
        `Desconto automatico de ${automaticDiscount.percentualDesconto}% aplicado ao produto ${product.id} por estar ha ${automaticDiscount.monthsInStore} mes(es) na loja.`,
      );
    }
  }

  function acceptSuggestion(draft: MovementDraft) {
    if (!draft.suggestion?.suggestedType) {
      return;
    }

    const automaticDiscount = getAutomaticDiscountForProduct(
      {
        ...draft,
        tipo: String(draft.suggestion.suggestedType),
      },
      draft.suggestion.product,
      storeConfigQuery.data ?? null,
    );

    addDraft({
      tipo: String(draft.suggestion.suggestedType),
      data: draft.data,
      descontoTotal: draft.descontoTotal,
      clienteId: draft.clienteId,
      clienteLabel: draft.clienteLabel,
      clienteSearch: draft.clienteLabel || draft.clienteSearch,
      products: [
        {
          ...draft.suggestion.product,
          desconto: automaticDiscount ? String(automaticDiscount.percentualDesconto) : "0",
        },
      ],
    });

    updateDraft(draft.id, (current) => ({
      ...current,
      suggestion: null,
    }));

    if (automaticDiscount) {
      toast.info(
        `Desconto automatico de ${automaticDiscount.percentualDesconto}% aplicado ao produto ${draft.suggestion.product.id} por estar ha ${automaticDiscount.monthsInStore} mes(es) na loja.`,
      );
    }
  }

  function buildReceiptFromDraft(draft: MovementDraft, movementId: number) {
    return {
      buyer: draft.clienteLabel,
      movementId,
      printedAt: new Date(),
      products: draft.products,
      sellType: Number(draft.tipo) as ReceiptPrintData["sellType"],
    };
  }

  function handleOpenDraftReceiptConfirmation() {
    if (!activeDraft) {
      return;
    }

    if (Number(activeDraft.tipo) !== 1) {
      toast.error("A nota esta disponivel apenas para movimentacoes de venda.");
      return;
    }

    if (!activeDraft.clienteLabel || activeDraft.products.length === 0) {
      toast.error("Selecione o cliente e adicione produtos antes de imprimir a nota.");
      return;
    }

    setPrintSettings(getStoredPrintSettings());
    setReceiptPreview(buildReceiptFromDraft(activeDraft, 0));
  }

  async function handlePrintReceiptConfirmation() {
    if (!receiptPreview) {
      return;
    }

    setIsPrintingReceipt(true);

    try {
      await printReceipt(receiptPreview);
      toast.success("Nota enviada para impressao.");
      setReceiptPreview(null);
    } catch (error) {
      toast.error(error instanceof Error ? error.message : "Nao foi possivel imprimir a nota.");
    } finally {
      setIsPrintingReceipt(false);
    }
  }

  async function handleSubmit(draft: MovementDraft) {
    if (!selectedStoreId) {
      toast.error("Selecione uma loja antes de criar movimentacoes.");
      return;
    }

    const validation = movementSchema.safeParse({
      tipo: draft.tipo,
      data: draft.data,
      clienteId: draft.clienteId,
      descontoTotal: Number(draft.tipo) === 1 ? draft.descontoTotal : "0",
    });

    const nextErrors = validation.success ? {} : mapMovementZodErrors(validation.error);
    const incompatibleMessage = buildIncompatibleProductsMessage(draft);
    const invalidDiscountProduct = draft.products.find((product) => {
      const productDiscount = parseDiscountValue(product.desconto);
      return productDiscount < 0 || productDiscount > 100;
    });

    if (draft.products.length === 0) {
      nextErrors.produtos = "Adicione ao menos um produto nesta movimentacao.";
    } else if (incompatibleMessage) {
      nextErrors.produtos = incompatibleMessage;
    } else if (invalidDiscountProduct) {
      nextErrors.produtos =
        `Revise o desconto do produto ${invalidDiscountProduct.id}. O desconto unitario precisa ficar entre 0% e 100%.`;
    }

    if (Object.keys(nextErrors).length > 0) {
      updateDraft(draft.id, (current) => ({
        ...current,
        errors: nextErrors,
      }));
      toast.error("Revise os campos obrigatorios antes de salvar.");
      return;
    }

    updateDraft(draft.id, (current) => ({
      ...current,
      errors: {},
    }));

    try {
      const response = await createMovementMutation.mutateAsync(draft);

      if (!response.ok) {
        const message =
          getMovementApiMessage(response.body) ?? "Nao foi possivel criar a movimentacao.";

        if (isMissingStorePaymentConfigMessage(message)) {
          setIsPaymentConfigRequiredOpen(true);
          return;
        }

        toast.error(message);
        return;
      }

      const createdMovement = asMovementResponse(response.body);
      await queryClient.invalidateQueries({ queryKey: ["products"] });

      if (createdMovement.tipo === 1) {
        setPrintSettings(getStoredPrintSettings());
        setReceiptPreview(buildReceiptFromDraft(draft, createdMovement.id));
      }

      toast.success(
        `Movimentacao ${createdMovement.id} criada como ${formatMovementType(createdMovement.tipo)}.`,
      );

      await queryClient.invalidateQueries({ queryKey: ["pending-clients"] });

      if (
        createdMovement.tipo === 1 &&
        typeof createdMovement.creditoClienteAntesPagamento === "number" &&
        typeof createdMovement.creditoClienteAtual === "number" &&
        createdMovement.creditoClienteAntesPagamento > createdMovement.creditoClienteAtual &&
        createdMovement.creditoClienteAtual >= 0
      ) {
        setCreditAutoPaidPrompt({
          creditoAnterior: createdMovement.creditoClienteAntesPagamento,
          creditoAtual: createdMovement.creditoClienteAtual,
          nome: draft.clienteLabel,
        });
      } else if (
        createdMovement.tipo === 1 &&
        typeof createdMovement.creditoPendenteCliente === "number" &&
        createdMovement.creditoPendenteCliente < 0
      ) {
        setPaymentClient({
          clienteId: createdMovement.clienteId,
          nome: draft.clienteLabel,
          contato: draft.clienteContato,
          credito: createdMovement.creditoPendenteCliente,
        });
      }

      if (drafts.length === 1) {
        updateDraft(draft.id, () => createDraft(draft.id));
        return;
      }

      removeDraft(draft.id);
    } catch (error) {
      toast.error(
        error instanceof Error
          ? error.message
          : "Nao foi possivel conectar ao backend. Verifique se a API esta em execucao.",
      );
    }
  }

  const hasStore = Boolean(selectedStoreId);

  return (
    <section className="space-y-6">
      <div className="overflow-hidden rounded-[32px] border border-[var(--border)] bg-white shadow-[0_20px_55px_rgba(15,23,42,0.08)]">
        <div className="border-b border-[var(--border)] bg-[linear-gradient(135deg,_#fef4ea_0%,_#fffaf4_42%,_#eef7ff_100%)] px-6 py-6">
          <div className="flex flex-col gap-4 lg:flex-row lg:items-end lg:justify-between">
            <div>
              <p className="text-sm font-semibold uppercase tracking-[0.18em] text-[var(--muted)]">
                Movimentacoes
              </p>
              <h1 className="mt-2 text-3xl font-semibold tracking-tight text-[var(--foreground)]">
                Criacao simultanea de transicoes
              </h1>
              <p className="mt-3 max-w-3xl text-sm leading-7 text-[var(--muted)]">
                Escolha o cliente, defina o tipo da movimentacao e adicione produtos pelo id. Se um
                item nao combinar com o tipo atual, a tela sugere abrir outra aba de movimentacao
                mais apropriada.
              </p>
            </div>

            <div className="flex flex-col gap-3 sm:flex-row lg:items-end">
              <button
                type="button"
                onClick={handleOpenDraftReceiptConfirmation}
                className="flex h-11 cursor-pointer items-center justify-center rounded-2xl border border-white/80 bg-white px-5 text-sm font-semibold text-[var(--foreground)] shadow-[0_12px_26px_rgba(15,23,42,0.06)] transition hover:border-[var(--border-strong)]"
              >
                Imprimir nota
              </button>
              <div className="rounded-3xl border border-white/80 bg-white/85 px-5 py-4 shadow-[0_18px_36px_rgba(15,23,42,0.06)] backdrop-blur">
                <p className="text-xs font-semibold uppercase tracking-[0.16em] text-[var(--muted)]">
                  Loja ativa
                </p>
                <p className="mt-2 text-lg font-semibold text-[var(--foreground)]">
                  {selectedStore?.nome ?? "Nenhuma loja selecionada"}
                </p>
              </div>
            </div>
          </div>
        </div>

        <div className="px-6 py-6">
          {!hasStore ? (
            <div className="rounded-[28px] border border-dashed border-[var(--border-strong)] bg-[var(--surface-muted)] px-6 py-12 text-center">
              <h2 className="text-xl font-semibold text-[var(--foreground)]">Selecione uma loja</h2>
              <p className="mt-3 text-sm leading-7 text-[var(--muted)]">
                A criacao de movimentacoes depende da loja ativa no topo da aplicacao.
              </p>
            </div>
          ) : (
            <div className="space-y-5">
              <div className="flex flex-col gap-3 lg:flex-row lg:items-center lg:justify-between">
                <div className="flex flex-wrap gap-3">
                  {drafts.map((draft, index) => {
                    const active = draft.id === activeDraftId;

                    return (
                      <button
                        key={draft.id}
                        type="button"
                        onClick={() => setActiveDraftId(draft.id)}
                        className={`group flex cursor-pointer items-center gap-3 rounded-2xl border px-4 py-3 text-left transition ${
                          active
                            ? "border-[var(--primary)] bg-[var(--primary-soft)] text-[var(--primary)] shadow-[0_16px_30px_rgba(106,92,255,0.14)]"
                            : "border-[var(--border)] bg-[var(--surface-muted)] text-[var(--muted)] hover:border-[var(--border-strong)] hover:bg-white"
                        }`}
                      >
                        <span className="flex flex-col">
                          <span className="text-xs uppercase tracking-[0.14em]">
                            Aba {index + 1}
                          </span>
                          <span className="text-sm font-semibold text-[inherit]">
                            {getDraftTitle(index, draft)}
                          </span>
                        </span>
                        <span className="rounded-full bg-white/85 px-2.5 py-1 text-xs font-semibold text-[inherit]">
                          {draft.products.length} itens
                        </span>
                        <span
                          role="button"
                          aria-label={`Fechar ${getDraftTitle(index, draft)}`}
                          onClick={(event) => {
                            event.stopPropagation();
                            removeDraft(draft.id);
                          }}
                          className="flex h-7 w-7 items-center justify-center rounded-full border border-transparent text-xs transition group-hover:border-[var(--border)]"
                        >
                          x
                        </span>
                      </button>
                    );
                  })}
                </div>

                {canCreateMovement ? (
                  <button
                    type="button"
                    onClick={() => addDraft()}
                    className="flex h-12 cursor-pointer items-center justify-center rounded-2xl bg-[linear-gradient(90deg,_#ff8a3d,_#ff6b3d)] px-5 text-sm font-semibold text-white shadow-[0_16px_30px_rgba(255,107,61,0.24)] transition hover:brightness-105"
                  >
                    Nova aba de movimentacao
                  </button>
                ) : null}
              </div>

              {activeDraft ? (
                <div className="grid gap-6 xl:grid-cols-[minmax(0,1.3fr)_minmax(360px,0.9fr)]">
                  <div className="space-y-5 rounded-[28px] border border-[var(--border)] bg-[var(--surface-muted)] p-5">
                    <div className="grid gap-4 md:grid-cols-2">
                      <TypeSelect
                        error={activeDraft.errors.tipo}
                        value={activeDraft.tipo}
                        options={movementTypeOptions}
                        onChange={(value) => handleTypeChange(activeDraft.id, value)}
                      />

                      <TextField
                        label="Data da movimentacao"
                        type="date"
                        value={activeDraft.data}
                        error={activeDraft.errors.data}
                        onChange={(value) =>
                          updateDraft(activeDraft.id, (draft) => ({
                            ...draft,
                            data: value,
                            errors: { ...draft.errors, data: undefined },
                          }))
                        }
                      />

                      <div className="md:col-span-2">
                        <span className="mb-2 block text-sm font-semibold text-[var(--foreground)]">
                          Cliente que realiza a movimentacao
                        </span>
                        <SearchableSelect
                          ariaLabel="Cliente da movimentacao"
                          disabled={!hasStore}
                          emptyLabel={
                            !trimmedClientSearch
                              ? "Digite para buscar clientes."
                              : clientOptionsQuery.isError
                                ? "Falha ao carregar os clientes."
                                : "Nenhum cliente encontrado."
                          }
                          error={activeDraft.errors.clienteId}
                          loading={
                            isLoadingStores ||
                            (Boolean(trimmedClientSearch) && clientOptionsQuery.isLoading)
                          }
                          options={
                            trimmedClientSearch
                              ? clientOptions.map((option) => ({
                                  label: option.label,
                                  value: option.value,
                                }))
                              : []
                          }
                          placeholder="Selecione um cliente"
                          searchPlaceholder="Pesquisar por nome"
                          searchValue={activeDraft.clienteSearch}
                          selectedLabel={activeDraft.clienteLabel}
                          value={activeDraft.clienteId || null}
                          actionLabel={canAddClient ? "Criar novo cliente" : undefined}
                          onAction={canAddClient ? openClientModal : undefined}
                          onSearchChange={(value) =>
                            updateDraft(activeDraft.id, (draft) => ({
                              ...draft,
                              clienteSearch: value,
                            }))
                          }
                          onChange={(option) => {
                            const selectedClient = clientOptionsQuery.data?.find(
                              (client) => client.id === Number(option.value),
                            );

                            if (!selectedClient) {
                              return;
                            }

                            handleClientSelect(activeDraft.id, selectedClient);
                            void checkOverdueOwnerReturnProducts(activeDraft.id, selectedClient);
                          }}
                        />
                        {activeDraft.errors.clienteId ? (
                          <p className="mt-2 text-sm text-red-500">
                            {activeDraft.errors.clienteId}
                          </p>
                        ) : null}
                      </div>
                    </div>

                    <div className="rounded-[24px] border border-[var(--border)] bg-white p-4">
                      <div className="flex flex-col gap-3 2xl:flex-row">
                        <div className="min-w-0 flex-1">
                          <TextField
                            label="Adicionar produto pela etiqueta"
                            placeholder="Ex.: 152"
                            value={activeDraft.productIdInput}
                            error={activeDraft.errors.produtos}
                            onChange={(value) =>
                              updateDraft(activeDraft.id, (draft) => ({
                                ...draft,
                                productIdInput: value.replace(/[^\d]/g, ""),
                                errors: { ...draft.errors, produtos: undefined },
                              }))
                            }
                          />
                        </div>
                        {canCreateMovement ? (
                          <div className="flex w-full flex-col gap-3 sm:flex-row sm:flex-wrap 2xl:mt-7 2xl:w-auto 2xl:shrink-0 2xl:flex-nowrap">
                            {canAddProduct ? (
                              <button
                                type="button"
                                onClick={() => setIsCreateProductModalOpen(true)}
                                className="flex h-12 min-w-0 flex-1 cursor-pointer items-center justify-center rounded-2xl border border-[var(--border)] bg-white px-5 text-sm font-semibold text-[var(--foreground)] transition hover:border-[var(--border-strong)] sm:flex-none"
                              >
                                Novo produto
                              </button>
                            ) : null}
                            <button
                              type="button"
                              onClick={() => handleAddProduct(activeDraft)}
                              disabled={fetchProductMutation.isPending}
                              className="flex h-12 min-w-0 flex-1 cursor-pointer items-center justify-center rounded-2xl bg-[var(--primary)] px-5 text-center text-sm font-semibold text-white transition hover:brightness-105 disabled:cursor-not-allowed disabled:opacity-60 sm:flex-none"
                            >
                              {fetchProductMutation.isPending ? "Buscando..." : "Buscar e adicionar"}
                            </button>
                          </div>
                        ) : null}
                      </div>

                      {activeDraft.suggestion ? (
                        <div className="mt-4 rounded-[24px] border border-amber-200 bg-amber-50 px-4 py-4 text-sm text-amber-900">
                          <p className="font-medium">{activeDraft.suggestion.message}</p>
                          <div className="mt-3 flex flex-wrap gap-3">
                            {activeDraft.suggestion.suggestedType ? (
                              <button
                                type="button"
                                onClick={() => acceptSuggestion(activeDraft)}
                                className="flex h-10 cursor-pointer items-center justify-center rounded-2xl bg-amber-500 px-4 text-sm font-semibold text-white transition hover:brightness-105"
                              >
                                Abrir nova aba com{" "}
                                {formatMovementType(activeDraft.suggestion.suggestedType)}
                              </button>
                            ) : null}
                            <button
                              type="button"
                              onClick={() =>
                                updateDraft(activeDraft.id, (draft) => ({
                                  ...draft,
                                  suggestion: null,
                                }))
                              }
                              className="flex h-10 cursor-pointer items-center justify-center rounded-2xl border border-amber-200 bg-white px-4 text-sm font-semibold text-amber-900 transition hover:border-amber-300"
                            >
                              Entendi
                            </button>
                          </div>
                        </div>
                      ) : null}
                    </div>

                    <div className="flex flex-col gap-3 lg:flex-row lg:items-end lg:justify-between">
                      <div className="w-full lg:w-[260px]">
                        <TextField
                          label="Desconto total da movimentacao (%)"
                          value={activeDraft.descontoTotal}
                          error={activeDraft.errors.descontoTotal}
                          onChange={(value) =>
                            updateDraft(activeDraft.id, (draft) => ({
                              ...draft,
                              descontoTotal: value.replace(/[^\d.,]/g, ""),
                              errors: { ...draft.errors, descontoTotal: undefined, produtos: undefined },
                            }))
                          }
                        />
                      </div>
                      <div className="flex flex-col gap-3 sm:flex-row sm:justify-end">
                        <button
                          type="button"
                          onClick={() => removeDraft(activeDraft.id)}
                          className="flex h-12 cursor-pointer items-center justify-center rounded-2xl border border-[var(--border)] bg-white px-5 text-sm font-semibold text-[var(--foreground)] transition hover:border-[var(--border-strong)]"
                        >
                          Limpar aba
                        </button>
                        {canCreateMovement ? (
                          <button
                            type="button"
                            onClick={() => handleSubmit(activeDraft)}
                            disabled={createMovementMutation.isPending}
                            className="flex h-12 cursor-pointer items-center justify-center rounded-2xl bg-[linear-gradient(90deg,_#ff8a3d,_#ff6b3d)] px-5 text-sm font-semibold text-white shadow-[0_16px_30px_rgba(255,107,61,0.24)] transition hover:brightness-105 disabled:cursor-not-allowed disabled:opacity-60"
                          >
                            {createMovementMutation.isPending ? "Salvando..." : "Salvar movimentacao"}
                          </button>
                        ) : null}
                      </div>
                    </div>
                  </div>

                  <div className="rounded-[28px] border border-[var(--border)] bg-white p-5">
                    <div className="flex items-start justify-between gap-3">
                      <div>
                        <p className="text-sm font-semibold uppercase tracking-[0.16em] text-[var(--muted)]">
                          Produtos vinculados
                        </p>
                        <h2 className="mt-2 text-2xl font-semibold text-[var(--foreground)]">
                          {activeDraft.products.length} item(ns) nesta aba
                        </h2>
                      </div>
                      <span className="rounded-full bg-[var(--primary-soft)] px-3 py-1 text-xs font-semibold text-[var(--primary)]">
                        {formatMovementType(Number(activeDraft.tipo))}
                      </span>
                    </div>

                    <div className="mt-5 rounded-[24px] border border-[var(--border)] bg-[linear-gradient(135deg,_#fff6eb_0%,_#ffffff_100%)] px-4 py-4">
                      <p className="text-xs font-semibold uppercase tracking-[0.16em] text-[var(--muted)]">
                        Preview do valor dos itens
                      </p>
                      <p className="mt-2 text-3xl font-semibold tracking-tight text-[var(--foreground)]">
                        {formatCurrencyValue(activeDraftTotalPrice)}
                      </p>
                      <p className="mt-2 text-sm text-[var(--muted)]">
                        {Number(activeDraft.tipo) === 1
                          ? "Preview com o desconto total como padrao por peca, sobrescrito quando a peca tiver desconto proprio."
                          : "Preview com desconto unitario quando a peca tiver valor configurado. No envio, movimentacoes sem venda continuam sendo salvas com desconto zerado."}
                      </p>
                    </div>

                    {activeDraft.products.length === 0 ? (
                      <div className="mt-5 rounded-[24px] border border-dashed border-[var(--border-strong)] bg-[var(--surface-muted)] px-5 py-10 text-center">
                        <p className="text-lg font-semibold text-[var(--foreground)]">
                          Nenhum produto adicionado
                        </p>
                        <p className="mt-2 text-sm leading-7 text-[var(--muted)]">
                          {autoLinkingDraftId === activeDraft.id
                            ? "Carregando emprestados do cliente selecionado..."
                            : "Busque um produto pela etiqueta para compor esta movimentacao."}
                        </p>
                      </div>
                    ) : (
                      <div className="mt-5 space-y-3">
                        {activeDraft.products.map((product) => {
                          const isCompatible = isProductSituationCompatible(
                            Number(activeDraft.tipo),
                            product.situacao,
                          );

                          return (
                            <article
                              key={product.id}
                              className={`rounded-[24px] border px-4 py-4 transition ${
                                isCompatible
                                  ? "border-[var(--border)] bg-[var(--surface-muted)]"
                                  : "border-red-200 bg-red-50"
                              }`}
                            >
                              <div className="flex items-start justify-between gap-3">
                                <div>
                                  <p className="text-sm font-semibold text-[var(--foreground)]">
                                    #{product.id} - {product.produto}
                                  </p>
                                  <p className="mt-1 text-sm text-[var(--muted)]">
                                    {product.descricao}
                                  </p>
                                </div>
                                <button
                                  type="button"
                                  onClick={() =>
                                    updateDraft(activeDraft.id, (draft) => ({
                                      ...draft,
                                      products: draft.products.filter(
                                        (item) => item.id !== product.id,
                                      ),
                                      autoLinkedBorrowedProductIds:
                                        draft.autoLinkedBorrowedProductIds.filter(
                                          (itemId) => itemId !== product.id,
                                        ),
                                      errors: { ...draft.errors, produtos: undefined },
                                    }))
                                  }
                                  className="flex h-9 w-9 cursor-pointer items-center justify-center rounded-full border border-[var(--border)] bg-white text-sm text-[var(--muted)] transition hover:border-[var(--border-strong)] hover:text-[var(--foreground)]"
                                  aria-label={`Remover produto ${product.id}`}
                                >
                                  x
                                </button>
                              </div>

                              <div className="mt-4 flex flex-wrap gap-2">
                                <span className="rounded-full bg-white px-3 py-1 text-xs font-semibold text-[var(--foreground)]">
                                  Marca - {product.marca}
                                </span>
                                <span className="rounded-full bg-white px-3 py-1 text-xs font-semibold text-[var(--foreground)]">
                                  Preco - {formatCurrencyValue(product.preco)}
                                </span>
                                <span className="rounded-full bg-white px-3 py-1 text-xs font-semibold text-[var(--foreground)]">
                                  Desconto - {getEffectiveProductDiscount(activeDraft, product).toFixed(2)}%
                                </span>
                                <span className="rounded-full bg-white px-3 py-1 text-xs font-semibold text-[var(--foreground)]">
                                  Entrada - {formatDateValue(product.entrada)}
                                </span>
                                <span
                                  className={`rounded-full px-3 py-1 text-xs font-semibold ${
                                    isCompatible
                                      ? "bg-emerald-100 text-emerald-700"
                                      : "bg-red-100 text-red-700"
                                  }`}
                                >
                                  Situacao - {formatSituacaoValue(product.situacao)}
                                </span>
                              </div>

                              <div className="mt-4 grid gap-3 md:grid-cols-2">
                                <TextField
                                  label="Desconto da peca (%)"
                                  value={product.desconto}
                                  onChange={(value) =>
                                    updateDraft(activeDraft.id, (draft) => ({
                                      ...draft,
                                      products: draft.products.map((item) =>
                                        item.id === product.id
                                          ? { ...item, desconto: value.replace(/[^\d.,]/g, "") }
                                          : item,
                                      ),
                                      errors: { ...draft.errors, produtos: undefined },
                                    }))
                                  }
                                />
                              </div>

                              {!isCompatible ? (
                                <p className="mt-3 text-sm text-red-600">
                                  Este produto nao respeita o tipo atual e impedira o salvamento
                                  enquanto permanecer nesta aba.
                                </p>
                              ) : null}
                            </article>
                          );
                        })}
                      </div>
                    )}
                  </div>
                </div>
              ) : null}
            </div>
          )}
        </div>
      </div>

      <PaymentConfigRequiredModal
        isOpen={isPaymentConfigRequiredOpen}
        storeName={selectedStore?.nome ?? null}
        onClose={() => setIsPaymentConfigRequiredOpen(false)}
        onOpenSettings={() => {
          setIsPaymentConfigRequiredOpen(false);
          setIsStoreConfigOpen(true);
        }}
      />

      <PrintConfirmationModal
        description={
          receiptPreview
            ? `${receiptPreview.products.length} item(ns) na nota de ${receiptPreview.buyer || "cliente"}.`
            : ""
        }
        isOpen={Boolean(receiptPreview)}
        isPrinting={isPrintingReceipt}
        onClose={() => setReceiptPreview(null)}
        onPreviewPdf={() => {
          if (receiptPreview) {
            void openReceiptPdfPreview(receiptPreview);
          }
        }}
        onPrint={() => void handlePrintReceiptConfirmation()}
        settings={printSettings}
        target="receipt"
        title={
          receiptPreview?.movementId
            ? `Imprimir nota ${receiptPreview.movementId}`
            : "Imprimir nota"
        }
      />

      {canHandleCredit ? (
        <PaymentCreditModal
          client={paymentClient}
          initialPaymentType={1}
          isOpen={Boolean(paymentClient)}
          storeId={selectedStoreId}
          storeName={selectedStore?.nome ?? null}
          onClose={() => setPaymentClient(null)}
          onSuccess={async () => {
            await queryClient.invalidateQueries({ queryKey: ["pending-clients"] });
          }}
        />
      ) : null}

      {canAddClient ? (
        <ClientCreateModal
          errors={clientFormErrors}
          isOpen={isCreateClientModalOpen}
          isSubmitting={createClientMutation.isPending}
          storeName={selectedStore?.nome ?? null}
          values={clientFormValues}
          onChange={updateClientField}
          onClose={closeClientModal}
          onSubmit={handleClientSubmit}
        />
      ) : null}

      <MovementCreditAutoPaidModal
        credit={creditAutoPaidPrompt}
        isOpen={Boolean(creditAutoPaidPrompt)}
        onClose={() => setCreditAutoPaidPrompt(null)}
      />

      {canEditStoreConfig ? (
        <StoreConfigModal
          isOpen={isStoreConfigOpen}
          storeId={selectedStoreId}
          storeName={selectedStore?.nome ?? null}
          onClose={() => setIsStoreConfigOpen(false)}
        />
      ) : null}

      <ProductCreateModal
        isOpen={isCreateProductModalOpen}
        storeId={selectedStoreId}
        storeName={selectedStore?.nome ?? null}
        onClose={() => setIsCreateProductModalOpen(false)}
        onProductCreated={(createdProduct) => {
          const draftId = activeDraftId;

          void (async () => {
            try {
              const response = await getProductById(createdProduct.id, token ?? "");

              if (!response.ok) {
                toast.error(
                  getProductApiMessage(response.body) ??
                    "Produto foi criado, mas nao foi possivel adiciona-lo na movimentacao.",
                );
                return;
              }

              appendProductToDraft(draftId, response.body as ProductListItem);
            } catch (error) {
              toast.error(
                error instanceof Error
                  ? error.message
                  : "Produto foi criado, mas nao foi possivel adiciona-lo na movimentacao.",
              );
            }
          })();
        }}
      />

      <MovementOverdueOwnerReturnModal
        isOpen={Boolean(overdueOwnerReturnPrompt)}
        clientName={overdueOwnerReturnPrompt?.clientName ?? ""}
        productCount={overdueOwnerReturnPrompt?.products.length ?? 0}
        onClose={() => setOverdueOwnerReturnPrompt(null)}
        onConfirm={() => {
          if (!overdueOwnerReturnPrompt) {
            return;
          }

          const sourceDraft =
            drafts.find((draft) => draft.id === overdueOwnerReturnPrompt.draftId) ?? activeDraft;

          addDraft({
            tipo: "4",
            data: sourceDraft?.data ?? initialMovementDraftFormValues.data,
            clienteId: sourceDraft?.clienteId ?? "",
            clienteContato: sourceDraft?.clienteContato ?? "",
            clienteLabel: sourceDraft?.clienteLabel ?? overdueOwnerReturnPrompt.clientName,
            clienteSearch:
              sourceDraft?.clienteLabel ||
              sourceDraft?.clienteSearch ||
              overdueOwnerReturnPrompt.clientName,
            products: overdueOwnerReturnPrompt.products,
          });
          setOverdueOwnerReturnPrompt(null);
        }}
      />
    </section>
  );
}
