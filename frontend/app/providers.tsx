"use client";

import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { useState, type ReactNode } from "react";

import { AppToaster } from "@/components/ui/app-toaster";

// Concentra os providers globais do app para manter o layout raiz limpo.
type AppProvidersProps = {
  children: ReactNode;
};

export function AppProviders({ children }: AppProvidersProps) {
  // O client e criado uma vez para preservar cache e estado entre renders.
  const [queryClient] = useState(
    () =>
      new QueryClient({
        defaultOptions: {
          queries: {
            retry: 1,
            refetchOnWindowFocus: false,
          },
        },
      })
  );

  return (
    <QueryClientProvider client={queryClient}>
      {children}
      {/* Toasts ficam globais para qualquer rota conseguir notificar o usuario. */}
      <AppToaster />
    </QueryClientProvider>
  );
}
