using Microsoft.Extensions.DependencyInjection;
using Renova.Domain.Model;
using Renova.Persistence;
using Renova.Service.Commands;
using Renova.Tests.Infrastructure;
using System.Net;
using System.Net.Http.Json;

namespace Renova.Tests.Endpoints;

public class Integracao
{
    [Fact]
    public async Task PostRenova_DeveRetornarCreatedEPersistirRegistro()
    {
        await using var factory = new RenovaApiFactory();
        var client = factory.CreateClient();

        var command = new RenovaCommand
        {
            Campo2 = "novo registro",
            Campo3 = 123
        };

        var response = await client.PostAsJsonAsync("/api/renova", command);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<RenovaModel>();

        Assert.NotNull(body);
        Assert.Equal("novo registro", body!.Campo2);
        Assert.Equal(123, body.Campo3);

        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();

        var salvo = await context.Renova.FindAsync(body.Campo1);

        Assert.NotNull(salvo);
        Assert.Equal("novo registro", salvo!.Campo2);
        Assert.Equal(123, salvo.Campo3);
    }

    [Fact]
    public async Task GetRenova_DeveRetornarOkQuandoRegistroExistir()
    {
        await using var factory = new RenovaApiFactory();

        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();

            var entidade = new RenovaModel
            {
                Campo2 = "existente",
                Campo3 = 50
            };

            context.Renova.Add(entidade);
            await context.SaveChangesAsync();

            var client = factory.CreateClient();
            var response = await client.GetAsync($"/api/renova?CampoQuery={entidade.Campo1}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadFromJsonAsync<RenovaModel>();

            Assert.NotNull(body);
            Assert.Equal(entidade.Campo1, body!.Campo1);
            Assert.Equal("existente", body.Campo2);
            Assert.Equal(50, body.Campo3);
        }
    }

    [Fact]
    public async Task GetRenova_DeveRetornarNoContentQuandoRegistroNaoExistir()
    {
        await using var factory = new RenovaApiFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/renova?CampoQuery=999");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
