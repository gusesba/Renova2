import { Card, CardBody } from "@/components/ui/card";
import type { PieceSummary } from "@/lib/services/pieces";

// Resume o estado geral do estoque da loja ativa na pagina de pecas.
type PiecesOverviewProps = {
  pieces: PieceSummary[];
};

export function PiecesOverview({ pieces }: PiecesOverviewProps) {
  const availableCount = pieces.filter(
    (piece) => piece.statusPeca === "disponivel",
  ).length;
  const consignedCount = pieces.filter(
    (piece) => piece.tipoPeca === "consignada",
  ).length;
  const withBarcodeCount = pieces.filter((piece) => piece.codigoBarras).length;

  const cards = [
    {
      label: "Pecas listadas",
      value: pieces.length,
      description: "Total de registros encontrados na loja ativa.",
    },
    {
      label: "Disponiveis",
      value: availableCount,
      description: "Itens aptos para venda ou reserva neste momento.",
    },
    {
      label: "Consignadas",
      value: consignedCount,
      description: "Pecas com ciclo comercial ligado ao fornecedor.",
    },
    {
      label: "Com codigo de barras",
      value: withBarcodeCount,
      description: "Itens ja preparados para leitura operacional rapida.",
    },
  ];

  return (
    <div className="rule-overview-shell">
      <div className="rule-overview-copy">
        <p className="rule-overview-eyebrow">Cadastro e estoque</p>
        <h2 className="rule-overview-title">Pecas da loja ativa</h2>
        <p className="rule-overview-subtitle">
          Consulte o estoque atual, cadastre novas pecas e mantenha o snapshot
          comercial de cada entrada.
        </p>
      </div>

      <div className="rule-overview-grid">
        {cards.map((card) => (
          <Card className="rule-overview-card" key={card.label}>
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
