const siteUrl = "https://renovacuritiba.com.br";

export default function Home() {
  const proofPoints = [
    {
      title: "Feita na rotina real",
      description:
        "Construida acompanhando um brecho que trabalha todos os dias com consignacao.",
    },
    {
      title: "Especializada no nicho",
      description:
        "Fluxos pensados para pecas, fornecedores, repasses, emprestimos e devolucoes.",
    },
    {
      title: "Clareza operacional",
      description:
        "Telas organizadas para encontrar informacoes, registrar movimentos e fechar o mes.",
    },
  ];

  const painPoints = [
    {
      title: "Evite perder pecas no estoque",
      description:
        "Controle por etiqueta, fornecedor, situacao da peca e historico de movimentacoes.",
    },
    {
      title: "Saiba o que deve ser repassado",
      description:
        "Pagamentos, creditos, pendencias e fechamento ficam conectados ao cliente-fornecedor.",
    },
    {
      title: "Controle devolucoes e emprestimos",
      description:
        "Movimentacoes mudam a situacao da peca e deixam a loja com rastreabilidade.",
    },
  ];

  const modules = [
    "Consignacao",
    "Produtos",
    "Clientes",
    "Movimentacoes",
    "Pagamentos",
    "Fechamento",
    "Etiquetas",
    "Area do cliente",
    "Permissoes",
  ];

  const consignmentItems = [
    "controle de pecas por fornecedor",
    "repasses, creditos e pendencias",
    "emprestimos, devolucoes e doacoes",
    "situacao da peca atualizada por movimentacao",
    "fechamento mensal da loja",
    "consulta rapida por etiqueta",
  ];

  const controlAreas = [
    {
      title: "Produtos e estoque",
      items: [
        "cadastro de pecas",
        "busca por etiqueta",
        "filtros e paginacao",
        "situacao e historico",
      ],
    },
    {
      title: "Clientes e fornecedores",
      items: ["historico", "pendencias", "creditos", "exportacoes"],
    },
    {
      title: "Movimentacoes",
      items: ["venda", "emprestimo", "devolucao", "doacao"],
    },
    {
      title: "Financeiro",
      items: ["pagamentos", "fechamento", "gastos da loja", "resumo mensal"],
    },
    {
      title: "Operacao da loja",
      items: ["funcionarios", "permissoes", "impressoras", "configuracoes"],
    },
  ];

  const printFeatures = [
    "etiquetas",
    "recibos",
    "impressoras termicas",
    "pre-visualizacao",
    "impressao direta",
  ];

  const clientAreaItems = [
    "pecas cadastradas",
    "vendas e movimentacoes",
    "pendencias",
    "creditos",
    "historico de compras",
  ];

  const mobileSteps = [
    "Consulte clientes, produtos, pagamentos e movimentacoes tambem pelo celular.",
    "Veja informacoes importantes da loja em telas organizadas para leitura rapida.",
    "Acompanhe a rotina do brecho mesmo longe do computador.",
  ];

  const targetProfiles = [
    "brechos consignados",
    "lojas com alto volume de pecas",
    "operacoes com repasse para fornecedores",
    "lojas que trabalham com emprestimos ou reservas",
    "equipes com mais de um atendente",
  ];

  const faqItems = [
    {
      question: "Para quais lojas a Renova serve?",
      answer:
        "A Renova foi pensada para brechos consignados, lojas com muitas pecas em estoque, operacoes com repasse para fornecedor e equipes que precisam de controle por permissao.",
    },
    {
      question: "Como a Renova controla pecas consignadas?",
      answer:
        "Cada peca pode ser cadastrada com dados do fornecedor, etiqueta, valor, situacao e historico. Assim a loja acompanha entrada, venda, emprestimo, devolucao e fechamento sem depender de planilhas.",
    },
    {
      question: "Como funciona o repasse para fornecedor?",
      answer:
        "O sistema conecta pecas, movimentacoes, pagamentos, creditos e pendencias ao cliente-fornecedor, ajudando a identificar o que precisa ser repassado no fechamento.",
    },
    {
      question: "O sistema imprime etiquetas?",
      answer:
        "Sim. A Renova ajuda na impressao de etiquetas e recibos para manter a peca identificada e facilitar a conferencia durante a rotina da loja.",
    },
    {
      question: "Clientes conseguem acompanhar pecas e creditos?",
      answer:
        "Sim. A area do cliente permite consultar pecas cadastradas, vendas, movimentacoes, pendencias, creditos e historico, reduzindo atendimentos manuais repetidos.",
    },
    {
      question: "Precisa substituir planilhas de uma vez?",
      answer:
        "Nao. A Renova pode entrar como uma plataforma para centralizar a operacao aos poucos, principalmente onde a loja mais perde tempo: pecas, repasses, etiquetas e fechamento financeiro.",
    },
    {
      question: "Funciona em celular?",
      answer:
        "Sim. A experiencia em telas menores facilita consultas rapidas de clientes, produtos, pagamentos e movimentacoes durante a rotina do brecho.",
    },
  ];

  const structuredData = [
    {
      "@context": "https://schema.org",
      "@type": "Organization",
      name: "Renova",
      url: siteUrl,
      email: "contato@renova.com.br",
      areaServed: {
        "@type": "Country",
        name: "Brasil",
      },
    },
    {
      "@context": "https://schema.org",
      "@type": "WebSite",
      name: "Renova",
      url: siteUrl,
      inLanguage: "pt-BR",
      description:
        "Sistema para brecho consignado com controle de pecas, clientes, movimentacoes, etiquetas, repasses e fechamento financeiro.",
    },
    {
      "@context": "https://schema.org",
      "@type": "SoftwareApplication",
      name: "Renova",
      applicationCategory: "BusinessApplication",
      operatingSystem: "Web",
      url: siteUrl,
      inLanguage: "pt-BR",
      areaServed: {
        "@type": "Country",
        name: "Brasil",
      },
      audience: {
        "@type": "BusinessAudience",
        audienceType: "Brechos consignados",
      },
      description:
        "Sistema para gestao de brecho consignado com controle de pecas consignadas, repasse para fornecedor, impressao de etiquetas e fechamento financeiro.",
    },
  ];

  return (
    <main className="relative overflow-hidden bg-[var(--background)]">
      <script
        type="application/ld+json"
        dangerouslySetInnerHTML={{
          __html: JSON.stringify(structuredData).replace(/</g, "\\u003c"),
        }}
      />
      <div className="absolute inset-0 landing-grid opacity-60" />
      <div className="absolute inset-x-0 top-0 h-[720px] bg-[radial-gradient(circle_at_top_left,_rgba(183,196,255,0.95)_0%,_rgba(143,153,243,0.75)_26%,_rgba(106,92,255,0.28)_54%,_rgba(244,246,251,0)_78%)]" />
      <div className="absolute -top-16 right-[8%] h-72 w-72 rounded-full bg-[rgba(106,92,255,0.20)] blur-3xl hero-orb" />
      <div className="absolute top-[36rem] -left-20 h-72 w-72 rounded-full bg-[rgba(47,201,143,0.12)] blur-3xl hero-orb-delay" />

      <section className="relative mx-auto flex min-h-screen w-full max-w-7xl flex-col px-6 pb-20 pt-8 lg:px-10 lg:pb-28 lg:pt-10">
        <header className="reveal-up flex items-center justify-between gap-4 rounded-full border border-white/55 bg-white/70 px-4 py-3 shadow-[0_18px_40px_rgba(62,63,140,0.10)] backdrop-blur md:px-6">
          <div className="flex items-center gap-3">
            <div className="flex h-11 w-11 shrink-0 items-center justify-center rounded-2xl bg-[linear-gradient(135deg,_#6a5cff_0%,_#8d84ff_100%)] text-base font-semibold text-white shadow-[0_14px_30px_rgba(106,92,255,0.35)]">
              R
            </div>
            <div>
              <p className="text-sm font-semibold tracking-[0.22em] text-[var(--primary)] uppercase">
                Renova
              </p>
              <p className="text-xs text-[var(--secondary)]">
                Sistema para brecho consignado
              </p>
            </div>
          </div>

          <nav className="hidden items-center gap-8 text-sm font-medium text-[var(--secondary)] xl:flex">
            {[
              ["Sistema", "#sistema"],
              ["Consignacao", "#consignacao"],
              ["Etiquetas", "#etiquetas"],
              ["Area do cliente", "#area-cliente"],
              ["Contato", "#contato"],
            ].map(([label, href]) => (
              <a
                key={label}
                href={href}
                className="rounded-full px-3 py-2 transition duration-200 hover:-translate-y-0.5 hover:bg-white/80 hover:text-[var(--foreground)] hover:shadow-[0_12px_24px_rgba(15,23,42,0.08)]"
              >
                {label}
              </a>
            ))}
          </nav>

          <div className="flex items-center gap-3">
            <a
              href="https://app.renovacuritiba.com.br"
              target="_blank"
              rel="noreferrer"
              className="hidden items-center justify-center rounded-full border border-[var(--border-strong)] bg-white/82 px-5 py-3 text-sm font-semibold text-[var(--foreground)] shadow-[0_12px_28px_rgba(15,23,42,0.06)] transition hover:translate-y-[-1px] hover:border-[var(--primary)] hover:text-[var(--primary)] sm:inline-flex"
            >
              Acessar portal
            </a>
            <a
              href="#contato"
              className="inline-flex items-center justify-center rounded-full bg-[var(--foreground)] px-5 py-3 text-sm font-semibold !text-white transition hover:translate-y-[-1px] hover:bg-[#18233b] hover:!text-white"
            >
              Conversar
            </a>
          </div>
        </header>

        <div className="grid flex-1 items-center gap-16 py-14 xl:grid-cols-[1.02fr_0.98fr] xl:py-20">
          <div className="mx-auto max-w-2xl text-center xl:mx-0 xl:text-left">
            <p className="reveal-up-delay mt-8 text-sm font-semibold tracking-[0.34em] text-[var(--primary)] uppercase">
              Renova
            </p>
            <h1 className="reveal-up-delay mt-4 text-4xl font-semibold leading-[1.05] text-[var(--foreground)] sm:text-5xl lg:text-6xl">
              Sistema para brecho consignado com controle real da{" "}
              <span className="bg-[linear-gradient(135deg,_#6a5cff_0%,_#5a52eb_100%)] bg-clip-text text-transparent">
                operacao.
              </span>
            </h1>

            <p className="reveal-up-delay-2 mx-auto mt-6 max-w-xl text-lg leading-8 text-[var(--secondary)] sm:text-xl xl:mx-0">
              Gestao de brecho consignado com cadastro de pecas, controle de
              movimentacoes, repasse para fornecedor, impressao de etiquetas e
              fechamento financeiro em um so lugar.
            </p>

            <div className="reveal-up-delay-2 mt-8 flex flex-col justify-center gap-4 sm:flex-row xl:justify-start">
              <a
                href="#contato"
                className="inline-flex items-center justify-center rounded-full bg-[linear-gradient(135deg,_#6a5cff_0%,_#5a52eb_100%)] px-6 py-4 text-sm font-semibold !text-white shadow-[0_22px_40px_rgba(90,82,235,0.34)] transition hover:translate-y-[-2px] hover:!text-white"
              >
                Conversar sobre sua operacao
              </a>
              <a
                href="#sistema"
                className="inline-flex items-center justify-center rounded-full border border-[var(--border-strong)] bg-white/82 px-6 py-4 text-sm font-semibold text-[var(--foreground)] shadow-[0_12px_28px_rgba(15,23,42,0.06)] transition hover:border-[var(--primary)] hover:text-[var(--primary)]"
              >
                Ver o que a Renova controla
              </a>
            </div>

            <div className="reveal-up-delay-2 mt-10 grid gap-4 sm:grid-cols-3">
              {proofPoints.map((item) => (
                <div
                  key={item.title}
                  className="rounded-[28px] border border-white/65 bg-white/75 p-5 text-left shadow-[0_20px_45px_rgba(33,38,89,0.08)] backdrop-blur"
                >
                  <p className="text-base font-semibold text-[var(--foreground)]">
                    {item.title}
                  </p>
                  <p className="mt-2 text-sm leading-6 text-[var(--secondary)]">
                    {item.description}
                  </p>
                </div>
              ))}
            </div>
          </div>

          <div id="demo" className="relative flex items-center justify-center">
            <div className="absolute inset-x-[10%] top-[10%] h-[68%] rounded-full bg-[radial-gradient(circle,_rgba(106,92,255,0.22)_0%,_rgba(106,92,255,0)_72%)] blur-3xl" />
            <div className="float-card relative w-full max-w-2xl rounded-[36px] border border-white/65 bg-[linear-gradient(180deg,_rgba(255,255,255,0.92)_0%,_rgba(247,249,255,0.96)_100%)] p-4 shadow-[var(--shadow-hero)] backdrop-blur">
              <div className="rounded-[30px] border border-[var(--border)] bg-[var(--surface)] p-4">
                <div className="flex flex-col gap-4 rounded-[24px] bg-[var(--surface-muted)] px-4 py-3 sm:flex-row sm:items-center sm:justify-between">
                  <div>
                    <p className="text-xs font-semibold tracking-[0.18em] text-[var(--primary)] uppercase">
                      Painel Renova
                    </p>
                    <p className="mt-1 text-lg font-semibold text-[var(--foreground)]">
                      Rotina do brecho em uma tela
                    </p>
                  </div>
                  <div className="rounded-2xl bg-white px-3 py-2 text-left shadow-[0_10px_28px_rgba(15,23,42,0.08)] sm:text-right">
                    <p className="text-xs text-[var(--secondary)]">Base</p>
                    <p className="text-sm font-semibold text-[var(--foreground)]">
                      Consignacao real
                    </p>
                  </div>
                </div>

                <div className="mt-4 grid gap-4 xl:grid-cols-[220px_1fr]">
                  <aside className="rounded-[26px] bg-[linear-gradient(180deg,_#6a5cff_0%,_#5a52eb_100%)] p-5 text-white">
                    <p className="text-xs font-semibold tracking-[0.18em] uppercase text-white/70">
                      Modulos
                    </p>
                    <div className="mt-4 space-y-2">
                      {modules.slice(0, 8).map((module) => (
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
                        ["Etiqueta", "08421", "Busca rapida"],
                        ["Fornecedor", "Cliente ativo", "Pecas vinculadas"],
                        ["Repasse", "Pendente", "No fechamento"],
                      ].map(([label, value, note]) => (
                        <div
                          key={label}
                          className="rounded-[24px] border border-[var(--border)] bg-white p-4 shadow-[0_16px_32px_rgba(15,23,42,0.05)]"
                        >
                          <p className="text-sm text-[var(--secondary)]">
                            {label}
                          </p>
                          <p className="mt-3 text-xl font-semibold text-[var(--foreground)]">
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
                        <div className="flex flex-col gap-3 sm:flex-row sm:items-end sm:justify-between">
                          <div>
                            <p className="text-sm font-medium text-[var(--secondary)]">
                              Produtos em acompanhamento
                            </p>
                            <p className="mt-1 text-xl font-semibold text-[var(--foreground)]">
                              Situacao clara por peca
                            </p>
                          </div>
                          <div className="w-fit rounded-full bg-white px-3 py-2 text-xs font-semibold text-[var(--primary)]">
                            Estoque vivo
                          </div>
                        </div>

                        <div className="mt-6 space-y-3">
                          {[
                            ["Casaco linho", "Consignado", "A repassar"],
                            ["Vestido floral", "Emprestado", "Pendente dev."],
                            ["Bolsa couro", "Vendido", "Fechamento"],
                          ].map(([product, status, note]) => (
                            <div
                              key={product}
                              className="grid gap-3 rounded-[20px] border border-[var(--border)] bg-white px-4 py-3 text-sm shadow-[0_10px_24px_rgba(15,23,42,0.04)] sm:grid-cols-[1fr_auto_auto] sm:items-center"
                            >
                              <span className="font-semibold text-[var(--foreground)]">
                                {product}
                              </span>
                              <span className="rounded-full bg-[var(--primary-soft)] px-3 py-1 text-xs font-semibold text-[var(--primary)]">
                                {status}
                              </span>
                              <span className="text-xs font-medium text-[var(--secondary)]">
                                {note}
                              </span>
                            </div>
                          ))}
                        </div>
                      </div>

                      <div className="rounded-[28px] border border-[var(--border)] bg-white p-5 shadow-[0_16px_32px_rgba(15,23,42,0.05)]">
                        <p className="text-sm font-medium text-[var(--secondary)]">
                          Fluxo da operacao
                        </p>
                        <div className="mt-5 space-y-3">
                          {[
                            "Entrada da peca",
                            "Etiqueta e conferencia",
                            "Venda ou devolucao",
                            "Repasse ao fornecedor",
                          ].map((task) => (
                            <div
                              key={task}
                              className="rounded-[22px] border border-[var(--border)] bg-[var(--surface-muted)] p-4"
                            >
                              <p className="text-sm font-semibold text-[var(--foreground)]">
                                {task}
                              </p>
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
                  Etiqueta
                </p>
                <p className="mt-2 text-lg font-semibold text-[var(--foreground)]">
                  Consulta rapida por peca
                </p>
                <p className="mt-3 text-sm leading-6 text-[var(--secondary)]">
                  Encontre dono, status e historico sem depender de planilhas.
                </p>
              </div>

              <div className="float-card absolute -right-5 top-10 hidden w-52 rounded-[28px] bg-[linear-gradient(180deg,_#1f2a44_0%,_#101726_100%)] p-4 text-white shadow-[0_24px_55px_rgba(10,17,30,0.25)] 2xl:block">
                <p className="text-xs font-semibold tracking-[0.18em] uppercase text-white/60">
                  Repasse
                </p>
                <p className="mt-3 text-2xl font-semibold">
                  Creditos e pendencias
                </p>
                <p className="mt-2 text-sm leading-6 text-white/72">
                  Controle financeiro conectado ao cliente-fornecedor.
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
        <div className="grid gap-6 xl:grid-cols-[0.88fr_1.12fr]">
          <div className="rounded-[36px] border border-[var(--border)] bg-white p-8 shadow-[var(--shadow-soft)] transition duration-300 hover:scale-[1.03] hover:shadow-[0_30px_70px_rgba(15,23,42,0.12)]">
            <p className="text-sm font-semibold tracking-[0.2em] text-[var(--primary)] uppercase">
              Dores resolvidas
            </p>
            <h2 className="mt-4 text-3xl font-semibold text-[var(--foreground)] sm:text-4xl">
              Controle de pecas consignadas sem depender de memoria, papel ou
              planilhas.
            </h2>
            <p className="mt-5 text-base leading-8 text-[var(--secondary)]">
              A Renova coloca estoque, clientes-fornecedores, movimentacoes e
              financeiro na mesma rotina operacional.
            </p>
          </div>

          <div className="grid gap-6 md:grid-cols-3">
            {painPoints.map((feature) => (
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
        id="consignacao"
        className="relative mx-auto max-w-7xl px-6 pb-24 lg:px-10"
      >
        <div className="grid gap-8 rounded-[38px] border border-[var(--border)] bg-[linear-gradient(180deg,_rgba(255,255,255,0.96)_0%,_rgba(241,244,255,0.98)_100%)] p-8 shadow-[0_36px_80px_rgba(15,23,42,0.10)] lg:p-10 xl:grid-cols-[0.9fr_1.1fr]">
          <div>
            <p className="text-sm font-semibold tracking-[0.2em] uppercase text-[var(--primary)]">
              Feito para consignacao
            </p>
            <h2 className="mt-4 text-3xl font-semibold text-[var(--foreground)] sm:text-4xl">
              Criado junto de um brecho que vive essa operacao.
            </h2>
            <p className="mt-5 max-w-xl text-base leading-8 text-[var(--secondary)]">
              A Renova foi desenvolvida acompanhando a rotina real de um brecho
              consignado, para quem recebe pecas, vende, devolve, empresta e
              precisa repassar valores com clareza no fim do mes.
            </p>
          </div>

          <div className="grid gap-4 sm:grid-cols-2">
            {consignmentItems.map((item) => (
              <div
                key={item}
                className="rounded-[24px] border border-[var(--border)] bg-white p-5 shadow-[0_16px_36px_rgba(15,23,42,0.05)]"
              >
                <p className="text-sm font-semibold text-[var(--foreground)]">
                  {item}
                </p>
              </div>
            ))}
          </div>
        </div>
      </section>

      <section className="relative mx-auto max-w-7xl px-6 pb-24 lg:px-10">
        <div className="mx-auto max-w-3xl text-center">
          <p className="text-sm font-semibold tracking-[0.2em] uppercase text-[var(--primary)]">
            O que a Renova controla
          </p>
          <h2 className="mt-4 text-3xl font-semibold text-[var(--foreground)] sm:text-4xl">
            Modulos conectados para a rotina inteira da loja.
          </h2>
          <p className="mt-5 text-base leading-8 text-[var(--secondary)]">
            Da entrada da peca ao fechamento mensal, cada modulo ajuda a
            manter rastreabilidade e menos retrabalho.
          </p>
        </div>

        <div className="mt-10 grid gap-6 md:grid-cols-2 xl:grid-cols-5">
          {controlAreas.map((area) => (
            <article
              key={area.title}
              className="rounded-[30px] border border-[var(--border)] bg-white p-6 shadow-[0_20px_45px_rgba(15,23,42,0.06)]"
            >
              <h3 className="text-lg font-semibold text-[var(--foreground)]">
                {area.title}
              </h3>
              <div className="mt-5 space-y-3">
                {area.items.map((item) => (
                  <div key={item} className="flex items-center gap-3">
                    <span className="h-2.5 w-2.5 shrink-0 rounded-full bg-[var(--primary)]" />
                    <span className="text-sm leading-6 text-[var(--secondary)]">
                      {item}
                    </span>
                  </div>
                ))}
              </div>
            </article>
          ))}
        </div>
      </section>

      <section
        id="etiquetas"
        className="relative mx-auto max-w-7xl px-6 pb-24 lg:px-10"
      >
        <div className="grid gap-8 xl:grid-cols-[1fr_0.95fr]">
          <div className="rounded-[38px] border border-[var(--border)] bg-white p-8 shadow-[var(--shadow-soft)] lg:p-10">
            <p className="text-sm font-semibold tracking-[0.2em] uppercase text-[var(--primary)]">
              Impressao integrada
            </p>
            <h2 className="mt-4 text-3xl font-semibold text-[var(--foreground)] sm:text-4xl">
              Impressao de etiquetas para brecho, direto do fluxo da loja.
            </h2>
            <p className="mt-5 max-w-2xl text-base leading-8 text-[var(--secondary)]">
              A impressao e parte central da operacao de brecho consignado. A
              Renova ajuda a imprimir etiquetas e recibos, fazer conferencias e
              manter a peca identificada desde a entrada ate a venda.
            </p>

            <div className="mt-8 flex flex-wrap gap-3">
              {printFeatures.map((item) => (
                <span
                  key={item}
                  className="rounded-full border border-[rgba(106,92,255,0.12)] bg-[var(--primary-soft)] px-4 py-2 text-sm font-semibold text-[var(--primary)]"
                >
                  {item}
                </span>
              ))}
            </div>
          </div>

          <div className="rounded-[38px] border border-white/70 bg-[linear-gradient(160deg,_#6a5cff_0%,_#8d84ff_100%)] p-8 text-white shadow-[0_30px_70px_rgba(90,82,235,0.24)] lg:p-10">
            <p className="text-sm font-semibold tracking-[0.2em] uppercase text-white/70">
              Etiqueta da peca
            </p>
            <div className="mt-6 rounded-[28px] bg-white p-6 text-[var(--foreground)] shadow-[0_24px_60px_rgba(15,23,42,0.18)]">
              <div className="flex items-start justify-between gap-4">
                <div>
                  <p className="text-xs font-semibold tracking-[0.16em] text-[var(--primary)] uppercase">
                    Renova
                  </p>
                  <p className="mt-2 text-2xl font-semibold">08421</p>
                </div>
                <div className="rounded-2xl border border-[var(--border)] px-3 py-2 text-right">
                  <p className="text-xs text-[var(--secondary)]">Status</p>
                  <p className="text-sm font-semibold">Estoque</p>
                </div>
              </div>
              <div className="mt-6 space-y-3">
                {[
                  ["Peca", "Blazer feminino"],
                  ["Fornecedor", "Cliente-fornecedor"],
                  ["Destino", "Venda consignada"],
                ].map(([label, value]) => (
                  <div
                    key={label}
                    className="flex items-center justify-between gap-4 rounded-[18px] bg-[var(--surface-muted)] px-4 py-3"
                  >
                    <span className="text-sm text-[var(--secondary)]">
                      {label}
                    </span>
                    <span className="text-sm font-semibold">{value}</span>
                  </div>
                ))}
              </div>
            </div>
          </div>
        </div>
      </section>

      <section
        id="area-cliente"
        className="relative mx-auto max-w-7xl px-6 pb-24 lg:px-10"
      >
        <div className="grid items-center gap-8 xl:grid-cols-[0.9fr_1.1fr]">
          <div>
            <p className="text-sm font-semibold tracking-[0.2em] uppercase text-[var(--primary)]">
              Area do cliente
            </p>
            <h2 className="mt-4 text-3xl font-semibold text-[var(--foreground)] sm:text-4xl">
              Clientes acompanham informacoes sem depender de atendimento
              manual.
            </h2>
            <p className="mt-5 max-w-xl text-base leading-8 text-[var(--secondary)]">
              A area do cliente reforca maturidade do produto e reduz perguntas
              repetidas sobre pecas, vendas, pendencias e creditos.
            </p>
          </div>

          <div className="grid gap-5 sm:grid-cols-2">
            {clientAreaItems.map((item) => (
              <div
                key={item}
                className="rounded-[26px] border border-[var(--border)] bg-white p-5 shadow-[0_18px_40px_rgba(15,23,42,0.06)]"
              >
                <p className="text-base font-semibold text-[var(--foreground)]">
                  {item}
                </p>
              </div>
            ))}
          </div>
        </div>
      </section>

      <section
        id="mobile"
        className="relative mx-auto max-w-7xl px-6 py-8 pb-24 lg:px-10"
      >
        <div className="grid gap-8 xl:grid-cols-[0.9fr_1.1fr]">
          <div className="rounded-[38px] border border-[var(--border)] bg-[linear-gradient(180deg,_rgba(255,255,255,0.96)_0%,_rgba(241,244,255,0.98)_100%)] p-8 text-[var(--foreground)] shadow-[0_36px_80px_rgba(15,23,42,0.10)]">
            <p className="text-sm font-semibold tracking-[0.2em] uppercase text-[var(--primary)]">
              Mobile
            </p>
            <h2 className="mt-4 text-3xl font-semibold sm:text-4xl">
              Acompanhe informacoes da loja tambem pelo celular.
            </h2>
            <p className="mt-5 max-w-xl text-base leading-8 text-[var(--secondary)]">
              A experiencia em telas menores facilita consultas rapidas de
              produtos, clientes, pagamentos e movimentacoes durante a rotina da
              loja.
            </p>

            <div id="fluxo" className="mt-10 grid gap-5">
              <div className="rounded-[28px] border border-[var(--border)] bg-white p-5 shadow-[0_16px_36px_rgba(15,23,42,0.05)]">
                <p className="text-sm font-semibold tracking-[0.16em] text-[var(--primary)] uppercase">
                  Uso no celular
                </p>
                <div className="mt-4 space-y-3">
                  {mobileSteps.map((step, index) => (
                    <div key={step} className="flex items-start gap-3">
                      <span className="mt-0.5 flex h-7 w-7 shrink-0 items-center justify-center rounded-full bg-[var(--primary-soft)] text-sm font-semibold text-[var(--primary)]">
                        {index + 1}
                      </span>
                      <p className="text-sm leading-7 text-[var(--secondary)]">
                        {step}
                      </p>
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
                    Operacao
                  </p>
                  <p className="mt-3 text-2xl font-semibold leading-tight">
                    Produtos, clientes e movimentos com leitura rapida.
                  </p>
                  <div className="mt-5 rounded-[22px] bg-white/12 p-4 backdrop-blur">
                    <p className="text-xs text-white/66">Consulta</p>
                    <p className="mt-1 text-sm font-semibold">
                      Etiqueta, status e fornecedor no mesmo fluxo
                    </p>
                  </div>
                </div>

                <div className="mt-5 space-y-3">
                  {[
                    ["Clientes", "Historico e pendencias"],
                    ["Produtos", "Pecas, valores e status"],
                    ["Movimentos", "Vendas, devolucoes e emprestimos"],
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
                    Cliente
                  </p>
                  <p className="mt-3 text-2xl font-semibold leading-tight text-[var(--foreground)]">
                    Acompanhamento simples de pecas, vendas e creditos.
                  </p>
                  <div className="mt-5 flex items-center justify-between gap-4 rounded-[22px] border border-[var(--border)] bg-white px-4 py-3 shadow-[0_12px_28px_rgba(15,23,42,0.05)]">
                    <div>
                      <p className="text-xs text-[var(--secondary)]">
                        Pendencias
                      </p>
                      <p className="mt-1 text-sm font-semibold text-[var(--foreground)]">
                        Centralizadas por cliente
                      </p>
                    </div>
                    <div className="h-3 w-3 shrink-0 rounded-full bg-[#2fc98f]" />
                  </div>
                </div>

                <div className="mt-5 rounded-[24px] border border-[var(--border)] bg-[var(--surface-muted)] p-4">
                  <p className="text-sm font-semibold text-[var(--foreground)]">
                    Consultas no celular
                  </p>
                  <div className="mt-4 space-y-4">
                    {[
                      ["Minhas pecas", "Como fornecedor"],
                      ["Minhas compras", "Como cliente"],
                      ["Creditos", "Pendencias e resgates"],
                    ].map(([label, detail]) => (
                      <div
                        key={label}
                        className="flex items-center justify-between gap-3"
                      >
                        <div className="flex items-center gap-3">
                          <span className="h-3 w-3 shrink-0 rounded-full bg-[var(--primary)]" />
                          <span className="text-sm text-[var(--foreground)]">
                            {label}
                          </span>
                        </div>
                        <span className="text-xs font-medium text-[var(--secondary)]">
                          {detail}
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

      <section className="relative mx-auto max-w-7xl px-6 pb-24 lg:px-10">
        <div className="grid gap-8 rounded-[38px] border border-[var(--border)] bg-white p-8 shadow-[var(--shadow-soft)] lg:p-10 xl:grid-cols-[0.8fr_1.2fr]">
          <div>
            <p className="text-sm font-semibold tracking-[0.2em] uppercase text-[var(--primary)]">
              Para quem e
            </p>
            <h2 className="mt-4 text-3xl font-semibold text-[var(--foreground)] sm:text-4xl">
              Feita para lojas que precisam de controle operacional real.
            </h2>
          </div>
          <div className="grid gap-4 sm:grid-cols-2">
            {targetProfiles.map((profile) => (
              <div
                key={profile}
                className="rounded-[24px] border border-[var(--border)] bg-[var(--surface-muted)] p-5"
              >
                <p className="text-sm font-semibold text-[var(--foreground)]">
                  {profile}
                </p>
              </div>
            ))}
          </div>
        </div>
      </section>

      <section id="faq" className="relative mx-auto max-w-7xl px-6 pb-24 lg:px-10">
        <div className="mx-auto max-w-3xl text-center">
          <p className="text-sm font-semibold tracking-[0.2em] uppercase text-[var(--primary)]">
            Duvidas frequentes
          </p>
          <h2 className="mt-4 text-3xl font-semibold text-[var(--foreground)] sm:text-4xl">
            Perguntas comuns sobre sistema para brecho consignado.
          </h2>
          <p className="mt-5 text-base leading-8 text-[var(--secondary)]">
            Respostas diretas sobre controle de pecas, repasses, etiquetas,
            clientes e fechamento financeiro de brecho.
          </p>
        </div>

        <div className="mt-10 grid gap-5 lg:grid-cols-2">
          {faqItems.map((item) => (
            <article
              key={item.question}
              className="rounded-[28px] border border-[var(--border)] bg-white p-6 shadow-[0_18px_40px_rgba(15,23,42,0.06)]"
            >
              <h3 className="text-lg font-semibold text-[var(--foreground)]">
                {item.question}
              </h3>
              <p className="mt-3 text-sm leading-7 text-[var(--secondary)]">
                {item.answer}
              </p>
            </article>
          ))}
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
              <h2 className="mt-4 text-3xl font-semibold sm:text-5xl">
                Vamos conversar sobre a rotina do seu brecho.
              </h2>
              <p className="mt-5 max-w-xl text-base leading-8 text-white/78">
                No momento inicial da Renova, a melhor demonstracao e entender
                sua operacao: volume de pecas, repasses, devolucoes, etiquetas,
                fechamento e atendimento aos clientes.
              </p>

              <div className="mt-8 flex flex-col gap-4 sm:flex-row">
                <a
                  href="mailto:contato@renova.com.br?subject=Conversar%20sobre%20operacao%20-%20Renova"
                  className="inline-flex items-center justify-center rounded-full bg-white px-6 py-4 text-sm font-semibold !text-[var(--primary-strong)] shadow-[0_18px_36px_rgba(255,255,255,0.18)] transition hover:translate-y-[-2px] hover:!text-[var(--primary-strong)]"
                >
                  Conversar sobre sua operacao
                </a>
                <a
                  href="#demo"
                  className="inline-flex items-center justify-center rounded-full border border-white/24 bg-white/8 px-6 py-4 text-sm font-semibold text-white backdrop-blur transition hover:bg-white/12"
                >
                  Ver demonstracao
                </a>
              </div>
            </div>

            <div className="rounded-[32px] border border-white/14 bg-white/10 p-6 backdrop-blur">
              <p className="text-sm font-semibold tracking-[0.18em] text-white/68 uppercase">
                O que informar
              </p>
              <div className="mt-5 space-y-3">
                {[
                  "Como sua loja controla pecas consignadas hoje.",
                  "Quais etapas mais geram retrabalho no fechamento.",
                  "Se voce usa etiquetas, impressora termica ou planilhas.",
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
