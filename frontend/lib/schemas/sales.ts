import { z } from "zod";

// Centraliza os schemas do modulo de vendas.
export const saleItemFormSchema = z.object({
  id: z.string(),
  identificadorPeca: z
    .string()
    .trim()
    .min(1, "Informe o codigo de barras ou o codigo da peca."),
  quantidade: z.coerce
    .number()
    .int()
    .min(1, "Informe a quantidade vendida."),
  descontoUnitario: z.coerce
    .number()
    .min(0, "O desconto nao pode ser negativo."),
});

export const salePaymentFormSchema = z
  .object({
    id: z.string(),
    tipoPagamento: z
      .string()
      .trim()
      .min(1, "Selecione o tipo de pagamento."),
    meioPagamentoId: z.string(),
    valor: z.coerce.number().gt(0, "Informe o valor do pagamento."),
  })
  .superRefine((data, context) => {
    if (data.tipoPagamento === "meio_pagamento" && !data.meioPagamentoId) {
      context.addIssue({
        code: z.ZodIssueCode.custom,
        message: "Selecione o meio de pagamento.",
        path: ["meioPagamentoId"],
      });
    }
  });

export const saleFormSchema = z
  .object({
    compradorPessoaId: z
      .string()
      .trim()
      .min(1, "Selecione o comprador da venda."),
    observacoes: z.string().trim(),
    itens: z.array(saleItemFormSchema),
    pagamentos: z.array(salePaymentFormSchema),
  })
  .superRefine((data, context) => {
    if (data.itens.length === 0) {
      context.addIssue({
        code: z.ZodIssueCode.custom,
        message: "Adicione ao menos uma peca na venda.",
        path: ["itens"],
      });
    }

    if (data.pagamentos.length === 0) {
      context.addIssue({
        code: z.ZodIssueCode.custom,
        message: "Adicione ao menos um pagamento.",
        path: ["pagamentos"],
      });
    }

    const hasCreditPayment = data.pagamentos.some(
      (payment) => payment.tipoPagamento === "credito_loja",
    );

    if (hasCreditPayment && !data.compradorPessoaId) {
      context.addIssue({
        code: z.ZodIssueCode.custom,
        message: "Selecione o comprador para usar credito da loja.",
        path: ["compradorPessoaId"],
      });
    }
  });

export const cancelSaleSchema = z.object({
  motivoCancelamento: z
    .string()
    .trim()
    .min(3, "Informe o motivo do cancelamento."),
});
