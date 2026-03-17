import { formatStatus, getStatusTone } from "@/lib/helpers/formatters";

// Badge visual para refletir o status sem espalhar regra de cor pela UI.
type StatusBadgeProps = {
  value: string;
};

export function StatusBadge({ value }: StatusBadgeProps) {
  return (
    <span className="status-badge" data-tone={getStatusTone(value)}>
      {formatStatus(value)}
    </span>
  );
}
