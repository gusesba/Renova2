"use client";

import { useEffect, useState } from "react";

import { formatCurrencyValue, type ProductRequestMatchItem } from "@/lib/product";

type ProductRequestMatchModalProps = {
  isOpen: boolean;
  matches: ProductRequestMatchItem[];
  productDescription: string | null;
  onClose: () => void;
};

export function ProductRequestMatchModal({
  isOpen,
  matches,
  productDescription,
  onClose,
}: ProductRequestMatchModalProps) {
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
        className={`w-full max-w-4xl rounded-[28px] border border-[var(--border)] bg-white p-6 shadow-[0_30px_90px_rgba(15,23,42,0.22)] transition duration-250 ease-out ${
          isVisible ? "translate-y-0 scale-100 opacity-100" : "translate-y-4 scale-[0.98] opacity-0"
        }`}
      >
        <div className="flex items-start justify-between gap-4">
          <div>
            <p className="text-sm font-semibold uppercase tracking-[0.16em] text-[var(--muted)]">
              Matches encontrados
            </p>
            <h2 className="mt-2 text-2xl font-semibold tracking-tight text-[var(--foreground)]">
              Solicitacoes compativeis com o produto cadastrado
            </h2>
            <p className="mt-2 text-sm leading-7 text-[var(--muted)]">
              {productDescription
                ? `O produto ${productDescription} corresponde a ${matches.length} solicitacao(oes).`
                : `${matches.length} solicitacao(oes) compativeis encontradas.`}
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

        <div className="mt-6 overflow-hidden rounded-[24px] border border-[var(--border)]">
          <div className="overflow-x-auto">
            <table className="min-w-full border-collapse bg-white">
              <thead className="bg-[var(--surface-muted)]">
                <tr className="text-left">
                  <th className="px-4 py-4 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                    Cliente
                  </th>
                  <th className="px-4 py-4 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                    Solicitacao
                  </th>
                  <th className="px-4 py-4 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                    Caracteristicas
                  </th>
                  <th className="px-4 py-4 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                    Faixa
                  </th>
                </tr>
              </thead>
              <tbody>
                {matches.map((match, index) => (
                  <tr
                    key={match.id}
                    className={
                      index % 2 === 0
                        ? "bg-white"
                        : "bg-[color:color-mix(in_srgb,var(--surface-muted)_55%,white)]"
                    }
                  >
                    <td className="px-4 py-4 text-sm font-semibold text-[var(--foreground)]">
                      {match.cliente}
                    </td>
                    <td className="px-4 py-4 text-sm text-[var(--foreground)]">{match.descricao}</td>
                    <td className="px-4 py-4 text-sm text-[var(--muted)]">
                      {[
                        match.produto || "Qualquer produto",
                        match.marca || "Qualquer marca",
                        match.tamanho || "Qualquer tamanho",
                        match.cor || "Qualquer cor",
                      ].join(" / ")}
                    </td>
                    <td className="px-4 py-4 text-sm font-semibold text-[var(--foreground)]">
                      {match.precoMaximo == null
                        ? "Qualquer preco"
                        : `Ate ${formatCurrencyValue(match.precoMaximo)}`}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>

        <div className="mt-6 flex justify-end">
          <button
            type="button"
            onClick={onClose}
            className="flex h-12 cursor-pointer items-center justify-center rounded-2xl bg-[linear-gradient(90deg,_#ff8a3d,_#ff6b3d)] px-5 text-sm font-semibold text-white shadow-[0_16px_30px_rgba(255,107,61,0.28)] transition hover:brightness-105"
          >
            Fechar
          </button>
        </div>
      </div>
    </div>
  );
}
