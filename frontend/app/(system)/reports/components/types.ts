import type { ReportQueryPayload, ReportWorkspace } from "@/lib/services/reports";

export type ReportQueryState = {
  tipoRelatorio: string;
  lojaId: string;
  dataInicial: string;
  dataFinal: string;
  fornecedorPessoaId: string;
  pessoaId: string;
  marcaId: string;
  vendedorUsuarioId: string;
  statusPeca: string;
  motivoMovimentacao: string;
  search: string;
};

// Cria o filtro inicial do modulo com a loja ativa e o periodo do mes atual.
export function createDefaultReportQuery(workspace?: ReportWorkspace): ReportQueryState {
  const now = new Date();
  const dataFinal = now.toISOString().slice(0, 10);
  const dataInicial = new Date(now.getFullYear(), now.getMonth(), 1)
    .toISOString()
    .slice(0, 10);

  return {
    tipoRelatorio: workspace?.tiposRelatorio[0]?.codigo ?? "estoque_atual",
    lojaId: workspace?.lojaAtivaId ?? "",
    dataInicial,
    dataFinal,
    fornecedorPessoaId: "",
    pessoaId: "",
    marcaId: "",
    vendedorUsuarioId: "",
    statusPeca: "",
    motivoMovimentacao: "",
    search: "",
  };
}

// Normaliza o estado do formulario para o payload esperado pela API.
export function toReportPayload(state: ReportQueryState): ReportQueryPayload {
  return {
    tipoRelatorio: state.tipoRelatorio,
    lojaId: state.lojaId || null,
    dataInicial: state.dataInicial || null,
    dataFinal: state.dataFinal || null,
    fornecedorPessoaId: state.fornecedorPessoaId || null,
    pessoaId: state.pessoaId || null,
    marcaId: state.marcaId || null,
    vendedorUsuarioId: state.vendedorUsuarioId || null,
    statusPeca: state.statusPeca || null,
    motivoMovimentacao: state.motivoMovimentacao || null,
    search: state.search || null,
  };
}
