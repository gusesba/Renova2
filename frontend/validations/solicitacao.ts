import { z } from "zod";

import {
  type SolicitacaoFieldErrors,
} from "@/lib/solicitacao";
import { normalizeDecimalValue } from "@/lib/product";

export const solicitacaoSchema = z
  .object({
    descricao: z.string(),
    precoMaximo: z
      .string()
      .trim()
      .refine((value) => {
        if (!value) {
          return true;
        }

        const parsed = Number(normalizeDecimalValue(value));
        return Number.isFinite(parsed) && parsed > 0;
      }, "Informe um preco maximo maior que zero."),
    produtoId: z.string(),
    marcaId: z.string(),
    tamanhoId: z.string(),
    corId: z.string(),
    clienteId: z.string(),
  });

export function mapSolicitacaoZodErrors(error: z.ZodError): SolicitacaoFieldErrors {
  const mapped: SolicitacaoFieldErrors = {};

  for (const issue of error.issues) {
    const field = issue.path[0];

    if (field === "descricao" && !mapped.descricao) {
      mapped.descricao = issue.message;
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
