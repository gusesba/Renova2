import type { LojaResponse } from "@/lib/store";

type StoreRegistrationResponseProps = {
  currentStore?: LojaResponse | null;
  latestStore: LojaResponse | null;
};

export function StoreRegistrationResponse({
  currentStore = null,
  latestStore,
}: StoreRegistrationResponseProps) {
  return (
    <div className="rounded-[28px] border border-[var(--border)] bg-white p-6 shadow-[0_18px_45px_rgba(15,23,42,0.05)]">
      <p className="text-sm font-semibold uppercase tracking-[0.18em] text-[var(--muted)]">Resumo</p>
      {currentStore ? (
        <div className="mt-4 rounded-[24px] border border-[var(--border)] bg-[linear-gradient(180deg,_#f6fbff_0%,_#fff_100%)] p-5">
          <p className="text-xs font-semibold uppercase tracking-[0.18em] text-[#3b82f6]">
            Loja atual
          </p>
          <p className="mt-3 text-2xl font-semibold text-[var(--foreground)]">{currentStore.nome}</p>
          <p className="mt-2 text-sm text-[var(--muted)]">Identificador selecionado: #{currentStore.id}</p>
        </div>
      ) : null}
      {latestStore ? (
        <div className={`${currentStore ? "mt-4" : "mt-4"} rounded-[24px] bg-[linear-gradient(180deg,_#fff7f0_0%,_#fff_100%)] p-5`}>
          <p className="text-xs font-semibold uppercase tracking-[0.18em] text-[#c56c2e]">
            Loja criada
          </p>
          <p className="mt-3 text-2xl font-semibold text-[var(--foreground)]">{latestStore.nome}</p>
          <p className="mt-2 text-sm text-[var(--muted)]">
            Identificador gerado: #{latestStore.id}
          </p>
        </div>
      ) : !currentStore ? (
        <p className="mt-4 text-sm leading-7 text-[var(--muted)]">
          Quando a loja for criada, ela aparecera aqui.
        </p>
      ) : null}
    </div>
  );
}
