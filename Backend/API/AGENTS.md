# API AGENTS

## Papel desta pasta
- Esta camada hospeda a Web API do Renova.
- Responsabilidades:
  - bootstrap da aplicacao
  - configuracao do pipeline HTTP
  - autenticacao e autorizacao
  - controllers versionados
  - middlewares, tratamento de erro e logging

## Pontos de entrada
- `Program.cs`: monta o host e chama `AddRenovaApi()` e `UseRenovaApi()`.
- `Infrastructure/DependencyInjection/ApiServiceCollectionExtensions.cs`: registra servicos da API.
- `Infrastructure/Hosting/WebApplicationExtensions.cs`: organiza middleware, OpenAPI e inicializacao.

## Seguranca
- Autenticacao atual usa `SessionTokenAuthenticationHandler`.
- O token nao e JWT; ele e opaco, enviado como `Bearer`, e validado contra `usuario_sessao`.
- Claims customizados usados hoje:
  - `renova:user_id`
  - `renova:session_id`
  - `renova:active_store_id`
- Autorizacao por permissao usa:
  - `RequirePermissionAttribute`
  - `PermissionPolicyProvider`
  - `PermissionAuthorizationHandler`

## Controllers e rotas
- Controllers do modulo de acesso estao em `Features/Access/V1`.
- O padrao atual e `api/v{version}/...`.
- A API usa `Asp.Versioning`.
- Endpoints devem continuar retornando envelopes e `ProblemDetails` para falhas.

## Middlewares e pipeline
- Ordem atual relevante:
  - exception handler
  - HSTS e HTTPS redirection fora de `Development`
  - CORS
  - request logging
  - authentication
  - authorization
  - controllers
- Em `Development` existe suporte para OpenAPI e rota de Swagger UI.

## Quando alterar esta pasta
- Mudancas de contrato HTTP: controllers, DTOs da API, policies ou pipeline.
- Mudancas de regra de negocio nao devem nascer aqui; elas devem vir de `Servicos`.
- Ao adicionar um novo modulo HTTP:
  - criar `Features/<Modulo>/V1`
  - manter o controller fino
  - proteger endpoints com `[Authorize]` ou `[RequirePermission(...)]`
