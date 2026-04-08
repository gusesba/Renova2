export type ProductListItem = {
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
};

export type ProductCreateResponse = {
  id: number;
  preco: number;
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
};

export type ProductFormValues = {
  descricao: string;
  preco: string;
  entrada: string;
  situacao: string;
  consignado: boolean;
  produtoId: string;
  produtoLabel: string;
  marcaId: string;
  marcaLabel: string;
  tamanhoId: string;
  tamanhoLabel: string;
  corId: string;
  corLabel: string;
  fornecedorId: string;
  fornecedorLabel: string;
};

export type ProductFieldErrors = Partial<
  Record<
    | "descricao"
    | "preco"
    | "entrada"
    | "situacao"
    | "produtoId"
    | "marcaId"
    | "tamanhoId"
    | "corId"
    | "fornecedorId",
    string
  >
>;

export type ProductLookupOption = {
  id: number;
  label: string;
};

export type ProductListResponse = {
  itens: ProductListItem[];
  pagina: number;
  tamanhoPagina: number;
  totalItens: number;
  totalPaginas: number;
};

export type ProductFilters = {
  descricao: string;
  produto: string;
  marca: string;
  tamanho: string;
  cor: string;
  fornecedor: string;
  precoInicial: string;
  precoFinal: string;
  dataInicial: string;
  dataFinal: string;
  ordenarPor:
    | "descricao"
    | "produto"
    | "marca"
    | "tamanho"
    | "cor"
    | "fornecedor"
    | "preco"
    | "entrada"
    | "id";
  direcao: "asc" | "desc";
  pagina: number;
  tamanhoPagina: number;
};

export type ProductVisibleField =
  | "produto"
  | "descricao"
  | "marca"
  | "tamanho"
  | "cor"
  | "fornecedor"
  | "preco"
  | "entrada"
  | "situacao"
  | "consignado"
  | "id";

export type ProductTableSettings = {
  tamanhoPagina: number;
  visibleFields: ProductVisibleField[];
};

type ApiErrorResponse = {
  mensagem?: unknown;
  title?: unknown;
  errors?: Record<string, string[] | undefined>;
};

export const initialProductFilters: ProductFilters = {
  descricao: "",
  produto: "",
  marca: "",
  tamanho: "",
  cor: "",
  fornecedor: "",
  precoInicial: "",
  precoFinal: "",
  dataInicial: "",
  dataFinal: "",
  ordenarPor: "descricao",
  direcao: "asc",
  pagina: 1,
  tamanhoPagina: 10,
};

export const initialProductFormValues: ProductFormValues = {
  descricao: "",
  preco: "",
  entrada: new Date().toISOString().slice(0, 10),
  situacao: "1",
  consignado: true,
  produtoId: "",
  produtoLabel: "",
  marcaId: "",
  marcaLabel: "",
  tamanhoId: "",
  tamanhoLabel: "",
  corId: "",
  corLabel: "",
  fornecedorId: "",
  fornecedorLabel: "",
};

export const defaultProductTableSettings: ProductTableSettings = {
  tamanhoPagina: 10,
  visibleFields: [
    "produto",
    "descricao",
    "marca",
    "fornecedor",
    "preco",
    "entrada",
    "situacao",
    "consignado",
    "id",
  ],
};

const productTableSettingsStorageKey = "renova.productTableSettings";

export function asProductListResponse(body: unknown) {
  return body as ProductListResponse;
}

export function asProductResponse(body: unknown) {
  return body as ProductCreateResponse;
}

export function normalizeDecimalValue(value: string) {
  return value.replace(",", ".").trim();
}

export function formatCurrencyValue(value: number) {
  return new Intl.NumberFormat("pt-BR", {
    style: "currency",
    currency: "BRL",
  }).format(value);
}

export function formatDateValue(value: string) {
  const parsed = new Date(value);

  if (Number.isNaN(parsed.getTime())) {
    return value;
  }

  return new Intl.DateTimeFormat("pt-BR", {
    dateStyle: "short",
  }).format(parsed);
}

export function formatSituacaoValue(value: number) {
  const labels = Object.fromEntries(
    productSituacaoOptions.map((option) => [option.value, option.label]),
  ) as Record<number, string>;

  return labels[value] ?? `Situacao ${value}`;
}

