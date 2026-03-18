import { Button } from "@/components/ui/button";
import { Card, CardBody, CardHeading } from "@/components/ui/card";
import type {
  DocumentSearchItem,
  DocumentTypeOption,
} from "@/lib/services/documents";

type DocumentPreviewPanelProps = {
  busy: boolean;
  item: DocumentSearchItem | null;
  onPrint: () => Promise<void>;
  selectedType: DocumentTypeOption | null;
};

// Mostra o documento selecionado e concentra a acao de impressao.
export function DocumentPreviewPanel({
  busy,
  item,
  onPrint,
  selectedType,
}: DocumentPreviewPanelProps) {
  return (
    <Card className="documents-preview-card">
      <CardBody className="section-stack">
        <CardHeading
          subtitle="A impressao abre um HTML pronto para o navegador salvar ou enviar para a impressora."
          title="Documento selecionado"
        />

        {selectedType ? (
          <div className="documents-preview-summary">
            <span className="documents-preview-label">Tipo ativo</span>
            <strong className="documents-preview-title">{selectedType.nome}</strong>
            <span className="documents-preview-copy">{selectedType.descricao}</span>
          </div>
        ) : (
          <div className="empty-state">
            Selecione o tipo de documento para carregar a lista.
          </div>
        )}

        {item ? (
          <div className="documents-preview-detail">
            <div className="documents-preview-code">{item.titulo}</div>
            <div className="documents-preview-subtitle">{item.subtitulo}</div>
            <div className="record-tags">
              <span className="record-tag">{item.meta}</span>
            </div>
          </div>
        ) : (
          <div className="empty-state">
            Selecione um registro na lista para liberar a impressao.
          </div>
        )}

        <Button
          disabled={busy || !item || !selectedType}
          onClick={() => {
            void onPrint();
          }}
        >
          Imprimir documento
        </Button>
      </CardBody>
    </Card>
  );
}
