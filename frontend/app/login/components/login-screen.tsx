"use client";

import {
  startTransition,
  useEffect,
  useEffectEvent,
  useState,
  type FormEvent,
} from "react";
import { useRouter } from "next/navigation";

import { AuthIllustration } from "@/app/login/components/auth-illustration";
import { LoginForm } from "@/app/login/components/login-form";
import { ResetPasswordPanel } from "@/app/login/components/reset-password-panel";
import { RenovaMark } from "@/components/brand/renova-mark";
import { FeedbackBanner } from "@/components/ui/feedback-banner";
import { getErrorMessage } from "@/lib/helpers/formatters";
import {
  readSessionToken,
  writeSessionToken,
} from "@/lib/helpers/session-storage";
import {
  confirmPasswordReset,
  getMe,
  login,
  requestPasswordReset,
} from "@/lib/services/renova-api";

export function LoginScreen() {
  const router = useRouter();
  const [busy, setBusy] = useState(false);
  const [showReset, setShowReset] = useState(false);
  const [feedback, setFeedback] = useState<string | null>(null);
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

  const tryAutoRedirect = useEffectEvent(async () => {
    const storedToken = readSessionToken();

    if (!storedToken) {
      return;
    }

    try {
      await getMe(storedToken);
      router.replace("/dashboard");
    } catch {
      startTransition(() => {
        setFeedback(null);
      });
    }
  });

  useEffect(() => {
    void tryAutoRedirect();
  }, []);

  async function handleLoginSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setBusy(true);

    try {
      const response = await login(loginValues);
      writeSessionToken(response.token);
      startTransition(() => {
        setFeedback("Sessao iniciada com sucesso.");
      });
      router.replace("/dashboard");
    } catch (error) {
      startTransition(() => {
        setFeedback(getErrorMessage(error));
      });
    } finally {
      setBusy(false);
    }
  }

  async function handleResetRequest(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setBusy(true);

    try {
      const response = await requestPasswordReset(resetValues.email);
      startTransition(() => {
        setTokenIssued(true);
        setResetValues((current) => ({
          ...current,
          token: response.tokenRecuperacao ?? current.token,
        }));
        setFeedback(response.mensagem);
      });
    } catch (error) {
      startTransition(() => {
        setFeedback(getErrorMessage(error));
      });
    } finally {
      setBusy(false);
    }
  }

  async function handleResetConfirm(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setBusy(true);

    try {
      await confirmPasswordReset({
        token: resetValues.token,
        novaSenha: resetValues.novaSenha,
      });
      startTransition(() => {
        setFeedback("Senha redefinida com sucesso.");
        setShowReset(false);
      });
    } catch (error) {
      startTransition(() => {
        setFeedback(getErrorMessage(error));
      });
    } finally {
      setBusy(false);
    }
  }

  return (
    <div className="auth-page">
      <div className="auth-shell">
        <div className="auth-card">
          <section className="auth-visual">
            <RenovaMark subtitle="A melhor rede de brechós do Brasil" />
            <AuthIllustration />
          </section>

          <section className="auth-form-panel">
            {!showReset ? (
              <div style={{ marginBottom: "1.5rem" }}>
                <h1 className="auth-title">Bem vindo ao Renova!</h1>
                <p className="auth-subtitle">Digite seus dados para acessar.</p>
              </div>
            ) : null}

            {feedback ? <FeedbackBanner message={feedback} /> : null}

            <div
              className="section-stack"
              style={{ marginTop: feedback ? "1rem" : 0 }}
            >
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
