# Frontend App AGENTS

## Papel desta pasta

- Esta pasta concentra a composicao final das rotas do App Router.
- Aqui ficam:
  - layouts por grupo de rota
  - pages
  - components especificos de cada rota

## Grupos e rotas atuais

- `login`
  - fluxo de autenticacao e recuperacao de acesso
  - fica fora do shell do sistema
- `(system)`
  - agrupa rotas autenticadas
  - aplica `SystemSessionProvider`
  - aplica o shell com sidebar e header
- `(system)/dashboard`
  - home administrativa atual do modulo de acesso

## Relacionamentos importantes

- `/(system)/components/system-session-provider.tsx`
  - valida o token salvo
  - carrega `/access/auth/me`
  - troca loja ativa
  - executa logout
- `/(system)/components/system-route-frame.tsx`
  - controla o shell
  - controla o toggle da sidebar
- `login/components/login-screen.tsx`
  - faz login
  - faz recuperacao de senha
  - redireciona para `/dashboard` quando a sessao esta valida

## Como extender

- Nova tela autenticada:
  - criar dentro de `app/(system)/<rota>`
  - reutilizar componentes de `components`
  - manter logica de API em `lib/services`
- Nova tela publica:
  - criar fora de `/(system)`
  - nao depender do shell do sistema

## Cuidados

- Evitar logica de negocio profunda nos layouts.
- Providers globais devem ser poucos e previsiveis.
- Preferir pequenos componentes com responsabilidade unica.
