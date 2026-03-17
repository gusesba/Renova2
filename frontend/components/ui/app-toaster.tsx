"use client";

import { Toaster } from "sonner";

// Configuracao unica do Sonner para manter o mesmo padrao de notificacao no app.
export function AppToaster() {
  return (
    <Toaster
      closeButton
      duration={3600}
      position="top-right"
      richColors
      toastOptions={{
        style: {
          borderRadius: "18px",
        },
      }}
    />
  );
}
