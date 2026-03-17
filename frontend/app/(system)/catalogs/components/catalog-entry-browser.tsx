import { Button } from "@/components/ui/button";
import { Card, CardBody } from "@/components/ui/card";
import { cx } from "@/lib/helpers/classnames";

import {
  catalogEntryTypeOptions,
  type CatalogEntryListItem,
  type CatalogEntryType,
} from "./types";

// Mantem a navegacao entre grupos e a lista de registros do grupo atual.
type CatalogEntryBrowserProps = {
  entries: CatalogEntryListItem[];
  onNewEntry: () => void;
  onSelectEntry: (entryId: string) => void;
  onSelectType: (type: CatalogEntryType) => void;
  selectedEntryId: string;
  selectedType: CatalogEntryType;
};

export function CatalogEntryBrowser({
  entries,
  onNewEntry,
  onSelectEntry,
  onSelectType,
  selectedEntryId,
  selectedType,
}: CatalogEntryBrowserProps) {
  const selectedTypeMeta =
    catalogEntryTypeOptions.find((item) => item.type === selectedType) ??
    catalogEntryTypeOptions[0];

  return (
    <Card className="catalogs-browser-card">
      <CardBody className="catalogs-browser-body">
        <div className="catalogs-browser-header">
          <div>
            <p className="catalogs-browser-eyebrow">Grupo selecionado</p>
            <h3 className="catalogs-browser-title">{selectedTypeMeta.title}</h3>
            <p className="catalogs-browser-subtitle">{selectedTypeMeta.subtitle}</p>
          </div>

          <Button onClick={onNewEntry} type="button" variant="ghost">
            Novo
          </Button>
        </div>

        <div className="catalogs-type-grid">
          {catalogEntryTypeOptions.map((item) => (
            <button
              className={cx(
                "catalogs-type-chip",
                selectedType === item.type && "catalogs-type-chip-active",
              )}
              key={item.type}
              onClick={() => onSelectType(item.type)}
              type="button"
            >
              <span className="catalogs-type-chip-label">{item.label}</span>
            </button>
          ))}
        </div>

        <div className="catalogs-entry-list">
          {entries.length === 0 ? (
            <div className="empty-state">
              Nenhum cadastro encontrado na loja ativa.
            </div>
          ) : (
            entries.map((entry) => (
              <button
                className={cx(
                  "catalogs-entry-row",
                  selectedEntryId === entry.id && "catalogs-entry-row-active",
                )}
                key={entry.id}
                onClick={() => onSelectEntry(entry.id)}
                type="button"
              >
                <span className="catalogs-entry-name">{entry.nome}</span>
              </button>
            ))
          )}
        </div>
      </CardBody>
    </Card>
  );
}
