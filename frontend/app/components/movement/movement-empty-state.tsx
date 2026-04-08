type MovementEmptyStateProps = {
  title: string;
  description: string;
};

export function MovementEmptyState({ title, description }: MovementEmptyStateProps) {
  return (
    <div className="mt-6 rounded-[24px] border border-dashed border-[var(--border-strong)] bg-[var(--surface-muted)] px-6 py-12 text-center">
      <h3 className="text-xl font-semibold text-[var(--foreground)]">{title}</h3>
      <p className="mt-3 text-sm leading-7 text-[var(--muted)]">{description}</p>
    </div>
  );
}
