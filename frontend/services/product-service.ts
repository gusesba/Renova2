import { buildProductQuery, type ProductFilters, type ProductLookupOption } from "@/lib/product";

const apiBaseUrl = process.env.NEXT_PUBLIC_API_URL?.replace(/\/$/, "") ?? "http://localhost:5268";

type LookupResponse<TItem> = {
  itens: TItem[];
};

type ProductAuxiliarResponse = {
  id: number;
  valor: string;
};

type SupplierResponse = {
  id: number;
  nome: string;
};

export async function getProducts(
  token: string,
  storeId: number,
  filters: ProductFilters,
): Promise<{ body: unknown; ok: boolean; status: number }> {
  const response = await fetch(`${apiBaseUrl}/api/produto?${buildProductQuery(storeId, filters)}`, {
    method: "GET",
    headers: {
      Authorization: `Bearer ${token}`,
    },
  });

  const contentType = response.headers.get("content-type") ?? "";
  const body = contentType.includes("application/json")
    ? ((await response.json()) as unknown)
    : null;

  return {
    body,
    ok: response.ok,
    status: response.status,
  };
}

export async function getProductById(
  productId: number,
  token: string,
): Promise<{ body: unknown; ok: boolean; status: number }> {
  const response = await fetch(`${apiBaseUrl}/api/produto/${productId}`, {
    method: "GET",
    headers: {
      Authorization: `Bearer ${token}`,
    },
  });

  const contentType = response.headers.get("content-type") ?? "";
  const body = contentType.includes("application/json")
    ? ((await response.json()) as unknown)
    : null;

  return {
    body,
    ok: response.ok,
    status: response.status,
  };
}

export async function createProduct(
  payload: {
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
  },
  token: string,
): Promise<{ body: unknown; ok: boolean; status: number }> {
  const response = await fetch(`${apiBaseUrl}/api/produto`, {
    method: "POST",
    headers: {
      Authorization: `Bearer ${token}`,
      "Content-Type": "application/json",
    },
    body: JSON.stringify(payload),
  });

  const contentType = response.headers.get("content-type") ?? "";
  const body = contentType.includes("application/json")
    ? ((await response.json()) as unknown)
    : null;

  return {
    body,
    ok: response.ok,
    status: response.status,
  };
}

export async function updateProduct(
  productId: number,
  payload: {
    preco: number;
    produtoId: number;
    marcaId: number;
    tamanhoId: number;
    corId: number;
    fornecedorId: number;
    descricao: string;
    entrada: string;
    situacao: number;
    consignado: boolean;
  },
  token: string,
): Promise<{ body: unknown; ok: boolean; status: number }> {
  const response = await fetch(`${apiBaseUrl}/api/produto/${productId}`, {
    method: "PUT",
    headers: {
      Authorization: `Bearer ${token}`,
      "Content-Type": "application/json",
    },
    body: JSON.stringify(payload),
  });

  const contentType = response.headers.get("content-type") ?? "";
  const body = contentType.includes("application/json")
    ? ((await response.json()) as unknown)
    : null;

  return {
    body,
    ok: response.ok,
    status: response.status,
  };
}

export async function deleteProduct(
  productId: number,
  token: string,
): Promise<{ body: unknown; ok: boolean; status: number }> {
  const response = await fetch(`${apiBaseUrl}/api/produto/${productId}`, {
    method: "DELETE",
    headers: {
      Authorization: `Bearer ${token}`,
    },
  });

  const contentType = response.headers.get("content-type") ?? "";
  const body = contentType.includes("application/json")
    ? ((await response.json()) as unknown)
    : null;

  return {
    body,
    ok: response.ok,
    status: response.status,
  };
}

async function createProductAuxiliar(
  path: "referencia" | "marca" | "tamanho" | "cor",
  payload: { valor: string; lojaId: number },
  token: string,
): Promise<{ body: unknown; ok: boolean; status: number }> {
  const response = await fetch(`${apiBaseUrl}/api/produto/${path}`, {
    method: "POST",
    headers: {
      Authorization: `Bearer ${token}`,
      "Content-Type": "application/json",
    },
    body: JSON.stringify(payload),
  });

  const contentType = response.headers.get("content-type") ?? "";
  const body = contentType.includes("application/json")
    ? ((await response.json()) as unknown)
    : null;

  return {
    body,
    ok: response.ok,
    status: response.status,
  };
}

