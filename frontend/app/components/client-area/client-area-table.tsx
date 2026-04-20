import {
  formatCurrencyValue,
  formatDateValue,
  formatSituacaoValue,
} from "@/lib/product";
import type {
  ClientAreaProductItem,
  ClientAreaVisibleField,
} from "@/lib/client-area";

type ClientAreaTableProps = {
  products: ClientAreaProductItem[];
  visibleFields: ClientAreaVisibleField[];
};

function Cell({
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

export function ClientAreaTable({ products, visibleFields }: ClientAreaTableProps) {
  const showLoja = visibleFields.includes("loja");
  const showProduto = visibleFields.includes("produto");
  const showDescricao = visibleFields.includes("descricao");
  const showMarca = visibleFields.includes("marca");
  const showTamanho = visibleFields.includes("tamanho");
  const showCor = visibleFields.includes("cor");
  const showPreco = visibleFields.includes("preco");
  const showEntrada = visibleFields.includes("entrada");
  const showSituacao = visibleFields.includes("situacao");
  const showId = visibleFields.includes("id");

  return (
    <div className="mt-6 overflow-hidden rounded-[24px] border border-[var(--border)]">
      <div className="overflow-x-auto">
        <table className="min-w-full border-collapse bg-white">
          <thead className="bg-[var(--surface-muted)]">
            <tr className="text-left">
              {showLoja ? (
                <th className="px-4 py-4 text-xs font-semibold uppercase tracking-[0.14em] text-[var(--muted)]">
                  Loja
                </th>
              ) : null}
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
                key={`${product.storeName}-${product.id}`}
                className={
                  index % 2 === 0
                    ? "bg-white"
                    : "bg-[color:color-mix(in_srgb,var(--surface-muted)_55%,white)]"
                }
              >
                {showLoja ? <Cell>{product.storeName}</Cell> : null}
                {showProduto ? <Cell>{product.produto}</Cell> : null}
                {showDescricao ? <Cell>{product.descricao}</Cell> : null}
                {showMarca ? <Cell>{product.marca}</Cell> : null}
                {showTamanho ? <Cell subtle>{product.tamanho}</Cell> : null}
                {showCor ? <Cell subtle>{product.cor}</Cell> : null}
                {showPreco ? <Cell>{formatCurrencyValue(product.preco)}</Cell> : null}
                {showEntrada ? <Cell subtle>{formatDateValue(product.entrada)}</Cell> : null}
                {showSituacao ? <Cell subtle>{formatSituacaoValue(product.situacao)}</Cell> : null}
                {showId ? <Cell subtle>#{product.id}</Cell> : null}
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
