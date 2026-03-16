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
- Projeto de startup: `API`
- Connection string de desenvolvimento configurada em `API/appsettings.Development.json`

## Restaurar dependências

```powershell
dotnet restore
```

## Criar uma nova migration

```powershell
dotnet ef migrations add NomeDaMigration --project .\Persistencia\Persistencia.csproj --startup-project .\API\API.csproj
```

Exemplo:

```powershell
dotnet ef migrations add InitialCreate --project .\Persistencia\Persistencia.csproj --startup-project .\API\API.csproj
```

## Aplicar migrations no banco

```powershell
dotnet ef database update --project .\Persistencia\Persistencia.csproj --startup-project .\API\API.csproj
```

## Remover a última migration ainda não aplicada

```powershell
dotnet ef migrations remove --project .\Persistencia\Persistencia.csproj --startup-project .\API\API.csproj
```

## Apagar o banco

```powershell
dotnet ef database drop --project .\Persistencia\Persistencia.csproj --startup-project .\API\API.csproj --force
```

## Recriar do zero

```powershell
dotnet ef database drop --project .\Persistencia\Persistencia.csproj --startup-project .\API\API.csproj --force
dotnet ef database update --project .\Persistencia\Persistencia.csproj --startup-project .\API\API.csproj
```

## Observações

- Em produção, a connection string em `appsettings.json` permanece vazia e deve ser fornecida por variável de ambiente, secret manager ou configuração da infraestrutura.
- Se o comando `dotnet ef` não estiver disponível, instalar a ferramenta:

```powershell
dotnet tool install --global dotnet-ef
```
