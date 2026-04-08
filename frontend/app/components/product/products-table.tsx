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

export function ProductsTable({ products, visibleFields }: ProductsTableProps) {
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

  return (
    <div className="mt-6 overflow-hidden rounded-[24px] border border-[var(--border)]">
      <div className="overflow-x-auto">
        <table className="min-w-full border-collapse bg-white">
          <thead className="bg-[var(--surface-muted)]">
            <tr className="text-left">
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
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
