"use client";

import type { ProductListItem } from "@/lib/product";
import {
  downloadLabelsPdf,
  downloadReceiptPdf,
  openLabelsPdf,
  openReceiptPdf,
} from "@/lib/printing/pdf";
import { printLabelsWithQz, printReceiptWithQz } from "@/lib/printing/qz-client";
import { getStoredPrintSettings } from "@/lib/printing/settings";
import type { ReceiptPrintData } from "@/lib/printing/types";

export async function printLabels(products: ProductListItem[]) {
  const printer = getStoredPrintSettings().labelPrinter;

  if (!printer || printer.language === "PDF") {
    await openLabelsPdf(products);
    return;
  }

  await printLabelsWithQz(products, printer);
}

export function openLabelPdfPreview(products: ProductListItem[]) {
  return openLabelsPdf(products);
}

export function downloadLabelPdfFile(products: ProductListItem[]) {
  return downloadLabelsPdf(products);
}

export async function printReceipt(receipt: ReceiptPrintData) {
  const printer = getStoredPrintSettings().receiptPrinter;

  if (!printer || printer.language === "PDF") {
    await openReceiptPdf(receipt);
    return;
  }

  await printReceiptWithQz(receipt, printer);
}

export function openReceiptPdfPreview(receipt: ReceiptPrintData) {
  return openReceiptPdf(receipt);
}

export function downloadReceiptPdfFile(receipt: ReceiptPrintData) {
  return downloadReceiptPdf(receipt);
}
