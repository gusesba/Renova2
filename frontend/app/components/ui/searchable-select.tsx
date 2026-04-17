"use client";

import { useEffect, useId, useMemo, useRef, useState } from "react";

type SearchableSelectOption = {
  label: string;
  onSecondaryAction?: () => void;
  secondaryActionAriaLabel?: string;
  value: string;
};

type SearchableSelectProps = {
  actionLabel?: string;
  ariaLabel: string;
  disabled?: boolean;
  emptyLabel?: string;
  error?: string;
  loading?: boolean;
  onAction?: () => void;
  onChange: (option: SearchableSelectOption) => void;
  onSearchChange: (value: string) => void;
  options: SearchableSelectOption[];
  placeholder?: string;
  searchPlaceholder?: string;
  searchValue: string;
  selectedLabel?: string;
  value: string | null;
};

function ChevronDownIcon({ open }: { open: boolean }) {
  return (
    <svg
      aria-hidden="true"
      viewBox="0 0 24 24"
      className={`h-4 w-4 transition ${open ? "rotate-180" : ""}`}
    >
      <path
        d="M6 9l6 6 6-6"
        fill="none"
        stroke="currentColor"
        strokeLinecap="round"
        strokeLinejoin="round"
        strokeWidth="1.8"
      />
    </svg>
  );
}

function TrashIcon() {
  return (
    <svg
      aria-hidden="true"
      viewBox="0 0 24 24"
      className="h-4 w-4"
      fill="none"
      stroke="currentColor"
      strokeWidth="1.8"
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

export function SearchableSelect({
  actionLabel,
  ariaLabel,
  disabled = false,
  emptyLabel = "Nenhum resultado encontrado",
  error,
  loading = false,
  onAction,
  onChange,
  onSearchChange,
  options,
  placeholder = "Selecionar",
  searchPlaceholder = "Pesquisar",
  searchValue,
  selectedLabel,
  value,
}: SearchableSelectProps) {
  const [open, setOpen] = useState(false);
  const containerRef = useRef<HTMLDivElement | null>(null);
  const searchInputRef = useRef<HTMLInputElement | null>(null);
  const buttonId = useId();
  const listboxId = useId();

  const selectedOption = useMemo(
    () => options.find((option) => option.value === value) ?? null,
    [options, value],
  );

  useEffect(() => {
    if (!open) {
      return;
    }

    searchInputRef.current?.focus();

    function handlePointerDown(event: MouseEvent) {
      if (!containerRef.current?.contains(event.target as Node)) {
        setOpen(false);
      }
    }

    function handleEscape(event: KeyboardEvent) {
      if (event.key === "Escape") {
        setOpen(false);
      }
    }

    window.addEventListener("mousedown", handlePointerDown);
    window.addEventListener("keydown", handleEscape);

    return () => {
      window.removeEventListener("mousedown", handlePointerDown);
      window.removeEventListener("keydown", handleEscape);
    };
  }, [open]);

  const currentLabel = selectedOption?.label ?? selectedLabel ?? "";

  return (
    <div ref={containerRef} className="relative min-w-0">
      <button
        id={buttonId}
        type="button"
        disabled={disabled}
        aria-label={ariaLabel}
        aria-expanded={open}
        aria-haspopup="listbox"
        aria-controls={open ? listboxId : undefined}
        onClick={() => {
          if (!disabled) {
            setOpen((current) => !current);
          }
        }}
        className={`flex h-12 w-full items-center justify-between gap-3 rounded-2xl border bg-white px-4 text-left text-sm outline-none transition disabled:cursor-not-allowed disabled:opacity-60 ${
          error
            ? "border-red-300 shadow-[0_0_0_4px_rgba(248,113,113,0.12)]"
            : "border-[var(--border)] focus:border-[var(--primary)] focus:shadow-[0_0_0_4px_rgba(106,92,255,0.12)]"
        }`}
      >
        <span className={currentLabel ? "truncate text-[var(--foreground)]" : "truncate text-[var(--muted)]"}>
          {currentLabel || placeholder}
        </span>
        <span className="shrink-0 text-[var(--muted)]">
          <ChevronDownIcon open={open} />
        </span>
      </button>

      {open ? (
        <div className="absolute left-0 right-0 top-[calc(100%+0.75rem)] z-40 overflow-hidden rounded-3xl border border-[var(--border)] bg-white shadow-[0_24px_54px_rgba(15,23,42,0.16)]">
          <div className="border-b border-[var(--border)] p-3">
            <input
              ref={searchInputRef}
              type="text"
              value={searchValue}
              placeholder={searchPlaceholder}
              onChange={(event) => onSearchChange(event.target.value)}
              className="h-11 w-full rounded-2xl border border-[var(--border)] bg-[var(--surface-muted)] px-4 text-sm text-[var(--foreground)] outline-none transition focus:border-[var(--primary)] focus:bg-white"
            />
          </div>

          {actionLabel && onAction ? (
            <div className="border-b border-[var(--border)] p-3">
              <button
                type="button"
                onClick={() => {
                  onAction();
                  setOpen(false);
                }}
                className="flex w-full items-center justify-center rounded-2xl bg-[var(--primary-soft)] px-4 py-3 text-sm font-semibold text-[var(--primary)] transition hover:brightness-95"
              >
                {actionLabel}
              </button>
            </div>
          ) : null}

          {loading ? (
            <div className="px-4 py-4 text-sm text-[var(--muted)]">Buscando opcoes...</div>
          ) : options.length === 0 ? (
            <div className="px-4 py-4 text-sm text-[var(--muted)]">{emptyLabel}</div>
          ) : (
            <ul
              id={listboxId}
              role="listbox"
              aria-labelledby={buttonId}
              className="max-h-72 overflow-y-auto py-2"
            >
              {options.map((option) => {
                const isSelected = option.value === value;

                return (
                  <li key={option.value} role="option" aria-selected={isSelected}>
                    <div
                      className={`flex items-center gap-2 px-2 py-1 ${
                        isSelected ? "bg-[var(--primary-soft)]" : ""
                      }`}
                    >
                      <button
                        type="button"
                        onClick={() => {
                          onChange(option);
                          setOpen(false);
                        }}
                        className={`flex min-w-0 flex-1 items-center justify-between gap-3 rounded-2xl px-2 py-2 text-left text-sm transition hover:bg-[var(--surface-muted)] ${
                          isSelected
                            ? "font-semibold text-[var(--foreground)]"
                            : "text-[var(--foreground)]"
                        }`}
                      >
                        <span className="truncate">{option.label}</span>
                        {isSelected ? (
                          <span className="h-2.5 w-2.5 rounded-full bg-[var(--primary)]" />
                        ) : null}
                      </button>
                      {option.onSecondaryAction ? (
                        <button
                          type="button"
                          onClick={() => {
                            option.onSecondaryAction?.();
                            setOpen(false);
                          }}
                          className="flex h-9 w-9 shrink-0 items-center justify-center rounded-2xl border border-[#efdfdb] bg-[#fff7f5] text-[#b14a37] transition hover:bg-[#ffece7]"
                          aria-label={option.secondaryActionAriaLabel ?? `Excluir ${option.label}`}
                          title={option.secondaryActionAriaLabel ?? `Excluir ${option.label}`}
                        >
                          <TrashIcon />
                        </button>
                      ) : null}
                    </div>
                  </li>
                );
              })}
            </ul>
          )}
        </div>
      ) : null}
    </div>
  );
}
