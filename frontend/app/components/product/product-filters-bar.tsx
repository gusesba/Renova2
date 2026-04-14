"use client";

import { useState } from "react";

import { GearIcon } from "@/app/components/ui/gear-icon";
import { Select } from "@/app/components/ui/select";
import type { ProductFilters } from "@/lib/product";

type ProductFiltersBarProps = {
  filters: ProductFilters;
  hasStore: boolean;
  isLoading: boolean;
  onAddProduct: () => void;
  onOpenSettings: () => void;
  onChange: (next: Partial<ProductFilters>) => void;
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

function NumberField({
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
        type="number"
        step="0.01"
        min="0"
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

export function ProductFiltersBar({
  filters,
  hasStore,
  isLoading,
  onAddProduct,
  onOpenSettings,
  onChange,
}: ProductFiltersBarProps) {
  const [isExpanded, setIsExpanded] = useState(false);

  return (
    <div className="space-y-5">
      <div className="flex flex-col gap-4 lg:flex-row lg:items-end lg:justify-between">
        <div>
          <h2 className="text-xl font-semibold text-[var(--foreground)]">Lista de produtos</h2>
          <p className="mt-1 text-sm text-[var(--muted)]">
            Filtre por descricao, auxiliares, fornecedor, preco e data de entrada.
          </p>
        </div>

        <div className="flex gap-3">
          <button
            type="button"
            onClick={onOpenSettings}
            className="flex h-12 w-12 cursor-pointer items-center justify-center rounded-2xl bg-[linear-gradient(90deg,_#ff8a3d,_#ff6b3d)] text-white shadow-[0_16px_30px_rgba(255,107,61,0.28)] transition hover:brightness-105"
            aria-label="Configurar tabela de produtos"
          >
            <GearIcon />
          </button>
          <button
            type="button"
            disabled={!hasStore || isLoading}
            onClick={onAddProduct}
            className="flex h-12 cursor-pointer items-center justify-center rounded-2xl bg-[linear-gradient(90deg,_#ff8a3d,_#ff6b3d)] px-5 text-sm font-semibold text-white shadow-[0_16px_30px_rgba(255,107,61,0.28)] transition hover:brightness-105 disabled:cursor-not-allowed disabled:opacity-60"
          >
            Novo produto
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
              <TextField
                label="Descricao"
                value={filters.descricao}
                placeholder="Buscar por descricao"
                onChange={(descricao) => onChange({ descricao })}
              />
              <TextField
                label="Produto"
                value={filters.produto}
                placeholder="Buscar por produto"
                onChange={(produto) => onChange({ produto })}
              />
              <TextField
                label="Marca"
                value={filters.marca}
                placeholder="Buscar por marca"
                onChange={(marca) => onChange({ marca })}
              />
              <TextField
                label="Fornecedor"
                value={filters.fornecedor}
                placeholder="Buscar por fornecedor"
                onChange={(fornecedor) => onChange({ fornecedor })}
              />
              <TextField
                label="Tamanho"
                value={filters.tamanho}
                placeholder="Buscar por tamanho"
                onChange={(tamanho) => onChange({ tamanho })}
              />
              <TextField
                label="Cor"
                value={filters.cor}
                placeholder="Buscar por cor"
                onChange={(cor) => onChange({ cor })}
              />
              <NumberField
                label="Preco inicial"
                value={filters.precoInicial}
                placeholder="0,00"
                onChange={(precoInicial) => onChange({ precoInicial })}
              />
              <NumberField
                label="Preco final"
                value={filters.precoFinal}
                placeholder="0,00"
                onChange={(precoFinal) => onChange({ precoFinal })}
              />
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
              <SelectField
                label="Ordenar por"
                value={filters.ordenarPor}
                options={[
                  { label: "Descricao", value: "descricao" },
                  { label: "Produto", value: "produto" },
                  { label: "Marca", value: "marca" },
                  { label: "Tamanho", value: "tamanho" },
                  { label: "Cor", value: "cor" },
                  { label: "Fornecedor", value: "fornecedor" },
                  { label: "Preco", value: "preco" },
                  { label: "Entrada", value: "entrada" },
                  { label: "Id", value: "id" },
                ]}
                onChange={(ordenarPor) =>
                  onChange({ ordenarPor: ordenarPor as ProductFilters["ordenarPor"] })
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
                  onChange({ direcao: direcao as ProductFilters["direcao"] })
                }
              />
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
