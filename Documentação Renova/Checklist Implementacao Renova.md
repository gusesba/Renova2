---
tags:
  - renova
  - checklist
  - implementacao
  - backlog
status: proposta
origem:
  - "[[Ideia Inicial]]"
  - "[[Requisitos por Modulos]]"
  - "[[Modelagem Banco de Dados Renova]]"
  - "[[Relacoes Funcoes App]]"
  - "[[comandosDB]]"
last_update: 2026-03-16
---

# Checklist de ImplementaÃ§Ã£o Renova

Documento mestre em formato todo list para orientar a implementaÃ§Ã£o completa do sistema. A Ã¡rvore abaixo consolida os requisitos funcionais, tÃ©cnicos e operacionais levantados na pasta `DocumentaÃ§Ã£o Renova` e expande cada item em passos de implementaÃ§Ã£o.

## 00. FundaÃ§Ã£o TÃ©cnica e Setup

- [x] Estruturar a soluÃ§Ã£o backend para suportar domÃ­nio, persistÃªncia, serviÃ§os e API
  - [x] revisar referÃªncias entre projetos
  - [x] padronizar namespaces, pastas e convenÃ§Ãµes de nomes
  - [x] definir estratÃ©gia para DTOs, handlers, services e validators
- [x] Configurar PostgreSQL local e ambientes
  - [x] manter connection string vazia em produÃ§Ã£o
  - [x] manter connection string de desenvolvimento em `appsettings.Development.json`
  - [x] validar leitura da connection string em runtime
- [x] Configurar EF Core para PostgreSQL
  - [x] manter `RenovaDbContext` registrado na API
  - [x] manter provider Npgsql configurado
  - [x] manter pacotes de design e migrations instalados
- [x] Preparar o fluxo de migrations e comandos operacionais
  - [x] manter manifesto local do `dotnet-ef`
  - [x] documentar comandos em `comandosDB.md`
  - [x] validar `database update` e `database drop`
- [x] Definir padrÃµes transversais do backend
  - [x] tratamento de erro padronizado
  - [x] respostas HTTP consistentes
  - [x] logging estruturado
  - [x] ProblemDetails
  - [x] versionamento de API

## 01. Banco de Dados e PersistÃªncia

- [x] Consolidar o modelo relacional no banco
  - [x] revisar entidades do domÃ­nio geradas
  - [x] revisar mapeamentos do `OnModelCreating`
  - [x] revisar Ã­ndices, chaves Ãºnicas e FKs
  - [x] revisar precisÃ£o monetÃ¡ria e concorrÃªncia

## 02. MÃ³dulo 01 - Acesso, UsuÃ¡rios e PermissÃµes

- [x] Implementar cadastro de usuÃ¡rio
  - [x] criar contrato de entrada e saÃ­da
  - [x] validar unicidade de e-mail
  - [x] gerar hash de senha
  - [x] persistir em `usuario`
  - [x] registrar auditoria
  - [x] criar tela web de cadastro e ediÃ§Ã£o
- [x] Implementar login
  - [x] validar credenciais
  - [x] registrar evento em `usuario_acesso_evento`
  - [x] retornar contexto inicial do usuÃ¡rio
  - [x] carregar lojas disponÃ­veis
  - [x] carregar permissÃµes do contexto
- [x] Implementar recuperaÃ§Ã£o de acesso
  - [x] definir fluxo de reset de senha
  - [x] registrar expiraÃ§Ã£o e uso do token
  - [x] registrar auditoria do processo
- [x] Implementar ativaÃ§Ã£o, inativaÃ§Ã£o e bloqueio de usuÃ¡rio
  - [x] atualizar `status_usuario`
  - [x] impedir login quando bloqueado/inativo
  - [x] registrar auditoria da mudanÃ§a
- [x] Implementar cargos por loja
  - [x] CRUD de `cargo`
  - [x] CRUD de `cargo_permissao`
  - [x] tela web para matriz de permissÃµes
- [x] Implementar vÃ­nculo usuÃ¡rio x loja
  - [x] CRUD de `usuario_loja`
  - [x] CRUD de `usuario_loja_cargo`
  - [x] definir loja ativa na sessÃ£o
