"use client";

import type { Dispatch, SetStateAction } from "react";

import { Card, CardBody, CardHeading } from "@/components/ui/card";
import { SelectField, TextInput } from "@/components/ui/field";
import { cx } from "@/lib/helpers/classnames";
import { formatCurrency } from "@/lib/helpers/formatters";
import type {
  ConsignmentPieceSummary,
  ConsignmentStatusOption,
  ConsignmentSupplierOption,
} from "@/lib/services/consignments";

import type { ConsignmentFiltersState } from "./types";

// Reune os filtros e a listagem principal das pecas consignadas.
type ConsignmentsListPanelProps = {
  busy: boolean;
  filters: ConsignmentFiltersState;
  items: ConsignmentPieceSummary[];
  onSelectPiece: (pieceId: string) => void;
  selectedPieceId: string;
  setFilters: Dispatch<SetStateAction<ConsignmentFiltersState>>;
  statuses: ConsignmentStatusOption[];
  suppliers: ConsignmentSupplierOption[];
};

export function ConsignmentsListPanel({
  busy,
  filters,
  items,
  onSelectPiece,
  selectedPieceId,
  setFilters,
  statuses,
  suppliers,
}: ConsignmentsListPanelProps) {
  return (
    <Card>
      <CardBody className="section-stack">
        <CardHeading
          subtitle="Filtre por fornecedor, status do ciclo e descontos automaticos ativos."
          title="Pecas consignadas"
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
            placeholder="Codigo, produto, marca ou fornecedor"
            value={filters.search}
          />

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
              label="Status do ciclo"
              onChange={(event) =>
                setFilters((current) => ({
                  ...current,
                  statusConsignacao: event.target.value,
                }))
              }
              value={filters.statusConsignacao}
            >
              <option value="">Todos</option>
              {statuses.map((status) => (
                <option key={status.codigo} value={status.codigo}>
                  {status.nome}
                </option>
              ))}
            </SelectField>
          </div>

          <div className="section-stack">
            <label className="rule-toggle-card">
              <input
                checked={filters.somenteProximasDoFim}
                disabled={busy}
                onChange={(event) =>
                  setFilters((current) => ({
                    ...current,
                    somenteProximasDoFim: event.target.checked,
                  }))
                }
                type="checkbox"
              />
              <div>
                <div className="selection-item-title">Somente proximas do fim</div>
                <div className="selection-item-copy">
                  Exibe apenas vencidas ou dentro da janela curta de alerta.
                </div>
              </div>
            </label>

            <label className="rule-toggle-card">
              <input
                checked={filters.somenteDescontoPendente}
                disabled={busy}
                onChange={(event) =>
                  setFilters((current) => ({
                    ...current,
                    somenteDescontoPendente: event.target.checked,
                  }))
                }
                type="checkbox"
              />
              <div>
                <div className="selection-item-title">Somente desconto automatico</div>
                <div className="selection-item-copy">
                  Exibe itens cujo desconto da consignacao ja deve ser considerado na venda.
                </div>
              </div>
            </label>
          </div>
        </div>

        <div className="record-list">
          {items.length === 0 ? (
            <div className="empty-state">
              Nenhuma peca consignada encontrada com os filtros atuais.
            </div>
          ) : (
            items.map((item) => (
              <button
                className={cx(
                  "record-item",
                  selectedPieceId === item.id && "catalogs-entry-row-active",
                )}
                key={item.id}
                onClick={() => onSelectPiece(item.id)}
                type="button"
              >
                <div className="selection-item-title">
                  {item.codigoInterno} | {item.produtoNome}
                </div>
                <div className="record-item-copy">
                  {item.marca} | {item.tamanho} | {item.cor}
                </div>
                <div className="record-item-copy">
                  {item.fornecedorNome ?? "Sem fornecedor"} | {formatCurrency(item.precoVendaAtual)}
                </div>
                <div className="record-tags">
                  <span className="record-tag">{item.statusConsignacao}</span>
                  <span className="record-tag">
                    {item.diasRestantes === null || item.diasRestantes === undefined
                      ? "encerrada"
                      : `${item.diasRestantes} dia(s)`}
                  </span>
                  {item.descontoPendente ? (
                    <span className="record-tag">desconto automatico</span>
                  ) : null}
                </div>
              </button>
            ))
          )}
        </div>
      </CardBody>
    </Card>
  );
}
