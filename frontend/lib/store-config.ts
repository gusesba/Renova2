type ApiErrorResponse = {
  mensagem?: unknown;
  title?: unknown;
  errors?: Record<string, string[] | undefined>;
};

export type ConfigLojaResponse = {
  lojaId: number;
  percentualRepasseFornecedor: number;
};

export type StoreConfigFormValues = {
  percentualRepasseFornecedor: string;
};

export const initialStoreConfigValues: StoreConfigFormValues = {
  percentualRepasseFornecedor: "",
};

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
