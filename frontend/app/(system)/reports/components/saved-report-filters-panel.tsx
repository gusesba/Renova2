import type { Dispatch, SetStateAction } from "react";

import { Button } from "@/components/ui/button";
import { Card, CardBody, CardHeading } from "@/components/ui/card";
import { TextInput } from "@/components/ui/field";
import type { SavedReportFilter } from "@/lib/services/reports";

import type { ReportQueryState } from "./types";

type SavedReportFiltersPanelProps = {
  busy: boolean;
  filters: ReportQueryState;
  filterName: string;
  savedFilters: SavedReportFilter[];
  setFilterName: Dispatch<SetStateAction<string>>;
  onApplyFilter: (filter: SavedReportFilter) => void;
  onDeleteFilter: (filterId: string) => Promise<void>;
  onSaveFilter: () => Promise<void>;
};

// Gerencia os filtros frequentes do usuario para o modulo de relatorios.
export function SavedReportFiltersPanel({
  busy,
  filters,
  filterName,
  savedFilters,
  setFilterName,
  onApplyFilter,
  onDeleteFilter,
  onSaveFilter,
}: SavedReportFiltersPanelProps) {
  const compatibleFilters = savedFilters.filter(
    (item) => item.tipoRelatorio === filters.tipoRelatorio,
  );

  return (
    <Card>
      <CardBody className="section-stack">
        <CardHeading
          title="Filtros salvos"
          subtitle="Guarde combinacoes frequentes por usuario e loja."
        />

        <div className="section-stack">
          <TextInput
            disabled={busy}
            label="Nome do filtro"
            onChange={(event) => setFilterName(event.target.value)}
            placeholder="Ex.: Estoque consignado"
            value={filterName}
          />

          <Button disabled={busy} onClick={() => void onSaveFilter()} variant="soft">
            Salvar configuracao atual
          </Button>
        </div>

        <div className="record-list">
          {compatibleFilters.length === 0 ? (
            <div className="empty-state">
              Nenhum filtro salvo para o tipo de relatorio selecionado.
            </div>
          ) : (
            compatibleFilters.map((item) => (
              <div className="record-item" key={item.id}>
                <div className="stock-record-header">
                  <span className="selection-item-title">{item.nome}</span>
                  <span className="record-tag">{item.tipoRelatorio}</span>
                </div>

                <div className="record-tags">
                  <Button
                    disabled={busy}
                    onClick={() => onApplyFilter(item)}
                    variant="ghost"
                  >
                    Aplicar
                  </Button>
                  <Button
                    disabled={busy}
                    onClick={() => void onDeleteFilter(item.id)}
                    variant="ghost"
                  >
                    Remover
                  </Button>
                </div>
              </div>
            ))
          )}
        </div>
      </CardBody>
    </Card>
  );
}
