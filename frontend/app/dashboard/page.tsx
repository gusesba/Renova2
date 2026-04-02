export default function DashboardPage() {
  return (
    <section className="space-y-6">
      <div className="rounded-[28px] border border-[var(--border)] bg-white p-6 shadow-[0_18px_45px_rgba(15,23,42,0.05)]">
        <p className="text-sm font-semibold uppercase tracking-[0.18em] text-[var(--muted)]">
          Visao geral
        </p>
        <h1 className="mt-3 text-3xl font-semibold tracking-tight text-[var(--foreground)]">
          Estrutura principal do frontend
        </h1>
        <p className="mt-3 max-w-2xl text-sm leading-7 text-[var(--muted)]">
          Sidebar e header implementados como base compartilhada para as paginas autenticadas.
        </p>
      </div>

      <div className="grid gap-5 xl:grid-cols-3">
        <div className="rounded-[24px] border border-[var(--border)] bg-white p-5 shadow-[0_18px_45px_rgba(15,23,42,0.05)] xl:col-span-2">
          <div className="h-64 rounded-[20px] border border-dashed border-[var(--border-strong)] bg-[linear-gradient(180deg,_#fcfdff_0%,_#f5f7fc_100%)]" />
        </div>
        <div className="rounded-[24px] border border-[var(--border)] bg-white p-5 shadow-[0_18px_45px_rgba(15,23,42,0.05)]">
          <div className="h-64 rounded-[20px] border border-dashed border-[var(--border-strong)] bg-[linear-gradient(180deg,_#fcfdff_0%,_#f5f7fc_100%)]" />
        </div>
      </div>
    </section>
  );
}
