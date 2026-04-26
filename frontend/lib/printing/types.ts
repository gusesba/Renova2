import type { MovementTypeValue } from "@/lib/movement";
import type { ProductListItem } from "@/lib/product";

export type PrintTarget = "label" | "receipt";

export type PrinterLanguage = "DPL" | "ZPL" | "EPL" | "ESCPOS" | "PDF";

export type PrinterInfo = {
  name: string;
  language: PrinterLanguage;
};

export type PrintSettings = {
  labelPrinter: PrinterInfo | null;
  receiptPrinter: PrinterInfo | null;
};

export type ReceiptProduct = ProductListItem & {
  desconto?: string;
};

export type ReceiptPrintData = {
  buyer: string;
  movementId: number;
  products: ReceiptProduct[];
  printedAt: Date;
  sellType: MovementTypeValue;
};
