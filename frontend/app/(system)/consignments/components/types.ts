// Estado local dos filtros da listagem de consignacao.
export type ConsignmentFiltersState = {
  search: string;
  fornecedorPessoaId: string;
  statusConsignacao: string;
  somenteProximasDoFim: boolean;
  somenteDescontoPendente: boolean;
};

// Estado local do formulario de encerramento da consignacao.
export type ConsignmentCloseFormState = {
  acao: string;
  motivo: string;
};

// Filtros padrao da tela.
export const emptyConsignmentFilters: ConsignmentFiltersState = {
  search: "",
  fornecedorPessoaId: "",
  statusConsignacao: "",
  somenteProximasDoFim: false,
  somenteDescontoPendente: false,
};

// Monta o formulario inicial do encerramento da consignacao.
export function createEmptyConsignmentCloseForm(defaultAction = ""): ConsignmentCloseFormState {
  return {
    acao: defaultAction,
    motivo: "",
  };
}
