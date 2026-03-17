"use client";

import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useState, type SubmitEvent } from "react";
import { toast } from "sonner";

import {
  PasswordFormPanel,
  type PasswordFormState,
} from "@/app/(system)/profile/components/password-form-panel";
import { ProfileFormPanel, type ProfileFormState } from "@/app/(system)/profile/components/profile-form-panel";
import { useSystemSession } from "@/app/(system)/components/system-session-provider";
import { getErrorMessage } from "@/lib/helpers/formatters";
import { queryKeys } from "@/lib/helpers/query-keys";
import {
  changePasswordSchema,
  getZodErrorMessage,
  profileUpdateSchema,
} from "@/lib/schemas/access";
import { changePassword, updateUser } from "@/lib/services/access";

// Container da tela de perfil com mutacao do proprio usuario.
export function ProfileDashboard() {
  const { token, session } = useSystemSession();
  const queryClient = useQueryClient();
  const [form, setForm] = useState<ProfileFormState>({
    nome: session.usuario.nome,
    email: session.usuario.email,
    telefone: session.usuario.telefone,
  });
  const [passwordForm, setPasswordForm] = useState<PasswordFormState>({
    senhaAtual: "",
    novaSenha: "",
    confirmacaoNovaSenha: "",
  });

  const profileMutation = useMutation({
    mutationFn: async () => {
      const parsed = profileUpdateSchema.safeParse(form);
      if (!parsed.success) {
        throw new Error(getZodErrorMessage(parsed.error));
      }

      return updateUser(token, session.usuario.id, {
        nome: parsed.data.nome,
        email: parsed.data.email,
        telefone: parsed.data.telefone,
        pessoaId: null,
      });
    },
    onError: (error) => {
      toast.error(getErrorMessage(error));
    },
    onSuccess: async () => {
      toast.success("Perfil atualizado com sucesso.");
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: queryKeys.session(token) }),
        queryClient.invalidateQueries({ queryKey: ["access-workspace"] }),
      ]);
    },
  });

  const passwordMutation = useMutation({
    mutationFn: async () => {
      const parsed = changePasswordSchema.safeParse(passwordForm);
      if (!parsed.success) {
        throw new Error(getZodErrorMessage(parsed.error));
      }

      await changePassword(token, {
        senhaAtual: parsed.data.senhaAtual,
        novaSenha: parsed.data.novaSenha,
      });
    },
    onError: (error) => {
      toast.error(getErrorMessage(error));
    },
    onSuccess: () => {
      setPasswordForm({
        senhaAtual: "",
        novaSenha: "",
        confirmacaoNovaSenha: "",
      });
      toast.success("Senha alterada com sucesso.");
    },
  });

  async function handleSubmit(event: SubmitEvent<HTMLFormElement>) {
    event.preventDefault();
    await profileMutation.mutateAsync();
  }

  async function handlePasswordSubmit(event: SubmitEvent<HTMLFormElement>) {
    event.preventDefault();
    await passwordMutation.mutateAsync();
  }

  return (
    <div className="dashboard-column">
      <ProfileFormPanel
        busy={profileMutation.isPending}
        form={form}
        onSubmit={handleSubmit}
        setForm={setForm}
      />
      <PasswordFormPanel
        busy={passwordMutation.isPending}
        form={passwordForm}
        onSubmit={handlePasswordSubmit}
        setForm={setPasswordForm}
      />
    </div>
  );
}
