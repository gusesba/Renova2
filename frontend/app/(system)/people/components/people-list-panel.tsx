import { startTransition, type Dispatch, type SetStateAction } from "react";

import { Button } from "@/components/ui/button";
import { Card, CardBody, CardHeading } from "@/components/ui/card";
import { TextInput } from "@/components/ui/field";
import { StatusBadge } from "@/components/ui/status-badge";
import { formatCurrency } from "@/lib/helpers/formatters";
import type { PersonSummary } from "@/lib/services/people";

// Lista e filtra as pessoas vinculadas a loja ativa.
type PeopleListPanelProps = {
  canManage: boolean;
  people: PersonSummary[];
  search: string;
  selectedPersonId: string;
  setSearch: Dispatch<SetStateAction<string>>;
  onNewPerson: () => void;
  onSelectPerson: (personId: string) => void;
};

function describeProfile(person: PersonSummary) {
  if (person.relacaoLoja.ehCliente && person.relacaoLoja.ehFornecedor) {
    return "Cliente e fornecedor";
  }

  if (person.relacaoLoja.ehFornecedor) {
    return "Fornecedor";
  }

  return "Cliente";
}

export function PeopleListPanel({
  canManage,
  people,
  search,
  selectedPersonId,
  setSearch,
  onNewPerson,
  onSelectPerson,
}: PeopleListPanelProps) {
  const normalizedSearch = search.trim().toLowerCase();
  const filteredPeople = people.filter((person) => {
    if (!normalizedSearch) {
      return true;
    }

    return [person.nome, person.documento, person.email]
      .join(" ")
      .toLowerCase()
      .includes(normalizedSearch);
  });

  return (
    <Card>
      <CardBody className="section-stack">
        <CardHeading
          subtitle="Consulte clientes e fornecedores vinculados a loja ativa."
          title="Pessoas da loja"
        />

        <div className="section-stack">
          <TextInput
            label="Busca rapida"
            onChange={(event) => {
              const nextValue = event.target.value;
              startTransition(() => setSearch(nextValue));
            }}
            placeholder="Nome, documento ou email"
            value={search}
          />
          {canManage ? (
            <Button onClick={onNewPerson} type="button">
              Novo cadastro
            </Button>
          ) : null}
        </div>

        <div className="record-list">
          {filteredPeople.length === 0 ? (
            <div className="empty-state">
              Nenhuma pessoa encontrada para os filtros atuais.
            </div>
          ) : (
            filteredPeople.map((person) => (
              <button
                className="record-item"
                key={person.id}
                onClick={() => onSelectPerson(person.id)}
                style={{
                  borderColor:
                    selectedPersonId === person.id
                      ? "var(--brand-sand)"
                      : undefined,
                }}
                type="button"
              >
                <div
                  style={{
                    display: "flex",
                    justifyContent: "space-between",
                    gap: "1rem",
                  }}
                >
                  <div>
                    <div className="selection-item-title">{person.nome}</div>
                    <div className="record-item-copy">{person.documento}</div>
                  </div>
                  <StatusBadge
                    value={person.ativo ? "ativo" : "inativo"}
                  />
                </div>
                <div className="record-item-copy">
                  {describeProfile(person)} • {person.email}
                </div>
                <div className="record-tags">
                  <span className="record-tag">
                    Relacao {person.relacaoLoja.statusRelacao}
                  </span>
                  {person.usuarioVinculado ? (
                    <span className="record-tag">
                      Usuario {person.usuarioVinculado.nome}
                    </span>
                  ) : null}
                  {person.financeiro.totalPendencias > 0 ? (
                    <span className="record-tag">
                      Pendencias {formatCurrency(person.financeiro.totalPendencias)}
                    </span>
                  ) : null}
                </div>
              </button>
            ))
          )}
        </div>
      </CardBody>
    </Card>
  );
}
