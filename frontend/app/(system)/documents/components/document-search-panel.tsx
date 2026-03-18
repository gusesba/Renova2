import { type Dispatch, type FormEvent, type SetStateAction } from "react";

import type { DocumentQueryState } from "@/app/(system)/documents/components/types";
import { Button } from "@/components/ui/button";
import { Card, CardBody, CardHeading } from "@/components/ui/card";
import { SelectField, TextInput } from "@/components/ui/field";
import type {
  DocumentSearchItem,
  DocumentTypeOption,
} from "@/lib/services/documents";

type DocumentSearchPanelProps = {
  busy: boolean;
  filters: DocumentQueryState;
  onSubmit: (event: FormEvent<HTMLFormElement>) => void;
  results: DocumentSearchItem[];
  selectedItemId: string | null;
  setFilters: Dispatch<SetStateAction<DocumentQueryState>>;
  setSelectedItemId: (itemId: string) => void;
  types: DocumentTypeOption[];
};

// Reune o seletor de tipo, a busca livre e a lista de resultados do modulo.
export function DocumentSearchPanel({
  busy,
  filters,
  onSubmit,
  results,
  selectedItemId,
  setFilters,
  setSelectedItemId,
  types,
}: DocumentSearchPanelProps) {
  return (
    <Card className="documents-search-card">
      <CardBody className="section-stack">
        <CardHeading
          subtitle="Escolha o tipo de documento, filtre pelo identificador e selecione o registro para imprimir."
          title="Busca de documentos"
        />

        <form className="form-grid" onSubmit={onSubmit}>
          <SelectField
            disabled={busy}
            label="Tipo de documento"
            name="tipoDocumento"
            onChange={(event) =>
              setFilters((current) => ({
                ...current,
                tipoDocumento: event.target.value as DocumentQueryState["tipoDocumento"],
              }))
            }
            value={filters.tipoDocumento}
          >
            <option value="">Selecione</option>
            {types.map((type) => (
              <option key={type.codigo} value={type.codigo}>
                {type.nome}
              </option>
            ))}
          </SelectField>

          <TextInput
            disabled={busy}
            label="Busca"
            onChange={(event) =>
              setFilters((current) => ({ ...current, search: event.target.value }))
            }
            placeholder="Codigo, venda, fornecedor, motivo..."
            value={filters.search}
          />

          <Button disabled={busy} type="submit" variant="soft">
            Atualizar lista
          </Button>
        </form>

        <div className="documents-record-list">
          {results.length === 0 ? (
            <div className="empty-state">
              Nenhum documento encontrado com os filtros informados.
            </div>
          ) : (
            results.map((item) => (
              <button
                className={
                  selectedItemId === item.id
                    ? "record-item record-item-active"
                    : "record-item"
                }
                key={item.id}
                onClick={() => setSelectedItemId(item.id)}
                type="button"
              >
                <strong>{item.titulo}</strong>
                <div className="record-item-copy">{item.subtitulo}</div>
                <div className="record-tags">
                  <span className="record-tag">{item.meta}</span>
                </div>
              </button>
            ))
          )}
        </div>
      </CardBody>
    </Card>
  );
}
