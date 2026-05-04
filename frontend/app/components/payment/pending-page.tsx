"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useMemo, useState } from "react";
import { toast } from "sonner";

import { ClientEmptyState } from "@/app/components/client/client-empty-state";
import { useStoreContext } from "@/app/dashboard/store-context";
import { permissions } from "@/lib/access";
import {
  asPendingClientsResponse,
  asUpdatePendingResponse,
  formatCurrency,
  formatPhone,
  getPaymentApiMessage,
  getPreviousMonthLastDateInputValue,
  type PendingClientItem,
} from "@/lib/payment";
import { getAuthToken } from "@/lib/store";
import { getPendingClients, updatePendingPayments } from "@/services/payment-service";

import { PaymentCreditModal } from "./payment-credit-modal";

type PendingCreditFilter = "all" | "positive" | "negative";

const pendingCreditFilterOptions: Array<{ label: string; value: PendingCreditFilter }> = [
  { label: "Todos", value: "all" },
  { label: "Positivos", value: "positive" },
  { label: "Negativos", value: "negative" },
];

const emptyPendingClients: PendingClientItem[] = [];

function getCreditBadgeClass(value: number) {
  if (value > 0) {
    return "bg-emerald-100 text-emerald-700";
  }

  if (value < 0) {
    return "bg-rose-100 text-rose-700";
  }

  return "bg-slate-100 text-slate-600";
}

function PendingCreditFilterControls({
  value,
  onChange,
}: {
  value: PendingCreditFilter;
  onChange: (value: PendingCreditFilter) => void;
}) {
  return (
    <div className="mt-6 flex flex-wrap gap-2">
      {pendingCreditFilterOptions.map((option) => {
        const isActive = option.value === value;

        return (
          <button
            key={option.value}
            type="button"
            onClick={() => onChange(option.value)}
            className={`h-10 rounded-2xl px-4 text-sm font-semibold transition ${
              isActive
                ? "bg-[var(--primary)] text-white shadow-[0_14px_24px_rgba(106,92,255,0.18)]"
                : "border border-[var(--border)] bg-white text-[var(--muted)] hover:text-[var(--foreground)]"
            }`}
          >
            {option.label}
          </button>
        );
      })}
    </div>
  );
}

