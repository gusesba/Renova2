"use client";

import {
  buildDplLabels,
  buildEscPosReceipt,
  buildEplLabels,
  buildZplLabels,
} from "@/lib/printing/raw";
import type { PrinterInfo, PrinterLanguage, ReceiptPrintData } from "@/lib/printing/types";
import type { ProductListItem } from "@/lib/product";

const apiBaseUrl = process.env.NEXT_PUBLIC_API_URL?.replace(/\/$/, "") ?? "http://localhost:5268";

let configuredSecurity = false;

export type QzDiagnostics = {
  certificateConfigured: boolean;
  privateKeyConfigured: boolean;
  certificateValid: boolean;
  privateKeyValid: boolean;
  keyMatchesCertificate: boolean;
  certificateSource: string;
  certificatePath: string | null;
  certificatePathExists: boolean | null;
  privateKeySource: string;
  privateKeyPath: string | null;
  privateKeyPathExists: boolean | null;
  messages: string[];
};

type QzModule = {
  configs: {
    create: (printerName: string, options?: Record<string, unknown>) => unknown;
  };
  print: (config: unknown, data: string[]) => Promise<void>;
  printers: {
    details: () => Promise<Array<string | { name?: unknown }>>;
    find: () => Promise<string | string[]>;
  };
  security: {
    setCertificatePromise: (
      callback: (resolve: (certificate: string) => void, reject: (error: unknown) => void) => void,
    ) => void;
    setSignatureAlgorithm: (algorithm: string) => void;
    setSignaturePromise: (
      callback: (
        toSign: string,
      ) => (resolve: (signature: string) => void, reject: (error: unknown) => void) => void,
    ) => void;
  };
  websocket: {
    connect: () => Promise<void>;
    isActive: () => boolean;
  };
};

export async function listQzPrinters() {
  const qz = await getConnectedQz();

  try {
    const details = await qz.printers.details();

    if (Array.isArray(details)) {
      return details
        .map((item) => (typeof item === "string" ? item : item?.name))
        .filter((name): name is string => typeof name === "string" && name.trim().length > 0);
    }
  } catch {
    // Some QZ versions/drivers do not expose details reliably; fall back to find().
  }

  const found = await qz.printers.find();
  return Array.isArray(found) ? found : [String(found)];
}

export async function getQzStatus() {
  if (typeof window === "undefined") {
    return false;
  }

  try {
    const qz = await importQz();
    return qz.websocket.isActive();
  } catch {
    return false;
  }
}

export async function getQzDiagnostics(): Promise<QzDiagnostics> {
  const response = await fetch(`${apiBaseUrl}/api/qz/diagnostics`, {
    headers: buildAuthHeaders(),
  });

  if (!response.ok) {
    throw new Error(await readResponseError(response, "Nao foi possivel validar a configuracao do QZ Tray."));
  }

  return response.json() as Promise<QzDiagnostics>;
}

export async function printLabelsWithQz(products: ProductListItem[], printer: PrinterInfo) {
  const language = normalizeLabelLanguage(printer.language);
  const payloads =
    language === "ZPL"
      ? buildZplLabels(products)
      : language === "EPL"
        ? buildEplLabels(products)
        : buildDplLabels(products);

  const qz = await getConnectedQz();
  const config = qz.configs.create(printer.name, {
    colorType: "blackwhite",
    size: { width: 10.5, height: 6 },
    units: "cm",
  });

  for (const payload of payloads) {
    await qz.print(config, Array.isArray(payload) ? payload : [payload]);
  }
}

export async function printReceiptWithQz(receipt: ReceiptPrintData, printer: PrinterInfo) {
  const qz = await getConnectedQz();
  const config = qz.configs.create(printer.name);

  await qz.print(config, buildEscPosReceipt(receipt));
}

async function getConnectedQz(): Promise<QzModule> {
  await assertQzDiagnosticsHealthy();

  const qz = await importQz();
  configureQzSecurity(qz);

  if (!qz.websocket.isActive()) {
    await qz.websocket.connect();
  }

  return qz;
}

async function assertQzDiagnosticsHealthy() {
  const diagnostics = await getQzDiagnostics();

  if (!isQzDiagnosticsHealthy(diagnostics)) {
    throw new Error(diagnostics.messages.join(" "));
  }
}

async function importQz(): Promise<QzModule> {
  const qzModule = await import("qz-tray");
  const loaded = "default" in qzModule ? qzModule.default : qzModule;

  return loaded as QzModule;
}

function configureQzSecurity(qz: QzModule) {
  if (configuredSecurity) {
    return;
  }

  qz.security.setCertificatePromise((resolve: (certificate: string) => void, reject: (error: unknown) => void) => {
    fetch(`${apiBaseUrl}/api/qz/certificate`, {
      headers: buildAuthHeaders(),
    })
      .then((response) => {
        if (!response.ok) {
          return readResponseError(response, "Certificado QZ Tray nao configurado no backend.").then((message) => {
            throw new Error(message);
          });
        }

        return response.text();
      })
      .then(resolve)
      .catch(reject);
  });

  qz.security.setSignatureAlgorithm("SHA512");
  qz.security.setSignaturePromise((toSign: string) => {
    return (resolve: (signature: string) => void, reject: (error: unknown) => void) => {
      fetch(`${apiBaseUrl}/api/qz/sign`, {
        method: "POST",
        headers: {
          ...buildAuthHeaders(),
          "Content-Type": "text/plain",
        },
        body: toSign,
      })
        .then((response) => {
          if (!response.ok) {
            return readResponseError(response, "Nao foi possivel assinar a chamada do QZ Tray.").then((message) => {
              throw new Error(message);
            });
          }

          return response.text();
        })
        .then(resolve)
        .catch(reject);
    };
  });

  configuredSecurity = true;
}

function buildAuthHeaders(): Record<string, string> {
  if (typeof window === "undefined") {
    return {};
  }

  const token = window.localStorage.getItem("renova.token");

  return token ? { Authorization: `Bearer ${token}` } : {};
}

async function readResponseError(response: Response, fallback: string) {
  const body = await response.text();
  const trimmedBody = body.trim();

  if (!trimmedBody) {
    return `${fallback} HTTP ${response.status}.`;
  }

  return `${trimmedBody} HTTP ${response.status}.`;
}

function isQzDiagnosticsHealthy(diagnostics: QzDiagnostics) {
  return (
    diagnostics.certificateConfigured &&
    diagnostics.privateKeyConfigured &&
    diagnostics.certificateValid &&
    diagnostics.privateKeyValid &&
    diagnostics.keyMatchesCertificate
  );
}

function normalizeLabelLanguage(language: PrinterLanguage) {
  return language === "ZPL" || language === "EPL" || language === "DPL" ? language : "DPL";
}
