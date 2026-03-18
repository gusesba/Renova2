import { callApi } from "@/lib/services/core/api-client";

import type {
  PieceDetail,
  PieceImage,
  PieceSummary,
  PieceWorkspace,
} from "./contracts";

// Reune as operacoes HTTP do modulo 06.
export async function getPiecesWorkspace(token: string) {
  return callApi<PieceWorkspace>("/pieces/workspace", { method: "GET" }, token);
}

export async function listPieces(
  token: string,
  filters: {
    search?: string;
    codigoBarras?: string;
    statusPeca?: string;
    produtoNomeId?: string;
    marcaId?: string;
    fornecedorPessoaId?: string;
  },
) {
  const params = new URLSearchParams();
  if (filters.search) {
    params.set("search", filters.search);
  }
  if (filters.codigoBarras) {
    params.set("codigoBarras", filters.codigoBarras);
  }
  if (filters.statusPeca) {
    params.set("statusPeca", filters.statusPeca);
  }
  if (filters.produtoNomeId) {
    params.set("produtoNomeId", filters.produtoNomeId);
  }
  if (filters.marcaId) {
    params.set("marcaId", filters.marcaId);
  }
  if (filters.fornecedorPessoaId) {
    params.set("fornecedorPessoaId", filters.fornecedorPessoaId);
  }

  const queryString = params.toString();
  return callApi<PieceSummary[]>(
    `/pieces${queryString ? `?${queryString}` : ""}`,
    { method: "GET" },
    token,
  );
}

export async function getPieceById(token: string, pecaId: string) {
  return callApi<PieceDetail>(`/pieces/${pecaId}`, { method: "GET" }, token);
}

export async function createPiece(
  token: string,
  payload: {
    tipoPeca: string;
    codigoBarras: string;
    produtoNomeId: string;
    marcaId: string;
    tamanhoId: string;
    corId: string;
    fornecedorPessoaId?: string | null;
    descricao: string;
    observacoes: string;
    dataEntrada?: string | null;
    quantidadeInicial: number;
    precoVendaAtual: number;
    custoUnitario?: number | null;
    localizacaoFisica: string;
    regraManual?: {
      percentualRepasseDinheiro: number;
      percentualRepasseCredito: number;
      permitePagamentoMisto: boolean;
      tempoMaximoExposicaoDias: number;
      politicaDesconto: Array<{
        diasMinimos: number;
        percentualDesconto: number;
      }>;
    } | null;
  },
) {
  return callApi<PieceDetail>(
    "/pieces",
    {
      method: "POST",
      body: JSON.stringify(payload),
    },
    token,
  );
}

export async function updatePiece(
  token: string,
  pecaId: string,
  payload: {
    tipoPeca: string;
    codigoBarras: string;
    produtoNomeId: string;
    marcaId: string;
    tamanhoId: string;
    corId: string;
    fornecedorPessoaId?: string | null;
    descricao: string;
    observacoes: string;
    dataEntrada?: string | null;
    precoVendaAtual: number;
    custoUnitario?: number | null;
    localizacaoFisica: string;
    regraManual?: {
      percentualRepasseDinheiro: number;
      percentualRepasseCredito: number;
      permitePagamentoMisto: boolean;
      tempoMaximoExposicaoDias: number;
      politicaDesconto: Array<{
        diasMinimos: number;
        percentualDesconto: number;
      }>;
    } | null;
  },
) {
  return callApi<PieceDetail>(
    `/pieces/${pecaId}`,
    {
      method: "PUT",
      body: JSON.stringify(payload),
    },
    token,
  );
}

export async function uploadPieceImage(
  token: string,
  pecaId: string,
  payload: {
    arquivo: File;
    ordem: number;
    tipoVisibilidade: string;
  },
) {
  const formData = new FormData();
  formData.set("arquivo", payload.arquivo);
  formData.set("ordem", String(payload.ordem));
  formData.set("tipoVisibilidade", payload.tipoVisibilidade);

  return callApi<PieceImage>(
    `/pieces/${pecaId}/images`,
    {
      method: "POST",
      body: formData,
    },
    token,
  );
}

export async function updatePieceImage(
  token: string,
  pecaId: string,
  imageId: string,
  payload: {
    ordem: number;
    tipoVisibilidade: string;
  },
) {
  return callApi<PieceImage>(
    `/pieces/${pecaId}/images/${imageId}`,
    {
      method: "PUT",
      body: JSON.stringify(payload),
    },
    token,
  );
}

export async function deletePieceImage(
  token: string,
  pecaId: string,
  imageId: string,
) {
  return callApi<PieceImage>(
    `/pieces/${pecaId}/images/${imageId}`,
    {
      method: "DELETE",
    },
    token,
  );
}
