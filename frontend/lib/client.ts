import type { LojaResponse } from "@/lib/store";

export type ClientListItem = {
  id: number;
  nome: string;
  contato: string;
  obs: string | null;
  doacao: boolean;
  lojaId: number;
  userId: number | null;
  userNome?: string | null;
  userEmail?: string | null;
};

export type ClientListResponse = {
  itens: ClientListItem[];
  pagina: number;
  tamanhoPagina: number;
  totalItens: number;
  totalPaginas: number;
};

export type ClientDetailFilters = {
  dataInicial: string;
  dataFinal: string;
  situacao: string;
};

export type ClientDetailResponse = ClientListItem & {
  quantidadePecasCompradas: number;
  quantidadePecasVendidas: number;
  valorRetiradoLoja: number;
  valorAportadoLoja: number;
  produtosFornecedor: Array<{
    id: number;
    preco: number;
    produtoId: number;
    produto: string;
    marcaId: number;
    marca: string;
    tamanhoId: number;
    tamanho: string;
    corId: number;
    cor: string;
    fornecedorId: number;
    fornecedor: string;
    descricao: string;
    entrada: string;
    lojaId: number;
    situacao: number;
    consignado: boolean;
  }>;
  produtosComCliente: Array<{
    id: number;
    preco: number;
    produtoId: number;
    produto: string;
    marcaId: number;
    marca: string;
    tamanhoId: number;
    tamanho: string;
    corId: number;
    cor: string;
    fornecedorId: number;
    fornecedor: string;
    descricao: string;
    entrada: string;
    lojaId: number;
    situacao: number;
    consignado: boolean;
  }>;
};

export type ClientFormValues = {
  nome: string;
  contato: string;
  obs: string;
  doacao: boolean;
  userId: string;
};

export type ClientFieldErrors = Partial<Record<keyof ClientFormValues, string>>;

export type ClientUserOption = {
  id: number;
  nome: string;
  email: string;
};

export type ClientFilters = {
  nome: string;
  contato: string;
  ordenarPor: "nome" | "contato" | "id";
  direcao: "asc" | "desc";
  pagina: number;
  tamanhoPagina: number;
};

export type ClientVisibleField = "nome" | "contato" | "obs" | "doacao" | "userId" | "id";

export type ClientTableSettings = {
  tamanhoPagina: number;
  visibleFields: ClientVisibleField[];
};

type ApiErrorResponse = {
  mensagem?: unknown;
  title?: unknown;
  errors?: Record<string, string[] | undefined>;
};

export const initialClientFormValues: ClientFormValues = {
  nome: "",
  contato: "",
  obs: "",
  doacao: false,
  userId: "",
};

export const initialClientFilters: ClientFilters = {
  nome: "",
  contato: "",
  ordenarPor: "nome",
  direcao: "asc",
  pagina: 1,
  tamanhoPagina: 10,
};

export const initialClientDetailFilters: ClientDetailFilters = {
  dataInicial: "",
  dataFinal: "",
  situacao: "",
};

export const defaultClientTableSettings: ClientTableSettings = {
  tamanhoPagina: 10,
  visibleFields: ["nome", "contato", "obs", "doacao", "userId", "id"],
};

export function normalizeNumericValue(value: string) {
  return value.replace(/\D+/g, "");
}

export function formatPhoneValue(value: string) {
  const digits = normalizeNumericValue(value).slice(0, 11);

  if (digits.length <= 2) {
    return digits.length ? `(${digits}` : "";
  }

  const ddd = digits.slice(0, 2);
  const remaining = digits.slice(2);

  if (remaining.length <= 4) {
    return `(${ddd}) ${remaining}`;
  }

  if (digits.length <= 10) {
    return `(${ddd}) ${remaining.slice(0, 4)}-${remaining.slice(4)}`;
  }

  return `(${ddd}) ${remaining.slice(0, 5)}-${remaining.slice(5)}`;
}

const clientTableSettingsStorageKey = "renova.clientTableSettings";

export function asClientListResponse(body: unknown) {
  return body as ClientListResponse;
}

export function asClientResponse(body: unknown) {
  return body as ClientListItem;
}

export function asClientDetailResponse(body: unknown) {
  return body as ClientDetailResponse;
}

export function buildClientQuery(storeId: number, filters: ClientFilters) {
  const params = new URLSearchParams({
    lojaId: String(storeId),
    pagina: String(filters.pagina),
    tamanhoPagina: String(filters.tamanhoPagina),
    ordenarPor: filters.ordenarPor,
    direcao: filters.direcao,
  });

  if (filters.nome.trim()) {
    params.set("nome", filters.nome.trim());
  }

  if (filters.contato.trim()) {
    params.set("contato", normalizeNumericValue(filters.contato));
  }

  return params.toString();
}

