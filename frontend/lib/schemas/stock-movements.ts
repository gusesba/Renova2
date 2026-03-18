import { z } from "zod";

// Valida o payload de ajuste manual antes do envio para a API.
export const adjustStockSchema = z.object({
  motivo: z
    .string()
    .trim()
    .min(5, "Informe um motivo com pelo menos 5 caracteres."),
  pecaId: z.string().uuid("Selecione uma peca para ajustar."),
  quantidadeNova: z.coerce
    .number()
    .int("Informe uma quantidade inteira.")
    .min(0, "A quantidade nova nao pode ser negativa."),
  statusPeca: z.string().trim().optional(),
});
