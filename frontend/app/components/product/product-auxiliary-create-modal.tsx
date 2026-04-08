"use client";

import { useEffect, useState } from "react";

type ProductAuxiliaryCreateModalProps = {
  error?: string;
  isOpen: boolean;
  isSubmitting: boolean;
  label: string;
  onChange: (value: string) => void;
  onClose: () => void;
  onSubmit: (event: React.FormEvent<HTMLFormElement>) => Promise<void>;
  storeName: string | null;
  value: string;
};

export function ProductAuxiliaryCreateModal({
  error,
  isOpen,
  isSubmitting,
  label,
  onChange,
  onClose,
  onSubmit,
  storeName,
  value,
}: ProductAuxiliaryCreateModalProps) {
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
      className={`fixed inset-0 z-[60] flex items-center justify-center bg-[rgba(15,23,42,0.45)] p-4 transition-opacity duration-200 ease-out ${
        isVisible ? "opacity-100" : "opacity-0"
      }`}
    >
      <div
        className={`w-full max-w-xl rounded-[28px] border border-[var(--border)] bg-white p-6 shadow-[0_30px_90px_rgba(15,23,42,0.22)] transition duration-250 ease-out ${
          isVisible ? "translate-y-0 scale-100 opacity-100" : "translate-y-4 scale-[0.98] opacity-0"
        }`}
      >
        <div className="flex items-start justify-between gap-4">
          <div>
            <p className="text-sm font-semibold uppercase tracking-[0.16em] text-[var(--muted)]">
              Novo {label.toLowerCase()}
            </p>
            <h2 className="mt-2 text-2xl font-semibold tracking-tight text-[var(--foreground)]">
              Criar opcao para o cadastro
            </h2>
            <p className="mt-2 text-sm leading-7 text-[var(--muted)]">
              {storeName
                ? `O novo ${label.toLowerCase()} sera vinculado a ${storeName}.`
                : "Selecione uma loja no topo antes de continuar."}
            </p>
          </div>

          <button
            type="button"
            onClick={onClose}
            className="flex h-11 w-11 cursor-pointer items-center justify-center rounded-2xl border border-[var(--border)] bg-[var(--surface-muted)] text-[var(--muted)] transition hover:border-[var(--border-strong)] hover:text-[var(--foreground)]"
            aria-label="Fechar modal"
          >
            x
          </button>
        </div>

        <form className="mt-6 space-y-5" onSubmit={onSubmit} noValidate>
          <label className="block space-y-2">
            <span className="text-sm font-semibold text-[var(--foreground)]">Valor</span>
            <input
              type="text"
              value={value}
              placeholder={`Ex.: novo ${label.toLowerCase()}`}
              onChange={(event) => onChange(event.target.value)}
              className={`h-12 w-full rounded-2xl border bg-white px-4 text-sm text-[var(--foreground)] outline-none transition ${
                error
                  ? "border-red-300 shadow-[0_0_0_4px_rgba(248,113,113,0.12)]"
                  : "border-[var(--border)] focus:border-[var(--primary)] focus:shadow-[0_0_0_4px_rgba(106,92,255,0.12)]"
              }`}
            />
            {error ? <p className="text-sm text-red-500">{error}</p> : null}
          </label>

          <div className="flex flex-col gap-3 sm:flex-row sm:justify-end">
            <button
              type="button"
              onClick={onClose}
              className="flex h-12 cursor-pointer items-center justify-center rounded-2xl border border-[var(--border)] bg-[var(--surface-muted)] px-5 text-sm font-semibold text-[var(--foreground)] transition hover:border-[var(--border-strong)] hover:bg-white"
            >
              Cancelar
            </button>
            <button
              type="submit"
              disabled={isSubmitting || !storeName}
              className="flex h-12 cursor-pointer items-center justify-center rounded-2xl bg-[linear-gradient(90deg,_#ff8a3d,_#ff6b3d)] px-5 text-sm font-semibold text-white shadow-[0_16px_30px_rgba(255,107,61,0.28)] transition hover:brightness-105 disabled:cursor-not-allowed disabled:opacity-60"
            >
              {isSubmitting ? "Salvando..." : `Salvar ${label.toLowerCase()}`}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
