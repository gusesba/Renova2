# Planejamento de refatoracao do codigo

Documento de planejamento tecnico criado a partir da analise anterior do sistema e das correcoes funcionais definidas depois. Este arquivo nao propoe implementar nada agora; ele organiza como a refatoracao futura deve acontecer, quais comportamentos devem ser preservados, quais pontos devem ser reorganizados no codigo e qual estrategia de testes deve bloquear a execucao de cada etapa.

## 1. Objetivo do documento

O objetivo deste planejamento e orientar uma refatoracao estrutural do sistema sem perder regras de negocio validas, sem simplificacoes perigosas e sem regressao funcional.

Este plano assume como premissas:

- o sistema e um backoffice multiusuario e multiloja para loja de brecho
- o usuario e uma entidade global, independente de loja
- o vinculo do usuario com a loja e o que define cargos e acesso
- os testes sao obrigatorios e devem cobrir todos os casos de uso documentados
- portal e mobile continuam no escopo, mas como implementacao futura

Resultado esperado da refatoracao:

- codigo mais simples de manter
- regras de negocio mais explicitas e rastreaveis
- separacao mais clara entre identidade global, vinculo por loja e pessoa
- menos acoplamento entre modulos
- cobertura de testes completa por caso de uso
- base pronta para evoluir sem quebrar fluxos centrais

## 2. Premissas funcionais consolidadas

As premissas abaixo devem ser tratadas como verdade funcional para a refatoracao.

### 2.1 Regras de negocio ja decididas

- Nao ha hoje portal web externo nem app mobile implementados nesta base, mas ambos continuam no escopo futuro.
- Os cadastros auxiliares sao da loja. Nao existe necessidade de voltar para um catalogo compartilhado.
- A antiga ideia de configuracao operacional da loja foi removida e nao deve voltar como prioridade.
- Usuarios recem-cadastrados ficam ativos de imediato.
- O usuario e global e pode se relacionar com varias lojas em papeis diferentes.
- Apenas o proprio usuario pode editar seu perfil global.
- O que terceiros podem administrar e o vinculo do usuario com a loja, incluindo cargos e status do vinculo.
- Se um usuario ja estiver associado a uma pessoa em alguma loja, os dados dessa pessoa devem poder ser recuperados para reutilizacao em outra loja.

### 2.2 Consignacao e desconto automatico

- O desconto automatico de consignacao nao deve alterar o preco base persistido do produto.
- O desconto automatico deve ser calculado quando a peca for selecionada para venda.
- O desconto automatico tambem deve aparecer na pagina da peca/produto como informacao operacional, sem gravar preco alterado no banco.
- O encerramento de consignacao deve ser manual.
- O fluxo de encerramento deve puxar os produtos que passaram do prazo.
- O usuario deve poder aumentar prazo ou retirar itens da lista de encerramento antes de confirmar.

### 2.3 Credito da loja

- O fornecedor/cliente com credito na loja pode usar o saldo comprando na loja.
- O fornecedor/cliente tambem pode sacar esse valor.
- Tanto no uso em compra quanto no saque deve ser respeitada a regra da porcentagem que o fornecedor recebe.
- Essa regra precisa ficar centralizada e reutilizavel, nao duplicada em fluxos paralelos.

### 2.4 Encadeamento automatico de eventos

- Venda e cancelamento devem continuar gerando efeitos transacionais automaticos.
- Se outros eventos relacionados precisarem ser encadeados automaticamente, eles devem ser catalogados e tratados de forma explicita.

### 2.5 Formatos de exportacao

- O fato de o sistema chamar exportacoes de PDF/Excel enquanto entrega HTML/CSV nao e problema prioritario agora.
- Portanto, isso nao deve liderar a refatoracao atual.
- O nome e a semantica podem ser revisitados depois, sem entrar como trava desta etapa.

## 3. Principios obrigatorios da refatoracao

### 3.1 Principios de execucao

- Nao refatorar para "embelezar" o codigo; refatorar para reduzir risco, duplicacao, ambiguidade e acoplamento.
- Nao mover regra de negocio valida para a interface.
- Nao depender do frontend para garantir consistencia de regra.
- Nao alterar comportamento funcional decidido sem registrar antes o impacto e sem validar com negocio.
- Nao misturar refatoracao estrutural com mudanca de escopo.

### 3.2 Principios de arquitetura

