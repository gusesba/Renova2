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

# Checklist de Implementação Renova

Documento mestre em formato todo list para orientar a implementação completa do sistema. A árvore abaixo consolida os requisitos funcionais, técnicos e operacionais levantados na pasta `Documentação Renova` e expande cada item em passos de implementação.

## 00. Fundação Técnica e Setup

- [x] Estruturar a solução backend para suportar domínio, persistência, serviços e API
  - [x] revisar referências entre projetos
  - [x] padronizar namespaces, pastas e convenções de nomes
  - [x] definir estratégia para DTOs, handlers, services e validators
- [x] Configurar PostgreSQL local e ambientes
  - [x] manter connection string vazia em produção
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
- [x] Definir padrões transversais do backend
  - [x] tratamento de erro padronizado
  - [x] respostas HTTP consistentes
  - [x] logging estruturado
  - [x] ProblemDetails
  - [x] versionamento de API

## 01. Banco de Dados e Persistência

- [x] Consolidar o modelo relacional no banco
  - [x] revisar entidades do domínio geradas
  - [x] revisar mapeamentos do `OnModelCreating`
  - [x] revisar índices, chaves únicas e FKs
  - [x] revisar precisão monetária e concorrência

## 02. Módulo 01 - Acesso, Usuários e Permissões

- [x] Implementar cadastro de usuário
  - [x] criar contrato de entrada e saída
  - [x] validar unicidade de e-mail
  - [x] gerar hash de senha
  - [x] persistir em `usuario`
  - [x] registrar auditoria
  - [x] criar tela web de cadastro e edição
- [x] Implementar login
  - [x] validar credenciais
  - [x] registrar evento em `usuario_acesso_evento`
  - [x] retornar contexto inicial do usuário
  - [x] carregar lojas disponíveis
  - [x] carregar permissões do contexto
- [x] Implementar recuperação de acesso
  - [x] definir fluxo de reset de senha
  - [x] registrar expiração e uso do token
  - [x] registrar auditoria do processo
- [x] Implementar ativação, inativação e bloqueio de usuário
  - [x] atualizar `status_usuario`
  - [x] impedir login quando bloqueado/inativo
  - [x] registrar auditoria da mudança
- [x] Implementar cargos por loja
  - [x] CRUD de `cargo`
  - [x] CRUD de `cargo_permissao`
  - [x] tela web para matriz de permissões
- [x] Implementar vínculo usuário x loja
  - [x] CRUD de `usuario_loja`
  - [x] CRUD de `usuario_loja_cargo`
  - [x] definir loja ativa na sessão
- [x] Implementar autorização por função
  - [x] middleware/policy para autenticação
  - [x] verificação de `usuario_loja`
  - [x] verificação de `cargo_permissao`
  - [x] bloqueio de acesso por loja não autorizada

## 03. Módulo 02 - Lojas e Estrutura Operacional

- [x] Implementar cadastro de loja
  - [x] CRUD de `loja`
  - [x] tela web de listagem e edição
- [x] Implementar visão consolidada por usuário
  - [x] listar apenas lojas vinculadas
  - [x] permitir troca de loja ativa
  - [x] consolidar visão gerencial somente das lojas autorizadas

## 04. Módulo 03 - Cadastro de Clientes e Fornecedores

- [x] Implementar cadastro mestre de pessoa
  - [x] CRUD de `pessoa`
  - [x] validação de documento
  - [x] inativação lógica
  - [x] tela web de busca e edição
- [x] Implementar vínculo pessoa x loja
  - [x] CRUD de `pessoa_loja`
  - [x] marcar cliente, fornecedor ou ambos
  - [x] definir política padrão de fim de consignação
  - [x] exibir situação por loja
- [x] Implementar contas bancárias
  - [x] CRUD de `pessoa_conta_bancaria`
  - [x] suporte a PIX
  - [x] conta principal
- [x] Implementar vínculo pessoa x usuário
  - [x] permitir associar `usuario` a `pessoa`
  - [x] preparar acesso ao portal
- [x] Implementar visão financeira da pessoa
  - [x] consolidar saldo de crédito
  - [x] consolidar pendências
  - [x] consolidar histórico de transações

## 05. Módulo 04 - Cadastros Auxiliares e Tabelas Base

