import { callApi } from "@/lib/services/core/api-client";

import type {
  CommercialRulesWorkspace,
  PaymentMethod,
  StoreCommercialRule,
  SupplierCommercialRule,
} from "./contracts";

// Reune as operacoes HTTP do modulo 05.
export async function getCommercialRulesWorkspace(token: string) {
  return callApi<CommercialRulesWorkspace>(
    "/commercial-rules/workspace",
    { method: "GET" },
    token,
  );
}

export async function saveStoreCommercialRule(
  token: string,
  payload: {
    percentualRepasseDinheiro: number;
    percentualRepasseCredito: number;
    permitePagamentoMisto: boolean;
    tempoMaximoExposicaoDias: number;
    politicaDesconto: Array<{
      diasMinimos: number;
      percentualDesconto: number;
    }>;
    ativo: boolean;
  },
) {
  return callApi<StoreCommercialRule>(
    "/commercial-rules/store-rule",
    {
      method: "PUT",
      body: JSON.stringify(payload),
    },
    token,
  );
}

export async function createSupplierCommercialRule(
  token: string,
  payload: {
    pessoaLojaId: string;
    percentualRepasseDinheiro: number;
    percentualRepasseCredito: number;
    permitePagamentoMisto: boolean;
    tempoMaximoExposicaoDias: number;
    politicaDesconto: Array<{
      diasMinimos: number;
      percentualDesconto: number;
    }>;
    ativo: boolean;
  },
) {
  return callApi<SupplierCommercialRule>(
    "/commercial-rules/supplier-rules",
    {
      method: "POST",
      body: JSON.stringify(payload),
    },
    token,
  );
}

export async function updateSupplierCommercialRule(
  token: string,
  supplierRuleId: string,
  payload: {
    percentualRepasseDinheiro: number;
    percentualRepasseCredito: number;
    permitePagamentoMisto: boolean;
    tempoMaximoExposicaoDias: number;
    politicaDesconto: Array<{
      diasMinimos: number;
      percentualDesconto: number;
    }>;
    ativo: boolean;
  },
) {
  return callApi<SupplierCommercialRule>(
    `/commercial-rules/supplier-rules/${supplierRuleId}`,
    {
      method: "PUT",
      body: JSON.stringify(payload),
    },
    token,
  );
}

export async function createPaymentMethod(
  token: string,
  payload: {
    nome: string;
    tipoMeioPagamento: string;
    taxaPercentual: number;
    prazoRecebimentoDias: number;
    ativo: boolean;
  },
) {
  return callApi<PaymentMethod>(
    "/commercial-rules/payment-methods",
    {
      method: "POST",
      body: JSON.stringify(payload),
    },
    token,
  );
}

export async function updatePaymentMethod(
  token: string,
  paymentMethodId: string,
  payload: {
    nome: string;
    tipoMeioPagamento: string;
    taxaPercentual: number;
    prazoRecebimentoDias: number;
    ativo: boolean;
  },
) {
  return callApi<PaymentMethod>(
    `/commercial-rules/payment-methods/${paymentMethodId}`,
    {
      method: "PUT",
      body: JSON.stringify(payload),
    },
    token,
  );
}
