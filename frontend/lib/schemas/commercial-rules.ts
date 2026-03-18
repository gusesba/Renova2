import { z } from "zod";

// Centraliza os schemas do modulo de regras comerciais.
export const commercialDiscountBandSchema = z.object({
  id: z.string(),
  diasMinimos: z.coerce
    .number()
    .int()
    .min(1, "Informe os dias minimos da faixa."),
  percentualDesconto: z.coerce
    .number()
    .min(0, "O desconto nao pode ser negativo.")
    .max(100, "O desconto nao pode ser maior que 100%."),
});

const commercialRuleBaseSchema = z.object({
  id: z.string(),
  percentualRepasseDinheiro: z.coerce
    .number()
    .min(0, "Informe o percentual de dinheiro.")
    .max(100, "O percentual nao pode ser maior que 100%."),
  percentualRepasseCredito: z.coerce
    .number()
    .min(0, "Informe o percentual de credito.")
    .max(100, "O percentual nao pode ser maior que 100%."),
  permitePagamentoMisto: z.boolean(),
  tempoMaximoExposicaoDias: z.coerce
    .number()
    .int()
    .min(1, "Informe o prazo maximo de exposicao."),
  politicaDesconto: z.array(commercialDiscountBandSchema),
  ativo: z.boolean(),
});

export const storeCommercialRuleFormSchema = commercialRuleBaseSchema.superRefine(
  validateDiscountBands,
);

export const supplierCommercialRuleFormSchema = commercialRuleBaseSchema
  .extend({
    pessoaLojaId: z.string().trim().min(1, "Selecione o fornecedor."),
  })
  .superRefine(validateDiscountBands);

export const paymentMethodFormSchema = z.object({
  id: z.string(),
  nome: z.string().trim().min(1, "Informe o nome do meio de pagamento."),
  tipoMeioPagamento: z
    .string()
    .trim()
    .min(1, "Selecione o tipo do meio de pagamento."),
  taxaPercentual: z.coerce
    .number()
    .min(0, "A taxa nao pode ser negativa.")
    .max(100, "A taxa nao pode ser maior que 100%."),
  prazoRecebimentoDias: z.coerce
    .number()
    .int()
    .min(0, "O prazo de recebimento nao pode ser negativo."),
  ativo: z.boolean(),
});

function validateDiscountBands(
  data: { politicaDesconto: Array<{ diasMinimos: number }> },
  context: z.RefinementCtx,
) {
  const usedDays = new Set<number>();
  data.politicaDesconto.forEach((band, index) => {
    if (usedDays.has(band.diasMinimos)) {
      context.addIssue({
        code: z.ZodIssueCode.custom,
        message: "Nao repita a mesma faixa de dias.",
        path: ["politicaDesconto", index, "diasMinimos"],
      });
      return;
    }

    usedDays.add(band.diasMinimos);
  });
}
