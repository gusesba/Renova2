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
  {
    href: "/dashboard",
    label: "Dashboard",
    meta: "Acesso",
    title: "Access Control",
    subtitle: "Usuarios, cargos e permissoes",
  },
  {
    href: "/stores",
    label: "Lojas",
    meta: "Estrutura",
    title: "Lojas e Estrutura",
    subtitle: "Cadastro da loja, configuracao e visao consolidada",
  },
];

// Monta o shell visual do grupo autenticado e controla o estado da sidebar.
export function SystemRouteFrame({ children }: SystemRouteFrameProps) {
  const pathname = usePathname();
  const { session, changeStore, logoutCurrentUser } = useSystemSession();
  const [sidebarCollapsed, setSidebarCollapsed] = useState(true);
  const currentItem =
    navigationItems.find((item) => pathname.startsWith(item.href)) ??
    navigationItems[0];

  return (
    <AppShell
      header={
        <AppHeader
          onChangeStore={changeStore}
          onLogout={logoutCurrentUser}
          onToggleSidebar={() => setSidebarCollapsed((current) => !current)}
          session={session}
          sidebarCollapsed={sidebarCollapsed}
          subtitle={currentItem.subtitle}
          title={currentItem.title}
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
