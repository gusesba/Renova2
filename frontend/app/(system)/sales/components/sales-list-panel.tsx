import type { Dispatch, SetStateAction } from "react";

import { Card, CardBody, CardHeading } from "@/components/ui/card";
import { SelectField, TextInput } from "@/components/ui/field";
import { StatusBadge } from "@/components/ui/status-badge";
import { formatCurrency, formatDateTime } from "@/lib/helpers/formatters";
import type { SaleBuyerOption, SaleOption, SaleSummary } from "@/lib/services/sales";

import type { SaleFiltersState } from "./types";

type SalesListPanelProps = {
  busy: boolean;
  buyers: SaleBuyerOption[];
  filters: SaleFiltersState;
  items: SaleSummary[];
  selectedSaleId: string;
  statuses: SaleOption[];
  setFilters: Dispatch<SetStateAction<SaleFiltersState>>;
  onSelectSale: (saleId: string) => void;
};

// Exibe os filtros e a lista navegavel das vendas retornadas pela API.
export function SalesListPanel({
  busy,
  buyers,
  filters,
  items,
  selectedSaleId,
  statuses,
  setFilters,
  onSelectSale,
}: SalesListPanelProps) {
  return (
    <Card>
      <CardBody>
        <CardHeading
          subtitle="Consulte vendas ja registradas, filtre por periodo e abra o detalhe para cancelar ou imprimir."
          title="Historico de vendas"
        />

        <div className="form-grid">
          <TextInput
            label="Busca"
            onChange={(event) =>
              setFilters((current) => ({
                ...current,
                search: event.target.value,
              }))
            }
            placeholder="Numero da venda, comprador ou vendedor"
            value={filters.search}
          />

          <div className="split-fields">
            <SelectField
              label="Status"
              onChange={(event) =>
                setFilters((current) => ({
                  ...current,
                  statusVenda: event.target.value,
                }))
              }
              value={filters.statusVenda}
            >
              <option value="">Todos</option>
              {statuses.map((status) => (
                <option key={status.codigo} value={status.codigo}>
                  {status.nome}
                </option>
              ))}
            </SelectField>

            <SelectField
              label="Comprador"
              onChange={(event) =>
                setFilters((current) => ({
                  ...current,
                  compradorPessoaId: event.target.value,
                }))
              }
              value={filters.compradorPessoaId}
            >
              <option value="">Todos</option>
              {buyers.map((buyer) => (
                <option key={buyer.pessoaId} value={buyer.pessoaId}>
                  {buyer.nome}
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
        </div>

        <div className="record-list">
          {items.length === 0 ? (
            <div className="empty-state">
              {busy ? "Carregando vendas..." : "Nenhuma venda encontrada no filtro atual."}
            </div>
          ) : null}

          {items.map((sale) => (
            <button
              className={`record-item ${selectedSaleId === sale.id ? "record-item-active" : ""}`}
              key={sale.id}
              onClick={() => onSelectSale(sale.id)}
              type="button"
            >
              <div className="stock-record-header">
                <div>
                  <div className="selection-item-title">{sale.numeroVenda}</div>
                  <div className="record-item-copy">
                    {sale.compradorNome ?? "Venda sem comprador"} • {sale.vendedorNome}
                  </div>
                </div>

                <div className="stock-record-meta">
                  <StatusBadge value={sale.statusVenda} />
                </div>
              </div>

              <div className="record-tags">
                <span className="record-tag">{formatDateTime(sale.dataHoraVenda)}</span>
                <span className="record-tag">
                  {sale.quantidadeItens} item{sale.quantidadeItens === 1 ? "" : "s"}
                </span>
                <span className="record-tag">
                  {sale.quantidadePagamentos} pagamento
                  {sale.quantidadePagamentos === 1 ? "" : "s"}
                </span>
                <span className="record-tag">
                  {formatCurrency(sale.subtotal - sale.descontoTotal)}
                </span>
              </div>
            </button>
          ))}
        </div>
      </CardBody>
    </Card>
  );
}
