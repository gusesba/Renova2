import type { Dispatch, FormEvent, SetStateAction } from "react";

import { DiscountPolicyEditor } from "@/app/(system)/commercial-rules/components/discount-policy-editor";
import type {
  DiscountBandFormState,
  SupplierRuleFormState,
} from "@/app/(system)/commercial-rules/components/types";
import { Button } from "@/components/ui/button";
import { Card, CardBody, CardHeading } from "@/components/ui/card";
import { SelectField, TextInput } from "@/components/ui/field";
import { cx } from "@/lib/helpers/classnames";
import type {
  SupplierCommercialRule,
  SupplierRuleOption,
} from "@/lib/services/commercial-rules";

// Reune a listagem e a manutencao das regras comerciais por fornecedor.
type SupplierRulesPanelProps = {
  busy: boolean;
  form: SupplierRuleFormState;
  onAddBand: () => void;
  onNewRule: () => void;
  onSelectRule: (supplierRuleId: string) => void;
  onSubmit: (event: FormEvent<HTMLFormElement>) => void;
  selectedRuleId: string;
  setForm: Dispatch<SetStateAction<SupplierRuleFormState>>;
  supplierOptions: SupplierRuleOption[];
  supplierRules: SupplierCommercialRule[];
};

export function SupplierRulesPanel({
  busy,
  form,
  onAddBand,
  onNewRule,
  onSelectRule,
  onSubmit,
  selectedRuleId,
  setForm,
  supplierOptions,
  supplierRules,
}: SupplierRulesPanelProps) {
  function setBands(value: SetStateAction<DiscountBandFormState[]>) {
    setForm((current) => ({
      ...current,
      politicaDesconto:
        typeof value === "function" ? value(current.politicaDesconto) : value,
    }));
  }

  const selectedSupplier = supplierRules.find((rule) => rule.id === selectedRuleId);
  const canCreateRule = supplierOptions.length > 0;

  return (
    <Card>
      <CardBody className="section-stack">
        <CardHeading
          subtitle="Permite sobrescrever a regra padrao da loja para fornecedores especificos."
          title="Regras comerciais por fornecedor"
        />

        <div className="rule-supplier-grid">
          <div className="section-stack">
            <div className="rule-inline-header">
              <div>
                <div className="ui-card-title">Fornecedores com regra</div>
                <p className="ui-card-subtitle">
                  Selecione uma regra existente ou abra um novo cadastro.
                </p>
              </div>
              <Button
                disabled={busy || !canCreateRule}
                onClick={onNewRule}
                type="button"
                variant="ghost"
              >
                Nova regra
              </Button>
            </div>

            <div className="record-list">
              {supplierRules.length === 0 ? (
                <div className="empty-state">
                  Nenhuma sobrescrita configurada. Use a regra da loja ou crie uma
                  excecao para um fornecedor.
                </div>
              ) : (
                supplierRules.map((rule) => (
                  <button
                    className={cx(
                      "record-item",
                      selectedRuleId === rule.id && "catalogs-entry-row-active",
                    )}
                    key={rule.id}
                    onClick={() => onSelectRule(rule.id)}
                    type="button"
                  >
                    <div className="selection-item-title">{rule.fornecedorNome}</div>
                    <div className="record-item-copy">
                      {rule.fornecedorDocumento} | dinheiro {rule.percentualRepasseDinheiro}% |
                      credito {rule.percentualRepasseCredito}%
                    </div>
                    <div className="record-tags">
                      <span className="record-tag">
                        {rule.ativo ? "Regra ativa" : "Regra inativa"}
                      </span>
                      <span className="record-tag">
                        {rule.permitePagamentoMisto ? "Misto ligado" : "Misto desligado"}
                      </span>
                    </div>
                  </button>
                ))
              )}
            </div>
          </div>

          <form className="form-grid" onSubmit={onSubmit}>
            <SelectField
              disabled={busy}
              label="Fornecedor"
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  pessoaLojaId: event.target.value,
                }))
              }
              value={form.pessoaLojaId}
            >
              <option value="">Selecione</option>
              {supplierOptions.map((supplier) => (
                <option key={supplier.pessoaLojaId} value={supplier.pessoaLojaId}>
                  {supplier.nome}
                </option>
              ))}
            </SelectField>

            {selectedSupplier ? (
              <div className="ui-banner">
                Editando a regra de {selectedSupplier.fornecedorNome}.
              </div>
            ) : null}

            <div className="split-fields">
              <TextInput
                disabled={busy}
                label="Percentual repasse dinheiro"
                onChange={(event) =>
                  setForm((current) => ({
                    ...current,
                    percentualRepasseDinheiro: event.target.value,
                  }))
                }
                step="0.01"
                type="number"
                value={form.percentualRepasseDinheiro}
              />
              <TextInput
                disabled={busy}
                label="Percentual repasse credito"
                onChange={(event) =>
                  setForm((current) => ({
                    ...current,
                    percentualRepasseCredito: event.target.value,
                  }))
                }
                step="0.01"
                type="number"
                value={form.percentualRepasseCredito}
              />
            </div>

            <TextInput
              disabled={busy}
              label="Prazo maximo exposicao (dias)"
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  tempoMaximoExposicaoDias: event.target.value,
                }))
              }
              type="number"
              value={form.tempoMaximoExposicaoDias}
            />

            <div className="rule-toggle-grid">
              <label className="rule-toggle-card">
                <input
                  checked={form.permitePagamentoMisto}
                  disabled={busy}
                  onChange={(event) =>
                    setForm((current) => ({
                      ...current,
                      permitePagamentoMisto: event.target.checked,
                    }))
                  }
                  type="checkbox"
                />
                <div>
                  <div className="selection-item-title">Permite pagamento misto</div>
                  <div className="selection-item-copy">
                    Controla a excecao do fornecedor acima da regra da loja.
                  </div>
                </div>
              </label>

              <label className="rule-toggle-card">
                <input
                  checked={form.ativo}
                  disabled={busy}
                  onChange={(event) =>
                    setForm((current) => ({
                      ...current,
                      ativo: event.target.checked,
                    }))
                  }
                  type="checkbox"
                />
                <div>
                  <div className="selection-item-title">Regra ativa</div>
                  <div className="selection-item-copy">
                    Desative sem apagar a sobrescrita historica do fornecedor.
                  </div>
                </div>
              </label>
            </div>

            <DiscountPolicyEditor
              bands={form.politicaDesconto}
              disabled={busy}
              onAddBand={onAddBand}
              setBands={setBands}
            />

            <Button disabled={busy || !canCreateRule} type="submit">
              {busy
                ? "Salvando..."
                : form.id
                  ? "Salvar regra do fornecedor"
                  : "Criar regra do fornecedor"}
            </Button>
          </form>
        </div>
      </CardBody>
    </Card>
  );
}
