import { FormEvent } from "react";

import type { FieldErrors, FormValues } from "@/lib/auth";

import { AuthField } from "./shared-field";

type LoginFormProps = {
  errors: FieldErrors;
  isSubmitting: boolean;
  values: FormValues;
  onFieldChange: (field: keyof FormValues, value: string) => void;
  onModeChange: (mode: "cadastro") => void;
  onSubmit: () => Promise<void>;
};

export function LoginForm({
  errors,
  isSubmitting,
  values,
  onFieldChange,
  onModeChange,
  onSubmit,
}: LoginFormProps) {
  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    await onSubmit();
  }

  return (
    <>
      <div className="mb-8 space-y-3">
        <h1 className="text-3xl font-semibold tracking-[-0.03em] text-[#5a52eb] sm:text-4xl">
          Bem-vindo a Renova!
        </h1>
        <p className="text-sm leading-6 text-[#7f79c8] sm:text-base">
          Digite seus dados para acessar.
        </p>
      </div>

      <form className="space-y-5" onSubmit={handleSubmit} noValidate>
        <AuthField
          label="E-mail"
          name="email"
          placeholder="voce@renova.com"
          type="email"
          value={values.email}
          onChange={onFieldChange}
          error={errors.email}
          autoComplete="email"
        />

        <AuthField
          label="Senha"
          name="senha"
          placeholder="••••••••"
          type="password"
          value={values.senha}
          onChange={onFieldChange}
          error={errors.senha}
          autoComplete="current-password"
        />

        <button
          type="submit"
          disabled={isSubmitting}
          className="flex h-13 w-full items-center justify-center rounded-2xl bg-[linear-gradient(90deg,_#6a63f4,_#5a52eb)] px-4 text-sm font-semibold text-white shadow-[0_16px_28px_rgba(91,83,235,0.34)] transition hover:brightness-105 disabled:cursor-not-allowed disabled:opacity-70"
        >
          {isSubmitting ? "Entrando..." : "Login"}
        </button>
      </form>

      <p className="mt-5 text-center text-sm text-[#7d76cb]">
        Nao tem conta?{" "}
        <button
          type="button"
          onClick={() => onModeChange("cadastro")}
          className="font-semibold text-[#5b53eb] transition hover:text-[#473fd7]"
        >
          Cadastrar
        </button>
      </p>
    </>
  );
}