- [x] Implementar autorizaÃ§Ã£o por funÃ§Ã£o
  - [x] middleware/policy para autenticaÃ§Ã£o
  - [x] verificaÃ§Ã£o de `usuario_loja`
  - [x] verificaÃ§Ã£o de `cargo_permissao`
  - [x] bloqueio de acesso por loja nÃ£o autorizada

## 03. MÃ³dulo 02 - Lojas e Estrutura Operacional

- [x] Implementar cadastro de loja
  - [x] CRUD de `loja`
  - [x] tela web de listagem e ediÃ§Ã£o
- [x] Implementar visÃ£o consolidada por usuÃ¡rio
  - [x] listar apenas lojas vinculadas
  - [x] permitir troca de loja ativa
  - [x] consolidar visÃ£o gerencial somente das lojas autorizadas

## 04. MÃ³dulo 03 - Cadastro de Clientes e Fornecedores

- [x] Implementar cadastro mestre de pessoa
  - [x] CRUD de `pessoa`
  - [x] validaÃ§Ã£o de documento
  - [x] inativaÃ§Ã£o lÃ³gica
  - [x] tela web de busca e ediÃ§Ã£o
- [x] Implementar vÃ­nculo pessoa x loja
  - [x] CRUD de `pessoa_loja`
  - [x] marcar cliente, fornecedor ou ambos
  - [x] definir polÃ­tica padrÃ£o de fim de consignaÃ§Ã£o
  - [x] exibir situaÃ§Ã£o por loja
- [x] Implementar contas bancÃ¡rias
  - [x] CRUD de `pessoa_conta_bancaria`
  - [x] suporte a PIX
  - [x] conta principal
- [x] Implementar vÃ­nculo pessoa x usuÃ¡rio
  - [x] permitir associar `usuario` a `pessoa`
  - [x] preparar acesso ao portal
- [x] Implementar visÃ£o financeira da pessoa
  - [x] consolidar saldo de crÃ©dito
  - [x] consolidar pendÃªncias
  - [x] consolidar histÃ³rico de transaÃ§Ãµes

## 05. MÃ³dulo 04 - Cadastros Auxiliares e Tabelas Base

- [x] Implementar cadastro de nomes de produto
  - [x] CRUD de `produto_nome`
  - [x] associaÃ§Ã£o direta Ã  `loja`
  - [x] manter somente `nome` como dado de negÃ³cio
- [x] Implementar cadastro de marcas
  - [x] CRUD de `marca`
  - [x] associaÃ§Ã£o direta Ã  `loja`
  - [x] manter somente `nome` como dado de negÃ³cio
- [x] Implementar cadastro de tamanhos
  - [x] CRUD de `tamanho`
  - [x] manter somente `nome` como dado de negÃ³cio
- [x] Implementar cadastro de cores
  - [x] CRUD de `cor`
  - [x] manter somente `nome` como dado de negÃ³cio
- [x] Garantir segregaÃ§Ã£o dos cadastros auxiliares por loja
  - [x] vincular registros diretamente Ã  `loja`
  - [x] impedir compartilhamento automÃ¡tico entre lojas

## 06. MÃ³dulo 05 - ConfiguraÃ§Ãµes Comerciais e Regras de NegÃ³cio

- [x] Implementar regra comercial padrÃ£o da loja
  - [x] CRUD de `loja_regra_comercial`
  - [x] percentuais para dinheiro
  - [x] percentuais para crÃ©dito
  - [x] pagamento misto
  - [x] prazo mÃ¡ximo de exposiÃ§Ã£o
  - [x] polÃ­tica de desconto em JSON ou estrutura equivalente
- [x] Implementar regra comercial por fornecedor
  - [x] CRUD de `fornecedor_regra_comercial`
  - [x] vÃ­nculo com `pessoa_loja`
  - [x] sobrescrita da regra padrÃ£o
- [x] Implementar configuraÃ§Ã£o de meios de pagamento
  - [x] CRUD de `meio_pagamento`
  - [x] taxa
  - [x] prazo de recebimento
  - [x] status ativo/inativo
