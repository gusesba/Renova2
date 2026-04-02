export type AuthMode = "login" | "cadastro";

export type FormValues = {
  nome: string;
  email: string;
  senha: string;
};

export type FieldErrors = Partial<Record<keyof FormValues, string>>;

export type Usuario = {
  id: number;
  nome: string;
  email: string;
};

export type UsuarioTokenResponse = {
  usuario: Usuario;
  token: string;
};

export type ApiErrorResponse = {
  mensagem?: unknown;
  title?: unknown;
  errors?: Record<string, string[] | undefined>;
};

export const fieldLabel: Record<keyof FormValues, string> = {
  nome: "Nome",
  email: "E-mail",
  senha: "Senha",
};

export const initialValues: FormValues = {
  nome: "",
  email: "",
  senha: "",
};

export function extractApiMessage(body: unknown): string | null {
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

export function extractApiFieldErrors(body: unknown): FieldErrors {
  if (!body || typeof body !== "object" || !("errors" in body)) {
    return {};
  }

  const errors = (body as ApiErrorResponse).errors;
  if (!errors) {
    return {};
  }

  const mappedEntries = Object.entries(errors)
    .map(([key, value]) => {
      const normalizedKey = key.toLowerCase();
      const formKey = (Object.keys(fieldLabel) as Array<keyof FormValues>).find(
        (item) => item.toLowerCase() === normalizedKey,
      );

      return formKey && value?.[0] ? [formKey, value[0]] : null;
    })
    .filter(Boolean) as Array<[keyof FormValues, string]>;

  return Object.fromEntries(mappedEntries);
}

export function persistAuthSession(response: UsuarioTokenResponse) {
  localStorage.setItem("renova.token", response.token);
  localStorage.setItem("renova.usuario", JSON.stringify(response.usuario));
}
