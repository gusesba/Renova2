import type {
  SaleDetail,
  SalePaymentMethodOption,
  SalePieceOption,
} from "@/lib/services/sales";

// Mantem os filtros simples usados na listagem do modulo.
export type SaleFiltersState = {
  search: string;
  statusVenda: string;
  compradorPessoaId: string;
  dataInicial: string;
  dataFinal: string;
};

// Representa um item editavel do formulario de venda.
export type SaleItemFormState = {
  id: string;
  identificadorPeca: string;
  quantidade: string;
  descontoUnitario: string;
};

// Representa um pagamento editavel do formulario de venda.
export type SalePaymentFormState = {
  id: string;
  tipoPagamento: string;
  meioPagamentoId: string;
  valor: string;
};

// Representa o formulario principal do registro de venda.
export type SaleFormState = {
  compradorPessoaId: string;
  observacoes: string;
  itens: SaleItemFormState[];
  pagamentos: SalePaymentFormState[];
};

// Representa o formulario simples de cancelamento.
export type CancelSaleFormState = {
  motivoCancelamento: string;
};

// Cria um item vazio para a venda em edicao.
export function createEmptySaleItem(pieceReference = ""): SaleItemFormState {
  return {
    id: crypto.randomUUID(),
    identificadorPeca: pieceReference,
    quantidade: "1",
    descontoUnitario: "0",
  };
}

// Cria um pagamento vazio para a venda em edicao.
export function createEmptySalePayment(
  paymentType = "meio_pagamento",
): SalePaymentFormState {
  return {
    id: crypto.randomUUID(),
    tipoPagamento: paymentType,
    meioPagamentoId: "",
    valor: "",
  };
}

// Cria o estado inicial do formulario de venda.
export function createEmptySaleForm(
  firstPaymentType = "meio_pagamento",
): SaleFormState {
  return {
    compradorPessoaId: "",
    observacoes: "",
    itens: [createEmptySaleItem()],
    pagamentos: [createEmptySalePayment(firstPaymentType)],
  };
}

// Cria o estado inicial do formulario de cancelamento.
export function createEmptyCancelSaleForm(): CancelSaleFormState {
  return {
    motivoCancelamento: "",
  };
}

// Cria os filtros padrao da listagem.
export function emptySaleFilters(): SaleFiltersState {
  return {
    search: "",
    statusVenda: "",
    compradorPessoaId: "",
    dataInicial: "",
    dataFinal: "",
  };
}

// Normaliza o identificador informado no formulario antes de comparar.
function normalizePieceReference(value: string) {
  return value.trim().toUpperCase();
}

// Resolve a peca pelo id, codigo interno ou codigo de barras digitado.
export function resolveSalePieceReference(
  pieces: SalePieceOption[],
  pieceReference: string,
) {
  const normalizedReference = normalizePieceReference(pieceReference);
  if (!normalizedReference) {
    return undefined;
  }

  return pieces.find((piece) => {
    const normalizedBarcode = piece.codigoBarras.trim().toUpperCase();
    const normalizedInternalCode = piece.codigoInterno.trim().toUpperCase();

    return (
      piece.pecaId === pieceReference.trim() ||
      normalizedInternalCode === normalizedReference ||
      normalizedBarcode === normalizedReference
    );
  });
}

// Resolve o meio de pagamento selecionado no formulario a partir do workspace.
export function getSelectedPaymentMethod(
  paymentMethods: SalePaymentMethodOption[],
  paymentMethodId: string,
) {
  return paymentMethods.find((paymentMethod) => paymentMethod.id === paymentMethodId);
}

// Calcula o total editavel da venda a partir do formulario e do workspace.
export function calculateSaleDraftTotals(
  form: SaleFormState,
  pieces: SalePieceOption[],
  paymentMethods: SalePaymentMethodOption[],
) {
  const subtotal = form.itens.reduce((sum, item) => {
    const piece = resolveSalePieceReference(pieces, item.identificadorPeca);
    const quantity = Number(item.quantidade || 0);
    return sum + (piece?.precoVendaAtual ?? 0) * quantity;
  }, 0);

  const discount = form.itens.reduce((sum, item) => {
    const quantity = Number(item.quantidade || 0);
    return sum + Number(item.descontoUnitario || 0) * quantity;
  }, 0);

  const grossPayments = form.pagamentos.reduce(
    (sum, payment) => sum + Number(payment.valor || 0),
    0,
  );

  const totalFees = form.pagamentos.reduce((sum, payment) => {
    if (payment.tipoPagamento !== "meio_pagamento") {
      return sum;
    }

    const method = getSelectedPaymentMethod(paymentMethods, payment.meioPagamentoId);
    const value = Number(payment.valor || 0);
    return sum + value * ((method?.taxaPercentual ?? 0) / 100);
  }, 0);

  return {
    subtotal,
    desconto: discount,
    totalVenda: subtotal - discount,
    totalPagamentos: grossPayments,
    taxa: totalFees,
    liquido: grossPayments - totalFees,
  };
}

// Reinicia o formulario a partir do workspace atual apos uma venda concluida.
export function resetSaleForm(
  workspace: {
    tiposPagamento: Array<{ codigo: string }>;
  } | null | undefined,
) {
  return createEmptySaleForm(workspace?.tiposPagamento[0]?.codigo ?? "meio_pagamento");
}

// Extrai o total bruto a partir do detalhe retornado pela API.
export function calculateSaleTotal(detail: SaleDetail) {
  return detail.subtotal - detail.descontoTotal;
}
