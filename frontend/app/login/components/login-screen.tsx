"use client";

import { useMutation } from "@tanstack/react-query";
import { useEffect, useEffectEvent, useState, type FormEvent } from "react";
import { useRouter } from "next/navigation";
import { toast } from "sonner";

import { AuthIllustration } from "@/app/login/components/auth-illustration";
import { LoginForm } from "@/app/login/components/login-form";
import { ResetPasswordPanel } from "@/app/login/components/reset-password-panel";
import { RenovaMark } from "@/components/brand/renova-mark";
import {
  getZodErrorMessage,
  loginSchema,
  passwordResetConfirmSchema,
  passwordResetRequestSchema,
} from "@/lib/helpers/access-schemas";
import { getErrorMessage } from "@/lib/helpers/formatters";
import { readSessionToken, writeSessionToken } from "@/lib/helpers/session-storage";
import {
  confirmPasswordReset,
  getMe,
  login,
  requestPasswordReset,
} from "@/lib/services/renova-api";

export function LoginScreen() {
  const router = useRouter();
  const [showReset, setShowReset] = useState(false);
  const [tokenIssued, setTokenIssued] = useState(false);
  const [loginValues, setLoginValues] = useState({
    email: "admin@renova.local",
    senha: "Renova123!",
  });
  const [resetValues, setResetValues] = useState({
    email: "admin@renova.local",
    token: "",
    novaSenha: "Renova123!",
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
      setShowReset(false);
      toast.success("Senha redefinida com sucesso.");
    },
    onError: (error) => {
      toast.error(getErrorMessage(error));
    },
  });

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

  async function handleLoginSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    const parsed = loginSchema.safeParse(loginValues);
    if (!parsed.success) {
      toast.error(getZodErrorMessage(parsed.error));
      return;
    }

    await loginMutation.mutateAsync(parsed.data);
  }

  async function handleResetRequest(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    const parsed = passwordResetRequestSchema.safeParse({ email: resetValues.email });
    if (!parsed.success) {
      toast.error(getZodErrorMessage(parsed.error));
      return;
    }

    await requestResetMutation.mutateAsync(parsed.data.email);
  }

  async function handleResetConfirm(event: FormEvent<HTMLFormElement>) {
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
            {!showReset ? (
              <div style={{ marginBottom: "1.5rem" }}>
                <h1 className="auth-title">Bem vindo ao Renova!</h1>
                <p className="auth-subtitle">Digite seus dados para acessar.</p>
              </div>
            ) : null}

            <div className="section-stack">
              {showReset ? (
                <ResetPasswordPanel
                  busy={busy}
                  onBackToLogin={() => setShowReset(false)}
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
              ) : (
                <LoginForm
                  busy={busy}
                  onChange={(field, value) =>
                    setLoginValues((current) => ({
                      ...current,
                      [field]: value,
                    }))
                  }
                  onSubmit={handleLoginSubmit}
                  onToggleReset={() => setShowReset(true)}
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
