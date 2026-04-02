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

namespace Renova.Tests.Services.Loja.Criar
{
    public class Integracao
    {
        [Fact]
        //Input: usuario autenticado e payload valido
        //Grava loja vinculada ao usuario autenticado
        //Retorna: created com id e nome
        public async Task PostLojaDeveRetornarCreatedQuandoUsuarioAutenticadoEnviarPayloadValido()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            CriarLojaCommand command = new()
            {
                Nome = "Loja Centro"
            };

            HttpResponseMessage response = await client.PostAsJsonAsync("/api/loja", command);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            LojaDto? body = await response.Content.ReadFromJsonAsync<LojaDto>();

            Assert.NotNull(body);
            Assert.True(body.Id > 0);
            Assert.Equal(command.Nome, body.Nome);

            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();

            LojaModel lojaSalva = Assert.Single(context.Lojas.Where(loja => loja.UsuarioId == autenticacao.Usuario.Id && loja.Nome == command.Nome));
            Assert.Equal(body.Id, lojaSalva.Id);
            Assert.Equal(command.Nome, lojaSalva.Nome);
        }

        [Fact]
        //Input: usuario autenticado com loja de mesmo nome ja cadastrada
        //Nao grava nova loja duplicada para o mesmo usuario
        //Retorna: conflict
        public async Task PostLojaDeveRetornarConflictQuandoUsuarioJaPossuirLojaComMesmoNome()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            CriarLojaCommand command = new()
            {
                Nome = "Loja Centro"
            };

            HttpResponseMessage response = await client.PostAsJsonAsync("/api/loja", command);

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();

            _ = Assert.Single(context.Lojas.Where(loja => loja.UsuarioId == autenticacao.Usuario.Id && loja.Nome == command.Nome));
        }

        [Fact]
        //Input: payload valido sem usuario autenticado
        //Nao grava loja
        //Retorna: unauthorized
        public async Task PostLojaDeveRetornarUnauthorizedQuandoUsuarioNaoEstiverAutenticado()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            CriarLojaCommand command = new()
            {
                Nome = "Loja Centro"
            };

            HttpResponseMessage response = await client.PostAsJsonAsync("/api/loja", command);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();

            Assert.Empty(context.Lojas.Where(loja => loja.Nome == command.Nome));
        }

        [Fact]
        //Input: usuario autenticado com payload invalido
        //Nao grava loja
        //Retorna: bad request
        public async Task PostLojaDeveRetornarBadRequestQuandoPayloadForInvalido()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            CriarLojaCommand command = new()
            {
                Nome = string.Empty
            };

            HttpResponseMessage response = await client.PostAsJsonAsync("/api/loja", command);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();

            Assert.Empty(context.Lojas.Where(loja => loja.UsuarioId == autenticacao.Usuario.Id));
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