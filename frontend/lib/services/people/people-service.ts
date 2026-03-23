import { callApi } from "@/lib/services/core/api-client";

import type {
  PersonDetail,
  PersonReuseDraft,
  PersonSummary,
  PersonUserOption,
} from "./contracts";

// Reune as operacoes HTTP do modulo de clientes e fornecedores.
export async function listPeople(token: string) {
  return callApi<PersonSummary[]>("/people", { method: "GET" }, token);
}

export async function getPersonById(token: string, pessoaId: string) {
  return callApi<PersonDetail>(`/people/${pessoaId}`, { method: "GET" }, token);
}

export async function listLinkablePeopleUsers(token: string) {
  return callApi<PersonUserOption[]>("/people/users", { method: "GET" }, token);
}

export async function getLinkedPersonDraftByUser(token: string, usuarioId: string) {
  return callApi<PersonReuseDraft | null>(
    `/people/users/${usuarioId}/linked-person`,
    { method: "GET" },
    token,
  );
}

export async function createPerson(
  token: string,
  payload: {
    tipoPessoa: string;
    nome: string;
    nomeSocial: string;
    documento: string;
    telefone: string;
    email: string;
    logradouro: string;
    numero: string;
    complemento: string;
    bairro: string;
    cidade: string;
    uf: string;
    cep: string;
    observacoes: string;
    ativo: boolean;
    usuarioId?: string | null;
    relacaoLoja: {
      ehCliente: boolean;
      ehFornecedor: boolean;
      aceitaCreditoLoja: boolean;
      politicaPadraoFimConsignacao: string;
      observacoesInternas: string;
      statusRelacao: string;
    };
    contasBancarias: Array<{
      id?: string | null;
      banco: string;
      agencia: string;
      conta: string;
      tipoConta: string;
      pixTipo: string;
      pixChave: string;
      favorecidoNome: string;
      favorecidoDocumento: string;
      principal: boolean;
    }>;
  },
) {
  return callApi<PersonDetail>(
    "/people",
    {
      method: "POST",
      body: JSON.stringify(payload),
    },
    token,
  );
}

export async function updatePerson(
  token: string,
  pessoaId: string,
  payload: {
    tipoPessoa: string;
    nome: string;
    nomeSocial: string;
    documento: string;
    telefone: string;
    email: string;
    logradouro: string;
    numero: string;
    complemento: string;
    bairro: string;
    cidade: string;
    uf: string;
    cep: string;
    observacoes: string;
    ativo: boolean;
    usuarioId?: string | null;
    relacaoLoja: {
      ehCliente: boolean;
      ehFornecedor: boolean;
      aceitaCreditoLoja: boolean;
      politicaPadraoFimConsignacao: string;
      observacoesInternas: string;
      statusRelacao: string;
    };
    contasBancarias: Array<{
      id?: string | null;
      banco: string;
      agencia: string;
      conta: string;
      tipoConta: string;
      pixTipo: string;
      pixChave: string;
      favorecidoNome: string;
      favorecidoDocumento: string;
      principal: boolean;
    }>;
  },
) {
  return callApi<PersonDetail>(
    `/people/${pessoaId}`,
    {
      method: "PUT",
      body: JSON.stringify(payload),
    },
    token,
  );
}
