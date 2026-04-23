"use client";

import { useEffect, useMemo, useState } from "react";
import { createPortal } from "react-dom";
import { toast } from "sonner";

import { Select } from "@/app/components/ui/select";
import { formatCurrency, getTodayDateInputValue } from "@/lib/payment";
import { getAuthToken } from "@/lib/store";
import {
  getStoreExpenseApiMessage,
  storeExpenseNatureOptions,
} from "@/lib/store-expense";
import { createStoreExpense } from "@/services/store-expense-service";

type StoreExpenseCreateModalProps = {
  isOpen: boolean;
  storeId: number | null;
  storeName: string | null;
  onClose: () => void;
  onSuccess: () => Promise<void> | void;
};

export function StoreExpenseCreateModal({
  isOpen,
  storeId,
  storeName,
  onClose,
  onSuccess,
}: StoreExpenseCreateModalProps) {
  const [shouldRender, setShouldRender] = useState(isOpen);
  const [isVisible, setIsVisible] = useState(false);
  const [dateValue, setDateValue] = useState(() => getTodayDateInputValue());
  const [natureza, setNatureza] = useState("2");
  const [descricao, setDescricao] = useState("");
  const [valor, setValor] = useState("");
  const [isSaving, setIsSaving] = useState(false);

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
    setDescricao("");
    setValor("");
  }, [isOpen, storeId]);

  const parsedValue = useMemo(() => {
    const normalized = valor.replace(",", ".").trim();
    const parsed = Number(normalized);
    return Number.isFinite(parsed) && parsed > 0 ? parsed : 0;
  }, [valor]);

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
      toast.error("Voce precisa estar autenticado para lancar um gasto da loja.");
      return;
    }

    if (!dateValue) {
      toast.error("Informe a data do lancamento.");
      return;
    }

    if (parsedValue <= 0) {
      toast.error("Informe um valor maior que zero.");
      return;
    }

    setIsSaving(true);

    try {
      const response = await createStoreExpense(
        {
          lojaId: storeId,
          natureza: Number(natureza),
          valor: parsedValue,
          data: toUtcStartOfDay(dateValue),
          descricao: descricao.trim() || undefined,
        },
        currentToken,
      );

      if (!response.ok) {
        toast.error(
          getStoreExpenseApiMessage(response.body) ??
            "Nao foi possivel criar o gasto da loja.",
        );
        return;
      }

      await onSuccess();
      toast.success(`Lancamento salvo no valor de ${formatCurrency(parsedValue)}.`);
      onClose();
    } catch {
      toast.error("Nao foi possivel conectar ao backend. Verifique se a API esta em execucao.");
    } finally {
      setIsSaving(false);
    }
  }

  return createPortal(
    <div
      className={`fixed inset-0 z-[220] flex items-start justify-center overflow-y-auto bg-[rgba(15,23,42,0.45)] px-4 pt-[calc(env(safe-area-inset-top,0px)+1rem)] pb-[calc(env(safe-area-inset-bottom,0px)+1.5rem)] transition-opacity duration-200 ease-out sm:items-center sm:px-4 sm:pt-4 sm:pb-4 ${
        isVisible ? "opacity-100" : "opacity-0"
      }`}
    >
      <div
        className={`max-h-[calc(100dvh-env(safe-area-inset-top,0px)-env(safe-area-inset-bottom,0px)-2.5rem)] w-full max-w-2xl overflow-y-auto rounded-[28px] border border-[var(--border)] bg-white p-6 shadow-[0_30px_90px_rgba(15,23,42,0.22)] transition duration-250 ease-out sm:max-h-[calc(100vh-2rem)] ${
          isVisible ? "translate-y-0 scale-100 opacity-100" : "translate-y-4 scale-[0.98] opacity-0"
        }`}
      >
        <div className="flex items-start justify-between gap-4">
          <div>
            <p className="text-sm font-semibold uppercase tracking-[0.16em] text-[var(--muted)]">
              Gastos da loja
            </p>
            <h2 className="mt-2 text-2xl font-semibold tracking-tight text-[var(--foreground)]">
              Novo lancamento
            </h2>
            <p className="mt-2 text-sm leading-7 text-[var(--muted)]">
              Loja ativa: {storeName ?? "Nenhuma loja selecionada"}.
            </p>
          </div>

          <button
            type="button"
            onClick={onClose}
            disabled={isSaving}
            className="flex h-11 w-11 cursor-pointer items-center justify-center rounded-2xl border border-[var(--border)] bg-[var(--surface-muted)] text-[var(--muted)] transition hover:border-[var(--border-strong)] hover:text-[var(--foreground)] disabled:cursor-not-allowed disabled:opacity-60"
            aria-label="Fechar novo gasto da loja"
          >
            x
          </button>
        </div>

        <form className="mt-6 space-y-6" onSubmit={handleSubmit}>
          <div className="grid gap-5 md:grid-cols-2">
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
              <span className="text-sm font-semibold text-[var(--foreground)]">Natureza</span>
              <div className="rounded-2xl border border-[var(--border)] bg-white px-4 py-3 text-sm text-[var(--foreground)]">
                <Select
                  ariaLabel="Natureza do gasto da loja"
                  value={natureza}
                  options={storeExpenseNatureOptions.map((option) => ({
                    label: option.label,
                    value: String(option.value),
                  }))}
                  onChange={(value) => setNatureza(value)}
                />
              </div>
            </label>

            <label className="block space-y-2 md:col-span-2">
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
              placeholder="Ex.: compra de cabide, reforma do telhado, conta de luz"
            />
          </label>

          <div className="rounded-3xl border border-[var(--border)] bg-[var(--surface-muted)] p-5">
            <p className="text-sm text-[var(--muted)]">Resumo do lancamento</p>
            <p className="mt-2 text-lg font-semibold text-[var(--foreground)]">
              {storeExpenseNatureOptions.find((option) => String(option.value) === natureza)?.label}
            </p>
            <p className="mt-1 text-sm text-[var(--muted)]">
              Valor informado: {formatCurrency(parsedValue)}
            </p>
          </div>

          <div className="flex flex-col gap-3 sm:flex-row sm:justify-end">
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
              {isSaving ? "Salvando..." : "Salvar lancamento"}
            </button>
          </div>
        </form>
      </div>
    </div>,
    document.body,
  );
}
