import type { ReactNode } from "react";

import { AppShell } from "@/app/components/layout/app-shell";
import { StoreProvider } from "@/app/dashboard/store-context";

type DashboardLayoutProps = {
  children: ReactNode;
};

export default function DashboardLayout({ children }: DashboardLayoutProps) {
  return (
    <StoreProvider>
      <AppShell>{children}</AppShell>
    </StoreProvider>
  );
}