- Identidade global de usuario deve ficar separada do contexto de loja.
- Permissao deve ser tratada como capacidade da acao e nao como atalho informal de menu.
- Regra de negocio deve ficar centralizada em servicos/calculadores reutilizaveis.
- Fluxos transacionais devem continuar fechando no backend.
- Fluxos derivados devem ser orientados por eventos/efeitos claramente catalogados, mesmo que a implementacao inicial ainda seja sincronizada e transacional.

### 3.3 Principios de testes

- Nenhuma refatoracao relevante deve entrar sem testes do caso de uso afetado.
- Todo caso de uso deve ter cobertura documentada.
- Teste unitario sozinho nao basta.
- Fluxos criticos devem ter cobertura em tres niveis: unitario, integracao e end-to-end.
- A documentacao de testes deve mapear regra de negocio, endpoint/servico e fluxo de interface.

## 4. Diagnostico tecnico priorizado para refatoracao

### 4.1 Backend

Pontos que mais pedem refatoracao:

- servicos grandes demais, com mistura de leitura, escrita, calculo, autorizacao, mapeamento e exportacao
- verificacoes de contexto e permissao repetidas em muitos modulos
- regras transacionais importantes espalhadas sem catalogo unificado de efeitos colaterais
- tratamento atual de consignacao desalinhado com a regra de negocio consolidada
- ausencia de mecanismo planejado para reaproveitar pessoa vinculada a usuario ja existente em outra loja
- ausencia de suite proprietaria de testes automatizados

### 4.2 Frontend

Pontos que mais pedem refatoracao:

- excesso de modulos de primeiro nivel
- rotas com nomenclatura pouco aderente ao dominio
- telas muito densas, especialmente em pessoas, pecas e vendas
- regras de acesso centralizadas, mas apoiadas em permissoes ainda incompletas
- mistura de modulo principal com subfluxos que poderiam ser contextuais

### 4.3 Arquitetura transversal

Problemas de desenho que precisam ser tratados como backlog tecnico real:

- identidade global de usuario e relacao por loja ainda precisam ficar mais explicitas como conceitos distintos
- calculos de desconto, credito e repasse precisam ficar mais previsiveis e compartilhados
- falta uma camada clara para orquestrar efeitos automaticos entre modulos
- falta uma matriz de testes rastreavel por caso de uso
- falta um fluxo formal de CI para travar regressao

## 5. Alvos de refatoracao no backend

### 5.1 Acesso, identidade global e vinculo por loja

Objetivo:

- deixar explicito no codigo que `Usuario` e global e `UsuarioLoja` representa o contexto operacional por loja

Direcao de refatoracao:

- separar com mais clareza servicos de perfil proprio de servicos administrativos de vinculo
- manter a regra de que apenas o proprio usuario altera seu perfil global
- concentrar administracao de cargos, vinculos e acessos da loja em fluxos especificos
- revisar nomenclatura de endpoints, contratos e services para refletir essa divisao

O que preservar:

- autoedicao do perfil global
- gestao administrativa de cargos e vinculos por loja
- visibilidade global de usuarios para fins de relacionamento multiloja

O que refatorar:

- servicos de usuario, cargo e vinculo em responsabilidades menores
- padrao unico de autorizacao nas rotas e nos servicos
- revalidacao ou atualizacao de contexto de sessao quando cargos/vinculos mudarem

### 5.2 Pessoas e relacionamento com usuarios

Objetivo:

- reorganizar o dominio `Pessoa` para contemplar reutilizacao entre lojas sem perder a separacao entre cadastro mestre e relacao da loja

Direcao de refatoracao:

- criar mecanismo claro de recuperacao de pessoa vinculada a usuario ja associado em outra loja
- garantir que a criacao em nova loja possa reaproveitar dados mestres em vez de duplicar cadastro desnecessariamente
- manter `Pessoa` como entidade de negocio distinta de `Usuario`, mas permitir busca e reaproveitamento consistente

O que preservar:

- pessoa mestre
- relacao operacional por loja
- possibilidade de ser cliente, fornecedor ou ambos

O que refatorar:

- busca e vinculacao cross-store
- contratos de tela e backend para recuperar dados existentes
- testes para evitar duplicidade e conflito de identidade

### 5.3 Cadastros auxiliares da loja

Objetivo:

