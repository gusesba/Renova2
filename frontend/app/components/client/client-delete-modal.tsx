"use client";

import { useEffect, useState } from "react";

type ClientDeleteModalProps = {
  clientName: string | null;
  isOpen: boolean;
  isSubmitting: boolean;
  onClose: () => void;
  onConfirm: () => Promise<void>;
};

function TrashIcon() {
  return (
    <svg
      aria-hidden="true"
      viewBox="0 0 24 24"
      className="h-5 w-5"
      fill="none"
      stroke="currentColor"
      strokeWidth="2"
      strokeLinecap="round"
      strokeLinejoin="round"
    >
      <path d="M3 6h18" />
      <path d="M8 6V4h8v2" />
      <path d="M19 6l-1 14H6L5 6" />
      <path d="M10 11v6" />
      <path d="M14 11v6" />
    </svg>
  );
}

export function ClientDeleteModal({
  clientName,
  isOpen,
  isSubmitting,
  onClose,
  onConfirm,
}: ClientDeleteModalProps) {
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
      if (event.key === "Escape" && !isSubmitting) {
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
  }, [isOpen, isSubmitting, onClose, shouldRender]);

  if (!shouldRender) {
    return null;
  }

  return (
    <div
      className={`fixed inset-0 z-50 flex items-center justify-center bg-[rgba(15,23,42,0.52)] p-4 transition-opacity duration-200 ease-out ${
        isVisible ? "opacity-100" : "opacity-0"
      }`}
    >
      <div
        className={`w-full max-w-lg rounded-[28px] border border-[var(--border)] bg-white p-6 shadow-[0_30px_90px_rgba(15,23,42,0.22)] transition duration-250 ease-out ${
          isVisible ? "translate-y-0 scale-100 opacity-100" : "translate-y-4 scale-[0.98] opacity-0"
        }`}
      >
        <div className="flex items-start justify-between gap-4">
          <div className="flex items-start gap-4">
            <div className="flex h-12 w-12 shrink-0 items-center justify-center rounded-2xl bg-rose-100 text-rose-600">
              <TrashIcon />
            </div>
            <div>
              <p className="text-sm font-semibold uppercase tracking-[0.16em] text-rose-500">
                Excluir cliente
              </p>
              <h2 className="mt-2 text-2xl font-semibold tracking-tight text-[var(--foreground)]">
                Confirmar exclusao
              </h2>
              <p className="mt-2 text-sm leading-7 text-[var(--muted)]">
                {clientName
                  ? `Voce esta prestes a excluir o cliente ${clientName}. Essa acao nao pode ser desfeita.`
                  : "Voce esta prestes a excluir este cliente. Essa acao nao pode ser desfeita."}
              </p>
            </div>
          </div>

          <button
            type="button"
            onClick={onClose}
            disabled={isSubmitting}
            className="flex h-11 w-11 shrink-0 cursor-pointer items-center justify-center rounded-2xl border border-[var(--border)] bg-[var(--surface-muted)] text-[var(--muted)] transition hover:border-[var(--border-strong)] hover:text-[var(--foreground)] disabled:cursor-not-allowed disabled:opacity-50"
            aria-label="Fechar modal"
          >
            x
          </button>
        </div>

        <div className="mt-8 flex flex-col gap-3 sm:flex-row sm:justify-end">
          <button
            type="button"
            onClick={onClose}
            disabled={isSubmitting}
            className="flex h-12 cursor-pointer items-center justify-center rounded-2xl border border-[var(--border)] bg-[var(--surface-muted)] px-5 text-sm font-semibold text-[var(--foreground)] transition hover:border-[var(--border-strong)] hover:bg-white disabled:cursor-not-allowed disabled:opacity-50"
          >
            Cancelar
          </button>
          <button
            type="button"
            onClick={onConfirm}
            disabled={isSubmitting}
            className="flex h-12 cursor-pointer items-center justify-center rounded-2xl bg-[linear-gradient(90deg,_#ef4444,_#dc2626)] px-5 text-sm font-semibold text-white shadow-[0_16px_30px_rgba(220,38,38,0.24)] transition hover:brightness-105 disabled:cursor-not-allowed disabled:opacity-60"
          >
            {isSubmitting ? "Excluindo cliente..." : "Confirmar exclusao"}
          </button>
        </div>
      </div>
    </div>
  );
}
