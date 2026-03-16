# Backend Renova

## Camadas

- `Renova.Domain`: entidades e modelos centrais do domínio.
- `Renova.Persistence`: `DbContext`, mapeamentos e integração com PostgreSQL.
- `Renova.Services`: contratos, handlers, services, validators e orquestração de caso de uso.
- `Renova.Api`: composição HTTP, versionamento, middleware, controllers e contratos públicos.

## Convenções

- Namespaces seguem o padrão `Renova.<Camada>`.
- APIs entram em `API/Features/<Feature>/V{Versao}`.
- Casos de uso entram em `Servicos/Features/<Feature>`.
- DTOs internos de aplicação ficam em `Servicos/Contracts`.
- Validators ficam em `Servicos/Validation` ou `Servicos/Features/<Feature>/Validators`.
- Serviços de orquestração ficam em `Servicos/Features/<Feature>/Services`.
- Infraestrutura HTTP compartilhada fica em `API/Common` e `API/Infrastructure`.

## Estratégia de Implementação

- Preferir feature slices em vez de pastas genéricas gigantes.
- Controllers devem ser finos e delegar para serviços/handlers.
- Respostas de sucesso usam envelope consistente.
- Falhas usam `ProblemDetails` com tratamento centralizado.
- Operações críticas devem ser transacionais na camada de serviços.
