import { ClientDetailPage } from "@/app/components/client/client-detail-page";

type ClientDetailRouteProps = {
  params: Promise<{
    id: string;
  }>;
};

export default async function ClientDetailRoute({ params }: ClientDetailRouteProps) {
  const { id } = await params;

  return <ClientDetailPage clientId={Number(id)} />;
}
