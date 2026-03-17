"use client";

import { useMutation } from "@tanstack/react-query";
import { useEffect, useEffectEvent, useState, type SubmitEvent } from "react";
import { useRouter } from "next/navigation";
import { toast } from "sonner";

import { AuthIllustration } from "@/app/login/components/auth-illustration";
import { LoginForm } from "@/app/login/components/login-form";
import { RegisterForm } from "@/app/login/components/register-form";
import { ResetPasswordPanel } from "@/app/login/components/reset-password-panel";
import { RenovaMark } from "@/components/brand/renova-mark";
import {
  getZodErrorMessage,
  loginSchema,
  registerSchema,
  passwordResetConfirmSchema,
  passwordResetRequestSchema,
} from "@/lib/helpers/access-schemas";
import { getErrorMessage } from "@/lib/helpers/formatters";
import {
  readSessionToken,
  writeSessionToken,
} from "@/lib/helpers/session-storage";
import {
  confirmPasswordReset,
  getMe,
  login,
  register,
  requestPasswordReset,
} from "@/lib/services/renova-api";

// Orquestra os fluxos de login e recuperacao dentro do mesmo card visual.
export function LoginScreen() {
  const router = useRouter();
  const [authMode, setAuthMode] = useState<"login" | "register" | "reset">(
    "login",
  );
  const [tokenIssued, setTokenIssued] = useState(false);
  const [loginValues, setLoginValues] = useState({
    email: "",
    senha: "",
  });
  const [registerValues, setRegisterValues] = useState({
    nome: "",
    email: "",
    telefone: "",
    senha: "",
    confirmacaoSenha: "",
  });
  const [resetValues, setResetValues] = useState({
    email: "",
    token: "",
    novaSenha: "",
  });

  const loginMutation = useMutation({
    mutationFn: login,
    onSuccess: (response) => {
      writeSessionToken(response.token);
      toast.success("Sessao iniciada com sucesso.");
      router.replace("/dashboard");
    },
    onError: (error) => {
      toast.error(getErrorMessage(error));
    },
  });

  const registerMutation = useMutation({
    mutationFn: register,
    onSuccess: (response, variables) => {
      setAuthMode("login");
      setLoginValues({
        email: variables.email,
        senha: variables.senha,
      });
      setResetValues((current) => ({
        ...current,
        email: variables.email,
      }));
      toast.success(response.mensagem);
    },
    onError: (error) => {
      toast.error(getErrorMessage(error));
    },
  });

  const requestResetMutation = useMutation({
    mutationFn: (email: string) => requestPasswordReset(email),
    onSuccess: (response) => {
      setTokenIssued(true);
      setResetValues((current) => ({
        ...current,
        token: response.tokenRecuperacao ?? current.token,
      }));
      toast.success(response.mensagem);
    },
    onError: (error) => {
      toast.error(getErrorMessage(error));
    },
  });

  const confirmResetMutation = useMutation({
    mutationFn: confirmPasswordReset,
    onSuccess: () => {
      setAuthMode("login");
      toast.success("Senha redefinida com sucesso.");
    },
    onError: (error) => {
      toast.error(getErrorMessage(error));
    },
  });

  // Se o token salvo ainda for valido, evita mostrar a tela de login novamente.
  const tryAutoRedirect = useEffectEvent(async () => {
    const storedToken = readSessionToken();

    if (!storedToken) {
      return;
    }

    try {
      await getMe(storedToken);
      router.replace("/dashboard");
    } catch {
      return;
    }
  });

  useEffect(() => {
    void tryAutoRedirect();
  }, []);

  async function handleLoginSubmit(event: SubmitEvent<HTMLFormElement>) {
    event.preventDefault();

    const parsed = loginSchema.safeParse(loginValues);
    if (!parsed.success) {
      toast.error(getZodErrorMessage(parsed.error));
      return;
    }

    await loginMutation.mutateAsync(parsed.data);
  }

  async function handleRegisterSubmit(event: SubmitEvent<HTMLFormElement>) {
    event.preventDefault();

    const parsed = registerSchema.safeParse(registerValues);
    if (!parsed.success) {
      toast.error(getZodErrorMessage(parsed.error));
      return;
    }

    await registerMutation.mutateAsync({
      nome: parsed.data.nome,
      email: parsed.data.email,
      telefone: parsed.data.telefone,
      senha: parsed.data.senha,
    });
  }

  async function handleResetRequest(event: SubmitEvent<HTMLFormElement>) {
    event.preventDefault();

    const parsed = passwordResetRequestSchema.safeParse({
      email: resetValues.email,
    });
    if (!parsed.success) {
      toast.error(getZodErrorMessage(parsed.error));
      return;
    }

    await requestResetMutation.mutateAsync(parsed.data.email);
  }

  async function handleResetConfirm(event: SubmitEvent<HTMLFormElement>) {
    event.preventDefault();

    const parsed = passwordResetConfirmSchema.safeParse({
      token: resetValues.token,
      novaSenha: resetValues.novaSenha,
    });
    if (!parsed.success) {
      toast.error(getZodErrorMessage(parsed.error));
      return;
    }

    await confirmResetMutation.mutateAsync(parsed.data);
  }

  const busy =
    loginMutation.isPending ||
    registerMutation.isPending ||
    requestResetMutation.isPending ||
    confirmResetMutation.isPending;

  return (
    <div className="auth-page">
      <div className="auth-shell">
        <div className="auth-card">
          <section className="auth-visual">
            <RenovaMark subtitle="A melhor rede de brechos do Brasil" />
            <AuthIllustration />
          </section>

          <section className="auth-form-panel">
            {authMode === "login" ? (
              <div style={{ marginBottom: "1.5rem" }}>
                <h1 className="auth-title">Bem vindo ao Renova!</h1>
                <p className="auth-subtitle">Digite seus dados para acessar.</p>
              </div>
            ) : null}

            <div className="section-stack">
              {/* A area central alterna entre login e recuperacao sem trocar de pagina. */}
              {authMode === "reset" ? (
                <ResetPasswordPanel
                  busy={busy}
                  onBackToLogin={() => setAuthMode("login")}
                  onChange={(field, value) =>
                    setResetValues((current) => ({
                      ...current,
                      [field]: value,
                    }))
                  }
                  onConfirm={handleResetConfirm}
                  onRequest={handleResetRequest}
                  tokenIssued={tokenIssued}
                  values={resetValues}
                />
              ) : authMode === "register" ? (
                <RegisterForm
                  busy={busy}
                  onBackToLogin={() => setAuthMode("login")}
                  onChange={(field, value) =>
                    setRegisterValues((current) => ({
                      ...current,
                      [field]: value,
                    }))
                  }
                  onSubmit={handleRegisterSubmit}
                  values={registerValues}
                />
              ) : (
                <LoginForm
                  busy={busy}
                  onCreateAccount={() => setAuthMode("register")}
                  onChange={(field, value) =>
                    setLoginValues((current) => ({
                      ...current,
                      [field]: value,
                    }))
                  }
                  onSubmit={handleLoginSubmit}
                  onToggleReset={() => setAuthMode("reset")}
                  values={loginValues}
                />
              )}
            </div>
          </section>
        </div>
      </div>
    </div>
  );
}
