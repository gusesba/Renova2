"use client";

import { useRouter } from "next/navigation";

import { clearAuthSession } from "@/lib/auth";

type LicensePageProps = {
  title: string;
  description: string;
};

export function LicensePage({ title, description }: LicensePageProps) {
  const router = useRouter();

  function handleLogout() {
    clearAuthSession();
    router.replace("/auth");
  }

  return (
    <main className="flex min-h-[100dvh] items-center justify-center bg-[#f6f7fb] px-4 py-8">
      <section className="w-full max-w-2xl rounded-[24px] border border-[#d9ddfb] bg-white px-6 py-8 shadow-[0_24px_70px_rgba(55,35,143,0.14)] sm:px-10 sm:py-10">
        <div className="space-y-4">
          <p className="text-xs font-semibold uppercase tracking-[0.16em] text-[#7f79c8]">
            Renova
          </p>
          <h1 className="text-3xl font-semibold text-[#35315f] sm:text-4xl">
            {title}
          </h1>
          <p className="text-base leading-7 text-[#686284]">
            {description}
          </p>
        </div>

        <div className="mt-8 flex flex-col gap-3 sm:flex-row">
          <a
            href="https://renovacuritiba.com.br"
            className="flex h-12 items-center justify-center rounded-2xl bg-[linear-gradient(90deg,_#6a63f4,_#5a52eb)] px-5 text-sm font-semibold !text-white shadow-[0_16px_28px_rgba(91,83,235,0.28)] transition visited:!text-white hover:brightness-105"
          >
            Visualizar funcionalidades
          </a>
          <button
            type="button"
            onClick={handleLogout}
            className="flex h-12 items-center justify-center rounded-2xl border border-[#d9ddfb] px-5 text-sm font-semibold text-[#5b53eb] transition hover:bg-[#f4f6ff]"
          >
            Voltar para login
          </button>
        </div>
      </section>
    </main>
  );
}
