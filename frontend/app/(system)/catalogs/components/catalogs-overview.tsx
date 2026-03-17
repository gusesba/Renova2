import { Card, CardBody } from "@/components/ui/card";
import { cx } from "@/lib/helpers/classnames";
import type { CatalogWorkspace } from "@/lib/services/catalogs";

import type { CatalogEntryType } from "./types";

// Renderiza os cards-resumo do topo e permite trocar o foco do modulo.
type CatalogsOverviewProps = {
  workspace?: CatalogWorkspace;
  selectedType: CatalogEntryType;
  onSelectType: (type: CatalogEntryType) => void;
};

const catalogSummaryMap: Array<{
  type: CatalogEntryType;
  label: string;
  description: string;
}> = [
  {
    type: "produtoNome",
    label: "Produtos",
    description: "Nomes base usados no cadastro das pecas.",
  },
  {
    type: "marca",
    label: "Marcas",
    description: "Marcas cadastradas para a loja ativa.",
  },
  {
    type: "cor",
    label: "Cores",
    description: "Cores disponiveis para classificacao visual.",
  },
  {
    type: "tamanho",
    label: "Tamanhos",
    description: "Tamanhos disponiveis para a loja ativa.",
  },
];

export function CatalogsOverview({
  workspace,
  selectedType,
  onSelectType,
}: CatalogsOverviewProps) {
  const countsByType: Record<CatalogEntryType, number> = {
    produtoNome: workspace?.produtoNomes.length ?? 0,
    marca: workspace?.marcas.length ?? 0,
    cor: workspace?.cores.length ?? 0,
    tamanho: workspace?.tamanhos.length ?? 0,
  };

  return (
    <div className="catalogs-hero">
      <div className="catalogs-hero-copy">
        <p className="catalogs-hero-eyebrow">Cadastros auxiliares</p>
        <h2 className="catalogs-hero-title">Loja ativa {workspace?.lojaAtivaNome ?? "-"}</h2>
        <p className="catalogs-hero-subtitle">
          Escolha um grupo para consultar a lista e manter os nomes base usados no
          cadastro das pecas.
        </p>
      </div>

      <div className="catalogs-summary-grid">
        {catalogSummaryMap.map((item) => (
          <button
            className={cx(
              "catalogs-summary-button",
              selectedType === item.type && "catalogs-summary-button-active",
            )}
            key={item.type}
            onClick={() => onSelectType(item.type)}
            type="button"
          >
            <Card className="catalogs-summary-card">
              <CardBody className="catalogs-summary-card-body">
                <span className="catalogs-summary-label">{item.label}</span>
                <strong className="catalogs-summary-value">
                  {countsByType[item.type]}
                </strong>
                <span className="catalogs-summary-description">
                  {item.description}
                </span>
              </CardBody>
            </Card>
          </button>
        ))}
      </div>
    </div>
  );
}
