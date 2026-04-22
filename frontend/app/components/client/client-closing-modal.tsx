"use client";

import { useEffect, useState } from "react";

type ClientClosingModalProps = {
  dataInicial: string;
  dataFinal: string;
  closingType: "produtos" | "movimentacoes";
  isOpen: boolean;
  isSubmitting: boolean;
  onChange: (
    field: "dataInicial" | "dataFinal" | "closingType",
    value: string,
  ) => void;
  onClose: () => void;
  onSubmit: (event: React.FormEvent<HTMLFormElement>) => void;
};

export function ClientClosingModal({
  dataInicial,
  dataFinal,
  closingType,
  isOpen,
  isSubmitting,
  onChange,
  onClose,
  onSubmit,
}: ClientClosingModalProps) {
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

  if (!shouldRender && !isOpen) {
    return null;
  }

  return (
    <div
      className={`fixed inset-0 z-50 flex items-center justify-center bg-[rgba(15,23,42,0.45)] p-4 transition-opacity duration-200 ease-out ${
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
              Fechamento
            </p>
            <h2 className="mt-2 text-2xl font-semibold tracking-tight text-[var(--foreground)]">
              Exportar fechamento dos clientes
            </h2>
            <p className="mt-2 text-sm leading-7 text-[var(--muted)]">
              Escolha o periodo e se deseja exportar produtos cadastrados ou movimentacoes dos
              clientes elegiveis.
            </p>
          </div>

          <button
            type="button"
            onClick={onClose}
            disabled={isSubmitting}
            className="flex h-11 w-11 cursor-pointer items-center justify-center rounded-2xl border border-[var(--border)] bg-[var(--surface-muted)] text-[var(--muted)] transition hover:border-[var(--border-strong)] hover:text-[var(--foreground)] disabled:cursor-not-allowed disabled:opacity-60"
            aria-label="Fechar fechamento"
          >
            x
          </button>
        </div>

        <form className="mt-6 space-y-6" onSubmit={onSubmit}>
          <div className="space-y-3">
            <span className="text-sm font-semibold text-[var(--foreground)]">Tipo de fechamento</span>
            <div className="grid gap-3 md:grid-cols-2">
              <label className="flex cursor-pointer items-start gap-3 rounded-2xl border border-[var(--border)] bg-white p-4 transition hover:border-[var(--border-strong)]">
                <input
                  type="radio"
                  name="closing-type"
                  value="produtos"
                  checked={closingType === "produtos"}
                  onChange={(event) => onChange("closingType", event.target.value)}
                  className="mt-1"
                />
                <span className="space-y-1">
                  <span className="block text-sm font-semibold text-[var(--foreground)]">Produtos</span>
                  <span className="block text-sm leading-6 text-[var(--muted)]">
                    Exporta os itens cadastrados no periodo com o cliente como fornecedor.
                  </span>
                </span>
              </label>

              <label className="flex cursor-pointer items-start gap-3 rounded-2xl border border-[var(--border)] bg-white p-4 transition hover:border-[var(--border-strong)]">
                <input
                  type="radio"
                  name="closing-type"
                  value="movimentacoes"
                  checked={closingType === "movimentacoes"}
                  onChange={(event) => onChange("closingType", event.target.value)}
                  className="mt-1"
                />
                <span className="space-y-1">
                  <span className="block text-sm font-semibold text-[var(--foreground)]">Movimentacoes</span>
                  <span className="block text-sm leading-6 text-[var(--muted)]">
                    Exporta vendas dos itens do cliente, compras realizadas e conta credito.
                  </span>
                </span>
              </label>
            </div>
          </div>

          <div className="grid gap-4 md:grid-cols-2">
            <label className="space-y-2">
              <span className="text-sm font-semibold text-[var(--foreground)]">Data inicial</span>
              <input
                type="date"
                value={dataInicial}
                onChange={(event) => onChange("dataInicial", event.target.value)}
                className="h-12 w-full rounded-2xl border border-[var(--border)] bg-white px-4 text-sm text-[var(--foreground)] outline-none transition focus:border-[var(--primary)] focus:shadow-[0_0_0_4px_rgba(106,92,255,0.12)]"
              />
            </label>

            <label className="space-y-2">
              <span className="text-sm font-semibold text-[var(--foreground)]">Data final</span>
              <input
                type="date"
                value={dataFinal}
                onChange={(event) => onChange("dataFinal", event.target.value)}
                className="h-12 w-full rounded-2xl border border-[var(--border)] bg-white px-4 text-sm text-[var(--foreground)] outline-none transition focus:border-[var(--primary)] focus:shadow-[0_0_0_4px_rgba(106,92,255,0.12)]"
              />
            </label>
          </div>

          <div className="flex flex-col gap-3 sm:flex-row sm:justify-end">
            <button
              type="button"
              onClick={onClose}
              disabled={isSubmitting}
              className="flex h-12 cursor-pointer items-center justify-center rounded-2xl border border-[var(--border)] bg-[var(--surface-muted)] px-5 text-sm font-semibold text-[var(--foreground)] transition hover:border-[var(--border-strong)] hover:bg-white disabled:cursor-not-allowed disabled:opacity-60"
            >
              Cancelar
            </button>
            <button
              type="submit"
              disabled={isSubmitting}
              className="flex h-12 cursor-pointer items-center justify-center rounded-2xl bg-[linear-gradient(90deg,_#ff8a3d,_#ff6b3d)] px-5 text-sm font-semibold text-white shadow-[0_16px_30px_rgba(255,107,61,0.28)] transition hover:brightness-105 disabled:cursor-not-allowed disabled:opacity-60"
            >
              {isSubmitting ? "Gerando..." : "Exportar Excel"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
