"use client";

import { Toaster } from "sonner";

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
