"use client";

import { useEffect, useRef, useState } from "react";
import { createPortal } from "react-dom";
import { toast } from "sonner";

import {
  extractStoreConfigApiMessage,
  initialStoreConfigValues,
  type StoreConfigFormValues,
} from "@/lib/store-config";
import { getAuthToken } from "@/lib/store";
import { asStoreConfigResponse, getStoreConfig, saveStoreConfig } from "@/services/store-config-service";

type StoreConfigModalProps = {
  isOpen: boolean;
  storeId: number | null;
  storeName: string | null;
  onClose: () => void;
};

export function StoreConfigModal({
  isOpen,
  storeId,
  storeName,
  onClose,
}: StoreConfigModalProps) {
  const [shouldRender, setShouldRender] = useState(isOpen);
  const [isVisible, setIsVisible] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [isMounted, setIsMounted] = useState(false);
  const [values, setValues] = useState<StoreConfigFormValues>(initialStoreConfigValues);
  const wasOpenRef = useRef(isOpen);

  useEffect(() => {
    setIsMounted(true);

    return () => {
      setIsMounted(false);
    };
  }, []);

  useEffect(() => {
    let animationFrame = 0;
    let visibilityFrame = 0;
    let closeTimeout = 0;

    if (isOpen) {
      animationFrame = window.requestAnimationFrame(() => {
        setShouldRender(true);
        visibilityFrame = window.requestAnimationFrame(() => {
          setIsVisible(true);
        });
      });
    } else if (shouldRender) {
      animationFrame = window.requestAnimationFrame(() => {
        setIsVisible(false);
      });

      closeTimeout = window.setTimeout(() => {
        setShouldRender(false);
      }, 220);
    }

    function handleEscape(event: KeyboardEvent) {
      if (event.key === "Escape" && !isSaving) {
        onClose();
      }
    }

    window.addEventListener("keydown", handleEscape);

    return () => {
      window.cancelAnimationFrame(animationFrame);
      window.cancelAnimationFrame(visibilityFrame);
      window.clearTimeout(closeTimeout);
      window.removeEventListener("keydown", handleEscape);
    };
  }, [isOpen, isSaving, onClose, shouldRender]);

  useEffect(() => {
    wasOpenRef.current = isOpen;
  }, [isOpen]);

  useEffect(() => {
    if (!isOpen || !storeId) {
      return;
    }

    const currentStoreId = storeId;

    const token = getAuthToken();

    if (!token) {
      toast.error("Voce precisa estar autenticado para alterar a configuracao da loja.");
      onClose();
      return;
    }

    const currentToken = token;
    let isMounted = true;

    async function loadConfig() {
      setIsLoading(true);

      try {
        const response = await getStoreConfig(currentStoreId, currentToken);

        if (!isMounted) {
          return;
        }

        if (response.status === 404) {
          setValues(initialStoreConfigValues);
          return;
        }

        if (!response.ok) {
          toast.error(
            extractStoreConfigApiMessage(response.body) ??
              "Nao foi possivel carregar a configuracao da loja.",
          );
          onClose();
          return;
        }

        const config = asStoreConfigResponse(response.body);

        setValues({
          percentualRepasseFornecedor: String(config.percentualRepasseFornecedor),
          percentualRepasseVendedorCredito: String(config.percentualRepasseVendedorCredito),
          tempoPermanenciaProdutoMeses: String(config.tempoPermanenciaProdutoMeses),
        });
      } catch {
        if (isMounted) {
          toast.error("Nao foi possivel conectar ao backend. Verifique se a API esta em execucao.");
          onClose();
        }
      } finally {
        if (isMounted) {
          setIsLoading(false);
        }
      }
    }

    void loadConfig();

    return () => {
      isMounted = false;
    };
  }, [isOpen, onClose, storeId]);

  if (!shouldRender || !isMounted) {
    return null;
  }

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (isSaving || !storeId) {
      return;
    }

    const token = getAuthToken();

    if (!token) {
      toast.error("Voce precisa estar autenticado para alterar a configuracao da loja.");
      return;
    }

    const normalizedValue = values.percentualRepasseFornecedor.replace(",", ".").trim();
    const parsedValue = Number(normalizedValue);
    const normalizedCreditValue = values.percentualRepasseVendedorCredito.replace(",", ".").trim();
    const parsedCreditValue = Number(normalizedCreditValue);
    const normalizedStayValue = values.tempoPermanenciaProdutoMeses.trim();
    const parsedStayValue = Number(normalizedStayValue);

    if (!normalizedValue || Number.isNaN(parsedValue) || parsedValue < 0 || parsedValue > 100) {
      toast.error("Informe um percentual de repasse valido entre 0 e 100.");
      return;
    }

    if (
      !normalizedCreditValue ||
      Number.isNaN(parsedCreditValue) ||
      parsedCreditValue < 0 ||
      parsedCreditValue > 100
    ) {
      toast.error("Informe um percentual de repasse ao vendedor em credito valido entre 0 e 100.");
      return;
    }

    if (parsedCreditValue < parsedValue) {
      toast.error("O repasse ao vendedor em credito deve ser maior ou igual ao repasse normal.");
      return;
    }

    if (
      !normalizedStayValue ||
      !Number.isInteger(parsedStayValue) ||
      parsedStayValue < 1
    ) {
      toast.error("Informe um tempo de permanencia valido em meses, com valor inteiro maior ou igual a 1.");
      return;
    }

    setIsSaving(true);

    try {
      const response = await saveStoreConfig(
        {
          lojaId: storeId,
          percentualRepasseFornecedor: parsedValue,
          percentualRepasseVendedorCredito: parsedCreditValue,
          tempoPermanenciaProdutoMeses: parsedStayValue,
        },
        token,
      );

      if (!response.ok) {
        toast.error(
          extractStoreConfigApiMessage(response.body) ??
            "Nao foi possivel salvar a configuracao da loja.",
        );
        return;
      }

      const config = asStoreConfigResponse(response.body);

      setValues({
        percentualRepasseFornecedor: String(config.percentualRepasseFornecedor),
        percentualRepasseVendedorCredito: String(config.percentualRepasseVendedorCredito),
        tempoPermanenciaProdutoMeses: String(config.tempoPermanenciaProdutoMeses),
      });
      toast.success("Configuracao da loja atualizada.");
      onClose();
    } catch {
      toast.error("Nao foi possivel conectar ao backend. Verifique se a API esta em execucao.");
    } finally {
      setIsSaving(false);
    }
  }

  return createPortal(
    <div
      className={`fixed inset-0 z-[200] flex items-center justify-center bg-[rgba(15,23,42,0.45)] p-4 transition-opacity duration-200 ease-out ${
        isVisible ? "opacity-100" : "opacity-0"
      }`}
    >
      <div
        className={`w-full max-w-xl rounded-[28px] border border-[var(--border)] bg-white p-6 shadow-[0_30px_90px_rgba(15,23,42,0.22)] transition duration-250 ease-out ${
          isVisible ? "translate-y-0 scale-100 opacity-100" : "translate-y-4 scale-[0.98] opacity-0"
        }`}
      >
        <div className="flex items-start justify-between gap-4">
          <div>
            <p className="text-sm font-semibold uppercase tracking-[0.16em] text-[var(--muted)]">
              Configuracoes da loja
            </p>
            <h2 className="mt-2 text-2xl font-semibold tracking-tight text-[var(--foreground)]">
              Repasse ao fornecedor
            </h2>
            <p className="mt-2 text-sm leading-7 text-[var(--muted)]">
              Defina o percentual usado para gerar a ordem de pagamento do fornecedor
              {storeName ? ` na loja ${storeName}.` : "."}
            </p>
          </div>

          <button
            type="button"
            onClick={onClose}
            disabled={isSaving}
            className="flex h-11 w-11 cursor-pointer items-center justify-center rounded-2xl border border-[var(--border)] bg-[var(--surface-muted)] text-[var(--muted)] transition hover:border-[var(--border-strong)] hover:text-[var(--foreground)] disabled:cursor-not-allowed disabled:opacity-60"
            aria-label="Fechar configuracoes"
          >
            x
          </button>
        </div>

        <form className="mt-6 space-y-6" onSubmit={handleSubmit}>
          <label className="block space-y-2">
            <span className="text-sm font-semibold text-[var(--foreground)]">
              Percentual de repasse
            </span>
            <div className="relative">
              <input
                type="number"
                min={0}
                max={100}
                step="0.01"
                value={values.percentualRepasseFornecedor}
                disabled={isLoading || isSaving}
                onChange={(event) => {
                  setValues((current) => ({
                    ...current,
                    percentualRepasseFornecedor: event.target.value,
                  }));
                }}
                className="h-12 w-full rounded-2xl border border-[var(--border)] bg-white px-4 pr-12 text-sm text-[var(--foreground)] outline-none transition focus:border-[var(--primary)] focus:shadow-[0_0_0_4px_rgba(106,92,255,0.12)] disabled:cursor-not-allowed disabled:bg-[var(--surface-muted)]"
                placeholder="Ex.: 45"
              />
              <span className="pointer-events-none absolute right-4 top-1/2 -translate-y-1/2 text-sm font-semibold text-[var(--muted)]">
                %
              </span>
            </div>
            <p className="text-sm text-[var(--muted)]">
              Use valores entre 0 e 100. Exemplo: 45 significa 45% de repasse ao fornecedor.
            </p>
          </label>

          <label className="block space-y-2">
            <span className="text-sm font-semibold text-[var(--foreground)]">
              Repasse ao vendedor em credito
            </span>
            <div className="relative">
              <input
                type="number"
                min={0}
                max={100}
                step="0.01"
                value={values.percentualRepasseVendedorCredito}
                disabled={isLoading || isSaving}
                onChange={(event) => {
                  setValues((current) => ({
                    ...current,
                    percentualRepasseVendedorCredito: event.target.value,
                  }));
                }}
                className="h-12 w-full rounded-2xl border border-[var(--border)] bg-white px-4 pr-12 text-sm text-[var(--foreground)] outline-none transition focus:border-[var(--primary)] focus:shadow-[0_0_0_4px_rgba(106,92,255,0.12)] disabled:cursor-not-allowed disabled:bg-[var(--surface-muted)]"
                placeholder="Ex.: 10"
              />
              <span className="pointer-events-none absolute right-4 top-1/2 -translate-y-1/2 text-sm font-semibold text-[var(--muted)]">
                %
              </span>
            </div>
            <p className="text-sm text-[var(--muted)]">
              Percentual aplicado quando o vendedor usa o valor pendente em compras na propria loja.
            </p>
          </label>

          <label className="block space-y-2">
            <span className="text-sm font-semibold text-[var(--foreground)]">
              Permanencia do produto na loja
            </span>
            <div className="relative">
              <input
                type="number"
                min={1}
                step="1"
                value={values.tempoPermanenciaProdutoMeses}
                disabled={isLoading || isSaving}
                onChange={(event) => {
                  setValues((current) => ({
                    ...current,
                    tempoPermanenciaProdutoMeses: event.target.value,
                  }));
                }}
                className="h-12 w-full rounded-2xl border border-[var(--border)] bg-white px-4 pr-20 text-sm text-[var(--foreground)] outline-none transition focus:border-[var(--primary)] focus:shadow-[0_0_0_4px_rgba(106,92,255,0.12)] disabled:cursor-not-allowed disabled:bg-[var(--surface-muted)]"
                placeholder="Ex.: 6"
              />
              <span className="pointer-events-none absolute right-4 top-1/2 -translate-y-1/2 text-sm font-semibold text-[var(--muted)]">
                meses
              </span>
            </div>
            <p className="text-sm text-[var(--muted)]">
              Tempo padrao de permanencia do produto na loja, informado em meses inteiros.
            </p>
          </label>

          <div className="rounded-2xl border border-[var(--border)] bg-[var(--surface-muted)] px-4 py-4 text-sm text-[var(--muted)]">
            {isLoading
              ? "Carregando configuracao atual da loja..."
              : "Essa configuracao sera aplicada na criacao das ordens de pagamento da movimentacao."}
          </div>

          <div className="flex flex-col gap-3 sm:flex-row sm:justify-end">
            <button
              type="button"
              onClick={onClose}
              disabled={isSaving}
              className="flex h-12 cursor-pointer items-center justify-center rounded-2xl border border-[var(--border)] bg-[var(--surface-muted)] px-5 text-sm font-semibold text-[var(--foreground)] transition hover:border-[var(--border-strong)] hover:bg-white disabled:cursor-not-allowed disabled:opacity-60"
            >
              Cancelar
            </button>
            <button
              type="submit"
              disabled={isLoading || isSaving}
              className="flex h-12 cursor-pointer items-center justify-center rounded-2xl bg-[linear-gradient(90deg,_#ff8a3d,_#ff6b3d)] px-5 text-sm font-semibold text-white shadow-[0_16px_30px_rgba(255,107,61,0.28)] transition hover:brightness-105 disabled:cursor-not-allowed disabled:opacity-60"
            >
              {isSaving ? "Salvando..." : "Salvar configuracoes"}
            </button>
          </div>
        </form>
      </div>
    </div>,
    document.body,
  );
}
