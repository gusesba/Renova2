FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Renova.Backend.sln ./
COPY Renova.API/Renova.API.csproj Renova.API/
COPY Renova.Domain/Renova.Domain.csproj Renova.Domain/
COPY Renova.Persistence/Renova.Persistence.csproj Renova.Persistence/
COPY Renova.Service/Renova.Service.csproj Renova.Service/

RUN dotnet restore Renova.API/Renova.API.csproj

COPY . .
RUN dotnet publish Renova.API/Renova.API.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "Renova.API.dll"]
