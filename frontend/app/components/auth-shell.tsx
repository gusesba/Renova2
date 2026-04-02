"use client";

import { FormEvent, useState } from "react";
import { z } from "zod";
import { toast } from "sonner";

type AuthMode = "login" | "cadastro";

type FormValues = {
  nome: string;
  email: string;
  senha: string;
};

type FieldErrors = Partial<Record<keyof FormValues, string>>;

type UsuarioTokenResponse = {
  usuario: {
    id: number;
    nome: string;
    email: string;
  };
  token: string;
};

const loginSchema = z.object({
  email: z.email("Informe um e-mail valido."),
  senha: z
    .string()
    .min(1, "Informe a senha.")
    .min(6, "A senha deve ter pelo menos 6 caracteres."),
});

const apiBaseUrl =
  process.env.NEXT_PUBLIC_API_URL?.replace(/\/$/, "") ?? "http://localhost:5268";

const cadastroSchema = z.object({
  nome: z.string().trim().min(1, "Informe o nome."),
  email: z.email("Informe um e-mail valido."),
  senha: z
    .string()
    .min(1, "Informe a senha.")
    .min(6, "A senha deve ter pelo menos 6 caracteres."),
});

const fieldLabel: Record<keyof FormValues, string> = {
  nome: "Nome",
  email: "E-mail",
  senha: "Senha",
};

const initialValues: FormValues = {
  nome: "",
  email: "",
  senha: "",
};

function getSchema(mode: AuthMode) {
  return mode === "login" ? loginSchema : cadastroSchema;
}

function mapZodErrors(error: z.ZodError): FieldErrors {
  const mapped: FieldErrors = {};

  for (const issue of error.issues) {
    const field = issue.path[0];

    if (
      typeof field === "string" &&
      !mapped[field as keyof FormValues] &&
      field in fieldLabel
    ) {
      mapped[field as keyof FormValues] = issue.message;
    }
  }

  return mapped;
}

function extractApiMessage(body: unknown): string | null {
  if (!body || typeof body !== "object") {
    return null;
  }

  const data = body as {
    mensagem?: unknown;
    title?: unknown;
    errors?: Record<string, string[] | undefined>;
  };

  if (typeof data.mensagem === "string" && data.mensagem.trim()) {
    return data.mensagem;
  }

  if (typeof data.title === "string" && data.title.trim()) {
    return data.title;
  }

  if (data.errors) {
    const firstError = Object.values(data.errors).flat().find(Boolean);
    if (firstError) {
      return firstError;
    }
  }

  return null;
}

function extractApiFieldErrors(body: unknown): FieldErrors {
  if (!body || typeof body !== "object" || !("errors" in body)) {
    return {};
  }

  const errors = (body as { errors?: Record<string, string[] | undefined> }).errors;
  if (!errors) {
    return {};
  }

  const mappedEntries = Object.entries(errors)
    .map(([key, value]) => {
      const normalizedKey = key.toLowerCase();
      const formKey = (Object.keys(fieldLabel) as Array<keyof FormValues>).find(
        (item) => item.toLowerCase() === normalizedKey,
      );

      return formKey && value?.[0] ? [formKey, value[0]] : null;
    })
    .filter(Boolean) as Array<[keyof FormValues, string]>;

  return Object.fromEntries(mappedEntries);
}

