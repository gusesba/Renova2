# Planejamento de revisao do sistema

Documento gerado sem alterar o codigo-fonte do sistema. O objetivo deste material e consolidar o diagnostico atual, as inconsistencias encontradas, os pontos a validar e um plano de revisao antes de qualquer refatoracao ou correcao.

## 1. Visao geral do sistema

### Resumo do que o sistema faz

O projeto implementa um sistema web para operacao de loja de brecho com foco em:

- controle de estoque de pecas
- ciclo de consignacao
- vendas com diferentes meios de pagamento
- credito da loja
- financeiro e repasses para fornecedores
- fechamentos por pessoa e periodo
- autenticacao multiusuario
- controle de permissoes por cargo e por acao
- relatorios e documentos imprimiveis

Como produto, o sistema ja cobre a maior parte do fluxo operacional principal de um brecho. Como codigo, a base ja possui muitas regras de negocio implementadas, porem com sinais claros de divergencia entre documentacao, nomenclatura de modulos, granularidade de permissoes e maturidade de testes.

### Principais modulos encontrados

Backend:

- Access
- Stores
- People
- Catalogs
- CommercialRules
- Pieces
- Consignments
- StockMovements
- Sales
- Credits
- Financial
- SupplierPayments
- Closings
- Dashboards
- Reports
- Documents
- Diagnostics

Frontend:

- `/dashboard` (na pratica, modulo administrativo de acesso)
- `/stores`
- `/catalogs`
- `/commercial-rules`
- `/people`
- `/pieces`
- `/consignments`
- `/stock-movements`
- `/sales`
- `/credits`
- `/financial`
- `/supplier-payments`
- `/closings`
- `/indicators`
- `/reports`
- `/documents`
- `/profile`
- `/login`

### Tecnologias utilizadas

- Backend: ASP.NET Core / .NET 10, EF Core 10, PostgreSQL, API versioning, autenticacao por token opaco de sessao, OpenAPI
- Frontend: Next.js 16, React 19, TypeScript, Tailwind CSS 4, React Query, Zod, Sonner
- Persistencia: DbContext unico (`RenovaDbContext`) com convencoes de naming snake_case e controle de concorrencia por `RowVersion`

### Estrutura geral do backend

- `Backend/API`: controllers, bootstrap, configuracao HTTP, auth e pipeline
- `Backend/Servicos`: servicos de aplicacao por modulo, contratos e regras de orquestracao
- `Backend/Persistencia`: `DbContext`, mapeamentos e migrations
- `Backend/Dominio`: entidades centrais do negocio

Leitura arquitetural atual:

- Controllers tendem a ser finos
- A maior parte da regra de negocio mora na camada `Servicos`
- O padrao geral e por feature/modulo, o que ajuda a rastreabilidade
- Existe auditoria de acesso e auditoria funcional em varios pontos
- Ha transacoes explicitas em operacoes criticas como venda e liquidacao de obrigacoes

### Estrutura geral do frontend

- App Router do Next com grupo autenticado em `frontend/app/(system)`
- Uma tela principal por modulo, normalmente em formato `dashboard`
- Separacao razoavel entre `components`, `lib/services`, `lib/schemas` e `helpers`
- Guardas visuais centralizados em `frontend/lib/helpers/access-control.ts`
- Sessao do usuario e troca de loja gerenciadas pelo provider do grupo autenticado

Leitura funcional atual:

- A interface e consistente entre modulos
- O sistema possui muitas entradas de menu de primeiro nivel
- Varias telas concentram responsabilidades demais em um unico fluxo
- A nomenclatura de algumas rotas cria ambiguidade de produto

### Bases analisadas

Documentacao:

- `Documentacao Renova/Ideia Inicial.md`
- `Documentacao Renova/Requisitos por Modulos.md`
- `Documentacao Renova/Checklist Implementacao Renova.md`
- `Documentacao Renova/Modelagem Banco de Dados Renova.md`
- `Documentacao Renova/Relacoes Funcoes App.md`
- `Documentacao Renova/Cadastro Publico Auth.md`
- `Backend/README.md`
- `frontend/README.md`

Codigo de referencia principal:

- `Backend/API/Program.cs`
- `Backend/API/Infrastructure/DependencyInjection/ApiServiceCollectionExtensions.cs`
- `Backend/Persistencia/RenovaDbContext.cs`
- `Backend/Servicos/Features/...`
- `frontend/app/(system)/components/system-route-frame.tsx`
- `frontend/lib/helpers/access-control.ts`
- dashboards e services de cada modulo no frontend

### Diagnostico executivo inicial

- O produto implementado e mais avancado do que a documentacao em algumas areas e menos completo em outras.
- Existe divergencia real entre o que a documentacao descreve e o que esta no repositorio.
- O backend possui regras de negocio relevantes bem implementadas, principalmente em vendas, financeiro, credito e fechamentos.
- O modelo de permissao existe, mas a granularidade atual nao esta bem equilibrada.
- O frontend e consistente, mas a arquitetura de navegacao ainda nao esta ideal para operacao diaria.
- Nao foram encontrados projetos de testes automatizados nem workflows de CI/CD no repositorio.
- Nao foram localizados projetos de portal web externo ou app mobile, apesar de a documentacao e o catalogo de permissoes mencionarem esses canais.

## 2. Mapeamento das regras de negocio

