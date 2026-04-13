"use client";

import { useQuery } from "@tanstack/react-query";
import { useMemo, useState } from "react";

import { MovementEmptyState } from "@/app/components/movement/movement-empty-state";
import { useStoreContext } from "@/app/dashboard/store-context";
import { formatCurrency } from "@/lib/payment";
import { getAuthToken } from "@/lib/store";
import {
  asStoreClosingResponse,
  formatClosingMonthLabel,
  formatClosingMonthShortLabel,
  getPreviousMonthInputValue,
  getStoreClosingApiMessage,
  type StoreClosingMonthItem,
} from "@/lib/store-closing";
import { getStoreClosing } from "@/services/store-closing-service";

const chartSeries = [
  {
    key: "quantidadePecasVendidas",
    label: "Pecas vendidas",
    color: "#1d4ed8",
  },
  {
    key: "valorRecebidoClientes",
    label: "Recebido",
    color: "#15803d",
  },
  {
    key: "valorPagoFornecedores",
    label: "Pago fornecedores",
    color: "#b45309",
  },
  {
    key: "total",
    label: "Total",
    color: "#7c3aed",
  },
] as const;

type MoneyMetricKey = "valorRecebidoClientes" | "valorPagoFornecedores" | "total";

function SummaryCard({
  label,
  value,
  tone,
}: {
  label: string;
  value: string;
  tone: "neutral" | "positive" | "warning" | "accent";
}) {
  const toneClass =
    tone === "positive"
      ? "border-emerald-200 bg-emerald-50/80"
      : tone === "warning"
        ? "border-amber-200 bg-amber-50/80"
        : tone === "accent"
          ? "border-violet-200 bg-violet-50/80"
          : "border-slate-200 bg-slate-50/80";

  return (
    <article className={`rounded-[24px] border p-5 shadow-[0_12px_30px_rgba(15,23,42,0.04)] ${toneClass}`}>
      <p className="text-sm font-medium text-[var(--muted)]">{label}</p>
      <p className="mt-3 text-3xl font-semibold tracking-tight text-[var(--foreground)]">{value}</p>
    </article>
  );
}

function StoreClosingChart({ months }: { months: StoreClosingMonthItem[] }) {
  const moneyMax = Math.max(
    0,
    ...months.flatMap((month) => [
      month.valorRecebidoClientes,
      month.valorPagoFornecedores,
      month.total,
    ]),
  );
  const moneyMin = Math.min(0, ...months.map((month) => month.total));
  const piecesMax = Math.max(1, ...months.map((month) => Math.max(0, month.quantidadePecasVendidas)));
  const width = 960;
  const height = 320;
  const paddingTop = 24;
  const paddingBottom = 54;
  const chartHeight = height - paddingTop - paddingBottom;
  const groupWidth = width / months.length;
  const moneyBarWidth = Math.max(10, Math.min(18, groupWidth * 0.16));
  const piecesBarWidth = Math.max(8, Math.min(12, groupWidth * 0.12));
  const moneyRange = Math.max(1, moneyMax - moneyMin);
  const moneyBaselineY = paddingTop + (moneyMax / moneyRange) * chartHeight;

  function getMoneyBarGeometry(value: number) {
    const normalizedHeight = (Math.abs(value) / moneyRange) * chartHeight;

    if (value >= 0) {
      return {
        y: moneyBaselineY - normalizedHeight,
        height: normalizedHeight,
      };
    }

    return {
      y: moneyBaselineY,
      height: normalizedHeight,
    };
  }

  function getPiecesBarHeight(value: number) {
    return Math.max(0, (Math.max(0, value) / piecesMax) * chartHeight);
  }

  return (
    <div className="overflow-x-auto">
      <div className="min-w-[760px] rounded-[28px] border border-[var(--border)] bg-[linear-gradient(180deg,#ffffff_0%,#f8fafc_100%)] p-5">
        <div className="flex flex-wrap items-center justify-between gap-3">
          <div>
            <h3 className="text-lg font-semibold text-[var(--foreground)]">Historico de 12 meses</h3>
            <p className="text-sm text-[var(--muted)]">
              Barras monetarias no eixo esquerdo e pecas vendidas no eixo direito.
            </p>
          </div>
          <div className="flex flex-wrap gap-3 text-xs text-[var(--muted)]">
            {chartSeries.map((series) => (
              <span key={series.key} className="inline-flex items-center gap-2">
                <span
                  className="h-3 w-3 rounded-full"
                  style={{ backgroundColor: series.color }}
                  aria-hidden="true"
                />
                {series.label}
              </span>
            ))}
          </div>
        </div>

        <div className="mt-6">
          <svg viewBox={`0 0 ${width} ${height}`} className="h-[320px] w-full">
            {[0, 1, 2, 3, 4].map((step) => {
              const ratio = step / 4;
              const value = moneyMax - moneyRange * ratio;
              const y = paddingTop + chartHeight * ratio;
              return (
                <g key={step}>
                  <line x1="0" y1={y} x2={width} y2={y} stroke="#e2e8f0" strokeDasharray="4 6" />
                  <text x="0" y={y - 6} fill="#64748b" fontSize="11">
                    {formatCurrency(value)}
                  </text>
                  <text x={width - 4} y={y - 6} fill="#64748b" fontSize="11" textAnchor="end">
                    {Math.round(piecesMax * (1 - ratio))}
                  </text>
                </g>
              );
            })}

            {months.map((month, index) => {
              const groupLeft = index * groupWidth;
              const moneyBaseX = groupLeft + groupWidth * 0.14;
              const piecesBaseX = groupLeft + groupWidth * 0.72;
              const moneyMetrics: MoneyMetricKey[] = [
                "valorRecebidoClientes",
                "valorPagoFornecedores",
                "total",
              ];

              return (
                <g key={`${month.ano}-${month.mes}`}>
                  {moneyMetrics.map((metric, metricIndex) => {
                    const value = month[metric];
                    const geometry = getMoneyBarGeometry(value);
                    const x = moneyBaseX + metricIndex * (moneyBarWidth + 6);
                    const color = chartSeries.find((series) => series.key === metric)?.color ?? "#334155";

                    return (
                      <rect
                        key={metric}
                        x={x}
                        y={geometry.y}
                        width={moneyBarWidth}
                        height={geometry.height}
                        rx="4"
                        fill={color}
                        opacity={metric === "total" ? 0.9 : 0.78}
                      />
                    );
                  })}

                  <rect
                    x={piecesBaseX}
                    y={paddingTop + chartHeight - getPiecesBarHeight(month.quantidadePecasVendidas)}
                    width={piecesBarWidth}
                    height={getPiecesBarHeight(month.quantidadePecasVendidas)}
                    rx="4"
                    fill="#1d4ed8"
                  />

                  <text
                    x={groupLeft + groupWidth / 2}
                    y={height - 18}
                    fill="#475569"
                    fontSize="11"
                    textAnchor="middle"
                  >
                    {formatClosingMonthShortLabel(month.ano, month.mes)}/{String(month.ano).slice(-2)}
                  </text>
                </g>
              );
            })}
          </svg>
        </div>
      </div>
    </div>
  );
}

