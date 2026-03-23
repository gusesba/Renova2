"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  startTransition,
  useDeferredValue,
  useEffect,
  useMemo,
  useState,
  type FormEvent,
  type SetStateAction,
} from "react";
import { toast } from "sonner";

import { useSystemSession } from "@/app/(system)/components/system-session-provider";
import { PieceFormPanel } from "@/app/(system)/pieces/components/piece-form-panel";
import { PieceImagesPanel } from "@/app/(system)/pieces/components/piece-images-panel";
import { PiecesListPanel } from "@/app/(system)/pieces/components/pieces-list-panel";
import { PiecesOverview } from "@/app/(system)/pieces/components/pieces-overview";
import {
  createEmptyPieceDiscountBand,
  emptyPieceFilters,
  emptyPieceForm,
  mapPieceDetailToForm,
  sortPieceImages,
  type PieceFiltersState,
  type PieceFormState,
} from "@/app/(system)/pieces/components/types";
import { AccessStateCard } from "@/components/ui/access-state-card";
import {
  accessPermissionCodes,
  hasAnyPermission,
  hasPermission,
} from "@/lib/helpers/access-control";
import { getErrorMessage } from "@/lib/helpers/formatters";
import { queryKeys } from "@/lib/helpers/query-keys";
import { getZodErrorMessage } from "@/lib/schemas/access";
import { pieceFormSchema, pieceManualRuleSchema } from "@/lib/schemas/pieces";
import {
  createPiece,
  deletePieceImage,
  getPieceById,
  getPiecesWorkspace,
  listPieces,
  updatePiece,
  updatePieceImage,
  uploadPieceImage,
} from "@/lib/services/pieces";

// Gera a data de hoje no formato aceito pelo input nativo de data.
function getTodayInputValue() {
  return new Date().toISOString().slice(0, 10);
}

// Monta os campos comuns do payload de peca.
function mapCommonFormPayload(form: PieceFormState) {
  const parsed = pieceFormSchema.safeParse(form);
  if (!parsed.success) {
    throw new Error(getZodErrorMessage(parsed.error));
  }

  const manualRule = parsed.data.usarRegraManual
    ? pieceManualRuleSchema.parse(parsed.data.regraManual)
    : null;

  return {
    tipoPeca: parsed.data.tipoPeca,
    codigoBarras: parsed.data.codigoBarras,
    produtoNomeId: parsed.data.produtoNomeId,
    marcaId: parsed.data.marcaId,
    tamanhoId: parsed.data.tamanhoId,
    corId: parsed.data.corId,
    fornecedorPessoaId: parsed.data.fornecedorPessoaId || null,
    descricao: parsed.data.descricao,
    observacoes: parsed.data.observacoes,
    dataEntrada: `${parsed.data.dataEntrada}T00:00:00Z`,
    precoVendaAtual: parsed.data.precoVendaAtual,
    custoUnitario: parsed.data.custoUnitario
      ? Number(parsed.data.custoUnitario)
      : null,
    localizacaoFisica: parsed.data.localizacaoFisica,
    regraManual: manualRule
      ? {
          percentualRepasseDinheiro: manualRule.percentualRepasseDinheiro,
          percentualRepasseCredito: manualRule.percentualRepasseCredito,
          permitePagamentoMisto: manualRule.permitePagamentoMisto,
          tempoMaximoExposicaoDias: manualRule.tempoMaximoExposicaoDias,
          politicaDesconto: manualRule.politicaDesconto.map((band) => ({
            diasMinimos: band.diasMinimos,
            percentualDesconto: band.percentualDesconto,
          })),
        }
      : null,
  };
}

// Converte o formulario da peca para o payload de criacao.
function mapFormToCreatePayload(form: PieceFormState) {
  const commonPayload = mapCommonFormPayload(form);
  const parsed = pieceFormSchema.parse(form);
  return {
    ...commonPayload,
    quantidadeInicial: parsed.quantidadeInicial,
  };
}

// Converte o formulario da peca para o payload de atualizacao.
function mapFormToUpdatePayload(form: PieceFormState) {
  return mapCommonFormPayload(form);
}

