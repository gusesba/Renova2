import { AuthIllustration } from "./illustration";

export function AuthLeftPanel() {
  return (
    <section className="relative flex flex-col justify-between bg-[#eef1ff] p-8 sm:p-10 lg:p-12">
      <div>
        <span className="inline-flex rounded-full bg-white/80 px-4 py-2 text-sm font-semibold tracking-[0.18em] text-[#5f5af1] uppercase shadow-sm">
          Renova
        </span>
      </div>

      <div className="relative mx-auto flex w-full max-w-md items-center justify-center py-8">
        <div className="absolute inset-x-10 top-1/2 h-52 -translate-y-1/2 rounded-full bg-[radial-gradient(circle,_rgba(111,103,242,0.18)_0%,_rgba(111,103,242,0)_72%)] blur-2xl" />
        <AuthIllustration />
      </div>

      <div className="space-y-3 text-[#544cbf]">
        <p className="text-3xl font-semibold leading-tight sm:text-4xl">Renova</p>
        <p className="max-w-sm text-sm leading-6 text-[#7c76c9] sm:text-base">
          Acesse sua conta e organize sua rotina com uma experiencia leve, limpa e direta.
        </p>
      </div>
    </section>
  );
}