- [x] Implementar cadastro de nomes de produto
  - [x] CRUD de `produto_nome`
  - [x] associação direta à `loja`
  - [x] manter somente `nome` como dado de negócio
- [x] Implementar cadastro de marcas
  - [x] CRUD de `marca`
  - [x] associação direta à `loja`
  - [x] manter somente `nome` como dado de negócio
- [x] Implementar cadastro de tamanhos
  - [x] CRUD de `tamanho`
  - [x] manter somente `nome` como dado de negócio
- [x] Implementar cadastro de cores
  - [x] CRUD de `cor`
  - [x] manter somente `nome` como dado de negócio
- [x] Garantir segregação dos cadastros auxiliares por loja
  - [x] vincular registros diretamente à `loja`
  - [x] impedir compartilhamento automático entre lojas

## 06. Módulo 05 - Configurações Comerciais e Regras de Negócio

- [x] Implementar regra comercial padrão da loja
  - [x] CRUD de `loja_regra_comercial`
  - [x] percentuais para dinheiro
  - [x] percentuais para crédito
  - [x] pagamento misto
  - [x] prazo máximo de exposição
  - [x] política de desconto em JSON ou estrutura equivalente
- [x] Implementar regra comercial por fornecedor
  - [x] CRUD de `fornecedor_regra_comercial`
  - [x] vínculo com `pessoa_loja`
  - [x] sobrescrita da regra padrão
- [x] Implementar configuração de meios de pagamento
  - [x] CRUD de `meio_pagamento`
  - [x] taxa
  - [x] prazo de recebimento
  - [x] status ativo/inativo
- [x] Implementar serviço de resolução da regra efetiva
  - [x] priorizar regra manual da peça
  - [x] depois regra do fornecedor
  - [x] depois regra da loja

## 07. Módulo 06 - Cadastro de Peças e Estoque

- [x] Implementar cadastro de peça
  - [x] contrato de entrada
  - [x] validação de loja, fornecedor e tabelas auxiliares
  - [x] geração de código interno
  - [x] suporte a código de barras
  - [x] gravação em `peca`
- [x] Implementar snapshot da condição comercial da peça
  - [x] resolver regra efetiva no momento da entrada
  - [x] gravar em `peca_condicao_comercial`
- [x] Implementar upload e vínculo de imagens
  - [x] armazenamento físico/lógico do arquivo
  - [x] gravação em `peca_imagem`
  - [x] visibilidade interna/externa
- [x] Implementar entrada inicial de estoque
  - [x] gravar `movimentacao_estoque` do tipo entrada
  - [x] atualizar saldo inicial
- [x] Implementar telas web de cadastro, edição e consulta de peça
  - [x] filtros rápidos
  - [x] busca por código de barras
  - [x] exibição de status e localização

## 08. Módulo 07 - Ciclo de Vida da Consignação

- [x] Implementar cálculo de prazo de consignação
  - [x] usar `peca_condicao_comercial`
  - [x] calcular data de início e fim
  - [x] exibir dias restantes
- [x] Implementar desconto por tempo de loja
  - [x] aplicar política configurada
  - [x] atualizar preço da peça quando necessário
  - [x] registrar histórico em `peca_historico_preco`
- [x] Implementar devolução ao fornecedor
  - [x] alterar status da peça
  - [x] registrar `movimentacao_estoque`
  - [x] gerar comprovante
  - [x] auditar ação
- [x] Implementar doação da peça
  - [x] alterar status da peça
  - [x] registrar `movimentacao_estoque`
  - [x] gerar comprovante
  - [x] auditar ação
- [x] Implementar perda e descarte
  - [x] alterar status
  - [x] registrar motivo e responsável
  - [x] auditar ação

## 09. Módulo 08 - Movimentações de Estoque

- [x] Implementar listagem completa de movimentações
  - [x] consulta por peça
  - [x] consulta por loja
  - [x] consulta por período
- [x] Implementar ajustes manuais
  - [x] permissões específicas
  - [x] atualização de saldo
  - [x] gravação em `movimentacao_estoque`
- [x] Impedir venda sem saldo
  - [x] validar `quantidade_atual`
  - [x] bloquear transação antes da conclusão
- [x] Implementar busca operacional de peças
  - [x] por código de barras
  - [x] por nome, marca, fornecedor e status
  - [x] por tempo em loja

