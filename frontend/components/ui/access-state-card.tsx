import { Card, CardBody, CardHeading } from "@/components/ui/card";

// Exibe estados simples de acesso negado ou indisponivel de forma consistente no shell.
type AccessStateCardProps = {
  title: string;
  subtitle: string;
  message: string;
};

export function AccessStateCard({
  title,
  subtitle,
  message,
}: AccessStateCardProps) {
  return (
    <Card>
      <CardBody className="section-stack">
        <CardHeading subtitle={subtitle} title={title} />
        <div className="empty-state">{message}</div>
      </CardBody>
    </Card>
  );
}
