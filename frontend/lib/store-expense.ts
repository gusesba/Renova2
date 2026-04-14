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
