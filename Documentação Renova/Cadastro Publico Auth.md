# Cadastro Publico na Autenticacao

## Regra funcional

- O cadastro de conta de acesso e publico.
- Qualquer pessoa pode criar sua conta diretamente na tela de autenticacao.
- Os campos obrigatorios do cadastro publico sao:
  - nome
  - e-mail
  - telefone
  - senha

## Comportamento apos o cadastro

- A conta criada pode fazer login normalmente.
- O cadastro publico nao cria automaticamente:
  - vinculo com loja
  - cargos
  - permissoes
- Enquanto nao existir `usuario_loja`, o usuario autenticado deve visualizar estado de acesso pendente.
- A liberacao operacional continua sendo administrativa, por meio do vinculo com loja e atribuicao de cargos.

## Impacto tecnico

- Endpoint publico: `POST /api/v1/access/auth/register`
- Persistencia principal: `usuario`
- Eventos gerados:
  - `usuario_acesso_evento` com `cadastro_publico`
  - `auditoria_evento` para rastreabilidade da criacao

## Fluxo resumido

1. Usuario abre a tela de autenticacao.
2. Usuario escolhe `Criar conta`.
3. Sistema valida os dados e cria o registro em `usuario`.
4. Usuario volta para o login e pode se autenticar.
5. Se ainda nao houver vinculo com loja, o sistema exibe acesso pendente.
