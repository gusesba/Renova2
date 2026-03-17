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
  - [ ] validação de documento único
  - [ ] inativação lógica
  - [ ] tela web de listagem e edição
- [ ] Implementar configuração operacional da loja
  - [x] CRUD de `loja_configuracao`
  - [ ] dados de impressão
  - [ ] nome de exibição
  - [ ] fuso horário e moeda
- [ ] Implementar visão consolidada por usuário
  - [x] listar apenas lojas vinculadas
  - [x] permitir troca de loja ativa
  - [ ] consolidar visão gerencial somente das lojas autorizadas

## 04. Módulo 03 - Cadastro de Clientes e Fornecedores

- [ ] Implementar cadastro mestre de pessoa
  - [ ] CRUD de `pessoa`
  - [ ] validação de documento
  - [ ] inativação lógica
  - [ ] tela web de busca e edição
- [ ] Implementar vínculo pessoa x loja
  - [ ] CRUD de `pessoa_loja`
  - [ ] marcar cliente, fornecedor ou ambos
  - [ ] definir política padrão de fim de consignação
  - [ ] exibir situação por loja
- [ ] Implementar contas bancárias
  - [ ] CRUD de `pessoa_conta_bancaria`
  - [ ] suporte a PIX
  - [ ] conta principal
- [ ] Implementar vínculo pessoa x usuário
  - [ ] permitir associar `usuario` a `pessoa`
  - [ ] preparar acesso ao portal
- [ ] Implementar visão financeira da pessoa
  - [ ] consolidar saldo de crédito
  - [ ] consolidar pendências
  - [ ] consolidar histórico de transações

## 05. Módulo 04 - Cadastros Auxiliares e Tabelas Base

- [ ] Implementar cadastro de nomes de produto
  - [ ] CRUD de `produto_nome`
  - [ ] associação ao `conjunto_catalogo`
  - [ ] inativação lógica
- [ ] Implementar cadastro de marcas
  - [ ] CRUD de `marca`
  - [ ] associação ao `conjunto_catalogo`
- [ ] Implementar cadastro de tamanhos
  - [ ] CRUD de `tamanho`
  - [ ] ordenação de exibição
- [ ] Implementar cadastro de cores
  - [ ] CRUD de `cor`
  - [ ] suporte a código hexadecimal quando útil
- [ ] Implementar cadastro de categorias
  - [ ] CRUD de `categoria`
  - [ ] filtros e agrupamentos
- [ ] Implementar cadastro de coleções
  - [ ] CRUD de `colecao`
  - [ ] ano de referência
- [ ] Implementar compartilhamento via conjunto de catálogo
  - [ ] CRUD de `conjunto_catalogo`
  - [ ] vincular lojas ao conjunto
  - [ ] garantir reutilização apenas entre lojas autorizadas

## 06. Módulo 05 - Configurações Comerciais e Regras de Negócio

- [ ] Implementar regra comercial padrão da loja
  - [ ] CRUD de `loja_regra_comercial`
  - [ ] percentuais para dinheiro
  - [ ] percentuais para crédito
  - [ ] pagamento misto
  - [ ] prazo máximo de exposição
  - [ ] política de desconto em JSON ou estrutura equivalente
- [ ] Implementar regra comercial por fornecedor
  - [ ] CRUD de `fornecedor_regra_comercial`
  - [ ] vínculo com `pessoa_loja`
  - [ ] sobrescrita da regra padrão
- [ ] Implementar configuração de meios de pagamento
  - [ ] CRUD de `meio_pagamento`
  - [ ] taxa
  - [ ] prazo de recebimento
  - [ ] status ativo/inativo
- [ ] Implementar serviço de resolução da regra efetiva
  - [ ] priorizar regra manual da peça
  - [ ] depois regra do fornecedor
  - [ ] depois regra da loja

## 07. Módulo 06 - Cadastro de Peças e Estoque

- [ ] Implementar cadastro de peça
  - [ ] contrato de entrada
  - [ ] validação de loja, fornecedor e tabelas auxiliares
  - [ ] geração de código interno
  - [ ] suporte a código de barras
  - [ ] gravação em `peca`
- [ ] Implementar snapshot da condição comercial da peça
  - [ ] resolver regra efetiva no momento da entrada
  - [ ] gravar em `peca_condicao_comercial`
- [ ] Implementar upload e vínculo de imagens
  - [ ] armazenamento físico/lógico do arquivo
  - [ ] gravação em `peca_imagem`
  - [ ] visibilidade interna/externa
- [ ] Implementar entrada inicial de estoque
  - [ ] gravar `movimentacao_estoque` do tipo entrada
  - [ ] atualizar saldo inicial
- [ ] Implementar telas web de cadastro, edição e consulta de peça
  - [ ] filtros rápidos
  - [ ] busca por código de barras
  - [ ] exibição de status e localização

## 08. Módulo 07 - Ciclo de Vida da Consignação

- [ ] Implementar cálculo de prazo de consignação
  - [ ] usar `peca_condicao_comercial`
  - [ ] calcular data de início e fim
  - [ ] exibir dias restantes
- [ ] Implementar desconto por tempo de loja
  - [ ] aplicar política configurada
  - [ ] atualizar preço da peça quando necessário
  - [ ] registrar histórico em `peca_historico_preco`
- [ ] Implementar devolução ao fornecedor
  - [ ] alterar status da peça
  - [ ] registrar `movimentacao_estoque`
  - [ ] gerar comprovante
  - [ ] auditar ação
- [ ] Implementar doação da peça
  - [ ] alterar status da peça
  - [ ] registrar `movimentacao_estoque`
  - [ ] gerar comprovante
  - [ ] auditar ação
