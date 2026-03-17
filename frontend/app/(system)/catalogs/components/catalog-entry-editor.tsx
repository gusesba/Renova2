import type { Dispatch, SetStateAction, SubmitEvent } from "react";

import { CatalogEntryBrowser } from "./catalog-entry-browser";
import { CatalogEntryFormPanel } from "./catalog-entry-form-panel";
import type {
  CatalogEntryFormState,
  CatalogEntryListItem,
  CatalogEntryType,
} from "./types";

// Organiza a area principal da pagina em navegador de registros e formulario.
type CatalogEntryEditorProps = {
  busy: boolean;
  entries: CatalogEntryListItem[];
  form: CatalogEntryFormState;
  selectedEntryId: string;
  selectedType: CatalogEntryType;
  setForm: Dispatch<SetStateAction<CatalogEntryFormState>>;
  onNewEntry: () => void;
  onSelectEntry: (entryId: string) => void;
  onSelectType: (type: CatalogEntryType) => void;
  onSubmit: (event: SubmitEvent<HTMLFormElement>) => void;
};

export function CatalogEntryEditor({
  busy,
  entries,
  form,
  selectedEntryId,
  selectedType,
  setForm,
  onNewEntry,
  onSelectEntry,
  onSelectType,
  onSubmit,
}: CatalogEntryEditorProps) {
  return (
    <div className="catalogs-workspace-grid">
      <CatalogEntryBrowser
        entries={entries}
        onNewEntry={onNewEntry}
        onSelectEntry={onSelectEntry}
        onSelectType={onSelectType}
        selectedEntryId={selectedEntryId}
        selectedType={selectedType}
      />

      <CatalogEntryFormPanel
        busy={busy}
        form={form}
        onSubmit={onSubmit}
        selectedType={selectedType}
        setForm={setForm}
      />
    </div>
  );
}
