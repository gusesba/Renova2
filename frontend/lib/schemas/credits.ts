import { z } from "zod";

// Centraliza os schemas do modulo 10.
export const ensureCreditAccountSchema = z.object({
  pessoaId: z.string().trim().min(1, "Selecione a pessoa da conta."),
});

export const manualCreditSchema = z.object({
  pessoaId: z.string().trim().min(1, "Selecione a pessoa para lancar o credito."),
  valor: z.coerce.number().positive("Informe um valor de credito maior que zero."),
  justificativa: z
    .string()
    .trim()
    .min(1, "Informe a justificativa do credito manual."),
});

export const creditAccountStatusSchema = z.object({
  contaId: z.string().trim().min(1, "Conta de credito invalida."),
  statusConta: z.string().trim().min(1, "Selecione o status da conta."),
});
