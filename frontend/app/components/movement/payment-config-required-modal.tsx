"use client";

import { useEffect, useState } from "react";
import { createPortal } from "react-dom";

type PaymentConfigRequiredModalProps = {
  isOpen: boolean;
  storeName: string | null;
  onClose: () => void;
  onOpenSettings: () => void;
};

export function PaymentConfigRequiredModal({
  isOpen,
  storeName,
  onClose,
  onOpenSettings,
}: PaymentConfigRequiredModalProps) {
  const [shouldRender, setShouldRender] = useState(isOpen);
  const [isVisible, setIsVisible] = useState(false);

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
      if (event.key === "Escape") {
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
  }, [isOpen, onClose, shouldRender]);

  if (typeof document === "undefined" || !shouldRender) {
    return null;
  }

  return createPortal(
    <div
      className={`fixed inset-0 z-[210] flex items-center justify-center bg-[rgba(15,23,42,0.45)] p-4 transition-opacity duration-200 ease-out ${
        isVisible ? "opacity-100" : "opacity-0"
      }`}
    >
      <div
        className={`w-full max-w-lg rounded-[28px] border border-[var(--border)] bg-white p-6 shadow-[0_30px_90px_rgba(15,23,42,0.22)] transition duration-250 ease-out ${
          isVisible ? "translate-y-0 scale-100 opacity-100" : "translate-y-4 scale-[0.98] opacity-0"
        }`}
      >
        <div className="flex items-start justify-between gap-4">
          <div>
            <p className="text-sm font-semibold uppercase tracking-[0.16em] text-[var(--muted)]">
              Pagamento bloqueado
            </p>
            <h2 className="mt-2 text-2xl font-semibold tracking-tight text-[var(--foreground)]">
              Configure o repasse da loja
            </h2>
            <p className="mt-2 text-sm leading-7 text-[var(--muted)]">
              Nao foi possivel criar a ordem de pagamento porque a loja
              {storeName ? ` ${storeName}` : ""} ainda nao possui percentual de repasse ao
              fornecedor configurado.
            </p>
          </div>

          <button
            type="button"
            onClick={onClose}
            className="flex h-11 w-11 cursor-pointer items-center justify-center rounded-2xl border border-[var(--border)] bg-[var(--surface-muted)] text-[var(--muted)] transition hover:border-[var(--border-strong)] hover:text-[var(--foreground)]"
            aria-label="Fechar aviso"
          >
            x
          </button>
        </div>

        <div className="mt-6 rounded-2xl border border-[var(--border)] bg-[var(--surface-muted)] px-4 py-4 text-sm text-[var(--muted)]">
          Defina o percentual de repasse para que vendas e devolucoes consigam gerar as ordens
          de pagamento automaticamente.
        </div>

        <div className="mt-6 flex flex-col gap-3 sm:flex-row sm:justify-end">
          <button
            type="button"
            onClick={onClose}
            className="flex h-12 cursor-pointer items-center justify-center rounded-2xl border border-[var(--border)] bg-[var(--surface-muted)] px-5 text-sm font-semibold text-[var(--foreground)] transition hover:border-[var(--border-strong)] hover:bg-white"
          >
            Fechar
          </button>
          <button
            type="button"
            onClick={onOpenSettings}
            className="flex h-12 cursor-pointer items-center justify-center rounded-2xl bg-[linear-gradient(90deg,_#ff8a3d,_#ff6b3d)] px-5 text-sm font-semibold text-white shadow-[0_16px_30px_rgba(255,107,61,0.28)] transition hover:brightness-105"
          >
            Abrir configuracao de repasse
          </button>
        </div>
      </div>
    </div>,
    document.body,
  );
}
