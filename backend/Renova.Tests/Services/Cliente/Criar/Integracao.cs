using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using Microsoft.Extensions.DependencyInjection;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.Auth;
using Renova.Service.Commands.Cliente;
using Renova.Tests.Infrastructure;

namespace Renova.Tests.Services.Cliente.Criar
{
    public class Integracao
    {
        [Fact]
        // Input: usuario autenticado e payload valido
        // Grava cliente na loja do usuario autenticado
        // Retorna: created com cliente criado
        public async Task PostClienteDeveRetornarCreatedQuandoUsuarioAutenticadoEnviarPayloadValido()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            CriarClienteCommand command = new()
            {
                Nome = "Cliente A",
                Contato = "44999990000",
                LojaId = loja.Id
            };

            HttpResponseMessage response = await client.PostAsJsonAsync("/api/cliente", command);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            ClienteDto? body = await response.Content.ReadFromJsonAsync<ClienteDto>();

            Assert.NotNull(body);
            Assert.True(body.Id > 0);
            Assert.Equal(command.Nome, body.Nome);
            Assert.Equal(command.Contato, body.Contato);
            Assert.Equal(command.LojaId, body.LojaId);
            Assert.Null(body.UserId);

            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();

            ClienteModel clienteSalvo = Assert.Single(context.Clientes.Where(cliente => cliente.LojaId == loja.Id && cliente.Nome == command.Nome));
            Assert.Equal(body.Id, clienteSalvo.Id);
            Assert.Equal(command.Contato, clienteSalvo.Contato);
        }

        [Fact]
        // Input: contato com caracteres nao numericos
        // Normaliza antes de salvar pela API
        // Retorna: created com contato somente numerico
        public async Task PostClienteDeveNormalizarContatoQuandoPayloadPossuirMascara()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            CriarClienteCommand command = new()
            {
                Nome = "Cliente A",
                Contato = "(44) 9 9999-0000",
                LojaId = loja.Id
            };

            HttpResponseMessage response = await client.PostAsJsonAsync("/api/cliente", command);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            ClienteDto? body = await response.Content.ReadFromJsonAsync<ClienteDto>();

            Assert.NotNull(body);
            Assert.Equal("44999990000", body.Contato);
        }

        [Fact]
        // Input: usuario autenticado com cliente de mesmo nome na mesma loja
        // Nao grava cliente duplicado
        // Retorna: conflict
        public async Task PostClienteDeveRetornarConflictQuandoLojaJaPossuirClienteComMesmoNome()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            await CriarClienteAsync(factory, loja.Id, "Cliente A", "44999990000");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            CriarClienteCommand command = new()
            {
                Nome = "Cliente A",
                Contato = "44888880000",
                LojaId = loja.Id
            };

            HttpResponseMessage response = await client.PostAsJsonAsync("/api/cliente", command);

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();

            _ = Assert.Single(context.Clientes.Where(cliente => cliente.LojaId == loja.Id && cliente.Nome == command.Nome));
        }

        [Fact]
        // Input: payload valido sem usuario autenticado
        // Nao grava cliente
        // Retorna: unauthorized
        public async Task PostClienteDeveRetornarUnauthorizedQuandoUsuarioNaoEstiverAutenticado()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");

            CriarClienteCommand command = new()
            {
                Nome = "Cliente A",
                Contato = "44999990000",
                LojaId = loja.Id
            };

            HttpResponseMessage response = await client.PostAsJsonAsync("/api/cliente", command);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();

            Assert.Empty(context.Clientes.Where(cliente => cliente.LojaId == loja.Id));
        }

        [Fact]
        // Input: usuario autenticado com payload invalido
        // Nao grava cliente
        // Retorna: bad request
        public async Task PostClienteDeveRetornarBadRequestQuandoPayloadForInvalido()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            CriarClienteCommand command = new()
            {
                Nome = string.Empty,
                Contato = string.Empty,
                LojaId = loja.Id
            };

            HttpResponseMessage response = await client.PostAsJsonAsync("/api/cliente", command);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();

            Assert.Empty(context.Clientes.Where(cliente => cliente.LojaId == loja.Id));
        }

        [Fact]
        // Input: usuario autenticado tentando criar cliente em loja de outro usuario
        // Nao grava cliente
        // Retorna: unauthorized
        public async Task PostClienteDeveRetornarUnauthorizedQuandoLojaNaoPertencerAoUsuarioAutenticado()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto donoDaLoja = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            UsuarioTokenDto outroUsuario = await CriarUsuarioAutenticadoAsync(client, "joao@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, donoDaLoja.Usuario.Id, "Loja Centro");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", outroUsuario.Token);

            CriarClienteCommand command = new()
            {
                Nome = "Cliente A",
                Contato = "44999990000",
                LojaId = loja.Id
            };

            HttpResponseMessage response = await client.PostAsJsonAsync("/api/cliente", command);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();

            Assert.Empty(context.Clientes.Where(cliente => cliente.LojaId == loja.Id));
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

        private static async Task CriarClienteAsync(RenovaApiFactory factory, int lojaId, string nome, string contato)
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
        }
    }
}
