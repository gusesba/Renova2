type TablePaginationProps = {
  currentPage: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
  summary: string;
  className?: string;
  onPageChange: (page: number) => void;
};

export function TablePagination({
  currentPage,
  totalPages,
  hasPreviousPage,
  hasNextPage,
  summary,
  className = "mt-5",
  onPageChange,
}: TablePaginationProps) {
  const normalizedTotalPages = Math.max(totalPages, 1);
  const normalizedCurrentPage = Math.min(currentPage, normalizedTotalPages);

  return (
    <div
      className={`${className} flex flex-col gap-3 border-t border-[var(--border)] px-1 pt-5 sm:flex-row sm:items-center sm:justify-between`}
    >
      <p className="text-sm text-[var(--muted)]">{summary}</p>

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
          Pagina {normalizedCurrentPage} de {normalizedTotalPages}
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
