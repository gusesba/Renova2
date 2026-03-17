import { Card, CardBody, CardHeading } from "@/components/ui/card";

// Card resumido para indicadores no topo da dashboard.
type MetricCardProps = {
  title: string;
  value: string;
  meta: string;
};

export function MetricCard({ title, value, meta }: MetricCardProps) {
  return (
    <Card className="metric-card">
      <CardBody>
        <CardHeading title={title} />
        <div className="metric-card-value">{value}</div>
        <div className="metric-card-meta">{meta}</div>
      </CardBody>
    </Card>
  );
}
