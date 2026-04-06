type ClientEmptyStateProps = {
  title: string;
  description: string;
};

export function ClientEmptyState({ title, description }: ClientEmptyStateProps) {
  return (
    <div className="mt-6 rounded-[24px] border border-dashed border-[var(--border-strong)] bg-[linear-gradient(180deg,_#fcfdff_0%,_#f5f7fc_100%)] px-6 py-14 text-center">
      <p className="text-lg font-semibold text-[var(--foreground)]">{title}</p>
      <p className="mx-auto mt-3 max-w-xl text-sm leading-7 text-[var(--muted)]">{description}</p>
    </div>
  );
}
