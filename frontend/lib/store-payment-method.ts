export function formatPaymentMethodAdjustment(value: string): string {
  const parsedValue = Number(value.replace(",", ".").trim());

  if (Number.isNaN(parsedValue)) {
    return `${value}%`;
  }

  if (parsedValue > 0) {
    return `+${parsedValue}% de taxa`;
  }

  if (parsedValue < 0) {
    return `${parsedValue}% de desconto`;
  }

  return "0% sem ajuste";
}