function toApiDateStart(value: string) {
  return `${value}T00:00:00`;
}

function toApiDateEnd(value: string) {
  return `${value}T23:59:59.999`;
}

export function buildClientDetailQuery(storeId: number, filters: ClientDetailFilters) {
  const params = new URLSearchParams({
    lojaId: String(storeId),
  });

  if (filters.dataInicial) {
    params.set("dataInicial", toApiDateStart(filters.dataInicial));
  }

  if (filters.dataFinal) {
    params.set("dataFinal", toApiDateEnd(filters.dataFinal));
  }

  if (filters.situacao.trim()) {
    params.set("situacao", filters.situacao.trim());
  }

  return params.toString();
}

export function getClientApiMessage(body: unknown): string | null {
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

export function extractClientFieldErrors(body: unknown): ClientFieldErrors {
  if (!body || typeof body !== "object" || !("errors" in body)) {
    return {};
  }

  const errors = (body as ApiErrorResponse).errors;

  if (!errors) {
    return {};
  }

  const fieldMap = Object.entries(errors).reduce<ClientFieldErrors>(
    (accumulator, [key, values]) => {
      const error = values?.[0];

      if (!error) {
        return accumulator;
      }

      const normalizedKey = key.toLowerCase();

      if (normalizedKey === "nome" && !accumulator.nome) {
        accumulator.nome = error;
      }

      if (normalizedKey === "contato" && !accumulator.contato) {
        accumulator.contato = error;
      }

      if (normalizedKey === "obs" && !accumulator.obs) {
        accumulator.obs = error;
      }

      if (normalizedKey === "userid" && !accumulator.userId) {
        accumulator.userId = error;
      }

      if (normalizedKey === "doacao" && !accumulator.doacao) {
        accumulator.doacao = error;
      }

      return accumulator;
    },
    {},
  );

  return fieldMap;
}

export function formatClientStoreLabel(store: LojaResponse | null) {
  return store ? store.nome : "Selecione uma loja";
}

export function getStoredClientTableSettings(): ClientTableSettings {
  if (typeof window === "undefined") {
    return defaultClientTableSettings;
  }

  const rawValue = window.localStorage.getItem(clientTableSettingsStorageKey);

  if (!rawValue) {
    return defaultClientTableSettings;
  }

  try {
    const parsed = JSON.parse(rawValue) as Partial<ClientTableSettings>;
    const tamanhoPagina =
      typeof parsed.tamanhoPagina === "number" &&
      Number.isInteger(parsed.tamanhoPagina) &&
      parsed.tamanhoPagina > 0 &&
      parsed.tamanhoPagina <= 100
        ? parsed.tamanhoPagina
        : defaultClientTableSettings.tamanhoPagina;

    const visibleFields = Array.isArray(parsed.visibleFields)
      ? parsed.visibleFields.filter((field): field is ClientVisibleField =>
          ["nome", "contato", "obs", "doacao", "userId", "id"].includes(String(field)),
        )
      : defaultClientTableSettings.visibleFields;

    return {
      tamanhoPagina,
      visibleFields: visibleFields.length
        ? visibleFields
        : defaultClientTableSettings.visibleFields,
    };
  } catch {
    return defaultClientTableSettings;
  }
}

export function persistClientTableSettings(settings: ClientTableSettings) {
  if (typeof window === "undefined") {
    return;
  }

  window.localStorage.setItem(clientTableSettingsStorageKey, JSON.stringify(settings));
}

export function getPreviousMonthRange() {
  const now = new Date();
  const firstDayOfCurrentMonth = new Date(now.getFullYear(), now.getMonth(), 1);
  const lastDayOfPreviousMonth = new Date(firstDayOfCurrentMonth.getTime() - 24 * 60 * 60 * 1000);
  const firstDayOfPreviousMonth = new Date(
    lastDayOfPreviousMonth.getFullYear(),
    lastDayOfPreviousMonth.getMonth(),
    1,
  );

  const toInputValue = (value: Date) => {
    const timezoneOffset = value.getTimezoneOffset() * 60_000;
    return new Date(value.getTime() - timezoneOffset).toISOString().slice(0, 10);
  };

  return {
    dataInicial: toInputValue(firstDayOfPreviousMonth),
    dataFinal: toInputValue(lastDayOfPreviousMonth),
  };
}
