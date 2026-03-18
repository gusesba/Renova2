import { z } from "zod";

// Centraliza os schemas do modulo de pecas e estoque.
export const pieceDiscountBandSchema = z.object({
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

// Mantem o estado editavel permissivo ate a regra manual ser ativada.
const pieceDiscountBandDraftSchema = z.object({
  id: z.string(),
  diasMinimos: z.string(),
  percentualDesconto: z.string(),
});

// Representa o rascunho da regra manual enquanto o usuario edita o formulario.
const pieceManualRuleDraftSchema = z.object({
  percentualRepasseDinheiro: z.string(),
  percentualRepasseCredito: z.string(),
  permitePagamentoMisto: z.boolean(),
  tempoMaximoExposicaoDias: z.string(),
  politicaDesconto: z.array(pieceDiscountBandDraftSchema),
});

// Valida a regra manual final apenas quando o modo manual estiver habilitado.
export const pieceManualRuleSchema = z
  .object({
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
    politicaDesconto: z.array(pieceDiscountBandSchema),
  })
  .superRefine((data, context) => {
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
  });

export const pieceFormSchema = z
  .object({
    id: z.string(),
    tipoPeca: z.enum(["consignada", "fixa", "lote"]),
    codigoInterno: z.string(),
    codigoBarras: z.string().trim(),
    produtoNomeId: z.string().trim().min(1, "Selecione o produto."),
    marcaId: z.string().trim().min(1, "Selecione a marca."),
    tamanhoId: z.string().trim().min(1, "Selecione o tamanho."),
    corId: z.string().trim().min(1, "Selecione a cor."),
    fornecedorPessoaId: z.string(),
    descricao: z.string().trim(),
    observacoes: z.string().trim(),
    dataEntrada: z.string().trim().min(1, "Informe a data de entrada."),
    quantidadeInicial: z.coerce
      .number()
      .int()
      .min(1, "Informe a quantidade inicial."),
    quantidadeAtual: z.coerce.number().int().min(0),
    precoVendaAtual: z.coerce
      .number()
      .gt(0, "Informe o preco de venda."),
    custoUnitario: z.string().trim(),
    localizacaoFisica: z
      .string()
      .trim()
      .min(1, "Informe a localizacao fisica."),
    statusPeca: z.string(),
    usarRegraManual: z.boolean(),
    regraManual: pieceManualRuleDraftSchema,
  })
  .superRefine((data, context) => {
    if (data.tipoPeca === "consignada" && !data.fornecedorPessoaId) {
      context.addIssue({
        code: z.ZodIssueCode.custom,
        message: "Selecione o fornecedor da peca consignada.",
        path: ["fornecedorPessoaId"],
      });
    }

    if (!data.usarRegraManual) {
      return;
    }

    const manualResult = pieceManualRuleSchema.safeParse(data.regraManual);
    if (!manualResult.success) {
      manualResult.error.issues.forEach((issue) => {
        context.addIssue({
          ...issue,
          path: ["regraManual", ...issue.path],
        });
      });
    }
  });

export const pieceImageMetaSchema = z.object({
  ordem: z.coerce.number().int().min(1, "Informe uma ordem valida."),
  tipoVisibilidade: z
    .string()
    .trim()
    .min(1, "Selecione a visibilidade da imagem."),
});
