import { z } from "zod";

// Valida o filtro simples de busca do modulo 16.
export const documentQuerySchema = z.object({
  search: z.string().trim().max(120, "A busca pode ter no maximo 120 caracteres."),
  tipoDocumento: z.string().trim().min(1, "Selecione um tipo de documento."),
});
