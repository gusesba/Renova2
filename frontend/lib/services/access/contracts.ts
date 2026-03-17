// Agrupa os contratos usados pelo modulo de acesso no frontend.
export type AuthenticatedUser = {
  id: string;
  nome: string;
  email: string;
  telefone: string;
  statusUsuario: string;
  pessoaId?: string | null;
};

export type RoleReference = {
  id: string;
  nome: string;
};

export type AccessibleStore = {
  id: string;
  nome: string;
  statusVinculo: string;
  ehResponsavel: boolean;
  cargos: RoleReference[];
};

export type SessionContext = {
  usuario: AuthenticatedUser;
  lojaAtivaId?: string | null;
  lojas: AccessibleStore[];
  permissoes: string[];
};

export type AccessPermission = {
  id: string;
  codigo: string;
  nome: string;
  descricao: string;
  modulo: string;
  ativo: boolean;
};

export type AccessRole = {
  id: string;
  nome: string;
  descricao: string;
  ativo: boolean;
  permissoes: AccessPermission[];
};

export type StoreMembershipSummary = {
  id: string;
  statusVinculo: string;
  ehResponsavel: boolean;
  cargos: RoleReference[];
};

export type AccessUser = {
  id: string;
  nome: string;
  email: string;
  telefone: string;
  statusUsuario: string;
  pessoaId?: string | null;
  vinculoLojaAtiva?: StoreMembershipSummary | null;
};

export type StoreMembership = {
  id: string;
  usuarioId: string;
  usuarioNome: string;
  usuarioEmail: string;
  statusVinculo: string;
  ehResponsavel: boolean;
  dataInicio: string;
  dataFim?: string | null;
  cargos: RoleReference[];
};

export type LoginResponse = {
  token: string;
  expiraEm: string;
  contexto: SessionContext;
};

export type RegisterResponse = {
  mensagem: string;
  usuario: AuthenticatedUser;
};

export type PasswordResetRequestResponse = {
  mensagem: string;
  tokenRecuperacao?: string | null;
  expiraEm?: string | null;
};

export type AccessWorkspace = {
  users: AccessUser[];
  permissions: AccessPermission[];
  roles: AccessRole[];
  memberships: StoreMembership[];
};
