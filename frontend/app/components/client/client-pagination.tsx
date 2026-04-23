import { TablePagination } from "@/app/components/ui/table-pagination";

type ClientPaginationProps = {
  currentPage: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
  totalItems: number;
  totalPages: number;
  onPageChange: (page: number) => void;
};

export function ClientPagination({
  currentPage,
  hasNextPage,
  hasPreviousPage,
  totalItems,
  totalPages,
  onPageChange,
}: ClientPaginationProps) {
  return (
    <TablePagination
      currentPage={currentPage}
      totalPages={totalPages}
      hasPreviousPage={hasPreviousPage}
      hasNextPage={hasNextPage}
      summary={`${totalItems} cliente(s) encontrado(s)`}
      onPageChange={onPageChange}
    />
  );
}
