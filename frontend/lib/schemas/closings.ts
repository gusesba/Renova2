import { z } from "zod";

// Centraliza os schemas do modulo 13.
export const generateClosingSchema = z
  .object({
    pessoaId: z.string().trim().min(1, "Selecione a pessoa do fechamento."),
    periodoInicio: z.string().trim().min(1, "Informe o inicio do periodo."),
    periodoFim: z.string().trim().min(1, "Informe o fim do periodo."),
  })
  .superRefine((value, context) => {
    if (value.periodoInicio && value.periodoFim && value.periodoFim < value.periodoInicio) {
      context.addIssue({
        code: z.ZodIssueCode.custom,
        message: "O fim do periodo precisa ser maior ou igual ao inicio.",
        path: ["periodoFim"],
      });
    }
  });
