export function formatStatus(status: string) {
  const normalized = status.trim().toLowerCase();

  if (normalized === "ativo") {
    return "Ativo";
  }

  if (normalized === "inativo") {
    return "Inativo";
  }

  if (normalized === "bloqueado") {
    return "Bloqueado";
  }

  return status;
}

export function getStatusTone(status: string) {
  const normalized = status.trim().toLowerCase();

  if (normalized === "ativo") {
    return "success";
  }

  if (normalized === "bloqueado") {
    return "danger";
  }

  if (normalized === "inativo") {
    return "warning";
  }

  return "neutral";
}

export function getInitials(name: string) {
  const parts = name
    .split(" ")
    .map((part) => part.trim())
    .filter(Boolean)
    .slice(0, 2);

  return parts.map((part) => part[0]?.toUpperCase() ?? "").join("");
}

export function getErrorMessage(error: unknown) {
  if (error instanceof Error) {
    return error.message;
  }

  return "Nao foi possivel concluir a operacao.";
}
