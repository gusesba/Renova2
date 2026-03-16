import type { FormEvent } from "react";

import { Button } from "@/components/ui/button";
import { TextInput } from "@/components/ui/field";

type LoginFormProps = {
  values: {
    email: string;
    senha: string;
  };
  busy: boolean;
  onChange: (field: "email" | "senha", value: string) => void;
  onSubmit: (event: FormEvent<HTMLFormElement>) => void;
  onToggleReset: () => void;
};

export function LoginForm({
  values,
  busy,
  onChange,
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
      </div>
      <Button disabled={busy} fullWidth type="submit">
        {busy ? "Entrando..." : "Login"}
      </Button>
    </form>
  );
}
