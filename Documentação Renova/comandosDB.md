---
tags:
  - renova
  - banco-de-dados
  - migrations
status: ativo
last_update: 2026-03-16
---

# Comandos DB

Comandos para criar migrations, atualizar o banco PostgreSQL local e remover o banco usando a solução backend do Renova.

## Premissas

- Executar os comandos a partir de `D:\utf\Renova2\Backend`
- Projeto do contexto: `Persistencia`
- O projeto `Persistencia` possui `RenovaDbContextFactory`, então não é necessário informar `--startup-project`
- A connection string de desenvolvimento fica em `API/appsettings.Development.json`
- Em produção, `API/appsettings.json` mantém `ConnectionStrings:RenovaDb` vazia

## Restaurar dependências e ferramentas

```powershell
dotnet restore
dotnet tool restore
```

## Criar uma nova migration

```powershell
dotnet ef migrations add NomeDaMigration --project .\Persistencia\Persistencia.csproj --context RenovaDbContext
```

Exemplo:

```powershell
dotnet ef migrations add InitialCreate --project .\Persistencia\Persistencia.csproj --context RenovaDbContext
```

## Aplicar migrations no banco

```powershell
dotnet ef database update --project .\Persistencia\Persistencia.csproj --context RenovaDbContext
```

## Remover a última migration ainda não aplicada

```powershell
dotnet ef migrations remove --project .\Persistencia\Persistencia.csproj --context RenovaDbContext
```

## Apagar o banco

```powershell
dotnet ef database drop --project .\Persistencia\Persistencia.csproj --context RenovaDbContext --force
```

## Recriar do zero

```powershell
dotnet ef database drop --project .\Persistencia\Persistencia.csproj --context RenovaDbContext --force
dotnet ef database update --project .\Persistencia\Persistencia.csproj --context RenovaDbContext
```

## Usar um banco temporário para validação

```powershell
$env:ConnectionStrings__RenovaDb = 'Host=localhost;Port=5432;Database=renova_temp;Username=postgres;Password=mps202504'
dotnet ef database update --project .\Persistencia\Persistencia.csproj --context RenovaDbContext
dotnet ef database drop --project .\Persistencia\Persistencia.csproj --context RenovaDbContext --force
```

## Observações

- O manifesto local `Backend/dotnet-tools.json` fixa `dotnet-ef` na versão `10.0.5`, alinhada ao runtime do projeto
- Se for necessário atualizar a ferramenta local:

```powershell
dotnet tool update dotnet-ef --version 10.0.5
```

- Em produção, a connection string deve ser fornecida por variável de ambiente, secret manager ou configuração da infraestrutura
