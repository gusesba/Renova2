"use client";

import { useEffect, useState } from "react";
import { createPortal } from "react-dom";

import type { StoreFieldErrors, StoreFormValues } from "@/lib/store";

type StoreEditModalProps = {
  errors: StoreFieldErrors;
  isDeleting: boolean;
  isOpen: boolean;
  isSubmitting: boolean;
  storeId: number | null;
  values: StoreFormValues;
  onChange: (value: string) => void;
  onClose: () => void;
  onDelete: () => Promise<void>;
  onSubmit: (event: React.FormEvent<HTMLFormElement>) => Promise<void>;
};

export function StoreEditModal({
  errors,
  isDeleting,
  isOpen,
  isSubmitting,
  storeId,
  values,
  onChange,
  onClose,
  onDelete,
  onSubmit,
}: StoreEditModalProps) {
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
      if (event.key === "Escape" && !isSubmitting && !isDeleting) {
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
  }, [isDeleting, isOpen, isSubmitting, onClose, shouldRender]);

  if (!shouldRender) {
    return null;
  }

  return createPortal(
    <div
      className={`fixed inset-0 z-[200] flex items-center justify-center bg-[rgba(15,23,42,0.45)] p-4 transition-opacity duration-200 ease-out ${
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
              Loja atual
            </p>
            <h2 className="mt-2 text-2xl font-semibold tracking-tight text-[var(--foreground)]">
              Editar ou excluir loja
            </h2>
            <p className="mt-2 text-sm leading-7 text-[var(--muted)]">
              Atualize o nome da loja selecionada ou remova esse cadastro.
            </p>
          </div>

          <button
            type="button"
            onClick={onClose}
            disabled={isSubmitting || isDeleting}
            className="flex h-11 w-11 cursor-pointer items-center justify-center rounded-2xl border border-[var(--border)] bg-[var(--surface-muted)] text-[var(--muted)] transition hover:border-[var(--border-strong)] hover:text-[var(--foreground)] disabled:cursor-not-allowed disabled:opacity-60"
            aria-label="Fechar modal"
          >
            x
          </button>
        </div>

        <form className="mt-6 space-y-5" onSubmit={onSubmit} noValidate>
          <label className="block space-y-2">
            <span className="text-sm font-semibold text-[var(--foreground)]">ID da loja</span>
            <input
              type="text"
              value={storeId ? `#${storeId}` : ""}
              readOnly
              className="h-12 w-full cursor-not-allowed rounded-2xl border border-[var(--border)] bg-[var(--surface-muted)] px-4 text-sm text-[var(--muted)] outline-none"
            />
          </label>

          <label className="block space-y-2">
            <span className="text-sm font-semibold text-[var(--foreground)]">Nome da loja</span>
            <input
              type="text"
              value={values.nome}
              onChange={(event) => onChange(event.target.value)}
              placeholder="Ex.: Atelier Centro"
              className={`h-12 w-full rounded-2xl border bg-white px-4 text-sm text-[var(--foreground)] outline-none transition ${
                errors.nome
                  ? "border-red-300 shadow-[0_0_0_4px_rgba(248,113,113,0.12)]"
                  : "border-[var(--border)] focus:border-[var(--primary)] focus:shadow-[0_0_0_4px_rgba(106,92,255,0.12)]"
              }`}
            />
            {errors.nome ? <p className="text-sm text-red-500">{errors.nome}</p> : null}
          </label>

          <div className="rounded-2xl border border-red-100 bg-[linear-gradient(180deg,_#fff7f7_0%,_#fff_100%)] p-4">
            <p className="text-sm font-semibold text-[#b42318]">Excluir loja</p>
            <p className="mt-1 text-sm leading-6 text-[#7a271a]">
              Essa acao remove a loja apenas quando ela nao possui registros ativos vinculados.
            </p>
            <button
              type="button"
              onClick={() => {
                void onDelete();
              }}
              disabled={isSubmitting || isDeleting || storeId === null}
              className="mt-4 flex h-11 cursor-pointer items-center justify-center rounded-2xl border border-red-200 bg-white px-5 text-sm font-semibold text-[#b42318] transition hover:bg-red-50 disabled:cursor-not-allowed disabled:opacity-60"
            >
              {isDeleting ? "Excluindo loja..." : "Excluir loja"}
            </button>
          </div>

          <div className="flex flex-col gap-3 sm:flex-row sm:justify-end">
            <button
              type="button"
              onClick={onClose}
              disabled={isSubmitting || isDeleting}
              className="flex h-12 cursor-pointer items-center justify-center rounded-2xl border border-[var(--border)] bg-[var(--surface-muted)] px-5 text-sm font-semibold text-[var(--foreground)] transition hover:border-[var(--border-strong)] hover:bg-white disabled:cursor-not-allowed disabled:opacity-60"
            >
              Cancelar
            </button>
            <button
              type="submit"
              disabled={isSubmitting || isDeleting || storeId === null}
              className="flex h-12 cursor-pointer items-center justify-center rounded-2xl bg-[linear-gradient(90deg,_#22c55e,_#16a34a)] px-5 text-sm font-semibold text-white shadow-[0_16px_30px_rgba(34,197,94,0.25)] transition hover:brightness-105 disabled:cursor-not-allowed disabled:opacity-60"
            >
              {isSubmitting ? "Salvando alteracoes..." : "Salvar alteracoes"}
            </button>
          </div>
        </form>
      </div>
    </div>,
    document.body,
  );
}
