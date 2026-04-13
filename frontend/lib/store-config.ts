type ApiErrorResponse = {
  mensagem?: unknown;
  title?: unknown;
  errors?: Record<string, string[] | undefined>;
};

export type ConfigLojaResponse = {
  lojaId: number;
  percentualRepasseFornecedor: number;
  percentualRepasseVendedorCredito: number;
  tempoPermanenciaProdutoMeses: number;
  descontosPermanencia: ConfigLojaDescontoPermanenciaResponse[];
};

export type StoreConfigFormValues = {
  percentualRepasseFornecedor: string;
  percentualRepasseVendedorCredito: string;
  tempoPermanenciaProdutoMeses: string;
};

export type ConfigLojaDescontoPermanenciaResponse = {
  aPartirDeMeses: number;
  percentualDesconto: number;
};

export type StoreDiscountFormValue = {
  id: string;
  aPartirDeMeses: string;
  percentualDesconto: string;
};

export const initialStoreConfigValues: StoreConfigFormValues = {
  percentualRepasseFornecedor: "",
  percentualRepasseVendedorCredito: "",
  tempoPermanenciaProdutoMeses: "",
};

export const initialStoreDiscountValues: StoreDiscountFormValue[] = [];

export function extractStoreConfigApiMessage(body: unknown): string | null {
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
