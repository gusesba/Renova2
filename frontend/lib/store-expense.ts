export type StoreExpenseNatureValue = 1 | 2;

export type StoreExpenseListItem = {
  id: number;
  lojaId: number;
  natureza: StoreExpenseNatureValue;
  valor: number;
  data: string;
  descricao: string | null;
};

export type StoreExpenseListResponse = {
  itens: StoreExpenseListItem[];
  pagina: number;
  tamanhoPagina: number;
  totalItens: number;
  totalPaginas: number;
};

export type StoreExpenseFilters = {
  pagina: number;
  tamanhoPagina: number;
  ordenarPor: "data" | "natureza" | "valor" | "descricao" | "id";
  direcao: "asc" | "desc";
};

export type StoreExpenseVisibleField = "data" | "natureza" | "valor" | "descricao" | "id";

export type StoreExpenseTableSettings = {
  tamanhoPagina: number;
  visibleFields: StoreExpenseVisibleField[];
};

type ApiErrorResponse = {
  mensagem?: unknown;
  title?: unknown;
};

export const storeExpenseNatureOptions: Array<{ label: string; value: StoreExpenseNatureValue }> =
  [
    { value: 1, label: "Recebimento" },
    { value: 2, label: "Pagamento" },
  ];

export const initialStoreExpenseFilters: StoreExpenseFilters = {
  pagina: 1,
  tamanhoPagina: 10,
  ordenarPor: "data",
  direcao: "desc",
};

const STORE_EXPENSE_TABLE_SETTINGS_KEY = "renova:store-expense-table-settings";

export const defaultStoreExpenseTableSettings: StoreExpenseTableSettings = {
  tamanhoPagina: initialStoreExpenseFilters.tamanhoPagina,
  visibleFields: ["data", "natureza", "valor", "descricao"],
};

export function asStoreExpenseListResponse(body: unknown) {
  return body as StoreExpenseListResponse;
}

export function getStoreExpenseApiMessage(body: unknown): string | null {
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

  return null;
}

export function buildStoreExpenseQuery(storeId: number, filters: StoreExpenseFilters) {
  const params = new URLSearchParams({
    lojaId: String(storeId),
    pagina: String(filters.pagina),
    tamanhoPagina: String(filters.tamanhoPagina),
    ordenarPor: filters.ordenarPor,
    direcao: filters.direcao,
  });

  return params.toString();
}

export function formatStoreExpenseNature(value: number) {
  return (
    storeExpenseNatureOptions.find((option) => option.value === value)?.label ?? `Natureza ${value}`
  );
}

export function getStoredStoreExpenseTableSettings(): StoreExpenseTableSettings {
  if (typeof window === "undefined") {
    return defaultStoreExpenseTableSettings;
  }

  const storedValue = window.localStorage.getItem(STORE_EXPENSE_TABLE_SETTINGS_KEY);

  if (!storedValue) {
    return defaultStoreExpenseTableSettings;
  }

  try {
    const parsed = JSON.parse(storedValue) as Partial<StoreExpenseTableSettings>;
    const tamanhoPagina =
      Number.isInteger(parsed.tamanhoPagina) && parsed.tamanhoPagina && parsed.tamanhoPagina > 0
        ? Math.min(parsed.tamanhoPagina, 100)
        : defaultStoreExpenseTableSettings.tamanhoPagina;
    const visibleFields = Array.isArray(parsed.visibleFields)
      ? parsed.visibleFields.filter((field): field is StoreExpenseVisibleField =>
          ["data", "natureza", "valor", "descricao", "id"].includes(String(field)),
        )
      : defaultStoreExpenseTableSettings.visibleFields;

    return {
      tamanhoPagina,
      visibleFields: visibleFields.length
        ? visibleFields
        : defaultStoreExpenseTableSettings.visibleFields,
    };
  } catch {
    return defaultStoreExpenseTableSettings;
  }
}

export function persistStoreExpenseTableSettings(settings: StoreExpenseTableSettings) {
  if (typeof window === "undefined") {
    return;
  }

  window.localStorage.setItem(STORE_EXPENSE_TABLE_SETTINGS_KEY, JSON.stringify(settings));
}
