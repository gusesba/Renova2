import type { ReactNode } from "react";

import { cx } from "@/lib/helpers/classnames";

// Estrutura base do ambiente autenticado: sidebar, header e area de conteudo.
type AppShellProps = {
  sidebar: ReactNode;
  header: ReactNode;
  children: ReactNode;
  sidebarCollapsed?: boolean;
};

export function AppShell({
  sidebar,
  header,
  children,
  sidebarCollapsed = false,
}: AppShellProps) {
  return (
    <div className="system-surface">
      <div className={cx("app-shell", sidebarCollapsed && "is-sidebar-collapsed")}>
        <aside className="app-sidebar">{sidebar}</aside>
        <div className="app-main">
          {header}
          <main className="app-content">{children}</main>
        </div>
      </div>
    </div>
  );
}
