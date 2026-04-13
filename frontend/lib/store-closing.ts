export type StoreClosingMonthItem = {
  ano: number;
  mes: number;
  inicioPeriodo: string;
  quantidadePecasVendidas: number;
  valorRecebidoClientes: number;
  valorPagoFornecedores: number;
  total: number;
};

export type StoreClosingResponse = {
  dataReferencia: string;
  inicioPeriodo: string;
  fimPeriodo: string;
  quantidadePecasVendidas: number;
  valorRecebidoClientes: number;
  valorPagoFornecedores: number;
  total: number;
  historico: StoreClosingMonthItem[];
};

type ApiErrorResponse = {
  mensagem?: unknown;
  title?: unknown;
};

export function asStoreClosingResponse(body: unknown) {
  return body as StoreClosingResponse;
}

export function getStoreClosingApiMessage(body: unknown): string | null {
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

export function getPreviousMonthInputValue() {
  const now = new Date();
  const previousMonth = new Date(now.getFullYear(), now.getMonth() - 1, 1);
  const timezoneOffset = previousMonth.getTimezoneOffset() * 60_000;
  return new Date(previousMonth.getTime() - timezoneOffset).toISOString().slice(0, 7);
}

export function buildStoreClosingQuery(storeId: number, referenceMonth: string) {
  const params = new URLSearchParams({
    lojaId: String(storeId),
    dataReferencia: `${referenceMonth}-01T00:00:00`,
  });

  return params.toString();
}

export function formatClosingMonthLabel(value: string) {
  const parsed = new Date(`${value}T00:00:00`);

  if (Number.isNaN(parsed.getTime())) {
    return value;
  }

  return new Intl.DateTimeFormat("pt-BR", {
    month: "long",
    year: "numeric",
    timeZone: "UTC",
  }).format(parsed);
}

export function formatClosingMonthShortLabel(year: number, month: number) {
  const parsed = new Date(Date.UTC(year, month - 1, 1));

  return new Intl.DateTimeFormat("pt-BR", {
    month: "short",
    timeZone: "UTC",
  })
    .format(parsed)
    .replace(".", "")
    .slice(0, 3);
}
