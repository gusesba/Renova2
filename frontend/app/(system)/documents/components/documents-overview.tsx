import { Card, CardBody } from "@/components/ui/card";
import type { DocumentTypeOption } from "@/lib/services/documents";

type DocumentsOverviewProps = {
  selectedType: string;
  types: DocumentTypeOption[];
  onSelectType: (type: string) => void;
};

// Exibe os quatro tipos de documento em cards de acesso rapido.
export function DocumentsOverview({
  selectedType,
  types,
  onSelectType,
}: DocumentsOverviewProps) {
  return (
    <section className="documents-overview-shell">
      <div className="documents-overview-copy">
        <p className="documents-overview-eyebrow">Modulo 16</p>
        <h1 className="documents-overview-title">Impressoes e Documentos</h1>
        <p className="documents-overview-subtitle">
          Etiquetas, recibos e comprovantes seguem um fluxo unico de busca e
          impressao dentro da loja ativa.
        </p>
      </div>

      <div className="documents-overview-grid">
        {types.map((type) => (
          <button
            className="documents-overview-button"
            key={type.codigo}
            onClick={() => onSelectType(type.codigo)}
            type="button"
          >
            <Card
              className={
                selectedType === type.codigo
                  ? "documents-overview-card documents-overview-card-active"
                  : "documents-overview-card"
              }
            >
              <CardBody className="documents-overview-card-body">
                <span className="documents-overview-card-label">{type.nome}</span>
                <strong className="documents-overview-card-value">
                  {type.codigo.replaceAll("_", " ")}
                </strong>
                <span className="documents-overview-card-description">
                  {type.descricao}
                </span>
              </CardBody>
            </Card>
          </button>
        ))}
      </div>
    </section>
  );
}
