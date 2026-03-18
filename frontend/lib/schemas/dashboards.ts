import { z } from "zod";

// Centraliza os schemas do modulo 14.
export const dashboardFiltersSchema = z
  .object({
    dataInicial: z.string().trim(),
    dataFinal: z.string().trim(),
    vendedorUsuarioId: z.string().trim(),
    fornecedorPessoaId: z.string().trim(),
    marcaId: z.string().trim(),
    tipoPeca: z.string().trim(),
  })
  .superRefine((value, context) => {
    if (value.dataInicial && value.dataFinal && value.dataFinal < value.dataInicial) {
      context.addIssue({
        code: z.ZodIssueCode.custom,
        message: "A data final precisa ser maior ou igual a data inicial.",
        path: ["dataFinal"],
      });
    }
  });
