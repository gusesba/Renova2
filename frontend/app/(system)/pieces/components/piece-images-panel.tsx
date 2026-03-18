import { useState, type ChangeEvent } from "react";

import { Button } from "@/components/ui/button";
import { Card, CardBody, CardHeading } from "@/components/ui/card";
import { SelectField, TextInput } from "@/components/ui/field";
import { resolveApiAssetUrl } from "@/lib/services/core/api-client";
import type { PieceImage, PieceOption } from "@/lib/services/pieces";

// Mantem o upload e os metadados das imagens vinculadas a peca.
type PieceImagesPanelProps = {
  busy: boolean;
  canUpload: boolean;
  images: PieceImage[];
  onDeleteImage: (imageId: string) => void;
  onUpdateImage: (imageId: string, ordem: number, tipoVisibilidade: string) => void;
  onUploadImage: (file: File, ordem: number, tipoVisibilidade: string) => void;
  visibilityOptions: PieceOption[];
};

export function PieceImagesPanel({
  busy,
  canUpload,
  images,
  onDeleteImage,
  onUpdateImage,
  onUploadImage,
  visibilityOptions,
}: PieceImagesPanelProps) {
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [uploadOrder, setUploadOrder] = useState("0");
  const [uploadVisibility, setUploadVisibility] = useState(
    visibilityOptions[0]?.codigo ?? "interna",
  );

  function handleFileChange(event: ChangeEvent<HTMLInputElement>) {
    setSelectedFile(event.target.files?.[0] ?? null);
  }

  return (
    <Card>
      <CardBody className="section-stack">
        <CardHeading
          subtitle="As imagens ficam armazenadas localmente na API e vinculadas a peca."
          title="Imagens da peca"
        />

        {!canUpload ? (
          <div className="empty-state">
            Salve a peca primeiro para habilitar o upload e o vinculo das imagens.
          </div>
        ) : (
          <div className="form-grid">
            <label className="ui-field">
              <span className="ui-field-label">Arquivo de imagem</span>
              <input className="ui-input" disabled={busy} onChange={handleFileChange} type="file" />
            </label>

            <div className="split-fields">
              <TextInput
                disabled={busy}
                label="Ordem"
                onChange={(event) => setUploadOrder(event.target.value)}
                type="number"
                value={uploadOrder}
              />
              <SelectField
                disabled={busy}
                label="Visibilidade"
                onChange={(event) => setUploadVisibility(event.target.value)}
                value={uploadVisibility}
              >
                {visibilityOptions.map((option) => (
                  <option key={option.codigo} value={option.codigo}>
                    {option.nome}
                  </option>
                ))}
              </SelectField>
            </div>

            <Button
              disabled={busy || !selectedFile}
              onClick={() =>
                selectedFile
                  ? onUploadImage(selectedFile, Number(uploadOrder || "0"), uploadVisibility)
                  : null
              }
              type="button"
            >
              Enviar imagem
            </Button>
          </div>
        )}

        <div className="piece-image-grid">
          {images.length === 0 ? (
            <div className="empty-state">Nenhuma imagem vinculada a esta peca.</div>
          ) : (
            images.map((image) => (
              <PieceImageItem
                busy={busy}
                image={image}
                key={image.id}
                onDeleteImage={onDeleteImage}
                onUpdateImage={onUpdateImage}
                visibilityOptions={visibilityOptions}
              />
            ))
          )}
        </div>
      </CardBody>
    </Card>
  );
}

// Isola a edicao de ordem e visibilidade de cada imagem listada no painel.
function PieceImageItem({
  busy,
  image,
  onDeleteImage,
  onUpdateImage,
  visibilityOptions,
}: {
  busy: boolean;
  image: PieceImage;
  onDeleteImage: (imageId: string) => void;
  onUpdateImage: (imageId: string, ordem: number, tipoVisibilidade: string) => void;
  visibilityOptions: PieceOption[];
}) {
  const [ordem, setOrdem] = useState(String(image.ordem));
  const [tipoVisibilidade, setTipoVisibilidade] = useState(image.tipoVisibilidade);

  return (
    <div className="piece-image-card">
      {/* A imagem vem de upload dinâmico da API local, então a prévia usa img direta. */}
      {/* eslint-disable-next-line @next/next/no-img-element */}
      <img
        alt="Imagem da peca"
        className="piece-image-preview"
        loading="lazy"
        src={resolveApiAssetUrl(image.urlArquivo)}
      />
      <div className="form-grid">
        <div className="split-fields">
          <TextInput
            disabled={busy}
            label="Ordem"
            onChange={(event) => setOrdem(event.target.value)}
            type="number"
            value={ordem}
          />
          <SelectField
            disabled={busy}
            label="Visibilidade"
            onChange={(event) => setTipoVisibilidade(event.target.value)}
            value={tipoVisibilidade}
          >
            {visibilityOptions.map((option) => (
              <option key={option.codigo} value={option.codigo}>
                {option.nome}
              </option>
            ))}
          </SelectField>
        </div>

        <div className="split-fields">
          <Button
            disabled={busy}
            onClick={() => onUpdateImage(image.id, Number(ordem || "1"), tipoVisibilidade)}
            type="button"
            variant="ghost"
          >
            Salvar metadados
          </Button>
          <Button
            disabled={busy}
            onClick={() => onDeleteImage(image.id)}
            type="button"
            variant="soft"
          >
            Remover imagem
          </Button>
        </div>
      </div>
    </div>
  );
}
