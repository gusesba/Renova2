import type { SubmitEvent } from "react";

import { Button } from "@/components/ui/button";
import { TextInput } from "@/components/ui/field";

// Formulario enxuto de login; toda regra de negocio fica no componente pai.
type LoginFormProps = {
  values: {
    email: string;
    senha: string;
  };
  busy: boolean;
  onChange: (field: "email" | "senha", value: string) => void;
  onSubmit: (event: SubmitEvent<HTMLFormElement>) => void;
  onCreateAccount: () => void;
  onToggleReset: () => void;
};

export function LoginForm({
  values,
  busy,
  onChange,
  onCreateAccount,
  onSubmit,
  onToggleReset,
}: LoginFormProps) {
  return (
    <form className="auth-form-stack" onSubmit={onSubmit}>
      <TextInput
        autoComplete="email"
        label="Email"
        onChange={(event) => onChange("email", event.target.value)}
        placeholder="seu@email.com"
        value={values.email}
      />
      <TextInput
        autoComplete="current-password"
        label="Senha"
        onChange={(event) => onChange("senha", event.target.value)}
        placeholder="Informe sua senha"
        type="password"
        value={values.senha}
      />
      <div className="auth-actions">
        <button className="auth-link" onClick={onToggleReset} type="button">
          Esqueceu a senha?
        </button>
        <button className="auth-link" onClick={onCreateAccount} type="button">
          Criar conta
        </button>
      </div>
      <Button disabled={busy} fullWidth type="submit">
        {busy ? "Entrando..." : "Login"}
      </Button>
    </form>
  );
}