## 10. Módulo 09 - Vendas

- [x] Implementar abertura e conclusão de venda
  - [x] criar `venda`
  - [x] adicionar `venda_item`
  - [x] validar disponibilidade das peças
  - [x] calcular subtotal, desconto, taxa e total líquido
- [x] Implementar composição de pagamento
  - [x] registrar `venda_pagamento`
  - [x] permitir múltiplos meios de pagamento
  - [x] permitir pagamento misto com crédito
- [x] Implementar atualização transacional pós-venda
  - [x] baixar estoque
  - [x] alterar status da peça
  - [x] gerar obrigação do fornecedor quando aplicável
  - [x] gerar movimentação financeira
  - [x] gerar movimentação de crédito quando houver uso de crédito
- [x] Implementar cancelamento de venda
  - [x] mudar `status_venda`
  - [x] estornar estoque
  - [x] estornar financeiro
  - [x] estornar crédito quando necessário
  - [x] registrar auditoria
- [x] Implementar emissão de recibo
  - [x] modelo único
  - [x] dados de venda e pagamento
## 11. Módulo 10 - Crédito da Loja

- [x] Implementar conta de crédito por loja e pessoa
  - [x] criar e manter `conta_credito_loja`
  - [x] garantir unicidade por loja + pessoa
- [x] Implementar livro razão do crédito
  - [x] registrar em `movimentacao_credito_loja`
  - [x] manter saldo anterior e posterior
- [x] Implementar crédito manual
  - [x] exigir justificativa
  - [x] exigir responsável
  - [x] auditar ação
- [x] Implementar crédito por repasse
  - [x] gerar crédito durante pagamento ao fornecedor quando aplicável
- [x] Implementar uso de crédito em compra
  - [x] validar saldo
  - [x] registrar débito
  - [x] relacionar à venda
- [x] Implementar consultas de extrato e saldo
  - [x] frontend web
  - [x] portal
  - [x] mobile consulta

## 12. Módulo 11 - Pagamentos e Repasses

- [x] Implementar geração de obrigação do fornecedor
  - [x] criar `obrigacao_fornecedor` para peça consignada vendida
  - [x] criar `obrigacao_fornecedor` para peça fixa/lote comprada
  - [x] definir tipo, saldo em aberto e status
- [x] Implementar liquidação da obrigação
  - [x] registrar `liquidacao_obrigacao_fornecedor`
  - [x] permitir dinheiro, crédito ou misto
  - [x] atualizar saldo em aberto
  - [x] atualizar status
- [x] Implementar comprovante de pagamento ao fornecedor
  - [x] modelo único
  - [x] dados da liquidação
- [x] Implementar listagem de pendências
  - [x] por fornecedor
  - [x] por loja
  - [x] por status

## 13. Módulo 12 - Meios de Pagamento e Conciliação Financeira

- [x] Implementar livro razão financeiro
  - [x] registrar entradas e saídas em `movimentacao_financeira`
  - [x] relacionar com `venda_pagamento` quando houver
  - [x] relacionar com `liquidacao_obrigacao_fornecedor` quando houver
- [x] Implementar lançamentos financeiros avulsos
  - [x] despesas
  - [x] receitas avulsas
  - [x] ajustes
  - [x] estornos
- [x] Implementar conciliação financeira
  - [x] consolidar por período
  - [x] consolidar por meio de pagamento
  - [x] evidenciar taxas e valores líquidos
- [x] Implementar resumo diário financeiro
  - [x] total de entradas
  - [x] total de saídas
  - [x] saldo bruto
  - [x] saldo líquido

## 14. Módulo 13 - Fechamento do Cliente/Fornecedor

- [x] Implementar geração de fechamento
  - [x] consolidar peças atuais
  - [x] consolidar peças vendidas
  - [x] consolidar valores vendidos e a receber
  - [x] consolidar compras feitas na loja
  - [x] consolidar pagamentos e saldo final
  - [x] gravar `fechamento_pessoa`
  - [x] gravar `fechamento_pessoa_item`
  - [x] gravar `fechamento_pessoa_movimento`
- [x] Implementar conferência e liquidação do fechamento
  - [x] permitir marcar como conferido
  - [x] permitir marcar como liquidado
  - [x] impedir alteração indevida após liquidação
