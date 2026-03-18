import { Card, CardBody } from "@/components/ui/card";
import type { CommercialRulesWorkspace } from "@/lib/services/commercial-rules";

// Resume o estado geral das configuracoes comerciais da loja ativa.
type CommercialRulesOverviewProps = {
  workspace?: CommercialRulesWorkspace;
};

const overviewCards = [
  {
    key: "store-rule",
    label: "Regra da loja",
    description: "Base padrao para pecas sem sobrescrita de fornecedor.",
  },
  {
    key: "supplier-rules",
    label: "Regras de fornecedor",
    description: "Excecoes comerciais aplicadas por fornecedor da loja.",
  },
  {
    key: "payment-methods",
    label: "Meios de pagamento",
    description: "Meios ativos usados na venda e na conciliacao futura.",
  },
  {
    key: "mixed-payment",
    label: "Pagamento misto",
    description: "Indicador rapido da regra padrao atual da loja.",
  },
] as const;

export function CommercialRulesOverview({
  workspace,
}: CommercialRulesOverviewProps) {
  const activePaymentMethods =
    workspace?.meiosPagamento.filter((method) => method.ativo).length ?? 0;

  const valuesByCard = {
    "store-rule": workspace?.regraLoja
      ? workspace.regraLoja.ativo
        ? "Ativa"
        : "Inativa"
      : "Pendente",
    "supplier-rules": String(workspace?.regrasFornecedor.length ?? 0),
    "payment-methods": String(activePaymentMethods),
    "mixed-payment": workspace?.regraLoja?.permitePagamentoMisto ? "Liberado" : "Desligado",
  } as const;

  return (
    <div className="rule-overview-shell">
      <div className="rule-overview-copy">
        <p className="rule-overview-eyebrow">Configuracoes comerciais</p>
        <h2 className="rule-overview-title">
          Loja ativa {workspace?.lojaNome ?? "-"}
        </h2>
        <p className="rule-overview-subtitle">
          Mantenha a regra padrao da loja, as sobrescritas por fornecedor e os
          meios de pagamento usados nas proximas etapas operacionais do sistema.
        </p>
      </div>

      <div className="rule-overview-grid">
        {overviewCards.map((card) => (
          <Card className="rule-overview-card" key={card.key}>
            <CardBody className="rule-overview-card-body">
              <span className="rule-overview-card-label">{card.label}</span>
              <strong className="rule-overview-card-value">
                {valuesByCard[card.key]}
              </strong>
              <span className="rule-overview-card-description">
                {card.description}
              </span>
            </CardBody>
          </Card>
        ))}
      </div>
    </div>
  );
}
