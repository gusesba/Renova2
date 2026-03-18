import { type Dispatch, type FormEvent, type SetStateAction } from "react";

import { Button } from "@/components/ui/button";
import { Card, CardBody, CardHeading } from "@/components/ui/card";
import { SelectField, TextArea, TextInput } from "@/components/ui/field";
import { formatDateTime, formatStatus } from "@/lib/helpers/formatters";
import type {
  StockOption,
  StockPieceLookup,
} from "@/lib/services/stock-movements";

import type { StockAdjustmentFormState } from "@/app/(system)/stock-movements/components/types";

// Renderiza o formulario lateral de ajuste manual para a peca selecionada.
type StockAdjustmentPanelProps = {
  busy: boolean;
  canManage: boolean;
  form: StockAdjustmentFormState;
  onSubmit: (event: FormEvent<HTMLFormElement>) => void;
  piece?: StockPieceLookup;
  setForm: Dispatch<SetStateAction<StockAdjustmentFormState>>;
  statuses: StockOption[];
};

export function StockAdjustmentPanel({
  busy,
  canManage,
  form,
  onSubmit,
  piece,
  setForm,
  statuses,
}: StockAdjustmentPanelProps) {
  return (
    <Card>
      <CardBody className="section-stack">
        <CardHeading
          subtitle="A operacao exige permissao de ajuste e sempre registra movimentacao de estoque com auditoria."
          title="Ajuste manual"
        />

        {!piece ? (
          <div className="empty-state">
            Selecione uma peca na busca operacional para registrar um ajuste.
          </div>
        ) : (
          <>
            <div className="ui-banner">
              <strong>{piece.codigoInterno}</strong> • {piece.produtoNome} • saldo
              atual {piece.quantidadeAtual} • entrada em{" "}
              {formatDateTime(piece.dataEntrada)}
            </div>

            <form className="form-grid" onSubmit={onSubmit}>
              <div className="split-fields">
                <TextInput
                  disabled={busy || !canManage}
                  label="Nova quantidade"
                  onChange={(event) =>
                    setForm((current) => ({
                      ...current,
                      quantidadeNova: event.target.value,
                    }))
                  }
                  type="number"
                  value={form.quantidadeNova}
                />

                <SelectField
                  disabled={busy || !canManage}
                  label="Status final"
                  onChange={(event) =>
                    setForm((current) => ({
                      ...current,
                      statusPeca: event.target.value,
                    }))
                  }
                  value={form.statusPeca}
                >
                  <option value="">Manter atual</option>
                  {statuses.map((status) => (
                    <option key={status.codigo} value={status.codigo}>
                      {status.nome}
                    </option>
                  ))}
                </SelectField>
              </div>

              <TextArea
                disabled={busy || !canManage}
                label="Motivo do ajuste"
                onChange={(event) =>
                  setForm((current) => ({
                    ...current,
                    motivo: event.target.value,
                  }))
                }
                placeholder="Descreva o motivo operacional do ajuste."
                value={form.motivo}
              />

              <div className="record-tags">
                <span className="record-tag">
                  Status atual: {formatStatus(piece.statusPeca)}
                </span>
                <span className="record-tag">
                  Venda permitida: {piece.disponivelParaVenda ? "sim" : "nao"}
                </span>
                <span className="record-tag">Localizacao: {piece.localizacaoFisica}</span>
              </div>

              <Button disabled={busy || !canManage} type="submit">
                Registrar ajuste
              </Button>
            </form>
          </>
        )}
      </CardBody>
    </Card>
  );
}
