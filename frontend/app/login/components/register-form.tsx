import type { SubmitEvent } from "react";

import { Button } from "@/components/ui/button";
import { TextInput } from "@/components/ui/field";

// Formulario de auto cadastro publico exibido dentro da tela de autenticacao.
type RegisterFormProps = {
  values: {
    nome: string;
    email: string;
    telefone: string;
    senha: string;
    confirmacaoSenha: string;
  };
  busy: boolean;
  onChange: (
    field: "nome" | "email" | "telefone" | "senha" | "confirmacaoSenha",
    value: string,
  ) => void;
  onSubmit: (event: SubmitEvent<HTMLFormElement>) => void;
  onBackToLogin: () => void;
};

export function RegisterForm({
  values,
  busy,
  onChange,
  onSubmit,
  onBackToLogin,
}: RegisterFormProps) {
  return (
    <div className="section-stack">
      <div>
        <h2 className="auth-title" style={{ fontSize: "2rem" }}>
          Criar conta
        </h2>
        <p className="auth-subtitle">
          Cadastre seus dados basicos para criar seu acesso publico na
          plataforma.
        </p>
      </div>

      <form className="auth-form-stack" onSubmit={onSubmit}>
        <TextInput
          autoComplete="name"
          label="Nome"
          onChange={(event) => onChange("nome", event.target.value)}
          placeholder="Seu nome completo"
          value={values.nome}
        />
        <TextInput
          autoComplete="email"
          label="Email"
          onChange={(event) => onChange("email", event.target.value)}
          placeholder="seu@email.com"
          value={values.email}
        />
        <TextInput
          autoComplete="tel"
          label="Telefone"
          onChange={(event) => onChange("telefone", event.target.value)}
          placeholder="(00) 00000-0000"
          value={values.telefone}
        />
        <div className="split-fields">
          <TextInput
            autoComplete="new-password"
            label="Senha"
            onChange={(event) => onChange("senha", event.target.value)}
            placeholder="Crie sua senha"
            type="password"
            value={values.senha}
          />
          <TextInput
            autoComplete="new-password"
            label="Confirmar senha"
            onChange={(event) => onChange("confirmacaoSenha", event.target.value)}
            placeholder="Repita sua senha"
            type="password"
            value={values.confirmacaoSenha}
          />
        </div>
        <div className="auth-actions">
          <button className="auth-link" onClick={onBackToLogin} type="button">
            Fazer login
          </button>
        </div>
        <Button disabled={busy} fullWidth type="submit">
          {busy ? "Criando conta..." : "Criar conta"}
        </Button>
      </form>
    </div>
  );
}
