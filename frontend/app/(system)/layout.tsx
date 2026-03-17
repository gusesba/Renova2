import type { ReactNode } from "react";

import { SystemRouteFrame } from "@/app/(system)/components/system-route-frame";
import { SystemSessionProvider } from "@/app/(system)/components/system-session-provider";

// Toda rota autenticada passa por sessao + shell antes de renderizar o conteudo.
export default function SystemLayout({ children }: { children: ReactNode }) {
  return (
    <SystemSessionProvider>
      <SystemRouteFrame>{children}</SystemRouteFrame>
    </SystemSessionProvider>
  );
}
