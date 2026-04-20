"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";

import { getDashboardRouteForArea, persistAccessArea, type AccessArea } from "@/lib/access-area";
import { useStoreContext } from "@/app/dashboard/store-context";
import { ProfileMenu } from "@/app/components/layout/profile-menu";
import { StoreConfigModal } from "@/app/components/layout/store-config-modal";
import { UserEditModal } from "@/app/components/layout/user-edit-modal";
import { Select } from "@/app/components/ui/select";
import { permissions } from "@/lib/access";
import { toast } from "sonner";

type AppHeaderProps = {
  accessArea: AccessArea;
  isCollapsed?: boolean;
  onAccessAreaChange: (area: AccessArea) => void;
};

export function AppHeader({
  accessArea,
  isCollapsed = false,
  onAccessAreaChange,
}: AppHeaderProps) {
  const [isStoreConfigOpen, setIsStoreConfigOpen] = useState(false);
  const [isUserEditOpen, setIsUserEditOpen] = useState(false);
  const router = useRouter();
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
  const isClientArea = accessArea === "cliente";
  const switchAreaLabel = isClientArea ? "Acessar area do lojista" : "Acessar area do cliente";

  return (
    <header
      className={`border-b border-[var(--border)] bg-[var(--surface)] transition-all duration-300 ${
        isCollapsed
          ? "max-h-0 overflow-hidden border-b-0 px-0 py-0 opacity-0"
          : "relative z-40 max-h-64 overflow-visible px-4 py-4 opacity-100 sm:px-6 lg:px-8"
      }`}
      aria-hidden={isCollapsed}
    >
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-end">
        {!isClientArea ? (
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
        ) : null}

        <button
          type="button"
          onClick={() => {
            const nextArea: AccessArea = isClientArea ? "lojista" : "cliente";
            persistAccessArea(nextArea);
            onAccessAreaChange(nextArea);
            router.replace(getDashboardRouteForArea(nextArea));
          }}
          className="inline-flex h-12 items-center justify-center rounded-2xl border border-[var(--border)] bg-white px-4 text-sm font-semibold text-[var(--foreground)] transition hover:border-[var(--border-strong)] hover:bg-[var(--surface-muted)]"
        >
          {switchAreaLabel}
        </button>

        <ProfileMenu
          initials={initials}
          name={currentUser?.nome ?? null}
          email={currentUser?.email ?? null}
          hasActiveStore={isClientArea || Boolean(selectedStoreId)}
          onEditUser={() => {
            setIsUserEditOpen(true);
          }}
          onOpenSettings={() => {
            if (isClientArea) {
              toast.error("Configuracoes da loja estao disponiveis apenas na area do lojista.");
              return;
            }

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
