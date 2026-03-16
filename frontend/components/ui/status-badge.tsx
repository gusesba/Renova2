import { formatStatus, getStatusTone } from "@/lib/helpers/formatters";

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
