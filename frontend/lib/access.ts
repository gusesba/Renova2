import { getAuthToken as getStoredAuthToken } from "@/lib/auth";

export const permissions = {
  clientesVisualizar: "clientes.visualizar",
  clientesVisualizarDetalhe: "clientes.visualizar_detalhe",
  clientesAdicionar: "clientes.adicionar",
  clientesEditar: "clientes.editar",
  clientesExcluir: "clientes.excluir",
  clientesExportarFechamento: "clientes.exportar_fechamento",
  produtosVisualizar: "produtos.visualizar",
  produtosVisualizarItem: "produtos.visualizar_item",
  produtosAdicionar: "produtos.adicionar",
  produtosEditar: "produtos.editar",
  produtosExcluir: "produtos.excluir",
  produtosEmprestadosVisualizar: "produtos.emprestados.visualizar",
  produtosAuxiliaresVisualizar: "produtos.auxiliares.visualizar",
  produtosAuxiliaresAdicionarReferencia: "produtos.auxiliares.adicionar_referencia",
  produtosAuxiliaresAdicionarMarca: "produtos.auxiliares.adicionar_marca",
  produtosAuxiliaresAdicionarTamanho: "produtos.auxiliares.adicionar_tamanho",
  produtosAuxiliaresAdicionarCor: "produtos.auxiliares.adicionar_cor",
  solicitacoesVisualizar: "solicitacoes.visualizar",
  solicitacoesAdicionar: "solicitacoes.adicionar",
  movimentacoesVisualizar: "movimentacoes.visualizar",
  movimentacoesAdicionar: "movimentacoes.adicionar",
  movimentacoesDestinacaoVisualizar: "movimentacoes.destinacao.visualizar",
  movimentacoesDestinacaoExecutar: "movimentacoes.destinacao.executar",
  pagamentosVisualizar: "pagamentos.visualizar",
  pagamentosManuaisAdicionar: "pagamentos.manuais.adicionar",
  pagamentosCreditoVisualizar: "pagamentos.credito.visualizar",
  pagamentosCreditoAdicionar: "pagamentos.credito.adicionar",
  pagamentosCreditoResgatar: "pagamentos.credito.resgatar",
  pagamentosPendenciasVisualizar: "pagamentos.pendencias.visualizar",
  pagamentosPendenciasAtualizar: "pagamentos.pendencias.atualizar",
  pagamentosFechamentoVisualizar: "pagamentos.fechamento.visualizar",
  gastosLojaVisualizar: "gastos_loja.visualizar",
  gastosLojaAdicionar: "gastos_loja.adicionar",
  lojasVisualizar: "lojas.visualizar",
  lojasAdicionar: "lojas.adicionar",
  lojasEditar: "lojas.editar",
  lojasExcluir: "lojas.excluir",
  configLojaVisualizar: "config_loja.visualizar",
  configLojaEditar: "config_loja.editar",
  funcionariosVisualizar: "funcionarios.visualizar",
  funcionariosAdicionar: "funcionarios.adicionar",
  funcionariosEditar: "funcionarios.editar",
  funcionariosRemover: "funcionarios.remover",
  cargosVisualizar: "cargos.visualizar",
  cargosAdicionar: "cargos.adicionar",
  cargosEditar: "cargos.editar",
  cargosExcluir: "cargos.excluir",
} as const;

export type PermissionKey = (typeof permissions)[keyof typeof permissions];

export type EmployeeListItem = {
  usuarioId: number;
  nome: string;
  email: string;
  lojaId: number;
  cargoId: number;
  cargoNome: string;
};

export type AccessProfile = {
  lojaId: number;
  ehDono: boolean;
  cargoId: number | null;
  cargoNome: string | null;
  funcionalidades: PermissionKey[];
};

export type RoleFunctionality = {
  id: number;
  chave: PermissionKey;
  grupo: string;
  descricao: string;
};

export type RoleItem = {
  id: number;
  nome: string;
  lojaId: number;
  funcionalidades: RoleFunctionality[];
  quantidadeFuncionarios: number;
};

export const menuPermissionGroups: Record<string, PermissionKey[]> = {
  "/dashboard/loja": [permissions.lojasVisualizar, permissions.lojasAdicionar, permissions.lojasEditar],
  "/dashboard/controle-acesso": [
    permissions.funcionariosVisualizar,
    permissions.funcionariosAdicionar,
    permissions.funcionariosEditar,
    permissions.funcionariosRemover,
    permissions.cargosVisualizar,
    permissions.cargosAdicionar,
    permissions.cargosEditar,
    permissions.cargosExcluir,
  ],
  "/dashboard/cliente": [
    permissions.clientesVisualizar,
    permissions.clientesVisualizarDetalhe,
    permissions.clientesAdicionar,
    permissions.clientesEditar,
    permissions.clientesExcluir,
    permissions.clientesExportarFechamento,
  ],
  "/dashboard/produto": [
    permissions.produtosVisualizar,
    permissions.produtosVisualizarItem,
    permissions.produtosAdicionar,
    permissions.produtosEditar,
    permissions.produtosExcluir,
    permissions.produtosAuxiliaresVisualizar,
  ],
  "/dashboard/solicitacao": [permissions.solicitacoesVisualizar, permissions.solicitacoesAdicionar],
  "/dashboard/movimentacao": [permissions.movimentacoesVisualizar, permissions.movimentacoesAdicionar],
  "/dashboard/pagamento": [permissions.pagamentosVisualizar, permissions.pagamentosManuaisAdicionar],
  "/dashboard/gasto-loja": [permissions.gastosLojaVisualizar, permissions.gastosLojaAdicionar],
  "/dashboard/pagamento-externo": [
    permissions.pagamentosCreditoVisualizar,
    permissions.pagamentosCreditoAdicionar,
    permissions.pagamentosCreditoResgatar,
  ],
  "/dashboard/pendencia": [
    permissions.pagamentosPendenciasVisualizar,
    permissions.pagamentosPendenciasAtualizar,
    permissions.pagamentosCreditoAdicionar,
    permissions.pagamentosCreditoResgatar,
  ],
  "/dashboard/fechamento": [permissions.pagamentosFechamentoVisualizar],
};

type ApiErrorResponse = {
  mensagem?: unknown;
  title?: unknown;
  errors?: Record<string, string[] | undefined>;
};

export function getAuthToken() {
  return getStoredAuthToken();
}

export function asEmployeeListResponse(body: unknown) {
  return body as EmployeeListItem[];
}

export function asEmployeeResponse(body: unknown) {
  return body as EmployeeListItem;
}

export function asAccessProfile(body: unknown) {
  return body as AccessProfile;
}

export function asRoleListResponse(body: unknown) {
  return body as RoleItem[];
}

export function asRoleResponse(body: unknown) {
  return body as RoleItem;
}

export function asRoleFunctionalityList(body: unknown) {
  return body as RoleFunctionality[];
}

export function extractAccessApiMessage(body: unknown): string | null {
  if (!body || typeof body !== "object") {
    return null;
  }

  const data = body as ApiErrorResponse;

  if (typeof data.mensagem === "string" && data.mensagem.trim()) {
    return data.mensagem;
  }

  if (typeof data.title === "string" && data.title.trim()) {
    return data.title;
  }

  if (data.errors) {
    const firstError = Object.values(data.errors).flat().find(Boolean);

    if (firstError) {
      return firstError;
    }
  }

  return null;
}
