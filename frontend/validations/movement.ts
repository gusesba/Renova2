import { z } from "zod";

import type { MovementFieldErrors } from "@/lib/movement";

export const movementSchema = z.object({
  tipo: z.coerce.string().trim().min(1, "Selecione o tipo da movimentacao."),
  data: z.string().trim().min(1, "Informe a data da movimentacao."),
  clienteId: z.string().trim().min(1, "Selecione o cliente da movimentacao."),
  descontoTotal: z
    .string()
    .trim()
    .refine(
      (value) => {
        const normalized = value === "" ? "0" : value.replace(",", ".");
        const parsed = Number(normalized);
        return Number.isFinite(parsed) && parsed >= 0 && parsed <= 100;
      },
      "Informe um desconto total entre 0 e 100.",
    ),
});

export function mapMovementZodErrors(error: z.ZodError): MovementFieldErrors {
  const mapped: MovementFieldErrors = {};

  for (const issue of error.issues) {
    const field = issue.path[0];

    if (field === "tipo" && !mapped.tipo) {
      mapped.tipo = issue.message;
    }

    if (field === "data" && !mapped.data) {
      mapped.data = issue.message;
    }

    if (field === "clienteId" && !mapped.clienteId) {
      mapped.clienteId = issue.message;
    }

    if (field === "descontoTotal" && !mapped.descontoTotal) {
      mapped.descontoTotal = issue.message;
    }
  }

  return mapped;
}
