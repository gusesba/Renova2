import type { StoreFieldErrors, StoreFormValues } from "@/lib/store";

type StoreRegistrationFormProps = {
  errors: StoreFieldErrors;
  isSubmitting: boolean;
  values: StoreFormValues;
  onChange: (value: string) => void;
  onReset: () => void;
  onSubmit: (event: React.FormEvent<HTMLFormElement>) => Promise<void>;
};

function StoreField({
  error,
  onChange,
  value,
}: {
  error?: string;
  onChange: (value: string) => void;
  value: string;
}) {
  return (
    <label className="block space-y-2">
      <span className="text-sm font-semibold text-[var(--foreground)]">Nome da loja</span>
      <input
        type="text"
        value={value}
        onChange={(event) => onChange(event.target.value)}
        placeholder="Ex.: Atelier Centro"
        className={`h-13 w-full rounded-2xl border bg-white px-4 text-sm text-[var(--foreground)] outline-none transition ${
          error
            ? "border-red-300 shadow-[0_0_0_4px_rgba(248,113,113,0.12)]"
            : "border-[var(--border)] focus:border-[var(--primary)] focus:shadow-[0_0_0_4px_rgba(106,92,255,0.12)]"
        }`}
      />
      {error ? <p className="text-sm text-red-500">{error}</p> : null}
    </label>
  );
}

export function StoreRegistrationForm({
  errors,
  isSubmitting,
  values,
  onChange,
  onReset,
  onSubmit,
}: StoreRegistrationFormProps) {
  return (
    <div className="rounded-[28px] border border-[var(--border)] bg-white p-6 shadow-[0_18px_45px_rgba(15,23,42,0.05)]">
      <form className="space-y-5" onSubmit={onSubmit} noValidate>
        <StoreField value={values.nome} onChange={onChange} error={errors.nome} />

        <div className="flex flex-col gap-3 sm:flex-row">
          <button
            type="submit"
            disabled={isSubmitting}
            className="flex h-13 items-center justify-center rounded-2xl bg-[linear-gradient(90deg,_#ff8a3d,_#ff6b3d)] px-6 text-sm font-semibold text-white shadow-[0_16px_30px_rgba(255,107,61,0.28)] transition hover:brightness-105 disabled:cursor-not-allowed disabled:opacity-70"
          >
            {isSubmitting ? "Salvando loja..." : "Cadastrar loja"}
          </button>

          <button
            type="button"
            onClick={onReset}
            className="flex h-13 items-center justify-center rounded-2xl border border-[var(--border)] bg-[var(--surface-muted)] px-6 text-sm font-semibold text-[var(--foreground)] transition hover:border-[var(--border-strong)] hover:bg-white"
          >
            Limpar
          </button>
        </div>
      </form>
    </div>
  );
}
