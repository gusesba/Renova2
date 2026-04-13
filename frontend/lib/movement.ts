import { formatSituacaoValue, type ProductListItem } from "@/lib/product";

type ApiErrorResponse = {
  mensagem?: unknown;
  title?: unknown;
  errors?: Record<string, string[] | undefined>;
};

export type MovementTypeValue = 1 | 2 | 3 | 4 | 5 | 6;

export type MovementDraftFormValues = {
  tipo: string;
  data: string;
  clienteId: string;
  descontoTotal: string;
};

export type MovementFieldErrors = Partial<Record<keyof MovementDraftFormValues | "produtos", string>>;

export type MovementDraftProduct = ProductListItem & {
  desconto: string;
};

export type MovementCreateResponse = {
  id: number;
  tipo: MovementTypeValue;
  data: string;
  clienteId: number;
  lojaId: number;
  creditoPendenteCliente: number | null;
  produtoIds: number[];
};

export type MovementDestinationProduct = ProductListItem & {
  tipoSugerido: MovementTypeValue;
};

export type MovementDestinationSuggestionResponse = {
  lojaId: number;
  tempoPermanenciaProdutoMeses: number;
  dataLimitePermanencia: string;
  produtos: MovementDestinationProduct[];
};

export type MovementListItem = {
  id: number;
  tipo: MovementTypeValue;
  data: string;
  clienteId: number;
  cliente: string;
  lojaId: number;
  quantidadeProdutos: number;
  produtos: ProductListItem[];
};

export type MovementListResponse = {
  itens: MovementListItem[];
  pagina: number;
  tamanhoPagina: number;
  totalItens: number;
  totalPaginas: number;
};

export type MovementFilters = {
  dataInicial: string;
  dataFinal: string;
  cliente: string;
  tipo: string;
  ordenarPor: "data" | "cliente" | "tipo" | "id";
  direcao: "asc" | "desc";
  pagina: number;
  tamanhoPagina: number;
};

export type MovementVisibleField = "id" | "data" | "cliente" | "quantidadeProdutos" | "tipo";

export type MovementTableSettings = {
  tamanhoPagina: number;
  visibleFields: MovementVisibleField[];
};

export type MovementSuggestion = {
  message: string;
  product: ProductListItem;
  suggestedType: MovementTypeValue | null;
};

export const movementTypeOptions: Array<{ label: string; value: MovementTypeValue }> = [
  { value: 1, label: "Venda" },
  { value: 2, label: "Emprestimo" },
  { value: 3, label: "Doacao" },
  { value: 4, label: "Devolucao dono" },
  { value: 5, label: "Devolucao venda" },
  { value: 6, label: "Devolucao emprestimo" },
];

export const initialMovementDraftFormValues: MovementDraftFormValues = {
  tipo: "1",
  data: new Date().toISOString().slice(0, 10),
  clienteId: "",
  descontoTotal: "0",
};

export const initialMovementFilters: MovementFilters = {
  dataInicial: "",
  dataFinal: "",
  cliente: "",
  tipo: "",
  ordenarPor: "data",
  direcao: "desc",
  pagina: 1,
  tamanhoPagina: 10,
};

export const defaultMovementTableSettings: MovementTableSettings = {
  tamanhoPagina: 10,
  visibleFields: ["id", "data", "cliente", "quantidadeProdutos", "tipo"],
};

const movementTableSettingsStorageKey = "renova.movementTableSettings";

export function asMovementResponse(body: unknown) {
  return body as MovementCreateResponse;
}

export function asMovementListResponse(body: unknown) {
  return body as MovementListResponse;
}

export function asMovementDestinationSuggestionResponse(body: unknown) {
  return body as MovementDestinationSuggestionResponse;
}

export function asMovementBatchResponse(body: unknown) {
  return body as MovementCreateResponse[];
}

export function formatMovementType(value: number) {
  return movementTypeOptions.find((option) => option.value === value)?.label ?? `Tipo ${value}`;
}

export function getMovementApiMessage(body: unknown): string | null {
  if (!body || typeof body !== "object") {
    return null;
  }

  const data = body as ApiErrorResponse;

  if (typeof data.mensagem === "string" && data.mensagem.trim()) {
    return data.mensagem;
  }

  if (typeof data.title === "string" && data.title.trim()) {
    return data.title;
  }

  if (data.errors) {
    const firstError = Object.values(data.errors).flat().find(Boolean);

    if (firstError) {
      return firstError;
    }
  }

  return null;
}

