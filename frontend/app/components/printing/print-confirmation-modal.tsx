"use client";

import type { PrintSettings, PrintTarget } from "@/lib/printing/types";

type PrintConfirmationModalProps = {
  description: string;
  isOpen: boolean;
  isPrinting?: boolean;
  onClose: () => void;
  onPreviewPdf: () => void;
  onPrint: () => void;
  settings: PrintSettings;
  target: PrintTarget;
  title: string;
};

export function PrintConfirmationModal({
  description,
  isOpen,
  isPrinting = false,
  onClose,
  onPreviewPdf,
  onPrint,
  settings,
  target,
  title,
}: PrintConfirmationModalProps) {
  if (!isOpen) {
    return null;
  }

  const printer = target === "label" ? settings.labelPrinter : settings.receiptPrinter;
  const printerLabel = printer ? `${printer.name} (${printer.language})` : "PDF";

  return (
    <div className="fixed inset-0 z-[220] flex items-center justify-center bg-slate-950/45 px-4 py-6">
      <div className="w-full max-w-xl rounded-[28px] border border-[var(--border)] bg-white p-6 shadow-[0_30px_80px_rgba(15,23,42,0.25)]">
        <p className="text-sm font-semibold uppercase tracking-[0.16em] text-[var(--muted)]">
          Impressao
        </p>
        <h2 className="mt-2 text-2xl font-semibold text-[var(--foreground)]">{title}</h2>
        <p className="mt-2 text-sm leading-6 text-[var(--muted)]">{description}</p>

        <div className="mt-5 rounded-2xl border border-[var(--border)] bg-[var(--surface-muted)] px-4 py-4">
          <p className="text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
            Destino
          </p>
          <p className="mt-2 text-sm font-semibold text-[var(--foreground)]">{printerLabel}</p>
          <p className="mt-1 text-sm text-[var(--muted)]">
            {printer
              ? "A impressao sera enviada pelo QZ Tray."
              : "Nenhuma impressora configurada; a acao de imprimir abre o PDF."}
          </p>
        </div>

        <div className="mt-6 grid gap-3 sm:grid-cols-3">
          <button
            type="button"
            onClick={onPrint}
            disabled={isPrinting}
            className="flex h-11 cursor-pointer items-center justify-center rounded-2xl bg-[var(--primary)] px-4 text-sm font-semibold text-white transition hover:brightness-105 disabled:cursor-not-allowed disabled:opacity-60"
          >
            {isPrinting ? "Enviando..." : "Imprimir"}
          </button>
          <button
            type="button"
            onClick={onPreviewPdf}
            className="flex h-11 cursor-pointer items-center justify-center rounded-2xl border border-[var(--border)] bg-white px-4 text-sm font-semibold text-[var(--foreground)] transition hover:border-[var(--border-strong)]"
          >
            Preview PDF
          </button>
          <button
            type="button"
            onClick={onClose}
            className="flex h-11 cursor-pointer items-center justify-center rounded-2xl border border-[var(--border)] bg-white px-4 text-sm font-semibold text-[var(--foreground)] transition hover:border-[var(--border-strong)]"
          >
            Cancelar
          </button>
        </div>
      </div>
    </div>
  );
}