| Nome da regra                                   | Descricao                                                                                               | Origem da regra                       | Modulos impactados                                          | Nivel de clareza   | Observacoes                                                                                             |
| ----------------------------------------------- | ------------------------------------------------------------------------------------------------------- | ------------------------------------- | ----------------------------------------------------------- | ------------------ | ------------------------------------------------------------------------------------------------------- |
| Cadastro publico de usuario                     | Qualquer pessoa pode criar conta basica a partir da tela de login                                       | documentacao + codigo                 | Access, Frontend login                                      | claro              | A conta nasce ativa imediatamente; nao ha verificacao de email                                          |
| Login com sessao e loja ativa                   | O login cria uma sessao autenticada e define uma loja ativa quando houver vinculo                       | documentacao + codigo                 | Access, Stores, shell do frontend                           | claro              | Quando nao ha loja vinculada, o usuario fica autenticado mas sem operacao de loja                       |
| Usuario vinculado a loja por cargo              | O acesso real depende de vinculo ativo do usuario com loja e cargos associados                          | documentacao + codigo                 | Access, Stores, todos os modulos                            | claro              | Base central do modelo multiusuario e multiloja                                                         |
| Cargos compostos por permissoes                 | Cargos agregam permissoes e sao atribuiveis por loja                                                    | documentacao + codigo                 | Access                                                      | claro              | Modelo coerente com RBAC por loja                                                                       |
| Cadastro mestre de pessoa com relacao por loja  | A pessoa existe como cadastro mestre e ganha atributos operacionais no contexto da loja                 | documentacao + codigo                 | People, Credits, Closings, SupplierPayments, Sales          | claro              | Bom modelo para cliente/fornecedor recorrente                                                           |
| Catalogos auxiliares por loja                   | Produto nome, marca, cor e tamanho sao gerenciados por loja                                             | codigo + inferencia a partir do fluxo | Catalogs, Pieces                                            | parcialmente claro | Diverge da documentacao antiga que cita categoria, colecao e cadastros compartilhados                   |
| Regras comerciais por loja e por fornecedor     | Politicas de repasse, desconto automatico e meios de pagamento ficam configuradas no contexto comercial | documentacao + codigo                 | CommercialRules, Pieces, Sales, Consignments                | claro              | Regras afetam venda, consignacao e liquidez                                                             |
| Cadastro de peca com historico e imagens        | Peca possui referencia de produto, fornecedor, preco, imagens e movimentacoes de estoque                | documentacao + codigo                 | Pieces, StockMovements, Sales, Consignments                 | claro              | E o nucleo operacional do sistema                                                                       |
| Ciclo de consignacao                            | Pecas em consignacao seguem prazo, desconto automatico, alertas e encerramento                          | documentacao + codigo                 | Consignments, Pieces, Alerts                                | parcialmente claro | No codigo, parte do comportamento automatico e disparada em leitura do modulo, nao em processo agendado |
| Venda com composicao de pagamentos              | Venda valida itens, descontos e pagamentos; afeta estoque, financeiro, credito e obrigacoes             | documentacao + codigo                 | Sales, StockMovements, Financial, Credits, SupplierPayments | claro              | Uma das regras mais solidas do sistema                                                                  |
| Cancelamento de venda com reversoes             | Cancelamento reverte efeitos gerados na venda original                                                  | documentacao + codigo                 | Sales, StockMovements, Financial, Credits, SupplierPayments | claro              | Regra importante e implementada com transacao                                                           |
| Credito da loja                                 | Cliente ou fornecedor pode ter conta de credito para uso ou liquidacao interna                          | documentacao + codigo                 | Credits, Sales, SupplierPayments, Closings                  | claro              | Existe consulta do proprio usuario ao proprio credito                                                   |
| Liquidacao de obrigacao com dinheiro ou credito | Repasse ao fornecedor pode ocorrer por financeiro ou por credito da loja                                | documentacao + codigo                 | SupplierPayments, Financial, Credits                        | claro              | Reforca o papel do credito como saldo operacional                                                       |
| Fechamento por pessoa e periodo                 | O sistema gera snapshot financeiro/comercial para conferencia e liquidacao                              | documentacao + codigo                 | Closings, Credits, SupplierPayments, Sales                  | claro              | Exportacao existe, mas formato diverge do nome                                                          |
| Relatorios com filtros salvos                   | Usuario pode filtrar estoque, pecas vendidas, financeiro e baixas e salvar filtros                      | documentacao + codigo                 | Reports                                                     | claro              | Visualizacao e exportacao estao no mesmo modulo                                                         |
| Documentos imprimiveis                          | O sistema gera etiquetas, recibos e comprovantes padronizados                                           | documentacao + codigo                 | Documents, Sales, SupplierPayments, Consignments            | claro              | Entrega em HTML imprimivel; nao ha configuracao visual por loja                                         |
| Controle de permissao por acao                  | Acoes e modulos sao liberados conforme as permissoes do usuario na loja ativa                           | documentacao + codigo                 | Access, todos os modulos                                    | claro              | A granularidade atual esta incompleta                                                                   |
| Portal externo para cliente/fornecedor          | Cliente/fornecedor deveria consultar dados, extratos ou relatorios externos                             | documentacao                          | Portal, Access, Credits, Reports                            | ambiguo            | Nao localizado no repositorio atual; ponto a validar                                                    |
| App mobile para consulta                        | Documentacao e permissoes sugerem canal mobile de consulta                                              | documentacao + permissao existente    | Mobile, Access, Credits                                     | ambiguo            | Nao localizado no repositorio atual; ponto a validar                                                    |
| Configuracao operacional da loja                | Documentacao antiga preve cabecalho, rodape, moeda e timezone da loja                                   | documentacao antiga                   | Stores, Documents, Reports                                  | ambiguo            | A entidade `loja_configuracao` foi removida por migration; ponto a validar                              |

### Regras ausentes na documentacao, mas aparentes no codigo

- Cadastro publico gera usuario imediatamente ativo, sem etapa de aprovacao ou verificacao de email.
- Venda e cancelamento geram efeitos transacionais encadeados em estoque, financeiro, credito e obrigacoes.
- O sistema mantem auditoria funcional e de acesso em diversos fluxos criticos.
- O modulo de consignacao sincroniza alertas e descontos durante leituras do workspace e da listagem.

### Regras documentadas, mas nao encontradas no codigo atual

- Portal web dedicado para cliente/fornecedor
- Aplicativo mobile de consulta
- Configuracao detalhada da loja para documentos, timezone e moeda
- Estrutura antiga de catalogos com categoria, colecao e conjunto de catalogo

### Regras conflitantes

- A documentacao antiga trata catalogos como mais amplos e parcialmente compartilhados; o codigo atual trabalha com cadastros auxiliares simplificados e isolados por loja.
- O checklist de implementacao sugere cobertura completa de varios modulos, mas o repositorio nao contem portal, mobile, testes automatizados nem configuracao de documentos por loja.
- O nome "PDF/Excel" aparece nos fluxos de exportacao, mas o codigo entrega HTML imprimivel e CSV.

### Regras que precisam ser esclarecidas

- Ponto a validar: usuario recem-cadastrado deve ficar ativo de imediato ou depender de aprovacao/verificacao?
- Ponto a validar: usuarios de uma loja podem enxergar usuarios de outras lojas para fins de vinculacao?
- Ponto a validar: desconto automatico de consignacao pode depender da abertura da tela ou deve rodar de forma autonoma?
- Ponto a validar: PDF e Excel reais sao obrigatorios ou HTML/CSV sao aceitaveis como MVP?
- Ponto a validar: portal e mobile continuam dentro do escopo desta base ou foram retirados do projeto?
- Ponto a validar: configuracao visual e fiscal da loja ainda e requisito de negocio?

## 3. Verificacao de aderencia entre regra de negocio e implementacao

