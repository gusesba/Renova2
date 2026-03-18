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
import { SaleDetailPanel } from "@/app/(system)/sales/components/sale-detail-panel";
import { SaleFormPanel } from "@/app/(system)/sales/components/sale-form-panel";
import { SalesListPanel } from "@/app/(system)/sales/components/sales-list-panel";
import { SalesOverview } from "@/app/(system)/sales/components/sales-overview";
import {
  createEmptyCancelSaleForm,
  createEmptySaleItem,
  createEmptySalePayment,
  emptySaleFilters,
  resetSaleForm,
  resolveSalePieceReference,
  type CancelSaleFormState,
  type SaleFiltersState,
  type SaleFormState,
} from "@/app/(system)/sales/components/types";
import { AccessStateCard } from "@/components/ui/access-state-card";
import {
  accessPermissionCodes,
  hasAnyPermission,
  hasPermission,
} from "@/lib/helpers/access-control";
import { getErrorMessage } from "@/lib/helpers/formatters";
import { queryKeys } from "@/lib/helpers/query-keys";
import { getZodErrorMessage } from "@/lib/schemas/access";
import { cancelSaleSchema, saleFormSchema } from "@/lib/schemas/sales";
import { getCommercialRulesWorkspace } from "@/lib/services/commercial-rules";
import { listPieces } from "@/lib/services/pieces";
import {
  cancelSale,
  createSale,
  getSaleById,
  getSalesWorkspace,
  listSales,
} from "@/lib/services/sales";
import type {
  SalePaymentMethodOption,
  SalePieceOption,
} from "@/lib/services/sales";

// Converte data local para inicio do dia em UTC antes de consultar a API.
function toStartOfDayIso(value: string) {
  return value ? `${value}T00:00:00Z` : "";
}

// Converte data local para fim do dia em UTC antes de consultar a API.
function toEndOfDayIso(value: string) {
  return value ? `${value}T23:59:59Z` : "";
}

// Traduz o formulario do frontend para o payload esperado no backend.
function mapSaleFormToPayload(form: SaleFormState, pieces: SalePieceOption[]) {
  const parsed = saleFormSchema.safeParse(form);
  if (!parsed.success) {
    throw new Error(getZodErrorMessage(parsed.error));
  }

  return {
    compradorPessoaId: parsed.data.compradorPessoaId || null,
    observacoes: parsed.data.observacoes,
    itens: parsed.data.itens.map((item) => ({
      pecaId:
        resolveSalePieceReference(pieces, item.identificadorPeca)?.pecaId ??
        (() => {
          throw new Error(
            `Peca nao encontrada para o identificador "${item.identificadorPeca}".`,
          );
        })(),
      quantidade: item.quantidade,
      descontoUnitario: item.descontoUnitario,
    })),
    pagamentos: parsed.data.pagamentos.map((payment) => ({
      tipoPagamento: payment.tipoPagamento,
      meioPagamentoId:
        payment.tipoPagamento === "meio_pagamento"
          ? payment.meioPagamentoId
          : null,
      valor: payment.valor,
    })),
  };
}

