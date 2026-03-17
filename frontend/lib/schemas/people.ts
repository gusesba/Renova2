import { z } from "zod";

// Centraliza os schemas do modulo de pessoas.
export const personBankAccountSchema = z.object({
  id: z.string(),
  banco: z.string().trim().min(1, "Informe o banco da conta."),
  agencia: z.string().trim(),
  conta: z.string().trim(),
  tipoConta: z.string().trim().min(1, "Informe o tipo da conta."),
  pixTipo: z.string().trim(),
  pixChave: z.string().trim(),
  favorecidoNome: z.string().trim().min(1, "Informe o nome do favorecido."),
  favorecidoDocumento: z
    .string()
    .trim()
    .min(1, "Informe o documento do favorecido."),
  principal: z.boolean(),
});

export const personFormSchema = z
  .object({
    id: z.string(),
    tipoPessoa: z.enum(["fisica", "juridica"]),
    nome: z.string().trim().min(1, "Informe o nome da pessoa."),
    nomeSocial: z.string().trim(),
    documento: z.string().trim().min(1, "Informe o documento da pessoa."),
    telefone: z.string().trim().min(1, "Informe o telefone da pessoa."),
    email: z.email("Informe um email valido."),
    logradouro: z.string().trim().min(1, "Informe o logradouro."),
    numero: z.string().trim().min(1, "Informe o numero."),
    complemento: z.string().trim(),
    bairro: z.string().trim().min(1, "Informe o bairro."),
    cidade: z.string().trim().min(1, "Informe a cidade."),
    uf: z.string().trim().min(2, "Informe a UF."),
    cep: z.string().trim().min(1, "Informe o CEP."),
    observacoes: z.string().trim(),
    ativo: z.boolean(),
    perfilRelacionamento: z.enum(["cliente", "fornecedor", "ambos"]),
    aceitaCreditoLoja: z.boolean(),
    politicaPadraoFimConsignacao: z.enum(["devolver", "doar"]),
    observacoesInternas: z.string().trim(),
    statusRelacao: z.enum(["ativo", "inativo"]),
    usuarioId: z.string(),
    contasBancarias: z.array(personBankAccountSchema),
  })
  .superRefine((data, context) => {
    data.contasBancarias.forEach((account, index) => {
      const hasPixType = account.pixTipo.length > 0;
      const hasPixKey = account.pixChave.length > 0;

      if (hasPixType !== hasPixKey) {
        context.addIssue({
          code: z.ZodIssueCode.custom,
          message: "Informe o tipo e a chave PIX juntos.",
          path: ["contasBancarias", index, hasPixType ? "pixChave" : "pixTipo"],
        });
      }
    });
  });