- [ ] Implementar perda e descarte
  - [ ] alterar status
  - [ ] registrar motivo e responsável
  - [ ] auditar ação

## 09. Módulo 08 - Movimentações de Estoque

- [ ] Implementar listagem completa de movimentações
  - [ ] consulta por peça
  - [ ] consulta por loja
  - [ ] consulta por período
- [ ] Implementar ajustes manuais
  - [ ] permissões específicas
  - [ ] atualização de saldo
  - [ ] gravação em `movimentacao_estoque`
- [ ] Impedir venda sem saldo
  - [ ] validar `quantidade_atual`
  - [ ] bloquear transação antes da conclusão
- [ ] Implementar busca operacional de peças
  - [ ] por código de barras
  - [ ] por nome, marca, fornecedor e status
  - [ ] por tempo em loja

## 10. Módulo 09 - Vendas

- [ ] Implementar abertura e conclusão de venda
  - [ ] criar `venda`
  - [ ] adicionar `venda_item`
  - [ ] validar disponibilidade das peças
  - [ ] calcular subtotal, desconto, taxa e total líquido
- [ ] Implementar composição de pagamento
  - [ ] registrar `venda_pagamento`
  - [ ] permitir múltiplos meios de pagamento
  - [ ] permitir pagamento misto com crédito
- [ ] Implementar atualização transacional pós-venda
  - [ ] baixar estoque
  - [ ] alterar status da peça
  - [ ] gerar obrigação do fornecedor quando aplicável
  - [ ] gerar movimentação financeira
  - [ ] gerar movimentação de crédito quando houver uso de crédito
- [ ] Implementar cancelamento de venda
  - [ ] mudar `status_venda`
  - [ ] estornar estoque
  - [ ] estornar financeiro
  - [ ] estornar crédito quando necessário
  - [ ] registrar auditoria
- [ ] Implementar emissão de recibo
  - [ ] modelo único
  - [ ] dados de venda e pagamento

## 11. Módulo 10 - Crédito da Loja

- [ ] Implementar conta de crédito por loja e pessoa
  - [ ] criar e manter `conta_credito_loja`
  - [ ] garantir unicidade por loja + pessoa
- [ ] Implementar livro razão do crédito
  - [ ] registrar em `movimentacao_credito_loja`
  - [ ] manter saldo anterior e posterior
- [ ] Implementar crédito manual
  - [ ] exigir justificativa
  - [ ] exigir responsável
  - [ ] auditar ação
- [ ] Implementar crédito por repasse
  - [ ] gerar crédito durante pagamento ao fornecedor quando aplicável
- [ ] Implementar uso de crédito em compra
  - [ ] validar saldo
  - [ ] registrar débito
  - [ ] relacionar à venda
- [ ] Implementar consultas de extrato e saldo
  - [ ] frontend web
  - [ ] portal
  - [ ] mobile consulta

## 12. Módulo 11 - Pagamentos e Repasses

- [ ] Implementar geração de obrigação do fornecedor
  - [ ] criar `obrigacao_fornecedor` para peça consignada vendida
  - [ ] criar `obrigacao_fornecedor` para peça fixa/lote comprada
  - [ ] definir tipo, saldo em aberto e status
- [ ] Implementar liquidação da obrigação
  - [ ] registrar `liquidacao_obrigacao_fornecedor`
  - [ ] permitir dinheiro, crédito ou misto
  - [ ] atualizar saldo em aberto
  - [ ] atualizar status
- [ ] Implementar comprovante de pagamento ao fornecedor
  - [ ] modelo único
  - [ ] dados da liquidação
- [ ] Implementar listagem de pendências
  - [ ] por fornecedor
  - [ ] por loja
  - [ ] por status

## 13. Módulo 12 - Meios de Pagamento e Conciliação Financeira

- [ ] Implementar livro razão financeiro
  - [ ] registrar entradas e saídas em `movimentacao_financeira`
  - [ ] relacionar com `venda_pagamento` quando houver
  - [ ] relacionar com `liquidacao_obrigacao_fornecedor` quando houver
- [ ] Implementar lançamentos financeiros avulsos
  - [ ] despesas
  - [ ] receitas avulsas
  - [ ] ajustes
  - [ ] estornos
- [ ] Implementar conciliação financeira
  - [ ] consolidar por período
  - [ ] consolidar por meio de pagamento
  - [ ] evidenciar taxas e valores líquidos
- [ ] Implementar resumo diário financeiro
  - [ ] total de entradas
  - [ ] total de saídas
  - [ ] saldo bruto
  - [ ] saldo líquido

## 14. Módulo 13 - Fechamento do Cliente/Fornecedor

- [ ] Implementar geração de fechamento
  - [ ] consolidar peças atuais
  - [ ] consolidar peças vendidas
  - [ ] consolidar valores vendidos e a receber
  - [ ] consolidar compras feitas na loja
  - [ ] consolidar pagamentos e saldo final
  - [ ] gravar `fechamento_pessoa`
  - [ ] gravar `fechamento_pessoa_item`
  - [ ] gravar `fechamento_pessoa_movimento`
- [ ] Implementar conferência e liquidação do fechamento
  - [ ] permitir marcar como conferido
  - [ ] permitir marcar como liquidado
  - [ ] impedir alteração indevida após liquidação
- [ ] Implementar exportação do fechamento
  - [ ] PDF
  - [ ] Excel
  - [ ] texto formatado para WhatsApp
- [ ] Implementar histórico de fechamentos
  - [ ] filtro por pessoa
  - [ ] filtro por loja
  - [ ] filtro por período

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
  - [ ] saldo bruto
  - [ ] saldo líquido
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
  - [ ] filtros por loja, status e categoria
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
  - [ ] ajustes
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
