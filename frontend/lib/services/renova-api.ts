const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:5131/api/v1";

// Cliente HTTP enxuto da aplicacao; centraliza endpoints, tipos e tratamento basico de erro.
type Envelope<T> = {
  data: T;
};

type ErrorEnvelope = {
  detail?: string;
  title?: string;
};

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

type LoginResponse = {
  token: string;
  expiraEm: string;
  contexto: SessionContext;
};

type RegisterResponse = {
  mensagem: string;
  usuario: AuthenticatedUser;
};

type PasswordResetRequestResponse = {
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

async function callApi<T>(path: string, init: RequestInit, token?: string | null) {
  // Todas as chamadas passam por aqui para reaproveitar headers e parsing do envelope.
  const headers = new Headers(init.headers);
  headers.set("Content-Type", "application/json");

  if (token) {
    headers.set("Authorization", `Bearer ${token}`);
  }

  let response: Response;

  try {
    response = await fetch(`${API_BASE_URL}${path}`, {
      ...init,
      headers,
      cache: "no-store",
    });
  } catch {
    throw new Error("Servidor indisponivel. Tente novamente em instantes.");
  }

  if (response.status === 204) {
    return undefined as T;
  }

  const rawBody = await response.text();
  const body = rawBody
    ? ((JSON.parse(rawBody) as Envelope<T> & ErrorEnvelope))
    : null;

  if (!response.ok) {
    throw new Error(
      body?.detail ??
        body?.title ??
        (response.status === 401 || response.status === 403
          ? "Voce nao tem acesso a esta funcionalidade."
          : "Falha ao consultar a API."),
    );
  }

  if (!body) {
    throw new Error("Resposta invalida do servidor.");
  }

  return body.data;
}

export async function login(credentials: { email: string; senha: string }) {
  return callApi<LoginResponse>("/access/auth/login", {
    method: "POST",
    body: JSON.stringify(credentials),
  });
}

export async function register(payload: {
  nome: string;
  email: string;
  telefone: string;
  senha: string;
}) {
  return callApi<RegisterResponse>("/access/auth/register", {
    method: "POST",
    body: JSON.stringify(payload),
  });
}

export async function requestPasswordReset(email: string) {
  return callApi<PasswordResetRequestResponse>("/access/auth/password-reset/request", {
    method: "POST",
    body: JSON.stringify({ email }),
  });
}

export async function confirmPasswordReset(payload: { token: string; novaSenha: string }) {
  return callApi<void>("/access/auth/password-reset/confirm", {
    method: "POST",
    body: JSON.stringify(payload),
  });
}

export async function logout(token: string) {
  return callApi<void>("/access/auth/logout", { method: "POST" }, token);
}

export async function getMe(token: string) {
  return callApi<SessionContext>("/access/auth/me", { method: "GET" }, token);
}

export async function changeActiveStore(token: string, lojaId: string) {
  return callApi<SessionContext>(
    "/access/auth/active-store",
    {
      method: "POST",
      body: JSON.stringify({ lojaId }),
    },
    token
  );
}

export async function listUsers(token: string) {
  return callApi<AccessUser[]>("/access/users", { method: "GET" }, token);
}

export async function createUser(
  token: string,
  payload: {
    nome: string;
    email: string;
    telefone: string;
    senha: string;
    pessoaId: null;
  }
) {
  return callApi<AccessUser>(
    "/access/users",
    {
      method: "POST",
      body: JSON.stringify(payload),
    },
    token
  );
}

export async function updateUser(
  token: string,
  usuarioId: string,
  payload: {
    nome: string;
    email: string;
    telefone: string;
    pessoaId: null;
  }
) {
  return callApi<AccessUser>(
    `/access/users/${usuarioId}`,
    {
      method: "PUT",
      body: JSON.stringify(payload),
    },
    token
  );
}

export async function changeUserStatus(token: string, usuarioId: string, statusUsuario: string) {
  return callApi<AccessUser>(
    `/access/users/${usuarioId}/status`,
    {
      method: "POST",
      body: JSON.stringify({ statusUsuario }),
    },
    token
  );
}

export async function listPermissions(token: string) {
  return callApi<AccessPermission[]>("/access/permissions", { method: "GET" }, token);
}

export async function listRoles(token: string) {
  return callApi<AccessRole[]>("/access/roles", { method: "GET" }, token);
}

export async function createRole(
  token: string,
  payload: { nome: string; descricao: string; permissaoIds: string[] }
) {
  return callApi<AccessRole>(
    "/access/roles",
    {
      method: "POST",
      body: JSON.stringify(payload),
    },
    token
  );
}

export async function updateRole(
  token: string,
  cargoId: string,
  payload: { nome: string; descricao: string; ativo: boolean }
) {
  return callApi<AccessRole>(
    `/access/roles/${cargoId}`,
    {
      method: "PUT",
      body: JSON.stringify(payload),
    },
    token
  );
}

export async function updateRolePermissions(token: string, cargoId: string, permissaoIds: string[]) {
  return callApi<AccessRole>(
    `/access/roles/${cargoId}/permissions`,
    {
      method: "PUT",
      body: JSON.stringify({ permissaoIds }),
    },
    token
  );
}

export async function listMemberships(token: string) {
  return callApi<StoreMembership[]>("/access/store-memberships", { method: "GET" }, token);
}

export async function createMembership(
  token: string,
  payload: {
    usuarioId: string;
    statusVinculo: string;
    ehResponsavel: boolean;
    dataFim: null;
    cargoIds: string[];
  }
) {
  return callApi<StoreMembership>(
    "/access/store-memberships",
    {
      method: "POST",
      body: JSON.stringify(payload),
    },
    token
  );
}

export async function updateMembershipRoles(token: string, usuarioLojaId: string, cargoIds: string[]) {
  return callApi<StoreMembership>(
    `/access/store-memberships/${usuarioLojaId}/roles`,
    {
      method: "PUT",
      body: JSON.stringify({ cargoIds }),
    },
    token
  );
}

export async function loadAccessWorkspace(token: string): Promise<AccessWorkspace> {
  // O dashboard depende desses quatro blocos ao mesmo tempo, entao carrega tudo em paralelo.
  const [users, permissions, roles, memberships] = await Promise.all([
    listUsers(token),
    listPermissions(token),
    listRoles(token),
    listMemberships(token),
  ]);

  return { users, permissions, roles, memberships };
}
