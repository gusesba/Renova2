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
  const [isChromeCollapsed, setIsChromeCollapsed] = useState(false);
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
      if (pathname !== "/dashboard/area-cliente") {
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
    if (accessArea === "lojista" && pathname === "/dashboard/area-cliente") {
      router.replace(getDashboardRouteForArea(accessArea));
    }
  }, [accessArea, pathname, router]);

  return (
    <div className="h-screen overflow-hidden bg-[var(--background)] p-4 lg:p-6">
      <div
        className={`relative mx-auto flex h-full w-full max-w-[1600px] overflow-hidden bg-[var(--surface)] transition-all duration-300 ${
          isChromeCollapsed
            ? "rounded-[16px] border border-[color:rgba(231,236,245,0.55)] shadow-[0_14px_30px_rgba(15,23,42,0.05)]"
            : "rounded-[28px] border border-[var(--border)] shadow-[var(--shadow-soft)]"
        }`}
      >
        <button
          type="button"
          onClick={() => setIsChromeCollapsed((current) => !current)}
          aria-label={isChromeCollapsed ? "Expandir header e sidebar" : "Contrair header e sidebar"}
          aria-pressed={isChromeCollapsed}
          className="absolute top-2 left-2 z-20 flex h-11 w-11 items-center justify-center rounded-2xl border border-[var(--border)] bg-white/92 text-[var(--foreground)] shadow-[0_16px_36px_rgba(15,23,42,0.12)] backdrop-blur transition hover:border-[var(--border-strong)] hover:bg-white lg:top-3 lg:left-3"
        >
          <span
            className={`transition-transform duration-300 ${isChromeCollapsed ? "rotate-180" : ""}`}
          >
            <svg aria-hidden="true" viewBox="0 0 24 24" className="h-5 w-5">
              <path
                d="M17 17L7 7m0 0h8.5M7 7v8.5"
                fill="none"
                stroke="currentColor"
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth="1.8"
              />
            </svg>
          </span>
        </button>

        <AppSidebar accessArea={accessArea} isCollapsed={isChromeCollapsed} />
        <div className="flex min-h-0 min-w-0 flex-1 flex-col bg-[var(--surface-muted)]">
          <AppHeader
            accessArea={accessArea}
            isCollapsed={isChromeCollapsed}
            onAccessAreaChange={setAccessArea}
          />
          <main
            className={`min-h-0 min-w-0 flex-1 overflow-x-hidden overflow-y-auto transition-[padding] duration-300 ${
              isChromeCollapsed ? "p-16 sm:p-16 lg:p-20" : "p-4 sm:p-6 lg:p-8"
            }`}
          >
            <div
              className={`mx-auto w-full min-w-0 ${
                isChromeCollapsed ? "max-w-full" : "max-w-[1250px]"
              }`}
            >
              {children}
            </div>
          </main>
        </div>
      </div>
    </div>
  );
}
