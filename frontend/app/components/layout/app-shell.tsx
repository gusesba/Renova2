"use client";

import type { ReactNode } from "react";
import { useEffect, useMemo, useState } from "react";
import { usePathname, useRouter } from "next/navigation";

import { getDashboardRouteForArea, getStoredAccessArea, persistAccessArea, type AccessArea } from "@/lib/access-area";
import { useStoreContext } from "@/app/dashboard/store-context";
import { menuPermissionGroups } from "@/lib/access";
import { getAuthToken } from "@/lib/auth";
import { getLicenseStatus } from "@/services/auth-service";
import { AppHeader } from "./app-header";
import { AppSidebar } from "./app-sidebar";

type AppShellProps = {
  children: ReactNode;
};

export function AppShell({ children }: AppShellProps) {
  const [isMobileChromeOpen, setIsMobileChromeOpen] = useState(false);
  const [isDesktopChromeVisible, setIsDesktopChromeVisible] = useState(true);
  const [isDesktopViewport, setIsDesktopViewport] = useState(() =>
    typeof window === "undefined" ? false : window.matchMedia("(min-width: 1024px)").matches,
  );
  const [accessArea, setAccessArea] = useState<AccessArea>(() =>
    typeof window === "undefined"
      ? "lojista"
      : window.location.pathname.startsWith("/dashboard/area-cliente")
        ? "cliente"
        : getStoredAccessArea(),
  );
  const pathname = usePathname();
  const router = useRouter();
  const { hasAnyPermission, isLoadingAccess, isLoadingStores, selectedStoreId, stores } = useStoreContext();
  const effectiveAccessArea: AccessArea = pathname.startsWith("/dashboard/area-cliente")
    ? "cliente"
    : accessArea;

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
    if (effectiveAccessArea === "cliente") {
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
  }, [effectiveAccessArea, allowedRoutes, hasAnyPermission, isLoadingAccess, pathname, router, selectedStoreId]);

  useEffect(() => {
    if (pathname.startsWith("/dashboard/area-cliente")) {
      persistAccessArea("cliente");
      return;
    }

    if (effectiveAccessArea === "lojista" && pathname.startsWith("/dashboard/area-cliente")) {
      router.replace(getDashboardRouteForArea(effectiveAccessArea));
    }
  }, [effectiveAccessArea, pathname, router]);

  useEffect(() => {
    if (
      effectiveAccessArea !== "lojista"
      || pathname.startsWith("/dashboard/area-cliente")
      || isLoadingStores
      || stores.length > 0
    ) {
      return;
    }

    const token = getAuthToken();

    if (!token) {
      return;
    }

    const authToken = token;
    let cancelled = false;

    async function redirectBlockedStoreOwner() {
      const response = await getLicenseStatus(authToken);

      if (cancelled || !response.ok) {
        return;
      }

      const status = response.body?.status;

      if (status === "pendente") {
        router.replace("/entrar-em-contato");
      }

      if (status === "expirada") {
        router.replace("/permissao-expirada");
      }
    }

    void redirectBlockedStoreOwner();

    return () => {
      cancelled = true;
    };
  }, [effectiveAccessArea, isLoadingStores, pathname, router, stores.length]);

  useEffect(() => {
    const desktopQuery = window.matchMedia("(min-width: 1024px)");
    const updateViewport = () => setIsDesktopViewport(desktopQuery.matches);

    updateViewport();
    desktopQuery.addEventListener("change", updateViewport);

    return () => {
      desktopQuery.removeEventListener("change", updateViewport);
    };
  }, []);

  const isChromeExpanded = isDesktopViewport ? isDesktopChromeVisible : isMobileChromeOpen;

  function toggleChrome() {
    if (isDesktopViewport) {
      setIsDesktopChromeVisible((current) => !current);
      return;
    }

    setIsMobileChromeOpen((current) => !current);
  }

  return (
    <div className="h-[100dvh] overflow-hidden bg-[var(--background)] px-4 pt-[calc(env(safe-area-inset-top,0px)+1rem)] pb-[calc(env(safe-area-inset-bottom,0px)+1rem)] lg:h-screen lg:p-6">
      <div className="relative mx-auto flex h-full w-full max-w-[1600px] overflow-hidden rounded-[28px] border border-[var(--border)] bg-[var(--surface)] shadow-[var(--shadow-soft)] transition-all duration-300">
        <button
          type="button"
          onClick={toggleChrome}
          aria-label={isChromeExpanded ? "Esconder header e menu lateral" : "Mostrar header e menu lateral"}
          aria-expanded={isChromeExpanded}
          className="absolute top-[max(0.5rem,env(safe-area-inset-top,0px))] left-2 z-50 flex h-11 w-11 items-center justify-center rounded-2xl border border-[var(--border)] bg-white/92 text-[var(--foreground)] shadow-[0_16px_36px_rgba(15,23,42,0.12)] backdrop-blur transition hover:border-[var(--border-strong)] hover:bg-white lg:top-2"
        >
          <svg aria-hidden="true" viewBox="0 0 24 24" className="h-5 w-5">
            <path
              d="M4 7h16M4 12h16M4 17h16"
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
          accessArea={effectiveAccessArea}
          isCollapsed={!isDesktopChromeVisible}
          isMobileOpen={isMobileChromeOpen}
          onNavigate={() => setIsMobileChromeOpen(false)}
        />
        <div className="flex min-h-0 min-w-0 flex-1 flex-col bg-[var(--surface-muted)]">
          <AppHeader
            accessArea={effectiveAccessArea}
            isCollapsed={!isDesktopChromeVisible}
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