export function PendingPage() {
  const queryClient = useQueryClient();
  const { hasPermission, isLoadingStores, selectedStore, selectedStoreId } = useStoreContext();
  const [dateValue, setDateValue] = useState(() => getPreviousMonthLastDateInputValue());
  const [creditFilter, setCreditFilter] = useState<PendingCreditFilter>("all");
  const [selectedClient, setSelectedClient] = useState<PendingClientItem | null>(null);
  const token = useMemo(() => (typeof window === "undefined" ? null : getAuthToken()), []);
  const canUpdatePending = hasPermission(permissions.pagamentosPendenciasAtualizar);
  const canHandleCredit =
    hasPermission(permissions.pagamentosCreditoAdicionar) ||
    hasPermission(permissions.pagamentosCreditoResgatar);

  const pendingClientsQuery = useQuery({
    queryKey: ["pending-clients", token, selectedStoreId],
    queryFn: async () => {
      if (!token || !selectedStoreId) {
        return [];
      }

      const response = await getPendingClients(token, selectedStoreId);

      if (!response.ok) {
        throw new Error(
          getPaymentApiMessage(response.body) ?? "Nao foi possivel carregar as pendencias.",
        );
      }

      return asPendingClientsResponse(response.body);
    },
    enabled: Boolean(token && selectedStoreId),
  });

  const updatePendingMutation = useMutation({
    mutationFn: async (payload: { lojaId: number; data: string }) => {
      if (!token) {
        throw new Error("Voce precisa estar autenticado para atualizar as pendencias.");
      }

      return updatePendingPayments(payload, token);
    },
  });

  async function handleUpdatePending() {
    if (!selectedStoreId) {
      toast.error("Selecione uma loja antes de atualizar as pendencias.");
      return;
    }

    if (!dateValue) {
      toast.error("Informe a data limite para atualizar as pendencias.");
      return;
    }

    try {
      const response = await updatePendingMutation.mutateAsync({
        lojaId: selectedStoreId,
        data: dateValue,
      });

      if (!response.ok) {
        toast.error(
          getPaymentApiMessage(response.body) ?? "Nao foi possivel atualizar as pendencias.",
        );
        return;
      }

      const result = asUpdatePendingResponse(response.body);

      await queryClient.invalidateQueries({ queryKey: ["pending-clients"] });

      if (result.quantidadeOrdensAtualizadas === 0) {
        toast.success("Nenhuma ordem pendente foi encontrada para a data informada.");
        return;
      }

      toast.success(
        `${result.quantidadeOrdensAtualizadas} pendencia(s) atualizada(s), total de ${formatCurrency(result.valorTotalCredito)} em credito.`,
      );
    } catch (error) {
      toast.error(
        error instanceof Error
          ? error.message
          : "Nao foi possivel conectar ao backend. Verifique se a API esta em execucao.",
      );
    }
  }

  const clients = pendingClientsQuery.data ?? emptyPendingClients;
  const visibleClients = useMemo(() => {
    if (creditFilter === "positive") {
      return clients.filter((item) => item.credito > 0);
    }

    if (creditFilter === "negative") {
      return clients.filter((item) => item.credito < 0);
    }

    return clients;
  }, [clients, creditFilter]);
  const totalCredit = visibleClients.reduce((total, item) => total + item.credito, 0);
  const hasStore = Boolean(selectedStoreId);

  return (
    <section className="space-y-6">
      <div className="rounded-[28px] border border-[var(--border)] bg-white p-6 shadow-[0_18px_45px_rgba(15,23,42,0.05)]">
        <div className="flex flex-col gap-5 lg:flex-row lg:items-end lg:justify-between">
          <div className="space-y-2">
            <p className="text-sm font-medium uppercase tracking-[0.28em] text-[var(--muted)]">
              Pendencias
            </p>
            <h1 className="text-3xl font-semibold tracking-tight text-[var(--foreground)]">
              Creditos em aberto por cliente
            </h1>
            <p className="max-w-2xl text-sm text-[var(--muted)]">
              Atualize as ordens de pagamento pendentes ate a data escolhida para transformalas em
              credito e marcar essas ordens como pagas.
            </p>
          </div>

          <div className="grid gap-3 sm:grid-cols-[minmax(0,220px)_auto] sm:items-end">
            <label className="flex flex-col gap-2 text-sm font-medium text-[var(--foreground)]">
              Data limite
              <input
                type="date"
                value={dateValue}
                onChange={(event) => setDateValue(event.target.value)}
                className="h-12 rounded-2xl border border-[var(--border)] bg-[var(--surface-muted)] px-4 text-sm outline-none transition focus:border-[var(--primary)] focus:bg-white"
              />
            </label>
            {canUpdatePending ? (
              <button
                type="button"
                onClick={handleUpdatePending}
                disabled={!hasStore || updatePendingMutation.isPending}
                className="h-12 rounded-2xl bg-[var(--primary)] px-5 text-sm font-semibold text-white shadow-[0_18px_30px_rgba(106,92,255,0.22)] transition hover:brightness-[1.03] disabled:cursor-not-allowed disabled:opacity-60"
              >
                {updatePendingMutation.isPending ? "Atualizando..." : "Atualizar pendencias"}
              </button>
            ) : null}
          </div>
        </div>

        <div className="mt-6 grid gap-4 md:grid-cols-3">
          <div className="rounded-3xl border border-[var(--border)] bg-[var(--surface-muted)] p-5">
            <p className="text-sm text-[var(--muted)]">Loja ativa</p>
            <p className="mt-2 text-lg font-semibold text-[var(--foreground)]">
              {selectedStore?.nome ?? "Nenhuma loja selecionada"}
            </p>
          </div>
          <div className="rounded-3xl border border-[var(--border)] bg-[var(--surface-muted)] p-5">
            <p className="text-sm text-[var(--muted)]">Clientes com credito</p>
            <p className="mt-2 text-3xl font-semibold text-[var(--foreground)]">
              {visibleClients.length}
            </p>
          </div>
          <div className="rounded-3xl border border-[var(--border)] bg-[var(--surface-muted)] p-5">
            <p className="text-sm text-[var(--muted)]">Credito total</p>
            <p className="mt-2 text-3xl font-semibold text-[var(--foreground)]">
              {formatCurrency(totalCredit)}
            </p>
          </div>
        </div>

        {!hasStore ? (
          <ClientEmptyState
            title="Selecione uma loja"
            description="As pendencias dependem da loja ativa no topo da pagina."
          />
        ) : pendingClientsQuery.isLoading || isLoadingStores ? (
          <ClientEmptyState
            title="Carregando pendencias"
            description="Buscando os clientes com credito diferente de zero."
          />
        ) : pendingClientsQuery.isError ? (
          <ClientEmptyState
            title="Falha ao carregar pendencias"
            description={
              pendingClientsQuery.error instanceof Error
                ? pendingClientsQuery.error.message
                : "Tente novamente em instantes."
            }
          />
        ) : clients.length === 0 ? (
          <ClientEmptyState
            title="Nenhuma pendencia encontrada"
            description="Nao ha clientes com credito diferente de zero para a loja selecionada."
          />
        ) : visibleClients.length === 0 ? (
          <>
            <PendingCreditFilterControls value={creditFilter} onChange={setCreditFilter} />
            <ClientEmptyState
              title="Nenhuma pendencia neste filtro"
              description="Troque o filtro para ver outros saldos da loja selecionada."
            />
          </>
        ) : (
          <>
            <PendingCreditFilterControls value={creditFilter} onChange={setCreditFilter} />

            <div className="mt-4 overflow-hidden rounded-[28px] border border-[var(--border)]">
              <div className="overflow-x-auto">
                <table className="min-w-full border-collapse">
                  <thead className="bg-[var(--surface-muted)]">
                    <tr className="text-left text-xs uppercase tracking-[0.24em] text-[var(--muted)]">
                      <th className="px-5 py-4 font-medium">Cliente</th>
                      <th className="px-5 py-4 font-medium">Contato</th>
                      <th className="px-5 py-4 font-medium text-right">Credito atual</th>
                      <th className="px-5 py-4 font-medium text-right">Acoes</th>
                    </tr>
                  </thead>
                  <tbody>
                    {visibleClients.map((client) => (
                      <tr
                        key={client.clienteId}
                        className="border-t border-[var(--border)] bg-white"
                      >
                        <td className="px-5 py-4">
                          <div className="font-medium text-[var(--foreground)]">{client.nome}</div>
                          <div className="text-sm text-[var(--muted)]">#{client.clienteId}</div>
                        </td>
                        <td className="px-5 py-4 text-sm text-[var(--muted)]">
                          {formatPhone(client.contato)}
                        </td>
                        <td className="px-5 py-4 text-right text-sm font-semibold text-[var(--foreground)]">
                          <span
                            className={`inline-flex rounded-full px-3 py-1 text-xs font-semibold ${getCreditBadgeClass(client.credito)}`}
                          >
                            {formatCurrency(client.credito)}
                          </span>
                        </td>
                        <td className="px-5 py-4 text-right">
                          {canHandleCredit ? (
                            <button
                              type="button"
                              onClick={() => setSelectedClient(client)}
                              className="inline-flex h-10 cursor-pointer items-center justify-center rounded-2xl bg-[linear-gradient(90deg,_#ff8a3d,_#ff6b3d)] px-4 text-sm font-semibold text-white shadow-[0_16px_30px_rgba(255,107,61,0.24)] transition hover:brightness-105"
                            >
                              Pagamento
                            </button>
                          ) : (
                            <span className="text-sm text-[var(--muted)]">Sem acoes</span>
                          )}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          </>
        )}
      </div>

      {canHandleCredit ? (
        <PaymentCreditModal
          client={selectedClient}
          isOpen={Boolean(selectedClient)}
          storeId={selectedStoreId}
          storeName={selectedStore?.nome ?? null}
          onClose={() => setSelectedClient(null)}
          onSuccess={async () => {
            await queryClient.invalidateQueries({ queryKey: ["pending-clients"] });
          }}
        />
      ) : null}
    </section>
  );
}
