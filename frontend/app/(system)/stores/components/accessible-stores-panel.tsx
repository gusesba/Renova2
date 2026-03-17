import type { Dispatch, SetStateAction } from "react";

import { Card, CardBody, CardHeading } from "@/components/ui/card";
import { StatusBadge } from "@/components/ui/status-badge";
import type { StoreSummary } from "@/lib/services/renova-api";

// Lista consolidada das lojas acessiveis ao usuario com selecao rapida para edicao.
type AccessibleStoresPanelProps = {
  selectedStoreId: string;
  setSelectedStoreId: Dispatch<SetStateAction<string>>;
  stores: StoreSummary[];
};

export function AccessibleStoresPanel({
  selectedStoreId,
  setSelectedStoreId,
  stores,
}: AccessibleStoresPanelProps) {
  return (
    <Card>
      <CardBody className="section-stack">
        <CardHeading
          subtitle="Mostra apenas lojas vinculadas ao usuario e destaca a loja ativa do momento."
          title="Visao consolidada por usuario"
        />

        <div className="record-list">
          {stores.length === 0 ? (
            <div className="empty-state">
              Nenhuma loja vinculada ainda. Crie a primeira loja para iniciar a
              estrutura operacional.
            </div>
          ) : (
            stores.map((store) => (
              <button
                className="record-item"
                key={store.id}
                onClick={() => setSelectedStoreId(store.id)}
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
                    <div className="selection-item-title">
                      {store.nomeFantasia}
                    </div>
                    <div className="record-item-copy">
                      {store.cidade} / {store.uf}
                    </div>
                  </div>
                  <StatusBadge value={store.statusLoja} />
                </div>

                <div className="record-tags">
                  {store.ehLojaAtiva ? (
                    <span className="record-tag">Loja ativa</span>
                  ) : null}
                  {store.ehResponsavel ? (
                    <span className="record-tag">Responsavel</span>
                  ) : null}
                  {store.podeGerenciar ? (
                    <span className="record-tag">Pode gerenciar</span>
                  ) : null}
                  {selectedStoreId === store.id ? (
                    <span className="record-tag">Selecionada</span>
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
