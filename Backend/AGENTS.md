# Backend AGENTS

## Visao geral
- Stack principal: .NET 10, ASP.NET Core Web API, EF Core, PostgreSQL.
- Solucao dividida em quatro projetos:
  - `Dominio`: models e tipos centrais do negocio.
  - `Persistencia`: `RenovaDbContext`, migrations e configuracao de banco.
  - `Servicos`: regras de negocio, contratos e servicos por modulo.
  - `API`: host HTTP, controllers, seguranca, pipeline e DI.

## Dependencias entre projetos
- `API` referencia `Servicos` e `Persistencia`.
- `Servicos` referencia `Dominio` e `Persistencia`.
- `Persistencia` referencia `Dominio`.
- `Dominio` nao deve depender de outras camadas da solucao.

## Banco e persistencia
- Provider atual: `Npgsql.EntityFrameworkCore.PostgreSQL`.
- `RenovaDbContext` fica em `Persistencia/RenovaDbContext.cs`.
- A convencao atual usa `snake_case` para tabelas e colunas.
- Nomes de entidades seguem o documento `Documentacao Renova/Modelagem Banco de Dados Renova.md`.
- A connection string usada pela aplicacao e `ConnectionStrings:RenovaDb`.

## Modulos implementados
- O modulo mais avancado neste momento e `Modulo 01 - Acesso, Usuarios e Permissoes`.
- Esse modulo usa:
  - autenticacao por token opaco persistido em `usuario_sessao`
  - autorizacao por permissao e loja ativa
  - recuperacao de senha por `usuario_recuperacao_acesso`
  - auditoria em `usuario_acesso_evento` e `auditoria_evento`

## Regras de evolucao
- Novas regras de negocio devem entrar em `Servicos/Features/<Modulo>`.
- Controllers devem continuar finos, delegando fluxo para servicos.
- Nao colocar regra de negocio em `DbContext`, controllers ou models.
- Ao adicionar tabela nova:
  - criar model em `Dominio/Models`
  - expor `DbSet` no `RenovaDbContext`
  - mapear em `OnModelCreating`
  - gerar migration

## Documentacao de referencia
- `Documentacao Renova/Requisitos por Modulos.md`
- `Documentacao Renova/Modelagem Banco de Dados Renova.md`
- `Documentacao Renova/Relacoes Funcoes App.md`
- `Documentacao Renova/Checklist Implementacao Renova.md`
