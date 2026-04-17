"use client";

import { useRouter } from "next/navigation";
import { useEffect, useRef, useState } from "react";

import { clearAuthSession } from "@/lib/auth";
import { GearIcon } from "@/app/components/ui/gear-icon";

type ProfileMenuProps = {
  email: string | null;
  hasActiveStore: boolean;
  initials: string;
  name: string | null;
  onEditUser: () => void;
  onOpenSettings: () => void;
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

function LogoutIcon() {
  return (
    <svg aria-hidden="true" viewBox="0 0 24 24" className="h-4 w-4">
      <path
        d="M15 3h3a2 2 0 012 2v14a2 2 0 01-2 2h-3M10 17l5-5-5-5M15 12H4"
        fill="none"
        stroke="currentColor"
        strokeLinecap="round"
        strokeLinejoin="round"
        strokeWidth="1.8"
      />
    </svg>
  );
}

export function ProfileMenu({
  email,
  hasActiveStore,
  initials,
  name,
  onEditUser,
  onOpenSettings,
}: ProfileMenuProps) {
  const router = useRouter();
  const containerRef = useRef<HTMLDivElement | null>(null);
  const [open, setOpen] = useState(false);
  const [shouldRenderList, setShouldRenderList] = useState(false);
  const [isVisible, setIsVisible] = useState(false);

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

  function handleLogout() {
    clearAuthSession();
    setOpen(false);
    router.replace("/auth");
  }

  function handleOpenSettings() {
    setOpen(false);
    onOpenSettings();
  }

  function handleEditUser() {
    setOpen(false);
    onEditUser();
  }

  return (
    <div ref={containerRef} className="relative z-50 min-w-0">
      <button
        type="button"
        aria-expanded={open}
        aria-haspopup="menu"
        onClick={() => {
          setOpen((current) => !current);
        }}
        className="flex min-w-0 cursor-pointer items-center gap-3 rounded-2xl border border-[var(--border)] bg-white px-3 py-2 text-left shadow-[0_12px_30px_rgba(15,23,42,0.04)] transition hover:border-[var(--border-strong)]"
      >
        <div className="flex h-11 w-11 items-center justify-center rounded-2xl bg-[linear-gradient(135deg,_#ff7b7b,_#ffb36b)] text-sm font-semibold text-white">
          {initials}
        </div>
        <div className="min-w-0">
          <p className="truncate text-sm font-semibold text-[var(--foreground)]">
            {name?.trim().split(/\s+/)?.[0] ?? "Usuario"}
          </p>
          <p className="truncate text-xs text-[var(--muted)]">{email ?? "Sessao autenticada"}</p>
        </div>
        <span className="text-[var(--muted)]">
          <ChevronDownIcon open={open} />
        </span>
      </button>

      {shouldRenderList ? (
        <div
          className={`absolute right-0 top-[calc(100%+0.75rem)] z-50 min-w-[220px] origin-top overflow-hidden rounded-2xl border border-[var(--border)] bg-white shadow-[0_20px_45px_rgba(15,23,42,0.12)] transition-all duration-250 ease-[cubic-bezier(0.22,1,0.36,1)] ${
            isVisible
              ? "translate-y-0 scale-100 opacity-100"
              : "pointer-events-none -translate-y-3 scale-95 opacity-0"
          }`}
        >
          <button
            type="button"
            role="menuitem"
            onClick={handleEditUser}
            className="w-full cursor-pointer border-b border-[var(--border)] px-4 py-3 text-left transition hover:bg-[var(--surface-muted)]"
          >
            <p className="truncate text-sm font-semibold text-[var(--foreground)]">
              {name?.trim().split(/\s+/)?.[0] ?? "Usuario"}
            </p>
            <p className="truncate text-xs text-[var(--muted)]">{email ?? "Sessao autenticada"}</p>
          </button>

          <div className="py-2">
            <button
              type="button"
              role="menuitem"
              onClick={handleOpenSettings}
              disabled={!hasActiveStore}
              className="flex w-full cursor-pointer items-center justify-between gap-3 px-4 py-3 text-left text-sm text-[var(--foreground)] transition hover:bg-[var(--surface-muted)] disabled:cursor-not-allowed disabled:opacity-50"
            >
              <span>Configuracoes</span>
              <span className="text-[var(--muted)]">
                <GearIcon className="h-4 w-4 fill-current" />
              </span>
            </button>
            <button
              type="button"
              role="menuitem"
              onClick={handleLogout}
              className="flex w-full cursor-pointer items-center justify-between gap-3 px-4 py-3 text-left text-sm text-[#d14343] transition hover:bg-[#fff1f1]"
            >
              <span>Sair</span>
              <span className="text-[#d14343]">
                <LogoutIcon />
              </span>
            </button>
          </div>
        </div>
      ) : null}
    </div>
  );
}
