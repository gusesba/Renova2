import { formatCurrency, formatPaymentDate } from "@/lib/payment";
import {
  formatStoreExpenseNature,
  type StoreExpenseListItem,
  type StoreExpenseTableSettings,
} from "@/lib/store-expense";

type StoreExpensesTableProps = {
  expenses: StoreExpenseListItem[];
  settings: StoreExpenseTableSettings;
};

function getNatureClass(natureza: number) {
  switch (natureza) {
    case 1:
      return "bg-emerald-100 text-emerald-700";
    case 2:
      return "bg-rose-100 text-rose-700";
    default:
      return "bg-slate-100 text-slate-600";
  }
}

export function StoreExpensesTable({ expenses, settings }: StoreExpensesTableProps) {
  const visibleFields = settings.visibleFields;

  return (
    <div className="mt-6 overflow-hidden rounded-[24px] border border-[var(--border)]">
      <div className="overflow-x-auto">
        <table className="min-w-full border-collapse bg-white">
          <thead className="bg-[var(--surface-muted)]">
            <tr className="text-left">
              {visibleFields.includes("id") ? (
                <th className="px-4 py-4 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                  Id
                </th>
              ) : null}
              {visibleFields.includes("data") ? (
                <th className="px-4 py-4 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                  Data
                </th>
              ) : null}
              {visibleFields.includes("natureza") ? (
                <th className="px-4 py-4 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                  Natureza
                </th>
              ) : null}
              {visibleFields.includes("valor") ? (
                <th className="px-4 py-4 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                  Valor
                </th>
              ) : null}
              {visibleFields.includes("descricao") ? (
                <th className="px-4 py-4 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                  Descricao
                </th>
              ) : null}
            </tr>
          </thead>
          <tbody>
            {expenses.map((expense, index) => (
              <tr
                key={expense.id}
                className={
                  index % 2 === 0
                    ? "bg-white"
                    : "bg-[color:color-mix(in_srgb,var(--surface-muted)_55%,white)]"
                }
              >
                {visibleFields.includes("id") ? (
                  <td className="px-4 py-4 text-sm text-[var(--foreground)]">{expense.id}</td>
                ) : null}
                {visibleFields.includes("data") ? (
                  <td className="px-4 py-4 text-sm text-[var(--foreground)]">
                    {formatPaymentDate(expense.data)}
                  </td>
                ) : null}
                {visibleFields.includes("natureza") ? (
                  <td className="px-4 py-4 text-sm text-[var(--foreground)]">
                    <span
                      className={`inline-flex rounded-full px-3 py-1 text-xs font-semibold ${getNatureClass(expense.natureza)}`}
                    >
                      {formatStoreExpenseNature(expense.natureza)}
                    </span>
                  </td>
                ) : null}
                {visibleFields.includes("valor") ? (
                  <td className="px-4 py-4 text-sm font-semibold text-[var(--foreground)]">
                    {formatCurrency(expense.valor)}
                  </td>
                ) : null}
                {visibleFields.includes("descricao") ? (
                  <td className="px-4 py-4 text-sm text-[var(--foreground)]">
                    {expense.descricao?.trim() ? expense.descricao : "-"}
                  </td>
                ) : null}
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
