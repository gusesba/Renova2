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

  const fieldMap = Object.entries(errors).reduce<ClientFieldErrors>((accumulator, [key, values]) => {
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
  }, {});

  return fieldMap;
}

export function formatClientStoreLabel(store: LojaResponse | null) {
  return store ? store.nome : "Selecione uma loja";
}
