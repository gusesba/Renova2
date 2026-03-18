import { type Dispatch, type SetStateAction } from "react";

import { Button } from "@/components/ui/button";
import { Card, CardBody, CardHeading } from "@/components/ui/card";
import { SelectField, TextInput } from "@/components/ui/field";
import { cx } from "@/lib/helpers/classnames";
import { formatDateTime, formatStatus, getStatusTone } from "@/lib/helpers/formatters";
import type {
  StockOption,
  StockPieceLookup,
  StockSupplierOption,
} from "@/lib/services/stock-movements";

import type { StockPieceSearchFiltersState } from "@/app/(system)/stock-movements/components/types";

// Renderiza a busca operacional de pecas usada pelo ajuste manual e pela consulta rapida.
type StockPieceSearchPanelProps = {
  busy: boolean;
  filters: StockPieceSearchFiltersState;
  items: StockPieceLookup[];
  onApplyMovementFilter: (pieceId: string) => void;
  onSelectPiece: (pieceId: string) => void;
  selectedPieceId: string;
  setFilters: Dispatch<SetStateAction<StockPieceSearchFiltersState>>;
  statuses: StockOption[];
  suppliers: StockSupplierOption[];
};

export function StockPieceSearchPanel({
  busy,
  filters,
  items,
  onApplyMovementFilter,
  onSelectPiece,
  selectedPieceId,
  setFilters,
  statuses,
  suppliers,
}: StockPieceSearchPanelProps) {
  return (
    <Card>
      <CardBody className="section-stack">
        <CardHeading
          subtitle="Localize pecas por codigo de barras, nome, marca, fornecedor, status ou tempo em loja."
          title="Busca operacional de pecas"
        />

        <div className="form-grid">
          <TextInput
            disabled={busy}
            label="Busca geral"
            onChange={(event) =>
              setFilters((current) => ({
                ...current,
                search: event.target.value,
              }))
            }
            placeholder="Codigo, produto, marca ou localizacao"
            value={filters.search}
          />

          <div className="split-fields">
            <TextInput
              disabled={busy}
              label="Codigo de barras"
              onChange={(event) =>
                setFilters((current) => ({
                  ...current,
                  codigoBarras: event.target.value,
                }))
              }
              placeholder="Leitura rapida"
              value={filters.codigoBarras}
            />

            <TextInput
              disabled={busy}
              label="Tempo minimo na loja"
              onChange={(event) =>
                setFilters((current) => ({
                  ...current,
                  tempoMinimoLojaDias: event.target.value,
                }))
              }
              placeholder="Dias"
              type="number"
              value={filters.tempoMinimoLojaDias}
            />
          </div>

          <div className="split-fields">
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

            <SelectField
              disabled={busy}
              label="Status da peca"
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
          </div>
        </div>

        <div className="stock-piece-actions">
          <Button
            disabled={busy || !selectedPieceId}
            onClick={() => onApplyMovementFilter(selectedPieceId)}
            variant="soft"
          >
            Ver movimentacoes da peca
          </Button>

          <Button
            disabled={busy}
            onClick={() =>
              setFilters({
                codigoBarras: "",
                fornecedorPessoaId: "",
                search: "",
                statusPeca: "",
                tempoMinimoLojaDias: "",
              })
            }
            variant="ghost"
          >
            Limpar busca
          </Button>
        </div>

        <div className="record-list">
          {items.length === 0 ? (
            <div className="empty-state">
              Nenhuma peca localizada para os filtros informados.
            </div>
          ) : (
            items.map((item) => (
              <button
                className={cx(
                  "record-item",
                  selectedPieceId === item.id && "record-item-active",
                )}
                key={item.id}
                onClick={() => onSelectPiece(item.id)}
                type="button"
              >
                <div className="stock-record-header">
                  <div>
                    <strong>
                      {item.codigoInterno} • {item.produtoNome}
                    </strong>
                    <div className="record-item-copy">
                      {item.marca} / {item.tamanho} / {item.cor}
                    </div>
                  </div>

                  <div className="stock-record-meta">
                    <span
                      className="status-badge"
                      data-tone={getStatusTone(item.statusPeca)}
                    >
                      {formatStatus(item.statusPeca)}
                    </span>
                    <span
                      className="status-badge"
                      data-tone={item.disponivelParaVenda ? "success" : "warning"}
                    >
                      {item.disponivelParaVenda
                        ? "Apta para venda"
                        : "Sem saldo p/ venda"}
                    </span>
                  </div>
                </div>

                <div className="record-item-copy">
                  {item.fornecedorNome ?? "Sem fornecedor"} • Ultima mov.:{" "}
                  {formatDateTime(item.ultimaMovimentacaoEm)}
                </div>

                <div className="record-tags">
                  <span className="record-tag">Saldo: {item.quantidadeAtual}</span>
                  <span className="record-tag">Dias na loja: {item.diasEmLoja}</span>
                  <span className="record-tag">{formatStatus(item.tipoPeca)}</span>
                  <span className="record-tag">{item.localizacaoFisica}</span>
                </div>
              </button>
            ))
          )}
        </div>
      </CardBody>
    </Card>
  );
}