- consolidar no codigo a decisao de negocio de que os cadastros auxiliares pertencem a cada loja

Direcao de refatoracao:

- revisar nomenclatura para reduzir ambiguidade com a ideia antiga de catalogo compartilhado
- manter o modulo como configuracao operacional da loja, nao como master catalog global
- garantir que regras, services e permissoes reflitam esse modelo

### 5.4 Pecas, consignacao e precificacao dinamica

Objetivo:

- separar preco base persistido de preco efetivo calculado para venda

Direcao de refatoracao:

- criar calculo de desconto automatico de consignacao como regra derivada e nao como mutacao de estado da peca
- disponibilizar esse calculo em pelo menos dois contextos:
- consulta operacional da peca
- selecao da peca na venda
- redesenhar o fluxo de encerramento manual de consignacao para trabalhar com lista de itens vencidos, extensao de prazo e exclusao manual da lista de encerramento

O que preservar:

- prazo da consignacao
- encerramento manual
- visibilidade de itens vencidos

O que refatorar:

- remover dependencias de alteracao automatica persistida de preco
- concentrar calculo de desconto em componente/calculador reutilizavel
- alinhar backend e frontend com o mesmo conceito

### 5.5 Vendas, credito, saque e repasse

Objetivo:

- unificar regras financeiras derivadas da venda e do credito

Direcao de refatoracao:

- extrair calculadores de composicao financeira da venda
- centralizar a regra da porcentagem do fornecedor para:
- uso do credito em compra
- saque do credito
- repasse financeiro
- manter criacao e cancelamento de venda como operacoes transacionais
- catalogar explicitamente todos os efeitos encadeados

O que preservar:

- venda com pagamentos mistos
- uso de credito
- cancelamento com reversao
- obrigacoes do fornecedor

O que refatorar:

- calculo compartilhado de percentuais e repasses
- servicos de venda e credito em unidades menores
- contratos de efeitos automaticos

### 5.6 Fechamentos, relatorios e documentos

Objetivo:

- manter os fluxos atuais, mas reduzir mistura de responsabilidade no codigo

Direcao de refatoracao:

- separar leitura, consolidacao, exportacao e montagem de documentos
- nao priorizar agora mudanca de HTML/CSV para PDF/XLSX reais
- manter foco em previsibilidade, legibilidade e testabilidade

### 5.7 Infraestrutura transversal

Objetivo:

- reduzir duplicacao sistemica

Direcao de refatoracao:

- criar padrao reutilizavel para contexto autenticado e escopo de loja
- padronizar policy de autorizacao por endpoint e validacao de escopo dentro do servico
- centralizar catalogo de efeitos/automacoes do dominio
- preparar base para CI e suite de testes

## 6. Alvos de refatoracao no frontend

### 6.1 Navegacao e arquitetura de telas

Objetivo:

- reorganizar o frontend por dominio de negocio, nao por subfuncao tecnica

Direcao de refatoracao:

- transformar `indicators` na entrada principal do sistema
- renomear `/dashboard` para refletir o modulo real de acesso
- aproximar subfluxos dos modulos de origem
- reduzir o numero de entradas de primeiro nivel

### 6.2 Pessoas

Objetivo:

- quebrar a tela atual em responsabilidades menores

Direcao de refatoracao:

- separar dados mestres, relacao por loja, contas bancarias, resumo financeiro e vinculacao de usuario
- incluir fluxo para recuperar pessoa ja associada ao usuario em outra loja
- melhorar previsibilidade do formulario e da leitura

### 6.3 Pecas, consignacao e venda

Objetivo:

- alinhar a interface com a regra correta de consignacao

Direcao de refatoracao:

- mostrar preco base e desconto calculado sem sobrescrever dado persistido
- refletir desconto efetivo no momento da selecao para venda
- criar tela/aba clara para encerramento manual de vencidos
- aproximar consignacao e movimentacoes do dominio de estoque

### 6.4 Credito e financeiro

Objetivo:

- deixar claro ao usuario quando se trata de compra com credito, saque, repasse ou lancamento financeiro

Direcao de refatoracao:

- explicitar regra de percentual aplicada ao fornecedor
- diferenciar visualmente movimentos de credito, saque e repasse
- aproximar repasses do dominio financeiro

### 6.5 Controle de acesso

Objetivo:

- alinhar guardas visuais a uma matriz de permissao mais fiel

