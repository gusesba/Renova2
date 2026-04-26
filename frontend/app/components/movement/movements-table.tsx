import { formatMovementDate, formatMovementType, type MovementListItem, type MovementVisibleField } from "@/lib/movement";

type MovementsTableProps = {
  expandedIds: number[];
  movements: MovementListItem[];
  visibleFields: MovementVisibleField[];
  onPrintMovement: (movement: MovementListItem) => void;
  onToggleExpanded: (movementId: number) => void;
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

function PrintIcon() {
  return (
    <svg
      aria-hidden="true"
      viewBox="0 0 24 24"
      className="h-4 w-4"
      fill="none"
      stroke="currentColor"
      strokeLinecap="round"
      strokeLinejoin="round"
      strokeWidth="2"
    >
      <path d="M6 9V2h12v7" />
      <path d="M6 18H4a2 2 0 0 1-2-2v-5a2 2 0 0 1 2-2h16a2 2 0 0 1 2 2v5a2 2 0 0 1-2 2h-2" />
      <path d="M6 14h12v8H6z" />
    </svg>
  );
}

export function MovementsTable({
  expandedIds,
  movements,
  onPrintMovement,
  visibleFields,
  onToggleExpanded,
}: MovementsTableProps) {
  const showId = visibleFields.includes("id");
  const showData = visibleFields.includes("data");
  const showCliente = visibleFields.includes("cliente");
  const showQuantidade = visibleFields.includes("quantidadeProdutos");
  const showTipo = visibleFields.includes("tipo");
  const visibleColumnCount = visibleFields.length + 2;

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
              {showQuantidade ? (
                <th className="px-4 py-4 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                  Quantidade
                </th>
              ) : null}
              {showTipo ? (
                <th className="px-4 py-4 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                  Tipo
                </th>
              ) : null}
              <th className="px-4 py-4 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                Acoes
              </th>
            </tr>
          </thead>
          <tbody>
            {movements.map((movement, index) => {
              const expanded = expandedIds.includes(movement.id);

              return (
                <>
                  <tr
                    key={movement.id}
                    className={
                      index % 2 === 0
                        ? "bg-white"
                        : "bg-[color:color-mix(in_srgb,var(--surface-muted)_55%,white)]"
                    }
                  >
                    <TableCell>
                      <button
                        type="button"
                        onClick={() => onToggleExpanded(movement.id)}
                        className="inline-flex h-10 w-10 cursor-pointer items-center justify-center rounded-2xl border border-[var(--border)] bg-white text-[var(--muted)] transition hover:border-[var(--border-strong)] hover:text-[var(--foreground)]"
                        aria-label={`${expanded ? "Ocultar" : "Exibir"} produtos da movimentacao ${movement.id}`}
                      >
                        <ChevronIcon expanded={expanded} />
                      </button>
                    </TableCell>
                    {showId ? <TableCell subtle>#{movement.id}</TableCell> : null}
                    {showData ? <TableCell>{formatMovementDate(movement.data)}</TableCell> : null}
                    {showCliente ? <TableCell>{movement.cliente}</TableCell> : null}
                    {showQuantidade ? (
                      <TableCell>
                        <span className="inline-flex rounded-full bg-[var(--primary-soft)] px-3 py-1 text-xs font-semibold text-[var(--primary)]">
                          {movement.quantidadeProdutos} item(ns)
                        </span>
                      </TableCell>
                    ) : null}
                    {showTipo ? <TableCell>{formatMovementType(movement.tipo)}</TableCell> : null}
                    <TableCell>
                      <button
                        type="button"
                        onClick={() => onPrintMovement(movement)}
                        className="inline-flex h-10 w-10 cursor-pointer items-center justify-center rounded-2xl border border-[var(--border)] bg-white text-[var(--muted)] transition hover:border-[var(--border-strong)] hover:text-[var(--foreground)]"
                        aria-label={`Imprimir nota da movimentacao ${movement.id}`}
                        title={`Imprimir nota da movimentacao ${movement.id}`}
                      >
                        <PrintIcon />
                      </button>
                    </TableCell>
                  </tr>
                  {expanded ? (
                    <tr key={`${movement.id}-expanded`} className="bg-[var(--surface-muted)]">
                      <td colSpan={visibleColumnCount} className="px-4 py-5">
                        <div className="rounded-[20px] border border-[var(--border)] bg-white p-4">
                          <p className="text-sm font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                            Produtos vinculados
                          </p>
                          <div className="mt-4 overflow-hidden rounded-[20px] border border-[var(--border)]">
                            <div className="overflow-x-auto">
                              <table className="min-w-full border-collapse bg-white">
                                <thead className="bg-[var(--surface-muted)]">
                                  <tr className="text-left">
                                    <th className="px-4 py-3 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                                      Id
                                    </th>
                                    <th className="px-4 py-3 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                                      Produto
                                    </th>
                                    <th className="px-4 py-3 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                                      Descricao
                                    </th>
                                    <th className="px-4 py-3 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                                      Marca
                                    </th>
                                    <th className="px-4 py-3 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                                      Fornecedor
                                    </th>
                                  </tr>
                                </thead>
                                <tbody>
                                  {movement.produtos.map((product, productIndex) => (
                                    <tr
                                      key={`${movement.id}-${product.id}`}
                                      className={
                                        productIndex % 2 === 0
                                          ? "bg-white"
                                          : "bg-[color:color-mix(in_srgb,var(--surface-muted)_55%,white)]"
                                      }
                                    >
                                      <TableCell subtle>#{product.id}</TableCell>
                                      <TableCell>{product.produto}</TableCell>
                                      <TableCell>{product.descricao}</TableCell>
                                      <TableCell subtle>{product.marca}</TableCell>
                                      <TableCell subtle>{product.fornecedor}</TableCell>
                                    </tr>
                                  ))}
                                </tbody>
                              </table>
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
