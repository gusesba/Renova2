import { startTransition, type Dispatch, type SetStateAction } from "react";

import { Card, CardBody, CardHeading } from "@/components/ui/card";
import { SelectField, TextInput } from "@/components/ui/field";
import { StatusBadge } from "@/components/ui/status-badge";
import {
  formatCurrency,
  formatDateTime,
  formatStatus,
} from "@/lib/helpers/formatters";
import type {
  FinancialLedgerEntry,
  FinancialWorkspace,
} from "@/lib/services/financial";

import {
  describeLedgerEntry,
  type FinancialFiltersState,
} from "@/app/(system)/financial/components/types";

type FinancialLedgerPanelProps = {
  entries: FinancialLedgerEntry[];
  filters: FinancialFiltersState;
  setFilters: Dispatch<SetStateAction<FinancialFiltersState>>;
  workspace?: FinancialWorkspace;
};

// Lista o livro razao com filtros simples de periodo, tipo e meio de pagamento.
export function FinancialLedgerPanel({
  entries,
  filters,
  setFilters,
  workspace,
}: FinancialLedgerPanelProps) {
  return (
    <Card>
      <CardBody className="section-stack">
        <CardHeading
          subtitle="Consolida recebimentos, pagamentos, ajustes e estornos na loja ativa."
          title="Livro razao"
        />

        <div className="split-fields">
          <TextInput
            label="Busca rapida"
            onChange={(event) => {
              const nextValue = event.target.value;
              startTransition(() =>
                setFilters((current) => ({ ...current, search: nextValue })),
              );
            }}
            placeholder="Descricao, venda, fornecedor ou usuario"
            value={filters.search}
          />
          <SelectField
            label="Meio de pagamento"
            onChange={(event) =>
              setFilters((current) => ({
                ...current,
                meioPagamentoId: event.target.value,
              }))
            }
            value={filters.meioPagamentoId}
          >
            <option value="">Todos</option>
            {(workspace?.meiosPagamento ?? []).map((method) => (
              <option key={method.id} value={method.id}>
                {method.nome}
              </option>
            ))}
          </SelectField>
        </div>

        <div className="split-fields">
          <SelectField
            label="Tipo de movimento"
            onChange={(event) =>
              setFilters((current) => ({
                ...current,
                tipoMovimentacao: event.target.value,
              }))
            }
            value={filters.tipoMovimentacao}
          >
            <option value="">Todos</option>
            {(workspace?.tiposMovimentacao ?? []).map((item) => (
              <option key={item.codigo} value={item.codigo}>
                {item.nome}
              </option>
            ))}
          </SelectField>
          <SelectField
            label="Direcao"
            onChange={(event) =>
              setFilters((current) => ({
                ...current,
                direcao: event.target.value,
              }))
            }
            value={filters.direcao}
          >
            <option value="">Todas</option>
            {(workspace?.direcoes ?? []).map((item) => (
              <option key={item.codigo} value={item.codigo}>
                {item.nome}
              </option>
            ))}
          </SelectField>
        </div>

        <div className="split-fields">
          <TextInput
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

        <div className="record-list">
          {entries.length === 0 ? (
            <div className="empty-state">
              Nenhum movimento financeiro encontrado com os filtros atuais.
            </div>
          ) : (
            entries.map((entry) => (
              <div className="record-item" key={entry.id}>
                <div className="stock-record-header">
                  <div>
                    <div className="selection-item-title">
                      {describeLedgerEntry(entry)}
                    </div>
                    <div className="record-item-copy">{entry.descricao}</div>
                  </div>

                  <div className="stock-record-meta">
                    <StatusBadge value={entry.direcao} />
                    <StatusBadge value={entry.tipoMovimentacao} />
                  </div>
                </div>

                <div className="record-tags">
                  <span className="record-tag">
                    Bruto {formatCurrency(entry.valorBruto)}
                  </span>
                  <span className="record-tag">
                    Taxa {formatCurrency(entry.taxa)}
                  </span>
                  <span className="record-tag">
                    Liquido {formatCurrency(entry.valorLiquido)}
                  </span>
                  <span className="record-tag">
                    Origem {formatStatus(entry.origemTipo)}
                  </span>
                </div>

                <div className="record-item-copy">
                  {entry.meioPagamentoNome
                    ? `Meio ${entry.meioPagamentoNome}`
                    : "Sem meio de pagamento"}
                </div>
                <div className="record-item-copy">
                  Usuario {entry.movimentadoPorUsuarioNome} em{" "}
                  {formatDateTime(entry.movimentadoEm)}
                </div>
                <div className="record-item-copy">
                  Competencia {formatDateTime(entry.competenciaEm)}
                </div>
              </div>
            ))
          )}
        </div>
      </CardBody>
    </Card>
  );
}
