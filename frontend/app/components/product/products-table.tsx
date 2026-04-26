import {
  formatCurrencyValue,
  formatDateValue,
  formatSituacaoValue,
  type ProductListItem,
  type ProductVisibleField,
} from "@/lib/product";

type ProductsTableProps = {
  products: ProductListItem[];
  visibleFields: ProductVisibleField[];
  canEditProduct: boolean;
  canDeleteProduct: boolean;
  selectedProductIds: number[];
  onEditProduct: (product: ProductListItem) => void;
  onDeleteProduct: (product: ProductListItem) => void;
  onToggleProductSelection: (productId: number) => void;
  onToggleAllProducts: () => void;
};

function ProductTableCell({
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

function ConsignadoBadge({ value }: { value: boolean }) {
  return (
    <span
      className={`inline-flex rounded-full px-3 py-1 text-xs font-semibold ${
        value ? "bg-amber-100 text-amber-700" : "bg-slate-100 text-slate-600"
      }`}
    >
      {value ? "Sim" : "Nao"}
    </span>
  );
}

function EditIcon() {
  return (
    <svg
      aria-hidden="true"
      viewBox="0 0 24 24"
      className="h-4 w-4"
      fill="none"
      stroke="currentColor"
      strokeWidth="2"
      strokeLinecap="round"
      strokeLinejoin="round"
    >
      <path d="M12 20h9" />
      <path d="M16.5 3.5a2.12 2.12 0 1 1 3 3L7 19l-4 1 1-4 12.5-12.5Z" />
    </svg>
  );
}

function DeleteIcon() {
  return (
    <svg
      aria-hidden="true"
      viewBox="0 0 24 24"
      className="h-4 w-4"
      fill="none"
      stroke="currentColor"
      strokeWidth="2"
      strokeLinecap="round"
      strokeLinejoin="round"
    >
      <path d="M3 6h18" />
      <path d="M8 6V4h8v2" />
      <path d="M19 6l-1 14H6L5 6" />
      <path d="M10 11v6" />
      <path d="M14 11v6" />
    </svg>
  );
}

export function ProductsTable({
  products,
  visibleFields,
  canEditProduct,
  canDeleteProduct,
  selectedProductIds,
  onEditProduct,
  onDeleteProduct,
  onToggleProductSelection,
  onToggleAllProducts,
}: ProductsTableProps) {
  const showProduto = visibleFields.includes("produto");
  const showDescricao = visibleFields.includes("descricao");
  const showMarca = visibleFields.includes("marca");
  const showTamanho = visibleFields.includes("tamanho");
  const showCor = visibleFields.includes("cor");
  const showFornecedor = visibleFields.includes("fornecedor");
  const showPreco = visibleFields.includes("preco");
  const showEntrada = visibleFields.includes("entrada");
  const showSituacao = visibleFields.includes("situacao");
  const showConsignado = visibleFields.includes("consignado");
  const showId = visibleFields.includes("id");

  const allVisibleSelected =
    products.length > 0 && products.every((product) => selectedProductIds.includes(product.id));

  return (
    <div className="mt-6 overflow-hidden rounded-[24px] border border-[var(--border)]">
      <div className="overflow-x-auto">
        <table className="min-w-full border-collapse bg-white">
          <thead className="bg-[var(--surface-muted)]">
            <tr className="text-left">
              <th className="w-14 px-4 py-4 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                <input
                  type="checkbox"
                  checked={allVisibleSelected}
                  onChange={onToggleAllProducts}
                  aria-label="Selecionar produtos visiveis para impressao"
                  className="h-4 w-4 cursor-pointer rounded border-[var(--border)]"
                />
              </th>
              {showProduto ? (
                <th className="px-4 py-4 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                  Produto
                </th>
              ) : null}
              {showDescricao ? (
                <th className="px-4 py-4 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                  Descricao
                </th>
              ) : null}
              {showMarca ? (
                <th className="px-4 py-4 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                  Marca
                </th>
              ) : null}
              {showTamanho ? (
                <th className="px-4 py-4 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                  Tamanho
                </th>
              ) : null}
              {showCor ? (
                <th className="px-4 py-4 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                  Cor
                </th>
              ) : null}
              {showFornecedor ? (
                <th className="px-4 py-4 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                  Fornecedor
                </th>
              ) : null}
              {showPreco ? (
                <th className="px-4 py-4 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                  Preco
                </th>
              ) : null}
              {showEntrada ? (
                <th className="px-4 py-4 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                  Entrada
                </th>
              ) : null}
              {showSituacao ? (
                <th className="px-4 py-4 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                  Situacao
                </th>
              ) : null}
              {showConsignado ? (
                <th className="px-4 py-4 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                  Consignado
                </th>
              ) : null}
              {showId ? (
                <th className="px-4 py-4 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                  Identificador
                </th>
              ) : null}
              <th className="px-4 py-4 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                Acoes
              </th>
            </tr>
          </thead>
          <tbody>
            {products.map((product, index) => (
              <tr
                key={product.id}
                className={
                  index % 2 === 0
                    ? "bg-white"
                    : "bg-[color:color-mix(in_srgb,var(--surface-muted)_55%,white)]"
                }
              >
                <ProductTableCell>
                  <input
                    type="checkbox"
                    checked={selectedProductIds.includes(product.id)}
                    onChange={() => onToggleProductSelection(product.id)}
                    aria-label={`Selecionar produto ${product.id} para impressao`}
                    className="h-4 w-4 cursor-pointer rounded border-[var(--border)]"
                  />
                </ProductTableCell>
                {showProduto ? (
                  <ProductTableCell>
                    <div className="flex items-center gap-3">
                      <div className="flex h-10 w-10 items-center justify-center rounded-2xl bg-[var(--primary-soft)] text-sm font-semibold text-[var(--primary)]">
                        {product.produto
                          .split(" ")
                          .filter(Boolean)
                          .slice(0, 2)
                          .map((part) => part[0]?.toUpperCase())
                          .join("")}
                      </div>
                      <div>
                        <p className="font-semibold text-[var(--foreground)]">{product.produto}</p>
                      </div>
                    </div>
                  </ProductTableCell>
                ) : null}
                {showDescricao ? <ProductTableCell>{product.descricao}</ProductTableCell> : null}
                {showMarca ? <ProductTableCell>{product.marca}</ProductTableCell> : null}
                {showTamanho ? <ProductTableCell subtle>{product.tamanho}</ProductTableCell> : null}
                {showCor ? <ProductTableCell subtle>{product.cor}</ProductTableCell> : null}
                {showFornecedor ? <ProductTableCell>{product.fornecedor}</ProductTableCell> : null}
                {showPreco ? (
                  <ProductTableCell>{formatCurrencyValue(product.preco)}</ProductTableCell>
                ) : null}
                {showEntrada ? (
                  <ProductTableCell subtle>{formatDateValue(product.entrada)}</ProductTableCell>
                ) : null}
                {showSituacao ? (
                  <ProductTableCell subtle>{formatSituacaoValue(product.situacao)}</ProductTableCell>
                ) : null}
                {showConsignado ? (
                  <ProductTableCell>
                    <ConsignadoBadge value={product.consignado} />
                  </ProductTableCell>
                ) : null}
                {showId ? <ProductTableCell subtle>#{product.id}</ProductTableCell> : null}
                <ProductTableCell>
                  <div className="flex items-center gap-2">
                    {canEditProduct ? (
                      <button
                        type="button"
                        onClick={() => onEditProduct(product)}
                        className="inline-flex h-10 w-10 cursor-pointer items-center justify-center rounded-2xl border border-emerald-200 bg-emerald-50 text-emerald-600 transition hover:border-emerald-300 hover:bg-emerald-100 hover:text-emerald-700"
                        aria-label={`Editar produto ${product.descricao}`}
                        title={`Editar produto ${product.descricao}`}
                      >
                        <EditIcon />
                      </button>
                    ) : null}
                    {canDeleteProduct ? (
                      <button
                        type="button"
                        onClick={() => onDeleteProduct(product)}
                        className="inline-flex h-10 w-10 cursor-pointer items-center justify-center rounded-2xl border border-rose-200 bg-rose-50 text-rose-600 transition hover:border-rose-300 hover:bg-rose-100 hover:text-rose-700"
                        aria-label={`Excluir produto ${product.descricao}`}
                        title={`Excluir produto ${product.descricao}`}
                      >
                        <DeleteIcon />
                      </button>
                    ) : null}
                  </div>
                </ProductTableCell>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
