import { startTransition, type Dispatch, type SetStateAction } from "react";

import { Card, CardBody, CardHeading } from "@/components/ui/card";
import { TextInput } from "@/components/ui/field";
import { StatusBadge } from "@/components/ui/status-badge";
import { formatCurrency, formatDateTime } from "@/lib/helpers/formatters";
import type { CreditAccountSummary } from "@/lib/services/credits";

import { describeCreditProfile } from "@/app/(system)/credits/components/types";

type CreditAccountsPanelProps = {
  accounts: CreditAccountSummary[];
  search: string;
  selectedPersonId: string;
  setSearch: Dispatch<SetStateAction<string>>;
  onSelectPerson: (personId: string) => void;
};

// Lista e filtra as contas de credito da loja ativa.
export function CreditAccountsPanel({
  accounts,
  search,
  selectedPersonId,
  setSearch,
  onSelectPerson,
}: CreditAccountsPanelProps) {
  const normalizedSearch = search.trim().toLowerCase();
  const filteredAccounts = accounts.filter((account) => {
    if (!normalizedSearch) {
      return true;
    }

    return [account.nome, account.documento]
      .join(" ")
      .toLowerCase()
      .includes(normalizedSearch);
  });

  return (
    <Card>
      <CardBody className="section-stack">
        <CardHeading
          subtitle="Consulte contas ja abertas por pessoa dentro da loja ativa."
          title="Contas de credito"
        />

        <TextInput
          label="Busca rapida"
          onChange={(event) => {
            const nextValue = event.target.value;
            startTransition(() => setSearch(nextValue));
          }}
          placeholder="Nome ou documento"
          value={search}
        />

        <div className="record-list">
          {filteredAccounts.length === 0 ? (
            <div className="empty-state">
              Nenhuma conta de credito encontrada para o filtro atual.
            </div>
          ) : (
            filteredAccounts.map((account) => (
              <button
                className={`record-item${selectedPersonId === account.pessoaId ? " record-item-active" : ""}`}
                key={account.contaId}
                onClick={() => onSelectPerson(account.pessoaId)}
                type="button"
              >
                <div className="stock-record-header">
                  <div>
                    <div className="selection-item-title">{account.nome}</div>
                    <div className="record-item-copy">{account.documento}</div>
                  </div>
                  <StatusBadge value={account.statusConta} />
                </div>

                <div className="record-item-copy">{describeCreditProfile(account)}</div>

                <div className="record-tags">
                  <span className="record-tag">
                    Saldo {formatCurrency(account.saldoAtual)}
                  </span>
                  <span className="record-tag">
                    Disponivel {formatCurrency(account.saldoDisponivel)}
                  </span>
                  {account.saldoComprometido > 0 ? (
                    <span className="record-tag">
                      Comprometido {formatCurrency(account.saldoComprometido)}
                    </span>
                  ) : null}
                </div>

                <div className="record-item-copy">
                  Ultimo movimento {formatDateTime(account.ultimaMovimentacaoEm)}
                </div>
              </button>
            ))
          )}
        </div>
      </CardBody>
    </Card>
  );
}
