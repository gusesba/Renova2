"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { type AccessArea } from "@/lib/access-area";
import { menuPermissionGroups } from "@/lib/access";
import { useStoreContext } from "@/app/dashboard/store-context";

type NavItem = {
  label: string;
  href: string;
  exact?: boolean;
};

const primaryItems: NavItem[] = [
  { label: "Lojas", href: "/dashboard/loja" },
  { label: "Controle de acesso", href: "/dashboard/controle-acesso" },
  { label: "Clientes", href: "/dashboard/cliente" },
  { label: "Produtos", href: "/dashboard/produto" },
  { label: "Solicitacoes", href: "/dashboard/solicitacao" },
  { label: "Movimentacoes", href: "/dashboard/movimentacao" },
  { label: "Pagamentos", href: "/dashboard/pagamento" },
  { label: "Gastos da loja", href: "/dashboard/gasto-loja" },
  { label: "Pag. externos", href: "/dashboard/pagamento-externo" },
  { label: "Pendencias", href: "/dashboard/pendencia" },
  { label: "Resumo", href: "/dashboard/fechamento" },
];

function HexagonMark() {
  return (
    <div className="relative h-11 w-11 shrink-0">
      <div className="absolute inset-0 rotate-45 rounded-[18px] bg-[linear-gradient(135deg,_#6a5cff,_#8b7dff)] shadow-[0_18px_30px_rgba(106,92,255,0.25)]" />
      <div className="absolute inset-[7px] rotate-45 rounded-[12px] border border-white/70 bg-white/20 backdrop-blur-sm" />
      <div className="absolute inset-0 flex items-center justify-center text-sm font-bold text-white">
        R
      </div>
    </div>
  );
}

function isActivePath(pathname: string, href: string, exact = false) {
  if (exact) {
    return pathname === href;
  }

  return pathname === href || pathname.startsWith(`${href}/`);
}

function SidebarLink({
  item,
  pathname,
  onNavigate,
}: {
  item: NavItem;
  pathname: string;
  onNavigate?: () => void;
}) {
  const active = isActivePath(pathname, item.href, item.exact);

  const activeClass = active
    ? "border-[var(--primary)] bg-[var(--primary-soft)] text-[var(--primary)] shadow-[0_16px_24px_rgba(106,92,255,0.14)]"
    : "border-transparent text-[var(--muted)] hover:border-[var(--border)] hover:bg-white hover:text-[var(--foreground)]";

  const iconClass = active
    ? "border-[color:color-mix(in_srgb,var(--primary)_18%,white)] bg-white text-[var(--primary)]"
    : "border-[var(--border)] bg-white text-[var(--muted)]";

  return (
    <Link
      href={item.href}
      onClick={onNavigate}
      className={`flex items-center gap-3 rounded-2xl border px-3 py-3 text-sm font-medium transition ${activeClass}`}
    >
      <span className={`flex h-10 w-10 items-center justify-center rounded-xl border ${iconClass}`}>
        <span className="h-2.5 w-2.5 rounded-full bg-current" />
      </span>
      <span>{item.label}</span>
    </Link>
  );
}

type AppSidebarProps = {
  accessArea: AccessArea;
  isCollapsed?: boolean;
  isMobileOpen?: boolean;
  onNavigate?: () => void;
};

export function AppSidebar({
  accessArea,
  isCollapsed = false,
  isMobileOpen = false,
  onNavigate,
}: AppSidebarProps) {
  const pathname = usePathname();
  const { hasAnyPermission, selectedStoreId } = useStoreContext();
  const visibleItems = accessArea === "cliente"
    ? [
        { label: "Pecas como fornecedor", href: "/dashboard/area-cliente", exact: true },
        { label: "Pecas como cliente", href: "/dashboard/area-cliente/como-cliente" },
        { label: "Pendencias", href: "/dashboard/area-cliente/pendencia" },
      ]
    : primaryItems.filter((item) => {
    const requiredPermissions = menuPermissionGroups[item.href];

    if (!requiredPermissions || !selectedStoreId) {
      return item.href === "/dashboard/loja";
    }

    return hasAnyPermission(requiredPermissions);
  });

  return (
    <aside
      className={`fixed top-[104px] bottom-4 left-4 z-40 flex w-[290px] max-w-[calc(100vw-2rem)] shrink-0 flex-col overflow-y-auto rounded-[24px] border border-[var(--border)] bg-[var(--surface)] px-5 py-6 shadow-[0_24px_60px_rgba(15,23,42,0.22)] transition-all duration-300 lg:static lg:top-auto lg:right-auto lg:bottom-auto lg:left-auto lg:z-auto lg:max-w-none lg:rounded-none lg:border-r lg:border-t-0 lg:border-b-0 lg:border-l-0 lg:shadow-none ${
        isMobileOpen ? "translate-x-0" : "-translate-x-[110%]"
      } lg:translate-x-0 ${
        isCollapsed
          ? "lg:w-0 lg:border-r-0 lg:px-0 lg:py-0 lg:opacity-0"
          : "lg:w-[290px] lg:opacity-100"
      }`}
      aria-hidden={isCollapsed && !isMobileOpen}
    >
      <div className="flex items-center gap-3 px-2">
        <HexagonMark />
        <div>
          <p className="text-lg font-semibold tracking-tight text-[var(--foreground)]">Renova</p>
          <p className="text-sm text-[var(--muted)]">
            {accessArea === "cliente" ? "Area do cliente" : "Painel principal"}
          </p>
        </div>
      </div>

      <nav className="mt-10 flex flex-col gap-2">
        {visibleItems.map((item) => (
          <SidebarLink
            key={item.label}
            item={item}
            pathname={pathname}
            onNavigate={onNavigate}
          />
        ))}
      </nav>
    </aside>
  );
}
