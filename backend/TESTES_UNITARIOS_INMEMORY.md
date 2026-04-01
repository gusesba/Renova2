# Testes Unitarios do `RenovaService` com banco em memoria

Este projeto ja possui um servico simples, o [`RenovaService`](/D:/utf/Renova2/backend/Renova.Service/Services/RenovaService.cs), que depende diretamente do [`RenovaDbContext`](/D:/utf/Renova2/backend/Renova.Persistence/RenovaDbContext.cs). Para testar esse servico sem acessar PostgreSQL, voce pode usar o provider `Microsoft.EntityFrameworkCore.InMemory`.

## Quando usar

Essa abordagem e util para testar a logica do servico:

- persistencia basica com `AddAsync`
- leitura com `FindAsync`
- comportamento esperado do metodo `CreateAsync`
- comportamento esperado do metodo `GetAsync`

Ela e rapida e simples, mas tem uma limitacao importante: o provider `InMemory` nao reproduz fielmente o comportamento relacional de um banco real. Se voce quiser validar regras mais proximas do PostgreSQL ou SQL Server, prefira `SQLite` em memoria.

## Estrutura atual relevante

No projeto atual:

- [`RenovaService`](/D:/utf/Renova2/backend/Renova.Service/Services/RenovaService.cs) cria e consulta `RenovaModel`
- [`RenovaDbContext`](/D:/utf/Renova2/backend/Renova.Persistence/RenovaDbContext.cs) expoe `DbSet<RenovaModel>`
- [`RenovaModel`](/D:/utf/Renova2/backend/Renova.Domain/Model/RenovaModel.cs) possui `Campo1`, `Campo2` e `Campo3`
- o projeto de testes ja existe em [`Renova.Tests`](/D:/utf/Renova2/backend/Renova.Tests/Renova.Tests.csproj)

## 1. Adicionar dependencias no projeto de testes

Hoje o projeto [`Renova.Tests.csproj`](/D:/utf/Renova2/backend/Renova.Tests/Renova.Tests.csproj) possui apenas `xUnit` e `Microsoft.NET.Test.Sdk`. Para testar o servico com EF Core em memoria, adicione:

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="10.0.4" />
</ItemGroup>

<ItemGroup>
  <ProjectReference Include="..\\Renova.Domain\\Renova.Domain.csproj" />
  <ProjectReference Include="..\\Renova.Persistence\\Renova.Persistence.csproj" />
  <ProjectReference Include="..\\Renova.Service\\Renova.Service.csproj" />
</ItemGroup>
```

Com isso, o projeto de testes passa a enxergar:

- `RenovaDbContext`
- `RenovaService`
- `RenovaCommand`
- `RenovaQuery`
- `RenovaModel`

## 2. Criar um helper para montar o contexto em memoria

Cada teste deve usar um nome de banco unico para evitar vazamento de dados entre execucoes.

Exemplo:

```csharp
using Microsoft.EntityFrameworkCore;
using Renova.Persistence;

private static RenovaDbContext CriarContextoEmMemoria()
{
    var options = new DbContextOptionsBuilder<RenovaDbContext>()
        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        .Options;

    return new RenovaDbContext(options);
}
```

## 3. Exemplo de teste para `CreateAsync`

O metodo [`CreateAsync`](/D:/utf/Renova2/backend/Renova.Service/Services/RenovaService.cs) deve:

- criar uma entidade `RenovaModel`
- copiar `Campo2` e `Campo3`
- salvar no contexto
- retornar a entidade persistida

Exemplo de teste:

```csharp
using Microsoft.EntityFrameworkCore;
using Renova.Service.Commands;
using Renova.Service.Services;
using Renova.Persistence;
using Xunit;

namespace Renova.Tests.Exemplo;

public class RenovaServiceTests
{
    private static RenovaDbContext CriarContextoEmMemoria()
    {
        var options = new DbContextOptionsBuilder<RenovaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new RenovaDbContext(options);
    }

    [Fact]
    public async Task CreateAsync_DeveSalvarERetornarEntidade()
    {
        await using var context = CriarContextoEmMemoria();
        var service = new RenovaService(context);

        var command = new RenovaCommand
        {
            Campo2 = "teste",
            Campo3 = 123
        };

        var resultado = await service.CreateAsync(command);

        Assert.NotNull(resultado);
        Assert.Equal("teste", resultado.Campo2);
        Assert.Equal(123, resultado.Campo3);

        var salvoNoBanco = await context.Renova.FirstAsync();
        Assert.Equal(resultado.Campo1, salvoNoBanco.Campo1);
        Assert.Equal("teste", salvoNoBanco.Campo2);
        Assert.Equal(123, salvoNoBanco.Campo3);
    }
}
```

## 4. Exemplo de teste para `GetAsync`

O metodo [`GetAsync`](/D:/utf/Renova2/backend/Renova.Service/Services/RenovaService.cs) usa `FindAsync` com a chave primaria `Campo1`. Portanto, o teste deve inserir dados antes da chamada.

Exemplo:

```csharp
using Microsoft.EntityFrameworkCore;
using Renova.Domain.Model;
using Renova.Persistence;
using Renova.Service.Queries;
using Renova.Service.Services;
using Xunit;

namespace Renova.Tests.Exemplo;

public class RenovaServiceQueryTests
{
    private static RenovaDbContext CriarContextoEmMemoria()
    {
        var options = new DbContextOptionsBuilder<RenovaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new RenovaDbContext(options);
    }

    [Fact]
    public async Task GetAsync_DeveRetornarRegistroQuandoExistir()
    {
        await using var context = CriarContextoEmMemoria();

        var entidade = new RenovaModel
        {
            Campo2 = "existente",
            Campo3 = 50
        };

        context.Renova.Add(entidade);
        await context.SaveChangesAsync();

        var service = new RenovaService(context);

        var query = new RenovaQuery
        {
            CampoQuery = entidade.Campo1
        };

        var resultado = await service.GetAsync(query);

        Assert.NotNull(resultado);
        Assert.Equal(entidade.Campo1, resultado!.Campo1);
        Assert.Equal("existente", resultado.Campo2);
        Assert.Equal(50, resultado.Campo3);
    }

    [Fact]
    public async Task GetAsync_DeveRetornarNullQuandoNaoExistir()
    {
        await using var context = CriarContextoEmMemoria();
        var service = new RenovaService(context);

        var query = new RenovaQuery
        {
            CampoQuery = 999
        };

        var resultado = await service.GetAsync(query);

        Assert.Null(resultado);
    }
}
```

## 5. O que esses testes validam

Com os exemplos acima, voce cobre o essencial do servico:

- `CreateAsync` salva a entidade no contexto
- `CreateAsync` copia corretamente os dados do comando
- `GetAsync` retorna o registro quando a chave existe
- `GetAsync` retorna `null` quando a chave nao existe

## 6. Como executar

Na pasta `backend`, execute:

```powershell
dotnet test
```

Se quiser rodar apenas o projeto de testes:

```powershell
dotnet test .\Renova.Tests\Renova.Tests.csproj
```

## 7. Observacao importante sobre `InMemory`

O provider em memoria e bom para testar servicos e fluxos simples, mas ele nao valida varios comportamentos reais de banco relacional, como:

- traducao de consultas SQL
- restricoes mais proximas do banco real
- diferencas de comparacao, ordenacao e nulabilidade
- integridade relacional mais fiel

Para o `RenovaService`, que hoje faz apenas `AddAsync`, `SaveChangesAsync` e `FindAsync`, o `InMemory` atende bem como primeiro nivel de teste.