- [x] Implementar serviÃ§o de resoluÃ§Ã£o da regra efetiva
  - [x] priorizar regra manual da peÃ§a
  - [x] depois regra do fornecedor
  - [x] depois regra da loja

## 07. MÃ³dulo 06 - Cadastro de PeÃ§as e Estoque

- [x] Implementar cadastro de peÃ§a
  - [x] contrato de entrada
  - [x] validaÃ§Ã£o de loja, fornecedor e tabelas auxiliares
  - [x] geraÃ§Ã£o de cÃ³digo interno
  - [x] suporte a cÃ³digo de barras
  - [x] gravaÃ§Ã£o em `peca`
- [x] Implementar snapshot da condiÃ§Ã£o comercial da peÃ§a
  - [x] resolver regra efetiva no momento da entrada
  - [x] gravar em `peca_condicao_comercial`
- [x] Implementar upload e vÃ­nculo de imagens
  - [x] armazenamento fÃ­sico/lÃ³gico do arquivo
  - [x] gravaÃ§Ã£o em `peca_imagem`
  - [x] visibilidade interna/externa
- [x] Implementar entrada inicial de estoque
  - [x] gravar `movimentacao_estoque` do tipo entrada
  - [x] atualizar saldo inicial
- [x] Implementar telas web de cadastro, ediÃ§Ã£o e consulta de peÃ§a
  - [x] filtros rÃ¡pidos
  - [x] busca por cÃ³digo de barras
  - [x] exibiÃ§Ã£o de status e localizaÃ§Ã£o

## 08. MÃ³dulo 07 - Ciclo de Vida da ConsignaÃ§Ã£o

- [x] Implementar cÃ¡lculo de prazo de consignaÃ§Ã£o
  - [x] usar `peca_condicao_comercial`
  - [x] calcular data de inÃ­cio e fim
  - [x] exibir dias restantes
- [x] Implementar desconto por tempo de loja
  - [x] aplicar polÃ­tica configurada
  - [x] atualizar preÃ§o da peÃ§a quando necessÃ¡rio
  - [x] registrar histÃ³rico em `peca_historico_preco`
- [x] Implementar devoluÃ§Ã£o ao fornecedor
  - [x] alterar status da peÃ§a
  - [x] registrar `movimentacao_estoque`
  - [x] gerar comprovante
  - [x] auditar aÃ§Ã£o
- [x] Implementar doaÃ§Ã£o da peÃ§a
  - [x] alterar status da peÃ§a
  - [x] registrar `movimentacao_estoque`
  - [x] gerar comprovante
  - [x] auditar aÃ§Ã£o
- [x] Implementar perda e descarte
  - [x] alterar status
  - [x] registrar motivo e responsÃ¡vel
  - [x] auditar aÃ§Ã£o

## 09. MÃ³dulo 08 - MovimentaÃ§Ãµes de Estoque

- [x] Implementar listagem completa de movimentaÃ§Ãµes
  - [x] consulta por peÃ§a
  - [x] consulta por loja
  - [x] consulta por perÃ­odo
- [x] Implementar ajustes manuais
  - [x] permissÃµes especÃ­ficas
  - [x] atualizaÃ§Ã£o de saldo
  - [x] gravaÃ§Ã£o em `movimentacao_estoque`
- [x] Impedir venda sem saldo
  - [x] validar `quantidade_atual`
  - [x] bloquear transaÃ§Ã£o antes da conclusÃ£o
- [x] Implementar busca operacional de peÃ§as
  - [x] por cÃ³digo de barras
  - [x] por nome, marca, fornecedor e status
  - [x] por tempo em loja

## 10. MÃ³dulo 09 - Vendas

- [x] Implementar abertura e conclusÃ£o de venda
  - [x] criar `venda`
  - [x] adicionar `venda_item`
  - [x] validar disponibilidade das peÃ§as
  - [x] calcular subtotal, desconto, taxa e total lÃ­quido
