import { z } from "zod";

// Centraliza os schemas do modulo 11.
export const supplierPaymentLineSchema = z
  .object({
    tipoLiquidacao: z.string().trim().min(1, "Selecione o tipo de liquidacao."),
    meioPagamentoId: z.string().trim(),
    valor: z.coerce.number().positive("Informe um valor maior que zero."),
  })
  .superRefine((value, context) => {
    if (value.tipoLiquidacao === "meio_pagamento" && !value.meioPagamentoId) {
      context.addIssue({
        code: z.ZodIssueCode.custom,
        message: "Selecione o meio de pagamento da linha financeira.",
        path: ["meioPagamentoId"],
      });
    }
  });

export const supplierSettlementSchema = z.object({
  pagamentos: z
    .array(supplierPaymentLineSchema)
    .min(1, "Informe ao menos uma forma de liquidacao."),
  comprovanteUrl: z.string().trim(),
  observacoes: z
    .string()
    .trim()
    .min(1, "Informe a observacao da liquidacao."),
});
