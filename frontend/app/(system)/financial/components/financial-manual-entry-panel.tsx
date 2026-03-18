import type { Dispatch, FormEvent, SetStateAction } from "react";

import { Button } from "@/components/ui/button";
import { Card, CardBody, CardHeading } from "@/components/ui/card";
import { SelectField, TextArea, TextInput } from "@/components/ui/field";
import type { FinancialWorkspace } from "@/lib/services/financial";

import {
  resolveDirection,
  type FinancialEntryFormState,
} from "@/app/(system)/financial/components/types";

type FinancialManualEntryPanelProps = {
  busy: boolean;
  canManage: boolean;
  form: FinancialEntryFormState;
  onSubmit: (event: FormEvent<HTMLFormElement>) => void;
  setForm: Dispatch<SetStateAction<FinancialEntryFormState>>;
  workspace?: FinancialWorkspace;
};

// Recebe o lancamento financeiro avulso com validacao e defaults do modulo.
export function FinancialManualEntryPanel({
  busy,
  canManage,
  form,
  onSubmit,
  setForm,
  workspace,
}: FinancialManualEntryPanelProps) {
  return (
    <Card>
      <CardBody className="section-stack">
        <CardHeading
          subtitle="Use para despesas, receitas avulsas, ajustes e estornos nao gerados por outros modulos."
          title="Lancamento avulso"
        />

        {!canManage ? (
          <div className="empty-state">
            Sua conta pode consultar o livro razao, mas nao pode criar lancamentos.
          </div>
        ) : (
          <form className="form-grid" onSubmit={onSubmit}>
            <div className="split-fields">
              <SelectField
                label="Tipo do lancamento"
                onChange={(event) =>
                  setForm((current) => {
                    const nextType = event.target.value;
                    const nextDirection = resolveDirection(nextType);

                    return {
                      ...current,
                      tipoMovimentacao: nextType,
                      direcao:
                        nextType === "ajuste" || nextType === "estorno"
                          ? current.direcao
                          : nextDirection,
                    };
                  })
                }
                value={form.tipoMovimentacao}
              >
                {(workspace?.tiposLancamentoManual ?? []).map((item) => (
                  <option key={item.codigo} value={item.codigo}>
                    {item.nome}
                  </option>
                ))}
              </SelectField>
              <SelectField
                label="Direcao"
                onChange={(event) =>
                  setForm((current) => ({
                    ...current,
                    direcao: event.target.value,
                  }))
                }
                value={form.direcao}
              >
                {(workspace?.direcoes ?? []).map((item) => (
                  <option
                    disabled={
                      (form.tipoMovimentacao === "despesa" && item.codigo !== "saida") ||
                      (form.tipoMovimentacao === "receita_avulsa" &&
                        item.codigo !== "entrada")
                    }
                    key={item.codigo}
                    value={item.codigo}
                  >
                    {item.nome}
                  </option>
                ))}
              </SelectField>
            </div>

            <div className="split-fields">
              <TextInput
                label="Valor bruto"
                min="0"
                onChange={(event) =>
                  setForm((current) => ({
                    ...current,
                    valorBruto: event.target.value,
                  }))
                }
                placeholder="0,00"
                step="0.01"
                type="number"
                value={form.valorBruto}
              />
              <TextInput
                label="Taxa"
                min="0"
                onChange={(event) =>
                  setForm((current) => ({
                    ...current,
                    taxa: event.target.value,
                  }))
                }
                placeholder="0,00"
                step="0.01"
                type="number"
                value={form.taxa}
              />
            </div>

            <SelectField
              label="Meio de pagamento"
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  meioPagamentoId: event.target.value,
                }))
              }
              value={form.meioPagamentoId}
            >
              <option value="">Sem meio de pagamento</option>
              {(workspace?.meiosPagamento ?? []).map((method) => (
                <option key={method.id} value={method.id}>
                  {method.nome}
                </option>
              ))}
            </SelectField>

            <div className="split-fields">
              <TextInput
                label="Competencia"
                onChange={(event) =>
                  setForm((current) => ({
                    ...current,
                    competenciaEm: event.target.value,
                  }))
                }
                type="date"
                value={form.competenciaEm}
              />
              <TextInput
                label="Data do movimento"
                onChange={(event) =>
                  setForm((current) => ({
                    ...current,
                    movimentadoEm: event.target.value,
                  }))
                }
                type="date"
                value={form.movimentadoEm}
              />
            </div>

            <TextArea
              label="Descricao"
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  descricao: event.target.value,
                }))
              }
              placeholder="Explique o motivo do lancamento financeiro."
              rows={4}
              value={form.descricao}
            />

            <Button disabled={busy} type="submit">
              {busy ? "Salvando..." : "Registrar lancamento"}
            </Button>
          </form>
        )}
      </CardBody>
    </Card>
  );
}