Direcao de refatoracao:

- ajustar helpers de acesso para refletir leitura e acao de forma distinta
- manter bloqueio no backend como fonte de verdade
- reduzir modulos liberados por permissao indireta

## 7. Eventos e automacoes que devem ser encadeados automaticamente

Os efeitos abaixo devem ser tratados de forma explicita durante a refatoracao. Mesmo que a implementacao continue sincrona no inicio, o codigo deve deixar claro quais sao os efeitos obrigatorios de cada acao.

| Acao de origem | Efeitos que devem acontecer automaticamente | Observacoes de refatoracao |
| --- | --- | --- |
| Cadastro publico de usuario | criar usuario ativo; registrar evento de acesso; permitir estado autenticado sem loja | Nao introduzir aprovacao manual sem nova decisao de negocio |
| Alteracao de vinculo/cargo do usuario na loja | refletir novo escopo de acesso; atualizar tela de acesso e contexto quando necessario | Tratar como alteracao de relacao da loja, nao do perfil global |
| Criacao de pessoa em nova loja reaproveitando usuario existente | recuperar dados mestres ja existentes; criar apenas a relacao da nova loja quando aplicavel | Evitar duplicacao desnecessaria de pessoa |
| Cadastro de peca | registrar estoque inicial; disponibilizar peca nos fluxos operacionais corretos; refletir em consultas e dashboards | Manter auditoria e historico |
| Exibicao de peca/selecionar peca para venda em consignacao | calcular desconto automatico derivado; mostrar preco efetivo sem gravar mutacao no preco base | Regra calculada e nao persistida |
| Venda criada | baixar estoque; registrar financeiro; debitar credito quando usado; gerar obrigacao do fornecedor; disponibilizar documentos; refletir em indicadores/relatorios | Catalogar tudo isso em um contrato de efeitos da venda |
| Venda cancelada | reverter estoque; reverter financeiro; reverter credito; reverter obrigacoes geradas; refletir status e documentos | Deve ser espelho da venda criada |
| Ajuste manual de estoque | registrar movimentacao; refletir consultas, indicadores e alertas aplicaveis | Evitar efeito lateral escondido |
| Encerramento manual de consignacao | fechar itens selecionados; atualizar status operacional; atualizar alertas/listas de vencidos | Fluxo deve suportar extensao de prazo e retirada da lista antes da confirmacao |
| Movimento de credito | atualizar saldo; registrar extrato; refletir elegibilidade de uso/saque | Regra percentual deve ser centralizada |
| Saque de credito | aplicar regra percentual; registrar financeiro e extrato; refletir saldo restante | Tratar saque como caso de uso proprio |
| Liquidacao de repasse ao fornecedor | registrar movimento financeiro ou de credito; atualizar obrigacao; refletir fechamentos e comprovantes | Mesma regra de percentual precisa ser reutilizada |
| Geracao/conferencia/liquidacao de fechamento | consolidar snapshot; atualizar status; disponibilizar exportacoes/documentos | Separar consolidacao de apresentacao |

Regra operacional adicional:

- Se outros eventos obrigatorios forem identificados durante a implementacao, eles devem entrar primeiro em um catalogo de efeitos de dominio antes de virar codigo de refatoracao.

## 8. Estrategia obrigatoria de testes documentados

### 8.1 Regra central

Testes nao sao etapa opcional nem etapa final. Neste sistema, testes devem ser tratados como parte do proprio trabalho de refatoracao.

Obrigacoes deste plano:

- todo caso de uso deve estar documentado
- todo caso de uso deve ter cobertura planejada
- todo fluxo critico deve ter teste unitario, integracao e end-to-end
- toda refatoracao deve informar exatamente quais casos de uso foram tocados
- nenhuma etapa relevante deve ser dada como concluida sem atualizar a matriz de rastreabilidade

### 8.2 Matriz minima de cobertura por caso de uso

