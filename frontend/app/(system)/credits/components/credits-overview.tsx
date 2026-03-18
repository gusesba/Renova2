import { MetricCard } from "@/components/ui/metric-card";
import { formatCurrency } from "@/lib/helpers/formatters";
import type { CreditsWorkspace } from "@/lib/services/credits";

type CreditsOverviewProps = {
  workspace?: CreditsWorkspace;
};

// Resume o modulo com indicadores rapidos de saldo e base de contas.
export function CreditsOverview({ workspace }: CreditsOverviewProps) {
  const accounts = workspace?.contas ?? [];
  const activeAccounts = accounts.filter((account) => account.statusConta === "ativa");
  const blockedAccounts = accounts.filter(
    (account) => account.statusConta === "bloqueada",
  );
  const totalBalance = accounts.reduce((sum, account) => sum + account.saldoAtual, 0);
  const totalCommitted = accounts.reduce(
    (sum, account) => sum + account.saldoComprometido,
    0,
  );

  return (
    <div className="catalogs-summary-grid">
      <MetricCard
        meta={`${workspace?.pessoas.length ?? 0} pessoas relacionadas na loja`}
        title="Contas de credito"
        value={String(accounts.length)}
      />
      <MetricCard
        meta={`${activeAccounts.length} ativas e ${blockedAccounts.length} bloqueadas`}
        title="Saldo total"
        value={formatCurrency(totalBalance)}
      />
      <MetricCard
        meta={`${activeAccounts.length} contas aptas a receber novos lancamentos`}
        title="Saldo comprometido"
        value={formatCurrency(totalCommitted)}
      />
      <MetricCard
        meta={`${workspace?.lojaNome ?? "Loja ativa"} como contexto da consulta`}
        title="Saldo disponivel"
        value={formatCurrency(totalBalance - totalCommitted)}
      />
    </div>
  );
}
