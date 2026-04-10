export type PendingClientItem = {
  clienteId: number;
  nome: string;
  contato: string;
  credito: number;
};

export type PaymentNatureValue = 1 | 2;

export type PaymentStatusValue = 1 | 2 | 3;

export type PaymentCreditTypeValue = 1 | 2;

export type PaymentMovementSummary = {
  id: number;
  tipo: number;
  data: string;
  clienteId: number;
  cliente: string;
  lojaId: number;
  quantidadeProdutos: number;
  produtoIds: number[];
};

export type PaymentListItem = {
  id: number;
  movimentacaoId: number;
  lojaId: number;
  clienteId: number;
  cliente: string;
  natureza: PaymentNatureValue;
  status: PaymentStatusValue;
  valor: number;
  data: string;
  movimentacao: PaymentMovementSummary;
};

export type PaymentCreditResponse = {
  id: number;
  lojaId: number;
  clienteId: number;
  tipo: PaymentCreditTypeValue;
  valorCredito: number;
  valorDinheiro: number;
  data: string;
};

export type ExternalPaymentListItem = {
  id: number;
  lojaId: number;
  clienteId: number;
  cliente: string;
  tipo: PaymentCreditTypeValue;
  valorCredito: number;
  valorDinheiro: number;
  data: string;
};

export type PaymentListResponse = {
  itens: PaymentListItem[];
  pagina: number;
  tamanhoPagina: number;
  totalItens: number;
  totalPaginas: number;
};

export type ExternalPaymentListResponse = {
  itens: ExternalPaymentListItem[];
  pagina: number;
  tamanhoPagina: number;
  totalItens: number;
  totalPaginas: number;
};

export type PaymentFilters = {
  dataInicial: string;
  dataFinal: string;
  cliente: string;
  movimentacaoId: string;
  natureza: string;
  status: string;
  ordenarPor: "data" | "cliente" | "valor" | "natureza" | "status" | "id";
  direcao: "asc" | "desc";
  pagina: number;
  tamanhoPagina: number;
};

export type ExternalPaymentFilters = {
  dataInicial: string;
  dataFinal: string;
  cliente: string;
  tipo: string;
  ordenarPor: "data" | "cliente" | "tipo" | "valorCredito" | "valorDinheiro" | "id";
  direcao: "asc" | "desc";
  pagina: number;
  tamanhoPagina: number;
};

export type PaymentVisibleField =
  | "id"
  | "data"
  | "cliente"
  | "valor"
  | "natureza"
  | "status"
  | "movimentacaoId";

export type PaymentTableSettings = {
  tamanhoPagina: number;
  visibleFields: PaymentVisibleField[];
};

export type ExternalPaymentVisibleField =
  | "id"
  | "data"
  | "cliente"
  | "tipo"
  | "valorCredito"
  | "valorDinheiro";

