"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { startTransition, useEffect, useMemo, useState } from "react";
import { toast } from "sonner";

import { StoreConfigModal } from "@/app/components/layout/store-config-modal";
import { Select } from "@/app/components/ui/select";
import { useStoreContext } from "@/app/dashboard/store-context";
import {
  asMovementBatchResponse,
  asMovementDestinationSuggestionResponse,
  formatMovementDate,
  formatMovementType,
  getMovementApiMessage,
  type MovementDestinationProduct,
} from "@/lib/movement";
import { formatDateValue, formatSituacaoValue, getProductApiMessage, type ProductListItem } from "@/lib/product";
import { getAuthToken } from "@/lib/store";
import {
  createMovementDestination,
  getMovementDestinationSuggestions,
} from "@/services/movement-service";
import { getProductById } from "@/services/product-service";

type DestinationItem = {
  id: number;
  product: ProductListItem;
  source: "automatic" | "manual";
  tipo: "3" | "4";
};

function toUtcStartOfDay(value: string) {
  return new Date(`${value}T00:00:00.000Z`).toISOString();
}

function asDestinationType(value: number): "3" | "4" {
  return value === 3 ? "3" : "4";
}

function buildAutomaticItems(products: MovementDestinationProduct[]): DestinationItem[] {
  return products.map((product) => ({
    id: product.id,
    product,
    source: "automatic",
    tipo: asDestinationType(product.tipoSugerido),
  }));
}

function FieldShell({
  children,
  error,
  label,
}: {
  children: React.ReactNode;
  error?: string;
  label: string;
}) {
  return (
    <label className="block space-y-2">
      <span className="text-sm font-semibold text-[var(--foreground)]">{label}</span>
      {children}
      {error ? <p className="text-sm text-red-500">{error}</p> : null}
    </label>
  );
}

