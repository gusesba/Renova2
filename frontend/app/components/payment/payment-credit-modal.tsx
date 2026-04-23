"use client";

import { useEffect, useMemo, useState } from "react";
import { createPortal } from "react-dom";
import { toast } from "sonner";

import { Select } from "@/app/components/ui/select";
import {
  asPaymentCreditResponse,
  calculateCustomerPaymentMoneyPreview,
  calculateSupplierMoneyPreview,
  formatCurrency,
  formatPaymentCreditType,
  getPaymentApiMessage,
  getTodayDateInputValue,
  paymentCreditTypeOptions,
  type PendingClientItem,
  type PaymentCreditTypeValue,
} from "@/lib/payment";
import { extractStoreConfigApiMessage, type ConfigLojaResponse } from "@/lib/store-config";
import { getAuthToken } from "@/lib/store";
import { createPaymentCredit } from "@/services/payment-service";
import { asStoreConfigResponse, getStoreConfig } from "@/services/store-config-service";

type PaymentCreditModalProps = {
  client: PendingClientItem | null;
  initialPaymentType?: PaymentCreditTypeValue;
  isOpen: boolean;
  storeId: number | null;
  storeName: string | null;
  onClose: () => void;
  onSuccess: () => Promise<void> | void;
};

export function PaymentCreditModal({
  client,
  initialPaymentType = 2,
  isOpen,
  storeId,
  storeName,
  onClose,
  onSuccess,
}: PaymentCreditModalProps) {
  const [shouldRender, setShouldRender] = useState(isOpen);
  const [isVisible, setIsVisible] = useState(false);
  const [dateValue, setDateValue] = useState(() => getTodayDateInputValue());
  const [paymentType, setPaymentType] = useState<PaymentCreditTypeValue>(2);
  const [creditValue, setCreditValue] = useState("");
  const [paymentMethodId, setPaymentMethodId] = useState("");
  const [config, setConfig] = useState<ConfigLojaResponse | null>(null);
  const [configMessage, setConfigMessage] = useState<string | null>(null);
  const [isLoadingConfig, setIsLoadingConfig] = useState(false);
  const [isSaving, setIsSaving] = useState(false);

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
    if (!isOpen) {
      return;
    }

    setDateValue(getTodayDateInputValue());
    setPaymentType(initialPaymentType);
    setCreditValue("");
    setPaymentMethodId("");
    setConfigMessage(null);
  }, [initialPaymentType, isOpen, client?.clienteId]);

  useEffect(() => {
    if (!isOpen || !storeId) {
      setConfig(null);
      return;
    }

    const currentStoreId = storeId;
    const token = getAuthToken();

    if (!token) {
      setConfig(null);
      setConfigMessage("Voce precisa estar autenticado para consultar a configuracao da loja.");
      return;
    }

    const currentToken = token;
    let active = true;

    async function loadConfig() {
      setIsLoadingConfig(true);

      try {
        const response = await getStoreConfig(currentStoreId, currentToken);

        if (!active) {
          return;
        }

        if (response.status === 404) {
          setConfig(null);
          setConfigMessage("A loja nao possui configuracao de repasse cadastrada.");
          return;
        }

        if (!response.ok) {
          setConfig(null);
          setConfigMessage(
            extractStoreConfigApiMessage(response.body) ??
              "Nao foi possivel carregar a configuracao da loja.",
          );
          return;
        }

        setConfig(asStoreConfigResponse(response.body));
        setConfigMessage(null);
      } catch {
        if (active) {
          setConfig(null);
          setConfigMessage("Nao foi possivel conectar ao backend para consultar a configuracao.");
        }
      } finally {
        if (active) {
          setIsLoadingConfig(false);
        }
      }
    }

    void loadConfig();

    return () => {
      active = false;
    };
  }, [isOpen, storeId]);

  const parsedCreditValue = useMemo(() => {
    const normalized = creditValue.replace(",", ".").trim();
    const parsed = Number(normalized);
    return Number.isFinite(parsed) && parsed > 0 ? parsed : 0;
  }, [creditValue]);

  useEffect(() => {
    if (!isOpen || paymentType !== 1) {
      return;
    }

    if (!config || config.formasPagamento.length === 0) {
      setPaymentMethodId("");
      return;
    }

    setPaymentMethodId((current) =>
      current && config.formasPagamento.some((item) => String(item.id ?? "") === current)
        ? current
        : String(config.formasPagamento[0]?.id ?? ""),
    );
  }, [config, isOpen, paymentType]);

  const currentCredit = client?.credito ?? 0;
  const nextCredit =
    paymentType === 1 ? currentCredit + parsedCreditValue : currentCredit - parsedCreditValue;
  const selectedPaymentMethod =
    paymentType === 1
      ? config?.formasPagamento.find((item) => String(item.id) === paymentMethodId) ?? null
      : null;
  const moneyPreview =
    paymentType === 2 && config
      ? calculateSupplierMoneyPreview(
          parsedCreditValue,
          config.percentualRepasseFornecedor,
          config.percentualRepasseVendedorCredito,
        )
      : paymentType === 1 && selectedPaymentMethod
        ? calculateCustomerPaymentMoneyPreview(
            parsedCreditValue,
            selectedPaymentMethod.percentualAjuste,
          )
      : parsedCreditValue;
  const isSupplierOperation = paymentType === 2;
  const isCustomerOperation = paymentType === 1;
  const hasEnoughCredit = !isSupplierOperation || parsedCreditValue <= currentCredit;
  const missingConfigForSupplier = isSupplierOperation && !config;
  const missingPaymentMethodForCustomer =
    isCustomerOperation && (!config || config.formasPagamento.length === 0 || !selectedPaymentMethod);
  const shouldBlockSubmit =
    (isSupplierOperation && (!hasEnoughCredit || missingConfigForSupplier)) ||
    missingPaymentMethodForCustomer;

  if (typeof document === "undefined" || !shouldRender || !client) {
    return null;
  }

  function toUtcStartOfDay(value: string) {
    return new Date(`${value}T00:00:00.000Z`).toISOString();
  }

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (isSaving || !storeId || !client) {
      return;
    }

    const currentClient = client;
    const token = getAuthToken();

    if (!token) {
      toast.error("Voce precisa estar autenticado para lancar um pagamento.");
      return;
    }

    if (!dateValue) {
      toast.error("Informe a data do pagamento.");
      return;
    }

    if (parsedCreditValue <= 0) {
      toast.error("Informe um valor de credito maior que zero.");
      return;
    }

    if (isSupplierOperation && (!hasEnoughCredit || nextCredit < 0)) {
      toast.error("O cliente nao possui credito suficiente para essa operacao.");
      return;
    }

    if (isSupplierOperation && missingConfigForSupplier) {
      toast.error("Configure o repasse da loja antes de pagar o fornecedor.");
      return;
    }

    if (isCustomerOperation && !selectedPaymentMethod) {
      toast.error("Selecione uma forma de pagamento para o lancamento.");
      return;
    }

    setIsSaving(true);

    try {
      const response = await createPaymentCredit(
        {
          lojaId: storeId,
          clienteId: currentClient.clienteId,
          tipo: paymentType,
          configLojaFormaPagamentoId: selectedPaymentMethod?.id,
          valorCredito: parsedCreditValue,
          data: toUtcStartOfDay(dateValue),
        },
        token,
      );

      if (!response.ok) {
        toast.error(
          getPaymentApiMessage(response.body) ?? "Nao foi possivel lancar o pagamento externo.",
        );
        return;
      }

      const result = asPaymentCreditResponse(response.body);

      await onSuccess();

      toast.success(
        `${formatPaymentCreditType(result.tipo)} registrado com ${formatCurrency(result.valorCredito)} em credito e ${formatCurrency(result.valorDinheiro)} em dinheiro.`,
      );
      onClose();
    } catch {
      toast.error("Nao foi possivel conectar ao backend. Verifique se a API esta em execucao.");
    } finally {
      setIsSaving(false);
    }
  }

  return createPortal(
    <div
      className={`fixed inset-0 z-[220] flex items-start justify-center overflow-y-auto bg-[rgba(15,23,42,0.45)] px-4 pt-[calc(env(safe-area-inset-top,0px)+1rem)] pb-[calc(env(safe-area-inset-bottom,0px)+1.5rem)] transition-opacity duration-200 ease-out sm:items-center sm:px-4 sm:pt-4 sm:pb-4 ${
        isVisible ? "opacity-100" : "opacity-0"
      }`}
    >
      <div
        className={`max-h-[calc(100dvh-env(safe-area-inset-top,0px)-env(safe-area-inset-bottom,0px)-2.5rem)] w-full max-w-[calc(100vw-2rem)] overflow-y-auto rounded-[28px] border border-[var(--border)] bg-white p-6 shadow-[0_30px_90px_rgba(15,23,42,0.22)] transition duration-250 ease-out sm:max-h-[calc(100vh-2rem)] sm:max-w-3xl ${
          isVisible ? "translate-y-0 scale-100 opacity-100" : "translate-y-4 scale-[0.98] opacity-0"
        }`}
      >
        <div className="flex items-start justify-between gap-4">
          <div>
            <p className="text-sm font-semibold uppercase tracking-[0.16em] text-[var(--muted)]">
              Pagamento externo
            </p>
            <h2 className="mt-2 text-2xl font-semibold tracking-tight text-[var(--foreground)]">
              Lancar pagamento para {client.nome}
            </h2>
            <p className="mt-2 text-sm leading-7 text-[var(--muted)]">
              Loja ativa: {storeName ?? "Nenhuma loja selecionada"}.
            </p>
          </div>

          <button
            type="button"
            onClick={onClose}
            disabled={isSaving}
            className="flex h-11 w-11 cursor-pointer items-center justify-center rounded-2xl border border-[var(--border)] bg-[var(--surface-muted)] text-[var(--muted)] transition hover:border-[var(--border-strong)] hover:text-[var(--foreground)] disabled:cursor-not-allowed disabled:opacity-60"
            aria-label="Fechar pagamento externo"
          >
            x
          </button>
        </div>

        <form className="mt-6 space-y-6" onSubmit={handleSubmit}>
            <div className="grid gap-4 md:grid-cols-3">
            <div className="rounded-3xl border border-[var(--border)] bg-[var(--surface-muted)] p-5">
              <p className="text-sm text-[var(--muted)]">Cliente</p>
              <p className="mt-2 text-lg font-semibold text-[var(--foreground)]">{client.nome}</p>
              <p className="mt-1 text-sm text-[var(--muted)]">#{client.clienteId}</p>
            </div>
            <div className="rounded-3xl border border-[var(--border)] bg-[var(--surface-muted)] p-5">
              <p className="text-sm text-[var(--muted)]">Credito atual</p>
              <p className="mt-2 text-3xl font-semibold text-[var(--foreground)]">
                {formatCurrency(currentCredit)}
              </p>
            </div>
            <div className="rounded-3xl border border-[var(--border)] bg-[var(--surface-muted)] p-5">
              <p className="text-sm text-[var(--muted)]">Credito apos operacao</p>
              <p
                className={`mt-2 text-3xl font-semibold ${
                  nextCredit < 0 ? "text-rose-700" : "text-[var(--foreground)]"
                }`}
              >
                {formatCurrency(nextCredit)}
              </p>
            </div>
            </div>

            <div className="grid gap-5 md:grid-cols-2">
              <label className="block space-y-2">
                <span className="text-sm font-semibold text-[var(--foreground)]">
                  Tipo de pagamento
                </span>
                <div className="rounded-2xl border border-[var(--border)] bg-white px-4 py-3 text-sm text-[var(--foreground)]">
                  <Select
                    ariaLabel="Tipo de pagamento"
                    value={String(paymentType)}
                    options={paymentCreditTypeOptions.map((option) => ({
                      label: option.label,
                      value: String(option.value),
                    }))}
                    onChange={(value) => setPaymentType(Number(value) as PaymentCreditTypeValue)}
                  />
                </div>
              </label>

              <label className="block space-y-2">
                <span className="text-sm font-semibold text-[var(--foreground)]">
                  Data do pagamento
                </span>
                <input
                  type="date"
                  value={dateValue}
                  onChange={(event) => setDateValue(event.target.value)}
                  disabled={isSaving}
                  className="h-12 w-full rounded-2xl border border-[var(--border)] bg-white px-4 text-sm text-[var(--foreground)] outline-none transition focus:border-[var(--primary)] focus:shadow-[0_0_0_4px_rgba(106,92,255,0.12)] disabled:cursor-not-allowed disabled:bg-[var(--surface-muted)]"
                />
              </label>
            </div>

            {isCustomerOperation ? (
              <label className="block space-y-2">
                <span className="text-sm font-semibold text-[var(--foreground)]">
                  Forma de pagamento
                </span>
                <div className="rounded-2xl border border-[var(--border)] bg-white px-4 py-3 text-sm text-[var(--foreground)]">
                  <Select
                    ariaLabel="Forma de pagamento"
                    value={paymentMethodId}
                    options={(config?.formasPagamento ?? []).map((item) => ({
                      label: `${item.nome} (${item.percentualAjuste > 0 ? "+" : ""}${item.percentualAjuste}%)`,
                      value: String(item.id),
                    }))}
                    onChange={(value) => setPaymentMethodId(value)}
                  />
                </div>
                <p className="text-sm text-[var(--muted)]">
                  Selecione a taxa ou desconto que sera aplicada sobre o valor do pagamento.
                </p>
              </label>
            ) : null}

            <label className="block space-y-2">
              <span className="text-sm font-semibold text-[var(--foreground)]">
                Valor em credito
              </span>
              <div className="relative">
                <input
                  type="number"
                  min={0}
                  step="0.01"
                  value={creditValue}
                  onChange={(event) => setCreditValue(event.target.value)}
                  disabled={isSaving}
                  className="h-12 w-full rounded-2xl border border-[var(--border)] bg-white px-4 pr-16 text-sm text-[var(--foreground)] outline-none transition focus:border-[var(--primary)] focus:shadow-[0_0_0_4px_rgba(106,92,255,0.12)] disabled:cursor-not-allowed disabled:bg-[var(--surface-muted)]"
                  placeholder="0,00"
                />
                <span className="pointer-events-none absolute right-4 top-1/2 -translate-y-1/2 text-sm font-semibold text-[var(--muted)]">
                  R$
                </span>
              </div>
              <p className="text-sm text-[var(--muted)]">
                {isSupplierOperation
                  ? "Informe quanto sera consumido do credito do sistema para converter em dinheiro ao fornecedor."
                  : "Informe quanto de credito o cliente recebera. A previa abaixo mostra o valor em dinheiro considerando a taxa ou desconto da forma de pagamento."}
              </p>
            </label>

            <div className="grid gap-4 md:grid-cols-2">
              <div className="rounded-3xl border border-[var(--border)] bg-[var(--surface-muted)] p-5">
                <p className="text-sm text-[var(--muted)]">
                  {isSupplierOperation ? "Dinheiro equivalente ao fornecedor" : "Previa em dinheiro"}
                </p>
                <p className="mt-2 text-3xl font-semibold text-[var(--foreground)]">
                  {formatCurrency(moneyPreview)}
                </p>
                <p className="mt-2 text-sm text-[var(--muted)]">
                  {isSupplierOperation && config
                    ? `Calculo: creditos x ${config.percentualRepasseFornecedor}% / ${config.percentualRepasseVendedorCredito}%.`
                    : isCustomerOperation && selectedPaymentMethod
                      ? `Calculo: credito x (1 + ${selectedPaymentMethod.percentualAjuste}%).`
                      : "Conversao direta de credito para dinheiro."}
                </p>
              </div>

              <div className="rounded-3xl border border-[var(--border)] bg-[var(--surface-muted)] p-5">
                <p className="text-sm text-[var(--muted)]">Resumo da operacao</p>
                <p className="mt-2 text-lg font-semibold text-[var(--foreground)]">
                  {formatPaymentCreditType(paymentType)}
                </p>
                <p className="mt-2 text-sm text-[var(--muted)]">
                  {isSupplierOperation
                    ? "Debita credito do sistema desse cliente e registra a saida em dinheiro."
                    : "Gera credito para o cliente e registra quanto foi pago em dinheiro com a forma de pagamento selecionada."}
                </p>
              </div>
            </div>

            {isLoadingConfig ? (
              <div className="rounded-2xl border border-[var(--border)] bg-[var(--surface-muted)] px-4 py-4 text-sm text-[var(--muted)]">
                Carregando configuracao da loja para calcular o repasse.
              </div>
            ) : configMessage ? (
              <div className="rounded-2xl border border-amber-200 bg-amber-50 px-4 py-4 text-sm text-amber-900">
                {configMessage}
              </div>
            ) : null}

            {isCustomerOperation && !configMessage && (!config || config.formasPagamento.length === 0) ? (
              <div className="rounded-2xl border border-amber-200 bg-amber-50 px-4 py-4 text-sm text-amber-900">
                Configure ao menos uma forma de pagamento na loja antes de lancar um pagamento do cliente.
              </div>
            ) : null}

            {isSupplierOperation && !hasEnoughCredit ? (
              <div className="rounded-2xl border border-rose-200 bg-rose-50 px-4 py-4 text-sm text-rose-700">
                O valor informado excede o credito atual do cliente.
              </div>
            ) : null}

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
              disabled={isSaving || !client || !storeId || shouldBlockSubmit}
              className="flex h-12 cursor-pointer items-center justify-center rounded-2xl bg-[linear-gradient(90deg,_#ff8a3d,_#ff6b3d)] px-5 text-sm font-semibold text-white shadow-[0_16px_30px_rgba(255,107,61,0.28)] transition hover:brightness-105 disabled:cursor-not-allowed disabled:opacity-60"
            >
              {isSaving ? "Salvando..." : "Lancar pagamento"}
            </button>
          </div>
        </form>
      </div>
    </div>,
    document.body,
  );
}