| Regra | Onde esta implementada no backend | Onde esta refletida no frontend | Status | Evidencias encontradas | Impacto do problema | Sugestao de ajuste futuro |
| --- | --- | --- | --- | --- | --- | --- |
| Cadastro publico de usuario | `AccessAuthService`, `AuthController` | `login-screen.tsx`, `register-form.tsx` | implementada parcialmente | Fluxo existe fim a fim, mas a conta nasce ativa sem verificacao adicional | Risco de seguranca e governanca de acesso | Validar politica de onboarding e aprovacoes |
| Login, sessao e loja ativa | `AccessAuthService`, auth custom e contexto de requisicao | `system-session-provider`, login e shell autenticado | implementada corretamente | Fluxo de login, `me`, logout e troca de loja estao presentes | Baixo no fluxo basico | Consolidar testes de regressao do ciclo de sessao |
| Gestao de usuarios, cargos e vinculos | `AccessUserService`, `AccessRoleService`, `AccessStoreMembershipService` | modulo `/dashboard` | implementada parcialmente | Criacao, status, cargos e vinculos existem, mas edicao de usuario no backend e apenas de si mesmo | Medio para governanca administrativa | Separar edicao propria de administracao de usuarios |
| Cadastro mestre de pessoas | `PersonService` | `/people` | implementada parcialmente | Cadastro, detalhe, contas bancarias e resumo financeiro existem | Medio por concentrar muitas responsabilidades e expor usuarios globais para vinculacao | Rever UX da tela e o escopo dos usuarios vinculaveis |
| Catalogos auxiliares | `CatalogService` | `/catalogs` | implementada de forma divergente | Codigo atual cobre produto nome, marca, cor e tamanho; docs antigas esperam mais entidades | Medio por desalinhamento documental | Atualizar documentacao e confirmar escopo real do catalogo |
| Regras comerciais | `CommercialRuleService` | `/commercial-rules` | implementada corretamente | Workspace, regra da loja, regra do fornecedor e meios de pagamento existem | Baixo | Cobrir com testes de validacao e leitura/escrita |
| Pecas e estoque | `PieceService`, `StockMovementService` | `/pieces`, `/stock-movements` | implementada parcialmente | Cadastro, detalhe, imagens e ajustes existem | Medio, pois nao ha permissao clara para editar peca sem cadastrar/ajustar | Separar permissoes de leitura, cadastro, edicao e ajuste |
| Consignacao | `ConsignmentService` | `/consignments` | implementada parcialmente | Prazo, desconto e encerramento existem; alertas e descontos sao atualizados em consultas | Medio/alto por dependencia de acesso de tela para automacao | Definir se deve haver job/processo dedicado |
| Vendas | `SaleService` e operacoes transacionais | `/sales` | implementada corretamente | Criacao e cancelamento atualizam estoque, financeiro, credito e obrigacoes | Baixo no fluxo principal | Priorizar testes automatizados para preservar esse fluxo |
| Credito da loja | `CreditService` | `/credits` | implementada parcialmente | Fluxo de conta, extrato e lancamento existe; ha consulta do proprio usuario | Medio por falta de portal/mobile dedicados | Confirmar se autoconsulta deve virar canal externo proprio |
| Financeiro e repasses | `FinancialService`, `SupplierPaymentService` | `/financial`, `/supplier-payments` | implementada parcialmente | Livro razao, conciliacao, obrigacoes e liquidacoes existem | Medio por sobreposicao de permissoes e modulos separados artificialmente | Rever IA de modulo e permissoes especificas |
| Fechamentos | `ClosingService` | `/closings` | implementada parcialmente | Geracao, conferencia, liquidacao e exportacao existem | Medio por exportacao chamada de PDF/Excel sem entregar esses formatos | Definir semantica de exportacao ou trocar tecnologia de geracao |
| Relatorios e exportacoes | `ReportService` | `/reports` | implementada de forma divergente | Relatorios e filtros salvos existem; exportacao "pdf" vira HTML e "excel" vira CSV | Medio/alto por expectativa incorreta de negocio e UX | Alinhar formato real e permissao de visualizacao/exportacao |
| Documentos imprimiveis | `DocumentService` | `/documents` | implementada parcialmente | Etiquetas, recibos e comprovantes existem | Medio por ausencia de configuracao por loja e permissao dedicada | Tornar impressao mais contextual e configuravel |
| Controle de permissao por acao | `RequirePermission`, helpers de contexto e codigos de permissao | `access-control.ts`, guards e botoes de modulos | implementada parcialmente | Ha protecao de rota e acao, mas o desenho da matriz e incompleto e indireto | Alto, pois pode gerar acesso amplo demais ou restricao errada | Revisar a matriz antes de mexer na implementacao |
| Portal externo e mobile | Apenas indicios em permissoes e consulta propria de credito | nao localizado | nao implementada | Nao ha projeto portal/mobile neste repositorio | Alto se continuar sendo requisito | Confirmar escopo e separar backlog fora do core web interno |
| Configuracao da loja para documentos | nao localizada no modelo atual | nao localizada | nao implementada | Migration remove `loja_configuracao`; nao ha tela equivalente | Medio/alto se a operacao exigir cabecalho/rodape/modelos | Validar se o requisito foi abandonado ou esta pendente |

## 4. Avaliacao funcional e estrutural do frontend

### Leitura geral

O frontend possui padrao visual e estrutural consistente, o que ajuda manutencao e onboarding. O problema principal nao esta na falta de organizacao basica, e sim no excesso de modulos de primeiro nivel, na densidade de algumas telas e na diferenca entre "nome do modulo" e "papel real no produto".

### Avaliacao por modulo/tela

| Modulo/tela | Objetivo da tela | Avaliacao funcional | Avaliacao estrutural | Direcao sugerida |
| --- | --- | --- | --- | --- |
| `/dashboard` | administrar usuarios, cargos, permissoes e vinculos | Faz sentido existir | O nome conflita com a ideia de painel operacional | Renomear para `Acesso` ou `Administracao > Acesso` |
| `/stores` | cadastro e estrutura de lojas | Necessario | Coerente | Manter |
| `/catalogs` | manter cadastros auxiliares | Necessario | Coerente, mas pode ficar perto de configuracoes | Manter ou agrupar em `Configuracoes` |
| `/commercial-rules` | configurar regras comerciais e meios de pagamento | Necessario | Muito proximo de catalogos/configuracoes | Considerar agrupamento com `Catalogos` dentro de `Configuracoes` |
| `/people` | gerir clientes e fornecedores | Necessario | Tela muito carregada: cadastro, vinculo, contas e resumo financeiro no mesmo espaco | Separar em abas: cadastro, relacionamento, financeiro |
| `/pieces` | operar pecas e estoque | Necessario e central | Coerente, mas e pesado | Manter como modulo principal |
| `/consignments` | acompanhar ciclo de consignacao | Faz sentido | Muito conectado a pecas/estoque | Considerar virar submodulo de estoque |
| `/stock-movements` | consultar historico e fazer ajustes | Faz sentido | Muito conectado a pecas/estoque | Considerar virar aba de estoque |
| `/sales` | registrar e cancelar vendas | Necessario e central | Coerente | Manter como modulo principal |
| `/credits` | operar credito da loja | Necessario se credito for parte da operacao | Pode se tornar contextual demais se usado pouco | Manter, mas integrar melhor com pessoas e vendas |
| `/financial` | livro razao, conciliacao e lancamentos | Necessario | Coerente | Manter |
| `/supplier-payments` | repasses e liquidacoes | Faz sentido | Muito acoplado a financeiro e fechamentos | Considerar virar secao de financeiro |
| `/closings` | gerar e conferir fechamentos | Necessario | Coerente, mas dependente do dominio financeiro | Manter, com navegacao melhor a partir de pessoas/financeiro |
| `/indicators` | dashboards e indicadores | Necessario | Este deveria ser o dashboard principal do produto | Reposicionar como tela inicial do sistema |
| `/reports` | consultar e exportar relatorios | Necessario | Hoje mistura leitura e exportacao numa permissao so | Manter, mas separar visualizar de exportar |
| `/documents` | centralizar impressoes | Util para operacao | Como menu de primeiro nivel pode ser generico demais | Preferir impressoes contextuais dentro dos modulos de origem |
| `/profile` | editar perfil proprio | Necessario | Coerente | Manter |
| `/login` | autenticar, registrar e redefinir senha | Necessario | Coerente, mas o fluxo de token e inseguro | Revisar fluxo de recuperacao |

### Sugestoes sobre modulos realmente necessarios

