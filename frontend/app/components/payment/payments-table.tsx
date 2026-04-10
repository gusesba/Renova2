import { formatMovementDate, formatMovementType } from "@/lib/movement";
import {
  formatCurrency,
  formatPaymentDate,
  formatPaymentNature,
  formatPaymentStatus,
  type PaymentListItem,
  type PaymentVisibleField,
} from "@/lib/payment";

type PaymentsTableProps = {
  expandedIds: number[];
  payments: PaymentListItem[];
  visibleFields: PaymentVisibleField[];
  onToggleExpanded: (paymentId: number) => void;
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

function ChevronIcon({ expanded }: { expanded: boolean }) {
  return (
    <svg
      aria-hidden="true"
      viewBox="0 0 24 24"
      className={`h-4 w-4 transition ${expanded ? "rotate-180" : "rotate-0"}`}
      fill="none"
      stroke="currentColor"
      strokeWidth="2"
      strokeLinecap="round"
      strokeLinejoin="round"
    >
      <path d="m6 9 6 6 6-6" />
    </svg>
  );
}

export function PaymentsTable({
  expandedIds,
  payments,
  visibleFields,
  onToggleExpanded,
}: PaymentsTableProps) {
  const showId = visibleFields.includes("id");
  const showData = visibleFields.includes("data");
  const showCliente = visibleFields.includes("cliente");
  const showValor = visibleFields.includes("valor");
  const showNatureza = visibleFields.includes("natureza");
  const showStatus = visibleFields.includes("status");
  const showMovimentacao = visibleFields.includes("movimentacaoId");
  const visibleColumnCount = visibleFields.length + 1;

  return (
    <div className="mt-6 overflow-hidden rounded-[24px] border border-[var(--border)]">
      <div className="overflow-x-auto">
        <table className="min-w-full border-collapse bg-white">
          <thead className="bg-[var(--surface-muted)]">
            <tr className="text-left">
              <th className="w-14 px-4 py-4 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                Detalhes
              </th>
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
              {showValor ? (
                <th className="px-4 py-4 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                  Valor
                </th>
              ) : null}
              {showNatureza ? (
                <th className="px-4 py-4 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                  Natureza
                </th>
              ) : null}
              {showStatus ? (
                <th className="px-4 py-4 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                  Status
                </th>
              ) : null}
              {showMovimentacao ? (
                <th className="px-4 py-4 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                  Movimentacao
                </th>
              ) : null}
            </tr>
          </thead>
          <tbody>
            {payments.map((payment, index) => {
              const expanded = expandedIds.includes(payment.id);

              return (
                <>
                  <tr
                    key={payment.id}
                    className={
                      index % 2 === 0
                        ? "bg-white"
                        : "bg-[color:color-mix(in_srgb,var(--surface-muted)_55%,white)]"
                    }
                  >
                    <TableCell>
                      <button
                        type="button"
                        onClick={() => onToggleExpanded(payment.id)}
                        className="inline-flex h-10 w-10 cursor-pointer items-center justify-center rounded-2xl border border-[var(--border)] bg-white text-[var(--muted)] transition hover:border-[var(--border-strong)] hover:text-[var(--foreground)]"
                        aria-label={`${expanded ? "Ocultar" : "Exibir"} detalhes do pagamento ${payment.id}`}
                      >
                        <ChevronIcon expanded={expanded} />
                      </button>
                    </TableCell>
                    {showId ? <TableCell subtle>#{payment.id}</TableCell> : null}
                    {showData ? <TableCell>{formatPaymentDate(payment.data)}</TableCell> : null}
                    {showCliente ? <TableCell>{payment.cliente}</TableCell> : null}
                    {showValor ? (
                      <TableCell>
                        <span className="font-semibold text-[var(--foreground)]">
                          {formatCurrency(payment.valor)}
                        </span>
                      </TableCell>
                    ) : null}
                    {showNatureza ? <TableCell>{formatPaymentNature(payment.natureza)}</TableCell> : null}
                    {showStatus ? <TableCell>{formatPaymentStatus(payment.status)}</TableCell> : null}
                    {showMovimentacao ? <TableCell subtle>#{payment.movimentacaoId}</TableCell> : null}
                  </tr>
                  {expanded ? (
                    <tr key={`${payment.id}-expanded`} className="bg-[var(--surface-muted)]">
                      <td colSpan={visibleColumnCount} className="px-4 py-5">
                        <div className="rounded-[20px] border border-[var(--border)] bg-white p-4">
                          <p className="text-sm font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                            Movimentacao relacionada
                          </p>
                          <div className="mt-4 grid gap-4 md:grid-cols-2 xl:grid-cols-4">
                            <div className="rounded-2xl border border-[var(--border)] bg-[var(--surface-muted)] p-4">
                              <p className="text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                                Identificador
                              </p>
                              <p className="mt-2 text-lg font-semibold text-[var(--foreground)]">
                                #{payment.movimentacao.id}
                              </p>
                            </div>
                            <div className="rounded-2xl border border-[var(--border)] bg-[var(--surface-muted)] p-4">
                              <p className="text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                                Tipo
                              </p>
                              <p className="mt-2 text-lg font-semibold text-[var(--foreground)]">
                                {formatMovementType(payment.movimentacao.tipo)}
                              </p>
                            </div>
                            <div className="rounded-2xl border border-[var(--border)] bg-[var(--surface-muted)] p-4">
                              <p className="text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                                Data
                              </p>
                              <p className="mt-2 text-lg font-semibold text-[var(--foreground)]">
                                {formatMovementDate(payment.movimentacao.data)}
                              </p>
                            </div>
                            <div className="rounded-2xl border border-[var(--border)] bg-[var(--surface-muted)] p-4">
                              <p className="text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                                Produtos
                              </p>
                              <p className="mt-2 text-lg font-semibold text-[var(--foreground)]">
                                {payment.movimentacao.quantidadeProdutos} item(ns)
                              </p>
                            </div>
                          </div>

                          <div className="mt-4 grid gap-4 lg:grid-cols-[minmax(0,1.4fr)_minmax(0,1fr)]">
                            <div className="rounded-[20px] border border-[var(--border)] bg-[var(--surface-muted)] p-4">
                              <p className="text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                                Cliente da movimentacao
                              </p>
                              <p className="mt-2 text-base font-semibold text-[var(--foreground)]">
                                {payment.movimentacao.cliente}
                              </p>
                              <p className="mt-1 text-sm text-[var(--muted)]">
                                Cliente #{payment.movimentacao.clienteId} na loja #
                                {payment.movimentacao.lojaId}
                              </p>
                            </div>

                            <div className="rounded-[20px] border border-[var(--border)] bg-[var(--surface-muted)] p-4">
                              <p className="text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                                Produto(s) vinculados
                              </p>
                              <div className="mt-3 flex flex-wrap gap-2">
                                {payment.movimentacao.produtoIds.length > 0 ? (
                                  payment.movimentacao.produtoIds.map((productId) => (
                                    <span
                                      key={`${payment.id}-${productId}`}
                                      className="inline-flex rounded-full border border-[var(--border)] bg-white px-3 py-1 text-xs font-semibold text-[var(--foreground)]"
                                    >
                                      #{productId}
                                    </span>
                                  ))
                                ) : (
                                  <span className="text-sm text-[var(--muted)]">
                                    Nenhum produto vinculado.
                                  </span>
                                )}
                              </div>
                            </div>
                          </div>
                        </div>
                      </td>
                    </tr>
                  ) : null}
                </>
              );
            })}
          </tbody>
        </table>
      </div>
    </div>
  );
}