export function createProductReference(
  payload: { valor: string; lojaId: number },
  token: string,
) {
  return createProductAuxiliar("referencia", payload, token);
}

export function createProductBrand(payload: { valor: string; lojaId: number }, token: string) {
  return createProductAuxiliar("marca", payload, token);
}

export function createProductSize(payload: { valor: string; lojaId: number }, token: string) {
  return createProductAuxiliar("tamanho", payload, token);
}

export function createProductColor(payload: { valor: string; lojaId: number }, token: string) {
  return createProductAuxiliar("cor", payload, token);
}

async function getLookupOptions<TItem>({
  token,
  url,
  mapper,
}: {
  token: string;
  url: string;
  mapper: (item: TItem) => ProductLookupOption;
}): Promise<{ body: ProductLookupOption[]; ok: boolean; status: number }> {
  const response = await fetch(url, {
    method: "GET",
    headers: {
      Authorization: `Bearer ${token}`,
    },
  });

  const contentType = response.headers.get("content-type") ?? "";
  const body = contentType.includes("application/json")
    ? ((await response.json()) as unknown)
    : null;

  if (!response.ok) {
    return {
      body: [],
      ok: false,
      status: response.status,
    };
  }

  const data = body as LookupResponse<TItem>;

  return {
    body: data.itens.map(mapper),
    ok: true,
    status: response.status,
  };
}

function buildAuxiliarLookupUrl(
  path: "referencia" | "marca" | "tamanho" | "cor",
  storeId: number,
  search: string,
) {
  const params = new URLSearchParams({
    lojaId: String(storeId),
    pagina: "1",
    tamanhoPagina: "20",
    ordenarPor: "valor",
    direcao: "asc",
  });

  if (search.trim()) {
    params.set("valor", search.trim());
  }

  return `${apiBaseUrl}/api/produto/${path}?${params.toString()}`;
}

export function getProductReferenceOptions(token: string, storeId: number, search: string) {
  return getLookupOptions<ProductAuxiliarResponse>({
    token,
    url: buildAuxiliarLookupUrl("referencia", storeId, search),
    mapper: (item) => ({
      id: item.id,
      label: item.valor,
    }),
  });
}

export function getProductBrandOptions(token: string, storeId: number, search: string) {
  return getLookupOptions<ProductAuxiliarResponse>({
    token,
    url: buildAuxiliarLookupUrl("marca", storeId, search),
    mapper: (item) => ({
      id: item.id,
      label: item.valor,
    }),
  });
}

export function getProductSizeOptions(token: string, storeId: number, search: string) {
  return getLookupOptions<ProductAuxiliarResponse>({
    token,
    url: buildAuxiliarLookupUrl("tamanho", storeId, search),
    mapper: (item) => ({
      id: item.id,
      label: item.valor,
    }),
  });
}

export function getProductColorOptions(token: string, storeId: number, search: string) {
  return getLookupOptions<ProductAuxiliarResponse>({
    token,
    url: buildAuxiliarLookupUrl("cor", storeId, search),
    mapper: (item) => ({
      id: item.id,
      label: item.valor,
    }),
  });
}

export function getProductSupplierOptions(token: string, storeId: number, search: string) {
  const params = new URLSearchParams({
    lojaId: String(storeId),
    pagina: "1",
    tamanhoPagina: "20",
    ordenarPor: "nome",
    direcao: "asc",
  });

  if (search.trim()) {
    params.set("nome", search.trim());
  }

  return getLookupOptions<SupplierResponse>({
    token,
    url: `${apiBaseUrl}/api/cliente?${params.toString()}`,
    mapper: (item) => ({
      id: item.id,
      label: item.nome,
    }),
  });
}
