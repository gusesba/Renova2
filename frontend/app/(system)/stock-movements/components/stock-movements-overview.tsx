import { Card, CardBody } from "@/components/ui/card";
import type { StockMovementWorkspace } from "@/lib/services/stock-movements";

// Resume o estado operacional do modulo 08 na loja ativa.
type StockMovementsOverviewProps = {
  workspace?: StockMovementWorkspace;
};

export function StockMovementsOverview({
  workspace,
}: StockMovementsOverviewProps) {
  const cards = [
    {
      label: "Movimentacoes",
      value: workspace?.resumo.totalMovimentacoes ?? 0,
      description: "Total historico de entradas, saidas e ajustes da loja ativa.",
    },
    {
      label: "Ajustes manuais",
      value: workspace?.resumo.ajustesManuais ?? 0,
      description: "Eventos registrados manualmente com trilha e responsavel.",
    },
    {
      label: "Pecas com saldo",
      value: workspace?.resumo.pecasComSaldo ?? 0,
      description: "Itens que ainda possuem quantidade disponivel em estoque.",
    },
    {
      label: "Pecas zeradas",
      value: workspace?.resumo.pecasSemSaldo ?? 0,
      description: "Itens atualmente sem saldo para operacao ou venda.",
    },
  ];

  return (
    <div className="rule-overview-shell">
      <div className="rule-overview-copy">
        <p className="rule-overview-eyebrow">Modulo 08</p>
        <h2 className="rule-overview-title">Movimentacoes de estoque</h2>
        <p className="rule-overview-subtitle">
          Consulte o historico operacional da loja ativa, localize pecas por
          codigo ou fornecedor e registre ajustes manuais com permissao
          especifica.
        </p>
      </div>

      <div className="rule-overview-grid">
        {cards.map((card) => (
          <Card key={card.label}>
            <CardBody className="rule-overview-card-body">
              <span className="rule-overview-card-label">{card.label}</span>
              <strong className="rule-overview-card-value">{card.value}</strong>
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
