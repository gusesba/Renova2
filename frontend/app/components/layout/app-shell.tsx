import type { ReactNode } from "react";

import { AppHeader } from "./app-header";
import { AppSidebar } from "./app-sidebar";

type AppShellProps = {
  children: ReactNode;
};

export function AppShell({ children }: AppShellProps) {
  return (
    <div className="h-screen overflow-hidden bg-[var(--background)] p-4 lg:p-6">
      <div className="mx-auto flex h-full w-full max-w-[1600px] overflow-hidden rounded-[28px] border border-[var(--border)] bg-[var(--surface)] shadow-[var(--shadow-soft)]">
        <AppSidebar />
        <div className="flex min-h-0 flex-1 flex-col bg-[var(--surface-muted)]">
          <AppHeader />
          <main className="min-h-0 flex-1 overflow-y-auto p-4 sm:p-6 lg:p-8">{children}</main>
        </div>
      </div>
    </div>
  );
}
