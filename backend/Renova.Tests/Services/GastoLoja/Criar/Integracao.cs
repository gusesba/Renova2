using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using Microsoft.Extensions.DependencyInjection;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.Auth;
using Renova.Service.Commands.GastoLoja;
using Renova.Tests.Infrastructure;

namespace Renova.Tests.Services.GastoLoja.Criar
{
    public class Integracao
    {
        [Fact]
        public async Task PostGastoLojaDeveRetornarCreatedQuandoComandoForValido()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "gasto-loja-api@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.PostAsJsonAsync("/api/gasto-loja", new CriarGastoLojaCommand
            {
                LojaId = loja.Id,
                Natureza = NaturezaGastoLoja.Pagamento,
                Valor = 89.9m,
                Data = new DateTime(2026, 4, 14, 0, 0, 0, DateTimeKind.Utc),
                Descricao = "Reforma do telhado"
            });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            GastoLojaDto? body = await response.Content.ReadFromJsonAsync<GastoLojaDto>();
            body = Assert.IsType<GastoLojaDto>(body);
            Assert.Equal(NaturezaGastoLoja.Pagamento, body.Natureza);
            Assert.Equal("Reforma do telhado", body.Descricao);

            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();
            GastoLojaModel gasto = Assert.Single(context.GastosLoja);
            Assert.Equal(89.9m, gasto.Valor);
        }

        private static async Task<UsuarioTokenDto> CriarUsuarioAutenticadoAsync(HttpClient client, string email)
        {
            HttpResponseMessage response = await client.PostAsJsonAsync("/api/auth/cadastro", new CadastroCommand
            {
                Nome = "Usuario de Teste",
                Email = email,
                Senha = "Senha@123"
            });

            _ = response.EnsureSuccessStatusCode();
            UsuarioTokenDto? resultado = await response.Content.ReadFromJsonAsync<UsuarioTokenDto>();
            return Assert.IsType<UsuarioTokenDto>(resultado);
        }

        private static async Task<LojaModel> CriarLojaAsync(RenovaApiFactory factory, int usuarioId, string nome)
        {
            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();

            LojaModel loja = new()
            {
                Nome = nome,
                UsuarioId = usuarioId
            };

            _ = context.Lojas.Add(loja);
            _ = await context.SaveChangesAsync();
            return loja;
        }
    }
}
