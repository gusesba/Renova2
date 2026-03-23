import type { FormEvent, SetStateAction } from "react";

import { Button } from "@/components/ui/button";
import { Card, CardBody, CardHeading } from "@/components/ui/card";
import { SelectField, TextArea, TextInput } from "@/components/ui/field";
import type { PersonUserOption } from "@/lib/services/people";

import type { PersonBankAccountFormState, PersonFormState } from "./types";
import { emptyBankAccountForm } from "./types";

// Mantem o formulario principal do modulo, incluindo vinculo com loja, usuario e contas bancarias.
type PersonFormPanelProps = {
  busy: boolean;
  canManage: boolean;
  form: PersonFormState;
  onLinkedUserChange?: (usuarioId: string) => void;
  onSubmit: (event: FormEvent<HTMLFormElement>) => void;
  setForm: (value: SetStateAction<PersonFormState>) => void;
  userOptions: PersonUserOption[];
};

function updateBankAccount(
  setForm: (value: SetStateAction<PersonFormState>) => void,
  index: number,
  updater: (current: PersonBankAccountFormState) => PersonBankAccountFormState,
) {
  setForm((current) => ({
    ...current,
    contasBancarias: current.contasBancarias.map((account, accountIndex) =>
      accountIndex === index ? updater(account) : account,
    ),
  }));
}

