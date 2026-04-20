"use client";

import { useQuery } from "@tanstack/react-query";
import { useMemo } from "react";

import { ClientEmptyState } from "@/app/components/client/client-empty-state";
import { getClientPendingBalances } from "@/lib/client-pending";
import { formatCurrency } from "@/lib/payment";
import { getAuthToken } from "@/lib/store";

function getSituationBadgeClass(situacao: string) {
  return situacao === "Receber"
    ? "bg-emerald-100 text-emerald-700"
    : "bg-amber-100 text-amber-800";
}

export function ClientPendingPage() {
  const token = useMemo(() => (typeof window === "undefined" ? null : getAuthToken()), []);

  const pendingQuery = useQuery({
    queryKey: ["client-area-pending-balances", token],
    queryFn: async () => {
      if (!token) {
        return [];
      }

      return getClientPendingBalances(token);
    },
    enabled: Boolean(token),
  });

  const items = pendingQuery.data ?? [];
  const totalReceberCredito = items
    .filter((item) => item.situacao === "Receber")
    .reduce((total, item) => total + item.valorCredito, 0);
  const totalPagarCredito = items
    .filter((item) => item.situacao === "Pagar")
    .reduce((total, item) => total + item.valorCredito, 0);

  return (
    <section className="space-y-6">
      <div className="rounded-[30px] border border-[var(--border)] bg-[linear-gradient(135deg,_#fffef9,_#f4f7ff_50%,_#eef6f1)] p-6">
        <div className="flex flex-col gap-4 lg:flex-row lg:items-end lg:justify-between">
          <div className="space-y-2">
            <span className="inline-flex rounded-full bg-[#eef4ea] px-3 py-1 text-xs font-semibold uppercase tracking-[0.22em] text-[#52624d]">
              Area do cliente
            </span>
            <div>
              <h1 className="text-3xl font-semibold tracking-tight text-[var(--foreground)]">
                Pendencias por loja
              </h1>
              <p className="mt-2 max-w-3xl text-sm text-[var(--muted)]">
                Acompanhe o saldo da sua conta de credito em cada loja e veja quanto ha para pagar
                ou receber em credito e em especie.
              </p>
            </div>
          </div>
          <div className="rounded-3xl border border-white/70 bg-white/75 px-5 py-4">
            <p className="text-xs font-semibold uppercase tracking-[0.2em] text-[var(--muted)]">
              Lojas com saldo
            </p>
            <p className="mt-1 text-3xl font-semibold text-[var(--foreground)]">{items.length}</p>
          </div>
        </div>
      </div>

      <div className="grid gap-4 md:grid-cols-3">
        <div className="rounded-3xl border border-[var(--border)] bg-white p-5 shadow-[0_18px_45px_rgba(15,23,42,0.05)]">
          <p className="text-sm text-[var(--muted)]">Total a receber em credito</p>
          <p className="mt-2 text-3xl font-semibold text-emerald-700">
            {formatCurrency(totalReceberCredito)}
          </p>
        </div>
        <div className="rounded-3xl border border-[var(--border)] bg-white p-5 shadow-[0_18px_45px_rgba(15,23,42,0.05)]">
          <p className="text-sm text-[var(--muted)]">Total a pagar em credito</p>
          <p className="mt-2 text-3xl font-semibold text-amber-800">
            {formatCurrency(totalPagarCredito)}
          </p>
        </div>
        <div className="rounded-3xl border border-[var(--border)] bg-white p-5 shadow-[0_18px_45px_rgba(15,23,42,0.05)]">
          <p className="text-sm text-[var(--muted)]">Saldo liquido</p>
          <p className="mt-2 text-3xl font-semibold text-[var(--foreground)]">
            {formatCurrency(totalReceberCredito - totalPagarCredito)}
          </p>
        </div>
      </div>

      <div className="rounded-[28px] border border-[var(--border)] bg-white p-6 shadow-[0_18px_45px_rgba(15,23,42,0.05)]">
        <div>
          <h2 className="text-xl font-semibold text-[var(--foreground)]">Contas por loja</h2>
          <p className="mt-1 text-sm text-[var(--muted)]">
            Os valores saem diretamente da conta de credito vinculada ao seu cadastro em cada loja.
          </p>
        </div>

        {pendingQuery.isLoading ? (
          <ClientEmptyState
            title="Carregando pendencias"
            description="Buscando as contas de credito vinculadas ao seu usuario."
          />
        ) : pendingQuery.isError ? (
          <ClientEmptyState
            title="Falha ao carregar pendencias"
            description={
              pendingQuery.error instanceof Error
                ? pendingQuery.error.message
                : "Tente novamente em instantes."
            }
          />
        ) : items.length === 0 ? (
          <ClientEmptyState
            title="Nenhuma pendencia encontrada"
            description="Seu usuario nao possui saldo diferente de zero nas contas de credito vinculadas."
          />
        ) : (
          <div className="mt-6 overflow-hidden rounded-[28px] border border-[var(--border)]">
            <div className="overflow-x-auto">
              <table className="min-w-full border-collapse bg-white">
                <thead className="bg-[var(--surface-muted)]">
                  <tr className="text-left text-xs uppercase tracking-[0.24em] text-[var(--muted)]">
                    <th className="px-5 py-4 font-medium">Loja</th>
                    <th className="px-5 py-4 font-medium">Situacao</th>
                    <th className="px-5 py-4 font-medium text-right">Saldo da conta</th>
                    <th className="px-5 py-4 font-medium text-right">Em credito</th>
                    <th className="px-5 py-4 font-medium text-right">Em especie</th>
                  </tr>
                </thead>
                <tbody>
                  {items.map((item, index) => (
                    <tr
                      key={`${item.lojaId}-${item.clienteId}`}
                      className={
                        index % 2 === 0
                          ? "border-t border-[var(--border)] bg-white"
                          : "border-t border-[var(--border)] bg-[color:color-mix(in_srgb,var(--surface-muted)_55%,white)]"
                      }
                    >
                      <td className="px-5 py-4">
                        <div className="font-medium text-[var(--foreground)]">{item.lojaNome}</div>
                        <div className="text-sm text-[var(--muted)]">Cliente #{item.clienteId}</div>
                      </td>
                      <td className="px-5 py-4">
                        <span
                          className={`inline-flex rounded-full px-3 py-1 text-xs font-semibold ${getSituationBadgeClass(item.situacao)}`}
                        >
                          {item.situacao}
                        </span>
                      </td>
                      <td className="px-5 py-4 text-right text-sm font-semibold text-[var(--foreground)]">
                        {formatCurrency(item.saldoConta)}
                      </td>
                      <td className="px-5 py-4 text-right text-sm font-semibold text-[var(--foreground)]">
                        {formatCurrency(item.valorCredito)}
                      </td>
                      <td className="px-5 py-4 text-right text-sm font-semibold text-[var(--foreground)]">
                        {item.valorEspecie === null ? (
                          <span className="text-[var(--muted)]">Nao disponivel</span>
                        ) : (
                          formatCurrency(item.valorEspecie)
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        )}
      </div>
    </section>
  );
}
