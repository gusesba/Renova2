import { redirect } from "next/navigation";

// A raiz publica apenas redireciona para o fluxo de autenticacao.
export default function HomePage() {
  redirect("/login");
}
