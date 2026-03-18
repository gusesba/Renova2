import { type Dispatch, type SetStateAction } from "react";

import { Button } from "@/components/ui/button";
import { Card, CardBody, CardHeading } from "@/components/ui/card";
import { SelectField, TextInput } from "@/components/ui/field";
import { StatusBadge } from "@/components/ui/status-badge";
import { formatCurrency, formatDateTime } from "@/lib/helpers/formatters";
import type {
  SupplierObligationSummary,
  SupplierPaymentOption,
  SupplierPaymentSupplierOption,
} from "@/lib/services/supplier-payments";

import type { SupplierPaymentFiltersState } from "@/app/(system)/supplier-payments/components/types";

type SupplierObligationsListPanelProps = {
  busy: boolean;
  filters: SupplierPaymentFiltersState;
  obligations: SupplierObligationSummary[];
  statuses: SupplierPaymentOption[];
  suppliers: SupplierPaymentSupplierOption[];
  types: SupplierPaymentOption[];
  selectedObligationId: string;
  setFilters: Dispatch<SetStateAction<SupplierPaymentFiltersState>>;
  onSelectObligation: (obligationId: string) => void;
};

// Lista as obrigacoes da loja ativa e aplica filtros operacionais.
export function SupplierObligationsListPanel({
  busy,
  filters,
  obligations,
  statuses,
  suppliers,
  types,
  selectedObligationId,
  setFilters,
  onSelectObligation,
}: SupplierObligationsListPanelProps) {
  return (
    <Card>
      <CardBody className="section-stack">
        <CardHeading
          subtitle="Consulte pendencias por fornecedor, tipo e status da obrigacao."
          title="Obrigacoes do fornecedor"
        />

        <div className="form-grid">
          <TextInput
            disabled={busy}
            label="Busca"
            onChange={(event) =>
              setFilters((current) => ({
                ...current,
                search: event.target.value,
              }))
            }
            placeholder="Fornecedor, venda, codigo da peca..."
            value={filters.search}
          />

          <div className="split-fields">
            <SelectField
              disabled={busy}
              label="Fornecedor"
              onChange={(event) =>
                setFilters((current) => ({
                  ...current,
                  pessoaId: event.target.value,
                }))
              }
              value={filters.pessoaId}
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
              label="Status"
              onChange={(event) =>
                setFilters((current) => ({
                  ...current,
                  statusObrigacao: event.target.value,
                }))
              }
              value={filters.statusObrigacao}
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
              label="Tipo da obrigacao"
              onChange={(event) =>
                setFilters((current) => ({
                  ...current,
                  tipoObrigacao: event.target.value,
                }))
              }
              value={filters.tipoObrigacao}
            >
              <option value="">Todos</option>
              {types.map((type) => (
                <option key={type.codigo} value={type.codigo}>
                  {type.nome}
                </option>
              ))}
            </SelectField>

            <div style={{ alignSelf: "end" }}>
              <Button
                disabled={busy}
                onClick={() =>
                  setFilters({
                    pessoaId: "",
                    search: "",
                    statusObrigacao: "",
                    tipoObrigacao: "",
                  })
                }
                type="button"
                variant="ghost"
              >
                Limpar filtros
              </Button>
            </div>
          </div>
        </div>

        <div className="record-list">
          {obligations.length === 0 ? (
            <div className="empty-state">
              Nenhuma obrigacao encontrada para os filtros atuais.
            </div>
          ) : (
            obligations.map((obligation) => (
              <button
                className={`record-item${selectedObligationId === obligation.id ? " record-item-active" : ""}`}
                key={obligation.id}
                onClick={() => onSelectObligation(obligation.id)}
                type="button"
              >
                <div className="stock-record-header">
                  <div>
                    <div className="selection-item-title">
                      {obligation.fornecedorNome}
                    </div>
                    <div className="record-item-copy">
                      {obligation.codigoInternoPeca
                        ? `${obligation.codigoInternoPeca} • ${obligation.produtoNomePeca}`
                        : obligation.fornecedorDocumento}
                    </div>
                  </div>

                  <StatusBadge value={obligation.statusObrigacao} />
                </div>

                <div className="record-item-copy">
                  {obligation.numeroVenda
                    ? `Venda ${obligation.numeroVenda}`
                    : obligation.fornecedorDocumento}
                </div>

                <div className="record-tags">
                  <span className="record-tag">
                    Original {formatCurrency(obligation.valorOriginal)}
                  </span>
                  <span className="record-tag">
                    Em aberto {formatCurrency(obligation.valorEmAberto)}
                  </span>
                  <span className="record-tag">
                    Liquidado {formatCurrency(obligation.valorLiquidado)}
                  </span>
                </div>

                <div className="record-item-copy">
                  Gerada em {formatDateTime(obligation.dataGeracao)}
                </div>
              </button>
            ))
          )}
        </div>
      </CardBody>
    </Card>
  );
}
