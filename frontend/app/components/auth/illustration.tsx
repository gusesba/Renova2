export function AuthIllustration() {
  return (
    <div className="relative w-full max-w-[380px] rounded-[24px] border border-white/70 bg-[linear-gradient(180deg,_rgba(255,255,255,0.75),_rgba(224,229,255,0.92))] p-4 shadow-[0_30px_55px_rgba(111,103,242,0.16)] sm:rounded-[32px] sm:p-6">
      <div className="absolute left-4 top-4 flex gap-2 sm:left-6 sm:top-6">
        <span className="h-2.5 w-2.5 rounded-full bg-[#ff8b7b] sm:h-3 sm:w-3" />
        <span className="h-2.5 w-2.5 rounded-full bg-[#ffd46f] sm:h-3 sm:w-3" />
        <span className="h-2.5 w-2.5 rounded-full bg-[#75d39b] sm:h-3 sm:w-3" />
      </div>

      <div className="mt-6 rounded-[20px] bg-[linear-gradient(180deg,_#766ff7,_#4f44d7)] p-3 shadow-[0_20px_35px_rgba(79,68,215,0.3)] sm:mt-8 sm:rounded-[24px] sm:p-4">
        <div className="mb-3 flex items-center justify-between sm:mb-4">
          <div className="h-3 w-24 rounded-full bg-white/30" />
          <div className="h-8 w-8 rounded-xl bg-white/18" />
        </div>
        <div className="grid grid-cols-[1.15fr_0.85fr] gap-3 sm:gap-4">
          <div className="space-y-3">
            <div className="h-24 rounded-[18px] bg-white/16 p-4">
              <div className="h-3 w-18 rounded-full bg-white/30" />
              <div className="mt-4 space-y-2">
                <div className="h-2 w-full rounded-full bg-white/20" />
                <div className="h-2 w-4/5 rounded-full bg-white/20" />
                <div className="h-2 w-2/3 rounded-full bg-white/20" />
              </div>
            </div>
            <div className="flex h-24 items-end gap-2 rounded-[18px] bg-white/14 p-4">
              <div className="h-8 w-full rounded-full bg-[#ff8d7a]" />
              <div className="h-12 w-full rounded-full bg-[#ff8d7a]" />
              <div className="h-16 w-full rounded-full bg-[#9fd0ff]" />
              <div className="h-10 w-full rounded-full bg-[#ff8d7a]" />
            </div>
          </div>
          <div className="flex flex-col gap-3">
            <div className="flex-1 rounded-[18px] bg-white/16 p-4">
              <div className="grid grid-cols-2 gap-2">
                <div className="h-12 rounded-2xl bg-white/18" />
                <div className="h-12 rounded-2xl bg-[#ff8d7a]" />
                <div className="col-span-2 h-16 rounded-[18px] bg-[#8eb5ff]" />
              </div>
            </div>
            <div className="flex h-20 items-center justify-center rounded-[18px] bg-white/14">
              <div className="flex items-end gap-2">
                <div className="h-9 w-9 rounded-t-[12px] bg-[#ff8d7a]" />
                <div className="h-14 w-9 rounded-t-[12px] bg-[#ffd26d]" />
                <div className="h-11 w-9 rounded-t-[12px] bg-[#9fd0ff]" />
              </div>
            </div>
          </div>
        </div>
      </div>

      <div className="absolute -left-3 bottom-4 hidden h-16 w-16 rounded-[20px] bg-[#ff8d7a] p-3 shadow-[0_18px_30px_rgba(255,141,122,0.35)] sm:block sm:-left-5 sm:bottom-6 sm:h-20 sm:w-20 sm:rounded-[24px] sm:p-4">
        <div className="h-full rounded-[18px] border border-white/35">
          <div className="mx-auto mt-3 h-2 w-8 rounded-full bg-white/70" />
          <div className="mx-auto mt-3 h-7 w-7 rounded-full bg-white/80" />
        </div>
      </div>

      <div className="absolute -right-3 top-16 hidden h-16 w-16 rounded-[18px] bg-white p-3 shadow-[0_16px_28px_rgba(111,103,242,0.18)] sm:block sm:-right-4 sm:top-20 sm:h-18 sm:w-18 sm:rounded-[22px] sm:p-4">
        <div className="space-y-2">
          <div className="h-2 w-8 rounded-full bg-[#6c63ef]" />
          <div className="h-2 w-10 rounded-full bg-[#d6d9fa]" />
          <div className="h-8 rounded-2xl bg-[#eff1ff]" />
        </div>
      </div>
    </div>
  );
}