export function PersonFormPanel({
  busy,
  canManage,
  form,
  onLinkedUserChange,
  onSubmit,
  setForm,
  userOptions,
}: PersonFormPanelProps) {
  if (!canManage) {
    return null;
  }

  const availableUsers = userOptions.filter(
    (user) => !user.pessoaIdLojaAtiva || user.pessoaIdLojaAtiva === form.id,
  );

  return (
    <Card>
      <CardBody className="section-stack">
        <CardHeading
          subtitle="Edite o cadastro mestre, o vinculo com a loja ativa e os dados bancarios em um unico fluxo."
          title={form.id ? "Editar pessoa" : "Novo cadastro"}
        />

        <form className="form-grid" onSubmit={onSubmit}>
          <div className="split-fields">
            <SelectField
              label="Tipo de pessoa"
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  tipoPessoa: event.target.value as "fisica" | "juridica",
                }))
              }
              value={form.tipoPessoa}
            >
              <option value="fisica">Pessoa fisica</option>
              <option value="juridica">Pessoa juridica</option>
            </SelectField>
            <SelectField
              label="Perfil na loja"
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  perfilRelacionamento: event.target.value as
                    | "cliente"
                    | "fornecedor"
                    | "ambos",
                }))
              }
              value={form.perfilRelacionamento}
            >
              <option value="cliente">Cliente</option>
              <option value="fornecedor">Fornecedor</option>
              <option value="ambos">Ambos</option>
            </SelectField>
          </div>

          <div className="split-fields">
            <TextInput
              label="Nome"
              onChange={(event) =>
                setForm((current) => ({ ...current, nome: event.target.value }))
              }
              value={form.nome}
            />
            <TextInput
              label="Nome social"
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  nomeSocial: event.target.value,
                }))
              }
              value={form.nomeSocial}
            />
          </div>

          <div className="split-fields">
            <TextInput
              label="Documento"
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  documento: event.target.value,
                }))
              }
              value={form.documento}
            />
            <TextInput
              label="Telefone"
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  telefone: event.target.value,
                }))
              }
              value={form.telefone}
            />
          </div>

          <TextInput
            label="Email"
            onChange={(event) =>
              setForm((current) => ({ ...current, email: event.target.value }))
            }
            value={form.email}
          />

          <div className="split-fields">
            <TextInput
              label="Logradouro"
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  logradouro: event.target.value,
                }))
              }
              value={form.logradouro}
            />
            <TextInput
              label="Numero"
              onChange={(event) =>
                setForm((current) => ({ ...current, numero: event.target.value }))
              }
              value={form.numero}
            />
          </div>

          <div className="split-fields">
            <TextInput
              label="Complemento"
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  complemento: event.target.value,
                }))
              }
              value={form.complemento}
            />
            <TextInput
              label="Bairro"
              onChange={(event) =>
                setForm((current) => ({ ...current, bairro: event.target.value }))
              }
              value={form.bairro}
            />
          </div>

          <div className="split-fields">
            <TextInput
              label="Cidade"
              onChange={(event) =>
                setForm((current) => ({ ...current, cidade: event.target.value }))
              }
              value={form.cidade}
            />
            <TextInput
              label="UF"
              onChange={(event) =>
                setForm((current) => ({ ...current, uf: event.target.value }))
              }
              value={form.uf}
            />
          </div>

          <div className="split-fields">
            <TextInput
              label="CEP"
              onChange={(event) =>
                setForm((current) => ({ ...current, cep: event.target.value }))
              }
              value={form.cep}
            />
            <SelectField
              label="Status do cadastro"
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  ativo: event.target.value === "ativo",
                }))
              }
              value={form.ativo ? "ativo" : "inativo"}
            >
              <option value="ativo">Ativo</option>
              <option value="inativo">Inativo</option>
            </SelectField>
          </div>

          <div className="split-fields">
            <SelectField
              label="Status da relacao"
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  statusRelacao: event.target.value as "ativo" | "inativo",
                }))
              }
              value={form.statusRelacao}
            >
              <option value="ativo">Ativa</option>
              <option value="inativo">Inativa</option>
            </SelectField>
            <SelectField
              label="Aceita credito da loja"
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  aceitaCreditoLoja: event.target.value === "sim",
                }))
              }
              value={form.aceitaCreditoLoja ? "sim" : "nao"}
            >
              <option value="sim">Sim</option>
              <option value="nao">Nao</option>
            </SelectField>
          </div>

          <div className="split-fields">
            <SelectField
              label="Fim padrao da consignacao"
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  politicaPadraoFimConsignacao: event.target.value as
                    | "devolver"
                    | "doar",
                }))
              }
              value={form.politicaPadraoFimConsignacao}
            >
              <option value="devolver">Devolver</option>
              <option value="doar">Doar</option>
            </SelectField>
            <SelectField
              label="Usuario vinculado"
              onChange={(event) =>
                {
                  const usuarioId = event.target.value;
                  setForm((current) => ({
                    ...current,
                    usuarioId,
                  }));
                  onLinkedUserChange?.(usuarioId);
                }
              }
              value={form.usuarioId}
            >
              <option value="">Sem vinculacao</option>
              {availableUsers.map((user) => (
                <option key={user.id} value={user.id}>
                  {user.nome}
                </option>
              ))}
            </SelectField>
          </div>

          {!form.id ? (
            <div className="record-item-copy">
              Ao selecionar um usuario ja vinculado a pessoa em outra loja, os
              dados mestres e as contas bancarias podem ser recuperados
              automaticamente para este novo vinculo.
            </div>
          ) : null}

          <TextArea
            label="Observacoes internas da relacao"
            onChange={(event) =>
              setForm((current) => ({
                ...current,
                observacoesInternas: event.target.value,
              }))
            }
            value={form.observacoesInternas}
          />

          <TextArea
            label="Observacoes do cadastro"
            onChange={(event) =>
              setForm((current) => ({
                ...current,
                observacoes: event.target.value,
              }))
            }
            value={form.observacoes}
          />

          <div className="section-stack">
            <div
              style={{
                display: "flex",
                justifyContent: "space-between",
                alignItems: "center",
                gap: "1rem",
              }}
            >
              <div className="selection-item-title">Contas bancarias</div>
              <Button
                onClick={() =>
                  setForm((current) => ({
                    ...current,
                    contasBancarias: [
                      ...current.contasBancarias,
                      {
                        ...emptyBankAccountForm(),
                        principal: current.contasBancarias.length === 0,
                      },
                    ],
                  }))
                }
                type="button"
                variant="ghost"
              >
                Adicionar conta
              </Button>
            </div>

            {form.contasBancarias.length === 0 ? (
              <div className="empty-state">
                Nenhuma conta bancaria informada para a pessoa.
              </div>
            ) : (
              <div className="record-list">
                {form.contasBancarias.map((account, index) => (
                  <div className="record-item" key={`${account.id || "new"}-${index}`}>
                    <div className="split-fields">
                      <TextInput
                        label="Banco"
                        onChange={(event) =>
                          updateBankAccount(setForm, index, (current) => ({
                            ...current,
                            banco: event.target.value,
                          }))
                        }
                        value={account.banco}
                      />
                      <SelectField
                        label="Tipo da conta"
                        onChange={(event) =>
                          updateBankAccount(setForm, index, (current) => ({
                            ...current,
                            tipoConta: event.target.value,
                          }))
                        }
                        value={account.tipoConta}
                      >
                        <option value="corrente">Corrente</option>
                        <option value="poupanca">Poupanca</option>
                        <option value="pix">PIX</option>
                      </SelectField>
                    </div>
                    <div className="split-fields">
                      <TextInput
                        label="Agencia"
                        onChange={(event) =>
                          updateBankAccount(setForm, index, (current) => ({
                            ...current,
                            agencia: event.target.value,
                          }))
                        }
                        value={account.agencia}
                      />
                      <TextInput
                        label="Conta"
                        onChange={(event) =>
                          updateBankAccount(setForm, index, (current) => ({
                            ...current,
                            conta: event.target.value,
                          }))
                        }
                        value={account.conta}
                      />
                    </div>
                    <div className="split-fields">
                      <TextInput
                        label="Tipo PIX"
                        onChange={(event) =>
                          updateBankAccount(setForm, index, (current) => ({
                            ...current,
                            pixTipo: event.target.value,
                          }))
                        }
                        value={account.pixTipo}
                      />
                      <TextInput
                        label="Chave PIX"
                        onChange={(event) =>
                          updateBankAccount(setForm, index, (current) => ({
                            ...current,
                            pixChave: event.target.value,
                          }))
                        }
                        value={account.pixChave}
                      />
                    </div>
                    <div className="split-fields">
                      <TextInput
                        label="Favorecido"
                        onChange={(event) =>
                          updateBankAccount(setForm, index, (current) => ({
                            ...current,
                            favorecidoNome: event.target.value,
                          }))
                        }
                        value={account.favorecidoNome}
                      />
                      <TextInput
                        label="Documento do favorecido"
                        onChange={(event) =>
                          updateBankAccount(setForm, index, (current) => ({
                            ...current,
                            favorecidoDocumento: event.target.value,
                          }))
                        }
                        value={account.favorecidoDocumento}
                      />
                    </div>
                    <div className="split-fields">
                      <SelectField
                        label="Conta principal"
                        onChange={(event) =>
                          setForm((current) => ({
                            ...current,
                            contasBancarias: current.contasBancarias.map(
                              (currentAccount, accountIndex) => ({
                                ...currentAccount,
                                principal:
                                  accountIndex === index &&
                                  event.target.value === "sim",
                              }),
                            ),
                          }))
                        }
                        value={account.principal ? "sim" : "nao"}
                      >
                        <option value="sim">Sim</option>
                        <option value="nao">Nao</option>
                      </SelectField>
                      <div style={{ display: "flex", alignItems: "end" }}>
                        <Button
                          onClick={() =>
                            setForm((current) => ({
                              ...current,
                              contasBancarias: current.contasBancarias.filter(
                                (_, accountIndex) => accountIndex !== index,
                              ),
                            }))
                          }
                          type="button"
                          variant="ghost"
                        >
                          Remover conta
                        </Button>
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>

          <Button disabled={busy} type="submit">
            {busy ? "Salvando..." : form.id ? "Salvar cadastro" : "Criar cadastro"}
          </Button>
        </form>
      </CardBody>
    </Card>
  );
}
