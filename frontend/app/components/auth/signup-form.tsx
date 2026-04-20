import { FormEvent } from "react";

import type { AccessArea } from "@/lib/access-area";
import type { FieldErrors, FormValues } from "@/lib/auth";

import { AuthField } from "./shared-field";

type CadastroFormProps = {
  accessArea: AccessArea;
  errors: FieldErrors;
  isSubmitting: boolean;
  values: FormValues;
  onAccessAreaChange: (area: AccessArea) => void;
  onFieldChange: (field: keyof FormValues, value: string) => void;
  onModeChange: (mode: "login") => void;
  onSubmit: () => Promise<void>;
};

export function CadastroForm({
  accessArea,
  errors,
  isSubmitting,
  values,
  onAccessAreaChange,
  onFieldChange,
  onModeChange,
  onSubmit,
}: CadastroFormProps) {
  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    await onSubmit();
  }

  return (
    <>
      <div className="mb-6 space-y-3 sm:mb-8">
        <h1 className="text-[1.85rem] font-semibold tracking-[-0.03em] text-[#5a52eb] sm:text-4xl">
          Crie sua conta na Renova!
        </h1>
        <p className="text-sm leading-6 text-[#7f79c8] sm:text-base">
          Preencha os campos para criar seu acesso.
        </p>
      </div>

      <form className="space-y-5" onSubmit={handleSubmit} noValidate>
        <div className="rounded-[20px] border border-[#d9ddfb] bg-[#f4f6ff] p-2 sm:rounded-[24px]">
          <p className="px-2 pb-2 text-xs font-semibold uppercase tracking-[0.16em] text-[#7f79c8]">
            Area de acesso
          </p>
          <div className="grid grid-cols-2 gap-2">
            {[
              { label: "Lojista", value: "lojista" as const },
              { label: "Cliente", value: "cliente" as const },
            ].map((item) => {
              const active = accessArea === item.value;

              return (
                <button
                  key={item.value}
                  type="button"
                  onClick={() => onAccessAreaChange(item.value)}
                  className={`rounded-2xl px-3 py-3 text-sm font-semibold transition sm:px-4 ${
                    active
                      ? "bg-[linear-gradient(90deg,_#6a63f4,_#5a52eb)] text-white shadow-[0_12px_24px_rgba(91,83,235,0.26)]"
                      : "bg-white text-[#6f69bb] hover:bg-[#eef1ff]"
                  }`}
                >
                  {item.label}
                </button>
              );
            })}
          </div>
        </div>

        <AuthField
          label="Nome"
          name="nome"
          placeholder="Seu nome"
          value={values.nome}
          onChange={onFieldChange}
          error={errors.nome}
          autoComplete="name"
        />

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
          autoComplete="new-password"
        />

        <button
          type="submit"
          disabled={isSubmitting}
          className="flex h-13 w-full items-center justify-center rounded-2xl bg-[linear-gradient(90deg,_#6a63f4,_#5a52eb)] px-4 text-sm font-semibold text-white shadow-[0_16px_28px_rgba(91,83,235,0.34)] transition hover:brightness-105 disabled:cursor-not-allowed disabled:opacity-70"
        >
          {isSubmitting ? "Cadastrando..." : "Cadastrar"}
        </button>
      </form>

      <p className="mt-5 text-center text-sm text-[#7d76cb]">
        Ja tem uma conta?{" "}
        <button
          type="button"
          onClick={() => onModeChange("login")}
          className="font-semibold text-[#5b53eb] transition hover:text-[#473fd7]"
        >
          Login
        </button>
      </p>
    </>
  );
}
