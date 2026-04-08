type ProductEmptyStateProps = {
  title: string;
  description: string;
};

export function ProductEmptyState({ title, description }: ProductEmptyStateProps) {
  return (
    <div className="mt-6 rounded-[24px] border border-dashed border-[var(--border)] bg-[var(--surface-muted)] px-6 py-10 text-center">
      <h3 className="text-lg font-semibold text-[var(--foreground)]">{title}</h3>
      <p className="mx-auto mt-2 max-w-2xl text-sm leading-7 text-[var(--muted)]">{description}</p>
    </div>
  );
}
