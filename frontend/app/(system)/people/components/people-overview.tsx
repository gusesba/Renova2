import { MetricCard } from "@/components/ui/metric-card";
import { formatCurrency } from "@/lib/helpers/formatters";
import type { PersonSummary } from "@/lib/services/people";

// Resume o volume operacional do modulo de pessoas para a loja ativa.
type PeopleOverviewProps = {
  people: PersonSummary[];
};

export function PeopleOverview({ people }: PeopleOverviewProps) {
  const clientes = people.filter((person) => person.relacaoLoja.ehCliente).length;
  const fornecedores = people.filter(
    (person) => person.relacaoLoja.ehFornecedor,
  ).length;
  const pendencias = people.reduce(
    (total, person) => total + person.financeiro.totalPendencias,
    0,
  );

  return (
    <div className="metrics-grid">
      <MetricCard
        meta="cadastros vinculados a loja ativa"
        title="Pessoas"
        value={String(people.length)}
      />
      <MetricCard
        meta="cadastros com perfil de cliente"
        title="Clientes"
        value={String(clientes)}
      />
      <MetricCard
        meta="cadastros com perfil de fornecedor"
        title="Fornecedores"
        value={String(fornecedores)}
      />
      <MetricCard
        meta="saldo pendente atual"
        title="Pendencias"
        value={formatCurrency(pendencias)}
      />
    </div>
  );
}
