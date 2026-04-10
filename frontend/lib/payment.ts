export type PendingClientItem = {
  clienteId: number;
  nome: string;
  contato: string;
  credito: number;
};

export type UpdatedPendingClientItem = {
  clienteId: number;
  nome: string;
  quantidadeOrdensAtualizadas: number;
  valorAtualizado: number;
};

export type UpdatePendingResult = {
  quantidadeOrdensAtualizadas: number;
  valorTotalCredito: number;
  clientesAtualizados: UpdatedPendingClientItem[];
};

type ApiErrorResponse = {
  mensagem?: unknown;
  title?: unknown;
};

export function asPendingClientsResponse(body: unknown) {
  return body as PendingClientItem[];
}

export function asUpdatePendingResponse(body: unknown) {
  return body as UpdatePendingResult;
}

export function getPaymentApiMessage(body: unknown): string | null {
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

export function formatCurrency(value: number) {
  return new Intl.NumberFormat("pt-BR", {
    style: "currency",
    currency: "BRL",
  }).format(value);
}

export function formatPhone(value: string) {
  const digits = value.replace(/\D+/g, "");

  if (digits.length === 10) {
    return `(${digits.slice(0, 2)}) ${digits.slice(2, 6)}-${digits.slice(6)}`;
  }

  if (digits.length === 11) {
    return `(${digits.slice(0, 2)}) ${digits.slice(2, 7)}-${digits.slice(7)}`;
  }

  return value;
}

export function getTodayDateInputValue() {
  const now = new Date();
  const timezoneOffset = now.getTimezoneOffset() * 60_000;
  return new Date(now.getTime() - timezoneOffset).toISOString().slice(0, 10);
}
