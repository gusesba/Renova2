import type { FormEvent } from "react";

import { Button } from "@/components/ui/button";
import { TextInput } from "@/components/ui/field";

type ResetPasswordPanelProps = {
  values: {
    email: string;
    token: string;
    novaSenha: string;
  };
  busy: boolean;
  tokenIssued: boolean;
  onChange: (field: "email" | "token" | "novaSenha", value: string) => void;
  onRequest: (event: FormEvent<HTMLFormElement>) => void;
  onConfirm: (event: FormEvent<HTMLFormElement>) => void;
  onBackToLogin: () => void;
};

export function ResetPasswordPanel({
  values,
  busy,
  tokenIssued,
  onChange,
  onRequest,
  onConfirm,
  onBackToLogin,
}: ResetPasswordPanelProps) {
  return (
    <div className="section-stack">
      <div>
        <h2 className="auth-title" style={{ fontSize: "2rem" }}>
          Recuperar acesso
        </h2>
        <p className="auth-subtitle">
          Gere o token e conclua a redefinicao no mesmo espaço onde o login aparece.
        </p>
      </div>

      <form className="form-grid" onSubmit={onRequest}>
        <TextInput
          label="Email de recuperacao"
          onChange={(event) => onChange("email", event.target.value)}
          value={values.email}
        />
        <Button disabled={busy} type="submit" variant="soft">
          {busy ? "Gerando..." : "Gerar token"}
        </Button>
      </form>

      <form className="form-grid" onSubmit={onConfirm}>
        <div className="split-fields">
          <TextInput
            label="Token"
            onChange={(event) => onChange("token", event.target.value)}
            value={values.token}
          />
          <TextInput
            label="Nova senha"
            onChange={(event) => onChange("novaSenha", event.target.value)}
            type="password"
            value={values.novaSenha}
          />
        </div>
        <div className="auth-actions">
          <button className="auth-link" onClick={onBackToLogin} type="button">
            Fazer login
          </button>
          <span className="app-nav-meta">
            {tokenIssued ? "Token gerado para a conta informada" : "Gere o token antes de confirmar"}
          </span>
        </div>
        <Button disabled={busy || !tokenIssued} type="submit" variant="secondary">
          Confirmar redefinicao
        </Button>
      </form>
    </div>
  );
}
