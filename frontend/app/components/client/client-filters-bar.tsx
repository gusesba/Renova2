"use client";

import { useState } from "react";

import type { ClientFilters } from "@/lib/client";
import { GearIcon } from "@/app/components/ui/gear-icon";
import { Select } from "@/app/components/ui/select";

type ClientFiltersBarProps = {
  filters: ClientFilters;
  hasStore: boolean;
  isLoading: boolean;
  canAddClient: boolean;
  canExportClosing: boolean;
  onAddClient: () => void;
  onOpenClosing: () => void;
  onOpenSettings: () => void;
  onChange: (next: Partial<ClientFilters>) => void;
};

function FilterField({
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

export function ClientFiltersBar({
  filters,
  hasStore,
  isLoading,
  canAddClient,
  canExportClosing,
  onAddClient,
  onOpenClosing,
  onOpenSettings,
  onChange,
}: ClientFiltersBarProps) {
  const [isExpanded, setIsExpanded] = useState(false);

  return (
    <div className="space-y-5">
      <div className="flex flex-col gap-4 lg:flex-row lg:items-end lg:justify-between">
        <div>
          <h2 className="text-xl font-semibold text-[var(--foreground)]">Lista de clientes</h2>
          <p className="mt-1 text-sm text-[var(--muted)]">
            Use filtros simples para navegar entre cadastros da loja ativa.
          </p>
        </div>

        <div className="grid gap-3 sm:grid-cols-2 lg:flex">
          <button
            type="button"
            onClick={onOpenSettings}
            className="flex h-12 w-full cursor-pointer items-center justify-center gap-2 rounded-2xl bg-[linear-gradient(90deg,_#ff8a3d,_#ff6b3d)] px-4 text-sm font-semibold text-white shadow-[0_16px_30px_rgba(255,107,61,0.28)] transition hover:brightness-105 sm:px-5 lg:w-12 lg:px-0"
            aria-label="Configurar tabela de clientes"
          >
            <GearIcon />
            <span className="lg:hidden">Configurar</span>
          </button>
          {canExportClosing ? (
            <button
              type="button"
              disabled={!hasStore || isLoading}
              onClick={onOpenClosing}
              className="flex h-12 w-full cursor-pointer items-center justify-center rounded-2xl bg-[linear-gradient(90deg,_#ff8a3d,_#ff6b3d)] px-5 text-sm font-semibold text-white shadow-[0_16px_30px_rgba(255,107,61,0.28)] transition hover:brightness-105 disabled:cursor-not-allowed disabled:opacity-60"
            >
              Fechamento
            </button>
          ) : null}
          {canAddClient ? (
            <button
              type="button"
              disabled={!hasStore || isLoading}
              onClick={onAddClient}
              className="flex h-12 w-full cursor-pointer items-center justify-center rounded-2xl bg-[linear-gradient(90deg,_#ff8a3d,_#ff6b3d)] px-5 text-sm font-semibold text-white shadow-[0_16px_30px_rgba(255,107,61,0.28)] transition hover:brightness-105 disabled:cursor-not-allowed disabled:opacity-60 sm:col-span-2 lg:col-auto"
            >
              Novo cliente
            </button>
          ) : null}
        </div>
      </div>

      <div className="rounded-[24px] border border-[var(--border)] bg-[var(--surface)]/55">
        <button
          type="button"
          onClick={() => setIsExpanded((current) => !current)}
          aria-expanded={isExpanded}
          className="flex w-full cursor-pointer items-center justify-between gap-3 px-4 py-3 text-left text-sm font-medium text-[var(--foreground)]"
        >
          <div className="flex items-center gap-2">
            <span className="inline-flex h-2 w-2 rounded-full bg-[var(--primary)]/70" />
            <span>Filtros e ordenacao</span>
          </div>
          <span
            className={`text-xs text-[var(--muted)] transition-transform duration-300 ${
              isExpanded ? "rotate-180" : ""
            }`}
          >
            ▾
          </span>
        </button>

        <div
          className={`grid transition-[grid-template-rows] duration-300 ease-out ${
            isExpanded ? "grid-rows-[1fr]" : "grid-rows-[0fr]"
          }`}
        >
          <div className={isExpanded ? "overflow-visible" : "overflow-hidden"}>
            <div
              className={`grid gap-4 border-t border-[var(--border)] px-4 transition-all duration-300 ease-out xl:grid-cols-[minmax(0,1.2fr)_minmax(0,1fr)_220px_180px] ${
                isExpanded ? "py-4 opacity-100" : "py-0 opacity-0"
              }`}
            >
              <FilterField
                label="Nome"
                value={filters.nome}
                placeholder="Buscar por nome"
                onChange={(nome) => onChange({ nome })}
              />
              <FilterField
                label="Contato"
                value={filters.contato}
                placeholder="Buscar por contato"
                onChange={(contato) => onChange({ contato })}
              />
              <SelectField
                label="Ordenar por"
                value={filters.ordenarPor}
                options={[
                  { label: "Nome", value: "nome" },
                  { label: "Contato", value: "contato" },
                  { label: "Id", value: "id" },
                ]}
                onChange={(ordenarPor) =>
                  onChange({ ordenarPor: ordenarPor as ClientFilters["ordenarPor"] })
                }
              />
              <SelectField
                label="Direcao"
                value={filters.direcao}
                options={[
                  { label: "Crescente", value: "asc" },
                  { label: "Decrescente", value: "desc" },
                ]}
                onChange={(direcao) =>
                  onChange({ direcao: direcao as ClientFilters["direcao"] })
                }
              />
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
