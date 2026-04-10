using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using Microsoft.Extensions.DependencyInjection;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.Auth;
using Renova.Service.Commands.Loja;
using Renova.Tests.Infrastructure;

namespace Renova.Tests.Services.Loja.Editar
{
    public class Integracao
    {
        [Fact]
        public async Task PutLojaDeveRetornarOkQuandoUsuarioAutenticadoEnviarPayloadValido()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.PutAsJsonAsync($"/api/loja/{loja.Id}", new EditarLojaCommand
            {
                Nome = "Loja Premium"
            });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            LojaDto? body = await response.Content.ReadFromJsonAsync<LojaDto>();
            Assert.NotNull(body);
            Assert.Equal(loja.Id, body.Id);
            Assert.Equal("Loja Premium", body.Nome);
        }

        [Fact]
        public async Task PutLojaDeveRetornarConflictQuandoUsuarioJaPossuirOutraLojaComMesmoNome()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            _ = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Bairro");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.PutAsJsonAsync($"/api/loja/{loja.Id}", new EditarLojaCommand
            {
                Nome = "Loja Bairro"
            });

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task PutLojaDeveRetornarUnauthorizedQuandoLojaNaoPertencerAoUsuarioAutenticado()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto donoDaLoja = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            UsuarioTokenDto outroUsuario = await CriarUsuarioAutenticadoAsync(client, "joao@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, donoDaLoja.Usuario.Id, "Loja Centro");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", outroUsuario.Token);

            HttpResponseMessage response = await client.PutAsJsonAsync($"/api/loja/{loja.Id}", new EditarLojaCommand
            {
                Nome = "Loja Premium"
            });

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
