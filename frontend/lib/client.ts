import type { LojaResponse } from "@/lib/store";

export type ClientListItem = {
  id: number;
  nome: string;
  contato: string;
  lojaId: number;
  userId: number | null;
};

export type ClientListResponse = {
  itens: ClientListItem[];
  pagina: number;
  tamanhoPagina: number;
  totalItens: number;
  totalPaginas: number;
};

export type ClientFormValues = {
  nome: string;
  contato: string;
  userId: string;
};

export type ClientFieldErrors = Partial<Record<keyof ClientFormValues, string>>;

export type ClientFilters = {
  nome: string;
  contato: string;
  ordenarPor: "nome" | "contato" | "id";
  direcao: "asc" | "desc";
  pagina: number;
  tamanhoPagina: number;
};

export type ClientVisibleField = "nome" | "contato" | "userId" | "id";

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

export const defaultClientTableSettings: ClientTableSettings = {
  tamanhoPagina: 10,
  visibleFields: ["nome", "contato", "userId", "id"],
};

const clientTableSettingsStorageKey = "renova.clientTableSettings";

export function asClientListResponse(body: unknown) {
  return body as ClientListResponse;
}

export function asClientResponse(body: unknown) {
  return body as ClientListItem;
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
    params.set("contato", filters.contato.trim());
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

      if (normalizedKey === "userid" && !accumulator.userId) {
        accumulator.userId = error;
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
          ["nome", "contato", "userId", "id"].includes(String(field)),
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
