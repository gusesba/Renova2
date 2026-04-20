"use client";

import { useEffect, useState } from "react";

import { SearchableSelect } from "@/app/components/ui/searchable-select";
import type { ClientFieldErrors, ClientFormValues } from "@/lib/client";

type ClientCreateModalProps = {
  errors: ClientFieldErrors;
  isOpen: boolean;
  isSubmitting: boolean;
  isUserLoading?: boolean;
  storeName: string | null;
  userEmptyLabel?: string;
  userOptions?: Array<{ id: number; nome: string; email: string }>;
  userSearchValue?: string;
  userSelectedLabel?: string;
  values: ClientFormValues;
  onChange: <K extends keyof ClientFormValues>(field: K, value: ClientFormValues[K]) => void;
  onClose: () => void;
  onUserSearchChange?: (value: string) => void;
  onSubmit: (event: React.FormEvent<HTMLFormElement>) => Promise<void>;
};

function FormField({
  label,
  placeholder,
  value,
  error,
  inputMode,
  onChange,
}: {
  label: string;
  placeholder: string;
  value: string;
  error?: string;
  inputMode?: React.HTMLAttributes<HTMLInputElement>["inputMode"];
  onChange: (value: string) => void;
}) {
  return (
    <label className="block space-y-2">
      <span className="text-sm font-semibold text-[var(--foreground)]">{label}</span>
      <input
        type="text"
        value={value}
        placeholder={placeholder}
        inputMode={inputMode}
        onChange={(event) => onChange(event.target.value)}
        className={`h-12 w-full rounded-2xl border bg-white px-4 text-sm text-[var(--foreground)] outline-none transition ${
          error
            ? "border-red-300 shadow-[0_0_0_4px_rgba(248,113,113,0.12)]"
            : "border-[var(--border)] focus:border-[var(--primary)] focus:shadow-[0_0_0_4px_rgba(106,92,255,0.12)]"
        }`}
      />
      {error ? <p className="text-sm text-red-500">{error}</p> : null}
    </label>
  );
}

function TextareaField({
  label,
  placeholder,
  value,
  error,
  onChange,
}: {
  label: string;
  placeholder: string;
  value: string;
  error?: string;
  onChange: (value: string) => void;
}) {
  return (
    <label className="block space-y-2">
      <span className="text-sm font-semibold text-[var(--foreground)]">{label}</span>
      <textarea
        value={value}
        placeholder={placeholder}
        rows={4}
        onChange={(event) => onChange(event.target.value)}
        className={`w-full rounded-2xl border bg-white px-4 py-3 text-sm text-[var(--foreground)] outline-none transition ${
          error
            ? "border-red-300 shadow-[0_0_0_4px_rgba(248,113,113,0.12)]"
            : "border-[var(--border)] focus:border-[var(--primary)] focus:shadow-[0_0_0_4px_rgba(106,92,255,0.12)]"
        }`}
      />
      {error ? <p className="text-sm text-red-500">{error}</p> : null}
    </label>
  );
}

function ToggleField({
  checked,
  label,
  description,
  onChange,
}: {
  checked: boolean;
  label: string;
  description: string;
  onChange: (checked: boolean) => void;
}) {
  return (
    <label className="flex cursor-pointer items-start gap-3 rounded-2xl border border-[var(--border)] bg-[var(--surface-muted)] px-4 py-4 transition hover:border-[var(--border-strong)]">
      <input
        type="checkbox"
        checked={checked}
        onChange={(event) => onChange(event.target.checked)}
        className="mt-1 h-4 w-4 rounded border-[var(--border-strong)]"
      />
      <div>
        <p className="text-sm font-semibold text-[var(--foreground)]">{label}</p>
        <p className="text-sm text-[var(--muted)]">{description}</p>
      </div>
    </label>
  );
}

export function ClientCreateModal({
  errors,
  isOpen,
  isSubmitting,
  isUserLoading = false,
  storeName,
  userEmptyLabel = "Nenhum usuario encontrado.",
  userOptions = [],
  userSearchValue = "",
  userSelectedLabel,
  values,
  onChange,
  onClose,
  onUserSearchChange,
  onSubmit,
}: ClientCreateModalProps) {
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
              Novo cliente
            </p>
            <h2 className="mt-2 text-2xl font-semibold tracking-tight text-[var(--foreground)]">
              Cadastro rapido na loja ativa
            </h2>
            <p className="mt-2 text-sm leading-7 text-[var(--muted)]">
              {storeName
                ? `Os novos clientes serao vinculados a ${storeName}.`
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
              placeholder="(41) 99717-3484"
              value={values.contato}
              error={errors.contato}
              inputMode="numeric"
              onChange={(value) => onChange("contato", value)}
            />
          </div>

          <TextareaField
            label="Obs (opcional)"
            placeholder="Anotacoes sobre cliente"
            value={values.obs}
            error={errors.obs}
            onChange={(value) => onChange("obs", value)}
          />

          {onUserSearchChange ? (
            <div className="space-y-2">
              <span className="text-sm font-semibold text-[var(--foreground)]">Usuario vinculado (opcional)</span>
              <>
                <SearchableSelect
                  ariaLabel="Usuario vinculado"
                  emptyLabel={userEmptyLabel}
                  error={errors.userId}
                  loading={isUserLoading}
                  options={userOptions.map((user) => ({
                    label: `${user.nome} - ${user.email}`,
                    value: String(user.id),
                  }))}
                  placeholder="Selecione um usuario"
                  searchPlaceholder="Pesquisar por nome ou email"
                  searchValue={userSearchValue}
                  selectedLabel={userSelectedLabel}
                  value={values.userId || null}
                  onSearchChange={onUserSearchChange}
                  onChange={(option) => onChange("userId", option.value)}
                />
                {errors.userId ? <p className="text-sm text-red-500">{errors.userId}</p> : null}
              </>
            </div>
          ) : (
            <FormField
              label="UserId (opcional)"
              placeholder="Ex.: 42"
              value={values.userId}
              error={errors.userId}
              onChange={(value) => onChange("userId", value)}
            />
          )}

          <ToggleField
            checked={values.doacao}
            label="Doacao"
            description="Marque quando esse cliente for um cadastro de doacao."
            onChange={(value) => onChange("doacao", value)}
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
              disabled={isSubmitting || !storeName}
              className="flex h-12 cursor-pointer items-center justify-center rounded-2xl bg-[linear-gradient(90deg,_#ff8a3d,_#ff6b3d)] px-5 text-sm font-semibold text-white shadow-[0_16px_30px_rgba(255,107,61,0.28)] transition hover:brightness-105 disabled:cursor-not-allowed disabled:opacity-60"
            >
              {isSubmitting ? "Salvando cliente..." : "Salvar cliente"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
