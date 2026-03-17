# Frontend AGENTS

## Stack

- Next.js 16 com App Router.
- React 19.
- TypeScript.
- Tailwind CSS v4 via `globals.css` com classes utilitarias e classes customizadas.
- `sonner` para toasts globais.
- `zod` para formulários.
- `react query` para http requests

## Estrutura adotada

- `app`: rotas, layouts e componentes especificos de pagina.
- `components`: componentes puros e reutilizaveis.
- `lib/helpers`: helpers internos, formatacao e session storage.
- `lib/services`: cliente HTTP base e services por modulo.

## Convencoes atuais

- Componentes visuais compartilhados ficam em `components/ui`, `components/layout` e `components/brand`.
- Componentes com logica de tela ficam em `app/<rota>/components`.
- Chamadas HTTP para o backend devem passar por `lib/services/core` e pelos modulos em `lib/services/access` ou `lib/services/stores`.
- Leitura e escrita de token local devem passar por `lib/helpers/session-storage.ts`.
- Feedback de sucesso, erro e alerta deve usar `sonner`; evitar banners locais novos.

## Relacao com o backend

- Base URL atual: `NEXT_PUBLIC_API_BASE_URL`, com fallback para `http://localhost:5131/api/v1`.
- A sessao depende do token opaco retornado por `/access/auth/login`.
- O shell do sistema depende de `/access/auth/me` para hidratar:
  - usuario autenticado
  - loja ativa
  - lojas acessiveis
  - permissoes efetivas

## Layouts ativos

- `/login`: tela isolada, fora do shell principal.
- `/(system)`: shell com sidebar, header e conteudo.
- `/dashboard`: primeira tela administrativa dentro do shell.

## Regras de evolucao

- Nao concentrar tudo em uma unica pagina grande.
- Se uma tela crescer, criar componentes especificos dentro da pasta da rota.
- Manter componentes puros sem acesso direto a API quando possivel.
- Estados de sessao compartilhados devem continuar centralizados no provider do grupo `/(system)`.
