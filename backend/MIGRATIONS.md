# Migrations

## Ferramenta EF

Se o `dotnet ef` da maquina estiver em versao anterior ao EF Core do projeto, atualize antes:

```powershell
dotnet tool update --global dotnet-ef --version 10.*
```

## Dropar a base

```powershell
dotnet ef database drop --project .\Renova.Persistence\Renova.Persistence.csproj --startup-project .\Renova.API\Renova.API.csproj --force
```

## Gerar uma nova migracao

```powershell
dotnet ef migrations add NomeDaMigracao --project .\Renova.Persistence\Renova.Persistence.csproj --startup-project .\Renova.API\Renova.API.csproj --output-dir Migrations
```

## Atualizar a base

```powershell
dotnet ef database update --project .\Renova.Persistence\Renova.Persistence.csproj --startup-project .\Renova.API\Renova.API.csproj
```
