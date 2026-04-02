import { z } from "zod";

import { fieldLabel, type AuthMode, type FieldErrors } from "@/lib/auth";

export const loginSchema = z.object({
  email: z.email("Informe um e-mail valido."),
  senha: z
    .string()
    .min(1, "Informe a senha.")
    .min(6, "A senha deve ter pelo menos 6 caracteres."),
});

export const cadastroSchema = z.object({
  nome: z.string().trim().min(1, "Informe o nome."),
  email: z.email("Informe um e-mail valido."),
  senha: z
    .string()
    .min(1, "Informe a senha.")
    .min(6, "A senha deve ter pelo menos 6 caracteres."),
});

export function getSchema(mode: AuthMode) {
  return mode === "login" ? loginSchema : cadastroSchema;
}

export function mapZodErrors(error: z.ZodError): FieldErrors {
  const mapped: FieldErrors = {};

  for (const issue of error.issues) {
    const field = issue.path[0];

    if (
      typeof field === "string" &&
      !mapped[field as keyof FieldErrors] &&
      field in fieldLabel
    ) {
      mapped[field as keyof FieldErrors] = issue.message;
    }
  }

  return mapped;
}
