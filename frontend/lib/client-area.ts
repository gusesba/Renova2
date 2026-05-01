import type { ProductListItem } from "@/lib/product";
import { getClientApiMessage } from "@/lib/client";
import { getMyClientProducts, getMyCustomerProducts } from "@/services/client-service";

export type ClientAreaScope = "fornecedor" | "cliente";

export type ClientAreaProductItem = ProductListItem & {
  storeName: string;
};

export type ClientAreaListResponse = {
  itens: ClientAreaProductItem[];
  pagina: number;
  tamanhoPagina: number;
  totalItens: number;
  totalPaginas: number;
};

export type ClientAreaFilters = {
  loja: string;
  produto: string;
  descricao: string;
  marca: string;
  tamanho: string;
  cor: string;
  precoInicial: string;
  precoFinal: string;
  dataInicial: string;
  dataFinal: string;
  ordenarPor:
    | "loja"
    | "produto"
    | "descricao"
    | "marca"
    | "tamanho"
    | "cor"
    | "preco"
    | "entrada"
    | "situacao"
    | "id";
  direcao: "asc" | "desc";
  pagina: number;
  tamanhoPagina: number;
};

export type ClientAreaVisibleField =
  | "loja"
  | "produto"
  | "descricao"
  | "marca"
  | "tamanho"
  | "cor"
  | "preco"
  | "entrada"
  | "situacao"
  | "id";

export type ClientAreaTableSettings = {
  tamanhoPagina: number;
  visibleFields: ClientAreaVisibleField[];
};

type ClientAreaInventoryApiItem = ProductListItem & {
  lojaNome?: string;
  LojaNome?: string;
};

type ClientAreaInventoryApiResponse = {
  itens: ClientAreaInventoryApiItem[];
  pagina: number;
  tamanhoPagina: number;
  totalItens: number;
  totalPaginas: number;
};

const clientAreaTableSettingsStorageKeys: Record<ClientAreaScope, string> = {
  fornecedor: "renova.clientAreaTableSettings",
  cliente: "renova.clientAreaCustomerTableSettings",
};

export const initialClientAreaFilters: ClientAreaFilters = {
  loja: "",
  produto: "",
  descricao: "",
  marca: "",
  tamanho: "",
  cor: "",
  precoInicial: "",
  precoFinal: "",
  dataInicial: "",
  dataFinal: "",
  ordenarPor: "id",
  direcao: "desc",
  pagina: 1,
  tamanhoPagina: 10,
};

export const defaultClientAreaTableSettings: ClientAreaTableSettings = {
  tamanhoPagina: 10,
  visibleFields: [
    "loja",
    "produto",
    "descricao",
    "marca",
    "tamanho",
    "cor",
    "preco",
    "entrada",
    "situacao",
    "id",
  ],
};

export function buildClientAreaQuery(filters: ClientAreaFilters) {
  const params = new URLSearchParams({
    pagina: String(filters.pagina),
    tamanhoPagina: String(filters.tamanhoPagina),
    ordenarPor: filters.ordenarPor,
    direcao: filters.direcao,
  });

  const textFields: Array<keyof Pick<
    ClientAreaFilters,
    "loja" | "produto" | "descricao" | "marca" | "tamanho" | "cor"
  >> = ["loja", "produto", "descricao", "marca", "tamanho", "cor"];

  for (const field of textFields) {
    if (filters[field].trim()) {
      params.set(field, filters[field].trim());
    }
  }

  if (filters.precoInicial.trim()) {
    params.set("precoInicial", filters.precoInicial.replace(",", ".").trim());
  }

  if (filters.precoFinal.trim()) {
    params.set("precoFinal", filters.precoFinal.replace(",", ".").trim());
  }

  if (filters.dataInicial) {
    params.set("dataInicial", `${filters.dataInicial}T00:00:00`);
  }

  if (filters.dataFinal) {
    params.set("dataFinal", `${filters.dataFinal}T23:59:59.999`);
  }

  return params.toString();
}

export function asClientAreaInventoryResponse(body: unknown): ClientAreaListResponse {
  const response = body as ClientAreaInventoryApiResponse;

  return {
    itens: (response.itens ?? []).map<ClientAreaProductItem>((item) => ({
      ...item,
      storeName: item.lojaNome ?? item.LojaNome ?? `Loja ${item.lojaId}`,
    })),
    pagina: response.pagina ?? 1,
    tamanhoPagina: response.tamanhoPagina ?? defaultClientAreaTableSettings.tamanhoPagina,
    totalItens: response.totalItens ?? 0,
    totalPaginas: response.totalPaginas ?? 0,
  };
}

export async function getClientAreaInventory(
  token: string,
  filters: ClientAreaFilters,
  scope: ClientAreaScope = "fornecedor",
) {
  const response =
    scope === "cliente"
      ? await getMyCustomerProducts(token, filters)
      : await getMyClientProducts(token, filters);

  if (!response.ok) {
    throw new Error(
      getClientApiMessage(response.body) ?? "Nao foi possivel carregar as pecas do cliente.",
    );
  }

  return asClientAreaInventoryResponse(response.body);
}

export function getStoredClientAreaTableSettings(
  scope: ClientAreaScope = "fornecedor",
): ClientAreaTableSettings {
  if (typeof window === "undefined") {
    return defaultClientAreaTableSettings;
  }

  const rawValue = window.localStorage.getItem(clientAreaTableSettingsStorageKeys[scope]);

  if (!rawValue) {
    return defaultClientAreaTableSettings;
  }

  try {
    const parsed = JSON.parse(rawValue) as Partial<ClientAreaTableSettings>;
    const tamanhoPagina =
      typeof parsed.tamanhoPagina === "number" &&
      Number.isInteger(parsed.tamanhoPagina) &&
      parsed.tamanhoPagina > 0 &&
      parsed.tamanhoPagina <= 100
        ? parsed.tamanhoPagina
        : defaultClientAreaTableSettings.tamanhoPagina;

    const visibleFields = Array.isArray(parsed.visibleFields)
      ? parsed.visibleFields.filter((field): field is ClientAreaVisibleField =>
          [
            "loja",
            "produto",
            "descricao",
            "marca",
            "tamanho",
            "cor",
            "preco",
            "entrada",
            "situacao",
            "id",
          ].includes(String(field)),
        )
      : defaultClientAreaTableSettings.visibleFields;

    return {
      tamanhoPagina,
      visibleFields: visibleFields.length
        ? visibleFields
        : defaultClientAreaTableSettings.visibleFields,
    };
  } catch {
    return defaultClientAreaTableSettings;
  }
}

export function persistClientAreaTableSettings(
  settings: ClientAreaTableSettings,
  scope: ClientAreaScope = "fornecedor",
) {
  if (typeof window === "undefined") {
    return;
  }

  window.localStorage.setItem(clientAreaTableSettingsStorageKeys[scope], JSON.stringify(settings));
}
