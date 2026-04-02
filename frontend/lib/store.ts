export type StoreFormValues = {
  nome: string;
};

export type StoreFieldErrors = Partial<Record<keyof StoreFormValues, string>>;

export type LojaResponse = {
  id: number;
  nome: string;
};

export type UsuarioResumo = {
  id: number;
  nome: string;
  email: string;
};

export const initialStoreValues: StoreFormValues = {
  nome: "",
};

type ApiErrorResponse = {
  mensagem?: unknown;
  title?: unknown;
  errors?: Record<string, string[] | undefined>;
};

export function getAuthToken() {
  return localStorage.getItem("renova.token");
}

export function getAuthUser(): UsuarioResumo | null {
  const rawUser = localStorage.getItem("renova.usuario");

  if (!rawUser) {
    return null;
  }

  try {
    return JSON.parse(rawUser) as UsuarioResumo;
  } catch {
    return null;
  }
}

export function getStoredSelectedStoreId() {
  const value = localStorage.getItem("renova.selectedStoreId");

  if (!value) {
    return null;
  }

  const parsed = Number(value);

  return Number.isInteger(parsed) && parsed > 0 ? parsed : null;
}

export function persistSelectedStoreId(storeId: number | null) {
  if (!storeId) {
    localStorage.removeItem("renova.selectedStoreId");
    return;
  }

  localStorage.setItem("renova.selectedStoreId", String(storeId));
}

export function extractStoreApiMessage(body: unknown): string | null {
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

export function extractStoreFieldErrors(body: unknown): StoreFieldErrors {
  if (!body || typeof body !== "object" || !("errors" in body)) {
    return {};
  }

  const errors = (body as ApiErrorResponse).errors;

  if (!errors) {
    return {};
  }

  const nomeError = Object.entries(errors).find(([key]) => key.toLowerCase() === "nome")?.[1]?.[0];

  return nomeError ? { nome: nomeError } : {};
}
