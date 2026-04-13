import {
  formatCurrency,
  formatPaymentCreditType,
  formatPaymentDate,
  type ExternalPaymentListItem,
  type ExternalPaymentVisibleField,
} from "@/lib/payment";

type ExternalPaymentsTableProps = {
  payments: ExternalPaymentListItem[];
  visibleFields: ExternalPaymentVisibleField[];
};

function TableCell({
  children,
  subtle = false,
}: {
  children: React.ReactNode;
  subtle?: boolean;
}) {
  return (
    <td
      className={`px-4 py-4 text-sm ${subtle ? "text-[var(--muted)]" : "text-[var(--foreground)]"}`}
    >
      {children}
    </td>
  );
}

function getTypeBadgeClass(type: number) {
  switch (type) {
    case 1:
      return "bg-emerald-100 text-emerald-700";
    case 2:
      return "bg-amber-100 text-amber-800";
    default:
      return "bg-slate-100 text-slate-600";
  }
}

export function ExternalPaymentsTable({
  payments,
  visibleFields,
}: ExternalPaymentsTableProps) {
  const showId = visibleFields.includes("id");
  const showData = visibleFields.includes("data");
  const showCliente = visibleFields.includes("cliente");
  const showTipo = visibleFields.includes("tipo");
  const showFormaPagamento = visibleFields.includes("formaPagamento");
  const showValorCredito = visibleFields.includes("valorCredito");
  const showValorDinheiro = visibleFields.includes("valorDinheiro");

  return (
    <div className="mt-6 overflow-hidden rounded-[24px] border border-[var(--border)]">
      <div className="overflow-x-auto">
        <table className="min-w-full border-collapse bg-white">
          <thead className="bg-[var(--surface-muted)]">
            <tr className="text-left">
              {showId ? (
                <th className="px-4 py-4 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                  Identificador
                </th>
              ) : null}
              {showData ? (
                <th className="px-4 py-4 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                  Data
                </th>
              ) : null}
              {showCliente ? (
                <th className="px-4 py-4 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                  Cliente
                </th>
              ) : null}
              {showTipo ? (
                <th className="px-4 py-4 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                  Tipo
                </th>
              ) : null}
              {showFormaPagamento ? (
                <th className="px-4 py-4 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                  Forma
                </th>
              ) : null}
              {showValorCredito ? (
                <th className="px-4 py-4 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                  Credito
                </th>
              ) : null}
              {showValorDinheiro ? (
                <th className="px-4 py-4 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                  Dinheiro
                </th>
              ) : null}
            </tr>
          </thead>
          <tbody>
            {payments.map((payment, index) => {
              return (
                <tr
                  key={payment.id}
                  className={
                    index % 2 === 0
                      ? "bg-white"
                      : "bg-[color:color-mix(in_srgb,var(--surface-muted)_55%,white)]"
                  }
                >
                  {showId ? <TableCell subtle>#{payment.id}</TableCell> : null}
                  {showData ? <TableCell>{formatPaymentDate(payment.data)}</TableCell> : null}
                  {showCliente ? <TableCell>{payment.cliente}</TableCell> : null}
                  {showTipo ? (
                    <TableCell>
                      <span
                        className={`inline-flex rounded-full px-3 py-1 text-xs font-semibold ${getTypeBadgeClass(payment.tipo)}`}
                      >
                        {formatPaymentCreditType(payment.tipo)}
                      </span>
                    </TableCell>
                  ) : null}
                  {showFormaPagamento ? (
                    <TableCell>{payment.formaPagamentoNome ?? "-"}</TableCell>
                  ) : null}
                  {showValorCredito ? (
                    <TableCell>
                      <span className="font-semibold text-[var(--foreground)]">
                        {formatCurrency(payment.valorCredito)}
                      </span>
                    </TableCell>
                  ) : null}
                  {showValorDinheiro ? (
                    <TableCell>
                      <span className="font-semibold text-[var(--foreground)]">
                        {formatCurrency(payment.valorDinheiro)}
                      </span>
                    </TableCell>
                  ) : null}
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>
    </div>
  );
}
