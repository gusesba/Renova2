import type { Dispatch, FormEvent, SetStateAction } from "react";

import { Button } from "@/components/ui/button";
import { Card, CardBody, CardHeading } from "@/components/ui/card";
import { SelectField, TextArea } from "@/components/ui/field";
import { formatCurrency } from "@/lib/helpers/formatters";
import type {
  ConsignmentActionOption,
  ConsignmentDetail,
} from "@/lib/services/consignments";

import type { ConsignmentCloseFormState } from "./types";

// Concentra as acoes mutaveis do ciclo de vida da consignacao.
type ConsignmentActionsPanelProps = {
  busy: boolean;
  canManage: boolean;
  closeForm: ConsignmentCloseFormState;
  detail?: ConsignmentDetail;
  onApplyDiscount: () => void;
  onClose: (event: FormEvent<HTMLFormElement>) => void;
  receiptText: string;
  setCloseForm: Dispatch<SetStateAction<ConsignmentCloseFormState>>;
  actions: ConsignmentActionOption[];
};

export function ConsignmentActionsPanel({
  busy,
  canManage,
  closeForm,
  detail,
  onApplyDiscount,
  onClose,
  receiptText,
  setCloseForm,
  actions,
}: ConsignmentActionsPanelProps) {
  const summary = detail?.resumo;

  return (
    <Card>
      <CardBody className="section-stack">
        <CardHeading
          subtitle="Ajuste preco quando a faixa vencer e conclua o destino final da peca."
          title="Acoes da consignacao"
        />

        {!summary ? (
          <div className="empty-state">
            Selecione uma peca para habilitar as acoes do ciclo de vida.
          </div>
        ) : (
          <>
            <div className="split-fields">
              <div className="ui-banner">
                <strong>Preco atual:</strong> {formatCurrency(summary.precoVendaAtual)}
              </div>
              <div className="ui-banner">
                <strong>Desconto esperado:</strong> {summary.percentualDescontoEsperado}%
              </div>
            </div>

            <Button
              disabled={busy || !canManage || !summary.descontoPendente}
              onClick={onApplyDiscount}
              type="button"
              variant="ghost"
            >
              Aplicar desconto automatico
            </Button>

            <form className="form-grid" onSubmit={onClose}>
              <SelectField
                disabled={busy || !canManage}
                label="Acao final"
                onChange={(event) =>
                  setCloseForm((current) => ({
                    ...current,
                    acao: event.target.value,
                  }))
                }
                value={closeForm.acao}
              >
                <option value="">Selecione</option>
                {actions.map((action) => (
                  <option key={action.codigo} value={action.codigo}>
                    {action.nome}
                  </option>
                ))}
              </SelectField>

              <TextArea
                disabled={busy || !canManage}
                label="Motivo"
                onChange={(event) =>
                  setCloseForm((current) => ({
                    ...current,
                    motivo: event.target.value,
                  }))
                }
                rows={4}
                value={closeForm.motivo}
              />

              <Button disabled={busy || !canManage || !summary} type="submit">
                Encerrar consignacao
              </Button>
            </form>

            {receiptText ? (
              <div className="ui-banner consignment-receipt">{receiptText}</div>
            ) : null}
          </>
        )}
      </CardBody>
    </Card>
  );
}
