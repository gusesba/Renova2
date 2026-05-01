import { getPaymentApiMessage } from "@/lib/payment";
import { getMyPendingBalances } from "@/services/payment-service";

export type ClientPendingItem = {
  lojaId: number;
  lojaNome: string;
  clienteId: number;
  saldoConta: number;
  valorCredito: number;
  valorEspecie: number | null;
  situacao: "Pagar" | "Receber" | string;
};

export function asClientPendingResponse(body: unknown) {
  return (body as ClientPendingItem[])
    .map((item) => ({
      ...item,
      valorEspecie:
        typeof item.valorEspecie === "number"
          ? item.valorEspecie
          : item.valorEspecie === null
            ? null
            : null,
    }))
    .sort((current, next) => next.clienteId - current.clienteId || next.lojaId - current.lojaId);
}

export async function getClientPendingBalances(token: string) {
  const response = await getMyPendingBalances(token);

  if (!response.ok) {
    throw new Error(
      getPaymentApiMessage(response.body) ?? "Nao foi possivel carregar as pendencias do cliente.",
    );
  }

  return asClientPendingResponse(response.body);
}
