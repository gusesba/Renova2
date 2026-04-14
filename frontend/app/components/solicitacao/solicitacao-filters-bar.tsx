"use client";

import { useState } from "react";

import { GearIcon } from "@/app/components/ui/gear-icon";
import { Select } from "@/app/components/ui/select";
import type { SolicitacaoFilters } from "@/lib/solicitacao";

type SolicitacaoFiltersBarProps = {
  filters: SolicitacaoFilters;
  hasStore: boolean;
  isLoading: boolean;
  onAddSolicitacao: () => void;
  onOpenSettings: () => void;
  onChange: (next: Partial<SolicitacaoFilters>) => void;
};

function Field({
  label,
  value,
  placeholder,
  type = "text",
  onChange,
}: {
  label: string;
  value: string;
  placeholder: string;
  type?: "text" | "number";
  onChange: (value: string) => void;
}) {
  return (
    <label className="space-y-2">
      <span className="text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
        {label}
      </span>
      <input
        type={type}
        step={type === "number" ? "0.01" : undefined}
        min={type === "number" ? "0" : undefined}
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

export function SolicitacaoFiltersBar({
  filters,
  hasStore,
  isLoading,
  onAddSolicitacao,
  onOpenSettings,
  onChange,
}: SolicitacaoFiltersBarProps) {
  const [isExpanded, setIsExpanded] = useState(false);

  return (
    <div className="space-y-5">
      <div className="flex flex-col gap-4 lg:flex-row lg:items-end lg:justify-between">
        <div>
          <h2 className="text-xl font-semibold text-[var(--foreground)]">Lista de solicitacoes</h2>
          <p className="mt-1 text-sm text-[var(--muted)]">
            Filtre por cliente, auxiliares, descricao e faixa de preco desejada.
          </p>
        </div>

        <div className="flex gap-3">
          <button
            type="button"
            onClick={onOpenSettings}
            className="flex h-12 w-12 cursor-pointer items-center justify-center rounded-2xl bg-[linear-gradient(90deg,_#ff8a3d,_#ff6b3d)] text-white shadow-[0_16px_30px_rgba(255,107,61,0.28)] transition hover:brightness-105"
            aria-label="Configurar tabela de solicitacoes"
          >
            <GearIcon />
          </button>
          <button
            type="button"
            disabled={!hasStore || isLoading}
            onClick={onAddSolicitacao}
            className="flex h-12 cursor-pointer items-center justify-center rounded-2xl bg-[linear-gradient(90deg,_#ff8a3d,_#ff6b3d)] px-5 text-sm font-semibold text-white shadow-[0_16px_30px_rgba(255,107,61,0.28)] transition hover:brightness-105 disabled:cursor-not-allowed disabled:opacity-60"
          >
            Nova solicitacao
          </button>
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
              className={`grid gap-4 border-t border-[var(--border)] px-4 transition-all duration-300 ease-out xl:grid-cols-4 ${
                isExpanded ? "py-4 opacity-100" : "py-0 opacity-0"
              }`}
            >
              <Field
                label="Descricao"
                value={filters.descricao}
                placeholder="Buscar por descricao"
                onChange={(descricao) => onChange({ descricao })}
              />
              <Field
                label="Cliente"
                value={filters.cliente}
                placeholder="Buscar por cliente"
                onChange={(cliente) => onChange({ cliente })}
              />
              <Field
                label="Produto"
                value={filters.produto}
                placeholder="Buscar por produto"
                onChange={(produto) => onChange({ produto })}
              />
              <Field
                label="Marca"
                value={filters.marca}
                placeholder="Buscar por marca"
                onChange={(marca) => onChange({ marca })}
              />
              <Field
                label="Tamanho"
                value={filters.tamanho}
                placeholder="Buscar por tamanho"
                onChange={(tamanho) => onChange({ tamanho })}
              />
              <Field
                label="Cor"
                value={filters.cor}
                placeholder="Buscar por cor"
                onChange={(cor) => onChange({ cor })}
              />
              <Field
                label="Preco minimo"
                type="number"
                value={filters.precoInicial}
                placeholder="0,00"
                onChange={(precoInicial) => onChange({ precoInicial })}
              />
              <Field
                label="Preco maximo"
                type="number"
                value={filters.precoFinal}
                placeholder="0,00"
                onChange={(precoFinal) => onChange({ precoFinal })}
              />
              <SelectField
                label="Ordenar por"
                value={filters.ordenarPor}
                options={[
                  { label: "Descricao", value: "descricao" },
                  { label: "Cliente", value: "cliente" },
                  { label: "Produto", value: "produto" },
                  { label: "Marca", value: "marca" },
                  { label: "Tamanho", value: "tamanho" },
                  { label: "Cor", value: "cor" },
                  { label: "Preco minimo", value: "precoMinimo" },
                  { label: "Preco maximo", value: "precoMaximo" },
                  { label: "Id", value: "id" },
                ]}
                onChange={(ordenarPor) =>
                  onChange({ ordenarPor: ordenarPor as SolicitacaoFilters["ordenarPor"] })
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
                  onChange({ direcao: direcao as SolicitacaoFilters["direcao"] })
                }
              />
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
