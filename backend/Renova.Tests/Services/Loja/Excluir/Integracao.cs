using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.Auth;
using Renova.Tests.Infrastructure;

namespace Renova.Tests.Services.Loja.Excluir
{
    public class Integracao
    {
        [Fact]
        public async Task DeleteLojaDeveRetornarNoContentQuandoLojaNaoPossuirRegistrosAtivos()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.DeleteAsync($"/api/loja/{loja.Id}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();
            Assert.Empty(context.Lojas);
        }

        [Fact]
        public async Task DeleteLojaDeveRetornarConflictQuandoLojaPossuirRegistrosAtivos()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            _ = await CriarClienteAsync(factory, loja.Id, "Cliente A", "44999990000");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.DeleteAsync($"/api/loja/{loja.Id}");
            JsonElement body = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync());

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
            Assert.Equal("Nao e possivel excluir loja com registros ativos", body.GetProperty("mensagem").GetString());

            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();
            _ = await context.Lojas.SingleAsync(item => item.Id == loja.Id);
        }

        [Fact]
        public async Task DeleteLojaDeveRetornarUnauthorizedQuandoLojaNaoPertencerAoUsuarioAutenticado()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto donoDaLoja = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            UsuarioTokenDto outroUsuario = await CriarUsuarioAutenticadoAsync(client, "joao@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, donoDaLoja.Usuario.Id, "Loja Centro");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", outroUsuario.Token);

            HttpResponseMessage response = await client.DeleteAsync($"/api/loja/{loja.Id}");

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

        private static async Task<ClienteModel> CriarClienteAsync(RenovaApiFactory factory, int lojaId, string nome, string contato)
        {
            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();

            ClienteModel cliente = new()
            {
                Nome = nome,
                Contato = contato,
                LojaId = lojaId
            };

            _ = context.Clientes.Add(cliente);
            _ = await context.SaveChangesAsync();
            return cliente;
        }
    }
}
