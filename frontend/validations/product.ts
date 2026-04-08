import { z } from "zod";

import { normalizeDecimalValue, type ProductFieldErrors } from "@/lib/product";

export const productSchema = z.object({
  descricao: z.string().trim().min(1, "Informe a descricao do produto."),
  preco: z
    .string()
    .trim()
    .min(1, "Informe o preco.")
    .refine((value) => {
      const normalized = normalizeDecimalValue(value);
      const parsed = Number(normalized);
      return Number.isFinite(parsed) && parsed > 0;
    }, "Informe um preco maior que zero."),
  entrada: z.string().trim().min(1, "Informe a data de entrada."),
  situacao: z.string().trim().min(1, "Selecione a situacao."),
  produtoId: z.string().trim().min(1, "Selecione o produto."),
  marcaId: z.string().trim().min(1, "Selecione a marca."),
  tamanhoId: z.string().trim().min(1, "Selecione o tamanho."),
  corId: z.string().trim().min(1, "Selecione a cor."),
  fornecedorId: z.string().trim().min(1, "Selecione o fornecedor."),
});

export function mapProductZodErrors(error: z.ZodError): ProductFieldErrors {
  const mapped: ProductFieldErrors = {};

  for (const issue of error.issues) {
    const field = issue.path[0];

    if (field === "descricao" && !mapped.descricao) {
      mapped.descricao = issue.message;
    }

    if (field === "preco" && !mapped.preco) {
      mapped.preco = issue.message;
    }

    if (field === "entrada" && !mapped.entrada) {
      mapped.entrada = issue.message;
    }

    if (field === "situacao" && !mapped.situacao) {
      mapped.situacao = issue.message;
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

    if (field === "fornecedorId" && !mapped.fornecedorId) {
      mapped.fornecedorId = issue.message;
    }
  }

  return mapped;
}
