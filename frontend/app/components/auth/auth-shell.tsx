"use client";

import { useMutation } from "@tanstack/react-query";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { toast } from "sonner";

import {
  extractApiFieldErrors,
  extractApiMessage,
  initialValues,
  persistAuthSession,
  type AuthMode,
  type FieldErrors,
  type FormValues,
} from "@/lib/auth";
import { asUsuarioTokenResponse, authenticate } from "@/services/auth-service";
import { getSchema, mapZodErrors } from "@/validations/auth";

import { AuthLeftPanel } from "./left-panel";
import { AuthRightPanel } from "./right-panel";

export function AuthShell() {
  const router = useRouter();
  const [mode, setMode] = useState<AuthMode>("login");
  const [values, setValues] = useState<FormValues>(initialValues);
  const [errors, setErrors] = useState<FieldErrors>({});

  const authMutation = useMutation({
    mutationFn: async ({
      currentMode,
      payload,
    }: {
      currentMode: AuthMode;
      payload: { email: string; senha: string } | FormValues;
    }) => authenticate(currentMode, payload),
  });

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

  async function submitCurrentMode() {
    if (authMutation.isPending) {
      return;
    }

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

    try {
      const response = await authMutation.mutateAsync({
        currentMode: mode,
        payload: validation.data,
      });

      if (!response.ok) {
        const apiFieldErrors = extractApiFieldErrors(response.body);
        if (Object.keys(apiFieldErrors).length > 0) {
          setErrors(apiFieldErrors);
        }

        toast.error(extractApiMessage(response.body) ?? "Nao foi possivel concluir a solicitacao.");
        return;
      }

      const result = asUsuarioTokenResponse(response.body);

      persistAuthSession(result);

      toast.success(
        mode === "login"
          ? `Login realizado com sucesso. Bem-vindo, ${result.usuario.nome}.`
          : `Cadastro realizado com sucesso. Bem-vindo, ${result.usuario.nome}.`,
      );
      router.replace("/dashboard");
    } catch {
      toast.error("Nao foi possivel conectar ao backend. Verifique se a API esta em execucao.");
    }
  }

  return (
    <main className="flex min-h-screen items-center justify-center overflow-hidden bg-[radial-gradient(circle_at_top_left,_#b7c4ff_0%,_#8f99f3_32%,_#6c63ef_68%,_#5a52eb_100%)] px-5 py-10">
      <div className="w-full max-w-5xl overflow-hidden rounded-[32px] bg-white shadow-[0_30px_80px_rgba(55,35,143,0.24)]">
        <div className="grid min-h-[640px] lg:grid-cols-[1.05fr_1fr]">
          <AuthLeftPanel />
          <AuthRightPanel
            errors={errors}
            isSubmitting={authMutation.isPending}
            mode={mode}
            values={values}
            onFieldChange={updateField}
            onModeChange={handleModeChange}
            onSubmit={submitCurrentMode}
          />
        </div>
      </div>
    </main>
  );
}
