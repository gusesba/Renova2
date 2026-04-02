import { z } from "zod";

import type { StoreFieldErrors } from "@/lib/store";

export const storeSchema = z.object({
  nome: z.string().trim().min(1, "Informe o nome da loja."),
});

export function mapStoreZodErrors(error: z.ZodError): StoreFieldErrors {
  const mapped: StoreFieldErrors = {};

  for (const issue of error.issues) {
    const field = issue.path[0];

    if (field === "nome" && !mapped.nome) {
      mapped.nome = issue.message;
    }
  }

  return mapped;
}
