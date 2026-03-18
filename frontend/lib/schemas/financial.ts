import { z } from "zod";

// Centraliza os schemas do modulo 12.
export const financialEntrySchema = z
  .object({
    tipoMovimentacao: z.string().trim().min(1, "Selecione o tipo do lancamento."),
    direcao: z.string().trim().min(1, "Selecione a direcao do lancamento."),
    meioPagamentoId: z.string().trim(),
    valorBruto: z.coerce.number().positive("Informe um valor bruto maior que zero."),
    taxa: z.coerce.number().min(0, "A taxa nao pode ser negativa."),
    descricao: z.string().trim().min(1, "Informe a descricao do lancamento."),
    competenciaEm: z.string().trim(),
    movimentadoEm: z.string().trim(),
  })
  .superRefine((value, context) => {
    if (value.tipoMovimentacao === "despesa" && value.direcao !== "saida") {
      context.addIssue({
        code: z.ZodIssueCode.custom,
        message: "Despesas precisam sair do financeiro.",
        path: ["direcao"],
      });
    }

    if (
      value.tipoMovimentacao === "receita_avulsa" &&
      value.direcao !== "entrada"
    ) {
      context.addIssue({
        code: z.ZodIssueCode.custom,
        message: "Receitas avulsas precisam entrar no financeiro.",
        path: ["direcao"],
      });
    }
  });
