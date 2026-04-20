"use client";

import { useEffect, useRef, useState } from "react";

import type { ClientAreaTableSettings, ClientAreaVisibleField } from "@/lib/client-area";

type ClientAreaSettingsModalProps = {
  isOpen: boolean;
  settings: ClientAreaTableSettings;
  onClose: () => void;
  onSave: (settings: ClientAreaTableSettings) => void;
};

const fieldOptions: Array<{
  description: string;
  label: string;
  value: ClientAreaVisibleField;
}> = [
  { label: "Loja", value: "loja", description: "Nome da loja vinculada a peca" },
  { label: "Produto", value: "produto", description: "Referencia principal do item" },
  { label: "Descricao", value: "descricao", description: "Descricao livre do produto" },
  { label: "Marca", value: "marca", description: "Marca vinculada" },
  { label: "Tamanho", value: "tamanho", description: "Tamanho vinculado" },
  { label: "Cor", value: "cor", description: "Cor vinculada" },
  { label: "Preco", value: "preco", description: "Preco de cadastro" },
  { label: "Entrada", value: "entrada", description: "Data de entrada" },
  { label: "Situacao", value: "situacao", description: "Estado atual do item" },
  { label: "Identificador", value: "id", description: "Codigo interno do produto" },
];

export function ClientAreaSettingsModal({
  isOpen,
  settings,
  onClose,
  onSave,
}: ClientAreaSettingsModalProps) {
  const [shouldRender, setShouldRender] = useState(isOpen);
  const [isVisible, setIsVisible] = useState(false);
  const [draft, setDraft] = useState<ClientAreaTableSettings>(settings);
  const wasOpenRef = useRef(isOpen);

  useEffect(() => {
    let draftFrame = 0;
    let animationFrame = 0;
    let visibilityFrame = 0;
    let closeTimeout = 0;

    if (isOpen) {
      if (!wasOpenRef.current) {
        draftFrame = window.requestAnimationFrame(() => {
          setDraft(settings);
        });
      }

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
      window.cancelAnimationFrame(draftFrame);
      window.cancelAnimationFrame(animationFrame);
      window.cancelAnimationFrame(visibilityFrame);
      window.clearTimeout(closeTimeout);
      window.removeEventListener("keydown", handleEscape);
    };
  }, [isOpen, onClose, settings, shouldRender]);

  useEffect(() => {
    wasOpenRef.current = isOpen;
  }, [isOpen]);

  if (!shouldRender) {
    return null;
  }

  const hasMinimumVisibleFields = draft.visibleFields.length > 0;

  return (
    <div
      className={`fixed inset-0 z-50 flex items-center justify-center bg-[rgba(15,23,42,0.45)] p-4 transition-opacity duration-200 ease-out ${
        isVisible ? "opacity-100" : "opacity-0"
      }`}
    >
      <div
        className={`w-full max-w-3xl rounded-[28px] border border-[var(--border)] bg-white p-6 shadow-[0_30px_90px_rgba(15,23,42,0.22)] transition duration-250 ease-out ${
          isVisible ? "translate-y-0 scale-100 opacity-100" : "translate-y-4 scale-[0.98] opacity-0"
        }`}
      >
        <div className="flex items-start justify-between gap-4">
          <div>
            <p className="text-sm font-semibold uppercase tracking-[0.16em] text-[var(--muted)]">
              Configuracoes da tabela
            </p>
            <h2 className="mt-2 text-2xl font-semibold tracking-tight text-[var(--foreground)]">
              Ajuste a visualizacao
            </h2>
            <p className="mt-2 text-sm leading-7 text-[var(--muted)]">
              Defina quantos registros aparecem por pagina e quais colunas ficam visiveis.
            </p>
          </div>

          <button
            type="button"
            onClick={onClose}
            className="flex h-11 w-11 cursor-pointer items-center justify-center rounded-2xl border border-[var(--border)] bg-[var(--surface-muted)] text-[var(--muted)] transition hover:border-[var(--border-strong)] hover:text-[var(--foreground)]"
            aria-label="Fechar configuracoes"
          >
            x
          </button>
        </div>

        <div className="mt-6 space-y-6">
          <label className="block space-y-2">
            <span className="text-sm font-semibold text-[var(--foreground)]">Itens por pagina</span>
            <input
              type="number"
              min={1}
              max={100}
              value={draft.tamanhoPagina}
              onChange={(event) => {
                const parsed = Number(event.target.value);

                setDraft((current) => ({
                  ...current,
                  tamanhoPagina: Number.isInteger(parsed) && parsed > 0 ? parsed : 1,
                }));
              }}
              className="h-12 w-full rounded-2xl border border-[var(--border)] bg-white px-4 text-sm text-[var(--foreground)] outline-none transition focus:border-[var(--primary)] focus:shadow-[0_0_0_4px_rgba(106,92,255,0.12)]"
            />
          </label>

          <div className="space-y-3">
            <p className="text-sm font-semibold text-[var(--foreground)]">Colunas visiveis</p>
            <div className="grid gap-3 md:grid-cols-2">
              {fieldOptions.map((field) => {
                const checked = draft.visibleFields.includes(field.value);

                return (
                  <label
                    key={field.value}
                    className="flex cursor-pointer items-start gap-3 rounded-2xl border border-[var(--border)] bg-[var(--surface-muted)] px-4 py-4 transition hover:border-[var(--border-strong)]"
                  >
                    <input
                      type="checkbox"
                      checked={checked}
                      onChange={(event) => {
                        setDraft((current) => ({
                          ...current,
                          visibleFields: event.target.checked
                            ? [...current.visibleFields, field.value]
                            : current.visibleFields.filter((value) => value !== field.value),
                        }));
                      }}
                      className="mt-1 h-4 w-4 rounded border-[var(--border-strong)]"
                    />
                    <div>
                      <p className="text-sm font-semibold text-[var(--foreground)]">{field.label}</p>
                      <p className="text-sm text-[var(--muted)]">{field.description}</p>
                    </div>
                  </label>
                );
              })}
            </div>
            {!hasMinimumVisibleFields ? (
              <p className="text-sm text-red-500">Selecione pelo menos uma coluna para a tabela.</p>
            ) : null}
          </div>

          <div className="flex flex-col gap-3 sm:flex-row sm:justify-end">
            <button
              type="button"
              onClick={onClose}
              className="flex h-12 cursor-pointer items-center justify-center rounded-2xl border border-[var(--border)] bg-[var(--surface-muted)] px-5 text-sm font-semibold text-[var(--foreground)] transition hover:border-[var(--border-strong)] hover:bg-white"
            >
              Cancelar
            </button>
            <button
              type="button"
              disabled={!hasMinimumVisibleFields}
              onClick={() => {
                onSave({
                  tamanhoPagina: Math.min(Math.max(draft.tamanhoPagina, 1), 100),
                  visibleFields: draft.visibleFields,
                });
              }}
              className="flex h-12 cursor-pointer items-center justify-center rounded-2xl bg-[linear-gradient(90deg,_#ff8a3d,_#ff6b3d)] px-5 text-sm font-semibold text-white shadow-[0_16px_30px_rgba(255,107,61,0.28)] transition hover:brightness-105 disabled:cursor-not-allowed disabled:opacity-60"
            >
              Salvar configuracoes
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
