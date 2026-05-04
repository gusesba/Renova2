import { formatMovementType } from "@/lib/movement";
import { formatDateValue, type ProductListItem } from "@/lib/product";
import type { ReceiptPrintData } from "@/lib/printing/types";

type LabelSlot = {
  barcodeX: string;
  renovaX: string;
  sizeX: string;
  textX: string;
};

const dplSlots: LabelSlot[] = [
  { renovaX: "0080", textX: "0110", sizeX: "0050", barcodeX: "0110" },
  { renovaX: "0215", textX: "0245", sizeX: "0185", barcodeX: "0245" },
  { renovaX: "0350", textX: "0380", sizeX: "0320", barcodeX: "0380" },
];

export function padProductId(etiqueta: number) {
  return String(etiqueta).padStart(9, "0");
}

export function chunkProductsForLabels(products: ProductListItem[]) {
  const chunks: ProductListItem[][] = [];

  for (let index = 0; index < products.length; index += 3) {
    chunks.push(products.slice(index, index + 3));
  }

  return chunks;
}

export function buildDplLabels(products: ProductListItem[]) {
  return chunkProductsForLabels(products).map((chunk) => [
    "\x02L\n",
    "A3\n",
    "D11\n",
    "H30\n",
    "Q1\n",
    ...chunk.flatMap((product, index) => buildDplLabel(product, dplSlots[index])),
    "E\n",
  ]);
}

export function buildZplLabels(products: ProductListItem[]) {
  return chunkProductsForLabels(products).map((chunk) => {
    const commands = ["^XA", "^PW840", "^LL480", "^CI28"];

    chunk.forEach((product, index) => {
      const x = 20 + index * 270;
      commands.push(
        `^FO${x + 45},35^A0N,28,28^FDRENOVA^FS`,
        `^FO${x + 35},78^A0N,22,22^FB220,2,0,L^FD${zplText(product.produto + product.cor)}^FS`,
        `^FO${x + 35},132^A0N,22,22^FD${zplText(product.marca)}^FS`,
        `^FO${x + 35},170^A0N,22,22^FD${zplText(formatDateValue(product.entrada))}^FS`,
        `^FO${x + 15},215^A0N,28,28^FD${zplText(product.tamanho)}^FS`,
        `^FO${x + 75},212^A0N,34,34^FDR$${formatRawPrice(product.preco)}^FS`,
        `^FO${x + 35},275^BY2,2,80^BCN,80,Y,N,N^FD${padProductId(product.etiqueta)}^FS`,
      );
    });

    commands.push("^XZ");

    return commands.join("\n");
  });
}

export function buildEplLabels(products: ProductListItem[]) {
  return chunkProductsForLabels(products).map((chunk) => {
    const commands = ["N", "q840", "Q480,24"];

    chunk.forEach((product, index) => {
      const x = 20 + index * 270;
      commands.push(
        `A${x + 45},30,0,3,1,1,N,"RENOVA"`,
        `A${x + 35},75,0,2,1,1,N,"${eplText(product.produto + product.cor)}"`,
        `A${x + 35},120,0,2,1,1,N,"${eplText(product.marca)}"`,
        `A${x + 35},160,0,2,1,1,N,"${eplText(formatDateValue(product.entrada))}"`,
        `A${x + 15},210,0,3,1,1,N,"${eplText(product.tamanho)}"`,
        `A${x + 75},210,0,3,1,1,N,"R$${formatRawPrice(product.preco)}"`,
        `B${x + 35},275,0,1,2,2,80,B,"${padProductId(product.etiqueta)}"`,
      );
    });

    commands.push("P1");

    return commands.join("\n");
  });
}

export function buildEscPosReceipt(receipt: ReceiptPrintData) {
  const total = receipt.products.reduce((sum, product) => sum + getReceiptProductPrice(product), 0);
  const productsData = receipt.products.flatMap((product) => [
    `${padRight(`${product.produto} ${product.cor} ${product.marca}`, 40)}${formatRawPrice(
      getReceiptProductPrice(product),
    )}\x1B\x74\x13\xAA`,
    "\x0A",
  ]);

  return [
    "\x1B\x40",
    "\x1B\x61\x31",
    "RENOVA\x0A",
    "\x0A",
    "@renova_sustentavel_curitiba\x0A",
    "\x0A",
    receipt.buyer,
    "\x0A",
    "\x0A",
    formatReceiptDate(receipt.printedAt),
    "\x0A",
    "\x0A",
    "\x0A",
    `Registo de ${formatMovementType(receipt.sellType)}\x0A`,
    "\x0A",
    "\x0A",
    "\x0A",
    "\x0A",
    "\x0A",
    "\x1B\x61\x30",
    ...productsData,
    "\x1B\x21\x0A\x1B\x45\x0A",
    "\x0A\x0A",
    "\x1B\x61\x30",
    "----------------------------------------------\x0A",
    `Total                                   ${formatRawPrice(total)}\x1B\x74\x13\xAA`,
    "\x1B\x61\x30",
    "\x0A\x0A\x0A\x0A\x0A\x0A\x0A",
    "\x1B\x69",
  ];
}

export function getReceiptProductPrice(product: { preco: number; desconto?: string }) {
  const discount = Number((product.desconto || "0").replace(",", "."));

  if (!Number.isFinite(discount) || discount <= 0) {
    return product.preco;
  }

  return Number((product.preco * Math.max(0, 1 - discount / 100)).toFixed(2));
}

export function formatReceiptDate(date: Date) {
  const day = String(date.getDate()).padStart(2, "0");
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const hours = String(date.getHours()).padStart(2, "0");
  const minutes = String(date.getMinutes()).padStart(2, "0");
  const seconds = String(date.getSeconds()).padStart(2, "0");

  return `${day}/${month}/${date.getFullYear()} ${hours}:${minutes}:${seconds}`;
}

function buildDplLabel(product: ProductListItem, slot: LabelSlot) {
  return [
    dplTextLine("0040", slot.renovaX, "RENOVA"),
    dplTextLine("0065", slot.textX, product.produto + product.cor),
    dplTextLine("0090", slot.textX, product.marca),
    dplTextLine("0115", slot.textX, formatDateValue(product.entrada)),
    dplTextLine("0140", slot.sizeX, product.tamanho),
    dplTextLine("0140", slot.textX, `R$${formatRawPrice(product.preco)}`, "3"),
    `3D520000200${slot.barcodeX}${padProductId(product.etiqueta)}\n`,
  ];
}

function dplTextLine(row: string, column: string, value: string, fontSize = "2") {
  return `3${fontSize}11000${row}${column}${sanitizeRawText(value)}\n`;
}

function formatRawPrice(value: number) {
  return Number(value).toFixed(2);
}

function padRight(value: string, size: number) {
  const text = sanitizeRawText(value);

  if (text.length >= size) {
    return text.slice(0, size);
  }

  return text.concat(" ".repeat(size - text.length));
}

function sanitizeRawText(value: string) {
  return value.normalize("NFD").replace(/[\u0300-\u036f]/g, "").replace(/[\r\n"]/g, " ").trim();
}

function zplText(value: string) {
  return sanitizeRawText(value).replace(/\^/g, " ").replace(/~/g, " ");
}

function eplText(value: string) {
  return sanitizeRawText(value);
}
