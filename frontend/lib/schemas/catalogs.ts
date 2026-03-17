import { z } from "zod";

// Centraliza os schemas do modulo de catalogos auxiliares enxutos.
export const catalogNameFormSchema = z.object({
  id: z.string(),
  nome: z.string().trim().min(1, "Informe o nome do cadastro."),
});
