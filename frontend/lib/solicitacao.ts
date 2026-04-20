import { normalizeDecimalValue, type ProductLookupOption } from "@/lib/product";

export type SolicitacaoMatchedProduct = {
  id: number;
  produto: string;
  marca: string;
  tamanho: string;
  cor: string;
  fornecedor: string;
  descricao: string;
  preco: number;
};

export type SolicitacaoListItem = {
  id: number;
  produtoId: number | null;
  produto: string;
  marcaId: number | null;
  marca: string;
  tamanhoId: number | null;
  tamanho: string;
  corId: number | null;
  cor: string;
  clienteId: number | null;
  cliente: string;
  descricao: string;
  precoMinimo: number | null;
  precoMaximo: number | null;
  lojaId: number;
  produtosCompativeis: SolicitacaoMatchedProduct[];
};

export type SolicitacaoListResponse = {
  itens: SolicitacaoListItem[];
  pagina: number;
  tamanhoPagina: number;
  totalItens: number;
  totalPaginas: number;
};

export type SolicitacaoCreateResponse = {
  id: number;
  produtoId: number | null;
  marcaId: number | null;
  tamanhoId: number | null;
  corId: number | null;
  clienteId: number | null;
  descricao: string;
  precoMinimo: number | null;
  precoMaximo: number | null;
  lojaId: number;
  produtosCompativeis: SolicitacaoMatchedProduct[];
};

export type SolicitacaoFormValues = {
  descricao: string;
  precoMaximo: string;
  produtoId: string;
  produtoLabel: string;
  marcaId: string;
  marcaLabel: string;
  tamanhoId: string;
  tamanhoLabel: string;
  corId: string;
  corLabel: string;
  clienteId: string;
  clienteLabel: string;
};

export type SolicitacaoFieldErrors = Partial<
  Record<
    | "descricao"
    | "precoMaximo"
    | "produtoId"
    | "marcaId"
    | "tamanhoId"
    | "corId"
    | "clienteId",
    string
  >
>;

export type SolicitacaoFilters = {
  descricao: string;
  produto: string;
  marca: string;
  tamanho: string;
  cor: string;
  cliente: string;
  precoInicial: string;
  precoFinal: string;
  ordenarPor:
    | "descricao"
    | "produto"
    | "marca"
    | "tamanho"
    | "cor"
    | "cliente"
    | "precoMaximo"
    | "id";
  direcao: "asc" | "desc";
  pagina: number;
  tamanhoPagina: number;
};

export type SolicitacaoVisibleField =
  | "produto"
  | "descricao"
  | "marca"
  | "tamanho"
  | "cor"
  | "cliente"
  | "precoMaximo"
  | "matches"
  | "id";

export type SolicitacaoTableSettings = {
  tamanhoPagina: number;
  visibleFields: SolicitacaoVisibleField[];
};

type ApiErrorResponse = {
  mensagem?: unknown;
  title?: unknown;
  errors?: Record<string, string[] | undefined>;
};

export const initialSolicitacaoFilters: SolicitacaoFilters = {
  descricao: "",
  produto: "",
  marca: "",
  tamanho: "",
  cor: "",
  cliente: "",
  precoInicial: "",
  precoFinal: "",
  ordenarPor: "descricao",
  direcao: "asc",
  pagina: 1,
  tamanhoPagina: 10,
};

export const initialSolicitacaoFormValues: SolicitacaoFormValues = {
  descricao: "",
  precoMaximo: "",
  produtoId: "",
  produtoLabel: "",
  marcaId: "",
  marcaLabel: "",
  tamanhoId: "",
  tamanhoLabel: "",
  corId: "",
  corLabel: "",
  clienteId: "",
  clienteLabel: "",
};

export const defaultSolicitacaoTableSettings: SolicitacaoTableSettings = {
  tamanhoPagina: 10,
  visibleFields: [
    "produto",
    "descricao",
    "marca",
    "tamanho",
    "cor",
    "cliente",
    "precoMaximo",
    "matches",
    "id",
  ],
};

const solicitacaoTableSettingsStorageKey = "renova.solicitacaoTableSettings";

export function asSolicitacaoListResponse(body: unknown) {
  return body as SolicitacaoListResponse;
}

export function asSolicitacaoResponse(body: unknown) {
  return body as SolicitacaoCreateResponse;
}

