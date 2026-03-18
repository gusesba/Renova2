export type DashboardFiltersState = {
  dataInicial: string;
  dataFinal: string;
  vendedorUsuarioId: string;
  fornecedorPessoaId: string;
  marcaId: string;
  tipoPeca: string;
};

// Cria o periodo padrao do mes atual para a consulta inicial.
export function createDefaultDashboardFilters(): DashboardFiltersState {
  const now = new Date();
  const dataFinal = now.toISOString().slice(0, 10);
  const dataInicial = new Date(now.getFullYear(), now.getMonth(), 1)
    .toISOString()
    .slice(0, 10);

  return {
    dataInicial,
    dataFinal,
    vendedorUsuarioId: "",
    fornecedorPessoaId: "",
    marcaId: "",
    tipoPeca: "",
  };
}
