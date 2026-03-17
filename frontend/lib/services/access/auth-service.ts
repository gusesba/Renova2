import { callApi } from "@/lib/services/core/api-client";

import type {
  LoginResponse,
  PasswordResetRequestResponse,
  RegisterResponse,
  SessionContext,
} from "./contracts";

// Reune as chamadas HTTP do fluxo de autenticacao.
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
  return callApi<PasswordResetRequestResponse>(
    "/access/auth/password-reset/request",
    {
      method: "POST",
      body: JSON.stringify({ email }),
    },
  );
}

export async function confirmPasswordReset(payload: {
  token: string;
  novaSenha: string;
}) {
  return callApi<void>("/access/auth/password-reset/confirm", {
    method: "POST",
    body: JSON.stringify(payload),
  });
}

export async function changePassword(
  token: string,
  payload: { senhaAtual: string; novaSenha: string },
) {
  return callApi<void>(
    "/access/auth/change-password",
    {
      method: "POST",
      body: JSON.stringify(payload),
    },
    token,
  );
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
    token,
  );
}
