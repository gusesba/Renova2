"use client";

import { useState } from "react";

import { useStoreContext } from "@/app/dashboard/store-context";
import { ProfileMenu } from "@/app/components/layout/profile-menu";
import { StoreConfigModal } from "@/app/components/layout/store-config-modal";
import { UserEditModal } from "@/app/components/layout/user-edit-modal";
import { Select } from "@/app/components/ui/select";
import { permissions } from "@/lib/access";
import { toast } from "sonner";

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

type AppHeaderProps = {
  isCollapsed?: boolean;
};

export function AppHeader({ isCollapsed = false }: AppHeaderProps) {
  const [isStoreConfigOpen, setIsStoreConfigOpen] = useState(false);
  const [isUserEditOpen, setIsUserEditOpen] = useState(false);
  const {
    currentUser,
    hasPermission,
    isLoadingStores,
    setCurrentUser,
    selectedStore,
    selectedStoreId,
    setSelectedStoreId,
    stores,
  } =
    useStoreContext();
  const canEditStoreConfig = hasPermission(permissions.configLojaEditar);

  const initials =
    currentUser?.nome
      ?.split(" ")
      .filter(Boolean)
      .slice(0, 2)
      .map((part) => part[0]?.toUpperCase())
      .join("") ?? "RN";

  const storeOptions = stores.map((store) => ({
    label: store.nome,
    value: String(store.id),
  }));

  return (
    <header
      className={`border-b border-[var(--border)] bg-[var(--surface)] transition-all duration-300 ${
        isCollapsed
          ? "max-h-0 overflow-hidden border-b-0 px-0 py-0 opacity-0"
          : "relative z-40 max-h-64 overflow-visible px-4 py-4 opacity-100 sm:px-6 lg:px-8"
      }`}
      aria-hidden={isCollapsed}
    >
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
            <div className="min-w-0 flex-1">
              <Select
                ariaLabel="Selecionar loja"
                value={selectedStoreId ? String(selectedStoreId) : null}
                options={storeOptions}
                disabled={isLoadingStores || stores.length === 0}
                placeholder={isLoadingStores ? "Carregando lojas..." : "Selecionar loja"}
                emptyLabel="Nenhuma loja cadastrada"
                helper={isLoadingStores ? "As lojas estao sendo carregadas." : null}
                onChange={(value) => {
                  setSelectedStoreId(value ? Number(value) : null);
                }}
              />
            </div>
          </label>

          <button
            type="button"
            className="flex h-12 w-12 items-center justify-center rounded-2xl border border-[var(--border)] bg-white text-[var(--muted)] transition hover:border-[var(--border-strong)] hover:text-[var(--foreground)]"
            aria-label="Notificacoes"
          >
            <BellIcon />
          </button>

          <ProfileMenu
            initials={initials}
            name={currentUser?.nome ?? null}
            email={currentUser?.email ?? null}
            hasActiveStore={Boolean(selectedStoreId)}
            onEditUser={() => {
              setIsUserEditOpen(true);
            }}
            onOpenSettings={() => {
              if (!selectedStoreId) {
                toast.error("Selecione uma loja antes de abrir as configuracoes.");
                return;
              }

              if (!canEditStoreConfig) {
                toast.error("Voce nao tem permissao para editar as configuracoes da loja.");
                return;
              }

              setIsStoreConfigOpen(true);
            }}
          />
        </div>
      </div>

      <StoreConfigModal
        isOpen={isStoreConfigOpen}
        storeId={selectedStoreId}
        storeName={selectedStore?.nome ?? null}
        onClose={() => {
          setIsStoreConfigOpen(false);
        }}
      />
      <UserEditModal
        isOpen={isUserEditOpen}
        userId={currentUser?.id ?? null}
        currentName={currentUser?.nome ?? null}
        onClose={() => {
          setIsUserEditOpen(false);
        }}
        onSaved={(user) => {
          setCurrentUser(user);
        }}
      />
    </header>
  );
}
