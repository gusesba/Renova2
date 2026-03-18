import type {
  SupplierObligationDetail,
  SupplierPaymentWorkspace,
} from "@/lib/services/supplier-payments";

// Estado editavel dos filtros da listagem do modulo.
export type SupplierPaymentFiltersState = {
  search: string;
  pessoaId: string;
  statusObrigacao: string;
  tipoObrigacao: string;
};

// Estado editavel de uma linha de liquidacao.
export type SupplierSettlementLineFormState = {
  tipoLiquidacao: string;
  meioPagamentoId: string;
  valor: string;
};

// Estado editavel do formulario de liquidacao.
export type SupplierSettlementFormState = {
  pagamentos: SupplierSettlementLineFormState[];
  comprovanteUrl: string;
  observacoes: string;
};

export const emptySupplierPaymentFilters: SupplierPaymentFiltersState = {
  search: "",
  pessoaId: "",
  statusObrigacao: "",
  tipoObrigacao: "",
};

export function createEmptySettlementLine(
  workspace?: SupplierPaymentWorkspace,
): SupplierSettlementLineFormState {
  return {
    tipoLiquidacao: workspace?.tiposLiquidacao[0]?.codigo ?? "meio_pagamento",
    meioPagamentoId: workspace?.meiosPagamento[0]?.id ?? "",
    valor: "",
  };
}

export function createEmptySettlementForm(
  workspace?: SupplierPaymentWorkspace,
): SupplierSettlementFormState {
  return {
    pagamentos: [createEmptySettlementLine(workspace)],
    comprovanteUrl: "",
    observacoes: "",
  };
}

export function createSettlementFormFromDetail(
  detail: SupplierObligationDetail | undefined,
  workspace?: SupplierPaymentWorkspace,
): SupplierSettlementFormState {
  const form = createEmptySettlementForm(workspace);
  if (!detail) {
    return form;
  }

  return {
    ...form,
    pagamentos: [
      {
        ...form.pagamentos[0],
        valor: String(detail.obrigacao.valorEmAberto || ""),
      },
    ],
    observacoes: `Liquidacao da obrigacao ${detail.obrigacao.id}.`,
  };
}
