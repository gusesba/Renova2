import { GearIcon } from "@/app/components/ui/gear-icon";
import { Select } from "@/app/components/ui/select";
import {
  paymentCreditTypeOptions,
  type ExternalPaymentFilters,
} from "@/lib/payment";

type ExternalPaymentFiltersBarProps = {
  filters: ExternalPaymentFilters;
  isLoading: boolean;
  onOpenSettings: () => void;
  onChange: (next: Partial<ExternalPaymentFilters>) => void;
};

function TextField({
  label,
  value,
  placeholder,
  onChange,
}: {
  label: string;
  value: string;
  placeholder: string;
  onChange: (value: string) => void;
}) {
  return (
    <label className="space-y-2">
      <span className="text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
        {label}
      </span>
      <input
        type="text"
        value={value}
        placeholder={placeholder}
        onChange={(event) => onChange(event.target.value)}
        className="h-12 w-full rounded-2xl border border-[var(--border)] bg-white px-4 text-sm text-[var(--foreground)] outline-none transition focus:border-[var(--primary)] focus:shadow-[0_0_0_4px_rgba(106,92,255,0.12)]"
      />
    </label>
  );
}

function DateField({
  label,
  value,
  onChange,
}: {
  label: string;
  value: string;
  onChange: (value: string) => void;
}) {
  return (
    <label className="space-y-2">
      <span className="text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
        {label}
      </span>
      <input
        type="date"
        value={value}
        onChange={(event) => onChange(event.target.value)}
        className="h-12 w-full rounded-2xl border border-[var(--border)] bg-white px-4 text-sm text-[var(--foreground)] outline-none transition focus:border-[var(--primary)] focus:shadow-[0_0_0_4px_rgba(106,92,255,0.12)]"
      />
    </label>
  );
}

function SelectField({
  label,
  value,
  options,
  onChange,
}: {
  label: string;
  value: string;
  options: Array<{ label: string; value: string }>;
  onChange: (value: string) => void;
}) {
  return (
    <div className="space-y-2">
      <span className="text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
        {label}
      </span>
      <div className="rounded-2xl border border-[var(--border)] bg-white px-4 py-3 text-sm text-[var(--foreground)]">
        <Select
          ariaLabel={label}
          value={value}
          options={options}
          placeholder="Selecionar"
          onChange={onChange}
        />
      </div>
    </div>
  );
}

export function ExternalPaymentFiltersBar({
  filters,
  isLoading,
  onOpenSettings,
  onChange,
}: ExternalPaymentFiltersBarProps) {
  return (
    <div className="space-y-5">
      <div className="flex flex-col gap-4 lg:flex-row lg:items-end lg:justify-between">
        <div>
          <h2 className="text-xl font-semibold text-[var(--foreground)]">
            Pagamentos externos
          </h2>
          <p className="mt-1 text-sm text-[var(--muted)]">
            Filtre por periodo, cliente e tipo do lancamento externo.
          </p>
        </div>

        <div className="flex gap-3">
          <button
            type="button"
            onClick={onOpenSettings}
            disabled={isLoading}
            className="flex h-12 w-12 cursor-pointer items-center justify-center rounded-2xl bg-[linear-gradient(90deg,_#ff8a3d,_#ff6b3d)] text-white shadow-[0_16px_30px_rgba(255,107,61,0.28)] transition hover:brightness-105 disabled:cursor-not-allowed disabled:opacity-60"
            aria-label="Configurar tabela de pagamentos externos"
          >
            <GearIcon />
          </button>
        </div>
      </div>

      <div className="grid gap-4 xl:grid-cols-4">
        <DateField
          label="Data inicial"
          value={filters.dataInicial}
          onChange={(dataInicial) => onChange({ dataInicial })}
        />
        <DateField
          label="Data final"
          value={filters.dataFinal}
          onChange={(dataFinal) => onChange({ dataFinal })}
        />
        <TextField
          label="Cliente"
          value={filters.cliente}
          placeholder="Buscar por cliente"
          onChange={(cliente) => onChange({ cliente })}
        />
        <SelectField
          label="Tipo"
          value={filters.tipo}
          options={[
            { label: "Todos", value: "" },
            ...paymentCreditTypeOptions.map((option) => ({
              label: option.label,
              value: String(option.value),
            })),
          ]}
          onChange={(tipo) => onChange({ tipo })}
        />
        <SelectField
          label="Ordenar por"
          value={filters.ordenarPor}
          options={[
            { label: "Data", value: "data" },
            { label: "Cliente", value: "cliente" },
            { label: "Tipo", value: "tipo" },
            { label: "Credito", value: "valorCredito" },
            { label: "Dinheiro", value: "valorDinheiro" },
            { label: "Id", value: "id" },
          ]}
          onChange={(ordenarPor) =>
            onChange({ ordenarPor: ordenarPor as ExternalPaymentFilters["ordenarPor"] })
          }
        />
        <SelectField
          label="Direcao"
          value={filters.direcao}
          options={[
            { label: "Crescente", value: "asc" },
            { label: "Decrescente", value: "desc" },
          ]}
          onChange={(direcao) => onChange({ direcao: direcao as ExternalPaymentFilters["direcao"] })}
        />
      </div>
    </div>
  );
}
