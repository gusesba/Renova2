import { z } from "zod";

// Schemas dos formularios do modulo de lojas e configuracao operacional.
export const storeConfigurationSchema = z.object({
  nomeExibicao: z.string().trim().min(1, "Informe o nome de exibicao."),
  cabecalhoImpressao: z.string().trim(),
  rodapeImpressao: z.string().trim(),
  usaModeloUnicoEtiqueta: z.boolean(),
  usaModeloUnicoRecibo: z.boolean(),
  fusoHorario: z.string().trim().min(1, "Informe o fuso horario."),
  moeda: z.string().trim().min(1, "Informe a moeda."),
});

export const createStoreSchema = z.object({
  nomeFantasia: z.string().trim().min(1, "Informe o nome fantasia."),
  razaoSocial: z.string().trim().min(1, "Informe a razao social."),
  documento: z.string().trim().min(1, "Informe o documento da loja."),
  telefone: z.string().trim().min(1, "Informe o telefone da loja."),
  email: z.email("Informe um email valido."),
  logradouro: z.string().trim().min(1, "Informe o logradouro."),
  numero: z.string().trim().min(1, "Informe o numero."),
  complemento: z.string().trim(),
  bairro: z.string().trim().min(1, "Informe o bairro."),
  cidade: z.string().trim().min(1, "Informe a cidade."),
  uf: z.string().trim().min(2, "Informe a UF."),
  cep: z.string().trim().min(1, "Informe o CEP."),
  configuracao: storeConfigurationSchema,
});

export const updateStoreSchema = z.object({
  nomeFantasia: z.string().trim().min(1, "Informe o nome fantasia."),
  razaoSocial: z.string().trim().min(1, "Informe a razao social."),
  documento: z.string().trim().min(1, "Informe o documento da loja."),
  telefone: z.string().trim().min(1, "Informe o telefone da loja."),
  email: z.email("Informe um email valido."),
  logradouro: z.string().trim().min(1, "Informe o logradouro."),
  numero: z.string().trim().min(1, "Informe o numero."),
  complemento: z.string().trim(),
  bairro: z.string().trim().min(1, "Informe o bairro."),
  cidade: z.string().trim().min(1, "Informe a cidade."),
  uf: z.string().trim().min(2, "Informe a UF."),
  cep: z.string().trim().min(1, "Informe o CEP."),
  statusLoja: z.enum(["ativa", "inativa"]),
});
