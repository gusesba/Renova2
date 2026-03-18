import type { Dispatch, FormEvent, SetStateAction } from "react";

import { Button } from "@/components/ui/button";
import { Card, CardBody, CardHeading } from "@/components/ui/card";
import { SelectField, TextInput } from "@/components/ui/field";
import type { ReportWorkspace } from "@/lib/services/reports";

import type { ReportQueryState } from "./types";

type ReportFiltersPanelProps = {
  busy: boolean;
  filters: ReportQueryState;
  workspace?: ReportWorkspace;
  setFilters: Dispatch<SetStateAction<ReportQueryState>>;
  onSubmit: (event: FormEvent<HTMLFormElement>) => Promise<void>;
};

function isInventoryReport(reportType: string) {
  return reportType === "estoque_atual";
}

function isSoldPiecesReport(reportType: string) {
  return reportType === "pecas_vendidas";
}

function isFinancialReport(reportType: string) {
  return reportType === "financeiro";
}

function isDisposalReport(reportType: string) {
  return reportType === "baixas_estoque";
}

// Concentra os filtros genericos e contextuais de cada relatorio.
export function ReportFiltersPanel({
  busy,
  filters,
  workspace,
  setFilters,
  onSubmit,
}: ReportFiltersPanelProps) {
  const showSupplier = isInventoryReport(filters.tipoRelatorio) || isSoldPiecesReport(filters.tipoRelatorio);
  const showBrand = isInventoryReport(filters.tipoRelatorio) || isSoldPiecesReport(filters.tipoRelatorio);
  const showSeller = isSoldPiecesReport(filters.tipoRelatorio);
  const showStatus = isInventoryReport(filters.tipoRelatorio);
  const showFinancialPerson = isFinancialReport(filters.tipoRelatorio);
  const showReason = isDisposalReport(filters.tipoRelatorio);
  const showPeriod =
    isSoldPiecesReport(filters.tipoRelatorio) ||
    isFinancialReport(filters.tipoRelatorio) ||
    isDisposalReport(filters.tipoRelatorio);

  return (
    <Card>
      <CardBody className="section-stack">
        <CardHeading
          title="Filtros"
          subtitle="Selecione o relatorio, ajuste os filtros e execute a consulta."
        />

        <form className="section-stack" onSubmit={onSubmit}>
          <div className="split-fields">
            <SelectField
              disabled={busy}
              label="Tipo de relatorio"
              onChange={(event) =>
                setFilters((current) => ({
                  ...current,
                  tipoRelatorio: event.target.value,
                }))
              }
              value={filters.tipoRelatorio}
            >
              {(workspace?.tiposRelatorio ?? []).map((item) => (
                <option key={item.codigo} value={item.codigo}>
                  {item.nome}
                </option>
              ))}
            </SelectField>

            <SelectField
              disabled={busy}
              label="Loja"
              onChange={(event) =>
                setFilters((current) => ({
                  ...current,
                  lojaId: event.target.value,
                }))
              }
              value={filters.lojaId}
            >
              {(workspace?.lojas ?? []).map((item) => (
                <option key={item.id} value={item.id}>
                  {item.nome}
                </option>
              ))}
            </SelectField>
          </div>

          {showPeriod ? (
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
          ) : null}

          {showSupplier || showBrand ? (
            <div className="split-fields">
              {showSupplier ? (
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
              ) : null}

              {showBrand ? (
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
                  {(workspace?.marcas ?? []).map((item) => (
                    <option key={item.id} value={item.id}>
                      {item.nome}
                    </option>
                  ))}
                </SelectField>
              ) : null}
            </div>
          ) : null}

          {showSeller || showStatus || showFinancialPerson || showReason ? (
            <div className="split-fields">
              {showSeller ? (
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
              ) : null}

              {showStatus ? (
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
                  {(workspace?.statusPeca ?? []).map((item) => (
                    <option key={item.codigo} value={item.codigo}>
                      {item.nome}
                    </option>
                  ))}
                </SelectField>
              ) : null}

              {showFinancialPerson ? (
                <SelectField
                  disabled={busy}
                  label="Cliente/Fornecedor"
                  onChange={(event) =>
                    setFilters((current) => ({
                      ...current,
                      pessoaId: event.target.value,
                    }))
                  }
                  value={filters.pessoaId}
                >
                  <option value="">Todos</option>
                  {(workspace?.pessoasFinanceiras ?? []).map((item) => (
                    <option key={item.id} value={item.id}>
                      {item.nome}
                    </option>
                  ))}
                </SelectField>
              ) : null}

              {showReason ? (
                <SelectField
                  disabled={busy}
                  label="Motivo"
                  onChange={(event) =>
                    setFilters((current) => ({
                      ...current,
                      motivoMovimentacao: event.target.value,
                    }))
                  }
                  value={filters.motivoMovimentacao}
                >
                  <option value="">Todos</option>
                  {(workspace?.motivosBaixa ?? []).map((item) => (
                    <option key={item.codigo} value={item.codigo}>
                      {item.nome}
                    </option>
                  ))}
                </SelectField>
              ) : null}
            </div>
          ) : null}

          <TextInput
            disabled={busy}
            label="Busca livre"
            onChange={(event) =>
              setFilters((current) => ({
                ...current,
                search: event.target.value,
              }))
            }
            placeholder="Codigo, nome, documento ou descricao"
            value={filters.search}
          />

          <Button disabled={busy} type="submit">
            Executar relatorio
          </Button>
        </form>
      </CardBody>
    </Card>
  );
}