| Caso de uso | Unitario | Integracao | E2E | Observacoes obrigatorias |
| --- | --- | --- | --- | --- |
| Cadastro publico de usuario | sim | sim | sim | usuario entra ativo de imediato |
| Login, logout e contexto `me` | sim | sim | sim | incluir sessao valida e invalida |
| Troca de loja ativa | sim | sim | sim | usuario com multiplas lojas |
| Recuperacao e alteracao de senha | sim | sim | sim | cobrir token, expiracao e troca de senha do perfil |
| Edicao do proprio perfil | sim | sim | sim | garantir que terceiros nao alterem perfil global |
| Listagem global de usuarios para vinculacao | sim | sim | opcional | validar modelo de identidade global |
| CRUD de cargos | sim | sim | sim | incluir atribuicao de permissoes |
| CRUD de vinculos usuario-loja | sim | sim | sim | incluir troca de cargo, status e reflexo de acesso |
| CRUD de lojas | sim | sim | sim | cobrir leitura e manutencao conforme perfil |
| Criacao de pessoa nova | sim | sim | sim | cliente, fornecedor e ambos |
| Reaproveitamento de pessoa/usuario em outra loja | sim | sim | sim | caso novo exigido pelas correcoes |
| Vinculacao de usuario a pessoa | sim | sim | sim | cobrir pessoa ja vinculada em outra loja |
| Cadastros auxiliares da loja | sim | sim | sim | tratar como dados da loja, nao catalogo global |
| Regras comerciais da loja | sim | sim | sim | loja e fornecedor |
| Cadastro de peca | sim | sim | sim | incluir historico e disponibilidade |
| Edicao de peca | sim | sim | sim | sem quebrar historico e integridade |
| Imagens de peca | sim | sim | sim | upload, troca e exclusao |
| Ajuste manual de estoque | sim | sim | sim | refletir movimentacao |
| Consulta de movimentacoes | sim | sim | sim | filtros e rastreabilidade |
| Visualizacao operacional de consignacao | sim | sim | sim | itens vencidos e alertas |
| Desconto automatico de consignacao exibido sem persistir preco | sim | sim | sim | regra central da refatoracao |
| Encerramento manual de consignacao | sim | sim | sim | incluir extensao de prazo e retirada da lista |
| Criacao de venda com pagamento simples | sim | sim | sim | baixa de estoque e financeiro |
| Criacao de venda com pagamento misto | sim | sim | sim | totais, validacoes e reflexos |
| Criacao de venda com uso de credito | sim | sim | sim | debito do saldo e regra percentual |
| Cancelamento de venda | sim | sim | sim | reversao completa dos efeitos |
| Consulta de credito da loja | sim | sim | sim | por operador e consulta propria |
| Movimento manual de credito | sim | sim | sim | entrada, saida e saldo |
| Saque de credito | sim | sim | sim | obrigatorio cobrir regra percentual |
| Liquidacao de repasse via financeiro | sim | sim | sim | obrigacao e comprovante |
| Liquidacao de repasse via credito | sim | sim | sim | cruzamento entre modulos |
| Lancamento financeiro manual | sim | sim | sim | categorias, valores e reflexo |
| Conciliacao financeira | sim | sim | sim | incluir taxas e ajustes |
| Geracao de fechamento | sim | sim | sim | snapshot por periodo |
| Conferencia e liquidacao de fechamento | sim | sim | sim | bloquear liquidacao indevida |
| Execucao de relatorios | sim | sim | sim | filtros, tipos e dados vazios |
| Salvar/remover filtros de relatorio | sim | sim | sim | manter rastreabilidade por usuario |
| Documentos e impressoes | sim | sim | sim | etiquetas, recibos e comprovantes |
| Indicadores por perfil | sim | sim | sim | acesso correto por permissao |
| Controle de menus e guardas do frontend | sim | opcional | sim | esconder e bloquear coerentemente |

### 8.3 Estrutura documental dos testes

Para a refatoracao funcionar, a cobertura nao pode ficar apenas "na cabeca". O plano deve produzir e manter:

- inventario de casos de uso
- matriz de rastreabilidade caso de uso -> servico -> endpoint -> tela -> testes
- seeds e fixtures padrao para lojas, usuarios, pessoas, pecas, credito e vendas
- convencao unica para nomes de testes
- classificacao de testes criticos, essenciais e complementares

### 8.4 Politica de CI para a refatoracao

Pipeline minima recomendada:

- etapa 1: validacao estatica
- lint frontend
- build frontend
- build backend
- etapa 2: testes unitarios
- etapa 3: testes de integracao
- etapa 4: smoke E2E dos fluxos criticos

Politica recomendada:

- nenhum merge sem build verde
- nenhum merge de refatoracao sem os testes do caso de uso afetado
- fluxo critico quebrado bloqueia merge

