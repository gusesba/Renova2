import Link from "next/link";

import { RenovaMark } from "@/components/brand/renova-mark";
import { cx } from "@/lib/helpers/classnames";

// Sidebar do sistema; recebe a navegacao pronta para continuar componente puro.
type SidebarItem = {
  href: string;
  label: string;
  meta: string;
};

type AppSidebarProps = {
  items: SidebarItem[];
  activeHref: string;
  collapsed?: boolean;
};

export function AppSidebar({
  items,
  activeHref,
  collapsed = false,
}: AppSidebarProps) {
  return (
    <>
      <RenovaMark compact={collapsed} subtitle="Plataforma operacional" />
      <div className="app-sidebar-section">
        {!collapsed ? <div className="app-sidebar-label">Navegacao</div> : null}
        {items.map((item) => (
          <Link
            key={item.href}
            className={cx(
              "app-nav-link",
              activeHref === item.href && "is-active",
            )}
            href={item.href}
            title={item.label}
          >
            <span>{collapsed ? item.label.slice(0, 1) : item.label}</span>
            {!collapsed ? (
              <span className="app-nav-meta">{item.meta}</span>
            ) : null}
          </Link>
        ))}
      </div>
    </>
  );
}
