import { z } from "zod";

import type { MovementFieldErrors } from "@/lib/movement";

export const movementSchema = z.object({
  tipo: z.string().trim().min(1, "Selecione o tipo da movimentacao."),
  data: z.string().trim().min(1, "Informe a data da movimentacao."),
  clienteId: z.string().trim().min(1, "Selecione o cliente da movimentacao."),
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
  }

  return mapped;
}