export function isMissingStorePaymentConfigMessage(message: string | null) {
  if (!message) {
    return false;
  }

  const normalizedMessage = message.trim().toLowerCase();

  return (
    normalizedMessage.includes("configuracao de repasse") ||
    normalizedMessage.includes("configuração de repasse") ||
    normalizedMessage.includes("repasse ao fornecedor")
  );
}

function toApiDateStart(value: string) {
  return `${value}T00:00:00`;
}

function toApiDateEnd(value: string) {
  return `${value}T23:59:59.999`;
}

export function buildMovementQuery(storeId: number, filters: MovementFilters) {
  const params = new URLSearchParams({
    lojaId: String(storeId),
    pagina: String(filters.pagina),
    tamanhoPagina: String(filters.tamanhoPagina),
    ordenarPor: filters.ordenarPor,
    direcao: filters.direcao,
  });

  if (filters.dataInicial) {
    params.set("dataInicial", toApiDateStart(filters.dataInicial));
  }

  if (filters.dataFinal) {
    params.set("dataFinal", toApiDateEnd(filters.dataFinal));
  }

  if (filters.cliente.trim()) {
    params.set("cliente", filters.cliente.trim());
  }

  if (filters.tipo.trim()) {
    params.set("tipo", filters.tipo.trim());
  }

  return params.toString();
}

export function formatMovementDate(value: string) {
  const parsed = new Date(value);

  if (Number.isNaN(parsed.getTime())) {
    return value;
  }

  return new Intl.DateTimeFormat("pt-BR", {
    dateStyle: "short",
    timeZone: "UTC",
  }).format(parsed);
}

export function getStoredMovementTableSettings(): MovementTableSettings {
  if (typeof window === "undefined") {
    return defaultMovementTableSettings;
  }

  const rawValue = window.localStorage.getItem(movementTableSettingsStorageKey);

  if (!rawValue) {
    return defaultMovementTableSettings;
  }

  try {
    const parsed = JSON.parse(rawValue) as Partial<MovementTableSettings>;
    const tamanhoPagina =
      typeof parsed.tamanhoPagina === "number" &&
      Number.isInteger(parsed.tamanhoPagina) &&
      parsed.tamanhoPagina > 0 &&
      parsed.tamanhoPagina <= 100
        ? parsed.tamanhoPagina
        : defaultMovementTableSettings.tamanhoPagina;

    const visibleFields = Array.isArray(parsed.visibleFields)
      ? parsed.visibleFields.filter((field): field is MovementVisibleField =>
          ["id", "data", "cliente", "quantidadeProdutos", "tipo"].includes(String(field)),
        )
      : defaultMovementTableSettings.visibleFields;

    return {
      tamanhoPagina,
      visibleFields: visibleFields.length
        ? visibleFields
        : defaultMovementTableSettings.visibleFields,
    };
  } catch {
    return defaultMovementTableSettings;
  }
}

export function persistMovementTableSettings(settings: MovementTableSettings) {
  if (typeof window === "undefined") {
    return;
  }

  window.localStorage.setItem(movementTableSettingsStorageKey, JSON.stringify(settings));
}

export function isProductSituationCompatible(movementType: number, productSituation: number) {
  if (movementType === 1 && productSituation === 4) {
    return true;
  }

  if (movementType === 5) {
    return productSituation === 2;
  }

  if (movementType === 6) {
    return productSituation === 4;
  }

  return productSituation === 1;
}

export function getSuggestedMovementType(
  currentMovementType: number,
  productSituation: number,
): MovementTypeValue | null {
  if (productSituation === 2) {
    return 5;
  }

  if (productSituation === 4) {
    return 6;
  }

  if (productSituation === 1 && currentMovementType === 5) {
    return 1;
  }

  if (productSituation === 1 && currentMovementType === 6) {
    return 2;
  }

  return null;
}

export function buildMovementSuggestion(
  currentMovementType: number,
  product: ProductListItem,
): MovementSuggestion {
  const suggestedType = getSuggestedMovementType(currentMovementType, product.situacao);
  const productSituation = formatSituacaoValue(product.situacao);
  const currentType = formatMovementType(currentMovementType);

  if (suggestedType) {
    return {
      suggestedType,
      product,
      message: `O produto ${product.id} esta em ${productSituation}, o que nao combina com ${currentType}. Deseja abrir uma nova movimentacao de ${formatMovementType(suggestedType)} e adicionar esse produto nela?`,
    };
  }

  return {
    suggestedType: null,
    product,
    message: `O produto ${product.id} esta em ${productSituation} e nao pode ser usado em ${currentType}. Escolha outro produto ou ajuste o tipo desta movimentacao.`,
  };
}
