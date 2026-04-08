import { formatSituacaoValue, type ProductListItem } from "@/lib/product";

type ApiErrorResponse = {
  mensagem?: unknown;
  title?: unknown;
  errors?: Record<string, string[] | undefined>;
};

export type MovementTypeValue = 1 | 2 | 3 | 4 | 5 | 6;

export type MovementDraftFormValues = {
  tipo: string;
  data: string;
  clienteId: string;
};

export type MovementFieldErrors = Partial<Record<keyof MovementDraftFormValues | "produtoIds", string>>;

export type MovementCreateResponse = {
  id: number;
  tipo: MovementTypeValue;
  data: string;
  clienteId: number;
  lojaId: number;
  produtoIds: number[];
};

export type MovementSuggestion = {
  message: string;
  product: ProductListItem;
  suggestedType: MovementTypeValue | null;
};

export const movementTypeOptions: Array<{ label: string; value: MovementTypeValue }> = [
  { value: 1, label: "Venda" },
  { value: 2, label: "Emprestimo" },
  { value: 3, label: "Doacao" },
  { value: 4, label: "Devolucao dono" },
  { value: 5, label: "Devolucao venda" },
  { value: 6, label: "Devolucao emprestimo" },
];

export const initialMovementDraftFormValues: MovementDraftFormValues = {
  tipo: "1",
  data: new Date().toISOString().slice(0, 10),
  clienteId: "",
};

export function asMovementResponse(body: unknown) {
  return body as MovementCreateResponse;
}

export function formatMovementType(value: number) {
  return movementTypeOptions.find((option) => option.value === value)?.label ?? `Tipo ${value}`;
}

export function getMovementApiMessage(body: unknown): string | null {
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

export function isProductSituationCompatible(movementType: number, productSituation: number) {
  if (movementType === 5) {
    return productSituation === 2;
  }

  if (movementType === 6) {
    return productSituation === 4;
  }

  return productSituation === 1;
}

export function getSuggestedMovementType(
  currentMovementType: number,
  productSituation: number,
): MovementTypeValue | null {
  if (productSituation === 2) {
    return 5;
  }

  if (productSituation === 4) {
    return 6;
  }

  if (productSituation === 1 && currentMovementType === 5) {
    return 1;
  }

  if (productSituation === 1 && currentMovementType === 6) {
    return 2;
  }

  return null;
}

export function buildMovementSuggestion(
  currentMovementType: number,
  product: ProductListItem,
): MovementSuggestion {
  const suggestedType = getSuggestedMovementType(currentMovementType, product.situacao);
  const productSituation = formatSituacaoValue(product.situacao);
  const currentType = formatMovementType(currentMovementType);

  if (suggestedType) {
    return {
      suggestedType,
      product,
      message: `O produto ${product.id} esta em ${productSituation}, o que nao combina com ${currentType}. Deseja abrir uma nova movimentacao de ${formatMovementType(suggestedType)} e adicionar esse produto nela?`,
    };
  }

  return {
    suggestedType: null,
    product,
    message: `O produto ${product.id} esta em ${productSituation} e nao pode ser usado em ${currentType}. Escolha outro produto ou ajuste o tipo desta movimentacao.`,
  };
}

