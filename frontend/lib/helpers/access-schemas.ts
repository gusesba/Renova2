import { z } from "zod";

export const loginSchema = z.object({
  email: z.email("Informe um email valido."),
  senha: z.string().trim().min(8, "A senha deve ter ao menos 8 caracteres."),
});

export const passwordResetRequestSchema = z.object({
  email: z.email("Informe um email valido."),
});

export const passwordResetConfirmSchema = z.object({
  token: z.string().trim().min(1, "Informe o token de recuperacao."),
  novaSenha: z.string().trim().min(8, "A nova senha deve ter ao menos 8 caracteres."),
});

export const userFormSchema = z.object({
  id: z.string(),
  nome: z.string().trim().min(1, "Informe o nome do usuario."),
  email: z.email("Informe um email valido."),
  telefone: z.string().trim().min(1, "Informe o telefone do usuario."),
  senha: z.string(),
  statusUsuario: z.enum(["ativo", "inativo", "bloqueado"]),
});

export const createUserFormSchema = userFormSchema.extend({
  senha: z.string().trim().min(8, "A senha inicial deve ter ao menos 8 caracteres."),
});

export const roleFormSchema = z.object({
  id: z.string(),
  nome: z.string().trim().min(1, "Informe o nome do cargo."),
  descricao: z.string().trim().min(1, "Informe a descricao do cargo."),
  permissaoIds: z.array(z.string().trim().min(1)).min(1, "Selecione ao menos uma permissao."),
});

export const membershipFormSchema = z.object({
  id: z.string(),
  usuarioId: z.string().trim().min(1, "Selecione um usuario."),
  cargoIds: z.array(z.string().trim().min(1)).min(1, "Selecione ao menos um cargo."),
});

export function getZodErrorMessage(error: z.ZodError) {
  return error.issues[0]?.message ?? "Dados invalidos.";
}
