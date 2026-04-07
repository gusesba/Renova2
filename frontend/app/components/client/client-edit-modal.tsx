"use client";

import { useEffect, useState } from "react";

import type { ClientFieldErrors, ClientFormValues } from "@/lib/client";

type ClientEditModalProps = {
  clientId: number | null;
  errors: ClientFieldErrors;
  isOpen: boolean;
  isSubmitting: boolean;
  values: ClientFormValues;
  onChange: <K extends keyof ClientFormValues>(field: K, value: ClientFormValues[K]) => void;
  onClose: () => void;
  onSubmit: (event: React.FormEvent<HTMLFormElement>) => Promise<void>;
};

function FormField({
  label,
  placeholder,
  value,
  error,
  readOnly = false,
  onChange,
}: {
  label: string;
  placeholder: string;
  value: string;
  error?: string;
  readOnly?: boolean;
  onChange?: (value: string) => void;
}) {
  return (
    <label className="block space-y-2">
      <span className="text-sm font-semibold text-[var(--foreground)]">{label}</span>
      <input
        type="text"
        value={value}
        placeholder={placeholder}
        readOnly={readOnly}
        onChange={(event) => onChange?.(event.target.value)}
        className={`h-12 w-full rounded-2xl border bg-white px-4 text-sm text-[var(--foreground)] outline-none transition ${
          readOnly
            ? "cursor-not-allowed border-[var(--border)] bg-[var(--surface-muted)] text-[var(--muted)]"
            : error
              ? "border-red-300 shadow-[0_0_0_4px_rgba(248,113,113,0.12)]"
              : "border-[var(--border)] focus:border-[var(--primary)] focus:shadow-[0_0_0_4px_rgba(106,92,255,0.12)]"
        }`}
      />
      {error ? <p className="text-sm text-red-500">{error}</p> : null}
    </label>
  );
}

export function ClientEditModal({
  clientId,
  errors,
  isOpen,
  isSubmitting,
  values,
  onChange,
  onClose,
  onSubmit,
}: ClientEditModalProps) {
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

  if (!shouldRender) {
    return null;
  }

  return (
    <div
      className={`fixed inset-0 z-50 flex items-center justify-center bg-[rgba(15,23,42,0.45)] p-4 transition-opacity duration-200 ease-out ${
        isVisible ? "opacity-100" : "opacity-0"
      }`}
    >
      <div
        className={`w-full max-w-2xl rounded-[28px] border border-[var(--border)] bg-white p-6 shadow-[0_30px_90px_rgba(15,23,42,0.22)] transition duration-250 ease-out ${
          isVisible ? "translate-y-0 scale-100 opacity-100" : "translate-y-4 scale-[0.98] opacity-0"
        }`}
      >
        <div className="flex items-start justify-between gap-4">
          <div>
            <p className="text-sm font-semibold uppercase tracking-[0.16em] text-[var(--muted)]">
              Editar cliente
            </p>
            <h2 className="mt-2 text-2xl font-semibold tracking-tight text-[var(--foreground)]">
              Atualize os dados do cadastro
            </h2>
            <p className="mt-2 text-sm leading-7 text-[var(--muted)]">
              O identificador e fixo. Nome, contato e UserId podem ser ajustados nesta tela.
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
          <FormField
            label="ID"
            placeholder="Identificador do cliente"
            value={clientId ? String(clientId) : ""}
            readOnly
          />

          <div className="grid gap-4 md:grid-cols-2">
            <FormField
              label="Nome"
              placeholder="Ex.: Ana Paula"
              value={values.nome}
              error={errors.nome}
              onChange={(value) => onChange("nome", value)}
            />
            <FormField
              label="Contato"
              placeholder="Telefone, email ou identificador"
              value={values.contato}
              error={errors.contato}
              onChange={(value) => onChange("contato", value)}
            />
          </div>

          <FormField
            label="UserId (opcional)"
            placeholder="Ex.: 42"
            value={values.userId}
            error={errors.userId}
            onChange={(value) => onChange("userId", value)}
          />

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
              disabled={isSubmitting || clientId === null}
              className="flex h-12 cursor-pointer items-center justify-center rounded-2xl bg-[linear-gradient(90deg,_#22c55e,_#16a34a)] px-5 text-sm font-semibold text-white shadow-[0_16px_30px_rgba(34,197,94,0.25)] transition hover:brightness-105 disabled:cursor-not-allowed disabled:opacity-60"
            >
              {isSubmitting ? "Salvando alteracoes..." : "Salvar alteracoes"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