// Coordena a tela do modulo 06 com filtros, cadastro, detalhe e imagens.
export function PiecesDashboard() {
  const { token, session } = useSystemSession();
  const queryClient = useQueryClient();
  const [filters, setFilters] = useState<PieceFiltersState>(emptyPieceFilters);
  const deferredFilters = useDeferredValue(filters);
  const [selectedPieceId, setSelectedPieceId] = useState("");
  const [draftForm, setDraftForm] = useState<PieceFormState | null>(null);
  const canViewPieces = hasAnyPermission(session, [
    accessPermissionCodes.piecesView,
    accessPermissionCodes.piecesCreate,
  ]);
  const canManagePieces = hasPermission(session, accessPermissionCodes.piecesCreate);

  const workspaceQuery = useQuery({
    enabled: Boolean(session.lojaAtivaId && canViewPieces),
    queryFn: () => getPiecesWorkspace(token),
    queryKey: queryKeys.piecesWorkspace(token, session.lojaAtivaId),
  });

  const filtersKey = useMemo(
    () => JSON.stringify(deferredFilters),
    [deferredFilters],
  );

  const piecesQuery = useQuery({
    enabled: Boolean(session.lojaAtivaId && canViewPieces),
    queryFn: () => listPieces(token, deferredFilters),
    queryKey: queryKeys.pieces(token, session.lojaAtivaId, filtersKey),
  });

  const pieceDetailQuery = useQuery({
    enabled: Boolean(selectedPieceId && canViewPieces),
    queryFn: () => getPieceById(token, selectedPieceId),
    queryKey: queryKeys.pieceDetail(token, session.lojaAtivaId, selectedPieceId),
  });

  const pieceMutation = useMutation({
    mutationFn: async () => {
      return form.id
        ? updatePiece(token, form.id, mapFormToUpdatePayload(form))
        : createPiece(token, mapFormToCreatePayload(form));
    },
    onError: (error) => {
      toast.error(getErrorMessage(error));
    },
    onSuccess: async (response) => {
      setSelectedPieceId(response.id);
      setDraftForm(mapPieceDetailToForm(response));
      toast.success(
        form.id ? "Peca atualizada com sucesso." : "Peca criada com sucesso.",
      );
      queryClient.setQueryData(
        queryKeys.pieceDetail(token, session.lojaAtivaId, response.id),
        response,
      );
      await queryClient.invalidateQueries({
        queryKey: queryKeys.pieces(token, session.lojaAtivaId, filtersKey),
      });
    },
  });

  const imageUploadMutation = useMutation({
    mutationFn: async (payload: {
      file: File;
      ordem: number;
      tipoVisibilidade: string;
    }) => {
      if (!form.id) {
        throw new Error("Salve a peca antes de enviar imagens.");
      }

      return uploadPieceImage(token, form.id, {
        arquivo: payload.file,
        ordem: payload.ordem,
        tipoVisibilidade: payload.tipoVisibilidade,
      });
    },
    onError: (error) => {
      toast.error(getErrorMessage(error));
    },
    onSuccess: async () => {
      toast.success("Imagem enviada com sucesso.");
      await refreshPieceDetail();
    },
  });

  const imageMetaMutation = useMutation({
    mutationFn: async (payload: {
      imageId: string;
      ordem: number;
      tipoVisibilidade: string;
    }) => {
      if (!form.id) {
        throw new Error("Selecione uma peca antes de editar a imagem.");
      }

      return updatePieceImage(token, form.id, payload.imageId, {
        ordem: payload.ordem,
        tipoVisibilidade: payload.tipoVisibilidade,
      });
    },
    onError: (error) => {
      toast.error(getErrorMessage(error));
    },
    onSuccess: async () => {
      toast.success("Imagem atualizada com sucesso.");
      await refreshPieceDetail();
    },
  });

  const imageDeleteMutation = useMutation({
    mutationFn: async (imageId: string) => {
      if (!form.id) {
        throw new Error("Selecione uma peca antes de remover a imagem.");
      }

      return deletePieceImage(token, form.id, imageId);
    },
    onError: (error) => {
      toast.error(getErrorMessage(error));
    },
    onSuccess: async () => {
      toast.success("Imagem removida com sucesso.");
      await refreshPieceDetail();
    },
  });

  useEffect(() => {
    if (workspaceQuery.isError) {
      toast.error(getErrorMessage(workspaceQuery.error));
    }
  }, [workspaceQuery.error, workspaceQuery.isError]);

  useEffect(() => {
    if (piecesQuery.isError) {
      toast.error(getErrorMessage(piecesQuery.error));
    }
  }, [piecesQuery.error, piecesQuery.isError]);

  useEffect(() => {
    if (pieceDetailQuery.isError) {
      toast.error(getErrorMessage(pieceDetailQuery.error));
    }
  }, [pieceDetailQuery.error, pieceDetailQuery.isError]);

  useEffect(() => {
    const pieces = piecesQuery.data ?? [];
    if (pieces.length === 0) {
      return;
    }

    if (!selectedPieceId || !pieces.some((piece) => piece.id === selectedPieceId)) {
      startTransition(() => {
        setSelectedPieceId(pieces[0]?.id ?? "");
      });
    }
  }, [piecesQuery.data, selectedPieceId]);

  if (!canViewPieces) {
    return (
      <AccessStateCard
        message="Solicite a permissao adequada para consultar ou cadastrar pecas na loja ativa."
        subtitle="Sua conta nao possui acesso ao modulo de pecas e estoque."
        title="Modulo sem permissao"
      />
    );
  }

  const detail = pieceDetailQuery.data;
  const form =
    draftForm ??
    (selectedPieceId && detail
      ? mapPieceDetailToForm(detail)
      : emptyPieceForm(getTodayInputValue()));
  const busy =
    workspaceQuery.isLoading ||
    piecesQuery.isLoading ||
    pieceDetailQuery.isLoading ||
    pieceMutation.isPending ||
    imageUploadMutation.isPending ||
    imageMetaMutation.isPending ||
    imageDeleteMutation.isPending;

  function setForm(value: SetStateAction<PieceFormState>) {
    setDraftForm((current) => {
      const baseValue = current ?? form;
      return typeof value === "function"
        ? (value as (current: PieceFormState) => PieceFormState)(baseValue)
        : value;
    });
  }

  async function refreshPieceDetail() {
    if (!selectedPieceId) {
      return;
    }

    await Promise.all([
      queryClient.invalidateQueries({
        queryKey: queryKeys.pieceDetail(token, session.lojaAtivaId, selectedPieceId),
      }),
      queryClient.invalidateQueries({
        queryKey: queryKeys.pieces(token, session.lojaAtivaId, filtersKey),
      }),
    ]);
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    await pieceMutation.mutateAsync();
  }

  function handleNewPiece() {
    setSelectedPieceId("");
    setDraftForm(emptyPieceForm(getTodayInputValue()));
  }

  return (
    <div className="dashboard-grid">
      <div className="dashboard-column" style={{ gridColumn: "1 / -1" }}>
        <PiecesOverview pieces={piecesQuery.data ?? []} />
      </div>

      <div className="dashboard-column">
        <PiecesListPanel
          brands={workspaceQuery.data?.marcas ?? []}
          busy={busy}
          canManage={canManagePieces}
          filters={filters}
          onNewPiece={handleNewPiece}
          onSelectPiece={(pieceId) => {
            setSelectedPieceId(pieceId);
            setDraftForm(null);
          }}
          pieces={piecesQuery.data ?? []}
          productNames={workspaceQuery.data?.produtoNomes ?? []}
          selectedPieceId={selectedPieceId}
          setFilters={setFilters}
          statuses={workspaceQuery.data?.statusPeca ?? []}
          suppliers={workspaceQuery.data?.fornecedores ?? []}
        />
      </div>

      <div className="dashboard-column">
        <PieceFormPanel
          brands={workspaceQuery.data?.marcas ?? []}
          busy={busy || !canManagePieces}
          colors={workspaceQuery.data?.cores ?? []}
          detail={detail}
          form={form}
          onAddManualBand={() =>
            setForm((current) => ({
              ...current,
              regraManual: {
                ...current.regraManual,
                politicaDesconto: [
                  ...current.regraManual.politicaDesconto,
                  createEmptyPieceDiscountBand(),
                ],
              },
            }))
          }
          onSubmit={handleSubmit}
          pieceTypes={workspaceQuery.data?.tiposPeca ?? []}
          productNames={workspaceQuery.data?.produtoNomes ?? []}
          setForm={setForm}
          sizes={workspaceQuery.data?.tamanhos ?? []}
          suppliers={workspaceQuery.data?.fornecedores ?? []}
        />

        <PieceImagesPanel
          busy={busy || !canManagePieces}
          canUpload={Boolean(form.id)}
          images={sortPieceImages(detail?.imagens ?? [])}
          onDeleteImage={(imageId) => imageDeleteMutation.mutate(imageId)}
          onUpdateImage={(imageId, ordem, tipoVisibilidade) =>
            imageMetaMutation.mutate({ imageId, ordem, tipoVisibilidade })
          }
          onUploadImage={(file, ordem, tipoVisibilidade) =>
            imageUploadMutation.mutate({ file, ordem, tipoVisibilidade })
          }
          visibilityOptions={workspaceQuery.data?.visibilidadesImagem ?? []}
        />
      </div>
    </div>
  );
}
