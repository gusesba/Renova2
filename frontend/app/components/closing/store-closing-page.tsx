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
  getCurrentMonthInputValue,
  getStoreClosingApiMessage,
  type StoreClosingMonthItem,
} from "@/lib/store-closing";
import { getStoreClosing } from "@/services/store-closing-service";

const chartSeries = [
  {
    key: "valorRecebidoClientes",
    label: "Entradas",
    color: "#15803d",
  },
  {
    key: "valorPagoFornecedores",
    label: "Saidas",
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

function MonthlySummaryChart({ months }: { months: StoreClosingMonthItem[] }) {
  const [activeIndex, setActiveIndex] = useState(months.length - 1);
  const moneyMax = Math.max(
    0,
    ...months.flatMap((month) => [
      month.valorRecebidoClientes,
      month.valorPagoFornecedores,
      month.total,
    ]),
  );
  const moneyMin = Math.min(0, ...months.map((month) => month.total));
  const width = 960;
  const height = 360;
  const paddingTop = 32;
  const paddingBottom = 64;
  const paddingHorizontal = 18;
  const chartHeight = height - paddingTop - paddingBottom;
  const groupWidth = width / months.length;
  const moneyBarWidth = Math.max(12, Math.min(22, groupWidth * 0.22));
  const moneyRange = Math.max(1, moneyMax - moneyMin);
  const moneyBaselineY = paddingTop + (moneyMax / moneyRange) * chartHeight;
  const activeMonth = months[activeIndex] ?? months[months.length - 1];

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

  function getMoneyPointY(value: number) {
    return paddingTop + ((moneyMax - value) / moneyRange) * chartHeight;
  }

  const totalLinePath = months
    .map((month, index) => {
      const x = index * groupWidth + groupWidth / 2;
      const y = getMoneyPointY(month.total);
      return `${index === 0 ? "M" : "L"} ${x} ${y}`;
    })
    .join(" ");

  return (
    <div className="overflow-x-auto">
      <div className="min-w-[760px] rounded-[28px] border border-[var(--border)] bg-[linear-gradient(180deg,#ffffff_0%,#f8fafc_100%)] p-5 shadow-[0_18px_40px_rgba(15,23,42,0.05)]">
        <div className="flex flex-wrap items-center justify-between gap-3">
          <div>
            <h3 className="text-lg font-semibold text-[var(--foreground)]">Grafico mensal</h3>
            <p className="text-sm text-[var(--muted)]">
              Barras para entradas e saidas, com linha de total e destaque interativo por mes.
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

        {activeMonth ? (
          <div className="mt-5 grid gap-3 rounded-[24px] border border-[var(--border)] bg-white/80 p-4 md:grid-cols-4">
            <div>
              <p className="text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                Mes ativo
              </p>
              <p className="mt-2 text-base font-semibold text-[var(--foreground)]">
                {formatClosingMonthLabel(`${activeMonth.ano}-${String(activeMonth.mes).padStart(2, "0")}`)}
              </p>
            </div>
            <div>
              <p className="text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                Entradas
              </p>
              <p className="mt-2 inline-flex rounded-full bg-emerald-100 px-3 py-1 text-sm font-semibold text-emerald-700">
                {formatCurrency(activeMonth.valorRecebidoClientes)}
              </p>
            </div>
            <div>
              <p className="text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                Saidas
              </p>
              <p className="mt-2 inline-flex rounded-full bg-rose-100 px-3 py-1 text-sm font-semibold text-rose-700">
                {formatCurrency(activeMonth.valorPagoFornecedores)}
              </p>
            </div>
            <div>
              <p className="text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                Total
              </p>
              <p className="mt-2 inline-flex rounded-full bg-violet-100 px-3 py-1 text-sm font-semibold text-violet-700">
                {formatCurrency(activeMonth.total)}
              </p>
            </div>
          </div>
        ) : null}

        <div className="mt-6">
          <svg viewBox={`0 0 ${width} ${height}`} className="h-[320px] w-full">
            {[0, 1, 2, 3, 4].map((step) => {
              const ratio = step / 4;
              const value = moneyMax - moneyRange * ratio;
              const y = paddingTop + chartHeight * ratio;
              return (
                <g key={step}>
                  <line
                    x1={paddingHorizontal}
                    y1={y}
                    x2={width - paddingHorizontal}
                    y2={y}
                    stroke={step === 4 ? "#cbd5e1" : "#e2e8f0"}
                    strokeDasharray={step === 4 ? "0" : "4 6"}
                  />
                  <text x={0} y={y - 6} fill="#64748b" fontSize="11">
                    {formatCurrency(value)}
                  </text>
                </g>
              );
            })}

            <line
              x1={paddingHorizontal}
              y1={moneyBaselineY}
              x2={width - paddingHorizontal}
              y2={moneyBaselineY}
              stroke="#94a3b8"
              strokeOpacity="0.55"
            />

            {months[activeIndex] ? (
              <rect
                x={activeIndex * groupWidth + 6}
                y={paddingTop - 8}
                width={groupWidth - 12}
                height={chartHeight + 16}
                rx="18"
                fill="#e2e8f0"
                opacity="0.45"
              />
            ) : null}

            <path
              d={totalLinePath}
              fill="none"
              stroke="#7c3aed"
              strokeWidth="3"
              strokeLinecap="round"
              strokeLinejoin="round"
            />

            {months.map((month, index) => {
              const groupLeft = index * groupWidth;
              const moneyBaseX = groupLeft + groupWidth * 0.2;
              const moneyMetrics: Array<Exclude<MoneyMetricKey, "total">> = [
                "valorRecebidoClientes",
                "valorPagoFornecedores",
              ];
              const isActive = index === activeIndex;

              return (
                <g key={`${month.ano}-${month.mes}`}>
                  {moneyMetrics.map((metric, metricIndex) => {
                    const value = month[metric];
                    const geometry = getMoneyBarGeometry(value);
                    const x = moneyBaseX + metricIndex * (moneyBarWidth + 10);
                    const color = chartSeries.find((series) => series.key === metric)?.color ?? "#334155";

                    return (
                      <rect
                        key={metric}
                        x={x}
                        y={geometry.y}
                        width={moneyBarWidth}
                        height={geometry.height}
                        rx="8"
                        fill={color}
                        opacity={isActive ? 0.95 : 0.78}
                        className="transition-opacity duration-200"
                      />
                    );
                  })}

                  <circle
                    cx={groupLeft + groupWidth / 2}
                    cy={getMoneyPointY(month.total)}
                    r={isActive ? 6 : 4}
                    fill="#ffffff"
                    stroke="#7c3aed"
                    strokeWidth="3"
                  />

                  <rect
                    x={groupLeft + 8}
                    y={paddingTop - 8}
                    width={groupWidth - 16}
                    height={chartHeight + 44}
                    rx="18"
                    fill="transparent"
                    onMouseEnter={() => setActiveIndex(index)}
                  />

                  <text
                    x={groupLeft + groupWidth / 2}
                    y={height - 22}
                    fill={isActive ? "#0f172a" : "#475569"}
                    fontSize="11"
                    fontWeight={isActive ? "700" : "500"}
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

function MonthlySummaryTable({ months }: { months: StoreClosingMonthItem[] }) {
  return (
    <div className="overflow-x-auto">
      <div className="min-w-[760px] rounded-[28px] border border-[var(--border)] bg-[linear-gradient(180deg,#ffffff_0%,#f8fafc_100%)] p-5">
        <div className="flex flex-wrap items-center justify-between gap-3">
          <div>
            <h3 className="text-lg font-semibold text-[var(--foreground)]">Tabela mensal</h3>
            <p className="text-sm text-[var(--muted)]">
              Entradas, saidas e total dos ultimos 12 meses, encerrando na referencia escolhida.
            </p>
          </div>
        </div>

        <div className="mt-6 overflow-hidden rounded-[24px] border border-[var(--border)]">
          <table className="min-w-full border-collapse bg-white">
            <thead className="bg-[var(--surface-muted)]">
              <tr>
                <th className="px-4 py-4 text-left text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                  Mes
                </th>
                <th className="px-4 py-4 text-right text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                  Entradas
                </th>
                <th className="px-4 py-4 text-right text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                  Saidas
                </th>
                <th className="px-4 py-4 text-right text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                  Total
                </th>
              </tr>
            </thead>
            <tbody>
              {months.map((month, index) => (
                <tr
                  key={`${month.ano}-${month.mes}`}
                  className={
                    index % 2 === 0
                      ? "bg-white"
                      : "bg-[color:color-mix(in_srgb,var(--surface-muted)_55%,white)]"
                  }
                >
                  <td className="px-4 py-4 text-sm font-medium text-[var(--foreground)]">
                    {formatClosingMonthShortLabel(month.ano, month.mes)}/{String(month.ano).slice(-2)}
                  </td>
                  <td className="px-4 py-4 text-right text-sm font-semibold text-emerald-700">
                    <span className="inline-flex rounded-full bg-emerald-100 px-3 py-1 text-xs font-semibold text-emerald-700">
                      {formatCurrency(month.valorRecebidoClientes)}
                    </span>
                  </td>
                  <td className="px-4 py-4 text-right text-sm font-semibold text-rose-700">
                    <span className="inline-flex rounded-full bg-rose-100 px-3 py-1 text-xs font-semibold text-rose-700">
                      {formatCurrency(month.valorPagoFornecedores)}
                    </span>
                  </td>
                  <td className="px-4 py-4 text-right text-sm font-semibold text-violet-700">
                    <span className="inline-flex rounded-full bg-violet-100 px-3 py-1 text-xs font-semibold text-violet-700">
                      {formatCurrency(month.total)}
                    </span>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}

export function StoreClosingPage() {
  const { isLoadingStores, selectedStoreId } = useStoreContext();
  const [referenceMonth, setReferenceMonth] = useState(() => getCurrentMonthInputValue());
  const [viewMode, setViewMode] = useState<"tabela" | "grafico">("tabela");
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
              Resumo geral
            </p>
            <h1 className="text-3xl font-semibold tracking-tight text-[var(--foreground)]">Resumo</h1>
            <p className="max-w-2xl text-sm leading-7 text-[var(--muted)]">
              O resumo considera pagamentos externos pela coluna dinheiro e gastos da loja para montar
              entradas, saidas e total do mes selecionado.
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
            description="O resumo depende da loja ativa no topo da pagina."
          />
        ) : closingQuery.isLoading || isLoadingStores ? (
          <MovementEmptyState
            title="Carregando resumo"
            description="Consolidando entradas e saidas da referencia selecionada."
          />
        ) : closingQuery.isError ? (
          <MovementEmptyState
            title="Falha ao carregar resumo"
            description={
              closingQuery.error instanceof Error
                ? closingQuery.error.message
                : "Tente novamente em instantes."
            }
          />
        ) : data ? (
          <div className="mt-8 space-y-8">
            <div className="rounded-[28px] border border-[var(--border)] bg-[linear-gradient(135deg,rgba(15,23,42,0.04),rgba(148,163,184,0.03))] p-6">
              <div className="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
                <p className="text-sm text-[var(--muted)]">
                  Referencia selecionada:{" "}
                  <span className="font-semibold text-[var(--foreground)]">
                    {formatClosingMonthLabel(referenceMonth)}
                  </span>
                </p>
                <div className="inline-flex w-fit rounded-2xl border border-[var(--border)] bg-white p-1 shadow-[0_12px_24px_rgba(15,23,42,0.06)]">
                  <button
                    type="button"
                    onClick={() => setViewMode("tabela")}
                    className={`rounded-xl px-4 py-2 text-sm font-medium transition ${
                      viewMode === "tabela"
                        ? "bg-[var(--foreground)] text-white"
                        : "text-[var(--muted)] hover:text-[var(--foreground)]"
                    }`}
                  >
                    Tabela
                  </button>
                  <button
                    type="button"
                    onClick={() => setViewMode("grafico")}
                    className={`rounded-xl px-4 py-2 text-sm font-medium transition ${
                      viewMode === "grafico"
                        ? "bg-[var(--foreground)] text-white"
                        : "text-[var(--muted)] hover:text-[var(--foreground)]"
                    }`}
                  >
                    Grafico
                  </button>
                </div>
              </div>
              <div className="mt-6 grid gap-4 md:grid-cols-2 xl:grid-cols-4">
                <SummaryCard
                  label="Entradas"
                  value={formatCurrency(data.valorRecebidoClientes)}
                  tone="positive"
                />
                <SummaryCard
                  label="Saidas"
                  value={formatCurrency(data.valorPagoFornecedores)}
                  tone="warning"
                />
                <SummaryCard label="Total" value={formatCurrency(data.total)} tone="accent" />
              </div>
            </div>

            {viewMode === "tabela" ? (
              <MonthlySummaryTable months={data.historico} />
            ) : (
              <MonthlySummaryChart months={data.historico} />
            )}
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