- Essenciais como modulos principais: Indicadores, Pecas/Estoque, Vendas, Pessoas, Financeiro, Fechamentos, Acesso, Lojas
- Necessarios, mas passivos de agrupamento: Catalogos, Regras comerciais, Credito, Relatorios
- Melhor tratados como submodulos ou fluxos contextuais: Consignacao, Movimentacoes de estoque, Repasses a fornecedor, Documentos

### Modulos que podem ser fundidos

- `Catalogos` + `Commercial Rules` em uma area macro de `Configuracoes comerciais`
- `Pieces` + `Consignments` + `Stock Movements` em uma area macro de `Estoque`
- `Financial` + `Supplier Payments` em uma area macro de `Financeiro`
- `Documents` como acoes contextuais de `Pieces`, `Sales`, `Supplier Payments` e `Closings`

### Modulos que podem precisar ser separados

- `People` em blocos claros: cadastro basico, relacionamento por loja, contas bancarias, resumo financeiro
- `Acesso` em subareas: usuarios, cargos, vinculos e eventualmente auditoria
- `Pecas` em listagem operacional e formulario detalhado, caso a tela continue crescendo

### Fluxos que deveriam ser simplificados

- Navegacao inicial: o usuario deveria cair em `Indicadores` ou numa home operacional, nao em `Acesso`
- Estoque: consignacao e movimentacoes poderiam ser acessadas pela peca ou pelo dominio de estoque, nao como menus separados obrigatorios
- Financeiro: repasses e fechamentos precisam parecer continuidade natural do fluxo financeiro
- Documentos: impressao deve nascer de contexto, nao exigir ida para modulo isolado sempre que possivel

### Paginas que parecem acumular responsabilidades demais

- `People`
- `Pieces`
- `Sales`
- `Commercial Rules`
- shell principal por concentrar navegacao, contexto e controle de acesso em uma unica composicao

### Componentes que parecem reutilizaveis ou excessivamente especificos

Reutilizaveis:

- paineis de overview
- listas com filtros e selecao
- `AccessStateCard`
- formularios com Zod + React Query

Excessivamente especificos:

- dashboards muito densos com regras de composicao internas
- telas que fazem fallback para dados de outros modulos por falta de contrato mais claro de workspace

### Coerencia com um sistema de loja/brecho multiusuario

A organizacao atual e coerente o suficiente para um backoffice rico, mas ainda nao esta ideal para operacao diaria de loja. A estrutura atende um usuario administrativo/tatico; falta simplificacao para operador, caixa, vendedor e financeiro. O sistema parece mais "modular por implementacao" do que "modular por jornada operacional".

### Proposta de reorganizacao do frontend

Navegacao macro sugerida:

- Inicio
- Estoque
- Vendas
- Pessoas
- Financeiro
- Relatorios
- Administracao
- Perfil

Distribuicao sugerida:

- Inicio: indicadores, alertas operacionais, pendencias, atalhos
- Estoque: pecas, consignacao, movimentacoes, documentos de etiqueta
- Vendas: registrar venda, consultar vendas, cancelar, recibos
- Pessoas: clientes, fornecedores, vinculo com usuario, contas, credito
- Financeiro: livro razao, lancamentos, repasses, fechamentos
- Relatorios: consultas e exportacoes
- Administracao: acesso, cargos, permissoes, lojas, configuracoes comerciais e catalogos

Diretriz de UX:

- menus principais devem refletir dominos de negocio, nao subfuncoes tecnicas
- acoes de impressao e exportacao devem estar proximas do contexto onde nascem
- telas longas devem migrar para abas ou subpassos com responsabilidade mais clara

## 5. Avaliacao de permissoes e perfis de acesso

### Diagnostico da matriz atual

Permissoes localizadas no codigo:

- `usuarios.visualizar`
- `usuarios.gerenciar`
- `cargos.gerenciar`
- `lojas.gerenciar`
- `pessoas.visualizar`
- `pessoas.gerenciar`
- `catalogo.gerenciar`
- `regras.gerenciar`
- `pecas.visualizar`
- `pecas.cadastrar`
- `pecas.ajustar`
- `vendas.registrar`
- `vendas.cancelar`
- `credito.visualizar`
- `credito.gerenciar`
- `financeiro.visualizar`
- `financeiro.conciliar`
- `fechamento.gerar`
- `fechamento.conferir`
- `relatorios.exportar`
- `alertas.visualizar`
- `portal.consultar`
- `mobile.consultar`

Problemas observados:

- Faltam permissoes de leitura para alguns modulos importantes, como vendas, relatorios, fechamentos e repasses.
- Alguns modulos dependem de permissoes indiretas ou pouco relacionadas ao proprio dominio.
- `documents` nao possui permissao dedicada.
- `supplier-payments` depende de permissao financeira generica.
- `indicators` depende de varias permissoes operacionais, sem permissao propria de leitura.
- `catalogo.gerenciar` ainda carrega descricao ligada a tabelas compartilhadas, o que conflita com o modelo atual por loja.
- Existem permissoes de portal e mobile sem os respectivos canais no repositorio.

### Lista final sugerida de permissoes

| Modulo | Permissao sugerida | Descricao | Justificativa |
| --- | --- | --- | --- |
| Acesso | `usuarios.visualizar` | listar usuarios do contexto permitido | Necessaria para leitura sem liberar manutencao |
| Acesso | `usuarios.gerenciar` | criar, editar status e administrar usuarios | Acoes administrativas exigem permissao propria |
| Acesso | `cargos.gerenciar` | criar e editar cargos e suas permissoes | RBAC precisa de ownership claro |
| Acesso | `vinculos.gerenciar` | vincular usuario a loja e cargos | Separar gestao de vinculos da gestao de usuarios |
| Lojas | `lojas.visualizar` | consultar lojas acessiveis e dados basicos | Evita liberar manutencao desnecessaria |
| Lojas | `lojas.gerenciar` | criar e editar lojas | Acao estrutural e critica |
| Pessoas | `pessoas.visualizar` | consultar clientes e fornecedores da loja | Leitura operacional frequente |
| Pessoas | `pessoas.gerenciar` | criar e editar pessoas, contas e relacoes | Cadastro e manutencao exigem controle |
| Catalogos | `catalogo.gerenciar` | manter cadastros auxiliares da loja | Pode permanecer sem permissao so de leitura se o modulo for administrativo |
| Regras comerciais | `regras.gerenciar` | configurar politicas comerciais e meios de pagamento | Impacta venda, consignacao e repasses |
| Estoque | `pecas.visualizar` | consultar pecas e estoque | Necessaria para vendedor, caixa e consulta |
| Estoque | `pecas.cadastrar` | cadastrar novas pecas | Separa entrada de estoque de outras operacoes |
| Estoque | `pecas.editar` | alterar cadastro e precificacao de pecas | Hoje esta misturado; vale explicitar |
| Estoque | `estoque.ajustar` | registrar ajustes e correcao de estoque | Acao sensivel, precisa de trilha clara |
| Consignacao | `consignacao.visualizar` | consultar prazos, alertas e situacao | Evita usar permissao de peca como proxy |
| Consignacao | `consignacao.gerenciar` | aplicar desconto, encerrar e tratar consignacao | Acoes operacionais especificas |
| Movimentos | `movimentos.visualizar` | consultar historico de estoque | Leitura importante para auditoria e conferencias |
| Vendas | `vendas.visualizar` | consultar historico de vendas | Faltante na matriz atual |
| Vendas | `vendas.registrar` | criar vendas | Acao primaria do comercial |
| Vendas | `vendas.cancelar` | cancelar venda e gerar reversoes | Acao critica e sensivel |
| Credito | `credito.visualizar` | consultar contas e extratos | Leitura frequente em atendimento |
| Credito | `credito.gerenciar` | criar movimentos e ajustar saldo | Operacao sensivel |
| Financeiro | `financeiro.visualizar` | consultar livro razao, resumos e conciliacao | Faltante em alguns fluxos derivados |
| Financeiro | `financeiro.lancar` | criar lancamentos manuais | Diferencia lancar de conciliar |
| Financeiro | `financeiro.conciliar` | conciliar movimentos e taxas | Acao especifica do financeiro |
| Repasses | `repasses.visualizar` | consultar obrigacoes e historico de liquidacoes | Evita depender apenas de financeiro generico |
| Repasses | `repasses.liquidar` | liquidar obrigacoes do fornecedor | Acao sensivel |
| Fechamentos | `fechamentos.visualizar` | consultar snapshots e detalhes | Faltante na matriz atual |
| Fechamentos | `fechamentos.gerar` | gerar ou regerar fechamento | Acao operacional especifica |
| Fechamentos | `fechamentos.conferir` | conferir e liquidar fechamento | Pode cobrir conferencia e liquidacao ou ser quebrada em duas |
| Indicadores | `indicadores.visualizar` | acessar dashboards e KPIs | Evita liberar indicadores por efeito colateral |
| Relatorios | `relatorios.visualizar` | abrir e consultar relatorios | Deve ser diferente de exportar |
| Relatorios | `relatorios.exportar` | exportar relatorios | Acao mais sensivel ou controlada |
| Documentos | `documentos.imprimir` | gerar etiquetas, recibos e comprovantes | Falta hoje e deveria ser explicita |
| Alertas | `alertas.visualizar` | visualizar pendencias operacionais | Permissao ja existe e precisa de uso coerente |
| Portal | `portal.consultar` | acessar canal externo de consulta | Manter apenas se o portal continuar no escopo |
| Mobile | `mobile.consultar` | acessar canal mobile de consulta | Manter apenas se o mobile continuar no escopo |

