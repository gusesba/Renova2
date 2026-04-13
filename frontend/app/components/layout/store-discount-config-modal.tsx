"use client";

import { useEffect, useState } from "react";
import { createPortal } from "react-dom";
import { toast } from "sonner";

import type { StoreDiscountFormValue } from "@/lib/store-config";

type StoreDiscountConfigModalProps = {
  isOpen: boolean;
  discounts: StoreDiscountFormValue[];
  isSavingParent: boolean;
  onClose: () => void;
  onSave: (discounts: StoreDiscountFormValue[]) => void;
};

function createDiscountItem(): StoreDiscountFormValue {
  return {
    id: crypto.randomUUID(),
    aPartirDeMeses: "",
    percentualDesconto: "",
  };
}

export function StoreDiscountConfigModal({
  isOpen,
  discounts,
  isSavingParent,
  onClose,
  onSave,
}: StoreDiscountConfigModalProps) {
  const [shouldRender, setShouldRender] = useState(isOpen);
  const [isVisible, setIsVisible] = useState(false);
  const [draftDiscounts, setDraftDiscounts] = useState<StoreDiscountFormValue[]>(
    discounts.length > 0 ? discounts : [createDiscountItem()],
  );

  useEffect(() => {
    let syncTimeout = 0;

    if (isOpen) {
      syncTimeout = window.setTimeout(() => {
        setDraftDiscounts(discounts.length > 0 ? discounts : [createDiscountItem()]);
      }, 0);
    }

    return () => {
      window.clearTimeout(syncTimeout);
    };
  }, [discounts, isOpen]);

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

  function updateDiscount(
    id: string,
    field: "aPartirDeMeses" | "percentualDesconto",
    value: string,
  ) {
    setDraftDiscounts((current) =>
      current.map((item) => (item.id === id ? { ...item, [field]: value } : item)),
    );
  }

  function handleAddDiscount() {
    setDraftDiscounts((current) => [...current, createDiscountItem()]);
  }

  function handleRemoveDiscount(id: string) {
    setDraftDiscounts((current) => current.filter((item) => item.id !== id));
  }

  function handleSave() {
    const normalizedDiscounts = draftDiscounts.filter(
      (item) => item.aPartirDeMeses.trim() || item.percentualDesconto.trim(),
    );

    const parsedDiscounts = normalizedDiscounts.map((item) => ({
      ...item,
      aPartirDeMesesNumber: Number(item.aPartirDeMeses.trim()),
      percentualDescontoNumber: Number(item.percentualDesconto.replace(",", ".").trim()),
    }));

    if (
      parsedDiscounts.some(
        (item) =>
          !Number.isInteger(item.aPartirDeMesesNumber) ||
          item.aPartirDeMesesNumber < 1 ||
          Number.isNaN(item.percentualDescontoNumber) ||
          item.percentualDescontoNumber < 0 ||
          item.percentualDescontoNumber > 100,
      )
    ) {
      toast.error("Revise as faixas: meses devem ser inteiros maiores que zero e desconto entre 0 e 100.");
      return;
    }

    const uniqueMonths = new Set(parsedDiscounts.map((item) => item.aPartirDeMesesNumber));

    if (uniqueMonths.size !== parsedDiscounts.length) {
      toast.error("Nao e permitido repetir a mesma quantidade de meses em mais de uma faixa.");
      return;
    }

    onSave(
      parsedDiscounts
        .sort((left, right) => left.aPartirDeMesesNumber - right.aPartirDeMesesNumber)
        .map(({ id, aPartirDeMesesNumber, percentualDescontoNumber }) => ({
          id,
          aPartirDeMeses: String(aPartirDeMesesNumber),
          percentualDesconto: String(percentualDescontoNumber),
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
              Descontos por permanencia
            </p>
            <h2 className="mt-2 text-2xl font-semibold tracking-tight text-[var(--foreground)]">
              Configurar descontos
            </h2>
            <p className="mt-2 text-sm leading-7 text-[var(--muted)]">
              Defina regras como “a partir de X meses, aplicar Y% de desconto”.
            </p>
          </div>

          <button
            type="button"
            onClick={onClose}
            disabled={isSavingParent}
            className="flex h-11 w-11 cursor-pointer items-center justify-center rounded-2xl border border-[var(--border)] bg-[var(--surface-muted)] text-[var(--muted)] transition hover:border-[var(--border-strong)] hover:text-[var(--foreground)] disabled:cursor-not-allowed disabled:opacity-60"
            aria-label="Fechar configuracao de descontos"
          >
            x
          </button>
        </div>

        <div className="mt-6 space-y-4">
          {draftDiscounts.length === 0 ? (
            <div className="rounded-2xl border border-dashed border-[var(--border)] bg-[var(--surface-muted)] px-4 py-5 text-sm text-[var(--muted)]">
              Nenhuma faixa cadastrada. Adicione a primeira regra de desconto.
            </div>
          ) : null}

          {draftDiscounts.map((discount, index) => (
            <div
              key={discount.id}
              className="grid gap-3 rounded-2xl border border-[var(--border)] bg-[var(--surface-muted)] p-4 md:grid-cols-[1fr_1fr_auto]"
            >
              <label className="block space-y-2">
                <span className="text-sm font-semibold text-[var(--foreground)]">
                  A partir de quantos meses
                </span>
                <div className="relative">
                  <input
                    type="number"
                    min={1}
                    step="1"
                    value={discount.aPartirDeMeses}
                    disabled={isSavingParent}
                    onChange={(event) =>
                      updateDiscount(discount.id, "aPartirDeMeses", event.target.value)
                    }
                    className="h-12 w-full rounded-2xl border border-[var(--border)] bg-white px-4 pr-20 text-sm text-[var(--foreground)] outline-none transition focus:border-[var(--primary)] focus:shadow-[0_0_0_4px_rgba(106,92,255,0.12)] disabled:cursor-not-allowed disabled:bg-[var(--surface-muted)]"
                    placeholder="Ex.: 6"
                  />
                  <span className="pointer-events-none absolute right-4 top-1/2 -translate-y-1/2 text-sm font-semibold text-[var(--muted)]">
                    meses
                  </span>
                </div>
              </label>

              <label className="block space-y-2">
                <span className="text-sm font-semibold text-[var(--foreground)]">
                  Percentual de desconto
                </span>
                <div className="relative">
                  <input
                    type="number"
                    min={0}
                    max={100}
                    step="0.01"
                    value={discount.percentualDesconto}
                    disabled={isSavingParent}
                    onChange={(event) =>
                      updateDiscount(discount.id, "percentualDesconto", event.target.value)
                    }
                    className="h-12 w-full rounded-2xl border border-[var(--border)] bg-white px-4 pr-12 text-sm text-[var(--foreground)] outline-none transition focus:border-[var(--primary)] focus:shadow-[0_0_0_4px_rgba(106,92,255,0.12)] disabled:cursor-not-allowed disabled:bg-[var(--surface-muted)]"
                    placeholder="Ex.: 15"
                  />
                  <span className="pointer-events-none absolute right-4 top-1/2 -translate-y-1/2 text-sm font-semibold text-[var(--muted)]">
                    %
                  </span>
                </div>
              </label>

              <div className="flex items-end">
                <button
                  type="button"
                  onClick={() => handleRemoveDiscount(discount.id)}
                  disabled={isSavingParent}
                  className="flex h-12 w-full cursor-pointer items-center justify-center rounded-2xl border border-[var(--border)] bg-white px-4 text-sm font-semibold text-[var(--foreground)] transition hover:border-[var(--border-strong)] disabled:cursor-not-allowed disabled:opacity-60 md:w-auto"
                >
                  Remover
                </button>
              </div>

              <p className="md:col-span-3 text-xs uppercase tracking-[0.16em] text-[var(--muted)]">
                Faixa {index + 1}
              </p>
            </div>
          ))}

          <button
            type="button"
            onClick={handleAddDiscount}
            disabled={isSavingParent}
            className="flex h-12 w-full cursor-pointer items-center justify-center rounded-2xl border border-dashed border-[var(--border-strong)] bg-white px-5 text-sm font-semibold text-[var(--foreground)] transition hover:bg-[var(--surface-muted)] disabled:cursor-not-allowed disabled:opacity-60"
          >
            Adicionar faixa de desconto
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
            Aplicar descontos
          </button>
        </div>
      </div>
    </div>,
    document.body,
  );
}
