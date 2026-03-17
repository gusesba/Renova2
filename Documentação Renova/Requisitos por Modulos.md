---
tags:
  - renova
  - requisitos
  - backlog
status: revisado
origem:
  - "[[Requisitos por Modulos]]"
  - "[[Revisão Requisitos por Modulos]]"
last_update: 2026-03-16
---

# Requisitos do Sistema Renova

Documento consolidado a partir de [[Requisitos por Modulos]] e [[Revisão Requisitos por Modulos]] para servir como base de escopo, backlog e refinamento funcional do sistema.

## Escopo Geral

- [ ] Centralizar a operação de lojas independentes em um único sistema compartilhado.
- [ ] Suportar peças consignadas, peças fixas e peças em lote.
- [ ] Atender donos de loja, funcionários e clientes/fornecedores.
- [ ] Disponibilizar operação principal no web, serviços no backend e consultas essenciais no mobile.
- [ ] Permitir uso por múltiplas lojas independentes com separação de dados e permissões por loja.
- [ ] Permitir compartilhamento de usuários entre lojas autorizadas, inclusive clientes/fornecedores com vínculo em mais de uma loja.

## Módulo 01 - Acesso, Usuários e Permissões

- [ ] Permitir cadastro de usuários do sistema com nome, e-mail, telefone e senha.
- [ ] Permitir login seguro e recuperação de acesso.
- [ ] Permitir ativar, inativar e bloquear usuários.
- [ ] Suportar perfis base como dono da loja, gerente, funcionário e cliente.
- [ ] Permitir que o dono da loja crie cargos personalizados e defina as funcionalidades disponíveis em cada cargo.
- [ ] Permitir atribuir cargos aos funcionários e demais usuários internos.
- [ ] Permitir que um mesmo usuário tenha acesso a mais de uma loja.
- [ ] Vincular um usuário a um cadastro de cliente/fornecedor quando aplicável.
- [ ] Controlar permissões por ação, como cadastrar peças, vender, pagar fornecedor, consultar movimentação financeira e fechar cliente.
- [ ] Registrar histórico de acessos e alterações críticas feitas pelos usuários.

## Módulo 02 - Lojas e Estrutura Operacional

- [ ] Permitir cadastrar lojas com nome, razão social, documento, endereço, contatos e status.
- [ ] Permitir configurar dados de identificação e impressão da loja dentro de um modelo padrão do sistema.
- [ ] Permitir definir usuários responsáveis por cada loja.
- [ ] Permitir que o mesmo sistema atenda uma ou mais lojas independentes.
- [ ] Garantir separação de estoque, vendas, movimentação financeira e relatórios por loja.
- [ ] Permitir visão gerencial por loja e, quando aplicável, visão consolidada apenas das lojas às quais o usuário tenha acesso.

## Módulo 03 - Cadastro de Clientes e Fornecedores

- [ ] Manter cadastro único de pessoa com possibilidade de atuar como cliente, fornecedor ou ambos.
- [ ] Permitir registrar nome, documento, telefone, e-mail, endereço e observações.
- [ ] Permitir registrar dados bancários e preferências de pagamento.
- [ ] Permitir registrar a política padrão do fornecedor ao fim da consignação, como devolver ou doar.
- [ ] Permitir vincular o cadastro da pessoa a um usuário do sistema.
- [ ] Permitir associar a mesma pessoa a uma ou mais lojas.
- [ ] Exibir saldo de crédito, pendências financeiras e histórico de transações da pessoa.
- [ ] Permitir inativar cadastro sem perder histórico.

## Módulo 04 - Cadastros Auxiliares e Tabelas Base

- [ ] Manter cadastro de nome do produto em tabela separada.
- [ ] Manter cadastro de marcas em tabela separada.
- [ ] Manter cadastro de tamanhos em tabela separada.
- [ ] Manter cadastro de cores em tabela separada.
- [ ] Manter cada cadastro auxiliar com apenas identificação e nome.
- [ ] Vincular os cadastros auxiliares diretamente à loja.
- [ ] Impedir compartilhamento automático dos cadastros auxiliares entre lojas.