### Permissoes faltantes

- `vinculos.gerenciar`
- `lojas.visualizar`
- `pecas.editar`
- `consignacao.visualizar`
- `consignacao.gerenciar`
- `movimentos.visualizar`
- `vendas.visualizar`
- `financeiro.lancar`
- `repasses.visualizar`
- `repasses.liquidar`
- `fechamentos.visualizar`
- `indicadores.visualizar`
- `relatorios.visualizar`
- `documentos.imprimir`

### Permissoes desnecessarias ou a validar

- `portal.consultar` e `mobile.consultar` se esses canais nao fizerem mais parte do produto

### Permissoes redundantes ou excessivamente amplas

- Uso de permissoes de pecas para abrir consignacao e movimentacoes
- Uso de permissoes financeiras para abrir repasses
- Uso de `relatorios.exportar` como chave de entrada do modulo inteiro
- Uso de varias permissoes operacionais como chave de leitura de indicadores

### Sugestao de perfis de usuario

| Perfil sugerido | Escopo | Permissoes predominantes |
| --- | --- | --- |
| Proprietario/Administrador da loja | gestao completa da unidade | praticamente todas as permissoes da loja |
| Gerente | operacao ampla com algumas restricoes administrativas | pessoas, pecas, vendas, credito, financeiro, relatorios, fechamentos, leitura de acesso |
| Vendedor/Caixa | atendimento e venda | pessoas.visualizar, pecas.visualizar, vendas.visualizar, vendas.registrar, credito.visualizar, documentos.imprimir |
| Estoquista/Consignacao | entrada, manutencao de pecas e ciclo de consignacao | pecas.visualizar, pecas.cadastrar, pecas.editar, estoque.ajustar, consignacao.visualizar, consignacao.gerenciar, movimentos.visualizar |
| Financeiro | controle financeiro e repasses | financeiro.visualizar, financeiro.lancar, financeiro.conciliar, repasses.visualizar, repasses.liquidar, fechamentos.visualizar, fechamentos.conferir, relatorios.visualizar, relatorios.exportar |
| Consulta/Relatorios | leitura gerencial sem operacao | indicadores.visualizar, relatorios.visualizar, eventualmente relatorios.exportar |

Ponto a validar:

- Administrador deve poder editar cadastro completo de outros usuarios ou apenas status/vinculos/cargos?
- Fechamento deve separar `conferir` de `liquidar`?
- `catalogo` e `regras` devem ter leitura independente ou podem continuar administrativos puros?

## 6. Verificacao da aplicacao das permissoes no backend e frontend

### Backend

Pontos positivos:

- A maior parte dos modulos possui autenticacao obrigatoria.
- Varias operacoes criticas validam permissao e contexto de loja dentro dos servicos.
- Fluxos financeiros e de venda concentram a regra no servidor, o que reduz dependencia do frontend.

Problemas identificados:

| Local | Descricao | Severidade | Risco | Sugestao futura |
| --- | --- | --- | --- | --- |
| `AccessUserService.ListarAsync` | lista todos os usuarios do sistema e depois apenas enriquece o vinculo da loja ativa | alta | exposicao de nome, email e telefone de usuarios sem relacao com a loja | Restringir consulta ao escopo permitido ou criar modo explicito de busca global somente para administracao superior |
| `PersonService.ListarUsuariosVinculaveisAsync` | retorna todos os usuarios do sistema para vinculacao com pessoa | alta | vazamento de dados entre lojas e aumento de superficie de acesso indevido | Definir politica de visibilidade de usuarios e filtrar por escopo valido |
| `AccessAuthService.SolicitarRecuperacaoAsync` | devolve `token.RawToken` diretamente na resposta da API | alta | fluxo de recuperacao inseguro para ambiente real | Trocar por envio externo controlado ou fluxo somente administrativo/desenvolvimento |
| Controllers com apenas `[Authorize]` e sem permissao explicita em todas as rotas | ha inconsistencias entre uso de atributo e checagem interna de servico | media | padrao pouco previsivel e mais sujeito a erro futuro | Padronizar: rota critica com policy explicita e validacao de escopo no servico |
| Modulos sem permissao propria de leitura | leitura de relatorios, indicadores, fechamentos e repasses depende de permissoes de outro dominio | media/alta | acesso amplo demais ou restricao indevida | Revisar a matriz antes de reforcar controles |
| `UsersController.Update` + `AccessUserService.AtualizarAsync` | endpoint no modulo administrativo so permite autoedicao | media | regra de negocio fica ambigua e a UX administrativa fica incompleta | Separar endpoint de autoedicao e endpoint administrativo |
| Permissoes `alertas.visualizar`, `portal.consultar`, `mobile.consultar` sem cobertura equivalente | codigos existem, mas nao ha fluxo claro correspondente | baixa/media | matriz fica inflada e menos confiavel | Validar escopo real e remover ou implementar conscientemente |

### Frontend

Pontos positivos:

- O shell centraliza visibilidade de menu por permissao.
- Varios modulos fazem controle de acao em botoes e formularios, nao apenas de rota.
- O usuario recebe feedback explicito quando nao possui acesso.

Problemas identificados:

