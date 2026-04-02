import Link from "next/link";

type NavItem = {
  label: string;
  href: string;
  active?: boolean;
};

const primaryItems: NavItem[] = [
  { label: "Início", href: "/dashboard", active: true },
  { label: "Relatórios", href: "/relatorios" },
  { label: "Faturas", href: "/faturas" },
  { label: "Transações", href: "/transacoes" },
  { label: "Agenda", href: "/agenda" },
];

const secondaryItems: NavItem[] = [
  { label: "Pedidos", href: "/pedidos" },
  { label: "Configurações", href: "/configuracoes" },
];

function HexagonMark() {
  return (
    <div className="relative h-11 w-11 shrink-0">
      <div className="absolute inset-0 rotate-45 rounded-[18px] bg-[linear-gradient(135deg,_#6a5cff,_#8b7dff)] shadow-[0_18px_30px_rgba(106,92,255,0.25)]" />
      <div className="absolute inset-[7px] rotate-45 rounded-[12px] border border-white/70 bg-white/20 backdrop-blur-sm" />
      <div className="absolute inset-0 flex items-center justify-center text-sm font-bold text-white">R</div>
    </div>
  );
}

function SidebarIcon() {
  return (
    <span className="flex h-9 w-9 items-center justify-center rounded-xl border border-[var(--border)] bg-white text-[var(--muted)]">
      <span className="grid h-3.5 w-3.5 grid-cols-2 gap-0.5">
        <span className="rounded-[2px] bg-current/90" />
        <span className="rounded-[2px] bg-current/65" />
        <span className="rounded-[2px] bg-current/65" />
        <span className="rounded-[2px] bg-current/90" />
      </span>
    </span>
  );
}

function SidebarLink({ item }: { item: NavItem }) {
  const activeClass = item.active
    ? "border-[var(--primary)] bg-[var(--primary-soft)] text-[var(--primary)] shadow-[0_16px_24px_rgba(106,92,255,0.14)]"
    : "border-transparent text-[var(--muted)] hover:border-[var(--border)] hover:bg-white hover:text-[var(--foreground)]";

  const iconClass = item.active
    ? "border-[color:color-mix(in_srgb,var(--primary)_18%,white)] bg-white text-[var(--primary)]"
    : "border-[var(--border)] bg-white text-[var(--muted)]";

  return (
    <Link
      href={item.href}
      className={`flex items-center gap-3 rounded-2xl border px-3 py-3 text-sm font-medium transition ${activeClass}`}
    >
      <span className={`flex h-10 w-10 items-center justify-center rounded-xl border ${iconClass}`}>
        <span className="h-2.5 w-2.5 rounded-full bg-current" />
      </span>
      <span>{item.label}</span>
    </Link>
  );
}

export function AppSidebar() {
  return (
    <aside className="hidden h-full w-[290px] shrink-0 overflow-y-auto border-r border-[var(--border)] bg-[var(--surface)] px-5 py-6 lg:flex lg:flex-col">
      <div className="flex items-center gap-3 px-2">
        <HexagonMark />
        <div>
          <p className="text-lg font-semibold tracking-tight text-[var(--foreground)]">Renova</p>
          <p className="text-sm text-[var(--muted)]">Painel principal</p>
        </div>
      </div>

      <nav className="mt-10 flex flex-col gap-2">
        {primaryItems.map((item) => (
          <SidebarLink key={item.label} item={item} />
        ))}
      </nav>

      <div className="mt-8 border-t border-[var(--border)] pt-8">
        <p className="px-3 text-xs font-semibold uppercase tracking-[0.18em] text-[var(--muted)]">
          Gestão
        </p>
        <nav className="mt-3 flex flex-col gap-2">
          {secondaryItems.map((item) => (
            <SidebarLink key={item.label} item={item} />
          ))}
        </nav>
      </div>

      <div className="mt-auto rounded-[28px] bg-[linear-gradient(180deg,_#f8f6ff_0%,_#eef2ff_100%)] p-5">
        <div className="flex items-center gap-3">
          <SidebarIcon />
          <div>
            <p className="text-sm font-semibold text-[var(--foreground)]">Espaço de trabalho</p>
            <p className="text-xs text-[var(--muted)]">Operação central</p>
          </div>
        </div>
        <p className="mt-4 text-sm leading-6 text-[var(--muted)]">
          Estrutura base pronta para receber as telas autenticadas do sistema.
        </p>
        <button
          type="button"
          className="mt-5 w-full rounded-2xl bg-[var(--primary)] px-4 py-3 text-sm font-semibold text-white transition hover:brightness-105"
        >
          Ver visão geral
        </button>
      </div>
    </aside>
  );
}