## 9. Roadmap de refatoracao

### Fase 0 - Consolidacao funcional e catalogo de casos de uso

Objetivo:

- transformar regras de negocio decididas em contrato tecnico de refatoracao

Entregaveis:

- matriz definitiva de casos de uso
- catalogo de efeitos automaticos do dominio
- definicao formal de identidade global x vinculo por loja x pessoa

Prioridade:

- altissima

### Fase 1 - Base de testes e CI

Objetivo:

- preparar a rede de seguranca antes de tocar na estrutura

Entregaveis:

- esqueleto de testes unitarios
- esqueleto de integracao HTTP/backend
- esqueleto de E2E
- pipeline minima de CI
- primeiros testes dos fluxos de autenticacao, venda e perfil

Prioridade:

- altissima

### Fase 2 - Refatoracao de acesso e identidade

Objetivo:

- separar definitivamente perfil global de usuario do contexto da loja

Entregaveis:

- reorganizacao dos servicos de usuario, cargos e vinculos
- ajuste de nomenclatura e contratos
- testes completos de RBAC e autoedicao

Prioridade:

- alta

### Fase 3 - Refatoracao de pessoas e relacionamentos cross-store

Objetivo:

- suportar reaproveitamento de dados mestres sem duplicacao indevida

Entregaveis:

- fluxo de recuperacao de pessoa/usuario ja existente
- reorganizacao de servicos e endpoints de pessoas
- testes cobrindo criacao nova e reaproveitamento

Prioridade:

- alta

### Fase 4 - Refatoracao de pecas, consignacao e venda

Objetivo:

- alinhar o dominio de consignacao com a regra correta e proteger o fluxo de venda

Entregaveis:

- calculador de desconto automatico nao persistente
- fluxo manual de encerramento de vencidos
- refatoracao da orquestracao de venda e cancelamento
- testes completos do encadeamento automatico

Prioridade:

- altissima

### Fase 5 - Refatoracao de credito, saque, financeiro, repasses e fechamentos

Objetivo:

- unificar regras monetarias e percentuais

Entregaveis:

- calculador unico para percentual aplicavel
- reorganizacao de servicos de credito e repasse
- validacao dos reflexos automaticos em financeiro e fechamentos
- cobertura de testes completa desses fluxos

Prioridade:

- altissima

### Fase 6 - Refatoracao do frontend por dominio

Objetivo:

- simplificar navegacao e reduzir densidade de telas

Entregaveis:

- nova IA de navegacao
- renomeacao de rotas e modulos
- separacao das telas mais densas
- ajuste dos guardas de permissao conforme nova matriz

Prioridade:

- alta

### Fase 7 - Estabilizacao final

Objetivo:

- fechar lacunas, revisar documentacao e preparar novas frentes

Entregaveis:

- revisao final de testes
- revisao da documentacao funcional e tecnica
- backlog separado para portal e mobile

Prioridade:

- alta

## 10. Itens fora da refatoracao imediata

Itens que devem permanecer fora da prioridade atual, embora continuem registrados:

- portal web externo
- app mobile
- geracao real de PDF/XLSX
- retorno da configuracao operacional removida da loja

Esses itens nao devem confundir a refatoracao do core atual. O objetivo agora e estabilizar o sistema principal, deixar o dominio mais claro e criar cobertura de testes completa.

## 11. Criterios de aceite da futura refatoracao

A refatoracao so deve ser considerada bem-sucedida quando:

- os casos de uso centrais estiverem documentados
- a matriz de rastreabilidade estiver atualizada
- os testes cobrirem todos os casos de uso planejados
- os fluxos criticos de autenticacao, venda, consignacao, credito, repasse e fechamento estiverem verdes em tres niveis
- a separacao entre usuario global, vinculo por loja e pessoa estiver clara no codigo
- o desconto automatico de consignacao estiver implementado como calculo e nao como mutacao persistida de preco
- o reaproveitamento de pessoa/usuario em outra loja estiver coberto por regra, API, interface e testes

## 12. Resultado esperado

Ao final da execucao deste plano, o sistema deve estar:

- tecnicamente mais previsivel
- funcionalmente aderente as regras consolidadas
- protegido por testes em todos os casos de uso relevantes
- preparado para evoluir o core atual antes de abrir novas frentes como portal e mobile