## Módulo 05 - Configurações Comerciais e Regras de Negócio

- [ ] Permitir configurar percentual padrão de consignação com pagamento em dinheiro.
- [ ] Permitir configurar percentual padrão de consignação com pagamento em crédito da loja.
- [ ] Permitir configurar regra de pagamento misto entre dinheiro e crédito.
- [ ] Permitir definir tempo máximo de permanência da peça na loja.
- [ ] Permitir definir a política de desconto por tempo na loja.
- [ ] Permitir configurar descontos progressivos nos últimos meses de exposição.
- [ ] Permitir definir regras por loja e sobrescrever padrões por fornecedor ou por peça.
- [ ] Permitir cadastrar meios de pagamento da loja com taxa, prazo de recebimento e status.
- [ ] Garantir que o crédito da loja seja controlado por loja, sem compartilhamento automático entre lojas.

## Módulo 06 - Cadastro de Peças e Estoque

- [ ] Permitir cadastrar peça com identificador único.
- [ ] Permitir gerar ou associar código de barras para cada peça.
- [ ] Permitir informar tipo da peça: consignada, fixa ou lote.
- [ ] Permitir informar preço de venda, custo e valores esperados de repasse quando aplicável.
- [ ] Permitir selecionar nome do produto, marca, tamanho, cor e fornecedor a partir das tabelas base.
- [ ] Permitir informar descrição, observações e data de entrada.
- [ ] Permitir informar quantidade para itens em lote.
- [ ] Permitir informar loja, status atual e localização da peça.
- [ ] Permitir anexar foto da peça para consulta interna e externa.
- [ ] Permitir registrar o responsável pelo cadastro.

## Módulo 07 - Ciclo de Vida da Consignação

- [ ] Controlar início e fim do período de consignação de cada peça.
- [ ] Permitir definir regra padrão por fornecedor e exceção por peça.
- [ ] Exibir quantos dias ou meses faltam para encerrar a permanência da peça.
- [ ] Aplicar automaticamente descontos por tempo de loja conforme configuração.
- [ ] Registrar histórico de alterações de preço da peça.
- [ ] Identificar peças próximas do vencimento da consignação.
- [ ] Permitir concluir a consignação com devolução ao fornecedor.
- [ ] Permitir concluir a consignação com doação da peça.
- [ ] Registrar data, responsável e motivo da devolução, doação, perda ou descarte.

## Módulo 08 - Movimentações de Estoque

- [ ] Registrar entradas de peças no estoque.
- [ ] Registrar saídas por venda, devolução, doação, perda, descarte ou ajuste.
- [ ] Registrar ajustes manuais de quantidade para itens em lote.
- [ ] Exibir histórico completo de movimentações por peça.
- [ ] Impedir venda de peça sem saldo disponível.
- [ ] Permitir localizar rapidamente peças por código de barras, nome, marca, fornecedor, status e loja.
- [ ] Permitir filtros por período de entrada, tipo de peça e tempo em loja.

## Módulo 09 - Vendas

- [ ] Permitir criar uma venda contendo uma ou mais peças.
- [ ] Registrar data, hora, loja, vendedor e comprador quando informado.
- [ ] Validar disponibilidade das peças antes da conclusão da venda.
- [ ] Permitir pagamento com um ou mais meios de pagamento na mesma venda.
- [ ] Permitir uso de crédito da loja total ou parcialmente.
- [ ] Permitir combinação entre crédito da loja e pagamento financeiro.
- [ ] Calcular automaticamente total bruto, descontos, taxas e total líquido da venda.
- [ ] Atualizar estoque e status das peças imediatamente após a venda.
- [ ] Gerar comprovante ou recibo da venda.
- [ ] Permitir cancelamento de venda com controle de permissão e trilha de auditoria.
- [ ] Registrar o vendedor para fins operacionais e de rastreabilidade, sem cálculo de comissão.

## Módulo 10 - Crédito da Loja

