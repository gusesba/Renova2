type SolicitacaoEmptyStateProps = {
  title: string;
  description: string;
};

export function SolicitacaoEmptyState({ title, description }: SolicitacaoEmptyStateProps) {
  return (
    <div className="mt-6 rounded-[24px] border border-dashed border-[var(--border)] bg-[var(--surface-muted)] px-6 py-10 text-center">
      <p className="text-base font-semibold text-[var(--foreground)]">{title}</p>
      <p className="mx-auto mt-2 max-w-2xl text-sm leading-7 text-[var(--muted)]">{description}</p>
    </div>
  );
}