| Local | Descricao | Severidade | Risco | Sugestao futura |
| --- | --- | --- | --- | --- |
| `frontend/lib/helpers/access-control.ts` | varios modulos sao liberados por permissoes indiretas ou amplas demais | media/alta | menu e UX podem induzir acesso errado ou esconder modulo valido | Alinhar helpers com matriz revisada de permissoes |
| `frontend/app/(system)/components/system-route-frame.tsx` | guardas de rota sao client-side; nao ha middleware dedicado | media | navegacao inicial pode depender demais do cliente, embora o backend ainda proteja os dados | Considerar endurecimento de protecao de rota no frontend apenas como complemento |
| `/dashboard` versus `/indicators` | "dashboard" hoje significa acesso administrativo, enquanto o painel analitico esta em outra rota | media | confusao de produto e onboarding pior | Renomear rotas e labels para refletir o dominio real |
| `reports` | o modulo so abre para quem exporta | media | usuario pode precisar consultar sem exportar | Criar leitura separada de exportacao |
| `documents` | modulo aberto por permissoes de pecas, vendas ou financeiro, sem permissao dedicada | media | impressao pode ficar ampla demais | Introduzir `documentos.imprimir` e revisar visibilidade |
| `supplier-payments` | leitura e acao dependem de permissoes financeiras genericas | media | mistura responsabilidade de financeiro e repasse | Criar permissoes especificas de repasse |
| `login-screen.tsx` e `reset-password-panel.tsx` | o token de recuperacao e exibido e preenchido no proprio fluxo da UI | alta | risco direto de seguranca e padrao inadequado para producao | Redesenhar o fluxo apos alinhamento funcional |

Diferenca importante:

- Esconder acao no frontend nao substitui bloqueio no backend.
- O projeto ate protege boa parte das operacoes no servidor, mas a matriz atual ainda nao representa o negocio com a precisao necessaria.

## 7. Avaliacao da arquitetura do codigo

### Padroes arquiteturais usados hoje

Backend:

- separacao em `Dominio`, `Persistencia`, `Servicos` e `API`
- organizacao por feature dentro de `Servicos` e `API`
- controllers finos com servicos de aplicacao fazendo a orquestracao
- EF Core com `DbContext` unico
- autenticacao e autorizacao customizadas

Frontend:

- App Router com grupo autenticado central
- um `dashboard` por modulo
- `lib/services` para chamadas HTTP
- `lib/schemas` para validacao com Zod
- React Query para query/mutation

### Pontos onde a arquitetura esta boa

- Separacao geral de backend esta clara e previsivel
- Controllers estao relativamente leves
- O dominio principal do negocio ja existe no modelo relacional
- Ha transacoes nas operacoes mais criticas
- Auditoria funcional e de acesso ja foi considerada no desenho
- No frontend, o padrao entre modulos e consistente
- O provider de sessao e a troca de loja centralizam uma preocupacao importante

### Pontos onde esta inconsistente

- Servicos muito grandes acumulam regra de negocio, autorizacao, mapeamento, exportacao e montagem de resposta
- A validacao de permissao aparece parte em atributo, parte em helper de servico, parte em combinacoes ad hoc
- A documentacao esta atrasada em relacao a migrations e ao modelo atual
- O frontend usa nomenclatura de dominio inconsistente (`dashboard` administrativo vs `indicators` analitico)
- Alguns workspaces do frontend dependem de fallbacks em outros modulos, o que indica contrato incompleto entre tela e API

### Partes complexas demais para o problema

- Servicos de centenas de linhas para um unico modulo, principalmente `PieceService`, `ClosingService`, `ConsignmentService`, `PersonService`, `DocumentService`, `ReportService`
- Mistura de exportacao HTML/CSV dentro da camada de servico de negocio sem uma fronteira mais clara de renderizacao
- Duplica-se muita logica de contexto ativo, loja ativa e permissao ao longo de varios servicos

### Partes frageis demais

- Fluxo de recuperacao de senha
- Dependencia da abertura de tela para aplicar certas automacoes de consignacao
- Matriz de permissoes sem cobertura uniforme por modulo
- Ausencia de testes automatizados e CI

### Excesso de abstracao

- Nao ha excesso generalizado de abstracao. O problema maior e o inverso: regras demais concentradas no mesmo servico concreto.

### Falta de abstracao

- Falta camada reutilizavel para enforcement de escopo e permissao por modulo
- Falta isolamento de calculos de dominio mais pesados para facilitar teste unitario
- Falta contrato explicito para formatos de exportacao

### Duplicacao e acoplamento

- Duplicacao de verificacoes de contexto e permissao
- Acoplamento alto entre `DbContext` e servicos de aplicacao
- Acoplamento medio entre modulos do frontend por fallbacks de workspace

### Naming, padronizacao e previsibilidade

- A estrutura de pastas e previsivel
- Os nomes de contratos e responses seguem padrao consistente
- O problema de naming esta mais forte na camada de produto/rotas do frontend e na semantica de exportacoes

### Proposta futura de padronizacao arquitetural

Diretrizes sugeridas:

- Tratar documentacao funcional como contrato versionado e atualiza-la junto com mudancas de modelo
- Padronizar autorizacao em dois niveis
- policy explicita na borda HTTP para a capacidade do endpoint
- validacao de escopo de loja e regras sensiveis dentro do servico
- Dividir servicos muito grandes por responsabilidade
- consultas/workspace
- comandos de escrita
- calculos de dominio
- exportacao/renderizacao
- Extrair regras de calculo criticas para componentes testaveis sem dependencia direta de HTTP
- Nomear exportacoes pelo que realmente entregam ou passar a gerar formatos reais
- No frontend, reorganizar rotas por dominio de negocio e nao por subfuncao tecnica
- Evitar fallbacks cross-module quando o workspace do modulo deveria ser suficiente
- Padronizar os modulos administrativos em nomenclatura, permissao e visibilidade

## 8. Estrategia completa de testes

Observacao central:

- Nao foram encontrados projetos de teste do backend
- Nao foram encontrados testes proprietarios do frontend
- Nao foram encontrados workflows em `.github/workflows`
- O `frontend/package.json` possui apenas `dev`, `build`, `start` e `lint`

### 8.1 Testes unitarios

