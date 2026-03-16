import type { ReactNode } from "react";

import { SystemRouteFrame } from "@/app/(system)/components/system-route-frame";
import { SystemSessionProvider } from "@/app/(system)/components/system-session-provider";

export default function SystemLayout({ children }: { children: ReactNode }) {
  return (
    <SystemSessionProvider>
      <SystemRouteFrame>{children}</SystemRouteFrame>
    </SystemSessionProvider>
  );
}
