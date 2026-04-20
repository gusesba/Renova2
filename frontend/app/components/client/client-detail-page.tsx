"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useQuery } from "@tanstack/react-query";
import { useMemo, useState } from "react";

import { useStoreContext } from "@/app/dashboard/store-context";
import {
  asClientDetailResponse,
  formatPhoneValue,
  getClientApiMessage,
  getPreviousMonthRange,
  initialClientDetailFilters,
  type ClientDetailFilters,
} from "@/lib/client";
import { formatCurrency } from "@/lib/payment";
import {
  formatCurrencyValue,
  formatDateValue,
  formatSituacaoValue,
  productSituacaoOptions,
  type ProductListItem,
} from "@/lib/product";
import { getAuthToken } from "@/lib/store";
import { getClientDetail } from "@/services/client-service";
import { Select } from "@/app/components/ui/select";

function MetricCard({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-[24px] border border-[var(--border)] bg-white p-5 shadow-[0_18px_45px_rgba(15,23,42,0.05)]">
      <p className="text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">{label}</p>
      <p className="mt-2 text-2xl font-semibold text-[var(--foreground)]">{value}</p>
    </div>
  );
}

function ProductsSnapshot({
  title,
  description,
  products,
}: {
  title: string;
  description: string;
  products: ProductListItem[];
}) {
  return (
    <section className="rounded-[28px] border border-[var(--border)] bg-white p-6 shadow-[0_18px_45px_rgba(15,23,42,0.05)]">
      <div className="mb-4">
        <h2 className="text-xl font-semibold text-[var(--foreground)]">{title}</h2>
        <p className="mt-1 text-sm text-[var(--muted)]">{description}</p>
      </div>

      {products.length === 0 ? (
        <div className="rounded-[24px] border border-dashed border-[var(--border)] bg-[var(--surface)]/50 px-4 py-10 text-center text-sm text-[var(--muted)]">
          Nenhum produto encontrado para os filtros aplicados.
        </div>
      ) : (
        <div className="overflow-hidden rounded-[24px] border border-[var(--border)]">
          <div className="overflow-x-auto">
            <table className="min-w-full border-collapse bg-white">
              <thead className="bg-[var(--surface-muted)]">
                <tr className="text-left">
                  <th className="px-4 py-4 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                    Produto
                  </th>
                  <th className="px-4 py-4 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                    Descricao
                  </th>
                  <th className="px-4 py-4 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                    Fornecedor
                  </th>
                  <th className="px-4 py-4 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                    Situacao
                  </th>
                  <th className="px-4 py-4 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                    Entrada
                  </th>
                  <th className="px-4 py-4 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                    Preco
                  </th>
                </tr>
              </thead>
              <tbody>
                {products.map((product, index) => (
                  <tr
                    key={product.id}
                    className={
                      index % 2 === 0
                        ? "bg-white"
                        : "bg-[color:color-mix(in_srgb,var(--surface-muted)_55%,white)]"
                    }
                  >
                    <td className="px-4 py-4 text-sm font-semibold text-[var(--foreground)]">
                      {product.produto}
                    </td>
                    <td className="px-4 py-4 text-sm text-[var(--foreground)]">{product.descricao}</td>
                    <td className="px-4 py-4 text-sm text-[var(--foreground)]">{product.fornecedor}</td>
                    <td className="px-4 py-4 text-sm text-[var(--muted)]">
                      {formatSituacaoValue(product.situacao)}
                    </td>
                    <td className="px-4 py-4 text-sm text-[var(--muted)]">
                      {formatDateValue(product.entrada)}
                    </td>
                    <td className="px-4 py-4 text-sm text-[var(--foreground)]">
                      {formatCurrencyValue(product.preco)}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </section>
  );
}

export function ClientDetailPage({ clientId }: { clientId: number }) {
  const router = useRouter();
  const { isLoadingStores, selectedStoreId } = useStoreContext();
  const previousMonthRange = useMemo(() => getPreviousMonthRange(), []);
  const [filters, setFilters] = useState<ClientDetailFilters>({
    ...initialClientDetailFilters,
    ...previousMonthRange,
  });
  const token = useMemo(() => (typeof window === "undefined" ? null : getAuthToken()), []);

  const detailQuery = useQuery({
    queryKey: ["client-detail", token, selectedStoreId, clientId, filters],
    queryFn: async () => {
      if (!token || !selectedStoreId || !Number.isFinite(clientId)) {
        return null;
      }

      const response = await getClientDetail(token, selectedStoreId, clientId, filters);

      if (!response.ok) {
        throw new Error(
          getClientApiMessage(response.body) ?? "Nao foi possivel carregar o detalhe do cliente.",
        );
      }

      return asClientDetailResponse(response.body);
    },
    enabled: Boolean(token && selectedStoreId && Number.isFinite(clientId)),
  });

  const detail = detailQuery.data;
  const soldProductsWithClient = useMemo(
    () => detail?.produtosComCliente.filter((product) => product.situacao === 2) ?? [],
    [detail],
  );
  const returnSaleHref = useMemo(() => {
    if (!detail || soldProductsWithClient.length === 0) {
      return null;
    }

    const params = new URLSearchParams({
      clienteId: String(detail.id),
      clienteNome: detail.nome,
      clienteContato: detail.contato,
      tipo: "5",
      produtoIds: soldProductsWithClient.map((product) => String(product.id)).join(","),
    });

    return `/dashboard/movimentacao/nova?${params.toString()}`;
  }, [detail, soldProductsWithClient]);

  return (
    <section className="space-y-6">
      <div className="flex flex-col gap-4 rounded-[28px] border border-[var(--border)] bg-white p-6 shadow-[0_18px_45px_rgba(15,23,42,0.05)] lg:flex-row lg:items-end lg:justify-between">
        <div>
          <Link
            href="/dashboard/cliente"
            className="text-sm font-medium text-[var(--primary)] transition hover:opacity-80"
          >
            Voltar para clientes
          </Link>
          <h1 className="mt-3 text-3xl font-semibold text-[var(--foreground)]">Detalhe do cliente</h1>
          <p className="mt-2 text-sm text-[var(--muted)]">
            O filtro inicial usa automaticamente o primeiro e o ultimo dia do mes anterior.
          </p>
        </div>

        <div className="grid gap-3 md:grid-cols-3">
          <label className="space-y-2">
            <span className="text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
              Data inicial
            </span>
            <input
              type="date"
              value={filters.dataInicial}
              onChange={(event) =>
                setFilters((current) => ({ ...current, dataInicial: event.target.value }))
              }
              className="h-12 rounded-2xl border border-[var(--border)] bg-white px-4 text-sm text-[var(--foreground)] outline-none transition focus:border-[var(--primary)]"
            />
          </label>
          <label className="space-y-2">
            <span className="text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
              Data final
            </span>
            <input
              type="date"
              value={filters.dataFinal}
              onChange={(event) =>
                setFilters((current) => ({ ...current, dataFinal: event.target.value }))
              }
              className="h-12 rounded-2xl border border-[var(--border)] bg-white px-4 text-sm text-[var(--foreground)] outline-none transition focus:border-[var(--primary)]"
            />
          </label>
          <label className="space-y-2">
            <span className="text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
              Situacao
            </span>
            <div className="rounded-2xl border border-[var(--border)] bg-white px-4 py-3 text-sm text-[var(--foreground)]">
              <Select
                ariaLabel="Situacao"
                value={filters.situacao}
                options={[
                  { label: "Todas", value: "" },
                  ...productSituacaoOptions.map((option) => ({
                    label: option.label,
                    value: String(option.value),
                  })),
                ]}
                placeholder="Selecionar"
                onChange={(situacao) => setFilters((current) => ({ ...current, situacao }))}
              />
            </div>
          </label>
        </div>

        {returnSaleHref ? (
          <button
            type="button"
            onClick={() => router.push(returnSaleHref)}
            className="flex h-12 cursor-pointer items-center justify-center rounded-2xl bg-[linear-gradient(90deg,_#ff8a3d,_#ff6b3d)] px-5 text-sm font-semibold text-white shadow-[0_16px_30px_rgba(255,107,61,0.28)] transition hover:brightness-105 disabled:cursor-not-allowed disabled:opacity-60"
          >
            Devolver produtos vendidos
          </button>
        ) : null}
      </div>

      {!selectedStoreId ? (
        <div className="rounded-[28px] border border-[var(--border)] bg-white p-10 text-center text-sm text-[var(--muted)] shadow-[0_18px_45px_rgba(15,23,42,0.05)]">
          Selecione uma loja para visualizar o detalhe do cliente.
        </div>
      ) : isLoadingStores || detailQuery.isLoading ? (
        <div className="rounded-[28px] border border-[var(--border)] bg-white p-10 text-center text-sm text-[var(--muted)] shadow-[0_18px_45px_rgba(15,23,42,0.05)]">
          Carregando detalhe do cliente.
        </div>
      ) : detailQuery.isError ? (
        <div className="rounded-[28px] border border-[var(--border)] bg-white p-10 text-center text-sm text-rose-700 shadow-[0_18px_45px_rgba(15,23,42,0.05)]">
          {detailQuery.error instanceof Error
            ? detailQuery.error.message
            : "Nao foi possivel carregar o detalhe do cliente."}
        </div>
      ) : detail ? (
        <>
          <div className="grid gap-6 lg:grid-cols-[1.2fr_1fr]">
            <section className="rounded-[28px] border border-[var(--border)] bg-white p-6 shadow-[0_18px_45px_rgba(15,23,42,0.05)]">
              <h2 className="text-xl font-semibold text-[var(--foreground)]">{detail.nome}</h2>
              <div className="mt-5 grid gap-4 md:grid-cols-2">
                <div>
                  <p className="text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                    Contato
                  </p>
                  <p className="mt-2 text-base text-[var(--foreground)]">
                    {formatPhoneValue(detail.contato)}
                  </p>
                </div>
                <div>
                  <p className="text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                    Doacao
                  </p>
                  <p className="mt-2 text-base text-[var(--foreground)]">
                    {detail.doacao ? "Sim" : "Nao"}
                  </p>
                </div>
                <div>
                  <p className="text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                    Usuario vinculado
                  </p>
                  <p className="mt-2 text-base text-[var(--foreground)]">
                    {detail.userNome ?? "Nao vinculado"}
                  </p>
                </div>
                <div>
                  <p className="text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                    Email
                  </p>
                  <p className="mt-2 text-base text-[var(--foreground)]">
                    {detail.userEmail ?? "Nao vinculado"}
                  </p>
                </div>
              </div>
            </section>

            <div className="grid gap-4 sm:grid-cols-2">
              <MetricCard label="Pecas compradas" value={String(detail.quantidadePecasCompradas)} />
              <MetricCard label="Pecas vendidas" value={String(detail.quantidadePecasVendidas)} />
              <MetricCard label="Dinheiro retirado" value={formatCurrency(detail.valorRetiradoLoja)} />
              <MetricCard label="Dinheiro aportado" value={formatCurrency(detail.valorAportadoLoja)} />
            </div>
          </div>

          <ProductsSnapshot
            title="Produtos do cliente como fornecedor"
            description="Itens cadastrados com esse cliente como fornecedor, respeitando o periodo e a situacao selecionados."
            products={detail.produtosFornecedor}
          />

          <ProductsSnapshot
            title="Produtos atualmente com o cliente"
            description="Itens cuja ultima movimentacao deixou o produto com esse cliente por venda ou emprestimo."
            products={detail.produtosComCliente}
          />
        </>
      ) : null}
    </section>
  );
}