- [x] Implementar exportação do fechamento
  - [x] PDF
  - [x] Excel
  - [x] texto formatado para WhatsApp
- [x] Implementar histórico de fechamentos
  - [x] filtro por pessoa
  - [x] filtro por loja
  - [x] filtro por período

## 15. Módulo 14 - Dashboards e Indicadores

- [ ] Implementar dashboard de vendas
  - [ ] por dia
  - [ ] por mês
  - [ ] por loja
  - [ ] por vendedor
  - [ ] por período
- [ ] Implementar dashboard financeiro
  - [ ] entradas
  - [ ] saídas
  - [x] saldo bruto
  - [x] saldo líquido
- [ ] Implementar dashboard de consignação
  - [ ] peças próximas do vencimento
  - [ ] peças paradas em estoque
- [ ] Implementar dashboard de pendências
  - [ ] valores a pagar
  - [ ] valores pendentes de recebimento
  - [ ] inconsistências operacionais

## 16. Módulo 15 - Relatórios e Exportações

- [ ] Implementar exportação genérica com filtros
  - [ ] Excel
  - [ ] PDF
- [ ] Implementar relatório de estoque atual
  - [ ] filtros por loja, status, marca e fornecedor
- [ ] Implementar relatório de peças vendidas
  - [ ] filtros por período, fornecedor e vendedor
- [ ] Implementar relatório financeiro
  - [ ] por cliente/fornecedor
  - [ ] por período e loja
- [ ] Implementar relatório de peças devolvidas, doadas, perdidas e descartadas
  - [ ] filtros por período e motivo
- [ ] Implementar filtros salvos
  - [ ] persistir configuração de filtro frequente

## 17. Módulo 16 - Impressões e Documentos

- [ ] Implementar impressão de etiqueta
  - [ ] gerar layout único
  - [ ] incluir código de barras
  - [ ] integrar com dados da peça
- [ ] Implementar impressão de recibo de venda
  - [ ] layout único
  - [ ] dados de loja, venda e pagamento
- [ ] Implementar comprovantes de pagamento ao fornecedor
  - [ ] layout único
  - [ ] dados da liquidação
- [ ] Implementar comprovantes de devolução/doação
  - [ ] dados da peça
  - [ ] responsável
  - [ ] data e motivo

## 18. Módulo 17 - Portal Web do Cliente/Fornecedor

- [ ] Implementar autenticação do portal
  - [ ] login de usuário vinculado a `pessoa`
  - [ ] restrição às próprias informações
- [ ] Implementar visão das lojas vinculadas
  - [ ] listar lojas de `pessoa_loja`
- [ ] Implementar consulta de peças
  - [ ] atuais
  - [ ] vendidas
  - [ ] valores relacionados
- [ ] Implementar consulta de saldo e pendências
  - [ ] crédito
  - [ ] pagamentos
  - [ ] obrigações
- [ ] Implementar consulta de fechamento
  - [ ] lista histórica
  - [ ] resumo por período

## 19. Módulo 18 - Mobile React Native

- [ ] Definir o escopo do mobile como leitura apenas
  - [ ] bloquear funções transacionais
- [ ] Implementar autenticação do mobile
  - [ ] reaproveitar API e permissões
- [ ] Implementar dashboards resumidos
  - [ ] vendas
  - [ ] financeiro
  - [ ] alertas
- [ ] Implementar consultas operacionais
  - [ ] peças
  - [ ] vendas
  - [ ] saldos
  - [ ] pendências
  - [ ] fechamentos

## 20. Módulo 19 - Backend .NET

- [ ] Estruturar a API por módulos ou feature slices
  - [ ] acesso
  - [ ] lojas
  - [ ] pessoas
  - [ ] catálogo
  - [ ] estoque
  - [ ] vendas
  - [ ] crédito
  - [ ] financeiro
  - [ ] fechamento
  - [ ] dashboards e relatórios
- [ ] Garantir transações nas operações críticas
  - [ ] venda
  - [ ] cancelamento de venda
  - [ ] pagamento ao fornecedor
  - [ ] geração de fechamento
- [ ] Implementar logs e monitoramento técnico
  - [ ] logs estruturados
  - [ ] correlação por request
  - [ ] rastreabilidade de erros
- [ ] Preparar API para evolução
  - [ ] versionamento
  - [ ] documentação OpenAPI
  - [ ] contratos estáveis