export type ExternalPaymentTableSettings = {
  tamanhoPagina: number;
  visibleFields: ExternalPaymentVisibleField[];
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

export const paymentNatureOptions: Array<{ label: string; value: PaymentNatureValue }> = [
  { value: 1, label: "Receber" },
  { value: 2, label: "Pagar" },
];

export const paymentStatusOptions: Array<{ label: string; value: PaymentStatusValue }> = [
  { value: 1, label: "Pendente" },
  { value: 2, label: "Pago" },
  { value: 3, label: "Cancelado" },
];

export const paymentCreditTypeOptions: Array<{ label: string; value: PaymentCreditTypeValue }> = [
  { value: 1, label: "Pagamento do cliente" },
  { value: 2, label: "Pagamento para o fornecedor" },
];

export const initialPaymentFilters: PaymentFilters = {
  dataInicial: "",
  dataFinal: "",
  cliente: "",
  movimentacaoId: "",
  natureza: "",
  status: "",
  ordenarPor: "data",
  direcao: "desc",
  pagina: 1,
  tamanhoPagina: 10,
};

export const defaultPaymentTableSettings: PaymentTableSettings = {
  tamanhoPagina: 10,
  visibleFields: ["id", "data", "cliente", "valor", "natureza", "status", "movimentacaoId"],
};

export const initialExternalPaymentFilters: ExternalPaymentFilters = {
  dataInicial: "",
  dataFinal: "",
  cliente: "",
  tipo: "",
  ordenarPor: "data",
  direcao: "desc",
  pagina: 1,
  tamanhoPagina: 10,
};

export const defaultExternalPaymentTableSettings: ExternalPaymentTableSettings = {
  tamanhoPagina: 10,
  visibleFields: ["id", "data", "cliente", "tipo", "valorCredito", "valorDinheiro"],
};

const paymentTableSettingsStorageKey = "renova.paymentTableSettings";
const externalPaymentTableSettingsStorageKey = "renova.externalPaymentTableSettings";

export function asPendingClientsResponse(body: unknown) {
  return body as PendingClientItem[];
}

export function asPaymentListResponse(body: unknown) {
  return body as PaymentListResponse;
}

export function asPaymentCreditResponse(body: unknown) {
  return body as PaymentCreditResponse;
}

export function asExternalPaymentListResponse(body: unknown) {
  return body as ExternalPaymentListResponse;
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

export function formatPaymentNature(value: number) {
  return paymentNatureOptions.find((option) => option.value === value)?.label ?? `Natureza ${value}`;
}

export function formatPaymentStatus(value: number) {
  return paymentStatusOptions.find((option) => option.value === value)?.label ?? `Status ${value}`;
}

export function formatPaymentCreditType(value: number) {
  return (
    paymentCreditTypeOptions.find((option) => option.value === value)?.label ?? `Tipo ${value}`
  );
}

export function getPaymentStatusBadgeClass(value: number) {
  switch (value) {
    case 1:
      return "bg-amber-100 text-amber-800";
    case 2:
      return "bg-emerald-100 text-emerald-700";
    case 3:
      return "bg-rose-100 text-rose-700";
    default:
      return "bg-slate-100 text-slate-600";
  }
}

export function calculateSupplierMoneyPreview(
  creditValue: number,
  percentualRepasseFornecedor: number,
  percentualRepasseVendedorCredito: number,
) {
  if (percentualRepasseVendedorCredito <= 0) {
    return 0;
  }

  return Number(
    (
      (creditValue * percentualRepasseFornecedor) /
      percentualRepasseVendedorCredito
    ).toFixed(2),
  );
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

export function getPreviousMonthLastDateInputValue() {
  const now = new Date();
  const previousMonthLastDay = new Date(now.getFullYear(), now.getMonth(), 0);
  const timezoneOffset = previousMonthLastDay.getTimezoneOffset() * 60_000;
  return new Date(previousMonthLastDay.getTime() - timezoneOffset).toISOString().slice(0, 10);
}

function toApiDateStart(value: string) {
  return `${value}T00:00:00`;
}

function toApiDateEnd(value: string) {
  return `${value}T23:59:59.999`;
}

export function buildPaymentQuery(storeId: number, filters: PaymentFilters) {
  const params = new URLSearchParams({
    lojaId: String(storeId),
    pagina: String(filters.pagina),
    tamanhoPagina: String(filters.tamanhoPagina),
    ordenarPor: filters.ordenarPor,
    direcao: filters.direcao,
  });

  if (filters.dataInicial) {
    params.set("dataInicial", toApiDateStart(filters.dataInicial));
  }

  if (filters.dataFinal) {
    params.set("dataFinal", toApiDateEnd(filters.dataFinal));
  }

  if (filters.cliente.trim()) {
    params.set("cliente", filters.cliente.trim());
  }

  if (filters.movimentacaoId.trim()) {
    params.set("movimentacaoId", filters.movimentacaoId.trim());
  }

  if (filters.natureza.trim()) {
    params.set("natureza", filters.natureza.trim());
  }

  if (filters.status.trim()) {
    params.set("status", filters.status.trim());
  }

  return params.toString();
}

export function buildExternalPaymentQuery(storeId: number, filters: ExternalPaymentFilters) {
  const params = new URLSearchParams({
    lojaId: String(storeId),
    pagina: String(filters.pagina),
    tamanhoPagina: String(filters.tamanhoPagina),
    ordenarPor: filters.ordenarPor,
    direcao: filters.direcao,
  });

  if (filters.dataInicial) {
    params.set("dataInicial", toApiDateStart(filters.dataInicial));
  }

  if (filters.dataFinal) {
    params.set("dataFinal", toApiDateEnd(filters.dataFinal));
  }

  if (filters.cliente.trim()) {
    params.set("cliente", filters.cliente.trim());
  }

  if (filters.tipo.trim()) {
    params.set("tipo", filters.tipo.trim());
  }

  return params.toString();
}

export function formatPaymentDate(value: string) {
  const parsed = new Date(value);

  if (Number.isNaN(parsed.getTime())) {
    return value;
  }

  return new Intl.DateTimeFormat("pt-BR", {
    dateStyle: "short",
    timeZone: "UTC",
  }).format(parsed);
}

export function getStoredPaymentTableSettings(): PaymentTableSettings {
  if (typeof window === "undefined") {
    return defaultPaymentTableSettings;
  }

  const rawValue = window.localStorage.getItem(paymentTableSettingsStorageKey);

  if (!rawValue) {
    return defaultPaymentTableSettings;
  }

  try {
    const parsed = JSON.parse(rawValue) as Partial<PaymentTableSettings>;
    const tamanhoPagina =
      typeof parsed.tamanhoPagina === "number" &&
      Number.isInteger(parsed.tamanhoPagina) &&
      parsed.tamanhoPagina > 0 &&
      parsed.tamanhoPagina <= 100
        ? parsed.tamanhoPagina
        : defaultPaymentTableSettings.tamanhoPagina;

    const visibleFields = Array.isArray(parsed.visibleFields)
      ? parsed.visibleFields.filter((field): field is PaymentVisibleField =>
          ["id", "data", "cliente", "valor", "natureza", "status", "movimentacaoId"].includes(
            String(field),
          ),
        )
      : defaultPaymentTableSettings.visibleFields;

    return {
      tamanhoPagina,
      visibleFields: visibleFields.length ? visibleFields : defaultPaymentTableSettings.visibleFields,
    };
  } catch {
    return defaultPaymentTableSettings;
  }
}

export function persistPaymentTableSettings(settings: PaymentTableSettings) {
  if (typeof window === "undefined") {
    return;
  }

  window.localStorage.setItem(paymentTableSettingsStorageKey, JSON.stringify(settings));
}

export function getStoredExternalPaymentTableSettings(): ExternalPaymentTableSettings {
  if (typeof window === "undefined") {
    return defaultExternalPaymentTableSettings;
  }

  const rawValue = window.localStorage.getItem(externalPaymentTableSettingsStorageKey);

  if (!rawValue) {
    return defaultExternalPaymentTableSettings;
  }

  try {
    const parsed = JSON.parse(rawValue) as Partial<ExternalPaymentTableSettings>;
    const tamanhoPagina =
      typeof parsed.tamanhoPagina === "number" &&
      Number.isInteger(parsed.tamanhoPagina) &&
      parsed.tamanhoPagina > 0 &&
      parsed.tamanhoPagina <= 100
        ? parsed.tamanhoPagina
        : defaultExternalPaymentTableSettings.tamanhoPagina;

    const visibleFields = Array.isArray(parsed.visibleFields)
      ? parsed.visibleFields.filter((field): field is ExternalPaymentVisibleField =>
          ["id", "data", "cliente", "tipo", "valorCredito", "valorDinheiro"].includes(
            String(field),
          ),
        )
      : defaultExternalPaymentTableSettings.visibleFields;

    return {
      tamanhoPagina,
      visibleFields: visibleFields.length
        ? visibleFields
        : defaultExternalPaymentTableSettings.visibleFields,
    };
  } catch {
    return defaultExternalPaymentTableSettings;
  }
}

export function persistExternalPaymentTableSettings(settings: ExternalPaymentTableSettings) {
  if (typeof window === "undefined") {
    return;
  }

  window.localStorage.setItem(externalPaymentTableSettingsStorageKey, JSON.stringify(settings));
}
