using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using Microsoft.Extensions.DependencyInjection;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.Auth;
using Renova.Tests.Infrastructure;

namespace Renova.Tests.Services.Loja.Get
{
    public class Integracao
    {
        [Fact]
        //Input: usuario autenticado com lojas cadastradas
        //Retorna apenas as lojas vinculadas ao usuario autenticado
        //Retorna: ok com lista de lojas
        public async Task GetLojasDeveRetornarOkComLojasDoUsuarioAutenticado()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            UsuarioTokenDto outroUsuario = await CriarUsuarioAutenticadoAsync(client, "joao@renova.com");

            await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Bairro");
            await CriarLojaAsync(factory, outroUsuario.Usuario.Id, "Loja Externa");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.GetAsync("/api/loja");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            List<LojaDto>? body = await response.Content.ReadFromJsonAsync<List<LojaDto>>();

            Assert.NotNull(body);
            Assert.Collection(body,
                loja => Assert.Equal("Loja Bairro", loja.Nome),
                loja => Assert.Equal("Loja Centro", loja.Nome));
        }

        [Fact]
        //Input: usuario autenticado sem lojas cadastradas
        //Nao retorna lojas de outros usuarios
        //Retorna: ok com lista vazia
        public async Task GetLojasDeveRetornarOkComListaVaziaQuandoUsuarioNaoPossuirLojas()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            UsuarioTokenDto outroUsuario = await CriarUsuarioAutenticadoAsync(client, "joao@renova.com");

            await CriarLojaAsync(factory, outroUsuario.Usuario.Id, "Loja Externa");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.GetAsync("/api/loja");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            List<LojaDto>? body = await response.Content.ReadFromJsonAsync<List<LojaDto>>();

            Assert.NotNull(body);
            Assert.Empty(body);
        }

        [Fact]
        //Input: requisicao sem usuario autenticado
        //Nao retorna lojas
        //Retorna: unauthorized
        public async Task GetLojasDeveRetornarUnauthorizedQuandoUsuarioNaoEstiverAutenticado()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            HttpResponseMessage response = await client.GetAsync("/api/loja");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        private static async Task<UsuarioTokenDto> CriarUsuarioAutenticadoAsync(HttpClient client, string email)
        {
            CadastroCommand command = new()
            {
                Nome = "Usuario de Teste",
                Email = email,
                Senha = "Senha@123"
            };

            HttpResponseMessage response = await client.PostAsJsonAsync("/api/auth/cadastro", command);

            _ = response.EnsureSuccessStatusCode();

            UsuarioTokenDto? resultado = await response.Content.ReadFromJsonAsync<UsuarioTokenDto>();

            return Assert.IsType<UsuarioTokenDto>(resultado);
        }

        private static async Task CriarLojaAsync(RenovaApiFactory factory, int usuarioId, string nome)
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
        }
    }
}