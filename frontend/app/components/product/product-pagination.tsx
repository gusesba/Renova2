import { TablePagination } from "@/app/components/ui/table-pagination";

type ProductPaginationProps = {
  currentPage: number;
  totalPages: number;
  totalItems: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
  onPageChange: (page: number) => void;
};

export function ProductPagination({
  currentPage,
  totalPages,
  totalItems,
  hasPreviousPage,
  hasNextPage,
  onPageChange,
}: ProductPaginationProps) {
  return (
    <TablePagination
      currentPage={currentPage}
      totalPages={totalPages}
      hasPreviousPage={hasPreviousPage}
      hasNextPage={hasNextPage}
      summary={`${totalItems} produto(s) encontrado(s)`}
      onPageChange={onPageChange}
    />
  );
}
