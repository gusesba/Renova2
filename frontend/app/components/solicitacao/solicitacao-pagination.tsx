type SolicitacaoPaginationProps = {
  currentPage: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
  totalItems: number;
  totalPages: number;
  onPageChange: (page: number) => void;
};

export function SolicitacaoPagination({
  currentPage,
  hasNextPage,
  hasPreviousPage,
  totalItems,
  totalPages,
  onPageChange,
}: SolicitacaoPaginationProps) {
  return (
    <div className="mt-6 flex flex-col gap-3 border-t border-[var(--border)] px-1 pt-5 sm:flex-row sm:items-center sm:justify-between">
      <p className="text-sm text-[var(--muted)]">
        {totalItems} registro(s) distribuido(s) em {Math.max(totalPages, 1)} pagina(s).
      </p>

      <div className="flex items-center gap-3">
        <button
          type="button"
          disabled={!hasPreviousPage}
          onClick={() => onPageChange(currentPage - 1)}
          className="flex h-11 cursor-pointer items-center justify-center rounded-2xl border border-[var(--border)] bg-white px-4 text-sm font-semibold text-[var(--foreground)] transition hover:border-[var(--border-strong)] disabled:cursor-not-allowed disabled:opacity-50"
        >
          Anterior
        </button>
        <span className="min-w-20 text-center text-sm font-semibold text-[var(--foreground)]">
          Pagina {currentPage}
        </span>
        <button
          type="button"
          disabled={!hasNextPage}
          onClick={() => onPageChange(currentPage + 1)}
          className="flex h-11 cursor-pointer items-center justify-center rounded-2xl border border-[var(--border)] bg-white px-4 text-sm font-semibold text-[var(--foreground)] transition hover:border-[var(--border-strong)] disabled:cursor-not-allowed disabled:opacity-50"
        >
          Proxima
        </button>
      </div>
    </div>
  );
}
