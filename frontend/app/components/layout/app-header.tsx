"use client";

import { useStoreContext } from "@/app/dashboard/store-context";

function SearchIcon() {
  return (
    <svg aria-hidden="true" viewBox="0 0 24 24" className="h-4 w-4">
      <path
        d="M21 21l-4.35-4.35m1.85-5.15a7 7 0 11-14 0 7 7 0 0114 0z"
        fill="none"
        stroke="currentColor"
        strokeLinecap="round"
        strokeLinejoin="round"
        strokeWidth="1.8"
      />
    </svg>
  );
}

function BellIcon() {
  return (
    <svg aria-hidden="true" viewBox="0 0 24 24" className="h-5 w-5">
      <path
        d="M15 17H9m9-1V11a6 6 0 10-12 0v5l-1.2 2.1A1 1 0 005.67 20h12.66a1 1 0 00.87-1.49L18 16zm-8 4a2 2 0 004 0"
        fill="none"
        stroke="currentColor"
        strokeLinecap="round"
        strokeLinejoin="round"
        strokeWidth="1.8"
      />
    </svg>
  );
}

function ChevronDownIcon() {
  return (
    <svg aria-hidden="true" viewBox="0 0 24 24" className="h-4 w-4">
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

export function AppHeader() {
  const { currentUser, isLoadingStores, selectedStoreId, setSelectedStoreId, stores } =
    useStoreContext();

  const initials =
    currentUser?.nome
      ?.split(" ")
      .filter(Boolean)
      .slice(0, 2)
      .map((part) => part[0]?.toUpperCase())
      .join("") ?? "RN";

  return (
    <header className="border-b border-[var(--border)] bg-[var(--surface)] px-4 py-4 sm:px-6 lg:px-8">
      <div className="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
        <label className="flex h-12 w-full max-w-md items-center gap-3 rounded-2xl border border-[var(--border)] bg-white px-4 text-[var(--muted)] shadow-[0_12px_30px_rgba(15,23,42,0.04)]">
          <SearchIcon />
          <input
            type="search"
            placeholder="Buscar"
            className="w-full bg-transparent text-sm text-[var(--foreground)] outline-none placeholder:text-[var(--muted)]"
          />
        </label>

        <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-end">
          <label className="flex min-w-0 items-center gap-3 rounded-2xl border border-[var(--border)] bg-white px-4 py-3 text-sm text-[var(--foreground)] shadow-[0_12px_30px_rgba(15,23,42,0.04)] sm:min-w-[280px]">
            <span className="text-xs font-semibold uppercase tracking-[0.18em] text-[var(--muted)]">
              Loja
            </span>
            <div className="relative min-w-0 flex-1">
              <select
                value={selectedStoreId ?? ""}
                onChange={(event) => {
                  const value = event.target.value;
                  setSelectedStoreId(value ? Number(value) : null);
                }}
                disabled={isLoadingStores || stores.length === 0}
                className="w-full appearance-none bg-transparent pr-6 text-right font-medium outline-none disabled:cursor-not-allowed disabled:text-[var(--muted)]"
                aria-label="Selecionar loja"
              >
                {isLoadingStores ? <option value="">Carregando lojas...</option> : null}
                {!isLoadingStores && stores.length === 0 ? (
                  <option value="">Nenhuma loja cadastrada</option>
                ) : null}
                {stores.map((store) => (
                  <option key={store.id} value={store.id}>
                    {store.nome}
                  </option>
                ))}
              </select>
              <span className="pointer-events-none absolute right-0 top-1/2 -translate-y-1/2 text-[var(--muted)]">
                <ChevronDownIcon />
              </span>
            </div>
          </label>

          <button
            type="button"
            className="flex h-12 w-12 items-center justify-center rounded-2xl border border-[var(--border)] bg-white text-[var(--muted)] transition hover:border-[var(--border-strong)] hover:text-[var(--foreground)]"
            aria-label="Notificacoes"
          >
            <BellIcon />
          </button>

          <button
            type="button"
            className="flex min-w-0 items-center gap-3 rounded-2xl border border-[var(--border)] bg-white px-3 py-2 text-left shadow-[0_12px_30px_rgba(15,23,42,0.04)] transition hover:border-[var(--border-strong)]"
          >
            <div className="flex h-11 w-11 items-center justify-center rounded-2xl bg-[linear-gradient(135deg,_#ff7b7b,_#ffb36b)] text-sm font-semibold text-white">
              {initials}
            </div>
            <div className="min-w-0">
              <p className="truncate text-sm font-semibold text-[var(--foreground)]">
                {currentUser?.nome ?? "Usuario"}
              </p>
              <p className="truncate text-xs text-[var(--muted)]">
                {currentUser?.email ?? "Sessao autenticada"}
              </p>
            </div>
            <span className="text-[var(--muted)]">
              <ChevronDownIcon />
            </span>
          </button>
        </div>
      </div>
    </header>
  );
}
