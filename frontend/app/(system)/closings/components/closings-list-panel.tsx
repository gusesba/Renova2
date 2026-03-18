import type { Dispatch, SetStateAction } from "react";

import { Card, CardBody, CardHeading } from "@/components/ui/card";
import { SelectField, TextInput } from "@/components/ui/field";
import { StatusBadge } from "@/components/ui/status-badge";
import { formatCurrency, formatDateTime } from "@/lib/helpers/formatters";
import type {
  ClosingOption,
  ClosingPersonOption,
  ClosingSummary,
} from "@/lib/services/closings";

import type { ClosingFiltersState } from "./types";

type ClosingsListPanelProps = {
  closings: ClosingSummary[];
  filters: ClosingFiltersState;
  people: ClosingPersonOption[];
  selectedClosingId: string;
  statuses: ClosingOption[];
  setFilters: Dispatch<SetStateAction<ClosingFiltersState>>;
  onSelectClosing: (closingId: string) => void;
};

// Exibe filtros e historico lateral dos fechamentos da loja ativa.
export function ClosingsListPanel({
  closings,
  filters,
  people,
  selectedClosingId,
  statuses,
  setFilters,
  onSelectClosing,
}: ClosingsListPanelProps) {
  return (
    <Card>
      <CardBody className="section-stack">
        <CardHeading
          title="Historico de fechamentos"
          subtitle="Filtre por pessoa, status e periodo para localizar snapshots anteriores."
        />

        <div className="split-fields">
          <TextInput
            label="Busca"
            onChange={(event) =>
              setFilters((current) => ({ ...current, search: event.target.value }))
            }
            placeholder="Nome, documento ou responsavel"
            value={filters.search}
          />
          <SelectField
            label="Pessoa"
            onChange={(event) =>
              setFilters((current) => ({ ...current, pessoaId: event.target.value }))
            }
            value={filters.pessoaId}
          >
            <option value="">Todas</option>
            {people.map((person) => (
              <option key={person.pessoaId} value={person.pessoaId}>
                {person.nome}
              </option>
            ))}
          </SelectField>
          <SelectField
            label="Status"
            onChange={(event) =>
              setFilters((current) => ({
                ...current,
                statusFechamento: event.target.value,
              }))
            }
            value={filters.statusFechamento}
          >
            <option value="">Todos</option>
            {statuses.map((status) => (
              <option key={status.codigo} value={status.codigo}>
                {status.nome}
              </option>
            ))}
          </SelectField>
          <TextInput
            label="Data inicial"
            onChange={(event) =>
              setFilters((current) => ({ ...current, dataInicial: event.target.value }))
            }
            type="date"
            value={filters.dataInicial}
          />
          <TextInput
            label="Data final"
            onChange={(event) =>
              setFilters((current) => ({ ...current, dataFinal: event.target.value }))
            }
            type="date"
            value={filters.dataFinal}
          />
        </div>

        <div className="record-list">
          {closings.length === 0 ? (
            <div className="empty-state">Nenhum fechamento encontrado para os filtros.</div>
          ) : (
            closings.map((closing) => (
              <button
                className="record-item"
                key={closing.id}
                onClick={() => onSelectClosing(closing.id)}
                style={{
                  background:
                    closing.id === selectedClosingId ? "var(--panel-soft)" : "transparent",
                  cursor: "pointer",
                  textAlign: "left",
                }}
                type="button"
              >
                <div className="stock-record-header">
                  <div>
                    <div className="selection-item-title">{closing.pessoaNome}</div>
                    <div className="record-item-copy">
                      {closing.pessoaDocumento} • {formatDateTime(closing.geradoEm)}
                    </div>
                  </div>
                  <StatusBadge value={closing.statusFechamento} />
                </div>

                <div className="record-tags">
                  <span className="record-tag">
                    Periodo {closing.periodoInicio.slice(0, 10)} ate{" "}
                    {closing.periodoFim.slice(0, 10)}
                  </span>
                  <span className="record-tag">
                    Vendido {formatCurrency(closing.valorVendido)}
                  </span>
                  <span className="record-tag">
                    Saldo {formatCurrency(closing.saldoFinal)}
                  </span>
                </div>
              </button>
            ))
          )}
        </div>
      </CardBody>
    </Card>
  );
}
