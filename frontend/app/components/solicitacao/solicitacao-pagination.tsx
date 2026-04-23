import { TablePagination } from "@/app/components/ui/table-pagination";

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
    <TablePagination
      currentPage={currentPage}
      totalPages={totalPages}
      hasPreviousPage={hasPreviousPage}
      hasNextPage={hasNextPage}
      summary={`${totalItems} registro(s) encontrado(s)`}
      className="mt-6"
      onPageChange={onPageChange}
    />
  );
}
