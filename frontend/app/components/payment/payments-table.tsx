import { formatMovementDate, formatMovementType } from "@/lib/movement";
import {
  formatCurrency,
  formatPaymentDate,
  formatPaymentNature,
  formatPaymentStatus,
  getPaymentStatusBadgeClass,
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
  const showDescricao = visibleFields.includes("descricao");
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
              {showDescricao ? (
                <th className="px-4 py-4 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                  Descricao
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
              const hasMovimentacao = Boolean(payment.movimentacao);

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
                        disabled={!hasMovimentacao}
                        className="inline-flex h-10 w-10 items-center justify-center rounded-2xl border border-[var(--border)] bg-white text-[var(--muted)] transition hover:border-[var(--border-strong)] hover:text-[var(--foreground)] disabled:cursor-not-allowed disabled:opacity-45"
                        aria-label={
                          hasMovimentacao
                            ? `${expanded ? "Ocultar" : "Exibir"} detalhes do pagamento ${payment.id}`
                            : `Pagamento ${payment.id} sem movimentacao vinculada`
                        }
                      >
                        <ChevronIcon expanded={expanded} />
                      </button>
                    </TableCell>
                    {showId ? <TableCell subtle>#{payment.id}</TableCell> : null}
                    {showData ? <TableCell>{formatPaymentDate(payment.data)}</TableCell> : null}
                    {showCliente ? <TableCell>{payment.cliente}</TableCell> : null}
                    {showDescricao ? <TableCell>{payment.descricao?.trim() || "-"}</TableCell> : null}
                    {showValor ? (
                      <TableCell>
                        <span className="font-semibold text-[var(--foreground)]">
                          {formatCurrency(payment.valor)}
                        </span>
                      </TableCell>
                    ) : null}
                    {showNatureza ? <TableCell>{formatPaymentNature(payment.natureza)}</TableCell> : null}
                    {showStatus ? (
                      <TableCell>
                        <span
                          className={`inline-flex rounded-full px-3 py-1 text-xs font-semibold ${getPaymentStatusBadgeClass(payment.status)}`}
                        >
                          {formatPaymentStatus(payment.status)}
                        </span>
                      </TableCell>
                    ) : null}
                    {showMovimentacao ? (
                      <TableCell subtle>
                        {payment.movimentacaoId ? `#${payment.movimentacaoId}` : "-"}
                      </TableCell>
                    ) : null}
                  </tr>
                  {expanded && payment.movimentacao ? (
                    <tr key={`${payment.id}-expanded`} className="bg-[var(--surface-muted)]">
                      <td colSpan={visibleColumnCount} className="px-4 py-5">
                        <div className="overflow-hidden rounded-[20px] border border-[var(--border)] bg-white">
                          <div className="px-4 py-4">
                            <p className="text-sm font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                              Movimentacao relacionada
                            </p>
                          </div>
                          <div className="overflow-x-auto border-t border-[var(--border)]">
                            <table className="min-w-[720px] w-full border-collapse">
                              <thead className="bg-[var(--surface-muted)]">
                                <tr className="text-left">
                                  <th className="px-4 py-3 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                                    Id
                                  </th>
                                  <th className="px-4 py-3 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                                    Tipo
                                  </th>
                                  <th className="px-4 py-3 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                                    Data
                                  </th>
                                  <th className="px-4 py-3 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                                    Produtos
                                  </th>
                                  <th className="px-4 py-3 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                                    Cliente
                                  </th>
                                  <th className="px-4 py-3 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                                    Loja
                                  </th>
                                  <th className="px-4 py-3 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                                    Produto(s) vinculados
                                  </th>
                                </tr>
                              </thead>
                              <tbody>
                                <tr className="border-t border-[var(--border)] align-top">
                                  <td className="px-4 py-4 text-sm font-medium text-[var(--foreground)]">
                                    #{payment.movimentacao.id}
                                  </td>
                                  <td className="px-4 py-4 text-sm text-[var(--foreground)]">
                                    {formatMovementType(payment.movimentacao.tipo)}
                                  </td>
                                  <td className="px-4 py-4 text-sm text-[var(--foreground)]">
                                    {formatMovementDate(payment.movimentacao.data)}
                                  </td>
                                  <td className="px-4 py-4 text-sm text-[var(--foreground)]">
                                    {payment.movimentacao.quantidadeProdutos} item(ns)
                                  </td>
                                  <td className="px-4 py-4 text-sm text-[var(--foreground)]">
                                    <div className="min-w-0">
                                      <p className="break-words font-medium">
                                        {payment.movimentacao.cliente}
                                      </p>
                                      <p className="mt-1 text-xs text-[var(--muted)]">
                                        Cliente #{payment.movimentacao.clienteId}
                                      </p>
                                    </div>
                                  </td>
                                  <td className="px-4 py-4 text-sm text-[var(--foreground)]">
                                    #{payment.movimentacao.lojaId}
                                  </td>
                                  <td className="px-4 py-4 text-sm text-[var(--foreground)]">
                                    {payment.movimentacao.produtoIds.length > 0 ? (
                                      <div className="flex flex-wrap gap-2">
                                        {payment.movimentacao.produtoIds.map((productId) => (
                                          <span
                                            key={`${payment.id}-${productId}`}
                                            className="inline-flex rounded-full border border-[var(--border)] bg-[var(--surface-muted)] px-3 py-1 text-xs font-semibold text-[var(--foreground)]"
                                          >
                                            #{productId}
                                          </span>
                                        ))}
                                      </div>
                                    ) : (
                                      <span className="text-[var(--muted)]">Nenhum produto vinculado.</span>
                                    )}
                                  </td>
                                </tr>
                              </tbody>
                            </table>
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
