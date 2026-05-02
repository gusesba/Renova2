"use client";

import { Document, Page, StyleSheet, Text, View, pdf } from "@react-pdf/renderer";

import { formatMovementType } from "@/lib/movement";
import { formatCurrencyValue, formatDateValue, type ProductListItem } from "@/lib/product";
import { formatReceiptDate, getReceiptProductPrice, padProductId } from "@/lib/printing/raw";
import type { ReceiptPrintData } from "@/lib/printing/types";

const labelStyles = StyleSheet.create({
  barcode: {
    fontFamily: "Courier",
    fontSize: 10,
    letterSpacing: 2,
    marginTop: 4,
    textAlign: "center",
  },
  brand: {
    fontSize: 8,
    marginTop: 4,
  },
  label: {
    borderRightColor: "#ddd",
    borderRightWidth: 1,
    height: "100%",
    paddingHorizontal: 8,
    paddingVertical: 10,
    width: "33.333%",
  },
  labelLast: {
    borderRightWidth: 0,
  },
  page: {
    backgroundColor: "#fff",
    flexDirection: "row",
    height: "6cm",
    padding: 4,
    width: "10.5cm",
  },
  price: {
    fontSize: 14,
    fontWeight: "bold",
  },
  product: {
    fontSize: 8,
    marginTop: 5,
  },
  row: {
    alignItems: "center",
    flexDirection: "row",
    justifyContent: "space-between",
    marginTop: 5,
  },
  title: {
    fontSize: 12,
    fontWeight: "bold",
    textAlign: "center",
  },
});

const receiptStyles = StyleSheet.create({
  center: {
    textAlign: "center",
  },
  divider: {
    borderTopColor: "#111",
    borderTopWidth: 1,
    marginVertical: 8,
  },
  item: {
    flexDirection: "row",
    fontSize: 8,
    gap: 8,
    justifyContent: "space-between",
    marginBottom: 4,
  },
  itemName: {
    flexGrow: 1,
    maxWidth: "72%",
  },
  page: {
    backgroundColor: "#fff",
    fontFamily: "Courier",
    padding: 12,
    width: "80mm",
  },
  sectionGap: {
    marginTop: 10,
  },
  text: {
    fontSize: 8,
  },
  title: {
    fontSize: 12,
    fontWeight: "bold",
    textAlign: "center",
  },
  total: {
    flexDirection: "row",
    fontSize: 10,
    fontWeight: "bold",
    justifyContent: "space-between",
  },
});

export async function openLabelsPdf(products: ProductListItem[]) {
  const blob = await createLabelsPdfBlob(products);
  openBlob(blob);
}

export async function downloadLabelsPdf(products: ProductListItem[]) {
  const blob = await createLabelsPdfBlob(products);
  downloadBlob(blob, `etiquetas-renova-${formatFileDate(new Date())}.pdf`);
}

export async function openReceiptPdf(receipt: ReceiptPrintData) {
  const blob = await createReceiptPdfBlob(receipt);
  openBlob(blob);
}

export async function downloadReceiptPdf(receipt: ReceiptPrintData) {
  const blob = await createReceiptPdfBlob(receipt);
  downloadBlob(blob, `nota-renova-movimentacao-${receipt.movementId}.pdf`);
}

async function createLabelsPdfBlob(products: ProductListItem[]) {
  return pdf(
    <Document title="Etiquetas Renova">
      {chunk(products, 3).map((pageProducts, pageIndex) => (
        <Page key={pageIndex} size={[297.64, 170.08]} style={labelStyles.page}>
          {[0, 1, 2].map((slot) => {
            const product = pageProducts[slot];

            return (
              <View
                key={slot}
                style={slot === 2 ? [labelStyles.label, labelStyles.labelLast] : labelStyles.label}
              >
                {product ? (
                  <>
                    <Text style={labelStyles.title}>RENOVA</Text>
                    <Text style={labelStyles.product}>{product.produto}{product.cor}</Text>
                    <Text style={labelStyles.brand}>{product.marca}</Text>
                    <Text style={labelStyles.brand}>{formatDateValue(product.entrada)}</Text>
                    <View style={labelStyles.row}>
                      <Text style={labelStyles.price}>{product.tamanho}</Text>
                      <Text style={labelStyles.price}>{formatCurrencyValue(product.preco)}</Text>
                    </View>
                    <Barcode value={padProductId(product.etiqueta)} />
                    <Text style={labelStyles.barcode}>{padProductId(product.etiqueta)}</Text>
                  </>
                ) : null}
              </View>
            );
          })}
        </Page>
      ))}
    </Document>,
  ).toBlob();
}

