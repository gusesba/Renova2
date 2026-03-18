import { type FormEvent } from "react";

import { Button } from "@/components/ui/button";
import { Card, CardBody, CardHeading } from "@/components/ui/card";
import { SelectField } from "@/components/ui/field";
import { StatusBadge } from "@/components/ui/status-badge";
import {
  formatCurrency,
  formatDateTime,
  formatStatus,
} from "@/lib/helpers/formatters";
import type {
  CreditAccountDetail,
  CreditOption,
} from "@/lib/services/credits";

type CreditStatementPanelProps = {
  busy: boolean;
  canManage: boolean;
  detail?: CreditAccountDetail;
  statusOptions: CreditOption[];
  statusValue: string;
  onChangeStatus: (value: string) => void;
  onSubmitStatus: (event: FormEvent<HTMLFormElement>) => Promise<void>;
};

// Exibe saldo, status e extrato detalhado da conta selecionada.
export function CreditStatementPanel({
  busy,
  canManage,
  detail,
  statusOptions,
  statusValue,
  onChangeStatus,
  onSubmitStatus,
}: CreditStatementPanelProps) {
  if (!detail) {
    return (
      <Card>
        <CardBody className="section-stack">
          <CardHeading
            subtitle="Selecione uma conta na lateral para consultar saldo, historico e status."
            title="Extrato da conta"
          />
          <div className="empty-state">
            Nenhuma conta de credito selecionada.
          </div>
        </CardBody>
      </Card>
    );
  }

  return (
    <Card>
      <CardBody className="section-stack">
        <CardHeading
          subtitle={`${detail.conta.nome} • ${detail.conta.documento}`}
          title="Extrato da conta"
        />

        <div className="catalogs-summary-grid">
          <div className="ui-banner">
            <div className="ui-field-label">Saldo atual</div>
            <strong>{formatCurrency(detail.conta.saldoAtual)}</strong>
          </div>
          <div className="ui-banner">
            <div className="ui-field-label">Saldo disponivel</div>
            <strong>{formatCurrency(detail.conta.saldoDisponivel)}</strong>
          </div>
          <div className="ui-banner">
            <div className="ui-field-label">Comprometido</div>
            <strong>{formatCurrency(detail.conta.saldoComprometido)}</strong>
          </div>
          <div className="ui-banner">
            <div className="ui-field-label">Status</div>
            <StatusBadge value={detail.conta.statusConta} />
          </div>
        </div>

        {canManage ? (
          <form className="split-fields" onSubmit={onSubmitStatus}>
            <SelectField
              disabled={busy}
              label="Status operacional"
              onChange={(event) => onChangeStatus(event.target.value)}
              value={statusValue}
            >
              {statusOptions.map((status) => (
                <option key={status.codigo} value={status.codigo}>
                  {status.nome}
                </option>
              ))}
            </SelectField>
            <div style={{ alignSelf: "end" }}>
              <Button disabled={busy} type="submit">
                Salvar status
              </Button>
            </div>
          </form>
        ) : null}

        <div className="record-list">
          {detail.movimentacoes.length === 0 ? (
            <div className="empty-state">
              Nenhuma movimentacao registrada para esta conta.
            </div>
          ) : (
            detail.movimentacoes.map((movement) => (
              <div className="record-item" key={movement.id}>
                <div className="stock-record-header">
                  <div>
                    <div className="selection-item-title">
                      {formatStatus(movement.tipoMovimentacao)}
                    </div>
                    <div className="record-item-copy">
                      {movement.movimentadoPorUsuarioNome} •{" "}
                      {formatDateTime(movement.movimentadoEm)}
                    </div>
                  </div>
                  <StatusBadge value={movement.direcao} />
                </div>

                <div className="record-tags">
                  <span className="record-tag">
                    Valor {formatCurrency(movement.valor)}
                  </span>
                  <span className="record-tag">
                    Saldo {formatCurrency(movement.saldoAnterior)} →{" "}
                    {formatCurrency(movement.saldoPosterior)}
                  </span>
                  <span className="record-tag">
                    Origem {formatStatus(movement.origemTipo)}
                  </span>
                </div>

                <div className="record-item-copy">{movement.observacoes}</div>
              </div>
            ))
          )}
        </div>
      </CardBody>
    </Card>
  );
}