export const productSituacaoOptions = [
  { value: 1, label: "Estoque" },
  { value: 2, label: "Vendido" },
  { value: 3, label: "Devolvido" },
  { value: 4, label: "Emprestado" },
  { value: 5, label: "Doado" },
  { value: 6, label: "Perdido" },
] as const;

function toApiDateStart(value: string) {
  return `${value}T00:00:00`;
}

function toApiDateEnd(value: string) {
  return `${value}T23:59:59.999`;
}

export function buildProductQuery(storeId: number, filters: ProductFilters) {
  const params = new URLSearchParams({
    lojaId: String(storeId),
    pagina: String(filters.pagina),
    tamanhoPagina: String(filters.tamanhoPagina),
    ordenarPor: filters.ordenarPor,
    direcao: filters.direcao,
  });

  const textFields: Array<keyof Pick<
    ProductFilters,
    "descricao" | "produto" | "marca" | "tamanho" | "cor" | "fornecedor"
  >> = ["descricao", "produto", "marca", "tamanho", "cor", "fornecedor"];

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

  if (filters.dataInicial) {
    params.set("dataInicial", toApiDateStart(filters.dataInicial));
  }

  if (filters.dataFinal) {
    params.set("dataFinal", toApiDateEnd(filters.dataFinal));
  }

  return params.toString();
}

export function getProductApiMessage(body: unknown): string | null {
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

export function extractProductFieldErrors(body: unknown): ProductFieldErrors {
  if (!body || typeof body !== "object" || !("errors" in body)) {
    return {};
  }

  const errors = (body as ApiErrorResponse).errors;

  if (!errors) {
    return {};
  }

  return Object.entries(errors).reduce<ProductFieldErrors>((accumulator, [key, values]) => {
    const error = values?.[0];

    if (!error) {
      return accumulator;
    }

    const normalizedKey = key.toLowerCase();

    if (normalizedKey === "descricao" && !accumulator.descricao) {
      accumulator.descricao = error;
    }

    if (normalizedKey === "preco" && !accumulator.preco) {
      accumulator.preco = error;
    }

    if (normalizedKey === "entrada" && !accumulator.entrada) {
      accumulator.entrada = error;
    }

    if (normalizedKey === "situacao" && !accumulator.situacao) {
      accumulator.situacao = error;
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

    if (normalizedKey === "fornecedorid" && !accumulator.fornecedorId) {
      accumulator.fornecedorId = error;
    }

    return accumulator;
  }, {});
}

export function getStoredProductTableSettings(): ProductTableSettings {
  if (typeof window === "undefined") {
    return defaultProductTableSettings;
  }

  const rawValue = window.localStorage.getItem(productTableSettingsStorageKey);

  if (!rawValue) {
    return defaultProductTableSettings;
  }

  try {
    const parsed = JSON.parse(rawValue) as Partial<ProductTableSettings>;
    const tamanhoPagina =
      typeof parsed.tamanhoPagina === "number" &&
      Number.isInteger(parsed.tamanhoPagina) &&
      parsed.tamanhoPagina > 0 &&
      parsed.tamanhoPagina <= 100
        ? parsed.tamanhoPagina
        : defaultProductTableSettings.tamanhoPagina;

    const visibleFields = Array.isArray(parsed.visibleFields)
      ? parsed.visibleFields.filter((field): field is ProductVisibleField =>
          [
            "produto",
            "descricao",
            "marca",
            "tamanho",
            "cor",
            "fornecedor",
            "preco",
            "entrada",
            "situacao",
            "consignado",
            "id",
          ].includes(String(field)),
        )
      : defaultProductTableSettings.visibleFields;

    return {
      tamanhoPagina,
      visibleFields: visibleFields.length
        ? visibleFields
        : defaultProductTableSettings.visibleFields,
    };
  } catch {
    return defaultProductTableSettings;
  }
}

export function persistProductTableSettings(settings: ProductTableSettings) {
  if (typeof window === "undefined") {
    return;
  }

  window.localStorage.setItem(productTableSettingsStorageKey, JSON.stringify(settings));
}
