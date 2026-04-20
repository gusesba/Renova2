"use client";

import { useQuery } from "@tanstack/react-query";
import { useEffect, useMemo, useState } from "react";
import { createPortal } from "react-dom";
import { toast } from "sonner";

import { SearchableSelect } from "@/app/components/ui/searchable-select";
import { Select } from "@/app/components/ui/select";
import {
  asClientListResponse,
  formatPhoneValue,
  getClientApiMessage,
  initialClientFilters,
} from "@/lib/client";
import {
  formatCurrency,
  getPaymentApiMessage,
  getTodayDateInputValue,
  paymentNatureOptions,
} from "@/lib/payment";
import { getAuthToken } from "@/lib/store";
import { getClients } from "@/services/client-service";
import { createManualPayment } from "@/services/payment-service";

type PaymentCreateModalProps = {
  isOpen: boolean;
  storeId: number | null;
  storeName: string | null;
  onClose: () => void;
  onSuccess: () => Promise<void> | void;
};

export function PaymentCreateModal({
  isOpen,
  storeId,
  storeName,
  onClose,
  onSuccess,
}: PaymentCreateModalProps) {
  const [shouldRender, setShouldRender] = useState(isOpen);
  const [isVisible, setIsVisible] = useState(false);
  const [dateValue, setDateValue] = useState(() => getTodayDateInputValue());
  const [natureza, setNatureza] = useState("2");
  const [clienteId, setClienteId] = useState("");
  const [clienteLabel, setClienteLabel] = useState("");
  const [clienteSearch, setClienteSearch] = useState("");
  const [descricao, setDescricao] = useState("");
  const [valor, setValor] = useState("");
  const [isSaving, setIsSaving] = useState(false);
  const [debouncedClientSearch, setDebouncedClientSearch] = useState("");

  useEffect(() => {
    let animationFrame = 0;
    let visibilityFrame = 0;
    let closeTimeout = 0;

    if (isOpen) {
      animationFrame = window.requestAnimationFrame(() => {
        setShouldRender(true);
        visibilityFrame = window.requestAnimationFrame(() => {
          setIsVisible(true);
        });
      });
    } else if (shouldRender) {
      animationFrame = window.requestAnimationFrame(() => {
        setIsVisible(false);
      });

      closeTimeout = window.setTimeout(() => {
        setShouldRender(false);
      }, 220);
    }

    function handleEscape(event: KeyboardEvent) {
      if (event.key === "Escape" && !isSaving) {
        onClose();
      }
    }

    window.addEventListener("keydown", handleEscape);

    return () => {
      window.cancelAnimationFrame(animationFrame);
      window.cancelAnimationFrame(visibilityFrame);
      window.clearTimeout(closeTimeout);
      window.removeEventListener("keydown", handleEscape);
    };
  }, [isOpen, isSaving, onClose, shouldRender]);

  useEffect(() => {
    if (!isOpen) {
      return;
    }

    setDateValue(getTodayDateInputValue());
    setNatureza("2");
    setClienteId("");
    setClienteLabel("");
    setClienteSearch("");
    setDescricao("");
    setValor("");
  }, [isOpen, storeId]);

  useEffect(() => {
    const timeoutId = window.setTimeout(() => {
      setDebouncedClientSearch(clienteSearch);
    }, 300);

    return () => {
      window.clearTimeout(timeoutId);
    };
  }, [clienteSearch]);

  const token = useMemo(() => (typeof window === "undefined" ? null : getAuthToken()), []);
  const trimmedClientSearch = debouncedClientSearch.trim();

  const clientsQuery = useQuery({
    queryKey: ["manual-payment-clients", token, storeId, trimmedClientSearch],
    queryFn: async () => {
      if (!token || !storeId) {
        return [];
      }

      const response = await getClients(token, storeId, {
        ...initialClientFilters,
        nome: trimmedClientSearch,
        tamanhoPagina: 5,
      });

      if (!response.ok) {
        throw new Error(
          getClientApiMessage(response.body) ?? "Nao foi possivel carregar os clientes.",
        );
      }

      return asClientListResponse(response.body).itens;
    },
    enabled: Boolean(token && storeId && isOpen && trimmedClientSearch),
  });

  const clientOptions = useMemo(
    () =>
      (clientsQuery.data ?? []).map((client) => ({
        label: `${client.nome} - ${formatPhoneValue(client.contato)}`,
        value: String(client.id),
        raw: client,
      })),
    [clientsQuery.data],
  );

  const selectedClient =
    clientsQuery.data?.find((client) => client.id === Number(clienteId)) ?? null;
  const parsedValue = useMemo(() => {
    const normalized = valor.replace(",", ".").trim();
    const parsed = Number(normalized);
    return Number.isFinite(parsed) && parsed > 0 ? parsed : 0;
  }, [valor]);

  const creditEffectLabel =
    natureza === "2"
      ? `Credito apos salvar: +${formatCurrency(parsedValue)}`
      : `Credito apos salvar: -${formatCurrency(parsedValue)}`;

  if (typeof document === "undefined" || !shouldRender) {
    return null;
  }

  function toUtcStartOfDay(value: string) {
    return new Date(`${value}T00:00:00.000Z`).toISOString();
  }

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (isSaving || !storeId) {
      return;
    }

    const currentToken = getAuthToken();

    if (!currentToken) {
      toast.error("Voce precisa estar autenticado para lancar um pagamento.");
      return;
    }

    if (!clienteId) {
      toast.error("Selecione um cliente.");
      return;
    }

    if (!dateValue) {
      toast.error("Informe a data do pagamento.");
      return;
    }

    if (parsedValue <= 0) {
      toast.error("Informe um valor maior que zero.");
      return;
    }

    setIsSaving(true);

    try {
      const response = await createManualPayment(
        {
          lojaId: storeId,
          clienteId: Number(clienteId),
          natureza: Number(natureza),
          valor: parsedValue,
          data: toUtcStartOfDay(dateValue),
          descricao: descricao.trim() || undefined,
        },
        currentToken,
      );

      if (!response.ok) {
        toast.error(getPaymentApiMessage(response.body) ?? "Nao foi possivel criar o pagamento.");
        return;
      }

      await onSuccess();
      toast.success(
        `Pagamento manual faturado para ${selectedClient?.nome ?? `cliente #${clienteId}`} no valor de ${formatCurrency(parsedValue)}.`,
      );
      onClose();
    } catch {
      toast.error("Nao foi possivel conectar ao backend. Verifique se a API esta em execucao.");
    } finally {
      setIsSaving(false);
    }
  }

  return createPortal(
    <div
      className={`fixed inset-0 z-[220] flex items-center justify-center bg-[rgba(15,23,42,0.45)] p-4 transition-opacity duration-200 ease-out ${
        isVisible ? "opacity-100" : "opacity-0"
      }`}
    >
      <div
        className={`flex max-h-[calc(100vh-2rem)] w-full max-w-[calc(100vw-2rem)] flex-col overflow-hidden rounded-[28px] border border-[var(--border)] bg-white p-6 shadow-[0_30px_90px_rgba(15,23,42,0.22)] transition duration-250 ease-out sm:max-h-[calc(100vh-3rem)] sm:max-w-3xl ${
          isVisible ? "translate-y-0 scale-100 opacity-100" : "translate-y-4 scale-[0.98] opacity-0"
        }`}
      >
        <div className="flex items-start justify-between gap-4">
          <div>
            <p className="text-sm font-semibold uppercase tracking-[0.16em] text-[var(--muted)]">
              Pagamento manual
            </p>
            <h2 className="mt-2 text-2xl font-semibold tracking-tight text-[var(--foreground)]">
              Novo pagamento
            </h2>
            <p className="mt-2 text-sm leading-7 text-[var(--muted)]">
              Loja ativa: {storeName ?? "Nenhuma loja selecionada"}. O pagamento sera salvo como
              faturado e sem movimentacao vinculada.
            </p>
          </div>

          <button
            type="button"
            onClick={onClose}
            disabled={isSaving}
            className="flex h-11 w-11 cursor-pointer items-center justify-center rounded-2xl border border-[var(--border)] bg-[var(--surface-muted)] text-[var(--muted)] transition hover:border-[var(--border-strong)] hover:text-[var(--foreground)] disabled:cursor-not-allowed disabled:opacity-60"
            aria-label="Fechar novo pagamento"
          >
            x
          </button>
        </div>

        <form className="mt-6 flex min-h-0 flex-1 flex-col" onSubmit={handleSubmit}>
          <div className="min-h-0 flex-1 space-y-6 overflow-y-auto pr-1">
            <div className="grid gap-5 md:grid-cols-2">
            <div className="md:col-span-2">
              <span className="mb-2 block text-sm font-semibold text-[var(--foreground)]">
                Cliente
              </span>
              <SearchableSelect
                ariaLabel="Cliente do pagamento"
                disabled={!storeId}
                emptyLabel={
                  !trimmedClientSearch
                    ? "Digite para buscar clientes."
                    : clientsQuery.isError
                      ? "Falha ao carregar os clientes."
                      : "Nenhum cliente encontrado."
                }
                loading={Boolean(trimmedClientSearch) && clientsQuery.isLoading}
                options={
                  trimmedClientSearch
                    ? clientOptions.map((option) => ({
                        label: option.label,
                        value: option.value,
                      }))
                    : []
                }
                placeholder="Selecione um cliente"
                searchPlaceholder="Pesquisar por nome"
                searchValue={clienteSearch}
                selectedLabel={clienteLabel}
                value={clienteId || null}
                onSearchChange={(value) => setClienteSearch(value)}
                onChange={(option) => {
                  const client = clientsQuery.data?.find((item) => item.id === Number(option.value));
                  if (!client) {
                    return;
                  }

                  setClienteId(String(client.id));
                  setClienteLabel(client.nome);
                  setClienteSearch("");
                }}
              />
            </div>

            <label className="block space-y-2">
              <span className="text-sm font-semibold text-[var(--foreground)]">Natureza</span>
              <div className="rounded-2xl border border-[var(--border)] bg-white px-4 py-3 text-sm text-[var(--foreground)]">
                <Select
                  ariaLabel="Natureza do pagamento"
                  value={natureza}
                  options={paymentNatureOptions.map((option) => ({
                    label: option.label,
                    value: String(option.value),
                  }))}
                  onChange={(value) => setNatureza(value)}
                />
              </div>
            </label>

            <label className="block space-y-2">
              <span className="text-sm font-semibold text-[var(--foreground)]">Data</span>
              <input
                type="date"
                value={dateValue}
                onChange={(event) => setDateValue(event.target.value)}
                disabled={isSaving}
                className="h-12 w-full rounded-2xl border border-[var(--border)] bg-white px-4 text-sm text-[var(--foreground)] outline-none transition focus:border-[var(--primary)] focus:shadow-[0_0_0_4px_rgba(106,92,255,0.12)] disabled:cursor-not-allowed disabled:bg-[var(--surface-muted)]"
              />
            </label>

            <label className="block space-y-2">
              <span className="text-sm font-semibold text-[var(--foreground)]">Valor</span>
              <div className="relative">
                <input
                  type="number"
                  min={0}
                  step="0.01"
                  value={valor}
                  onChange={(event) => setValor(event.target.value)}
                  disabled={isSaving}
                  className="h-12 w-full rounded-2xl border border-[var(--border)] bg-white px-4 pr-16 text-sm text-[var(--foreground)] outline-none transition focus:border-[var(--primary)] focus:shadow-[0_0_0_4px_rgba(106,92,255,0.12)] disabled:cursor-not-allowed disabled:bg-[var(--surface-muted)]"
                  placeholder="0,00"
                />
                <span className="pointer-events-none absolute right-4 top-1/2 -translate-y-1/2 text-sm font-semibold text-[var(--muted)]">
                  R$
                </span>
              </div>
              <p className="text-sm text-[var(--muted)]">
                O valor deve ser positivo. Recebimentos reduzem credito; pagamentos aumentam.
              </p>
            </label>

            <label className="block space-y-2">
              <span className="text-sm font-semibold text-[var(--foreground)]">Status</span>
              <div className="flex h-12 items-center rounded-2xl border border-[var(--border)] bg-[var(--surface-muted)] px-4 text-sm font-semibold text-[var(--foreground)]">
                Faturado
              </div>
            </label>
            </div>

            <label className="block space-y-2">
              <span className="text-sm font-semibold text-[var(--foreground)]">Descricao</span>
              <textarea
                value={descricao}
                onChange={(event) => setDescricao(event.target.value)}
                disabled={isSaving}
                rows={4}
                maxLength={500}
                className="w-full rounded-2xl border border-[var(--border)] bg-white px-4 py-3 text-sm text-[var(--foreground)] outline-none transition focus:border-[var(--primary)] focus:shadow-[0_0_0_4px_rgba(106,92,255,0.12)] disabled:cursor-not-allowed disabled:bg-[var(--surface-muted)]"
                placeholder="Descreva o motivo ou contexto do pagamento"
              />
            </label>

            <div className="grid gap-4 md:grid-cols-2">
              <div className="rounded-3xl border border-[var(--border)] bg-[var(--surface-muted)] p-5">
                <p className="text-sm text-[var(--muted)]">Cliente selecionado</p>
                <p className="mt-2 text-lg font-semibold text-[var(--foreground)]">
                  {selectedClient?.nome ?? "Nenhum cliente selecionado"}
                </p>
                <p className="mt-1 text-sm text-[var(--muted)]">
                  {selectedClient ? formatPhoneValue(selectedClient.contato) : "Escolha um cliente da loja ativa."}
                </p>
              </div>

              <div className="rounded-3xl border border-[var(--border)] bg-[var(--surface-muted)] p-5">
                <p className="text-sm text-[var(--muted)]">Efeito imediato no credito</p>
                <p className="mt-2 text-lg font-semibold text-[var(--foreground)]">
                  {creditEffectLabel}
                </p>
                <p className="mt-1 text-sm text-[var(--muted)]">
                  Este lancamento nao cria nem exige movimentacao vinculada.
                </p>
              </div>
            </div>

            {clientsQuery.isError ? (
              <div className="rounded-2xl border border-amber-200 bg-amber-50 px-4 py-4 text-sm text-amber-900">
                {clientsQuery.error instanceof Error
                  ? clientsQuery.error.message
                  : "Nao foi possivel carregar os clientes."}
              </div>
            ) : null}
          </div>

          <div className="mt-6 flex flex-col gap-3 border-t border-[var(--border)] pt-4 sm:flex-row sm:justify-end">
            <button
              type="button"
              onClick={onClose}
              disabled={isSaving}
              className="flex h-12 cursor-pointer items-center justify-center rounded-2xl border border-[var(--border)] bg-[var(--surface-muted)] px-5 text-sm font-semibold text-[var(--foreground)] transition hover:border-[var(--border-strong)] hover:bg-white disabled:cursor-not-allowed disabled:opacity-60"
            >
              Cancelar
            </button>
            <button
              type="submit"
              disabled={isSaving || !storeId}
              className="flex h-12 cursor-pointer items-center justify-center rounded-2xl bg-[linear-gradient(90deg,_#ff8a3d,_#ff6b3d)] px-5 text-sm font-semibold text-white shadow-[0_16px_30px_rgba(255,107,61,0.28)] transition hover:brightness-105 disabled:cursor-not-allowed disabled:opacity-60"
            >
              {isSaving ? "Salvando..." : "Salvar pagamento"}
            </button>
          </div>
        </form>
      </div>
    </div>,
    document.body,
  );
}
