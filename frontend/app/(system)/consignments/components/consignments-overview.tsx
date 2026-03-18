import { Card, CardBody } from "@/components/ui/card";
import type { ConsignmentWorkspace } from "@/lib/services/consignments";

// Resume o estado geral das consignacoes da loja ativa.
type ConsignmentsOverviewProps = {
  workspace?: ConsignmentWorkspace;
};

export function ConsignmentsOverview({ workspace }: ConsignmentsOverviewProps) {
  const cards = [
    {
      label: "Ativas",
      value: workspace?.resumo.totalAtivas ?? 0,
      description: "Pecas consignadas ainda em acompanhamento operacional.",
    },
    {
      label: "Proximas do fim",
      value: workspace?.resumo.proximasDoFim ?? 0,
      description: "Itens dentro da janela curta para devolucao ou ajuste.",
    },
    {
      label: "Vencidas",
      value: workspace?.resumo.vencidas ?? 0,
      description: "Pecas que ultrapassaram o prazo congelado na entrada.",
    },
    {
      label: "Desconto pendente",
      value: workspace?.resumo.comDescontoPendente ?? 0,
      description: "Itens com faixa de desconto vencida e ainda nao refletida.",
    },
  ];

  return (
    <div className="rule-overview-shell">
      <div className="rule-overview-copy">
        <p className="rule-overview-eyebrow">Modulo 07</p>
        <h2 className="rule-overview-title">Ciclo de vida da consignacao</h2>
        <p className="rule-overview-subtitle">
          Acompanhe prazo, alertas, desconto automatico e encerramento das pecas
          consignadas da loja ativa.
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