// Coordena a tela do modulo 09 com workspace, consulta, venda e cancelamento.
export function SalesDashboard() {
  const { token, session } = useSystemSession();
  const queryClient = useQueryClient();
  const [filters, setFilters] = useState<SaleFiltersState>(emptySaleFilters);
  const deferredFilters = useDeferredValue(filters);
  const [selectedSaleId, setSelectedSaleId] = useState("");
  const [saleDraft, setSaleDraft] = useState<SaleFormState | null>(null);
  const [cancelDraft, setCancelDraft] = useState<CancelSaleFormState>(
    createEmptyCancelSaleForm(),
  );
  const canViewSales = hasAnyPermission(session, [
    accessPermissionCodes.salesCreate,
    accessPermissionCodes.salesCancel,
  ]);
  const canCreateSales = hasPermission(
    session,
    accessPermissionCodes.salesCreate,
  );
  const canCancelSales = hasPermission(
    session,
    accessPermissionCodes.salesCancel,
  );

  const workspaceQuery = useQuery({
    enabled: Boolean(session.lojaAtivaId && canViewSales),
    queryFn: () => getSalesWorkspace(token),
    queryKey: queryKeys.salesWorkspace(token, session.lojaAtivaId),
  });

  const filtersKey = useMemo(
    () => JSON.stringify(deferredFilters),
    [deferredFilters],
  );

  const salesQuery = useQuery({
    enabled: Boolean(session.lojaAtivaId && canViewSales),
    queryFn: () =>
      listSales(token, {
        compradorPessoaId: deferredFilters.compradorPessoaId || undefined,
        dataFinal: toEndOfDayIso(deferredFilters.dataFinal),
        dataInicial: toStartOfDayIso(deferredFilters.dataInicial),
        search: deferredFilters.search || undefined,
        statusVenda: deferredFilters.statusVenda || undefined,
      }),
    queryKey: queryKeys.sales(token, session.lojaAtivaId, filtersKey),
  });

  const fallbackPiecesQuery = useQuery({
    enabled: Boolean(session.lojaAtivaId && canViewSales),
    queryFn: () => listPieces(token, { statusPeca: "disponivel" }),
    queryKey: queryKeys.pieces(
      token,
      session.lojaAtivaId,
      JSON.stringify({ origin: "sales-fallback", statusPeca: "disponivel" }),
    ),
    retry: false,
  });

  const fallbackRulesQuery = useQuery({
    enabled: Boolean(session.lojaAtivaId && canViewSales),
    queryFn: () => getCommercialRulesWorkspace(token),
    queryKey: queryKeys.commercialRulesWorkspace(
      token,
      `sales-fallback-${session.lojaAtivaId ?? "none"}`,
    ),
    retry: false,
  });

  const saleDetailQuery = useQuery({
    enabled: Boolean(selectedSaleId && canViewSales),
    queryFn: () => getSaleById(token, selectedSaleId),
    queryKey: queryKeys.saleDetail(token, session.lojaAtivaId, selectedSaleId),
  });

  const createSaleMutation = useMutation({
    mutationFn: async () => createSale(token, mapSaleFormToPayload(form, resolvedPieces)),
    onError: (error) => {
      toast.error(getErrorMessage(error));
    },
    onSuccess: async (response) => {
      setSelectedSaleId(response.id);
      setCancelDraft(createEmptyCancelSaleForm());
      setSaleDraft(resetSaleForm(resolvedWorkspace));
      toast.success("Venda concluida com sucesso.");
      await refreshSalesData(response.id);
    },
  });

  const cancelSaleMutation = useMutation({
    mutationFn: async () => {
      const parsed = cancelSaleSchema.safeParse(cancelDraft);
      if (!parsed.success) {
        throw new Error(getZodErrorMessage(parsed.error));
      }

      if (!selectedSaleId) {
        throw new Error("Selecione a venda que sera cancelada.");
      }

      return cancelSale(token, selectedSaleId, parsed.data);
    },
    onError: (error) => {
      toast.error(getErrorMessage(error));
    },
    onSuccess: async (response) => {
      setCancelDraft(createEmptyCancelSaleForm());
      toast.success("Venda cancelada com sucesso.");
      await refreshSalesData(response.id);
    },
  });

  useEffect(() => {
    if (workspaceQuery.isError) {
      toast.error(getErrorMessage(workspaceQuery.error));
    }
  }, [workspaceQuery.error, workspaceQuery.isError]);

  useEffect(() => {
    if (salesQuery.isError) {
      toast.error(getErrorMessage(salesQuery.error));
    }
  }, [salesQuery.error, salesQuery.isError]);

  useEffect(() => {
    if (saleDetailQuery.isError) {
      toast.error(getErrorMessage(saleDetailQuery.error));
    }
  }, [saleDetailQuery.error, saleDetailQuery.isError]);

  useEffect(() => {
    const items = salesQuery.data ?? [];
    if (items.length === 0) {
      startTransition(() => {
        setSelectedSaleId("");
      });
      return;
    }

    if (!selectedSaleId || !items.some((item) => item.id === selectedSaleId)) {
      startTransition(() => {
        setSelectedSaleId(items[0]?.id ?? "");
      });
    }
  }, [salesQuery.data, selectedSaleId]);

  if (!canViewSales) {
    return (
      <AccessStateCard
        message="Solicite permissao de registro ou cancelamento para acessar o modulo de vendas."
        subtitle="Sua conta nao possui acesso ao fluxo de vendas da loja ativa."
        title="Modulo sem permissao"
      />
    );
  }

  const workspace = workspaceQuery.data;
  const resolvedPieces: SalePieceOption[] =
    workspace?.pecasDisponiveis && workspace.pecasDisponiveis.length > 0
      ? workspace.pecasDisponiveis
      : (fallbackPiecesQuery.data ?? []).map((piece) => ({
          pecaId: piece.id,
          codigoInterno: piece.codigoInterno,
          codigoBarras: piece.codigoBarras,
          tipoPeca: piece.tipoPeca,
          statusPeca: piece.statusPeca,
          produtoNome: piece.produtoNome,
          marca: piece.marca,
          tamanho: piece.tamanho,
          cor: piece.cor,
          fornecedorPessoaId: piece.fornecedorPessoaId,
          fornecedorNome: piece.fornecedorNome,
          quantidadeAtual: piece.quantidadeAtual,
          precoVendaAtual: piece.precoVendaAtual,
          percentualRepasseDinheiro: 0,
          percentualRepasseCredito: 0,
          permitePagamentoMisto: true,
        }));
  const resolvedPaymentMethods: SalePaymentMethodOption[] =
    workspace?.meiosPagamento && workspace.meiosPagamento.length > 0
      ? workspace.meiosPagamento
      : (fallbackRulesQuery.data?.meiosPagamento ?? [])
          .filter((method) => method.ativo)
          .map((method) => ({
            id: method.id,
            nome: method.nome,
            tipoMeioPagamento: method.tipoMeioPagamento,
            tipoMeioPagamentoNome: method.tipoMeioPagamento,
            taxaPercentual: method.taxaPercentual,
            prazoRecebimentoDias: method.prazoRecebimentoDias,
          }));
  const resolvedWorkspace = workspace
    ? {
        ...workspace,
        meiosPagamento: resolvedPaymentMethods,
        pecasDisponiveis: resolvedPieces,
      }
    : undefined;
  const form =
    saleDraft ??
    resetSaleForm(resolvedWorkspace);
  const busy =
    workspaceQuery.isLoading ||
    salesQuery.isLoading ||
    saleDetailQuery.isLoading ||
    (resolvedPieces.length === 0 && fallbackPiecesQuery.isLoading) ||
    (resolvedPaymentMethods.length === 0 && fallbackRulesQuery.isLoading) ||
    createSaleMutation.isPending ||
    cancelSaleMutation.isPending;

  function setForm(value: SetStateAction<SaleFormState>) {
    setSaleDraft((current) => {
      const baseValue = current ?? form;
      return typeof value === "function"
        ? (value as (current: SaleFormState) => SaleFormState)(baseValue)
        : value;
    });
  }

  async function refreshSalesData(saleId?: string) {
    await Promise.all([
      queryClient.invalidateQueries({
        queryKey: queryKeys.salesWorkspace(token, session.lojaAtivaId),
      }),
      queryClient.invalidateQueries({
        queryKey: queryKeys.sales(token, session.lojaAtivaId, filtersKey),
      }),
      saleId
        ? queryClient.invalidateQueries({
            queryKey: queryKeys.saleDetail(token, session.lojaAtivaId, saleId),
          })
        : Promise.resolve(),
    ]);
  }

  async function handleCreateSale(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    await createSaleMutation.mutateAsync();
  }

  async function handleCancelSale(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    await cancelSaleMutation.mutateAsync();
  }

  return (
    <div className="dashboard-grid">
      <div className="dashboard-column" style={{ gridColumn: "1 / -1" }}>
        <SalesOverview sales={salesQuery.data ?? []} workspace={resolvedWorkspace} />
      </div>

      <div className="dashboard-column">
        <SalesListPanel
          buyers={workspace?.compradores ?? []}
          busy={busy}
          filters={filters}
          items={salesQuery.data ?? []}
          onSelectSale={(saleId) => {
            setSelectedSaleId(saleId);
            setCancelDraft(createEmptyCancelSaleForm());
          }}
          selectedSaleId={selectedSaleId}
          setFilters={setFilters}
          statuses={workspace?.statusVenda ?? []}
        />
      </div>

      <div className="dashboard-column">
        <SaleFormPanel
          buyers={workspace?.compradores ?? []}
          busy={busy}
          canCreate={canCreateSales}
          form={form}
          onAddItem={() =>
            setForm((current) => ({
              ...current,
              itens: [
                ...current.itens,
                createEmptySaleItem(),
              ],
            }))
          }
          onAddPayment={() =>
            setForm((current) => ({
              ...current,
              pagamentos: [
                ...current.pagamentos,
                createEmptySalePayment(workspace?.tiposPagamento[0]?.codigo),
              ],
            }))
          }
          onRemoveItem={(itemId) =>
            setForm((current) => ({
              ...current,
              itens: current.itens.filter((item) => item.id !== itemId),
            }))
          }
          onRemovePayment={(paymentId) =>
            setForm((current) => ({
              ...current,
              pagamentos: current.pagamentos.filter(
                (payment) => payment.id !== paymentId,
              ),
            }))
          }
          onSubmit={handleCreateSale}
          paymentMethods={resolvedPaymentMethods}
          paymentTypes={workspace?.tiposPagamento ?? []}
          pieces={resolvedPieces}
          setForm={setForm}
        />

        <SaleDetailPanel
          busy={busy}
          canCancel={canCancelSales}
          cancelForm={cancelDraft}
          detail={saleDetailQuery.data}
          onCancelSale={handleCancelSale}
          setCancelForm={setCancelDraft}
        />
      </div>
    </div>
  );
}
