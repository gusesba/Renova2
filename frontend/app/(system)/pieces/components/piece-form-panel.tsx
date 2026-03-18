import type { Dispatch, FormEvent, SetStateAction } from "react";

import { PieceManualRuleEditor } from "@/app/(system)/pieces/components/piece-manual-rule-editor";
import type { PieceFormState } from "@/app/(system)/pieces/components/types";
import { Button } from "@/components/ui/button";
import { Card, CardBody, CardHeading } from "@/components/ui/card";
import { SelectField, TextArea, TextInput } from "@/components/ui/field";
import type {
  PieceCatalogOption,
  PieceOption,
  PieceSupplierOption,
} from "@/lib/services/pieces";

// Mantem o formulario principal de cadastro e edicao das pecas.
type PieceFormPanelProps = {
  busy: boolean;
  form: PieceFormState;
  onAddManualBand: () => void;
  onSubmit: (event: FormEvent<HTMLFormElement>) => void;
  productNames: PieceCatalogOption[];
  brands: PieceCatalogOption[];
  sizes: PieceCatalogOption[];
  colors: PieceCatalogOption[];
  pieceTypes: PieceOption[];
  suppliers: PieceSupplierOption[];
  setForm: Dispatch<SetStateAction<PieceFormState>>;
};

export function PieceFormPanel({
  busy,
  form,
  onAddManualBand,
  onSubmit,
  productNames,
  brands,
  sizes,
  colors,
  pieceTypes,
  suppliers,
  setForm,
}: PieceFormPanelProps) {
  return (
    <Card>
      <CardBody className="section-stack">
        <CardHeading
          subtitle="Cadastre a peca, congele a condicao comercial e mantenha os dados operacionais da entrada."
          title="Cadastro da peca"
        />

        <form className="form-grid" onSubmit={onSubmit}>
          <div className="split-fields">
            <TextInput disabled label="Codigo interno" value={form.codigoInterno || "Gerado ao salvar"} />
            <SelectField
              disabled={busy}
              label="Tipo da peca"
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  tipoPeca: event.target.value as PieceFormState["tipoPeca"],
                }))
              }
              value={form.tipoPeca}
            >
              {pieceTypes.map((type) => (
                <option key={type.codigo} value={type.codigo}>
                  {type.nome}
                </option>
              ))}
            </SelectField>
          </div>

          <div className="split-fields">
            <SelectField
              disabled={busy}
              label="Produto"
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  produtoNomeId: event.target.value,
                }))
              }
              value={form.produtoNomeId}
            >
              <option value="">Selecione</option>
              {productNames.map((product) => (
                <option key={product.id} value={product.id}>
                  {product.nome}
                </option>
              ))}
            </SelectField>
            <SelectField
              disabled={busy}
              label="Marca"
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  marcaId: event.target.value,
                }))
              }
              value={form.marcaId}
            >
              <option value="">Selecione</option>
              {brands.map((brand) => (
                <option key={brand.id} value={brand.id}>
                  {brand.nome}
                </option>
              ))}
            </SelectField>
          </div>

          <div className="split-fields">
            <SelectField
              disabled={busy}
              label="Tamanho"
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  tamanhoId: event.target.value,
                }))
              }
              value={form.tamanhoId}
            >
              <option value="">Selecione</option>
              {sizes.map((size) => (
                <option key={size.id} value={size.id}>
                  {size.nome}
                </option>
              ))}
            </SelectField>
            <SelectField
              disabled={busy}
              label="Cor"
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  corId: event.target.value,
                }))
              }
              value={form.corId}
            >
              <option value="">Selecione</option>
              {colors.map((color) => (
                <option key={color.id} value={color.id}>
                  {color.nome}
                </option>
              ))}
            </SelectField>
          </div>

          <div className="split-fields">
            <SelectField
              disabled={busy}
              label="Fornecedor"
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  fornecedorPessoaId: event.target.value,
                }))
              }
              value={form.fornecedorPessoaId}
            >
              <option value="">Sem fornecedor</option>
              {suppliers.map((supplier) => (
                <option key={supplier.pessoaId} value={supplier.pessoaId}>
                  {supplier.nome}
                </option>
              ))}
            </SelectField>
            <TextInput
              disabled={busy}
              label="Codigo de barras"
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  codigoBarras: event.target.value,
                }))
              }
              value={form.codigoBarras}
            />
          </div>

          <div className="split-fields">
            <TextInput
              disabled={busy || Boolean(form.id)}
              label="Quantidade inicial"
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  quantidadeInicial: event.target.value,
                }))
              }
              type="number"
              value={form.quantidadeInicial}
            />
            <TextInput
              disabled
              label="Quantidade atual"
              value={form.quantidadeAtual}
            />
          </div>

          <div className="split-fields">
            <TextInput
              disabled={busy}
              label="Preco de venda"
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  precoVendaAtual: event.target.value,
                }))
              }
              step="0.01"
              type="number"
              value={form.precoVendaAtual}
            />
            <TextInput
              disabled={busy}
              label="Custo unitario"
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  custoUnitario: event.target.value,
                }))
              }
              step="0.01"
              type="number"
              value={form.custoUnitario}
            />
          </div>

          <div className="split-fields">
            <TextInput
              disabled={busy}
              label="Data de entrada"
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  dataEntrada: event.target.value,
                }))
              }
              type="date"
              value={form.dataEntrada}
            />
            <TextInput disabled label="Status" value={form.statusPeca} />
          </div>

          <TextInput
            disabled={busy}
            label="Localizacao fisica"
            onChange={(event) =>
              setForm((current) => ({
                ...current,
                localizacaoFisica: event.target.value,
              }))
            }
            value={form.localizacaoFisica}
          />

          <TextArea
            disabled={busy}
            label="Descricao"
            onChange={(event) =>
              setForm((current) => ({
                ...current,
                descricao: event.target.value,
              }))
            }
            rows={3}
            value={form.descricao}
          />

          <TextArea
            disabled={busy}
            label="Observacoes"
            onChange={(event) =>
              setForm((current) => ({
                ...current,
                observacoes: event.target.value,
              }))
            }
            rows={3}
            value={form.observacoes}
          />

          <label className="rule-toggle-card">
            <input
              checked={form.usarRegraManual}
              disabled={busy}
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  usarRegraManual: event.target.checked,
                }))
              }
              type="checkbox"
            />
            <div>
              <div className="selection-item-title">Usar regra manual</div>
              <div className="selection-item-copy">
                Sobrescreve a regra da loja e a regra do fornecedor somente para
                esta peca.
              </div>
            </div>
          </label>

          {form.usarRegraManual ? (
            <PieceManualRuleEditor
              busy={busy}
              onAddBand={onAddManualBand}
              rule={form.regraManual}
              setRule={(value) =>
                setForm((current) => ({
                  ...current,
                  regraManual:
                    typeof value === "function"
                      ? value(current.regraManual)
                      : value,
                }))
              }
            />
          ) : null}

          <Button disabled={busy} type="submit">
            {busy ? "Salvando..." : form.id ? "Salvar peca" : "Criar peca"}
          </Button>
        </form>
      </CardBody>
    </Card>
  );
}