export function buildSolicitacaoQuery(storeId: number, filters: SolicitacaoFilters) {
  const params = new URLSearchParams({
    lojaId: String(storeId),
    pagina: String(filters.pagina),
    tamanhoPagina: String(filters.tamanhoPagina),
    ordenarPor: filters.ordenarPor,
    direcao: filters.direcao,
  });

  const textFields: Array<keyof Pick<
    SolicitacaoFilters,
    "descricao" | "produto" | "marca" | "tamanho" | "cor" | "cliente"
  >> = ["descricao", "produto", "marca", "tamanho", "cor", "cliente"];

  for (const field of textFields) {
    if (filters[field].trim()) {
      params.set(field, filters[field].trim());
    }
  }

  if (filters.precoInicial.trim()) {
    params.set("precoInicial", normalizeDecimalValue(filters.precoInicial));
  }

  if (filters.precoFinal.trim()) {
    params.set("precoFinal", normalizeDecimalValue(filters.precoFinal));
  }

  return params.toString();
}

export function formatSolicitacaoPriceRange(minimo: number, maximo: number) {
  return `${formatCurrencyValue(minimo)} - ${formatCurrencyValue(maximo)}`;
}

export function formatCurrencyValue(value: number) {
  return new Intl.NumberFormat("pt-BR", {
    style: "currency",
    currency: "BRL",
  }).format(value);
}

export function getSolicitacaoApiMessage(body: unknown): string | null {
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

export function extractSolicitacaoFieldErrors(body: unknown): SolicitacaoFieldErrors {
  if (!body || typeof body !== "object" || !("errors" in body)) {
    return {};
  }

  const errors = (body as ApiErrorResponse).errors;

  if (!errors) {
    return {};
  }

  return Object.entries(errors).reduce<SolicitacaoFieldErrors>((accumulator, [key, values]) => {
    const error = values?.[0];

    if (!error) {
      return accumulator;
    }

    const normalizedKey = key.toLowerCase();

    if (normalizedKey === "descricao" && !accumulator.descricao) {
      accumulator.descricao = error;
    }

    if (normalizedKey === "precomaximo" && !accumulator.precoMaximo) {
      accumulator.precoMaximo = error;
    }

    if (normalizedKey === "produtoid" && !accumulator.produtoId) {
      accumulator.produtoId = error;
    }

    if (normalizedKey === "marcaid" && !accumulator.marcaId) {
      accumulator.marcaId = error;
    }

    if (normalizedKey === "tamanhoid" && !accumulator.tamanhoId) {
      accumulator.tamanhoId = error;
    }

    if (normalizedKey === "corid" && !accumulator.corId) {
      accumulator.corId = error;
    }

    if (normalizedKey === "clienteid" && !accumulator.clienteId) {
      accumulator.clienteId = error;
    }

    return accumulator;
  }, {});
}

export function getStoredSolicitacaoTableSettings(): SolicitacaoTableSettings {
  if (typeof window === "undefined") {
    return defaultSolicitacaoTableSettings;
  }

  const rawValue = window.localStorage.getItem(solicitacaoTableSettingsStorageKey);

  if (!rawValue) {
    return defaultSolicitacaoTableSettings;
  }

  try {
    const parsed = JSON.parse(rawValue) as Partial<SolicitacaoTableSettings>;
    const tamanhoPagina =
      typeof parsed.tamanhoPagina === "number" &&
      Number.isInteger(parsed.tamanhoPagina) &&
      parsed.tamanhoPagina > 0 &&
      parsed.tamanhoPagina <= 100
        ? parsed.tamanhoPagina
        : defaultSolicitacaoTableSettings.tamanhoPagina;

    const visibleFields = Array.isArray(parsed.visibleFields)
      ? parsed.visibleFields.filter((field): field is SolicitacaoVisibleField =>
          [
            "produto",
            "descricao",
            "marca",
            "tamanho",
            "cor",
            "cliente",
            "precoMaximo",
            "matches",
            "id",
          ].includes(String(field)),
        )
      : defaultSolicitacaoTableSettings.visibleFields;

    return {
      tamanhoPagina,
      visibleFields: visibleFields.length
        ? visibleFields
        : defaultSolicitacaoTableSettings.visibleFields,
    };
  } catch {
    return defaultSolicitacaoTableSettings;
  }
}

export function persistSolicitacaoTableSettings(settings: SolicitacaoTableSettings) {
  if (typeof window === "undefined") {
    return;
  }

  window.localStorage.setItem(solicitacaoTableSettingsStorageKey, JSON.stringify(settings));
}

export function mapLookupOption(option: ProductLookupOption) {
  return {
    id: option.id,
    label: option.label,
  };
}
