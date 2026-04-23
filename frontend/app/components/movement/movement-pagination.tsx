import { TablePagination } from "@/app/components/ui/table-pagination";

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
    <TablePagination
      currentPage={currentPage}
      totalPages={totalPages}
      hasPreviousPage={hasPreviousPage}
      hasNextPage={hasNextPage}
      summary={`${totalItems} movimentacao(oes) encontrada(s)`}
      onPageChange={onPageChange}
    />
  );
}
