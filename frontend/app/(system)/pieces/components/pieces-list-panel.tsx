import type { Dispatch, SetStateAction } from "react";

import { Button } from "@/components/ui/button";
import { Card, CardBody, CardHeading } from "@/components/ui/card";
import { SelectField, TextInput } from "@/components/ui/field";
import { cx } from "@/lib/helpers/classnames";
import type {
  PieceCatalogOption,
  PieceSummary,
  PieceSupplierOption,
} from "@/lib/services/pieces";

import type { PieceFiltersState } from "./types";

// Reune os filtros e a listagem principal de pecas da loja ativa.
type PiecesListPanelProps = {
  busy: boolean;
  canManage: boolean;
  filters: PieceFiltersState;
  onNewPiece: () => void;
  onSelectPiece: (pieceId: string) => void;
  pieces: PieceSummary[];
  selectedPieceId: string;
  setFilters: Dispatch<SetStateAction<PieceFiltersState>>;
  suppliers: PieceSupplierOption[];
  productNames: PieceCatalogOption[];
  brands: PieceCatalogOption[];
  statuses: Array<{ codigo: string; nome: string }>;
};

export function PiecesListPanel({
  busy,
  canManage,
  filters,
  onNewPiece,
  onSelectPiece,
  pieces,
  selectedPieceId,
  setFilters,
  suppliers,
  productNames,
  brands,
  statuses,
}: PiecesListPanelProps) {
  return (
    <Card>
      <CardBody className="section-stack">
        <CardHeading
          subtitle="Use os filtros rapidos para localizar por codigo, catalogo, fornecedor ou status."
          title="Consulta de pecas"
        />

        <div className="form-grid">
          <div className="split-fields">
            <TextInput
              disabled={busy}
              label="Busca geral"
              onChange={(event) =>
                setFilters((current) => ({
                  ...current,
                  search: event.target.value,
                }))
              }
              placeholder="Codigo, produto, marca, fornecedor..."
              value={filters.search}
            />
            <TextInput
              disabled={busy}
              label="Codigo de barras"
              onChange={(event) =>
                setFilters((current) => ({
                  ...current,
                  codigoBarras: event.target.value,
                }))
              }
              value={filters.codigoBarras}
            />
          </div>

          <div className="split-fields">
            <SelectField
              disabled={busy}
              label="Status"
              onChange={(event) =>
                setFilters((current) => ({
                  ...current,
                  statusPeca: event.target.value,
                }))
              }
              value={filters.statusPeca}
            >
              <option value="">Todos</option>
              {statuses.map((status) => (
                <option key={status.codigo} value={status.codigo}>
                  {status.nome}
                </option>
              ))}
            </SelectField>

            <SelectField
              disabled={busy}
              label="Produto"
              onChange={(event) =>
                setFilters((current) => ({
                  ...current,
                  produtoNomeId: event.target.value,
                }))
              }
              value={filters.produtoNomeId}
            >
              <option value="">Todos</option>
              {productNames.map((product) => (
                <option key={product.id} value={product.id}>
                  {product.nome}
                </option>
              ))}
            </SelectField>
          </div>

          <div className="split-fields">
            <SelectField
              disabled={busy}
              label="Marca"
              onChange={(event) =>
                setFilters((current) => ({
                  ...current,
                  marcaId: event.target.value,
                }))
              }
              value={filters.marcaId}
            >
              <option value="">Todas</option>
              {brands.map((brand) => (
                <option key={brand.id} value={brand.id}>
                  {brand.nome}
                </option>
              ))}
            </SelectField>

            <SelectField
              disabled={busy}
              label="Fornecedor"
              onChange={(event) =>
                setFilters((current) => ({
                  ...current,
                  fornecedorPessoaId: event.target.value,
                }))
              }
              value={filters.fornecedorPessoaId}
            >
              <option value="">Todos</option>
              {suppliers.map((supplier) => (
                <option key={supplier.pessoaId} value={supplier.pessoaId}>
                  {supplier.nome}
                </option>
              ))}
            </SelectField>
          </div>

          <Button
            disabled={busy || !canManage}
            onClick={onNewPiece}
            type="button"
            variant="ghost"
          >
            Nova peca
          </Button>
        </div>

        <div className="record-list">
          {pieces.length === 0 ? (
            <div className="empty-state">
              Nenhuma peca encontrada com os filtros atuais.
            </div>
          ) : (
            pieces.map((piece) => (
              <button
                className={cx(
                  "record-item",
                  selectedPieceId === piece.id && "catalogs-entry-row-active",
                )}
                key={piece.id}
                onClick={() => onSelectPiece(piece.id)}
                type="button"
              >
                <div className="selection-item-title">
                  {piece.codigoInterno} | {piece.produtoNome}
                </div>
                <div className="record-item-copy">
                  {piece.marca} | {piece.tamanho} | {piece.cor}
                </div>
                <div className="record-item-copy">
                  {piece.fornecedorNome ?? "Sem fornecedor"} | {piece.localizacaoFisica}
                </div>
                <div className="record-tags">
                  <span className="record-tag">{piece.tipoPeca}</span>
                  <span className="record-tag">{piece.statusPeca}</span>
                  <span className="record-tag">Qtd. {piece.quantidadeAtual}</span>
                </div>
              </button>
            ))
          )}
        </div>
      </CardBody>
    </Card>
  );
}
