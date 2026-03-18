// Traduz valores tecnicos de status para textos e tons usados pela interface.
export function formatStatus(status: string) {
  const normalized = status.trim().toLowerCase();
  const labels: Record<string, string> = {
    ajuste: "Ajuste",
    cancelamento_venda: "Cancelamento de venda",
    consignada: "Consignada",
    descartada: "Descartada",
    devolucao: "Devolucao",
    devolvida: "Devolvida",
    disponivel: "Disponivel",
    doacao: "Doacao",
    doada: "Doada",
    entrada: "Entrada",
    fixa: "Fixa",
    inativa: "Inativa",
    lote: "Lote",
    perdida: "Perdida",
    reservado: "Reservado",
    reservada: "Reservada",
    venda: "Venda",
    vendida: "Vendida",
  };

  if (labels[normalized]) {
    return labels[normalized];
  }

  if (normalized === "ativo") {
    return "Ativo";
  }

  if (normalized === "inativo") {
    return "Inativo";
  }

  if (normalized === "entrada") {
    return "Entrada";
  }

  if (normalized === "saida") {
    return "Saida";
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

  if (normalized === "entrada") {
    return "success";
  }

  if (normalized === "disponivel") {
    return "success";
  }

  if (normalized === "vendida") {
    return "danger";
  }

  if (normalized === "devolvida") {
    return "warning";
  }

  if (normalized === "doada" || normalized === "descartada") {
    return "warning";
  }

  if (normalized === "perdida") {
    return "danger";
  }

  if (normalized === "reservada" || normalized === "ajuste") {
    return "neutral";
  }

  if (normalized === "cancelamento_venda") {
    return "warning";
  }

  if (normalized === "bloqueado") {
    return "danger";
  }

  if (normalized === "saida") {
    return "danger";
  }

  if (normalized === "inativo") {
    return "warning";
  }

  return "neutral";
}

export function getInitials(name: string) {
  // Limita a duas iniciais para manter o avatar compacto no header.
  const parts = name
    .split(" ")
    .map((part) => part.trim())
    .filter(Boolean)
    .slice(0, 2);

  return parts.map((part) => part[0]?.toUpperCase() ?? "").join("");
}

// Formata valores monetarios em real para os paineis do sistema.
export function formatCurrency(value: number) {
  return new Intl.NumberFormat("pt-BR", {
    style: "currency",
    currency: "BRL",
  }).format(value);
}

// Formata data e hora da API para exibicao resumida.
export function formatDateTime(value?: string | null) {
  if (!value) {
    return "Sem registro";
  }

  return new Intl.DateTimeFormat("pt-BR", {
    dateStyle: "short",
    timeStyle: "short",
  }).format(new Date(value));
}

export function getErrorMessage(error: unknown) {
  if (error instanceof Error) {
    return error.message;
  }

  return "Nao foi possivel concluir a operacao.";
}
