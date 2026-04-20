"use client";

import type { ReactNode } from "react";
import { useEffect, useMemo, useState } from "react";
import { usePathname, useRouter } from "next/navigation";

import { getDashboardRouteForArea, getStoredAccessArea, type AccessArea } from "@/lib/access-area";
import { useStoreContext } from "@/app/dashboard/store-context";
import { menuPermissionGroups } from "@/lib/access";
import { AppHeader } from "./app-header";
import { AppSidebar } from "./app-sidebar";

type AppShellProps = {
  children: ReactNode;
};

export function AppShell({ children }: AppShellProps) {
  const [isMobileChromeOpen, setIsMobileChromeOpen] = useState(false);
  const [accessArea, setAccessArea] = useState<AccessArea>(() =>
    typeof window === "undefined" ? "lojista" : getStoredAccessArea(),
  );
  const pathname = usePathname();
  const router = useRouter();
  const { hasAnyPermission, isLoadingAccess, selectedStoreId } = useStoreContext();

  const allowedRoutes = useMemo(
    () =>
      Object.entries(menuPermissionGroups)
        .filter(([href, permissionList]) =>
          href === "/dashboard/loja"
            ? true
            : selectedStoreId && hasAnyPermission(permissionList),
        )
        .map(([href]) => href),
    [hasAnyPermission, selectedStoreId],
  );

  useEffect(() => {
    if (accessArea === "cliente") {
      if (!pathname.startsWith("/dashboard/area-cliente")) {
        router.replace("/dashboard/area-cliente");
      }

      return;
    }

    if (!pathname.startsWith("/dashboard") || isLoadingAccess) {
      return;
    }

    if (!selectedStoreId) {
      return;
    }

    const matchedEntry = Object.entries(menuPermissionGroups).find(([href]) =>
      pathname === href || pathname.startsWith(`${href}/`),
    );

    if (!matchedEntry) {
      return;
    }

    const [matchedRoute, permissionList] = matchedEntry;
    const isAllowed = matchedRoute === "/dashboard/loja" || hasAnyPermission(permissionList);

    if (isAllowed) {
      return;
    }

    router.replace(allowedRoutes[0] ?? "/dashboard/loja");
  }, [accessArea, allowedRoutes, hasAnyPermission, isLoadingAccess, pathname, router, selectedStoreId]);

  useEffect(() => {
    if (accessArea === "lojista" && pathname.startsWith("/dashboard/area-cliente")) {
      router.replace(getDashboardRouteForArea(accessArea));
    }
  }, [accessArea, pathname, router]);

  return (
    <div className="h-screen overflow-hidden bg-[var(--background)] p-4 lg:p-6">
      <div className="relative mx-auto flex h-full w-full max-w-[1600px] overflow-hidden rounded-[28px] border border-[var(--border)] bg-[var(--surface)] shadow-[var(--shadow-soft)] transition-all duration-300">
        <button
          type="button"
          onClick={() => setIsMobileChromeOpen((current) => !current)}
          aria-label={isMobileChromeOpen ? "Fechar header e menu lateral" : "Abrir header e menu lateral"}
          aria-expanded={isMobileChromeOpen}
          className="absolute top-2 left-2 z-50 flex h-11 w-11 items-center justify-center rounded-2xl border border-[var(--border)] bg-white/92 text-[var(--foreground)] shadow-[0_16px_36px_rgba(15,23,42,0.12)] backdrop-blur transition hover:border-[var(--border-strong)] hover:bg-white lg:hidden"
        >
          <svg aria-hidden="true" viewBox="0 0 24 24" className="h-5 w-5">
            <path
              d={isMobileChromeOpen ? "M6 6l12 12M18 6L6 18" : "M4 7h16M4 12h16M4 17h16"}
              fill="none"
              stroke="currentColor"
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth="1.8"
            />
          </svg>
        </button>

        {isMobileChromeOpen ? (
          <button
            type="button"
            aria-label="Fechar header e menu lateral"
            onClick={() => setIsMobileChromeOpen(false)}
            className="absolute inset-0 z-30 bg-[rgba(15,23,42,0.36)] backdrop-blur-[2px] lg:hidden"
          />
        ) : null}

        <AppSidebar
          accessArea={accessArea}
          isMobileOpen={isMobileChromeOpen}
          onNavigate={() => setIsMobileChromeOpen(false)}
        />
        <div className="flex min-h-0 min-w-0 flex-1 flex-col bg-[var(--surface-muted)]">
          <AppHeader
            accessArea={accessArea}
            isMobileOpen={isMobileChromeOpen}
            onAccessAreaChange={setAccessArea}
            onNavigate={() => setIsMobileChromeOpen(false)}
          />
          <main className="min-h-0 min-w-0 flex-1 overflow-x-hidden overflow-y-auto p-4 transition-[padding] duration-300 sm:p-6 lg:p-8">
            <div className="mx-auto w-full min-w-0 max-w-[1250px]">
              {children}
            </div>
          </main>
        </div>
      </div>
    </div>
  );
}