- [ ] Manter conta corrente de crédito por cliente/fornecedor.
- [ ] Gerar crédito automaticamente quando o fornecedor optar por receber em crédito da loja.
- [ ] Permitir crédito manual com justificativa e responsável.
- [ ] Permitir débito automático do crédito ao realizar compras.
- [ ] Permitir pagamento misto usando parte em crédito e parte em dinheiro.
- [ ] Exibir saldo atual, saldo comprometido e histórico de movimentações do crédito.
- [ ] Relacionar cada movimentação de crédito à venda, pagamento, ajuste ou fechamento correspondente.
- [ ] Impedir uso de crédito acima do saldo disponível.

## Módulo 11 - Pagamentos e Repasses

- [ ] Registrar pagamentos feitos pelos clientes para a loja.
- [ ] Registrar pagamentos feitos pela loja para fornecedores.
- [ ] Gerar obrigação de repasse ao fornecedor apenas quando uma peça consignada for vendida.
- [ ] Gerar obrigação de pagamento imediato para peças fixas e peças em lote compradas pela loja.
- [ ] Permitir pagamento em dinheiro, crédito da loja ou combinação entre os dois.
- [ ] Calcular automaticamente o valor devido ao fornecedor com base na regra comercial aplicável.
- [ ] Permitir pagamentos parciais e controle de saldo pendente.
- [ ] Permitir registrar data, valor, forma de pagamento, comprovante e observações.
- [ ] Exibir situação do pagamento como pendente, parcial, pago, cancelado ou ajustado.

## Módulo 12 - Meios de Pagamento e Conciliação Financeira

- [ ] Consolidar entradas e saídas financeiras por tipo de movimentação.
- [ ] Permitir configurar meios de pagamento como dinheiro, PIX, cartão e outros.
- [ ] Calcular taxa e valor líquido de cada meio de pagamento conforme configuração vigente.
- [ ] Exibir totais por meio de pagamento e saldo financeiro consolidado por período.
- [ ] Permitir registrar despesas, ajustes e outras movimentações financeiras da loja.
- [ ] Permitir conciliação entre vendas, recebimentos, pagamentos a fornecedores e movimentações financeiras.
- [ ] Gerar resumo diário financeiro para conferência.

## Módulo 13 - Fechamento do Cliente/Fornecedor

- [ ] Disponibilizar tela de fechamento financeiro por cliente/fornecedor.
- [ ] Exibir peças atuais, peças vendidas, valores vendidos e valores a receber.
- [ ] Exibir compras feitas pelo cliente na loja.
- [ ] Exibir pagamentos recebidos, pagamentos feitos e saldo final.
- [ ] Permitir filtrar o fechamento por loja e por período.
- [ ] Permitir marcar um fechamento como conferido e liquidado.
- [ ] Manter histórico dos fechamentos realizados.
- [ ] Permitir gerar resumo formatado para copiar e enviar por WhatsApp ou outro canal.
- [ ] Permitir exportar o fechamento em PDF e Excel.

## Módulo 14 - Dashboards e Indicadores

- [ ] Exibir dashboard de vendas por dia, mês, loja, vendedor e período.
- [ ] Exibir dashboard financeiro com entradas, saídas, saldo bruto e saldo líquido.
- [ ] Exibir peças em consignação próximas do vencimento.
- [ ] Exibir peças paradas por tempo em estoque.
- [ ] Exibir valores a pagar para fornecedores e valores pendentes de recebimento.
- [ ] Exibir indicadores por tipo de peça, marca e fornecedor.
- [ ] Permitir filtros rápidos e detalhados nos painéis.

## Módulo 15 - Relatórios e Exportações

- [ ] Permitir exportar listas e relatórios com filtros para Excel.
- [ ] Permitir exportar listas e relatórios com filtros para PDF.
- [ ] Permitir emitir relatório de estoque atual.
- [ ] Permitir emitir relatório de peças vendidas por período.
- [ ] Permitir emitir relatório financeiro por cliente/fornecedor.
- [ ] Permitir emitir relatório financeiro por período e loja.
- [ ] Permitir emitir relatório de peças devolvidas, doadas, perdidas ou descartadas.
- [ ] Permitir salvar filtros frequentes para uso recorrente.

