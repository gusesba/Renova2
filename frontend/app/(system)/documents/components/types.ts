import type {
  DocumentSearchItem,
  DocumentTypeCode,
  DocumentTypeOption,
} from "@/lib/services/documents";

export type DocumentQueryState = {
  tipoDocumento: DocumentTypeCode | "";
  search: string;
};

// Cria o estado inicial do modulo 16.
export function createDefaultDocumentQuery(): DocumentQueryState {
  return {
    tipoDocumento: "",
    search: "",
  };
}

// Resolve o titulo exibido no painel a partir do codigo selecionado.
export function getSelectedDocumentType(
  items: DocumentTypeOption[] | undefined,
  type: string,
) {
  return items?.find((item) => item.codigo === type) ?? null;
}

// Mantem a selecao atual coerente com a lista carregada.
export function resolveSelectedDocument(
  currentId: string | null,
  items: DocumentSearchItem[] | undefined,
) {
  if (!items?.length) {
    return null;
  }

  return items.find((item) => item.id === currentId) ?? items[0];
}