export function AuthShell() {
  const [mode, setMode] = useState<AuthMode>("login");
  const [values, setValues] = useState<FormValues>(initialValues);
  const [errors, setErrors] = useState<FieldErrors>({});
  const [isSubmitting, setIsSubmitting] = useState(false);

  function handleModeChange(nextMode: AuthMode) {
    setMode(nextMode);
    setErrors({});
    setValues((current) => ({
      ...current,
      nome: nextMode === "login" ? "" : current.nome,
      senha: "",
    }));
  }

  function updateField(field: keyof FormValues, value: string) {
    setValues((current) => ({
      ...current,
      [field]: value,
    }));

    setErrors((current) => ({
      ...current,
      [field]: undefined,
    }));
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    const payload =
      mode === "login"
        ? { email: values.email.trim(), senha: values.senha }
        : {
            nome: values.nome.trim(),
            email: values.email.trim(),
            senha: values.senha,
          };

    const validation = getSchema(mode).safeParse(payload);

    if (!validation.success) {
      const mappedErrors = mapZodErrors(validation.error);
      setErrors(mappedErrors);
      toast.error("Corrija os campos destacados antes de continuar.");
      return;
    }

    setErrors({});
    setIsSubmitting(true);

    try {
      const response = await fetch(`${apiBaseUrl}/api/auth/${mode}`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(validation.data),
      });

      const contentType = response.headers.get("content-type") ?? "";
      const body = contentType.includes("application/json")
        ? ((await response.json()) as unknown)
        : null;

      if (!response.ok) {
        const apiFieldErrors = extractApiFieldErrors(body);
        if (Object.keys(apiFieldErrors).length > 0) {
          setErrors(apiFieldErrors);
        }

        toast.error(extractApiMessage(body) ?? "Nao foi possivel concluir a solicitacao.");
        return;
      }

      const result = body as UsuarioTokenResponse;

      localStorage.setItem("renova.token", result.token);
      localStorage.setItem("renova.usuario", JSON.stringify(result.usuario));

      toast.success(
        mode === "login"
          ? `Login realizado com sucesso. Bem-vindo, ${result.usuario.nome}.`
          : `Cadastro realizado com sucesso. Bem-vindo, ${result.usuario.nome}.`,
      );

      if (mode === "cadastro") {
        setMode("login");
        setValues({
          nome: "",
          email: result.usuario.email,
          senha: "",
        });
        return;
      }

      setValues((current) => ({
        ...current,
        senha: "",
      }));
    } catch {
      toast.error("Nao foi possivel conectar ao backend. Verifique se a API esta em execucao.");
    } finally {
      setIsSubmitting(false);
    }
  }

  const isLogin = mode === "login";

  return (
    <main className="flex min-h-screen items-center justify-center overflow-hidden bg-[radial-gradient(circle_at_top_left,_#b7c4ff_0%,_#8f99f3_32%,_#6c63ef_68%,_#5a52eb_100%)] px-5 py-10">
      <div className="w-full max-w-5xl overflow-hidden rounded-[32px] bg-white shadow-[0_30px_80px_rgba(55,35,143,0.24)]">
        <div className="grid min-h-[640px] lg:grid-cols-[1.05fr_1fr]">
          <section className="relative flex flex-col justify-between bg-[#eef1ff] p-8 sm:p-10 lg:p-12">
            <div>
              <span className="inline-flex rounded-full bg-white/80 px-4 py-2 text-sm font-semibold tracking-[0.18em] text-[#5f5af1] uppercase shadow-sm">
                Renova
              </span>
            </div>

            <div className="relative mx-auto flex w-full max-w-md items-center justify-center py-8">
              <div className="absolute inset-x-10 top-1/2 h-52 -translate-y-1/2 rounded-full bg-[radial-gradient(circle,_rgba(111,103,242,0.18)_0%,_rgba(111,103,242,0)_72%)] blur-2xl" />
              <Illustration />
            </div>

            <div className="space-y-3 text-[#544cbf]">
              <p className="text-3xl font-semibold leading-tight sm:text-4xl">
                Renova
              </p>
              <p className="max-w-sm text-sm leading-6 text-[#7c76c9] sm:text-base">
                Acesse sua conta e organize sua rotina com uma experiencia leve,
                limpa e direta.
              </p>
            </div>
          </section>

          <section className="flex items-center bg-white px-6 py-8 sm:px-10 lg:px-12">
            <div className="mx-auto w-full max-w-md">
              <div className="mb-8 space-y-3">
                <h1 className="text-3xl font-semibold tracking-[-0.03em] text-[#5a52eb] sm:text-4xl">
                  {isLogin ? "Bem-vindo a Renova!" : "Crie sua conta na Renova!"}
                </h1>
                <p className="text-sm leading-6 text-[#7f79c8] sm:text-base">
                  {isLogin
                    ? "Digite seus dados para acessar."
                    : "Preencha os campos para criar seu acesso."}
                </p>
              </div>

              <form className="space-y-5" onSubmit={handleSubmit} noValidate>
                {!isLogin ? (
                  <Field
                    label="Nome"
                    name="nome"
                    placeholder="Seu nome"
                    value={values.nome}
                    onChange={updateField}
                    error={errors.nome}
                    autoComplete="name"
                  />
                ) : null}

                <Field
                  label="E-mail"
                  name="email"
                  placeholder="voce@renova.com"
                  type="email"
                  value={values.email}
                  onChange={updateField}
                  error={errors.email}
                  autoComplete="email"
                />

                <Field
                  label="Senha"
                  name="senha"
                  placeholder="••••••••"
                  type="password"
                  value={values.senha}
                  onChange={updateField}
                  error={errors.senha}
                  autoComplete={isLogin ? "current-password" : "new-password"}
                />

                <button
                  type="submit"
                  disabled={isSubmitting}
                  className="flex h-13 w-full items-center justify-center rounded-2xl bg-[linear-gradient(90deg,_#6a63f4,_#5a52eb)] px-4 text-sm font-semibold text-white shadow-[0_16px_28px_rgba(91,83,235,0.34)] transition hover:brightness-105 disabled:cursor-not-allowed disabled:opacity-70"
                >
                  {isSubmitting
                    ? isLogin
                      ? "Entrando..."
                      : "Cadastrando..."
                    : isLogin
                      ? "Login"
                      : "Cadastrar"}
                </button>
              </form>

              <p className="mt-5 text-center text-sm text-[#7d76cb]">
                {isLogin ? "Nao tem conta?" : "Ja tem uma conta?"}{" "}
                <button
                  type="button"
                  onClick={() => handleModeChange(isLogin ? "cadastro" : "login")}
                  className="font-semibold text-[#5b53eb] transition hover:text-[#473fd7]"
                >
                  {isLogin ? "Cadastrar" : "Login"}
                </button>
              </p>
            </div>
          </section>
        </div>
      </div>
    </main>
  );
}

