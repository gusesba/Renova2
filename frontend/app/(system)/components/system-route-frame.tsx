"use client";

import { useState, type ReactNode } from "react";
import { usePathname } from "next/navigation";

import { AppHeader } from "@/components/layout/app-header";
import { AppShell } from "@/components/layout/app-shell";
import { AppSidebar } from "@/components/layout/app-sidebar";
import { AccessStateCard } from "@/components/ui/access-state-card";
import { useSystemSession } from "@/app/(system)/components/system-session-provider";
import {
  canAccessDashboardModule,
  canAccessStoresModule,
} from "@/lib/helpers/access-control";
import type { SessionContext } from "@/lib/services/access";

type SystemRouteFrameProps = {
  children: ReactNode;
};

type AccessPredicate = (session: SessionContext) => boolean;

type RouteItem = {
  href: string;
  label: string;
  meta: string;
  title: string;
  subtitle: string;
  canAccess: AccessPredicate;
  deniedTitle?: string;
  deniedSubtitle?: string;
  deniedMessage?: string;
};

// Normaliza a assinatura das funcoes que decidem se uma rota pode aparecer e renderizar.
function createAccessPredicate(check: AccessPredicate) {
  return check;
}

const navigationItems: RouteItem[] = [
  {
    href: "/dashboard",
    label: "Dashboard",
    meta: "Acesso",
    title: "Access Control",
    subtitle: "Usuarios, cargos e permissoes",
    canAccess: createAccessPredicate(canAccessDashboardModule),
    deniedTitle: "Modulo sem permissao",
    deniedSubtitle: "Sua conta nao possui acesso ao modulo administrativo de acesso.",
    deniedMessage:
      "Solicite um cargo com acesso a usuarios, vinculos ou cargos para visualizar esta pagina.",
  },
  {
    href: "/stores",
    label: "Lojas",
    meta: "Estrutura",
    title: "Lojas e Estrutura",
    subtitle: "Cadastro da loja e visao consolidada da estrutura",
    canAccess: createAccessPredicate(canAccessStoresModule),
    deniedTitle: "Gestao restrita",
    deniedSubtitle: "O modulo de lojas exige permissao especifica de gerenciamento.",
    deniedMessage:
      "Solicite a permissao de gerenciamento de lojas para acessar esta pagina.",
  },
];

const routeItems: RouteItem[] = [
  ...navigationItems,
  {
    href: "/profile",
    label: "Perfil",
    meta: "Conta",
    title: "Meu Perfil",
    subtitle: "Edicao dos dados do usuario autenticado",
    canAccess: createAccessPredicate(() => true),
  },
];

// Monta o shell visual do grupo autenticado e controla o estado da sidebar.
export function SystemRouteFrame({ children }: SystemRouteFrameProps) {
  const pathname = usePathname();
  const { session, changeStore, logoutCurrentUser } = useSystemSession();
  const [sidebarCollapsed, setSidebarCollapsed] = useState(true);
  const currentItem =
    routeItems.find((item) => pathname.startsWith(item.href)) ??
    routeItems[0];
  const visibleNavigationItems = navigationItems.filter((item) =>
    item.canAccess(session),
  );
  const canRenderCurrentRoute = currentItem.canAccess(session);

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
          items={visibleNavigationItems}
        />
      }
      sidebarCollapsed={sidebarCollapsed}
    >
      {canRenderCurrentRoute ? (
        children
      ) : (
        <AccessStateCard
          message={currentItem.deniedMessage ?? "Voce nao possui acesso a esta pagina."}
          subtitle={
            currentItem.deniedSubtitle ??
            "Sua sessao nao possui o cargo necessario para esta visualizacao."
          }
          title={currentItem.deniedTitle ?? "Acesso restrito"}
        />
      )}
    </AppShell>
  );
}