- [x] Implementar composiÃ§Ã£o de pagamento
  - [x] registrar `venda_pagamento`
  - [x] permitir mÃºltiplos meios de pagamento
  - [x] permitir pagamento misto com crÃ©dito
- [x] Implementar atualizaÃ§Ã£o transacional pÃ³s-venda
  - [x] baixar estoque
  - [x] alterar status da peÃ§a
  - [x] gerar obrigaÃ§Ã£o do fornecedor quando aplicÃ¡vel
  - [x] gerar movimentaÃ§Ã£o financeira
  - [x] gerar movimentaÃ§Ã£o de crÃ©dito quando houver uso de crÃ©dito
- [x] Implementar cancelamento de venda
  - [x] mudar `status_venda`
  - [x] estornar estoque
  - [x] estornar financeiro
  - [x] estornar crÃ©dito quando necessÃ¡rio
  - [x] registrar auditoria
- [x] Implementar emissÃ£o de recibo
  - [x] modelo Ãºnico
  - [x] dados de venda e pagamento
## 11. MÃ³dulo 10 - CrÃ©dito da Loja

- [x] Implementar conta de crÃ©dito por loja e pessoa
  - [x] criar e manter `conta_credito_loja`
  - [x] garantir unicidade por loja + pessoa
- [x] Implementar livro razÃ£o do crÃ©dito
  - [x] registrar em `movimentacao_credito_loja`
  - [x] manter saldo anterior e posterior
- [x] Implementar crÃ©dito manual
  - [x] exigir justificativa
  - [x] exigir responsÃ¡vel
  - [x] auditar aÃ§Ã£o
- [x] Implementar crÃ©dito por repasse
  - [x] gerar crÃ©dito durante pagamento ao fornecedor quando aplicÃ¡vel
- [x] Implementar uso de crÃ©dito em compra
  - [x] validar saldo
  - [x] registrar dÃ©bito
  - [x] relacionar Ã  venda
- [x] Implementar consultas de extrato e saldo
  - [x] frontend web
  - [x] portal
  - [x] mobile consulta

## 12. MÃ³dulo 11 - Pagamentos e Repasses

- [x] Implementar geraÃ§Ã£o de obrigaÃ§Ã£o do fornecedor
  - [x] criar `obrigacao_fornecedor` para peÃ§a consignada vendida
  - [x] criar `obrigacao_fornecedor` para peÃ§a fixa/lote comprada
  - [x] definir tipo, saldo em aberto e status
- [x] Implementar liquidaÃ§Ã£o da obrigaÃ§Ã£o
  - [x] registrar `liquidacao_obrigacao_fornecedor`
  - [x] permitir dinheiro, crÃ©dito ou misto
  - [x] atualizar saldo em aberto
  - [x] atualizar status
- [x] Implementar comprovante de pagamento ao fornecedor
  - [x] modelo Ãºnico
  - [x] dados da liquidaÃ§Ã£o
- [x] Implementar listagem de pendÃªncias
  - [x] por fornecedor
  - [x] por loja
  - [x] por status

## 13. MÃ³dulo 12 - Meios de Pagamento e ConciliaÃ§Ã£o Financeira

- [x] Implementar livro razÃ£o financeiro
  - [x] registrar entradas e saÃ­das em `movimentacao_financeira`
  - [x] relacionar com `venda_pagamento` quando houver
  - [x] relacionar com `liquidacao_obrigacao_fornecedor` quando houver
- [x] Implementar lanÃ§amentos financeiros avulsos
  - [x] despesas
  - [x] receitas avulsas
  - [x] ajustes
  - [x] estornos
- [x] Implementar conciliaÃ§Ã£o financeira
  - [x] consolidar por perÃ­odo
  - [x] consolidar por meio de pagamento
  - [x] evidenciar taxas e valores lÃ­quidos
- [x] Implementar resumo diÃ¡rio financeiro
  - [x] total de entradas
  - [x] total de saÃ­das
  - [x] saldo bruto
  - [x] saldo lÃ­quido

## 14. MÃ³dulo 13 - Fechamento do Cliente/Fornecedor

