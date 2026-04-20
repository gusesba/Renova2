import { AuthIllustration } from "./illustration";

export function AuthLeftPanel() {
  return (
    <section className="relative flex flex-col gap-6 bg-[#eef1ff] p-5 sm:p-8 lg:justify-between lg:p-12">
      <div>
        <span className="inline-flex rounded-full bg-white/80 px-3 py-2 text-xs font-semibold tracking-[0.18em] text-[#5f5af1] uppercase shadow-sm sm:px-4 sm:text-sm">
          Renova
        </span>
      </div>

      <div className="relative mx-auto flex w-full max-w-[280px] items-center justify-center py-2 sm:max-w-md sm:py-6 lg:py-8">
        <div className="absolute inset-x-6 top-1/2 h-36 -translate-y-1/2 rounded-full bg-[radial-gradient(circle,_rgba(111,103,242,0.18)_0%,_rgba(111,103,242,0)_72%)] blur-2xl sm:inset-x-10 sm:h-52" />
        <AuthIllustration />
      </div>

      <div className="space-y-3 text-[#544cbf]">
        <p className="text-2xl font-semibold leading-tight sm:text-3xl lg:text-4xl">Renova</p>
        <p className="max-w-sm text-sm leading-6 text-[#7c76c9] sm:text-base">
          Acesse sua conta e organize sua rotina com uma experiencia leve, limpa e direta.
        </p>
      </div>
    </section>
  );
}