type FieldProps = {
  autoComplete?: string;
  error?: string;
  label: string;
  name: keyof FormValues;
  onChange: (field: keyof FormValues, value: string) => void;
  placeholder: string;
  type?: string;
  value: string;
};

function Field({
  autoComplete,
  error,
  label,
  name,
  onChange,
  placeholder,
  type = "text",
  value,
}: FieldProps) {
  return (
    <label className="block space-y-2">
      <span className="text-sm font-medium text-[#867fce]">{label}</span>
      <input
        name={name}
        type={type}
        value={value}
        autoComplete={autoComplete}
        placeholder={placeholder}
        onChange={(event) => onChange(name, event.target.value)}
        className={`h-13 w-full rounded-2xl border bg-white px-4 text-sm text-[#2d2464] outline-none transition placeholder:text-[#b1add8] focus:border-[#6a63f4] focus:ring-4 focus:ring-[#6a63f4]/15 ${
          error ? "border-[#e86f8f]" : "border-[#dedaf8]"
        }`}
      />
      {error ? <span className="text-xs text-[#d25378]">{error}</span> : null}
    </label>
  );
}

function Illustration() {
  return (
    <div className="relative w-full max-w-[380px] rounded-[32px] border border-white/70 bg-[linear-gradient(180deg,_rgba(255,255,255,0.75),_rgba(224,229,255,0.92))] p-6 shadow-[0_30px_55px_rgba(111,103,242,0.16)]">
      <div className="absolute left-6 top-6 flex gap-2">
        <span className="h-3 w-3 rounded-full bg-[#ff8b7b]" />
        <span className="h-3 w-3 rounded-full bg-[#ffd46f]" />
        <span className="h-3 w-3 rounded-full bg-[#75d39b]" />
      </div>

      <div className="mt-8 rounded-[24px] bg-[linear-gradient(180deg,_#766ff7,_#4f44d7)] p-4 shadow-[0_20px_35px_rgba(79,68,215,0.3)]">
        <div className="mb-4 flex items-center justify-between">
          <div className="h-3 w-24 rounded-full bg-white/30" />
          <div className="h-8 w-8 rounded-xl bg-white/18" />
        </div>
        <div className="grid grid-cols-[1.15fr_0.85fr] gap-4">
          <div className="space-y-3">
            <div className="h-24 rounded-[18px] bg-white/16 p-4">
              <div className="h-3 w-18 rounded-full bg-white/30" />
              <div className="mt-4 space-y-2">
                <div className="h-2 w-full rounded-full bg-white/20" />
                <div className="h-2 w-4/5 rounded-full bg-white/20" />
                <div className="h-2 w-2/3 rounded-full bg-white/20" />
              </div>
            </div>
            <div className="flex h-24 items-end gap-2 rounded-[18px] bg-white/14 p-4">
              <div className="w-full rounded-full bg-[#ff8d7a] h-8" />
              <div className="w-full rounded-full bg-[#ff8d7a] h-12" />
              <div className="w-full rounded-full bg-[#9fd0ff] h-16" />
              <div className="w-full rounded-full bg-[#ff8d7a] h-10" />
            </div>
          </div>
          <div className="flex flex-col gap-3">
            <div className="flex-1 rounded-[18px] bg-white/16 p-4">
              <div className="grid grid-cols-2 gap-2">
                <div className="h-12 rounded-2xl bg-white/18" />
                <div className="h-12 rounded-2xl bg-[#ff8d7a]" />
                <div className="col-span-2 h-16 rounded-[18px] bg-[#8eb5ff]" />
              </div>
            </div>
            <div className="flex h-20 items-center justify-center rounded-[18px] bg-white/14">
              <div className="flex items-end gap-2">
                <div className="h-9 w-9 rounded-t-[12px] bg-[#ff8d7a]" />
                <div className="h-14 w-9 rounded-t-[12px] bg-[#ffd26d]" />
                <div className="h-11 w-9 rounded-t-[12px] bg-[#9fd0ff]" />
              </div>
            </div>
          </div>
        </div>
      </div>

      <div className="absolute -left-5 bottom-6 h-20 w-20 rounded-[24px] bg-[#ff8d7a] p-4 shadow-[0_18px_30px_rgba(255,141,122,0.35)]">
        <div className="h-full rounded-[18px] border border-white/35">
          <div className="mx-auto mt-3 h-2 w-8 rounded-full bg-white/70" />
          <div className="mx-auto mt-3 h-7 w-7 rounded-full bg-white/80" />
        </div>
      </div>

      <div className="absolute -right-4 top-20 h-18 w-18 rounded-[22px] bg-white p-4 shadow-[0_16px_28px_rgba(111,103,242,0.18)]">
        <div className="space-y-2">
          <div className="h-2 w-8 rounded-full bg-[#6c63ef]" />
          <div className="h-2 w-10 rounded-full bg-[#d6d9fa]" />
          <div className="h-8 rounded-2xl bg-[#eff1ff]" />
        </div>
      </div>
    </div>
  );
}
