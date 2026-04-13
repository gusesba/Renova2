"use client";

import { useEffect, useState } from "react";
import { createPortal } from "react-dom";
import { toast } from "sonner";

import type { StorePaymentMethodFormValue } from "@/lib/store-config";

type StorePaymentMethodConfigModalProps = {
  isOpen: boolean;
  paymentMethods: StorePaymentMethodFormValue[];
  isSavingParent: boolean;
  onClose: () => void;
  onSave: (paymentMethods: StorePaymentMethodFormValue[]) => void;
};

function createPaymentMethodItem(): StorePaymentMethodFormValue {
  return {
    id: crypto.randomUUID(),
    nome: "",
    percentualAjuste: "",
  };
}

export function StorePaymentMethodConfigModal({
  isOpen,
  paymentMethods,
  isSavingParent,
  onClose,
  onSave,
}: StorePaymentMethodConfigModalProps) {
  const [shouldRender, setShouldRender] = useState(isOpen);
  const [isVisible, setIsVisible] = useState(false);
  const [draftPaymentMethods, setDraftPaymentMethods] = useState<StorePaymentMethodFormValue[]>(
    paymentMethods.length > 0 ? paymentMethods : [createPaymentMethodItem()],
  );

  useEffect(() => {
    let syncTimeout = 0;

    if (isOpen) {
      syncTimeout = window.setTimeout(() => {
        setDraftPaymentMethods(paymentMethods.length > 0 ? paymentMethods : [createPaymentMethodItem()]);
      }, 0);
    }

    return () => {
      window.clearTimeout(syncTimeout);
    };
  }, [isOpen, paymentMethods]);

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
      if (event.key === "Escape" && !isSavingParent) {
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
  }, [isOpen, isSavingParent, onClose, shouldRender]);

  if (!shouldRender || typeof document === "undefined") {
    return null;
  }

  function updatePaymentMethod(
    id: string,
    field: "nome" | "percentualAjuste",
    value: string,
  ) {
    setDraftPaymentMethods((current) =>
      current.map((item) => (item.id === id ? { ...item, [field]: value } : item)),
    );
  }

  function handleAddPaymentMethod() {
    setDraftPaymentMethods((current) => [...current, createPaymentMethodItem()]);
  }

  function handleRemovePaymentMethod(id: string) {
    setDraftPaymentMethods((current) => current.filter((item) => item.id !== id));
  }

  function handleSave() {
    const normalizedPaymentMethods = draftPaymentMethods.filter(
      (item) => item.nome.trim() || item.percentualAjuste.trim(),
    );

    const parsedPaymentMethods = normalizedPaymentMethods.map((item) => ({
      ...item,
      nomeNormalizado: item.nome.trim(),
      percentualAjusteNumero: Number(item.percentualAjuste.replace(",", ".").trim()),
    }));

    if (
      parsedPaymentMethods.some(
        (item) =>
          !item.nomeNormalizado ||
          item.nomeNormalizado.length > 100 ||
          Number.isNaN(item.percentualAjusteNumero) ||
          item.percentualAjusteNumero < -100 ||
          item.percentualAjusteNumero > 100,
      )
    ) {
      toast.error("Revise as formas de pagamento: nome obrigatorio e percentual entre -100 e 100.");
      return;
    }

    const uniqueNames = new Set(parsedPaymentMethods.map((item) => item.nomeNormalizado.toLowerCase()));

    if (uniqueNames.size !== parsedPaymentMethods.length) {
      toast.error("Nao e permitido repetir formas de pagamento com o mesmo nome.");
      return;
    }

    onSave(
      parsedPaymentMethods
        .sort((left, right) => left.nomeNormalizado.localeCompare(right.nomeNormalizado))
        .map(({ id, nomeNormalizado, percentualAjusteNumero }) => ({
          id,
          nome: nomeNormalizado,
          percentualAjuste: String(percentualAjusteNumero),
        })),
    );
    onClose();
  }

  return createPortal(
    <div
      className={`fixed inset-0 z-[210] flex items-center justify-center bg-[rgba(15,23,42,0.52)] p-4 transition-opacity duration-200 ease-out ${
        isVisible ? "opacity-100" : "opacity-0"
      }`}
    >
      <div
        className={`w-full max-w-2xl rounded-[28px] border border-[var(--border)] bg-white p-6 shadow-[0_30px_90px_rgba(15,23,42,0.24)] transition duration-250 ease-out ${
          isVisible ? "translate-y-0 scale-100 opacity-100" : "translate-y-4 scale-[0.98] opacity-0"
        }`}
      >
        <div className="flex items-start justify-between gap-4">
          <div>
            <p className="text-sm font-semibold uppercase tracking-[0.16em] text-[var(--muted)]">
              Formas de pagamento
            </p>
            <h2 className="mt-2 text-2xl font-semibold tracking-tight text-[var(--foreground)]">
              Configurar formas de pagamento
            </h2>
            <p className="mt-2 text-sm leading-7 text-[var(--muted)]">
              Cadastre o nome e o percentual aplicado em cada forma de pagamento. Use valor
              positivo para taxa e negativo para desconto.
            </p>
          </div>

          <button
            type="button"
            onClick={onClose}
            disabled={isSavingParent}
            className="flex h-11 w-11 cursor-pointer items-center justify-center rounded-2xl border border-[var(--border)] bg-[var(--surface-muted)] text-[var(--muted)] transition hover:border-[var(--border-strong)] hover:text-[var(--foreground)] disabled:cursor-not-allowed disabled:opacity-60"
            aria-label="Fechar configuracao de formas de pagamento"
          >
            x
          </button>
        </div>

        <div className="mt-6 space-y-4">
          {draftPaymentMethods.length === 0 ? (
            <div className="rounded-2xl border border-dashed border-[var(--border)] bg-[var(--surface-muted)] px-4 py-5 text-sm text-[var(--muted)]">
              Nenhuma forma cadastrada. Adicione a primeira configuracao.
            </div>
          ) : null}

          {draftPaymentMethods.map((paymentMethod, index) => (
            <div
              key={paymentMethod.id}
              className="grid gap-3 rounded-2xl border border-[var(--border)] bg-[var(--surface-muted)] p-4 md:grid-cols-[1.3fr_1fr_auto]"
            >
              <label className="block space-y-2">
                <span className="text-sm font-semibold text-[var(--foreground)]">Nome</span>
                <input
                  type="text"
                  value={paymentMethod.nome}
                  disabled={isSavingParent}
                  onChange={(event) => updatePaymentMethod(paymentMethod.id, "nome", event.target.value)}
                  className="h-12 w-full rounded-2xl border border-[var(--border)] bg-white px-4 text-sm text-[var(--foreground)] outline-none transition focus:border-[var(--primary)] focus:shadow-[0_0_0_4px_rgba(106,92,255,0.12)] disabled:cursor-not-allowed disabled:bg-[var(--surface-muted)]"
                  placeholder="Ex.: Cartao de credito"
                />
              </label>

              <label className="block space-y-2">
                <span className="text-sm font-semibold text-[var(--foreground)]">
                  Taxa ou desconto
                </span>
                <div className="relative">
                  <input
                    type="number"
                    min={-100}
                    max={100}
                    step="0.01"
                    value={paymentMethod.percentualAjuste}
                    disabled={isSavingParent}
                    onChange={(event) =>
                      updatePaymentMethod(paymentMethod.id, "percentualAjuste", event.target.value)
                    }
                    className="h-12 w-full rounded-2xl border border-[var(--border)] bg-white px-4 pr-12 text-sm text-[var(--foreground)] outline-none transition focus:border-[var(--primary)] focus:shadow-[0_0_0_4px_rgba(106,92,255,0.12)] disabled:cursor-not-allowed disabled:bg-[var(--surface-muted)]"
                    placeholder="Ex.: 3.5 ou -5"
                  />
                  <span className="pointer-events-none absolute right-4 top-1/2 -translate-y-1/2 text-sm font-semibold text-[var(--muted)]">
                    %
                  </span>
                </div>
              </label>

              <div className="flex items-end">
                <button
                  type="button"
                  onClick={() => handleRemovePaymentMethod(paymentMethod.id)}
                  disabled={isSavingParent}
                  className="flex h-12 w-full cursor-pointer items-center justify-center rounded-2xl border border-[var(--border)] bg-white px-4 text-sm font-semibold text-[var(--foreground)] transition hover:border-[var(--border-strong)] disabled:cursor-not-allowed disabled:opacity-60 md:w-auto"
                >
                  Remover
                </button>
              </div>

              <p className="text-xs uppercase tracking-[0.16em] text-[var(--muted)] md:col-span-3">
                Forma {index + 1}
              </p>
            </div>
          ))}

          <button
            type="button"
            onClick={handleAddPaymentMethod}
            disabled={isSavingParent}
            className="flex h-12 w-full cursor-pointer items-center justify-center rounded-2xl border border-dashed border-[var(--border-strong)] bg-white px-5 text-sm font-semibold text-[var(--foreground)] transition hover:bg-[var(--surface-muted)] disabled:cursor-not-allowed disabled:opacity-60"
          >
            Adicionar forma de pagamento
          </button>
        </div>

        <div className="mt-6 flex flex-col gap-3 sm:flex-row sm:justify-end">
          <button
            type="button"
            onClick={onClose}
            disabled={isSavingParent}
            className="flex h-12 cursor-pointer items-center justify-center rounded-2xl border border-[var(--border)] bg-[var(--surface-muted)] px-5 text-sm font-semibold text-[var(--foreground)] transition hover:border-[var(--border-strong)] hover:bg-white disabled:cursor-not-allowed disabled:opacity-60"
          >
            Fechar
          </button>
          <button
            type="button"
            onClick={handleSave}
            disabled={isSavingParent}
            className="flex h-12 cursor-pointer items-center justify-center rounded-2xl bg-[linear-gradient(90deg,_#ff8a3d,_#ff6b3d)] px-5 text-sm font-semibold text-white shadow-[0_16px_30px_rgba(255,107,61,0.28)] transition hover:brightness-105 disabled:cursor-not-allowed disabled:opacity-60"
          >
            Aplicar formas
          </button>
        </div>
      </div>
    </div>,
    document.body,
  );
}