## 21. Módulo 20 - Frontend Web React

- [ ] Implementar shell administrativo
  - [ ] autenticação
  - [ ] layout principal
  - [ ] navegação por módulo
  - [ ] troca de loja ativa
- [ ] Implementar telas administrativas por módulo
  - [ ] usuários e permissões
  - [ ] lojas
  - [ ] pessoas
  - [ ] catálogos
  - [ ] regras comerciais
  - [ ] peças
  - [ ] vendas
  - [ ] crédito
  - [ ] financeiro
  - [ ] fechamento
  - [ ] dashboards e relatórios
- [ ] Implementar experiência de operação
  - [ ] busca rápida
  - [ ] filtros
  - [ ] impressão
  - [ ] exportação
  - [ ] alertas visuais

## 22. Módulo 21 - Segurança, Auditoria e Qualidade Operacional

- [ ] Implementar auditoria funcional
  - [ ] cadastros
  - [ ] vendas
  - [ ] cancelamentos
  - [ ] pagamentos
  - [x] ajustes
  - [ ] fechamentos
- [ ] Implementar proteção de dados e acesso
  - [ ] autenticação
  - [ ] autorização por cargo
  - [ ] segregação por loja
  - [ ] restrição de dados pessoais
- [ ] Implementar política de backup e restauração
  - [ ] rotina
  - [ ] validação de restore
- [ ] Implementar estratégia segura de exclusão
  - [ ] inativação
  - [ ] exclusão lógica quando necessário
- [ ] Validar desempenho operacional
  - [ ] busca de peças
  - [ ] venda
  - [ ] fechamento
  - [ ] dashboards

## 23. Módulo 22 - Alertas e Acompanhamento Operacional

- [ ] Implementar alertas de consignação
  - [ ] peças próximas do fim
  - [ ] regra de corte por dias
- [ ] Implementar alertas de pagamentos pendentes
  - [ ] fornecedor com obrigação vencida ou aberta
- [ ] Implementar alertas de crédito inconsistente
  - [ ] saldo negativo
  - [ ] saldo divergente
- [ ] Implementar alertas de cancelamento e ajuste
  - [ ] venda cancelada
  - [ ] ajuste financeiro relevante
- [ ] Implementar painel inicial de pendências
  - [ ] visão por loja
  - [ ] priorização por severidade

## 24. Testes e Qualidade

- [ ] Implementar testes unitários de regras de negócio
  - [ ] venda
  - [ ] crédito
  - [ ] obrigação do fornecedor
  - [ ] consignação
  - [ ] fechamento
- [ ] Implementar testes de integração
  - [ ] API + banco
  - [ ] transações críticas
  - [ ] autorização por loja
- [ ] Implementar testes de interface mais críticos
  - [ ] login
  - [ ] cadastro de peça
  - [ ] venda
  - [ ] pagamento a fornecedor
  - [ ] fechamento
- [ ] Implementar critérios de aceite por módulo
  - [ ] fluxo feliz
  - [ ] fluxo inválido
  - [ ] segurança
  - [ ] rastreabilidade

## 25. Pendências de Definição Antes da Implementação Completa

- [ ] Definir política de troca e devolução para compradores finais
  - [ ] identificar impacto em venda
  - [ ] identificar impacto em estoque
  - [ ] identificar impacto em financeiro
- [ ] Avaliar necessidade futura de emissão fiscal
  - [ ] identificar impacto em venda
  - [ ] identificar impacto em documentos
  - [ ] identificar impacto em integrações externas

## 26. Ordem Recomendada de Execução

- [ ] Fase 1
  - [ ] Fundação técnica e banco
  - [ ] acesso e permissões
  - [ ] lojas e pessoas
  - [ ] catálogos e regras comerciais
- [ ] Fase 2
  - [ ] peças e estoque
  - [ ] vendas
  - [ ] crédito da loja
  - [ ] obrigações e pagamentos do fornecedor
- [ ] Fase 3
  - [ ] movimentação financeira
  - [ ] fechamento
  - [ ] dashboards e relatórios
  - [ ] impressões
- [ ] Fase 4
  - [ ] portal do cliente/fornecedor
  - [ ] mobile de consulta
  - [ ] alertas operacionais
  - [ ] refinamentos de segurança, performance e testes