- [x] Implementar geraÃ§Ã£o de fechamento
  - [x] consolidar peÃ§as atuais
  - [x] consolidar peÃ§as vendidas
  - [x] consolidar valores vendidos e a receber
  - [x] consolidar compras feitas na loja
  - [x] consolidar pagamentos e saldo final
  - [x] gravar `fechamento_pessoa`
  - [x] gravar `fechamento_pessoa_item`
  - [x] gravar `fechamento_pessoa_movimento`
- [x] Implementar conferÃªncia e liquidaÃ§Ã£o do fechamento
  - [x] permitir marcar como conferido
  - [x] permitir marcar como liquidado
  - [x] impedir alteraÃ§Ã£o indevida apÃ³s liquidaÃ§Ã£o
- [x] Implementar exportaÃ§Ã£o do fechamento
  - [x] PDF
  - [x] Excel
  - [x] texto formatado para WhatsApp
- [x] Implementar histÃ³rico de fechamentos
  - [x] filtro por pessoa
  - [x] filtro por loja
  - [x] filtro por perÃ­odo

## 15. MÃ³dulo 14 - Dashboards e Indicadores

- [x] Implementar dashboard de vendas
  - [x] por dia
  - [x] por mÃªs
  - [x] por loja
  - [x] por vendedor
  - [x] por perÃ­odo
- [x] Implementar dashboard financeiro
  - [x] entradas
  - [x] saÃ­das
  - [x] saldo bruto
  - [x] saldo lÃ­quido
- [x] Implementar dashboard de consignaÃ§Ã£o
  - [x] peÃ§as prÃ³ximas do vencimento
  - [x] peÃ§as paradas em estoque
- [x] Implementar dashboard de pendÃªncias
  - [x] valores a pagar
  - [x] valores pendentes de recebimento
  - [x] inconsistÃªncias operacionais

## 16. MÃ³dulo 15 - RelatÃ³rios e ExportaÃ§Ãµes

- [x] Implementar exportaÃ§Ã£o genÃ©rica com filtros
  - [x] Excel
  - [x] PDF
- [x] Implementar relatÃ³rio de estoque atual
  - [x] filtros por loja, status, marca e fornecedor
- [x] Implementar relatÃ³rio de peÃ§as vendidas
  - [x] filtros por perÃ­odo, fornecedor e vendedor
- [x] Implementar relatÃ³rio financeiro
  - [x] por cliente/fornecedor
  - [x] por perÃ­odo e loja
- [x] Implementar relatÃ³rio de peÃ§as devolvidas, doadas, perdidas e descartadas
  - [x] filtros por perÃ­odo e motivo
- [x] Implementar filtros salvos
  - [x] persistir configuraÃ§Ã£o de filtro frequente

## 17. MÃ³dulo 16 - ImpressÃµes e Documentos

- [x] Implementar impressÃ£o de etiqueta
  - [x] gerar layout Ãºnico
  - [x] incluir cÃ³digo de barras
  - [x] integrar com dados da peÃ§a
- [x] Implementar impressÃ£o de recibo de venda
  - [x] layout Ãºnico
  - [x] dados de loja, venda e pagamento
- [x] Implementar comprovantes de pagamento ao fornecedor
  - [x] layout Ãºnico
  - [x] dados da liquidaÃ§Ã£o
- [x] Implementar comprovantes de devoluÃ§Ã£o/doaÃ§Ã£o
  - [x] dados da peÃ§a
  - [x] responsÃ¡vel
  - [x] data e motivo

## 18. MÃ³dulo 17 - Alertas 

## 19. PendÃªncias de DefiniÃ§Ã£o Antes da ImplementaÃ§Ã£o Completa

- [ ] Definir polÃ­tica de troca e devoluÃ§Ã£o para compradores finais
  - [ ] identificar impacto em venda
  - [ ] identificar impacto em estoque
  - [ ] identificar impacto em financeiro
- [ ] Avaliar necessidade futura de emissÃ£o fiscal
  - [ ] identificar impacto em venda
  - [ ] identificar impacto em documentos
  - [ ] identificar impacto em integraÃ§Ãµes externas