export function MovementDestinationPage() {
  const queryClient = useQueryClient();
  const { isLoadingStores, selectedStore, selectedStoreId } = useStoreContext();
  const [isStoreConfigOpen, setIsStoreConfigOpen] = useState(false);
  const [date, setDate] = useState(new Date().toISOString().slice(0, 10));
  const [manualProductId, setManualProductId] = useState("");
  const [items, setItems] = useState<DestinationItem[]>([]);
  const [manualError, setManualError] = useState<string | null>(null);
  const token = useMemo(() => (typeof window === "undefined" ? null : getAuthToken()), []);

  const suggestionsQuery = useQuery({
    queryKey: ["movement-destination", token, selectedStoreId],
    queryFn: async () => {
      if (!token || !selectedStoreId) {
        return null;
      }

      const response = await getMovementDestinationSuggestions(token, selectedStoreId);

      if (!response.ok) {
        throw new Error(
          getMovementApiMessage(response.body) ??
            "Nao foi possivel carregar os produtos elegiveis para doacao e devolucao.",
        );
      }

      return asMovementDestinationSuggestionResponse(response.body);
    },
    enabled: Boolean(token && selectedStoreId),
    refetchOnWindowFocus: false,
  });

  const fetchProductMutation = useMutation({
    mutationFn: async (productId: number) => {
      if (!token) {
        throw new Error("Voce precisa estar autenticado para buscar produtos.");
      }

      return getProductById(productId, token);
    },
  });

  const createDestinationMutation = useMutation({
    mutationFn: async () => {
      if (!token || !selectedStoreId) {
        throw new Error("Selecione uma loja valida antes de salvar.");
      }

      return createMovementDestination(
        {
          data: toUtcStartOfDay(date),
          lojaId: selectedStoreId,
          itens: items.map((item) => ({
            produtoId: item.product.id,
            tipo: Number(item.tipo),
          })),
        },
        token,
      );
    },
  });

  useEffect(() => {
    startTransition(() => {
      setItems(buildAutomaticItems(suggestionsQuery.data?.produtos ?? []));
      setManualProductId("");
      setManualError(null);
    });
  }, [selectedStoreId, suggestionsQuery.data]);

  const groupedItems = useMemo(() => {
    const groups = new Map<
      number,
      {
        fornecedor: string;
        items: DestinationItem[];
      }
    >();

    for (const item of items) {
      const current = groups.get(item.product.fornecedorId);

      if (current) {
        current.items.push(item);
      } else {
        groups.set(item.product.fornecedorId, {
          fornecedor: item.product.fornecedor,
          items: [item],
        });
      }
    }

    return [...groups.entries()]
      .map(([fornecedorId, group]) => ({
        fornecedorId,
        fornecedor: group.fornecedor,
        items: group.items.sort((left, right) => left.product.id - right.product.id),
      }))
      .sort((left, right) => left.fornecedor.localeCompare(right.fornecedor));
  }, [items]);

  const summary = useMemo(() => {
    return items.reduce(
      (accumulator, item) => {
        if (item.tipo === "3") {
          accumulator.doacao += 1;
        } else {
          accumulator.devolucao += 1;
        }

        return accumulator;
      },
      { devolucao: 0, doacao: 0 },
    );
  }, [items]);

  async function handleAddManualProduct() {
    const trimmed = manualProductId.trim();

    if (!trimmed) {
      setManualError("Informe o id de um produto.");
      return;
    }

    const parsedId = Number(trimmed);

    if (!Number.isInteger(parsedId) || parsedId <= 0) {
      setManualError("Informe um id numerico valido.");
      return;
    }

    if (items.some((item) => item.product.id === parsedId)) {
      setManualError("Este produto ja esta listado.");
      return;
    }

    try {
      const response = await fetchProductMutation.mutateAsync(parsedId);

      if (!response.ok) {
        setManualError(getProductApiMessage(response.body) ?? "Nao foi possivel buscar o produto.");
        return;
      }

      const product = response.body as ProductListItem;

      if (selectedStoreId && product.lojaId !== selectedStoreId) {
        setManualError("O produto buscado pertence a outra loja.");
        return;
      }

      if (product.situacao !== 1) {
        setManualError("Apenas produtos em estoque podem ser enviados para doacao ou devolucao.");
        return;
      }

      setItems((current) => [
        ...current,
        {
          id: product.id,
          product,
          source: "manual",
          tipo: "4",
        },
      ]);
      setManualProductId("");
      setManualError(null);
      toast.success(`Produto ${product.id} adicionado para revisao.`);
    } catch (error) {
      setManualError(
        error instanceof Error
          ? error.message
          : "Nao foi possivel conectar ao backend. Verifique se a API esta em execucao.",
      );
    }
  }

  async function handleSubmit() {
    if (!selectedStoreId) {
      toast.error("Selecione uma loja antes de salvar.");
      return;
    }

    if (items.length === 0) {
      toast.error("Adicione ao menos um produto antes de finalizar.");
      return;
    }

    try {
      const response = await createDestinationMutation.mutateAsync();

      if (!response.ok) {
        const message =
          getMovementApiMessage(response.body) ?? "Nao foi possivel criar as movimentacoes.";

        if (message.toLowerCase().includes("configuracao da loja")) {
          setIsStoreConfigOpen(true);
          return;
        }

        toast.error(message);
        return;
      }

      const createdMovements = asMovementBatchResponse(response.body);
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ["movements"] }),
        queryClient.invalidateQueries({ queryKey: ["products"] }),
        queryClient.invalidateQueries({ queryKey: ["movement-destination"] }),
      ]);
      await suggestionsQuery.refetch();

      toast.success(
        `${createdMovements.length} movimentacao(oes) criada(s) para ${items.length} produto(s).`,
      );
    } catch (error) {
      toast.error(
        error instanceof Error
          ? error.message
          : "Nao foi possivel conectar ao backend. Verifique se a API esta em execucao.",
      );
    }
  }

  const hasStore = Boolean(selectedStoreId);
  const destinationTypeOptions: Array<{ label: string; value: "3" | "4" }> = [
    { label: "Doacao", value: "3" },
    { label: "Devolucao dono", value: "4" },
  ];

  return (
    <section className="space-y-6">
      <div className="overflow-hidden rounded-[32px] border border-[var(--border)] bg-white shadow-[0_20px_55px_rgba(15,23,42,0.08)]">
        <div className="border-b border-[var(--border)] bg-[linear-gradient(135deg,_#eef7ff_0%,_#fffaf4_48%,_#fff0e6_100%)] px-6 py-6">
          <div className="flex flex-col gap-4 lg:flex-row lg:items-end lg:justify-between">
            <div>
              <p className="text-sm font-semibold uppercase tracking-[0.18em] text-[var(--muted)]">
                Movimentacoes
              </p>
              <h1 className="mt-2 text-3xl font-semibold tracking-tight text-[var(--foreground)]">
                Doacao e devolucao ao dono
              </h1>
              <p className="mt-3 max-w-3xl text-sm leading-7 text-[var(--muted)]">
                A tela sugere produtos que ja passaram da permanencia configurada, agrupa por
                fornecedor e permite trocar o destino de cada peca antes do envio.
              </p>
            </div>

            <div className="rounded-3xl border border-white/80 bg-white/85 px-5 py-4 shadow-[0_18px_36px_rgba(15,23,42,0.06)] backdrop-blur">
              <p className="text-xs font-semibold uppercase tracking-[0.16em] text-[var(--muted)]">
                Loja ativa
              </p>
              <p className="mt-2 text-lg font-semibold text-[var(--foreground)]">
                {selectedStore?.nome ?? "Nenhuma loja selecionada"}
              </p>
            </div>
          </div>
        </div>

        <div className="px-6 py-6">
          {!hasStore ? (
            <div className="rounded-[28px] border border-dashed border-[var(--border-strong)] bg-[var(--surface-muted)] px-6 py-12 text-center">
              <h2 className="text-xl font-semibold text-[var(--foreground)]">Selecione uma loja</h2>
              <p className="mt-3 text-sm leading-7 text-[var(--muted)]">
                A destinacao por permanencia depende da loja ativa no topo da aplicacao.
              </p>
            </div>
          ) : (
            <div className="grid gap-6 xl:grid-cols-[minmax(0,1.25fr)_minmax(360px,0.85fr)]">
              <div className="space-y-5 rounded-[28px] border border-[var(--border)] bg-[var(--surface-muted)] p-5">
                <div className="grid gap-4 md:grid-cols-[minmax(0,220px)_1fr]">
                  <FieldShell label="Data da movimentacao">
                    <input
                      type="date"
                      value={date}
                      onChange={(event) => setDate(event.target.value)}
                      className="h-12 w-full rounded-2xl border border-[var(--border)] bg-white px-4 text-sm text-[var(--foreground)] outline-none transition focus:border-[var(--primary)] focus:shadow-[0_0_0_4px_rgba(106,92,255,0.12)]"
                    />
                  </FieldShell>

                  <FieldShell error={manualError ?? undefined} label="Adicionar produto pelo id">
                    <div className="flex flex-col gap-3 md:flex-row">
                      <input
                        type="text"
                        value={manualProductId}
                        placeholder="Ex.: 152"
                        onChange={(event) => {
                          setManualProductId(event.target.value.replace(/[^\d]/g, ""));
                          setManualError(null);
                        }}
                        className="h-12 flex-1 rounded-2xl border border-[var(--border)] bg-white px-4 text-sm text-[var(--foreground)] outline-none transition focus:border-[var(--primary)] focus:shadow-[0_0_0_4px_rgba(106,92,255,0.12)]"
                      />
                      <button
                        type="button"
                        onClick={handleAddManualProduct}
                        disabled={fetchProductMutation.isPending}
                        className="flex h-12 shrink-0 cursor-pointer items-center justify-center rounded-2xl bg-[var(--primary)] px-5 text-sm font-semibold text-white transition hover:brightness-105 disabled:cursor-not-allowed disabled:opacity-60"
                      >
                        {fetchProductMutation.isPending ? "Buscando..." : "Adicionar"}
                      </button>
                    </div>
                  </FieldShell>
                </div>

                <div className="rounded-[24px] border border-[var(--border)] bg-white p-4">
                  <div>
                    <p className="text-sm font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                      Sugestao automatica
                    </p>
                    <p className="mt-2 text-sm text-[var(--muted)]">
                      {suggestionsQuery.isLoading || isLoadingStores
                        ? "Carregando os itens pela permanencia da loja..."
                        : suggestionsQuery.isError
                          ? suggestionsQuery.error instanceof Error
                            ? suggestionsQuery.error.message
                            : "Nao foi possivel carregar a sugestao."
                          : suggestionsQuery.data
                            ? `Produtos em estoque com entrada ate ${formatMovementDate(
                                suggestionsQuery.data.dataLimitePermanencia,
                              )}. Permanencia atual: ${
                                suggestionsQuery.data.tempoPermanenciaProdutoMeses
                              } mes(es).`
                            : "Nenhuma configuracao carregada."}
                    </p>
                  </div>
                </div>

                <div className="flex flex-col gap-3 sm:flex-row sm:justify-end">
                  <button
                    type="button"
                    onClick={() => setItems(buildAutomaticItems(suggestionsQuery.data?.produtos ?? []))}
                    className="flex h-12 cursor-pointer items-center justify-center rounded-2xl border border-[var(--border)] bg-white px-5 text-sm font-semibold text-[var(--foreground)] transition hover:border-[var(--border-strong)]"
                  >
                    Restaurar sugestao
                  </button>
                  <button
                    type="button"
                    onClick={handleSubmit}
                    disabled={createDestinationMutation.isPending}
                    className="flex h-12 cursor-pointer items-center justify-center rounded-2xl bg-[linear-gradient(90deg,_#ff8a3d,_#ff6b3d)] px-5 text-sm font-semibold text-white shadow-[0_16px_30px_rgba(255,107,61,0.24)] transition hover:brightness-105 disabled:cursor-not-allowed disabled:opacity-60"
                  >
                    {createDestinationMutation.isPending ? "Salvando..." : "Finalizar destinacao"}
                  </button>
                </div>
              </div>

              <div className="rounded-[28px] border border-[var(--border)] bg-white p-5">
                <div className="flex items-start justify-between gap-3">
                  <div>
                    <p className="text-sm font-semibold uppercase tracking-[0.16em] text-[var(--muted)]">
                      Resumo
                    </p>
                    <h2 className="mt-2 text-2xl font-semibold text-[var(--foreground)]">
                      {items.length} item(ns) selecionado(s)
                    </h2>
                  </div>
                  <div className="flex flex-wrap justify-end gap-2">
                    <span className="rounded-full bg-emerald-100 px-3 py-1 text-xs font-semibold text-emerald-700">
                      Doacao: {summary.doacao}
                    </span>
                    <span className="rounded-full bg-amber-100 px-3 py-1 text-xs font-semibold text-amber-800">
                      Devolucao: {summary.devolucao}
                    </span>
                  </div>
                </div>

                {groupedItems.length === 0 ? (
                  <div className="mt-5 rounded-[24px] border border-dashed border-[var(--border-strong)] bg-[var(--surface-muted)] px-5 py-10 text-center">
                    <p className="text-lg font-semibold text-[var(--foreground)]">
                      Nenhum produto listado
                    </p>
                    <p className="mt-2 text-sm leading-7 text-[var(--muted)]">
                      Carregue a sugestao automatica ou adicione produtos manualmente pelo id.
                    </p>
                  </div>
                ) : (
                  <div className="mt-5 space-y-4">
                    {groupedItems.map((group) => (
                      <section
                        key={group.fornecedorId}
                        className="rounded-[24px] border border-[var(--border)] bg-[var(--surface-muted)] p-4"
                      >
                        <div className="flex flex-wrap items-center justify-between gap-3">
                          <div>
                            <p className="text-sm font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                              Fornecedor
                            </p>
                            <h3 className="mt-1 text-lg font-semibold text-[var(--foreground)]">
                              {group.fornecedor}
                            </h3>
                          </div>
                          <span className="rounded-full bg-white px-3 py-1 text-xs font-semibold text-[var(--foreground)]">
                            {group.items.length} peca(s)
                          </span>
                        </div>

                        <div className="mt-4 space-y-3">
                          {group.items.map((item) => (
                            <article
                              key={item.product.id}
                              className="rounded-[22px] border border-[var(--border)] bg-white p-4"
                            >
                              <div className="flex flex-col gap-4 xl:flex-row xl:items-start xl:justify-between">
                                <div className="min-w-0">
                                  <p className="text-sm font-semibold text-[var(--foreground)]">
                                    #{item.product.id} - {item.product.produto}
                                  </p>
                                  <p className="mt-1 text-sm text-[var(--muted)]">
                                    {item.product.descricao}
                                  </p>
                                  <div className="mt-3 flex flex-wrap gap-2">
                                    <span className="rounded-full bg-[var(--surface-muted)] px-3 py-1 text-xs font-semibold text-[var(--foreground)]">
                                      Marca - {item.product.marca}
                                    </span>
                                    <span className="rounded-full bg-[var(--surface-muted)] px-3 py-1 text-xs font-semibold text-[var(--foreground)]">
                                      Entrada - {formatDateValue(item.product.entrada)}
                                    </span>
                                    <span className="rounded-full bg-[var(--surface-muted)] px-3 py-1 text-xs font-semibold text-[var(--foreground)]">
                                      Situacao - {formatSituacaoValue(item.product.situacao)}
                                    </span>
                                    <span className="rounded-full bg-[var(--surface-muted)] px-3 py-1 text-xs font-semibold text-[var(--foreground)]">
                                      Origem - {item.source === "automatic" ? "Automatica" : "Manual"}
                                    </span>
                                  </div>
                                </div>

                                <div className="flex w-full flex-col gap-3 xl:w-[250px]">
                                  <div className="rounded-2xl border border-[var(--border)] bg-[var(--surface-muted)] px-4 py-3 text-sm text-[var(--foreground)]">
                                    <Select
                                      ariaLabel={`Destino do produto ${item.product.id}`}
                                      value={item.tipo}
                                      options={destinationTypeOptions}
                                      onChange={(value) =>
                                        setItems((current) =>
                                          current.map((currentItem) =>
                                            currentItem.product.id === item.product.id
                                              ? {
                                                  ...currentItem,
                                                  tipo: value as "3" | "4",
                                                }
                                              : currentItem,
                                          ),
                                        )
                                      }
                                    />
                                  </div>
                                  <p className="text-xs text-[var(--muted)]">
                                    Movimento final: {formatMovementType(Number(item.tipo))}
                                  </p>
                                  <button
                                    type="button"
                                    onClick={() =>
                                      setItems((current) =>
                                        current.filter((currentItem) => currentItem.product.id !== item.product.id),
                                      )
                                    }
                                    className="flex h-10 cursor-pointer items-center justify-center rounded-2xl border border-[var(--border)] bg-white px-4 text-sm font-semibold text-[var(--foreground)] transition hover:border-[var(--border-strong)]"
                                  >
                                    Remover
                                  </button>
                                </div>
                              </div>
                            </article>
                          ))}
                        </div>
                      </section>
                    ))}
                  </div>
                )}
              </div>
            </div>
          )}
        </div>
      </div>

      <StoreConfigModal
        isOpen={isStoreConfigOpen}
        storeId={selectedStoreId}
        storeName={selectedStore?.nome ?? null}
        onClose={() => setIsStoreConfigOpen(false)}
      />
    </section>
  );
}
