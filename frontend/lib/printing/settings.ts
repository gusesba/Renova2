import type { PrinterInfo, PrinterLanguage, PrintSettings, PrintTarget } from "@/lib/printing/types";

export const printSettingsStorageKey = "renova.printSettings.v1";

export const printerLanguageOptions: Array<{ label: string; value: PrinterLanguage }> = [
  { label: "Argox / Datamax (DPL)", value: "DPL" },
  { label: "Zebra (ZPL)", value: "ZPL" },
  { label: "Eltron / Zebra EPL", value: "EPL" },
  { label: "Cupom ESC/POS", value: "ESCPOS" },
  { label: "PDF", value: "PDF" },
];

export const defaultPrintSettings: PrintSettings = {
  labelPrinter: null,
  receiptPrinter: null,
};

export function detectPrinterLanguage(printerName: string, target?: PrintTarget): PrinterLanguage {
  const normalized = printerName.toLowerCase();

  if (target === "receipt") {
    return "ESCPOS";
  }

  if (normalized.includes("argox") || normalized.includes("datamax")) {
    return "DPL";
  }

  if (
    normalized.includes("eltron") ||
    normalized.includes(" epl") ||
    normalized.includes("lp ") ||
    normalized.includes("tlp")
  ) {
    return "EPL";
  }

  if (normalized.includes("zebra") || normalized.includes("zdesigner")) {
    return "ZPL";
  }

  if (
    normalized.includes("epson") ||
    normalized.includes("tm-") ||
    normalized.includes("bematech") ||
    normalized.includes("elgin") ||
    normalized.includes("daruma")
  ) {
    return "ESCPOS";
  }

  return target === "label" ? "DPL" : "ESCPOS";
}

export function createPrinterInfo(name: string, target: PrintTarget): PrinterInfo {
  return {
    name,
    language: detectPrinterLanguage(name, target),
  };
}

export function getStoredPrintSettings(): PrintSettings {
  if (typeof window === "undefined") {
    return defaultPrintSettings;
  }

  const rawValue = window.localStorage.getItem(printSettingsStorageKey);

  if (!rawValue) {
    return defaultPrintSettings;
  }

  try {
    const parsed = JSON.parse(rawValue) as Partial<PrintSettings>;

    return {
      labelPrinter: normalizePrinterInfo(parsed.labelPrinter),
      receiptPrinter: normalizePrinterInfo(parsed.receiptPrinter),
    };
  } catch {
    return defaultPrintSettings;
  }
}

export function persistPrintSettings(settings: PrintSettings) {
  if (typeof window === "undefined") {
    return;
  }

  window.localStorage.setItem(printSettingsStorageKey, JSON.stringify(settings));
}

function normalizePrinterInfo(value: unknown): PrinterInfo | null {
  if (!value || typeof value !== "object") {
    return null;
  }

  const printer = value as Partial<PrinterInfo>;

  if (typeof printer.name !== "string" || !printer.name.trim()) {
    return null;
  }

  const language: PrinterLanguage = printerLanguageOptions.some(
    (option) => option.value === printer.language,
  )
    ? (printer.language as PrinterLanguage)
    : "PDF";

  return {
    name: printer.name,
    language,
  };
}
