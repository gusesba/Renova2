import { TablePagination } from "@/app/components/ui/table-pagination";

type ClientAreaPaginationProps = {
  currentPage: number;
  totalPages: number;
  totalItems: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
  onPageChange: (page: number) => void;
};

export function ClientAreaPagination({
  currentPage,
  totalPages,
  totalItems,
  hasPreviousPage,
  hasNextPage,
  onPageChange,
}: ClientAreaPaginationProps) {
  return (
    <TablePagination
      currentPage={currentPage}
      totalPages={totalPages}
      hasPreviousPage={hasPreviousPage}
      hasNextPage={hasNextPage}
      summary={`${totalItems} peca(s) encontrada(s)`}
      onPageChange={onPageChange}
    />
  );
}
