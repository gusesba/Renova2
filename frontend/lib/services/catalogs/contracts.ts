// Tipos compartilhados do modulo 04 para os services e telas do frontend.
export type CatalogProductName = {
  id: string;
  nome: string;
};

export type CatalogBrand = {
  id: string;
  nome: string;
};

export type CatalogSize = {
  id: string;
  nome: string;
};

export type CatalogColor = {
  id: string;
  nome: string;
};

export type CatalogWorkspace = {
  lojaAtivaId: string;
  lojaAtivaNome: string;
  produtoNomes: CatalogProductName[];
  marcas: CatalogBrand[];
  tamanhos: CatalogSize[];
  cores: CatalogColor[];
};