| Dominio/modulo | Objetivo | Cenarios | Prioridade | Justificativa | Dependencias ou pre-condicoes |
| --- | --- | --- | --- | --- | --- |
| Autenticacao | validar regras de login, registro e senha | normalizacao de email; senha invalida; usuario inativo; troca de loja ativa; alteracao de senha; expiracao de token; recuperacao de senha | alta | fluxo sensivel de seguranca | isolacao de hasher, token service e relogio |
| Usuarios, cargos e vinculos | garantir consistencia do RBAC | criacao de usuario; status invalido; autoedicao; composicao de cargo; vinculo expirado; usuario sem loja | alta | base de acesso do sistema | fixtures de usuarios, lojas, cargos e permissoes |
| Pessoas | validar cadastro mestre e relacao por loja | CPF/CNPJ; duplicidade por documento; vinculacao de usuario; contas bancarias; perfis cliente/fornecedor | alta | modulo transversal a vendas, credito e fechamentos | validadores de documento e builders de request |
| Catalogos e regras comerciais | garantir coerencia dos cadastros auxiliares e politicas | duplicidade por loja; meios de pagamento; regra padrao da loja; regra especifica de fornecedor; combinacoes invalidas | media/alta | influencia venda e consignacao | dados minimos de loja e fornecedor |
| Pecas e estoque | validar integridade do estoque | criacao de peca; historico de preco; imagens; status de peca; ajuste positivo/negativo; bloqueio de quantidade invalida | alta | nucleo operacional do produto | fixtures de catalogos, fornecedor e loja |
| Consignacao | validar prazo, desconto e encerramento | prazo calculado; desconto pendente; encerramento indevido; geracao de alerta; impacto por politica da loja/fornecedor | alta | automacao sensivel e com risco de divergencia | relogio controlado e fixtures de peca consignada |
| Vendas | validar composicao e consistencia da venda | soma de pagamentos; pagamento misto; uso de credito; item indisponivel; desconto acima do permitido; cancelamento e reversoes | alta | fluxo mais critico financeiramente | mocks ou fakes de estoque, credito e financeiro |
| Credito | garantir saldo e extrato corretos | criacao de conta; movimentacao manual; debito por venda; credito por repasse; saldo insuficiente | alta | afeta cliente, caixa e fornecedor | fixtures de conta de credito |
| Financeiro e repasses | validar ledger e liquidacoes | lancamento manual; conciliacao; taxa; obrigacao de fornecedor; liquidacao por caixa; liquidacao por credito | alta | integridade financeira | fixtures de obrigacao e contas |
| Fechamentos | validar snapshot e liquidacao | periodo invalido; consolidacao de itens; consolidacao de movimentos; conferencia; bloqueio de liquidacao com saldo pendente | alta | fecha ciclo financeiro do fornecedor/cliente | dados de vendas, credito e repasses |
| Relatorios e documentos | validar filtros e formatos gerados | filtros salvos; tipos de relatorio; CSV/HTML; dataset vazio; rotulos de documentos; recibos e comprovantes | media | reduz regressao funcional e de UX | fixtures de dados reais por modulo |
| Validacoes, mapeamentos e tratamento de erro | garantir previsibilidade de mensagens e contratos | normalizacao de enums; validacao de campos; mapeamentos response; erros de negocio | media | evita falhas silenciosas e inconsistencias de API | utilitarios isolados |

### 8.2 Testes de integracao / request direto para o backend

| Area | Objetivo | Cenarios | Prioridade | Justificativa | Dependencias ou pre-condicoes |
| --- | --- | --- | --- | --- | --- |
| Auth publica e sessao | validar fluxo HTTP real de autenticacao | registrar; logar; obter `me`; trocar loja; logout; senha invalida; usuario inativo | alta | garante contrato real entre cliente e API | banco de teste e usuario seed |
| Autorizacao e escopo de loja | validar que o backend bloqueia acesso indevido | usuario sem loja; vinculo expirado; permissao ausente; troca de loja sem vinculo; acesso cross-store | alta | risco alto de seguranca e isolamento | dados de duas lojas e perfis diferentes |
| Acesso administrativo | validar usuarios, cargos e vinculos | listar; criar usuario; alterar status; criar cargo; atribuir permissao; criar vinculo; alterar cargos do vinculo | alta | sustenta o restante do sistema | usuarios e lojas seeds |
| Pessoas | validar CRUD e detalhe por loja | listar; detalhar; criar; atualizar; vincular usuario; bloquear duplicidade por documento | alta | modulo base de varios fluxos | pessoa seed e loja ativa |
| Catalogos e regras | validar configuracoes da loja | workspace; CRUD de catalogos; regra da loja; regra de fornecedor; meios de pagamento | media/alta | depende de boa configuracao do sistema | loja ativa e fornecedor seed |
| Pecas e movimentacoes | validar estoque real | criar peca; listar; detalhar; anexar imagem; ajuste manual; consulta de historico | alta | regra operacional critica | catalogos e fornecedor seeds |
| Vendas | validar fluxo completo de venda | workspace; criar venda; consulta de detalhe; cancelamento; estoque apos venda; ledger apos venda | alta | maior impacto no negocio | pecas disponiveis, meios de pagamento, possivel comprador |
| Credito | validar conta e extrato | criar/garantir conta; listar saldo; lancamento manual; uso em venda; consulta propria | alta | integra com vendas e repasses | conta seed e usuario logado |
| Financeiro e repasses | validar operacao financeira real | listar ledger; lancamento manual; conciliacao; listar obrigacoes; liquidar repasse via financeiro e credito | alta | risco de inconsistencias financeiras | obrigacao seed e conta de credito opcional |
| Fechamentos | validar consolidacao e exportacao | gerar; detalhar; conferir; liquidar; exportar HTML/CSV; bloquear liquidacao indevida | alta | fecha o ciclo de negocio | dados de vendas/repasses/credito no periodo |
| Relatorios e documentos | validar contratos de consulta e exportacao | workspace; executar relatorios; salvar/remover filtro; exportar; gerar etiqueta/recibo/comprovante | media/alta | impacto alto de uso operacional | dados multi-modulo |
| Casos invalidos, concorrencia e idempotencia | validar robustez | venda simultanea da mesma peca; dupla liquidacao; token expirado; cancelamento duplicado; payload invalido | alta | evita corrupcao de dados e falhas raras | ambiente de teste transacional e controle de paralelismo |

### 8.3 Testes end-to-end

Referencia sugerida:

- Playwright com ambiente seedado e usuarios por perfil
- Uso de MCP para orquestrar cenarios de navegacao, inspecao visual e validacao de estados

| Fluxo real | Objetivo | Cenarios | Prioridade | Justificativa | Dependencias ou pre-condicoes |
| --- | --- | --- | --- | --- | --- |
| Login, logout e acesso pendente | validar entrada no sistema | login valido; login invalido; logout; usuario sem loja; troca de loja | alta | fluxo universal | usuarios seedados com e sem vinculo |
| Cadastro publico e recuperacao | validar onboarding e redefinicao | registrar conta; tentar email duplicado; solicitar recuperacao; confirmar nova senha | alta | fluxo sensivel e hoje com risco de seguranca | ambiente controlado e definicao de como o token sera entregue |
| Gestao de usuarios/cargos/vinculos | validar administracao de acesso | criar cargo; atribuir permissoes; criar usuario; vincular loja; alterar status | alta | garante RBAC funcional | usuario administrador seedado |
| Acesso negado por perfil | validar matriz de permissao | vendedor sem acesso a financeiro; financeiro sem acesso a acesso administrativo; leitura sem acao de escrita | alta | evita regressao de seguranca/UX | perfis seedados |
| Navegacao por loja ativa | validar adaptacao do menu e dos dados | trocar loja; ver menu mudar; ver dados do contexto correto | alta | requisito multiloja central | usuario com duas lojas seedado |
| Cadastro e edicao de pessoa | validar jornada de cliente/fornecedor | criar pessoa; editar; vincular usuario; incluir conta bancaria; ver resumo financeiro | alta | fluxo transversal | loja ativa e usuario com permissao |
| Cadastro de peca e estoque | validar entrada de produto | criar peca; consultar lista; editar dados; ver movimentacao inicial; anexar imagem | alta | nucleo operacional | catalogos e fornecedor seedados |
| Ciclo de consignacao | validar prazos e acoes | consultar consignadas; aplicar desconto; encerrar consignacao; ver alerta | alta | regra especifica do brecho | pecas consignadas seedadas e relogio controlado |
| Venda com pagamento misto | validar venda ponta a ponta | selecionar peca; aplicar desconto; usar dinheiro/cartao/credito; concluir venda; validar recibo | alta | fluxo mais critico do produto | peca disponivel, comprador opcional e regras comerciais |
| Cancelamento de venda | validar reversao operacional | cancelar venda; validar retorno da peca; validar reflexo no financeiro/credito | alta | previne dano financeiro | venda seedada ou criada no proprio teste |
| Credito da loja | validar extrato e uso do saldo | criar/garantir conta; lancar manualmente; usar credito em venda; consultar extrato | media/alta | integra varios modulos | conta seedada e permissao adequada |
| Financeiro e repasses | validar operacao financeira diaria | lancamento manual; conciliacao; listar repasses; liquidar obrigacao; emitir comprovante | alta | fluxo central para fechamento | dados financeiros e obrigacoes seedadas |
| Fechamentos | validar fechamento de periodo | gerar snapshot; conferir; bloquear liquidacao com saldo; liquidar quando zerado; exportar | alta | fluxo gerencial sensivel | dados consolidados no periodo |
| Relatorios e documentos | validar leitura e exportacao | abrir modulo; aplicar filtros; salvar filtro; exportar; abrir documento imprimivel | media/alta | muito usado em conferencia e operacao | massa de dados representativa |
| Perfil proprio | validar manutencao do usuario autenticado | editar nome, email e telefone; trocar senha | media | diferencia autoedicao de administracao | usuario logado |
| Navegacao, feedback e resiliencia | validar UX funcional | loaders; mensagens de erro; links de retorno; responsividade basica; expiracao de sessao | media | reduz atrito operacional | ambiente com mocks de erro e sessao expirada |

