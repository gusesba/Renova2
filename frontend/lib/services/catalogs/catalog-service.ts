import { callApi } from "@/lib/services/core/api-client";

import type {
  CatalogBrand,
  CatalogColor,
  CatalogProductName,
  CatalogSize,
  CatalogWorkspace,
} from "./contracts";

// Reune as operacoes HTTP do modulo de cadastros auxiliares enxutos.
export async function getCatalogWorkspace(token: string) {
  return callApi<CatalogWorkspace>("/catalogs/workspace", { method: "GET" }, token);
}

export async function createProductName(token: string, payload: { nome: string }) {
  return callApi<CatalogProductName>(
    "/catalogs/product-names",
    {
      method: "POST",
      body: JSON.stringify(payload),
    },
    token,
  );
}

export async function updateProductName(
  token: string,
  produtoNomeId: string,
  payload: { nome: string },
) {
  return callApi<CatalogProductName>(
    `/catalogs/product-names/${produtoNomeId}`,
    {
      method: "PUT",
      body: JSON.stringify(payload),
    },
    token,
  );
}

export async function createBrand(token: string, payload: { nome: string }) {
  return callApi<CatalogBrand>(
    "/catalogs/brands",
    {
      method: "POST",
      body: JSON.stringify(payload),
    },
    token,
  );
}

export async function updateBrand(
  token: string,
  marcaId: string,
  payload: { nome: string },
) {
  return callApi<CatalogBrand>(
    `/catalogs/brands/${marcaId}`,
    {
      method: "PUT",
      body: JSON.stringify(payload),
    },
    token,
  );
}

export async function createSize(token: string, payload: { nome: string }) {
  return callApi<CatalogSize>(
    "/catalogs/sizes",
    {
      method: "POST",
      body: JSON.stringify(payload),
    },
    token,
  );
}

export async function updateSize(
  token: string,
  tamanhoId: string,
  payload: { nome: string },
) {
  return callApi<CatalogSize>(
    `/catalogs/sizes/${tamanhoId}`,
    {
      method: "PUT",
      body: JSON.stringify(payload),
    },
    token,
  );
}

export async function createColor(token: string, payload: { nome: string }) {
  return callApi<CatalogColor>(
    "/catalogs/colors",
    {
      method: "POST",
      body: JSON.stringify(payload),
    },
    token,
  );
}

export async function updateColor(
  token: string,
  corId: string,
  payload: { nome: string },
) {
  return callApi<CatalogColor>(
    `/catalogs/colors/${corId}`,
    {
      method: "PUT",
      body: JSON.stringify(payload),
    },
    token,
  );
}
