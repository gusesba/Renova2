using System.Net;
using System.Net.Http.Json;

using Microsoft.Extensions.DependencyInjection;

using Renova.Domain.Model;
using Renova.Persistence;
using Renova.Service.Commands.Renova;
using Renova.Tests.Infrastructure;

namespace Renova.Tests.Services.Renova
{
    public class Integracao
    {
        [Fact]
        public async Task PostRenovaDeveRetornarCreatedEPersistirRegistro()
        {
            await using var factory = new RenovaApiFactory();
            HttpClient client = factory.CreateClient();

            var command = new RenovaCommand
            {
                Campo2 = "novo registro",
                Campo3 = 123
            };

            HttpResponseMessage response = await client.PostAsJsonAsync("/api/renova", command);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            RenovaModel? body = await response.Content.ReadFromJsonAsync<RenovaModel>();

            Assert.NotNull(body);
            Assert.Equal("novo registro", body.Campo2);
            Assert.Equal(123, body.Campo3);

            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();

            RenovaModel? salvo = await context.Renova.FindAsync(body.Campo1);

            Assert.NotNull(salvo);
            Assert.Equal("novo registro", salvo.Campo2);
            Assert.Equal(123, salvo.Campo3);
        }

        [Fact]
        public async Task GetRenovaDeveRetornarOkQuandoRegistroExistir()
        {
            await using var factory = new RenovaApiFactory();

            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();

            var entidade = new RenovaModel
            {
                Campo2 = "existente",
                Campo3 = 50
            };

            _ = context.Renova.Add(entidade);
            _ = await context.SaveChangesAsync();

            HttpClient client = factory.CreateClient();
            HttpResponseMessage response = await client.GetAsync($"/api/renova?CampoQuery={entidade.Campo1}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            RenovaModel? body = await response.Content.ReadFromJsonAsync<RenovaModel>();

            Assert.NotNull(body);
            Assert.Equal(entidade.Campo1, body.Campo1);
            Assert.Equal("existente", body.Campo2);
            Assert.Equal(50, body.Campo3);
        }

        [Fact]
        public async Task GetRenovaDeveRetornarNoContentQuandoRegistroNaoExistir()
        {
            await using var factory = new RenovaApiFactory();
            HttpClient client = factory.CreateClient();

            HttpResponseMessage response = await client.GetAsync("/api/renova?CampoQuery=999");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }
    }
}