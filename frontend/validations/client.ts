import { z } from "zod";

import { normalizeNumericValue, type ClientFieldErrors } from "@/lib/client";

export const clientSchema = z.object({
  nome: z.string().trim().min(1, "Informe o nome do cliente."),
  contato: z
    .string()
    .trim()
    .min(1, "Informe um contato.")
    .refine(
      (value) => {
        const digits = normalizeNumericValue(value);
        return digits.length === 10 || digits.length === 11;
      },
      "Informe um contato com 10 ou 11 numeros.",
    ),
  obs: z.string().trim().max(1000, "A observacao deve ter no maximo 1000 caracteres."),
  doacao: z.boolean(),
  userId: z
    .string()
    .trim()
    .refine((value) => value === "" || /^\d+$/.test(value), "Informe um UserId numerico valido."),
});

export function mapClientZodErrors(error: z.ZodError): ClientFieldErrors {
  const mapped: ClientFieldErrors = {};

  for (const issue of error.issues) {
    const field = issue.path[0];

    if (field === "nome" && !mapped.nome) {
      mapped.nome = issue.message;
    }

    if (field === "contato" && !mapped.contato) {
      mapped.contato = issue.message;
    }

    if (field === "obs" && !mapped.obs) {
      mapped.obs = issue.message;
    }

    if (field === "doacao" && !mapped.doacao) {
      mapped.doacao = issue.message;
    }

    if (field === "userId" && !mapped.userId) {
      mapped.userId = issue.message;
    }
  }

  return mapped;
}