export function StoreClosingPage() {
  const { isLoadingStores, selectedStoreId } = useStoreContext();
  const [referenceMonth, setReferenceMonth] = useState(() => getPreviousMonthInputValue());
  const token = useMemo(() => (typeof window === "undefined" ? null : getAuthToken()), []);

  const closingQuery = useQuery({
    queryKey: ["store-closing", token, selectedStoreId, referenceMonth],
    queryFn: async () => {
      if (!token || !selectedStoreId) {
        return null;
      }

      const response = await getStoreClosing(token, selectedStoreId, referenceMonth);

      if (!response.ok) {
        throw new Error(
          getStoreClosingApiMessage(response.body) ?? "Nao foi possivel carregar o fechamento da loja.",
        );
      }

      return asStoreClosingResponse(response.body);
    },
    enabled: Boolean(token && selectedStoreId),
  });

  const data = closingQuery.data;

  return (
    <section className="space-y-6">
      <div className="rounded-[28px] border border-[var(--border)] bg-white p-6 shadow-[0_18px_45px_rgba(15,23,42,0.05)]">
        <div className="flex flex-col gap-4 lg:flex-row lg:items-end lg:justify-between">
          <div className="space-y-2">
            <p className="text-sm font-medium uppercase tracking-[0.18em] text-[var(--muted)]">
              Fechamento geral
            </p>
            <h1 className="text-3xl font-semibold tracking-tight text-[var(--foreground)]">
              Resumo mensal da loja
            </h1>
            <p className="max-w-2xl text-sm leading-7 text-[var(--muted)]">
              O fechamento considera o mes selecionado no resumo e monta o grafico com os 12 meses
              encerrando nessa referencia.
            </p>
          </div>

          <label className="flex flex-col gap-2 text-sm font-medium text-[var(--foreground)]">
            Mes de referencia
            <input
              type="month"
              value={referenceMonth}
              onChange={(event) => setReferenceMonth(event.target.value)}
              className="min-w-[220px] rounded-2xl border border-[var(--border)] bg-[var(--surface)] px-4 py-3 text-sm outline-none transition focus:border-[var(--primary)] focus:ring-2 focus:ring-[color:color-mix(in_srgb,var(--primary)_18%,transparent)]"
            />
          </label>
        </div>

        {!selectedStoreId ? (
          <MovementEmptyState
            title="Selecione uma loja"
            description="O fechamento geral depende da loja ativa no topo da pagina."
          />
        ) : closingQuery.isLoading || isLoadingStores ? (
          <MovementEmptyState
            title="Carregando fechamento"
            description="Consolidando vendas e pagamentos da referencia selecionada."
          />
        ) : closingQuery.isError ? (
          <MovementEmptyState
            title="Falha ao carregar fechamento"
            description={
              closingQuery.error instanceof Error
                ? closingQuery.error.message
                : "Tente novamente em instantes."
            }
          />
        ) : data ? (
          <div className="mt-8 space-y-8">
            <div className="rounded-[28px] border border-[var(--border)] bg-[linear-gradient(135deg,rgba(15,23,42,0.04),rgba(148,163,184,0.03))] p-6">
              <p className="text-sm text-[var(--muted)]">
                Referencia selecionada:{" "}
                <span className="font-semibold text-[var(--foreground)]">
                  {formatClosingMonthLabel(referenceMonth)}
                </span>
              </p>
              <div className="mt-6 grid gap-4 md:grid-cols-2 xl:grid-cols-4">
                <SummaryCard
                  label="Quantidade de pecas vendidas"
                  value={String(data.quantidadePecasVendidas)}
                  tone="neutral"
                />
                <SummaryCard
                  label="Valor recebido de clientes"
                  value={formatCurrency(data.valorRecebidoClientes)}
                  tone="positive"
                />
                <SummaryCard
                  label="Valor pago aos fornecedores"
                  value={formatCurrency(data.valorPagoFornecedores)}
                  tone="warning"
                />
                <SummaryCard label="Total" value={formatCurrency(data.total)} tone="accent" />
              </div>
            </div>

            <StoreClosingChart months={data.historico} />
          </div>
        ) : (
          <MovementEmptyState
            title="Nenhum dado encontrado"
            description="Nao foi possivel montar o fechamento para a referencia informada."
          />
        )}
      </div>
    </section>
  );
}
