import { type FormEvent, type SetStateAction } from "react";

import { Button } from "@/components/ui/button";
import { Card, CardBody, CardHeading } from "@/components/ui/card";
import { SelectField, TextInput } from "@/components/ui/field";
import { StatusBadge } from "@/components/ui/status-badge";
import type {
  ClosingDetail,
  ClosingPersonOption,
  ClosingWorkspace,
} from "@/lib/services/closings";

import type { GenerateClosingFormState } from "./types";

type ClosingFormPanelProps = {
  busy: boolean;
  canGenerate: boolean;
  canReview: boolean;
  detail?: ClosingDetail;
  form: GenerateClosingFormState;
  workspace?: ClosingWorkspace;
  setForm: (value: SetStateAction<GenerateClosingFormState>) => void;
  onSubmit: (event: FormEvent<HTMLFormElement>) => Promise<void>;
  onReview: () => Promise<void>;
  onSettle: () => Promise<void>;
  onExport: (exportType: "pdf" | "excel") => Promise<void>;
  onCopySummary: () => Promise<void>;
};

// Reune geracao, acoes operacionais e exportacoes do fechamento selecionado.
export function ClosingFormPanel({
  busy,
  canGenerate,
  canReview,
  detail,
  form,
  workspace,
  setForm,
  onSubmit,
  onReview,
  onSettle,
  onExport,
  onCopySummary,
}: ClosingFormPanelProps) {
  const selectedPerson = workspace?.pessoas.find(
    (person) => person.pessoaId === form.pessoaId,
  );

  return (
    <Card>
      <CardBody className="section-stack">
        <CardHeading
          title="Gerar fechamento"
          subtitle="Monte ou regenere o snapshot financeiro da pessoa para um periodo."
        />

        <form className="section-stack" onSubmit={onSubmit}>
          <SelectField
            disabled={busy || !canGenerate}
            label="Pessoa"
            onChange={(event) =>
              setForm((current) => ({ ...current, pessoaId: event.target.value }))
            }
            value={form.pessoaId}
          >
            <option value="">Selecione</option>
            {(workspace?.pessoas ?? []).map((person: ClosingPersonOption) => (
              <option key={person.pessoaId} value={person.pessoaId}>
                {person.nome}
              </option>
            ))}
          </SelectField>

          <div className="split-fields">
            <TextInput
              disabled={busy || !canGenerate}
              label="Periodo inicial"
              onChange={(event) =>
                setForm((current) => ({ ...current, periodoInicio: event.target.value }))
              }
              type="date"
              value={form.periodoInicio}
            />
            <TextInput
              disabled={busy || !canGenerate}
              label="Periodo final"
              onChange={(event) =>
                setForm((current) => ({ ...current, periodoFim: event.target.value }))
              }
              type="date"
              value={form.periodoFim}
            />
          </div>

          {selectedPerson ? (
            <div className="record-tags">
              <span className="record-tag">{selectedPerson.documento}</span>
              {selectedPerson.ehCliente ? <span className="record-tag">Cliente</span> : null}
              {selectedPerson.ehFornecedor ? (
                <span className="record-tag">Fornecedor</span>
              ) : null}
              {selectedPerson.aceitaCreditoLoja ? (
                <span className="record-tag">Aceita credito</span>
              ) : null}
            </div>
          ) : null}

          <Button disabled={busy || !canGenerate} type="submit">
            Gerar fechamento
          </Button>
        </form>

        <CardHeading
          title="Acoes do fechamento"
          subtitle="Conferencia, liquidacao e exportacoes do snapshot selecionado."
        />

        {detail ? (
          <div className="section-stack">
            <div className="stock-record-header">
              <div>
                <div className="selection-item-title">{detail.fechamento.pessoaNome}</div>
                <div className="record-item-copy">
                  Periodo {detail.fechamento.periodoInicio.slice(0, 10)} ate{" "}
                  {detail.fechamento.periodoFim.slice(0, 10)}
                </div>
              </div>
              <StatusBadge value={detail.fechamento.statusFechamento} />
            </div>

            <div className="record-tags">
              <span className="record-tag">
                Vendido {detail.fechamento.quantidadePecasVendidas} pecas
              </span>
              <span className="record-tag">
                Atuais {detail.fechamento.quantidadePecasAtuais} pecas
              </span>
            </div>

            <div className="split-fields">
              <Button
                disabled={
                  busy ||
                  !canReview ||
                  detail.fechamento.statusFechamento === "conferido" ||
                  detail.fechamento.statusFechamento === "liquidado"
                }
                onClick={() => {
                  void onReview();
                }}
                variant="secondary"
              >
                Marcar conferido
              </Button>
              <Button
                disabled={
                  busy ||
                  !canReview ||
                  detail.fechamento.statusFechamento === "liquidado"
                }
                onClick={() => {
                  void onSettle();
                }}
              >
                Marcar liquidado
              </Button>
            </div>

            <div className="split-fields">
              <Button
                disabled={busy}
                onClick={() => {
                  void onExport("pdf");
                }}
                variant="soft"
              >
                Exportar PDF
              </Button>
              <Button
                disabled={busy}
                onClick={() => {
                  void onExport("excel");
                }}
                variant="soft"
              >
                Exportar Excel
              </Button>
            </div>

            <Button
              disabled={busy}
              onClick={() => {
                void onCopySummary();
              }}
              variant="ghost"
            >
              Copiar resumo para WhatsApp
            </Button>
          </div>
        ) : (
          <div className="empty-state">
            Gere ou selecione um fechamento para acessar as acoes operacionais.
          </div>
        )}
      </CardBody>
    </Card>
  );
}
