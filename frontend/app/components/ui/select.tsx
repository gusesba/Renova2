"use client";

import { useEffect, useId, useMemo, useRef, useState, type ReactNode } from "react";

type SelectOption = {
  label: string;
  value: string;
};

type SelectProps = {
  ariaLabel: string;
  disabled?: boolean;
  emptyLabel?: string;
  helper?: ReactNode;
  onChange: (value: string) => void;
  options: SelectOption[];
  placeholder?: string;
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

export function Select({
  ariaLabel,
  disabled = false,
  emptyLabel = "Nenhuma opcao disponivel",
  helper,
  onChange,
  options,
  placeholder = "Selecione",
  value,
}: SelectProps) {
  const [open, setOpen] = useState(false);
  const [shouldRenderList, setShouldRenderList] = useState(false);
  const [isVisible, setIsVisible] = useState(false);
  const containerRef = useRef<HTMLDivElement | null>(null);
  const buttonId = useId();
  const listboxId = useId();

  const selectedOption = useMemo(
    () => options.find((option) => option.value === value) ?? null,
    [options, value],
  );

  useEffect(() => {
    let animationFrame = 0;
    let visibilityFrame = 0;
    let closeTimeout = 0;

    if (open) {
      animationFrame = window.requestAnimationFrame(() => {
        setShouldRenderList(true);
        visibilityFrame = window.requestAnimationFrame(() => {
          setIsVisible(true);
        });
      });
    } else if (shouldRenderList) {
      animationFrame = window.requestAnimationFrame(() => {
        setIsVisible(false);
      });

      closeTimeout = window.setTimeout(() => {
        setShouldRenderList(false);
      }, 250);
    }

    return () => {
      window.cancelAnimationFrame(animationFrame);
      window.cancelAnimationFrame(visibilityFrame);
      window.clearTimeout(closeTimeout);
    };
  }, [open, shouldRenderList]);

  useEffect(() => {
    if (!open) {
      return;
    }

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
        className="flex w-full cursor-pointer items-center justify-end gap-2 bg-transparent text-right font-medium outline-none disabled:cursor-not-allowed disabled:text-[var(--muted)]"
      >
        <span
          className={`truncate ${selectedOption ? "text-[var(--foreground)]" : "text-[var(--muted)]"}`}
        >
          {selectedOption?.label ?? placeholder}
        </span>
        <span className="text-[var(--muted)]">
          <ChevronDownIcon open={open} />
        </span>
      </button>

      {open ? <div /> : null}

      {shouldRenderList ? (
        <div
          className={`absolute right-0 top-[calc(100%+0.75rem)] z-20 min-w-full origin-top overflow-hidden rounded-2xl border border-[var(--border)] bg-white shadow-[0_20px_45px_rgba(15,23,42,0.12)] transition-all duration-250 ease-[cubic-bezier(0.22,1,0.36,1)] ${
            isVisible
              ? "translate-y-0 scale-100 opacity-100"
              : "pointer-events-none -translate-y-3 scale-95 opacity-0"
          }`}
        >
          {helper ? (
            <div className="border-b border-[var(--border)] px-4 py-3 text-xs text-[var(--muted)]">
              {helper}
            </div>
          ) : null}

          {options.length === 0 ? (
            <div className="px-4 py-3 text-sm text-[var(--muted)]">{emptyLabel}</div>
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
                    <button
                      type="button"
                      onClick={() => {
                        onChange(option.value);
                        setOpen(false);
                      }}
                      className={`flex w-full cursor-pointer items-center justify-between gap-3 px-4 py-3 text-left text-sm transition hover:bg-[var(--surface-muted)] ${
                        isSelected
                          ? "bg-[var(--primary-soft)] font-semibold text-[var(--foreground)]"
                          : "text-[var(--foreground)]"
                      }`}
                    >
                      <span className="truncate">{option.label}</span>
                      {isSelected ? (
                        <span className="h-2.5 w-2.5 rounded-full bg-[var(--primary)]" />
                      ) : null}
                    </button>
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
