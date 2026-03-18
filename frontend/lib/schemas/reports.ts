import { z } from "zod";

// Centraliza os schemas do modulo 15.
export const reportQuerySchema = z
  .object({
    tipoRelatorio: z.string().trim().min(1, "Selecione o tipo de relatorio."),
    lojaId: z.string().trim(),
    dataInicial: z.string().trim(),
    dataFinal: z.string().trim(),
    fornecedorPessoaId: z.string().trim(),
    pessoaId: z.string().trim(),
    marcaId: z.string().trim(),
    vendedorUsuarioId: z.string().trim(),
    statusPeca: z.string().trim(),
    motivoMovimentacao: z.string().trim(),
    search: z.string().trim(),
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

export const saveReportFilterSchema = z.object({
  nome: z.string().trim().min(3, "Informe um nome para o filtro salvo."),
});
