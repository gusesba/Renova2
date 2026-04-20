export default function Home() {
  const metrics = [
    { label: "Lojas conectadas", value: "24+" },
    { label: "Rotinas automatizadas", value: "126" },
    { label: "Visao em tempo real", value: "100%" },
  ];

  const features = [
    {
      title: "Operacao centralizada",
      description:
        "Controle loja, estoque, movimentacoes, pagamentos e fechamento em uma unica interface limpa e rapida.",
    },
    {
      title: "Fluxos por permissao",
      description:
        "Cada perfil acessa apenas o que precisa, reduzindo ruído operacional e melhorando a seguranca da rotina.",
    },
    {
      title: "Desktop e mobile",
      description:
        "Equipe e clientes acompanham a operacao com o mesmo idioma visual no desktop e no mobile.",
    },
  ];

  const employeeSteps = [
    "Acesso aos mesmos modulos do sistema web com navegacao adaptada ao celular.",
    "Consulta de clientes, produtos, movimentacoes e solicitacoes em qualquer momento.",
    "Registro e acompanhamento das rotinas da loja com a mesma clareza do desktop.",
  ];

  const clientSteps = [
    "Visualizacao das informacoes essenciais da conta em uma interface simples e direta.",
    "Acompanhamento das mesmas consultas e historicos ja disponiveis no sistema.",
    "Experiencia consistente entre web e mobile, sem mudar a logica de uso da plataforma.",
  ];

  const modules = [
    "Controle de acesso",
    "Clientes",
    "Produtos",
    "Pagamentos",
    "Movimentacoes",
    "Solicitacoes",
    "Fechamento",
    "Gestao da loja",
  ];

  return (
    <main className="relative overflow-hidden bg-[var(--background)]">
      <div className="absolute inset-0 landing-grid opacity-60" />
      <div className="absolute inset-x-0 top-0 h-[720px] bg-[radial-gradient(circle_at_top_left,_rgba(183,196,255,0.95)_0%,_rgba(143,153,243,0.75)_26%,_rgba(106,92,255,0.28)_54%,_rgba(244,246,251,0)_78%)]" />
      <div className="absolute -top-16 right-[8%] h-72 w-72 rounded-full bg-[rgba(106,92,255,0.20)] blur-3xl hero-orb" />
      <div className="absolute top-[36rem] -left-20 h-72 w-72 rounded-full bg-[rgba(47,201,143,0.12)] blur-3xl hero-orb-delay" />

      <section className="relative mx-auto flex min-h-screen w-full max-w-7xl flex-col px-6 pb-20 pt-8 lg:px-10 lg:pb-28 lg:pt-10">
        <header className="reveal-up flex items-center justify-between rounded-full border border-white/55 bg-white/70 px-4 py-3 shadow-[0_18px_40px_rgba(62,63,140,0.10)] backdrop-blur md:px-6">
          <div className="flex items-center gap-3">
            <div className="flex h-11 w-11 items-center justify-center rounded-2xl bg-[linear-gradient(135deg,_#6a5cff_0%,_#8d84ff_100%)] text-base font-semibold text-white shadow-[0_14px_30px_rgba(106,92,255,0.35)]">
              R
            </div>
            <div>
              <p className="text-sm font-semibold tracking-[0.22em] text-[var(--primary)] uppercase">
                Renova
              </p>
              <p className="text-xs text-[var(--secondary)]">
                Plataforma operacional conectada
              </p>
            </div>
          </div>

          <nav className="hidden items-center gap-8 text-sm font-medium text-[var(--secondary)] lg:flex">
            <a
              href="#sistema"
              className="rounded-full px-3 py-2 transition duration-200 hover:-translate-y-0.5 hover:bg-white/80 hover:text-[var(--foreground)] hover:shadow-[0_12px_24px_rgba(15,23,42,0.08)]"
            >
              Sistema
            </a>
            <a
              href="#mobile"
              className="rounded-full px-3 py-2 transition duration-200 hover:-translate-y-0.5 hover:bg-white/80 hover:text-[var(--foreground)] hover:shadow-[0_12px_24px_rgba(15,23,42,0.08)]"
            >
              Mobile
            </a>
            <a
              href="#fluxo"
              className="rounded-full px-3 py-2 transition duration-200 hover:-translate-y-0.5 hover:bg-white/80 hover:text-[var(--foreground)] hover:shadow-[0_12px_24px_rgba(15,23,42,0.08)]"
            >
              Fluxo
            </a>
            <a
              href="#contato"
              className="rounded-full px-3 py-2 transition duration-200 hover:-translate-y-0.5 hover:bg-white/80 hover:text-[var(--foreground)] hover:shadow-[0_12px_24px_rgba(15,23,42,0.08)]"
            >
              Orçamento
            </a>
          </nav>

          <div className="flex items-center gap-3">
            <a
              href="https://app.renovacuritiba.com.br"
              target="_blank"
              rel="noreferrer"
              className="inline-flex items-center justify-center rounded-full border border-[var(--border-strong)] bg-white/82 px-5 py-3 text-sm font-semibold text-[var(--foreground)] shadow-[0_12px_28px_rgba(15,23,42,0.06)] transition hover:translate-y-[-1px] hover:border-[var(--primary)] hover:text-[var(--primary)]"
            >
              Acessar portal
            </a>
            <a
              href="#demo"
              className="inline-flex items-center justify-center rounded-full bg-[var(--foreground)] px-5 py-3 text-sm font-semibold !text-white transition hover:translate-y-[-1px] hover:bg-[#18233b] hover:!text-white"
            >
              Ver demonstracao
            </a>
          </div>
        </header>

        <div className="grid flex-1 items-center gap-16 py-14 lg:grid-cols-[1.02fr_0.98fr] lg:py-20">
          <div className="max-w-2xl">
            <p className="reveal-up-delay mt-8 text-sm font-semibold tracking-[0.34em] text-[var(--primary)] uppercase">
              Renova
            </p>
            <h1 className="reveal-up-delay mt-4 text-5xl font-semibold leading-[1.02] tracking-[-0.05em] text-[var(--foreground)] sm:text-6xl lg:text-7xl">
              <span className="bg-[linear-gradient(135deg,_#6a5cff_0%,_#5a52eb_100%)] bg-clip-text text-transparent">
                Renova
              </span>{" "}
              e o sistema completo para brechos que trabalham com venda
              consignada.
            </h1>

            <p className="reveal-up-delay-2 mt-6 max-w-xl text-lg leading-8 text-[var(--secondary)] sm:text-xl">
              A Renova organiza cadastro, entrada de pecas, comissoes,
              pagamentos, clientes e rotina da loja em uma plataforma clara,
              elegante e feita para o dia a dia do brecho.
            </p>

            <div className="reveal-up-delay-2 mt-8 flex flex-col gap-4 sm:flex-row">
              <a
                href="#demo"
                className="inline-flex items-center justify-center rounded-full bg-[linear-gradient(135deg,_#6a5cff_0%,_#5a52eb_100%)] px-6 py-4 text-sm font-semibold !text-white shadow-[0_22px_40px_rgba(90,82,235,0.34)] transition hover:translate-y-[-2px] hover:!text-white"
              >
                Explorar experiencia completa
              </a>
              <a
                href="#mobile"
                className="inline-flex items-center justify-center rounded-full border border-[var(--border-strong)] bg-white/82 px-6 py-4 text-sm font-semibold text-[var(--foreground)] shadow-[0_12px_28px_rgba(15,23,42,0.06)] transition hover:border-[var(--primary)] hover:text-[var(--primary)]"
              >
                Ver experiencia mobile
              </a>
            </div>

            <div className="reveal-up-delay-2 mt-10 grid gap-4 sm:grid-cols-3">
              {metrics.map((item) => (
                <div
                  key={item.label}
                  className="rounded-[28px] border border-white/65 bg-white/75 p-5 shadow-[0_20px_45px_rgba(33,38,89,0.08)] backdrop-blur"
                >
                  <p className="text-3xl font-semibold tracking-[-0.04em] text-[var(--foreground)]">
                    {item.value}
                  </p>
                  <p className="mt-2 text-sm leading-6 text-[var(--secondary)]">
                    {item.label}
                  </p>
                </div>
              ))}
            </div>
          </div>

          <div id="demo" className="relative flex items-center justify-center">
            <div className="absolute inset-x-[10%] top-[10%] h-[68%] rounded-full bg-[radial-gradient(circle,_rgba(106,92,255,0.22)_0%,_rgba(106,92,255,0)_72%)] blur-3xl" />
            <div className="float-card relative w-full max-w-2xl rounded-[36px] border border-white/65 bg-[linear-gradient(180deg,_rgba(255,255,255,0.92)_0%,_rgba(247,249,255,0.96)_100%)] p-4 shadow-[var(--shadow-hero)] backdrop-blur">
              <div className="rounded-[30px] border border-[var(--border)] bg-[var(--surface)] p-4">
                <div className="flex items-center justify-between rounded-[24px] bg-[var(--surface-muted)] px-4 py-3">
                  <div>
                    <p className="text-xs font-semibold tracking-[0.18em] text-[var(--primary)] uppercase">
                      Dashboard Renova
                    </p>
                    <p className="mt-1 text-lg font-semibold text-[var(--foreground)]">
                      Visao unificada da operacao
                    </p>
                  </div>
                  <div className="rounded-2xl bg-white px-3 py-2 text-right shadow-[0_10px_28px_rgba(15,23,42,0.08)]">
                    <p className="text-xs text-[var(--secondary)]">Hoje</p>
                    <p className="text-sm font-semibold text-[var(--foreground)]">
                      18 metas acompanhadas
                    </p>
                  </div>
                </div>

                <div className="mt-4 grid gap-4 xl:grid-cols-[220px_1fr]">
                  <aside className="rounded-[26px] bg-[linear-gradient(180deg,_#6a5cff_0%,_#5a52eb_100%)] p-5 text-white">
                    <p className="text-xs font-semibold tracking-[0.18em] uppercase text-white/70">
                      Modulos
                    </p>
                    <div className="mt-4 space-y-2">
                      {modules.map((module) => (
                        <div
                          key={module}
                          className="rounded-2xl border border-white/12 bg-white/8 px-3 py-2 text-sm font-medium backdrop-blur"
                        >
                          {module}
                        </div>
                      ))}
                    </div>
                  </aside>

                  <div className="space-y-4">
                    <div className="grid gap-4 md:grid-cols-3">
                      {[
                        ["Receita do dia", "R$ 32.480", "+14%"],
                        ["Pendencias", "06", "-2 hoje"],
                        ["Atendimentos", "184", "Fluxo estavel"],
                      ].map(([label, value, note]) => (
                        <div
                          key={label}
                          className="rounded-[24px] border border-[var(--border)] bg-white p-4 shadow-[0_16px_32px_rgba(15,23,42,0.05)]"
                        >
                          <p className="text-sm text-[var(--secondary)]">
                            {label}
                          </p>
                          <p className="mt-3 text-2xl font-semibold tracking-[-0.04em] text-[var(--foreground)]">
                            {value}
                          </p>
                          <p className="mt-2 text-xs font-semibold text-[var(--primary)]">
                            {note}
                          </p>
                        </div>
                      ))}
                    </div>

                    <div className="grid gap-4 lg:grid-cols-[1.35fr_0.9fr]">
                      <div className="rounded-[28px] border border-[var(--border)] bg-[var(--surface-muted)] p-5">
                        <div className="flex items-end justify-between">
                          <div>
                            <p className="text-sm font-medium text-[var(--secondary)]">
                              Performance por loja
                            </p>
                            <p className="mt-1 text-xl font-semibold text-[var(--foreground)]">
                              Evolucao dos resultados nos ultimos meses
                            </p>
                          </div>
                          <div className="rounded-full bg-white px-3 py-2 text-xs font-semibold text-[var(--primary)]">
                            Atualizado agora
                          </div>
                        </div>

                        <div className="mt-6 flex h-52 items-end gap-3">
                          {[58, 86, 68, 112, 96, 142].map((height, index) => (
                            <div
                              key={height}
                              className="flex flex-1 flex-col items-center gap-3"
                            >
                              <div
                                className="w-full rounded-t-[22px] bg-[linear-gradient(180deg,_#8d84ff_0%,_#5a52eb_100%)] shadow-[0_16px_24px_rgba(90,82,235,0.22)]"
                                style={{ height }}
                              />
                              <span className="text-xs font-medium text-[var(--secondary)]">
                                {
                                  ["Jan", "Fev", "Mar", "Abr", "Mai", "Jun"][
                                    index
                                  ]
                                }
                              </span>
                            </div>
                          ))}
                        </div>
                      </div>

                      <div className="rounded-[28px] border border-[var(--border)] bg-white p-5 shadow-[0_16px_32px_rgba(15,23,42,0.05)]">
                        <p className="text-sm font-medium text-[var(--secondary)]">
                          Prioridades do turno
                        </p>
                        <div className="mt-5 space-y-3">
                          {[
                            [
                              "Conferir pagamentos externos",
                              "Agora",
                              "bg-[#e9f8f2] text-[#1b9a68]",
                            ],
                            [
                              "Liberar solicitacoes aprovadas",
                              "11:30",
                              "bg-[var(--primary-soft)] text-[var(--primary)]",
                            ],
                            [
                              "Fechamento da loja central",
                              "18:00",
                              "bg-[#fff4e5] text-[#d68b1f]",
                            ],
                          ].map(([task, time, tone]) => (
                            <div
                              key={task}
                              className="rounded-[22px] border border-[var(--border)] bg-[var(--surface-muted)] p-4"
                            >
                              <div className="flex items-start justify-between gap-4">
                                <p className="text-sm font-semibold text-[var(--foreground)]">
                                  {task}
                                </p>
                                <span
                                  className={`rounded-full px-3 py-1 text-[11px] font-semibold ${tone}`}
                                >
                                  {time}
                                </span>
                              </div>
                            </div>
                          ))}
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>

              <div className="float-card-delay absolute -bottom-10 -left-6 hidden w-56 rounded-[28px] border border-white/60 bg-white/88 p-4 shadow-[0_24px_55px_rgba(15,23,42,0.12)] backdrop-blur md:block">
                <p className="text-xs font-semibold tracking-[0.18em] text-[var(--primary)] uppercase">
                  Acesso seguro
                </p>
                <p className="mt-2 text-lg font-semibold text-[var(--foreground)]">
                  Perfis e permissoes organizados
                </p>
                <p className="mt-3 text-sm leading-6 text-[var(--secondary)]">
                  Controle granular para operacao, administracao e consulta.
                </p>
              </div>

              <div className="float-card absolute -right-5 top-10 hidden w-52 rounded-[28px] bg-[linear-gradient(180deg,_#1f2a44_0%,_#101726_100%)] p-4 text-white shadow-[0_24px_55px_rgba(10,17,30,0.25)] lg:block">
                <p className="text-xs font-semibold tracking-[0.18em] uppercase text-white/60">
                  Tempo real
                </p>
                <p className="mt-3 text-3xl font-semibold tracking-[-0.04em]">
                  03s
                </p>
                <p className="mt-2 text-sm leading-6 text-white/72">
                  para enxergar os principais movimentos da operacao.
                </p>
              </div>
            </div>
          </div>
        </div>
      </section>

      <section
        id="sistema"
        className="relative border-y border-white/60 bg-[linear-gradient(180deg,_rgba(255,255,255,0.58)_0%,_rgba(248,250,255,0.95)_100%)] py-8"
      >
        <div className="mx-auto flex max-w-7xl overflow-hidden px-6 lg:px-10">
          <div className="marquee-track flex min-w-max gap-4 pr-4">
            {[...modules, ...modules].map((item, index) => (
              <div
                key={`${item}-${index}`}
                className="rounded-full border border-[rgba(106,92,255,0.12)] bg-white px-5 py-3 text-sm font-semibold text-[var(--foreground)] shadow-[0_12px_25px_rgba(15,23,42,0.05)]"
              >
                {item}
              </div>
            ))}
          </div>
        </div>
      </section>

      <section className="relative mx-auto max-w-7xl px-6 py-24 lg:px-10">
        <div className="grid gap-6 lg:grid-cols-[0.88fr_1.12fr]">
          <div className="rounded-[36px] border border-[var(--border)] bg-white p-8 shadow-[var(--shadow-soft)] transition duration-300 hover:scale-[1.03] hover:shadow-[0_30px_70px_rgba(15,23,42,0.12)]">
            <p className="text-sm font-semibold tracking-[0.2em] text-[var(--primary)] uppercase">
              Por que a Renova funciona
            </p>
            <h2 className="mt-4 text-4xl font-semibold tracking-[-0.04em] text-[var(--foreground)]">
              Uma experiencia premium sem perder clareza operacional.
            </h2>
            <p className="mt-5 text-base leading-8 text-[var(--secondary)]">
              A interface combina dados criticos, acessos rapidos e uma
              linguagem visual consistente para reduzir friccao no trabalho
              diario.
            </p>
          </div>

          <div className="grid gap-6 md:grid-cols-3">
            {features.map((feature) => (
              <article
                key={feature.title}
                className="rounded-[32px] border border-[var(--border)] bg-[linear-gradient(180deg,_rgba(255,255,255,0.95)_0%,_rgba(248,250,255,0.95)_100%)] p-7 shadow-[0_20px_40px_rgba(15,23,42,0.06)] transition duration-300 hover:scale-[1.03] hover:shadow-[0_30px_70px_rgba(15,23,42,0.12)]"
              >
                <div className="flex h-12 w-12 items-center justify-center rounded-2xl bg-[var(--primary-soft)] text-lg font-semibold text-[var(--primary)]">
                  {feature.title[0]}
                </div>
                <h3 className="mt-5 text-xl font-semibold text-[var(--foreground)]">
                  {feature.title}
                </h3>
                <p className="mt-3 text-sm leading-7 text-[var(--secondary)]">
                  {feature.description}
                </p>
              </article>
            ))}
          </div>
        </div>
      </section>

      <section
        id="mobile"
        className="relative mx-auto max-w-7xl px-6 py-8 pb-24 lg:px-10"
      >
        <div className="grid gap-8 lg:grid-cols-[0.9fr_1.1fr]">
          <div className="rounded-[38px] border border-[var(--border)] bg-[linear-gradient(180deg,_rgba(255,255,255,0.96)_0%,_rgba(241,244,255,0.98)_100%)] p-8 text-[var(--foreground)] shadow-[0_36px_80px_rgba(15,23,42,0.10)]">
            <p className="text-sm font-semibold tracking-[0.2em] uppercase text-[var(--primary)]">
              Mobile Renova
            </p>
            <h2 className="mt-4 text-4xl font-semibold tracking-[-0.04em]">
              O mesmo sistema da web, agora organizado para o celular.
            </h2>
            <p className="mt-5 max-w-xl text-base leading-8 text-[var(--secondary)]">
              A proposta do mobile e levar a experiencia da Renova para uma tela
              menor, preservando os mesmos modulos, a mesma logica de uso e o
              mesmo cuidado visual da versao web.
            </p>

            <div id="fluxo" className="mt-10 grid gap-5">
              <div className="rounded-[28px] border border-[var(--border)] bg-white p-5 shadow-[0_16px_36px_rgba(15,23,42,0.05)]">
                <p className="text-sm font-semibold tracking-[0.16em] text-[var(--primary)] uppercase">
                  Jornada do funcionario
                </p>
                <div className="mt-4 space-y-3">
                  {employeeSteps.map((step, index) => (
                    <div key={step} className="flex items-start gap-3">
                      <span className="mt-0.5 flex h-7 w-7 items-center justify-center rounded-full bg-[var(--primary-soft)] text-sm font-semibold text-[var(--primary)]">
                        {index + 1}
                      </span>
                      <p className="text-sm leading-7 text-[var(--secondary)]">{step}</p>
                    </div>
                  ))}
                </div>
              </div>

              <div className="rounded-[28px] border border-[var(--border)] bg-white p-5 shadow-[0_16px_36px_rgba(15,23,42,0.05)]">
                <p className="text-sm font-semibold tracking-[0.16em] text-[var(--primary)] uppercase">
                  Experiencia consistente
                </p>
                <div className="mt-4 space-y-3">
                  {clientSteps.map((step, index) => (
                    <div key={step} className="flex items-start gap-3">
                      <span className="mt-0.5 flex h-7 w-7 items-center justify-center rounded-full bg-[var(--primary-soft)] text-sm font-semibold text-[var(--primary)]">
                        {index + 1}
                      </span>
                      <p className="text-sm leading-7 text-[var(--secondary)]">{step}</p>
                    </div>
                  ))}
                </div>
              </div>
            </div>
          </div>

          <div className="grid items-center gap-8 md:grid-cols-2">
            <article className="float-card relative mx-auto w-full max-w-sm rounded-[42px] border border-white/70 bg-[linear-gradient(180deg,_rgba(255,255,255,0.96)_0%,_rgba(242,245,255,0.96)_100%)] p-3 shadow-[0_30px_70px_rgba(15,23,42,0.14)]">
              <div className="rounded-[34px] border border-[var(--border)] bg-white p-4">
                <div className="mx-auto h-1.5 w-20 rounded-full bg-[var(--border)]" />
                <div className="mt-5 rounded-[28px] bg-[linear-gradient(160deg,_#6a5cff_0%,_#8d84ff_100%)] p-5 text-white">
                  <p className="text-xs font-semibold tracking-[0.18em] uppercase text-white/72">
                    App funcionario
                  </p>
                  <p className="mt-3 text-2xl font-semibold leading-tight">
                    A operacao da loja no mesmo ritmo do sistema web.
                  </p>
                  <div className="mt-5 rounded-[22px] bg-white/12 p-4 backdrop-blur">
                    <p className="text-xs text-white/66">Visao rapida</p>
                    <p className="mt-1 text-sm font-semibold">
                      Clientes, produtos e movimentacoes em uma unica navegação
                    </p>
                  </div>
                </div>

                <div className="mt-5 space-y-3">
                  {[
                    ["Clientes", "Consulta e acompanhamento da base cadastrada"],
                    ["Produtos", "Visualizacao de pecas, valores e status"],
                    ["Movimentacoes", "Rotinas da loja acessiveis no celular"],
                  ].map(([title, detail]) => (
                    <div
                      key={title}
                      className="rounded-[22px] border border-[var(--border)] bg-[var(--surface-muted)] px-4 py-3"
                    >
                      <p className="text-sm font-semibold text-[var(--foreground)]">
                        {title}
                      </p>
                      <p className="mt-1 text-xs leading-6 text-[var(--secondary)]">
                        {detail}
                      </p>
                    </div>
                  ))}
                </div>
              </div>
            </article>

            <article className="float-card-delay relative mx-auto w-full max-w-sm rounded-[42px] border border-white/70 bg-[linear-gradient(180deg,_rgba(255,255,255,0.96)_0%,_rgba(242,245,255,0.96)_100%)] p-3 shadow-[0_30px_70px_rgba(15,23,42,0.14)] md:translate-y-12">
              <div className="rounded-[34px] border border-[var(--border)] bg-white p-4">
                <div className="mx-auto h-1.5 w-20 rounded-full bg-[var(--border)]" />
                <div className="mt-5 rounded-[28px] border border-[var(--border)] bg-[linear-gradient(180deg,_rgba(241,239,255,0.9)_0%,_rgba(255,255,255,1)_100%)] p-5">
                  <p className="text-xs font-semibold tracking-[0.18em] uppercase text-[var(--primary)]">
                    App mobile
                  </p>
                  <p className="mt-3 text-2xl font-semibold leading-tight text-[var(--foreground)]">
                    A mesma estrutura da Renova em uma interface leve e objetiva.
                  </p>
                  <div className="mt-5 flex items-center justify-between rounded-[22px] border border-[var(--border)] bg-white px-4 py-3 shadow-[0_12px_28px_rgba(15,23,42,0.05)]">
                    <div>
                      <p className="text-xs text-[var(--secondary)]">Base visual</p>
                      <p className="mt-1 text-sm font-semibold text-[var(--foreground)]">
                        Mesmo idioma de cards, modulos e consultas
                      </p>
                    </div>
                    <div className="h-3 w-3 rounded-full bg-[#2fc98f]" />
                  </div>
                </div>

                <div className="mt-5 rounded-[24px] border border-[var(--border)] bg-[var(--surface-muted)] p-4">
                  <p className="text-sm font-semibold text-[var(--foreground)]">
                    Elementos da versao mobile
                  </p>
                  <div className="mt-4 space-y-4">
                    {[
                      ["Consulta de modulos", "Produtos, clientes e pagamentos"],
                      ["Leitura clara", "Cards e informacoes bem organizados"],
                      ["Uso continuo", "Mesma experiencia entre desktop e celular"],
                    ].map(([label, hour]) => (
                      <div
                        key={label}
                        className="flex items-center justify-between gap-3"
                      >
                        <div className="flex items-center gap-3">
                          <span className="h-3 w-3 rounded-full bg-[var(--primary)]" />
                          <span className="text-sm text-[var(--foreground)]">
                            {label}
                          </span>
                        </div>
                        <span className="text-xs font-medium text-[var(--secondary)]">
                          {hour}
                        </span>
                      </div>
                    ))}
                  </div>
                </div>
              </div>
            </article>
          </div>
        </div>
      </section>

      <section
        id="contato"
        className="relative mx-auto max-w-7xl px-6 pb-24 lg:px-10"
      >
        <div className="relative overflow-hidden rounded-[40px] border border-[rgba(106,92,255,0.14)] bg-[linear-gradient(135deg,_#6a5cff_0%,_#5a52eb_48%,_#1f2a44_100%)] p-8 text-white shadow-[0_34px_90px_rgba(59,49,160,0.28)] lg:p-12">
          <div className="absolute -right-16 -top-16 h-56 w-56 rounded-full bg-white/10 blur-3xl" />
          <div className="absolute -bottom-20 left-10 h-56 w-56 rounded-full bg-[#8d84ff]/20 blur-3xl" />

          <div className="relative grid gap-8 lg:grid-cols-[1fr_0.85fr] lg:items-end">
            <div className="max-w-2xl">
              <p className="text-sm font-semibold tracking-[0.22em] text-white/70 uppercase">
                Entre em contato
              </p>
              <h2 className="mt-4 text-4xl font-semibold tracking-[-0.04em] sm:text-5xl">
                Solicite um orçamento para levar a Renova ao seu brechó.
              </h2>
              <p className="mt-5 max-w-xl text-base leading-8 text-white/78">
                Fale com a nossa equipe para entender como a Renova pode apoiar a
                gestão do seu brechó com venda consignada, organização da rotina
                e mais clareza no dia a dia da operação.
              </p>

              <div className="mt-8 flex flex-col gap-4 sm:flex-row">
                <a
                  href="mailto:contato@renova.com.br?subject=Pedido%20de%20orcamento%20-%20Renova"
                  className="inline-flex items-center justify-center rounded-full bg-white px-6 py-4 text-sm font-semibold !text-[var(--primary-strong)] shadow-[0_18px_36px_rgba(255,255,255,0.18)] transition hover:translate-y-[-2px] hover:!text-[var(--primary-strong)]"
                  >
                  Solicitar orçamento
                </a>
                <a
                  href="#demo"
                  className="inline-flex items-center justify-center rounded-full border border-white/24 bg-white/8 px-6 py-4 text-sm font-semibold text-white backdrop-blur transition hover:bg-white/12"
                >
                  Rever demonstração
                </a>
              </div>
            </div>

            <div className="rounded-[32px] border border-white/14 bg-white/10 p-6 backdrop-blur">
              <p className="text-sm font-semibold tracking-[0.18em] text-white/68 uppercase">
                O que informar
              </p>
              <div className="mt-5 space-y-3">
                {[
                  "Quantidade de lojas ou unidades que deseja organizar.",
                  "Principais rotinas do brechó que hoje precisam de mais controle.",
                  "Se a operação trabalha com venda consignada, repasse e gestão de clientes.",
                ].map((item) => (
                  <div
                    key={item}
                    className="rounded-[22px] border border-white/10 bg-white/8 px-4 py-4"
                  >
                    <p className="text-sm leading-7 text-white/82">{item}</p>
                  </div>
                ))}
              </div>
            </div>
          </div>
        </div>
      </section>
    </main>
  );
}