async function createReceiptPdfBlob(receipt: ReceiptPrintData) {
  const total = receipt.products.reduce((sum, product) => sum + getReceiptProductPrice(product), 0);

  return pdf(
    <Document title={`Nota Renova ${receipt.movementId}`}>
      <Page size={[226.77, Math.max(360, 230 + receipt.products.length * 24)]} style={receiptStyles.page}>
        <Text style={receiptStyles.title}>RENOVA</Text>
        <Text style={[receiptStyles.text, receiptStyles.center, receiptStyles.sectionGap]}>
          @renova_sustentavel_curitiba
        </Text>
        <Text style={[receiptStyles.text, receiptStyles.center, receiptStyles.sectionGap]}>
          {receipt.buyer}
        </Text>
        <Text style={[receiptStyles.text, receiptStyles.center, receiptStyles.sectionGap]}>
          {formatReceiptDate(receipt.printedAt)}
        </Text>
        <Text style={[receiptStyles.text, receiptStyles.center, receiptStyles.sectionGap]}>
          Registo de {formatMovementType(receipt.sellType)}
        </Text>
        <View style={receiptStyles.sectionGap}>
          {receipt.products.map((product) => (
            <View key={product.id} style={receiptStyles.item}>
              <Text style={receiptStyles.itemName}>
                {product.produto} {product.cor} {product.marca}
              </Text>
              <Text>{getReceiptProductPrice(product).toFixed(2)}</Text>
            </View>
          ))}
        </View>
        <View style={receiptStyles.divider} />
        <View style={receiptStyles.total}>
          <Text>Total</Text>
          <Text>{total.toFixed(2)}</Text>
        </View>
      </Page>
    </Document>,
  ).toBlob();
}

function Barcode({ value }: { value: string }) {
  return (
    <View style={{ flexDirection: "row", height: 34, justifyContent: "center", marginTop: 8 }}>
      {value.split("").flatMap((digit, digitIndex) => {
        const width = (Number(digit) % 3) + 1;

        return [
          <View
            key={`${digitIndex}-bar-a`}
            style={{ backgroundColor: "#111", height: 34, marginRight: 1, width }}
          />,
          <View
            key={`${digitIndex}-bar-b`}
            style={{ backgroundColor: "#111", height: 34, marginRight: 2, width: 1 }}
          />,
        ];
      })}
    </View>
  );
}

function chunk<T>(items: T[], size: number) {
  const chunks: T[][] = [];

  for (let index = 0; index < items.length; index += size) {
    chunks.push(items.slice(index, index + size));
  }

  return chunks;
}

function openBlob(blob: Blob) {
  const url = URL.createObjectURL(blob);
  window.open(url, "_blank", "noopener,noreferrer");
  window.setTimeout(() => URL.revokeObjectURL(url), 60_000);
}

function downloadBlob(blob: Blob, filename: string) {
  const url = URL.createObjectURL(blob);
  const anchor = document.createElement("a");
  anchor.href = url;
  anchor.download = filename;
  anchor.click();
  window.setTimeout(() => URL.revokeObjectURL(url), 60_000);
}

function formatFileDate(date: Date) {
  return `${date.getFullYear()}${String(date.getMonth() + 1).padStart(2, "0")}${String(
    date.getDate(),
  ).padStart(2, "0")}`;
}
