"use client";

import { useState, type ReactNode } from "react";
import { usePathname } from "next/navigation";

import { AppHeader } from "@/components/layout/app-header";
import { AppShell } from "@/components/layout/app-shell";
import { AppSidebar } from "@/components/layout/app-sidebar";
import { useSystemSession } from "@/app/(system)/components/system-session-provider";

type SystemRouteFrameProps = {
  children: ReactNode;
};

const navigationItems = [
  { href: "/dashboard", label: "Dashboard", meta: "Acesso" },
];

// Monta o shell visual do grupo autenticado e controla o estado da sidebar.
export function SystemRouteFrame({ children }: SystemRouteFrameProps) {
  const pathname = usePathname();
  const { session, changeStore, logoutCurrentUser } = useSystemSession();
  const [sidebarCollapsed, setSidebarCollapsed] = useState(true);

  return (
    <AppShell
      header={
        <AppHeader
          onChangeStore={changeStore}
          onLogout={logoutCurrentUser}
          onToggleSidebar={() => setSidebarCollapsed((current) => !current)}
          session={session}
          sidebarCollapsed={sidebarCollapsed}
        />
      }
      sidebar={
        <AppSidebar
          activeHref={pathname}
          collapsed={sidebarCollapsed}
          items={navigationItems}
        />
      }
      sidebarCollapsed={sidebarCollapsed}
    >
      {children}
    </AppShell>
  );
}
