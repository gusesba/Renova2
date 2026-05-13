import type { Metadata } from "next";
import { Poppins } from "next/font/google";

import "./globals.css";

const siteUrl = "https://www.renovacuritiba.com.br";

const poppins = Poppins({
  subsets: ["latin"],
  weight: ["400", "500", "600", "700"],
});

export const metadata: Metadata = {
  metadataBase: new URL(siteUrl),
  applicationName: "Renova",
  title: {
    default: "Renova | Sistema para brecho consignado",
    template: "%s | Renova",
  },
  description:
    "Sistema para brecho consignado com gestao de pecas, clientes, movimentacoes, etiquetas, repasses e fechamento financeiro.",
  keywords: [
    "sistema para brecho consignado",
    "gestao de brecho consignado",
    "controle de pecas consignadas",
    "repasse para fornecedor",
    "impressao de etiquetas para brecho",
    "fechamento financeiro de brecho",
  ],
  alternates: {
    canonical: "/",
  },
  openGraph: {
    title: "Renova | Sistema para brecho consignado",
    description:
      "Controle pecas consignadas, clientes, repasses, etiquetas e fechamento financeiro em uma plataforma feita para brechos.",
    url: "/",
    siteName: "Renova",
    locale: "pt_BR",
    type: "website",
  },
  twitter: {
    card: "summary",
    title: "Renova | Sistema para brecho consignado",
    description:
      "Gestao de brecho consignado com controle de pecas, clientes, repasses, etiquetas e fechamento.",
  },
  robots: {
    index: true,
    follow: true,
  },
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="pt-BR" className="h-full antialiased">
      <body className={`${poppins.className} min-h-full flex flex-col`}>
        {children}
      </body>
    </html>
  );
}
