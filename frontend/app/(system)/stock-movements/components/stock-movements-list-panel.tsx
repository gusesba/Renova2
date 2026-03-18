import { type Dispatch, type SetStateAction } from "react";

import { Button } from "@/components/ui/button";
import { Card, CardBody, CardHeading } from "@/components/ui/card";
import { SelectField, TextInput } from "@/components/ui/field";
import { formatDateTime, formatStatus, getStatusTone } from "@/lib/helpers/formatters";
import type {
  StockMovementItem,
  StockOption,
  StockSupplierOption,
} from "@/lib/services/stock-movements";

import type { StockMovementFiltersState } from "@/app/(system)/stock-movements/components/types";

// Renderiza a listagem principal de movimentacoes com filtros por peca e periodo.
type StockMovementsListPanelProps = {
  busy: boolean;
  filters: StockMovementFiltersState;
  items: StockMovementItem[];
  movementTypes: StockOption[];
  setFilters: Dispatch<SetStateAction<StockMovementFiltersState>>;
  statuses: StockOption[];
  suppliers: StockSupplierOption[];
};

export function StockMovementsListPanel({
  busy,
  filters,
  items,
  movementTypes,
  setFilters,
  statuses,
  suppliers,
}: StockMovementsListPanelProps) {
  return (
    <Card>
      <CardBody className="section-stack">
        <CardHeading
          subtitle="Consulta por peca, loja ativa e periodo. Use a busca operacional ao lado para filtrar uma peca especifica."
          title="Historico de movimentacoes"
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
            placeholder="Codigo, produto, marca, motivo..."
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

          <div className="split-fields">
            <SelectField
              disabled={busy}
              label="Tipo de movimentacao"
              onChange={(event) =>
                setFilters((current) => ({
                  ...current,
                  tipoMovimentacao: event.target.value,
                }))
              }
              value={filters.tipoMovimentacao}
            >
              <option value="">Todos</option>
              {movementTypes.map((movementType) => (
                <option key={movementType.codigo} value={movementType.codigo}>
                  {movementType.nome}
                </option>
              ))}
            </SelectField>

            <TextInput
              disabled={busy}
              label="Peca filtrada"
              onChange={(event) =>
                setFilters((current) => ({
                  ...current,
                  pecaId: event.target.value,
                }))
              }
              placeholder="GUID da peca"
              value={filters.pecaId}
            />
          </div>

          <div className="split-fields">
            <TextInput
              disabled={busy}
              label="Data inicial"
              onChange={(event) =>
                setFilters((current) => ({
                  ...current,
                  dataInicial: event.target.value,
                }))
              }
              type="date"
              value={filters.dataInicial}
            />

            <TextInput
              disabled={busy}
              label="Data final"
              onChange={(event) =>
                setFilters((current) => ({
                  ...current,
                  dataFinal: event.target.value,
                }))
              }
              type="date"
              value={filters.dataFinal}
            />
          </div>
        </div>

        <div className="rule-inline-header">
          <div className="ui-banner">
            {filters.pecaId
              ? "A listagem esta filtrada por uma peca especifica."
              : "A consulta sempre considera a loja ativa da sessao."}
          </div>

          <Button
            disabled={busy}
            onClick={() =>
              setFilters({
                dataFinal: "",
                dataInicial: "",
                fornecedorPessoaId: "",
                pecaId: "",
                search: "",
                statusPeca: "",
                tipoMovimentacao: "",
              })
            }
            variant="ghost"
          >
            Limpar filtros
          </Button>
        </div>

        <div className="record-list">
          {items.length === 0 ? (
            <div className="empty-state">
              Nenhuma movimentacao encontrada com os filtros atuais.
            </div>
          ) : (
            items.map((item) => (
              <div className="record-item" key={item.id}>
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
                      data-tone={getStatusTone(item.tipoMovimentacao)}
                    >
                      {formatStatus(item.tipoMovimentacao)}
                    </span>
                    <span
                      className="status-badge"
                      data-tone={getStatusTone(item.statusPeca)}
                    >
                      {formatStatus(item.statusPeca)}
                    </span>
                  </div>
                </div>

                <div className="record-item-copy">
                  {item.fornecedorNome ?? "Sem fornecedor"} •{" "}
                  {formatDateTime(item.movimentadoEm)}
                </div>

                <div className="record-tags">
                  <span className="record-tag">Qtd. movida: {item.quantidade}</span>
                  <span className="record-tag">
                    Saldo {item.saldoAnterior} → {item.saldoPosterior}
                  </span>
                  <span className="record-tag">Saldo atual: {item.quantidadeAtualPeca}</span>
                  <span className="record-tag">Dias na loja: {item.diasEmLoja}</span>
                </div>

                <div className="record-item-copy" style={{ marginTop: "0.8rem" }}>
                  {item.motivo}
                </div>
              </div>
            ))
          )}
        </div>
      </CardBody>
    </Card>
  );
}
