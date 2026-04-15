import { getAuthToken as getStoredAuthToken } from "@/lib/auth";

export type EmployeeListItem = {
  usuarioId: number;
  nome: string;
  email: string;
  lojaId: number;
};

type ApiErrorResponse = {
  mensagem?: unknown;
  title?: unknown;
  errors?: Record<string, string[] | undefined>;
};

export function getAuthToken() {
  return getStoredAuthToken();
}

export function asEmployeeListResponse(body: unknown) {
  return body as EmployeeListItem[];
}

export function asEmployeeResponse(body: unknown) {
  return body as EmployeeListItem;
}

export function extractAccessApiMessage(body: unknown): string | null {
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
