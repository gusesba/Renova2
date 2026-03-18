import type { Dispatch, FormEvent, SetStateAction } from "react";

import { Button } from "@/components/ui/button";
import { Card, CardBody, CardHeading } from "@/components/ui/card";
import { SelectField, TextInput } from "@/components/ui/field";
import type { DashboardWorkspace } from "@/lib/services/dashboards";

import type { DashboardFiltersState } from "./types";

type DashboardFiltersPanelProps = {
  busy: boolean;
  filters: DashboardFiltersState;
  workspace?: DashboardWorkspace;
  setFilters: Dispatch<SetStateAction<DashboardFiltersState>>;
  onSubmit: (event: FormEvent<HTMLFormElement>) => Promise<void>;
};

// Reune os filtros rapidos e detalhados do modulo.
export function DashboardFiltersPanel({
  busy,
  filters,
  workspace,
  setFilters,
  onSubmit,
}: DashboardFiltersPanelProps) {
  return (
    <Card>
      <CardBody className="section-stack">
        <CardHeading
          title="Filtros"
          subtitle="Aplique periodo, vendedor, fornecedor, marca e tipo de peca."
        />

        <form className="section-stack" onSubmit={onSubmit}>
          <div className="split-fields">
            <TextInput
              disabled={busy}
              label="Data inicial"
              onChange={(event) =>
                setFilters((current) => ({ ...current, dataInicial: event.target.value }))
              }
              type="date"
              value={filters.dataInicial}
            />
            <TextInput
              disabled={busy}
              label="Data final"
              onChange={(event) =>
                setFilters((current) => ({ ...current, dataFinal: event.target.value }))
              }
              type="date"
              value={filters.dataFinal}
            />
          </div>

          <div className="split-fields">
            <SelectField
              disabled={busy}
              label="Vendedor"
              onChange={(event) =>
                setFilters((current) => ({
                  ...current,
                  vendedorUsuarioId: event.target.value,
                }))
              }
              value={filters.vendedorUsuarioId}
            >
              <option value="">Todos</option>
              {(workspace?.vendedores ?? []).map((item) => (
                <option key={item.id} value={item.id}>
                  {item.nome}
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
              {(workspace?.fornecedores ?? []).map((item) => (
                <option key={item.id} value={item.id}>
                  {item.nome}
                </option>
              ))}
            </SelectField>
          </div>

          <div className="split-fields">
            <SelectField
              disabled={busy}
              label="Marca"
              onChange={(event) =>
                setFilters((current) => ({ ...current, marcaId: event.target.value }))
              }
              value={filters.marcaId}
            >
              <option value="">Todas</option>
              {(workspace?.marcas ?? []).map((item) => (
                <option key={item.id} value={item.id}>
                  {item.nome}
                </option>
              ))}
            </SelectField>
            <SelectField
              disabled={busy}
              label="Tipo de peca"
              onChange={(event) =>
                setFilters((current) => ({ ...current, tipoPeca: event.target.value }))
              }
              value={filters.tipoPeca}
            >
              <option value="">Todos</option>
              {(workspace?.tiposPeca ?? []).map((item) => (
                <option key={item.codigo} value={item.codigo}>
                  {item.nome}
                </option>
              ))}
            </SelectField>
          </div>

          <Button disabled={busy} type="submit">
            Atualizar indicadores
          </Button>
        </form>
      </CardBody>
    </Card>
  );
}
