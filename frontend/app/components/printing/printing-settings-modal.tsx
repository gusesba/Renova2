"use client";

import { useEffect, useState } from "react";
import { toast } from "sonner";

import { Select } from "@/app/components/ui/select";
import { listQzPrinters, getQzStatus } from "@/lib/printing/qz-client";
import {
  createPrinterInfo,
  defaultPrintSettings,
  getStoredPrintSettings,
  persistPrintSettings,
  printerLanguageOptions,
} from "@/lib/printing/settings";
import type { PrinterInfo, PrinterLanguage, PrintSettings, PrintTarget } from "@/lib/printing/types";

type PrintingSettingsModalProps = {
  isOpen: boolean;
  onClose: () => void;
};

export function PrintingSettingsModal({ isOpen, onClose }: PrintingSettingsModalProps) {
  const [printers, setPrinters] = useState<string[]>([]);
  const [isLoadingPrinters, setIsLoadingPrinters] = useState(false);
  const [qzConnected, setQzConnected] = useState(false);
  const [settings, setSettings] = useState<PrintSettings>(() => getStoredPrintSettings());

  useEffect(() => {
    if (!isOpen) {
      return;
    }

    setSettings(getStoredPrintSettings());

    void getQzStatus().then(setQzConnected);
  }, [isOpen]);

  if (!isOpen) {
    return null;
  }

  async function refreshPrinters() {
    setIsLoadingPrinters(true);

    try {
      const nextPrinters = await listQzPrinters();
      setPrinters(nextPrinters);
      setQzConnected(true);
      toast.success(`${nextPrinters.length} impressora(s) encontrada(s).`);
    } catch (error) {
      setQzConnected(false);
      toast.error(
        error instanceof Error
          ? error.message
          : "Nao foi possivel conectar ao QZ Tray. Verifique se ele esta aberto.",
      );
    } finally {
      setIsLoadingPrinters(false);
    }
  }

  function handleSave() {
    persistPrintSettings(settings);
    toast.success("Configuracao de impressao salva.");
    onClose();
  }

  return (
    <div className="fixed inset-0 z-[220] flex items-center justify-center bg-slate-950/45 px-4 py-6">
      <div className="max-h-[90vh] w-full max-w-3xl overflow-y-auto rounded-[28px] border border-[var(--border)] bg-white p-6 shadow-[0_30px_80px_rgba(15,23,42,0.25)]">
        <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
          <div>
            <p className="text-sm font-semibold uppercase tracking-[0.16em] text-[var(--muted)]">
              Impressao
            </p>
            <h2 className="mt-2 text-2xl font-semibold text-[var(--foreground)]">
              Impressoras e linguagens
            </h2>
            <p className="mt-2 text-sm leading-6 text-[var(--muted)]">
              A selecao fica salva neste navegador e pode ser alterada a qualquer momento.
            </p>
          </div>
          <span
            className={`inline-flex rounded-full px-3 py-1 text-xs font-semibold ${
              qzConnected ? "bg-emerald-100 text-emerald-700" : "bg-amber-100 text-amber-800"
            }`}
          >
            {qzConnected ? "QZ conectado" : "QZ nao conectado"}
          </span>
        </div>

        <div className="mt-5 flex flex-wrap gap-3">
          <button
            type="button"
            onClick={refreshPrinters}
            disabled={isLoadingPrinters}
            className="flex h-11 cursor-pointer items-center justify-center rounded-2xl bg-[var(--primary)] px-4 text-sm font-semibold text-white transition hover:brightness-105 disabled:cursor-not-allowed disabled:opacity-60"
          >
            {isLoadingPrinters ? "Buscando..." : "Atualizar lista do QZ"}
          </button>
          <button
            type="button"
            onClick={() => {
              setSettings(defaultPrintSettings);
              setPrinters([]);
            }}
            className="flex h-11 cursor-pointer items-center justify-center rounded-2xl border border-[var(--border)] bg-white px-4 text-sm font-semibold text-[var(--foreground)] transition hover:border-[var(--border-strong)]"
          >
            Limpar selecao
          </button>
        </div>

        <div className="mt-6 grid gap-4 md:grid-cols-2">
          <PrinterSection
            printer={settings.labelPrinter}
            printers={printers}
            target="label"
            title="Etiqueta"
            onChange={(labelPrinter) => setSettings((current) => ({ ...current, labelPrinter }))}
          />
          <PrinterSection
            printer={settings.receiptPrinter}
            printers={printers}
            target="receipt"
            title="Nota"
            onChange={(receiptPrinter) => setSettings((current) => ({ ...current, receiptPrinter }))}
          />
        </div>

        <div className="mt-6 flex flex-col-reverse gap-3 sm:flex-row sm:justify-end">
          <button
            type="button"
            onClick={onClose}
            className="flex h-11 cursor-pointer items-center justify-center rounded-2xl border border-[var(--border)] bg-white px-5 text-sm font-semibold text-[var(--foreground)] transition hover:border-[var(--border-strong)]"
          >
            Cancelar
          </button>
          <button
            type="button"
            onClick={handleSave}
            className="flex h-11 cursor-pointer items-center justify-center rounded-2xl bg-[linear-gradient(90deg,_#ff8a3d,_#ff6b3d)] px-5 text-sm font-semibold text-white shadow-[0_16px_30px_rgba(255,107,61,0.24)] transition hover:brightness-105"
          >
            Salvar configuracao
          </button>
        </div>
      </div>
    </div>
  );
}

function PrinterSection({
  onChange,
  printer,
  printers,
  target,
  title,
}: {
  onChange: (printer: PrinterInfo | null) => void;
  printer: PrinterInfo | null;
  printers: string[];
  target: PrintTarget;
  title: string;
}) {
  const selectedPrinterName = printer?.name ?? "";

  return (
    <div className="rounded-[24px] border border-[var(--border)] bg-[var(--surface-muted)] p-4">
      <h3 className="text-lg font-semibold text-[var(--foreground)]">{title}</h3>
      <p className="mt-1 text-sm text-[var(--muted)]">
        {printer
          ? `Selecionada: ${printer.name}`
          : "Sem impressora selecionada. As acoes de imprimir abrem PDF."}
      </p>

      <div className="mt-4 space-y-4">
        <div className="space-y-2">
          <span className="text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
            Impressora
          </span>
          <div className="rounded-2xl border border-[var(--border)] bg-white px-4 py-3 text-sm">
            <Select
              ariaLabel={`Impressora de ${title}`}
              emptyLabel="Atualize a lista do QZ Tray"
              onChange={(value) =>
                onChange(value ? createPrinterInfo(value, target) : null)
              }
              options={[
                { label: "Usar PDF", value: "" },
                ...printers.map((name) => ({ label: name, value: name })),
              ]}
              value={selectedPrinterName}
            />
          </div>
        </div>

        <div className="space-y-2">
          <span className="text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
            Linguagem
          </span>
          <div className="rounded-2xl border border-[var(--border)] bg-white px-4 py-3 text-sm">
            <Select
              ariaLabel={`Linguagem de ${title}`}
              onChange={(value) =>
                onChange({
                  name: printer?.name ?? "",
                  language: value as PrinterLanguage,
                })
              }
              options={printerLanguageOptions}
              value={printer?.language ?? "PDF"}
            />
          </div>
        </div>
      </div>
    </div>
  );
}
