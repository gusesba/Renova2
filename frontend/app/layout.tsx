import type { ReactNode } from "react";

import { AppProviders } from "@/app/providers";
import "./globals.css";

// Layout raiz da aplicacao; carrega estilos globais e providers compartilhados.
export default function RootLayout({
  children,
}: Readonly<{
  children: ReactNode;
}>) {
  return (
    <html lang="pt-BR">
      <body>
        <AppProviders>{children}</AppProviders>
      </body>
    </html>
  );
}
