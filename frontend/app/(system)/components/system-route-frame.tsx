"use client";

import { useState, type ReactNode } from "react";
import { usePathname } from "next/navigation";

import { AppHeader } from "@/components/layout/app-header";
import { AppShell } from "@/components/layout/app-shell";
import { AppSidebar } from "@/components/layout/app-sidebar";
import { AccessStateCard } from "@/components/ui/access-state-card";
import { useSystemSession } from "@/app/(system)/components/system-session-provider";
import {
  canAccessCatalogsModule,
  canAccessClosingsModule,
  canAccessConsignmentsModule,
  canAccessCommercialRulesModule,
  canAccessCreditsModule,
  canAccessDashboardModule,
  canAccessIndicatorsModule,
  canAccessFinancialModule,
  canAccessPeopleModule,
  canAccessPiecesModule,
  canAccessReportsModule,
  canAccessSalesModule,
  canAccessStockMovementsModule,
  canAccessStoresModule,
  canAccessSupplierPaymentsModule,
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
  {
    href: "/catalogs",
    label: "Catalogos",
    meta: "Base",
    title: "Cadastros Auxiliares",
    subtitle: "Produtos, marcas, cores e tamanhos da loja ativa",
    canAccess: createAccessPredicate(canAccessCatalogsModule),
    deniedTitle: "Modulo sem permissao",
    deniedSubtitle: "Sua conta nao possui acesso ao modulo de catalogos auxiliares.",
    deniedMessage:
      "Solicite a permissao de gerenciamento de catalogo para visualizar esta pagina.",
  },
  {
    href: "/commercial-rules",
    label: "Comercial",
    meta: "Regras",
    title: "Regras Comerciais",
    subtitle: "Repasse, desconto, pagamento misto e meios de pagamento",
    canAccess: createAccessPredicate(canAccessCommercialRulesModule),
    deniedTitle: "Modulo sem permissao",
    deniedSubtitle: "Sua conta nao possui acesso ao modulo de regras comerciais.",
    deniedMessage:
      "Solicite a permissao de gerenciamento comercial para visualizar esta pagina.",
  },
  {
    href: "/people",
    label: "Pessoas",
    meta: "Clientes",
    title: "Clientes e Fornecedores",
    subtitle: "Cadastro mestre, vinculos por loja e visao financeira",
    canAccess: createAccessPredicate(canAccessPeopleModule),
    deniedTitle: "Modulo sem permissao",
    deniedSubtitle: "Sua conta nao possui acesso ao modulo de clientes e fornecedores.",
    deniedMessage:
      "Solicite um cargo com permissao para visualizar ou gerenciar pessoas na loja ativa.",
  },
  {
    href: "/consignments",
    label: "Consignacao",
    meta: "Ciclo",
    title: "Ciclo de Vida da Consignacao",
    subtitle: "Prazo, desconto automatico, alertas e encerramento operacional",
    canAccess: createAccessPredicate(canAccessConsignmentsModule),
    deniedTitle: "Modulo sem permissao",
    deniedSubtitle: "Sua conta nao possui acesso ao modulo de consignacao.",
    deniedMessage:
      "Solicite permissao de visualizacao ou ajuste de pecas para acessar esta pagina.",
  },
  {
    href: "/sales",
    label: "Vendas",
    meta: "Comercial",
    title: "Vendas",
    subtitle: "Conclusao de venda, pagamentos, recibo e cancelamento",
    canAccess: createAccessPredicate(canAccessSalesModule),
    deniedTitle: "Modulo sem permissao",
    deniedSubtitle: "Sua conta nao possui acesso ao modulo de vendas.",
    deniedMessage:
      "Solicite permissao para registrar ou cancelar vendas na loja ativa.",
  },
  {
    href: "/credits",
    label: "Credito",
    meta: "Loja",
    title: "Credito da Loja",
    subtitle: "Contas por pessoa, saldo, extrato e lancamentos manuais",
    canAccess: createAccessPredicate(canAccessCreditsModule),
    deniedTitle: "Modulo sem permissao",
    deniedSubtitle: "Sua conta nao possui acesso ao modulo de credito.",
    deniedMessage:
      "Solicite permissao para consultar ou gerenciar credito na loja ativa.",
  },
  {
    href: "/financial",
    label: "Financeiro",
    meta: "Financeiro",
    title: "Meios de Pagamento e Conciliacao Financeira",
    subtitle: "Livro razao, lancamentos avulsos, taxas e resumo diario",
    canAccess: createAccessPredicate(canAccessFinancialModule),
    deniedTitle: "Modulo sem permissao",
    deniedSubtitle: "Sua conta nao possui acesso ao modulo financeiro da loja ativa.",
    deniedMessage:
      "Solicite permissao financeira para consultar ou conciliar o livro razao da loja.",
  },
  {
    href: "/indicators",
    label: "Indicadores",
    meta: "Analitico",
    title: "Dashboards e Indicadores",
    subtitle: "Vendas, financeiro, consignacao, pendencias e rankings da loja",
    canAccess: createAccessPredicate(canAccessIndicatorsModule),
    deniedTitle: "Modulo sem permissao",
    deniedSubtitle: "Sua conta nao possui acesso ao modulo de dashboards e indicadores.",
    deniedMessage:
      "Solicite permissao comercial, financeira, de estoque ou fechamento para acessar esta pagina.",
  },
  {
    href: "/reports",
    label: "Relatorios",
    meta: "Exportacao",
    title: "Relatorios e Exportacoes",
    subtitle: "Estoque, vendas, financeiro, baixas e filtros salvos da loja",
    canAccess: createAccessPredicate(canAccessReportsModule),
    deniedTitle: "Modulo sem permissao",
    deniedSubtitle: "Sua conta nao possui acesso ao modulo de relatorios e exportacoes.",
    deniedMessage:
      "Solicite a permissao de exportar relatorios para acessar esta pagina.",
  },
  {
    href: "/closings",
    label: "Fechamentos",
    meta: "Resumo",
    title: "Fechamento do Cliente e Fornecedor",
    subtitle: "Geracao de snapshot, conferencia, liquidacao e exportacao",
    canAccess: createAccessPredicate(canAccessClosingsModule),
    deniedTitle: "Modulo sem permissao",
    deniedSubtitle: "Sua conta nao possui acesso ao modulo de fechamento.",
    deniedMessage:
      "Solicite permissao para gerar ou conferir fechamentos na loja ativa.",
  },
  {
    href: "/supplier-payments",
    label: "Repasses",
    meta: "Financeiro",
    title: "Pagamentos e Repasses",
    subtitle: "Obrigacoes do fornecedor, liquidacoes e comprovante de pagamento",
    canAccess: createAccessPredicate(canAccessSupplierPaymentsModule),
    deniedTitle: "Modulo sem permissao",
    deniedSubtitle: "Sua conta nao possui acesso ao modulo de pagamentos ao fornecedor.",
    deniedMessage:
      "Solicite permissao financeira para consultar ou liquidar repasses na loja ativa.",
  },
  {
    href: "/stock-movements",
    label: "Movimentos",
    meta: "Estoque",
    title: "Movimentacoes de Estoque",
    subtitle: "Historico operacional, busca de pecas e ajustes manuais",
    canAccess: createAccessPredicate(canAccessStockMovementsModule),
    deniedTitle: "Modulo sem permissao",
    deniedSubtitle:
      "Sua conta nao possui acesso ao modulo de movimentacoes de estoque.",
    deniedMessage:
      "Solicite permissao de visualizacao ou ajuste de pecas para acessar esta pagina.",
  },
  {
    href: "/pieces",
    label: "Pecas",
    meta: "Estoque",
    title: "Pecas e Estoque",
    subtitle: "Cadastro, consulta, imagens e entrada inicial de estoque",
    canAccess: createAccessPredicate(canAccessPiecesModule),
    deniedTitle: "Modulo sem permissao",
    deniedSubtitle: "Sua conta nao possui acesso ao modulo de pecas e estoque.",
    deniedMessage:
      "Solicite permissao de visualizacao ou cadastro de pecas para acessar esta pagina.",
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
