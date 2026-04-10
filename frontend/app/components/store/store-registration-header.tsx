type StoreRegistrationHeaderProps = {
  canEditCurrentStore?: boolean;
  currentStoreName?: string | null;
  onEditCurrentStore?: () => void;
};

export function StoreRegistrationHeader({
  canEditCurrentStore = false,
  currentStoreName = null,
  onEditCurrentStore,
}: StoreRegistrationHeaderProps) {
  return (
    <div className="rounded-[30px] border border-[var(--border)] bg-[linear-gradient(135deg,_#fff8e8_0%,_#fffdf6_45%,_#eef6ff_100%)] px-6 py-8 shadow-[0_20px_60px_rgba(15,23,42,0.08)] lg:px-8">
      <div className="flex flex-col gap-4 lg:flex-row lg:items-end lg:justify-between">
        <div>
          <p className="text-xs font-semibold uppercase tracking-[0.22em] text-[#b67a18]">
            Cadastro de loja
          </p>
          <h1 className="mt-3 text-3xl font-semibold tracking-tight text-[#2a3247] sm:text-4xl">
            Crie sua loja
          </h1>
          <p className="mt-3 max-w-xl text-sm leading-7 text-[#6d7892] sm:text-base">
            Informe o nome da sua loja para continuar.
          </p>
        </div>

        {canEditCurrentStore ? (
          <button
            type="button"
            onClick={onEditCurrentStore}
            className="inline-flex h-12 cursor-pointer items-center justify-center rounded-2xl border border-[var(--border-strong)] bg-white/85 px-5 text-sm font-semibold text-[#2a3247] shadow-[0_14px_28px_rgba(42,50,71,0.08)] transition hover:-translate-y-0.5 hover:bg-white"
          >
            {currentStoreName ? `Editar loja atual: ${currentStoreName}` : "Editar loja atual"}
          </button>
        ) : null}
      </div>
    </div>
  );
}
