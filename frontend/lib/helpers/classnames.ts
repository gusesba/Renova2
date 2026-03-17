// Junta classes opcionais sem adicionar dependencias extras para algo simples.
export function cx(...values: Array<string | false | null | undefined>) {
  return values.filter(Boolean).join(" ");
}
