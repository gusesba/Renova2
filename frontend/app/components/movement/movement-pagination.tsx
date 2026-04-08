type MovementPaginationProps = {
  currentPage: number;
  totalPages: number;
  totalItems: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
  onPageChange: (page: number) => void;
};

export function MovementPagination({
  currentPage,
  totalPages,
  totalItems,
  hasPreviousPage,
  hasNextPage,
  onPageChange,
}: MovementPaginationProps) {
  return (
    <div className="mt-5 flex flex-col gap-3 border-t border-[var(--border)] px-1 pt-5 sm:flex-row sm:items-center sm:justify-between">
      <p className="text-sm text-[var(--muted)]">{totalItems} movimentacao(oes) encontrada(s)</p>

      <div className="flex items-center gap-3">
        <button
          type="button"
          disabled={!hasPreviousPage}
          onClick={() => onPageChange(currentPage - 1)}
          className="flex h-11 cursor-pointer items-center justify-center rounded-2xl border border-[var(--border)] bg-white px-4 text-sm font-medium text-[var(--foreground)] transition hover:border-[var(--border-strong)] disabled:cursor-not-allowed disabled:opacity-50"
        >
          Anterior
        </button>
        <span className="text-sm text-[var(--muted)]">
          Pagina {Math.min(currentPage, Math.max(totalPages, 1))} de {Math.max(totalPages, 1)}
        </span>
        <button
          type="button"
          disabled={!hasNextPage}
          onClick={() => onPageChange(currentPage + 1)}
          className="flex h-11 cursor-pointer items-center justify-center rounded-2xl border border-[var(--border)] bg-white px-4 text-sm font-medium text-[var(--foreground)] transition hover:border-[var(--border-strong)] disabled:cursor-not-allowed disabled:opacity-50"
        >
          Proxima
        </button>
      </div>
    </div>
  );
}
