import type {
  CatalogBrand,
  CatalogColor,
  CatalogProductName,
  CatalogSize,
  CatalogWorkspace,
} from "@/lib/services/catalogs";

// Tipos locais do modulo 04 usados pelos componentes da pagina.
export type CatalogEntryType = "produtoNome" | "marca" | "tamanho" | "cor";

export type CatalogEntryFormState = {
  id: string;
  nome: string;
};

export type CatalogEntryListItem = {
  id: string;
  nome: string;
};

export const catalogEntryTypeOptions: Array<{
  type: CatalogEntryType;
  label: string;
  title: string;
  subtitle: string;
}> = [
  {
    type: "produtoNome",
    label: "Produtos",
    title: "Nomes de produto",
    subtitle: "Base padrao usada no cadastro de pecas da loja ativa.",
  },
  {
    type: "marca",
    label: "Marcas",
    title: "Marcas",
    subtitle: "Marcas disponiveis para a loja ativa.",
  },
  {
    type: "tamanho",
    label: "Tamanhos",
    title: "Tamanhos",
    subtitle: "Tamanhos disponiveis para a loja ativa.",
  },
  {
    type: "cor",
    label: "Cores",
    title: "Cores",
    subtitle: "Cores disponiveis para a loja ativa.",
  },
];

export const emptyCatalogEntryForm: CatalogEntryFormState = {
  id: "",
  nome: "",
};

// Resolve os itens da lista conforme o cadastro auxiliar selecionado.
export function getCatalogEntries(
  workspace: CatalogWorkspace | undefined,
  type: CatalogEntryType,
) {
  if (!workspace) {
    return [];
  }

  switch (type) {
    case "produtoNome":
      return workspace.produtoNomes.map((entry) => ({
        id: entry.id,
        nome: entry.nome,
      }));
    case "marca":
      return workspace.marcas.map((entry) => ({
        id: entry.id,
        nome: entry.nome,
      }));
    case "tamanho":
      return workspace.tamanhos.map((entry) => ({
        id: entry.id,
        nome: entry.nome,
      }));
    case "cor":
      return workspace.cores.map((entry) => ({
        id: entry.id,
        nome: entry.nome,
      }));
  }
}

// Localiza o item bruto do detalhe para montar o formulario generico.
export function findCatalogEntry(
  workspace: CatalogWorkspace | undefined,
  type: CatalogEntryType,
  entryId: string,
): CatalogProductName | CatalogBrand | CatalogSize | CatalogColor | undefined {
  if (!workspace || !entryId) {
    return undefined;
  }

  switch (type) {
    case "produtoNome":
      return workspace.produtoNomes.find((entry) => entry.id === entryId);
    case "marca":
      return workspace.marcas.find((entry) => entry.id === entryId);
    case "tamanho":
      return workspace.tamanhos.find((entry) => entry.id === entryId);
    case "cor":
      return workspace.cores.find((entry) => entry.id === entryId);
  }
}

// Converte o item selecionado para o formulario generico do editor.
export function mapCatalogEntryToForm(
  entry: CatalogProductName | CatalogBrand | CatalogSize | CatalogColor | undefined,
): CatalogEntryFormState {
  if (!entry) {
    return emptyCatalogEntryForm;
  }

  return {
    id: entry.id,
    nome: entry.nome,
  };
}
