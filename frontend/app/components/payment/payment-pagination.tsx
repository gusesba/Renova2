import { TablePagination } from "@/app/components/ui/table-pagination";

type PaymentPaginationProps = {
  currentPage: number;
  totalPages: number;
  totalItems: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
  onPageChange: (page: number) => void;
  itemLabel?: string;
};

export function PaymentPagination({
  currentPage,
  totalPages,
  totalItems,
  hasPreviousPage,
  hasNextPage,
  onPageChange,
  itemLabel = "pagamento(s) encontrado(s)",
}: PaymentPaginationProps) {
  return (
    <TablePagination
      currentPage={currentPage}
      totalPages={totalPages}
      hasPreviousPage={hasPreviousPage}
      hasNextPage={hasNextPage}
      summary={`${totalItems} ${itemLabel}`}
      onPageChange={onPageChange}
    />
  );
}
