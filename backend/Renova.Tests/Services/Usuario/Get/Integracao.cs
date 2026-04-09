using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using Renova.Domain.Model.Dto;
using Renova.Service.Commands.Auth;
using Renova.Tests.Infrastructure;

namespace Renova.Tests.Services.Usuario.Get
{
    public class Integracao
    {
        [Fact]
        public async Task GetUsuariosDeveFiltrarPorNomeOuEmailQuandoBuscaForInformada()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "Maria Teste", "maria@renova.com");
            _ = await CriarUsuarioAutenticadoAsync(client, "Ana Paula", "ana@renova.com");
            _ = await CriarUsuarioAutenticadoAsync(client, "Carlos Lima", "carlos@dominio.com");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.GetAsync("/api/usuario?busca=dominio.com");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            PaginacaoDto<UsuarioDto>? body = await response.Content.ReadFromJsonAsync<PaginacaoDto<UsuarioDto>>();

            Assert.NotNull(body);
            UsuarioDto item = Assert.Single(body.Itens);
            Assert.Equal("Carlos Lima", item.Nome);
        }

        [Fact]
        public async Task GetUsuariosDeveRetornarUnauthorizedQuandoUsuarioNaoEstiverAutenticado()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            HttpResponseMessage response = await client.GetAsync("/api/usuario");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
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
