import type {
  CreditAccountDetail,
  CreditAccountSummary,
  CreditPersonOption,
} from "@/lib/services/credits";

// Estado editavel do formulario de lancamento manual.
export type ManualCreditFormState = {
  pessoaId: string;
  valor: string;
  justificativa: string;
};

export function emptyManualCreditForm(pessoaId = ""): ManualCreditFormState {
  return {
    pessoaId,
    valor: "",
    justificativa: "",
  };
}

// Gera o formulario manual a partir da conta atualmente selecionada.
export function createManualCreditForm(
  detail: CreditAccountDetail | undefined,
  people: CreditPersonOption[],
) {
  return emptyManualCreditForm(
    detail?.conta.pessoaId ?? people[0]?.pessoaId ?? "",
  );
}

// Resume os perfis da pessoa para exibir na listagem lateral.
export function describeCreditProfile(account: CreditAccountSummary) {
  if (account.ehCliente && account.ehFornecedor) {
    return "Cliente e fornecedor";
  }

  if (account.ehFornecedor) {
    return "Fornecedor";
  }

  return "Cliente";
}