## 9. Riscos, lacunas e pontos a validar

### Riscos de negocio

- Documentacao funcional e checklist nao refletem com precisao o estado atual do repositorio.
- O produto aparenta prometer portal, mobile e configuracoes de loja que nao foram localizados nesta base.
- Exportacoes com nome de PDF/Excel podem gerar expectativa errada de cliente ou equipe.

### Riscos de seguranca

- Token de recuperacao de senha retornado pela API e exposto na interface.
- Listagem global de usuarios em contexto de loja.
- Lista global de usuarios vinculaveis em pessoas.
- Credencial de banco presente em `Backend/API/appsettings.Development.json`.
- Dependencia excessiva de guardas client-side para UX, ainda que o backend proteja parte dos dados.

### Riscos de inconsistencias de dados

- Regras automaticas de consignacao dependentes da abertura de telas/consultas.
- Matriz de permissao indireta pode liberar leitura/escrita inadequada em modulos relacionados.
- Ausencia de testes automatizados aumenta risco de regressao silenciosa em vendas, credito, repasses e fechamentos.

### Riscos de UX

- Menu principal com muitos modulos de primeiro nivel.
- `/dashboard` e `/indicators` competem semanticamente.
- Modulo de documentos pode parecer generico demais fora do contexto da origem.
- Telas como `People`, `Pieces` e `Sales` podem sobrecarregar usuarios operacionais.

### Riscos de manutencao

- Servicos muito grandes e densos.
- Logica de permissao e contexto repetida em muitos servicos.
- Baixa confianca de mudanca por falta de testes e CI.
- Divergencia entre documentacao, migrations e implementacao corrente.

### Pontos nao documentados ou mal documentados

- Politica oficial de onboarding de usuario publico
- Politica oficial de reset de senha para producao
- Regra formal de visibilidade de usuarios entre lojas
- Estrategia esperada para automacoes de consignacao
- Escopo atual de portal e mobile
- Escopo atual de configuracoes operacionais da loja

### Pontos que precisam ser confirmados depois

- Ponto a validar: portal e mobile ainda fazem parte desta entrega?
- Ponto a validar: configuracao de loja para documentos, moeda e timezone segue sendo requisito?
- Ponto a validar: usuarios devem ser visiveis globalmente ou somente por loja?
- Ponto a validar: o administrador da loja deve poder editar todos os dados de outro usuario?
- Ponto a validar: exportacao precisa entregar PDF/XLSX reais?
- Ponto a validar: alertas e descontos de consignacao devem ser processados sem depender da interface?
- Ponto a validar: repasses devem continuar como modulo separado de financeiro?

## 10. Plano de acao sugerido

| Fase | Objetivo | Itens incluidos | Prioridade | Impacto esperado |
| --- | --- | --- | --- | --- |
| Fase 1 - Alinhar regra de negocio | fechar contrato funcional do sistema antes de mexer na implementacao | revisar documentacao x codigo; decidir sobre portal/mobile; decidir sobre configuracao de loja; definir semantica de exportacoes; validar onboarding e reset de senha | altissima | reduz retrabalho e evita corrigir o sistema para a regra errada |
| Fase 2 - Revisar seguranca e permissoes | estabilizar o modelo de acesso e os riscos de exposicao | redesenhar matriz de permissoes; separar leitura x escrita; definir escopo de usuario por loja; revisar reset de senha; revisar dados sensiveis em configuracao | altissima | reduz risco de acesso indevido e cria base correta para o restante |
| Fase 3 - Revisar aderencia regra x implementacao | transformar lacunas identificadas em backlog objetivo | confrontar cada regra mapeada com implementacao real; classificar divergencias; priorizar o que e bug, o que e lacuna e o que e apenas documentacao desatualizada | alta | gera backlog rastreavel e evita analise superficial |
| Fase 4 - Reorganizar frontend | simplificar navegacao e responsabilidade das telas | redefinir IA; renomear modulos; decidir fusoes/separacoes; reduzir excesso de menus; tornar documentos e subfluxos mais contextuais | alta | melhora usabilidade, onboarding e consistencia funcional |
| Fase 5 - Padronizar arquitetura | reduzir fragilidade tecnica sem criar complexidade desnecessaria | definir padrao de autorizacao; quebrar servicos grandes por responsabilidade; padronizar workspaces; alinhar naming de exportacao e modulo | alta | aumenta previsibilidade e manutencao futura |
| Fase 6 - Implementar estrategia de testes | criar rede de seguranca antes de refatorar pesado | testes unitarios por dominio; integracao por endpoint; E2E por jornada critica; pipeline de CI minima | altissima | reduz regressao e aumenta confianca para qualquer ajuste |
| Fase 7 - Somente entao refatorar e corrigir | executar mudancas com base estabilizada | corrigir lacunas priorizadas; endurecer seguranca; ajustar UX; refatorar codigo; revisar documentacao final | alta | permite evolucao consistente sem apagar regras validas do negocio |

### Ordem pratica recomendada

- Primeiro confirmar escopo, regras e permissoes
- Depois endurecer seguranca e definir backlog de divergencias
- Em seguida reorganizar navegacao e padrao arquitetural
- So depois investir em correcoes estruturais e refatoracoes amplas

### Resultado esperado ao fim da revisao

- regra de negocio documentada e validada
- matriz de permissao coerente com o produto
- backend e frontend com padroes previsiveis
- backlog objetivo de correcoes e refatoracoes
- estrategia de testes cobrindo fluxos criticos
- base pronta para manutencao e evolucao com menos risco