## Módulo 16 - Impressões e Documentos

- [ ] Permitir imprimir etiquetas de peças com código de barras.
- [ ] Utilizar modelo único de etiqueta para todas as lojas.
- [ ] Permitir imprimir recibo de venda.
- [ ] Utilizar modelo único de recibo e comprovantes no sistema.
- [ ] Permitir imprimir comprovante de pagamento ao fornecedor.
- [ ] Permitir imprimir comprovante de devolução ou doação de peças.
- [ ] Tornar o processo de impressão simples para o dono da loja, com poucos passos.

## Módulo 17 - Portal Web do Cliente/Fornecedor

- [ ] Permitir que o cliente veja as lojas em que possui vínculo.
- [ ] Permitir visualizar peças atuais que estão na loja.
- [ ] Permitir visualizar peças já vendidas e respectivos valores.
- [ ] Permitir visualizar saldo de crédito, pendências e saldo final.
- [ ] Permitir visualizar histórico financeiro e de pagamentos.
- [ ] Permitir visualizar resumo de fechamento por período.
- [ ] Restringir o portal às informações do próprio usuário.

## Módulo 18 - Mobile React Native

- [ ] Disponibilizar visualizações essenciais para donos de loja em modo consulta.
- [ ] Disponibilizar visualizações essenciais para clientes/fornecedores em modo consulta.
- [ ] Permitir consulta de dashboards resumidos no mobile.
- [ ] Permitir consulta de peças, vendas e saldos no mobile.
- [ ] Permitir consulta de pendências e fechamentos no mobile.
- [ ] Não exigir operações de cadastro, edição ou venda no mobile nesta versão.
- [ ] Garantir experiência de uso adequada em celulares Android e iOS.

## Módulo 19 - Backend .NET

- [ ] Expor API segura para autenticação, cadastros, estoque, vendas, pagamentos e relatórios.
- [ ] Centralizar no backend todas as regras de negócio e cálculos financeiros.
- [ ] Garantir consistência entre estoque, venda, crédito e pagamento em transações críticas.
- [ ] Permitir versionamento da API para evolução do produto.
- [ ] Disponibilizar logs de erro, auditoria e monitoramento técnico.
- [ ] Preparar estrutura modular para crescimento do sistema.

## Módulo 20 - Frontend Web React

- [ ] Disponibilizar interface administrativa completa para operação da loja.
- [ ] Disponibilizar interface de portal para clientes/fornecedores.
- [ ] Permitir navegação por módulos com busca e filtros rápidos.
- [ ] Permitir leitura facilitada em desktop e tablet.
- [ ] Permitir impressão, exportação e cópia formatada diretamente pela interface.
- [ ] Exibir alertas visuais para pendências, vencimentos e inconsistências operacionais.

## Módulo 21 - Segurança, Auditoria e Qualidade Operacional

- [ ] Registrar auditoria de cadastros, vendas, cancelamentos, pagamentos, ajustes e fechamentos.
- [ ] Proteger dados pessoais e financeiros com controle de acesso adequado.
- [ ] Garantir que usuários vejam apenas os dados das lojas e pessoas autorizadas.
- [ ] Permitir backup e restauração de dados.
- [ ] Tratar exclusões de forma segura, preferencialmente com inativação ou exclusão lógica.
- [ ] Registrar erros e falhas operacionais para suporte e investigação.
- [ ] Garantir desempenho adequado para busca de peças, fechamento e dashboard.

## Módulo 22 - Alertas e Acompanhamento Operacional

- [ ] Alertar sobre peças próximas do fim da consignação.
- [ ] Alertar sobre pagamentos pendentes a fornecedores.
- [ ] Alertar sobre saldo de crédito inconsistente ou negativo.
- [ ] Alertar sobre vendas canceladas e ajustes financeiros relevantes.
- [ ] Exibir tarefas pendentes na visão inicial do dono da loja.

## Pontos Para Validar no Refinamento

- [ ] Definir política de troca e devolução para compradores finais.
- [ ] Avaliar necessidade futura de emissão fiscal ou integração com sistema fiscal.
