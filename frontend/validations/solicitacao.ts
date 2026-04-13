import { z } from "zod";

import {
  type SolicitacaoFieldErrors,
} from "@/lib/solicitacao";
import { normalizeDecimalValue } from "@/lib/product";

export const solicitacaoSchema = z
  .object({
    descricao: z.string().trim().min(1, "Informe a descricao da solicitacao."),
    precoMinimo: z
      .string()
      .trim()
      .min(1, "Informe o preco minimo.")
      .refine((value) => {
        const parsed = Number(normalizeDecimalValue(value));
        return Number.isFinite(parsed) && parsed > 0;
      }, "Informe um preco minimo maior que zero."),
    precoMaximo: z
      .string()
      .trim()
      .min(1, "Informe o preco maximo.")
      .refine((value) => {
        const parsed = Number(normalizeDecimalValue(value));
        return Number.isFinite(parsed) && parsed > 0;
      }, "Informe um preco maximo maior que zero."),
    produtoId: z.string().trim().min(1, "Selecione o produto."),
    marcaId: z.string().trim().min(1, "Selecione a marca."),
    tamanhoId: z.string().trim().min(1, "Selecione o tamanho."),
    corId: z.string().trim().min(1, "Selecione a cor."),
    clienteId: z.string().trim().min(1, "Selecione o cliente."),
  })
  .superRefine((value, context) => {
    const precoMinimo = Number(normalizeDecimalValue(value.precoMinimo));
    const precoMaximo = Number(normalizeDecimalValue(value.precoMaximo));

    if (Number.isFinite(precoMinimo) && Number.isFinite(precoMaximo) && precoMaximo < precoMinimo) {
      context.addIssue({
        code: z.ZodIssueCode.custom,
        message: "O preco maximo deve ser maior ou igual ao preco minimo.",
        path: ["precoMaximo"],
      });
    }
  });

export function mapSolicitacaoZodErrors(error: z.ZodError): SolicitacaoFieldErrors {
  const mapped: SolicitacaoFieldErrors = {};

  for (const issue of error.issues) {
    const field = issue.path[0];

    if (field === "descricao" && !mapped.descricao) {
      mapped.descricao = issue.message;
    }

    if (field === "precoMinimo" && !mapped.precoMinimo) {
      mapped.precoMinimo = issue.message;
    }

    if (field === "precoMaximo" && !mapped.precoMaximo) {
      mapped.precoMaximo = issue.message;
    }

    if (field === "produtoId" && !mapped.produtoId) {
      mapped.produtoId = issue.message;
    }

    if (field === "marcaId" && !mapped.marcaId) {
      mapped.marcaId = issue.message;
    }

    if (field === "tamanhoId" && !mapped.tamanhoId) {
      mapped.tamanhoId = issue.message;
    }

    if (field === "corId" && !mapped.corId) {
      mapped.corId = issue.message;
    }

    if (field === "clienteId" && !mapped.clienteId) {
      mapped.clienteId = issue.message;
    }
  }

  return mapped;
}
