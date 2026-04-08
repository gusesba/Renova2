"use client";

type ThemedCheckboxProps = {
  checked: boolean;
  disabled?: boolean;
  label: string;
  onChange: (checked: boolean) => void;
};

export function ThemedCheckbox({
  checked,
  disabled = false,
  label,
  onChange,
}: ThemedCheckboxProps) {
  return (
    <label
      className={`flex items-center gap-3 rounded-2xl border px-4 py-3 transition ${
        checked
          ? "border-[color:color-mix(in_srgb,var(--primary)_32%,white)] bg-[linear-gradient(180deg,rgba(106,92,255,0.12),rgba(106,92,255,0.05))] shadow-[0_14px_30px_rgba(106,92,255,0.12)]"
          : "border-[var(--border)] bg-[var(--surface-muted)]"
      } ${disabled ? "cursor-not-allowed opacity-60" : "cursor-pointer hover:border-[var(--primary)]"}`}
    >
      <input
        type="checkbox"
        checked={checked}
        disabled={disabled}
        onChange={(event) => onChange(event.target.checked)}
        className="sr-only"
      />
      <span
        aria-hidden="true"
        className={`flex h-6 w-6 items-center justify-center rounded-lg border transition ${
          checked
            ? "border-[var(--primary)] bg-[linear-gradient(180deg,var(--primary),#7b70ff)] text-white shadow-[0_10px_20px_rgba(106,92,255,0.28)]"
            : "border-[var(--border-strong)] bg-white text-transparent"
        }`}
      >
        <svg viewBox="0 0 16 16" className="h-3.5 w-3.5" fill="none">
          <path
            d="M3.5 8.5 6.5 11.5 12.5 5"
            stroke="currentColor"
            strokeLinecap="round"
            strokeLinejoin="round"
            strokeWidth="2"
          />
        </svg>
      </span>
      <span className="text-sm font-semibold text-[var(--foreground)]">{label}</span>
    </label>
  );
}
