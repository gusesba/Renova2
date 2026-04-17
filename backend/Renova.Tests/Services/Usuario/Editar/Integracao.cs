using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using Renova.Domain.Model.Dto;
using Renova.Service.Commands.Auth;
using Renova.Service.Commands.Usuario;
using Renova.Tests.Infrastructure;

namespace Renova.Tests.Services.Usuario.Editar
{
    public class Integracao
    {
        [Fact]
        public async Task PutUsuarioDeveRetornarOkQuandoUsuarioEditarProprioNome()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "Maria Teste", "maria@renova.com");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.PutAsJsonAsync($"/api/usuario/{autenticacao.Usuario.Id}", new EditarUsuarioCommand
            {
                Nome = "Maria Atualizada"
            });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            UsuarioDto? body = await response.Content.ReadFromJsonAsync<UsuarioDto>();
            Assert.NotNull(body);
            Assert.Equal(autenticacao.Usuario.Id, body.Id);
            Assert.Equal("Maria Atualizada", body.Nome);
            Assert.Equal(autenticacao.Usuario.Email, body.Email);
        }

        [Fact]
        public async Task PutUsuarioDeveRetornarUnauthorizedQuandoUsuarioTentarEditarOutroUsuario()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "Maria Teste", "maria@renova.com");
            UsuarioTokenDto outroUsuario = await CriarUsuarioAutenticadoAsync(client, "Joao Teste", "joao@renova.com");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.PutAsJsonAsync($"/api/usuario/{outroUsuario.Usuario.Id}", new EditarUsuarioCommand
            {
                Nome = "Nome Indevido"
            });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task PutUsuarioDeveRetornarNotFoundQuandoUsuarioNaoExistir()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "Maria Teste", "maria@renova.com");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.PutAsJsonAsync("/api/usuario/999", new EditarUsuarioCommand
            {
                Nome = "Nome Novo"
            });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        private static async Task<UsuarioTokenDto> CriarUsuarioAutenticadoAsync(HttpClient client, string nome, string email)
        {
            CadastroCommand command = new()
            {
                Nome = nome,
                Email = email,
                Senha = "Senha@123"
            };

            HttpResponseMessage response = await client.PostAsJsonAsync("/api/auth/cadastro", command);

            _ = response.EnsureSuccessStatusCode();

            UsuarioTokenDto? resultado = await response.Content.ReadFromJsonAsync<UsuarioTokenDto>();
            return Assert.IsType<UsuarioTokenDto>(resultado);
        }
    }
}
