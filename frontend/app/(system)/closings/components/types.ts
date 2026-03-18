import type { ClosingDetail, ClosingWorkspace } from "@/lib/services/closings";

export type ClosingFiltersState = {
  search: string;
  pessoaId: string;
  statusFechamento: string;
  dataInicial: string;
  dataFinal: string;
};

export type GenerateClosingFormState = {
  pessoaId: string;
  periodoInicio: string;
  periodoFim: string;
};

export const emptyClosingFilters: ClosingFiltersState = {
  search: "",
  pessoaId: "",
  statusFechamento: "",
  dataInicial: "",
  dataFinal: "",
};

// Cria o rascunho inicial do formulario de geracao.
export function createGenerateClosingForm(
  workspace?: ClosingWorkspace,
  detail?: ClosingDetail,
) {
  const today = new Date();
  const todayText = today.toISOString().slice(0, 10);
  const monthStart = new Date(today.getFullYear(), today.getMonth(), 1)
    .toISOString()
    .slice(0, 10);

  return {
    pessoaId: detail?.fechamento.pessoaId ?? workspace?.pessoas[0]?.pessoaId ?? "",
    periodoInicio: detail?.fechamento.periodoInicio.slice(0, 10) ?? monthStart,
    periodoFim: detail?.fechamento.periodoFim.slice(0, 10) ?? todayText,
  } satisfies GenerateClosingFormState;
}
