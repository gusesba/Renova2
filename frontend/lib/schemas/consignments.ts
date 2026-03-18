import { z } from "zod";

// Centraliza os schemas do modulo de consignacao.
export const consignmentCloseFormSchema = z.object({
  acao: z.string().trim().min(1, "Selecione a acao final da consignacao."),
  motivo: z
    .string()
    .trim()
    .min(3, "Informe um motivo resumido para o encerramento."),
});
