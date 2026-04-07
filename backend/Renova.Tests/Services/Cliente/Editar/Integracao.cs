using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.Auth;
using Renova.Service.Commands.Cliente;
using Renova.Tests.Infrastructure;

namespace Renova.Tests.Services.Cliente.Editar
{
    public class Integracao
    {
        [Fact]
        // Input: usuario autenticado e payload valido para cliente existente
        // Atualiza cliente da loja do usuario autenticado
        // Retorna: ok com cliente editado
        public async Task PutClienteDeveRetornarOkQuandoUsuarioAutenticadoEnviarPayloadValido()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            ClienteModel cliente = await CriarClienteAsync(factory, loja.Id, "Cliente A", "44999990000");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            EditarClienteCommand command = new()
            {
                Nome = "Cliente Editado",
                Contato = "44888880000"
            };

            HttpResponseMessage response = await client.PutAsJsonAsync($"/api/cliente/{cliente.Id}", command);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            ClienteDto? body = await response.Content.ReadFromJsonAsync<ClienteDto>();

            Assert.NotNull(body);
            Assert.Equal(cliente.Id, body.Id);
            Assert.Equal(command.Nome, body.Nome);
            Assert.Equal(command.Contato, body.Contato);
            Assert.Equal(loja.Id, body.LojaId);

            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();

            ClienteModel clienteSalvo = await context.Clientes.SingleAsync(clienteAtual => clienteAtual.Id == cliente.Id);
            Assert.Equal(command.Nome, clienteSalvo.Nome);
            Assert.Equal(command.Contato, clienteSalvo.Contato);
        }

        [Fact]
        // Input: contato editado com caracteres nao numericos
        // Normaliza antes de salvar pela API
        // Retorna: ok com contato somente numerico
        public async Task PutClienteDeveNormalizarContatoQuandoPayloadPossuirMascara()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            ClienteModel cliente = await CriarClienteAsync(factory, loja.Id, "Cliente A", "44999990000");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            EditarClienteCommand command = new()
            {
                Nome = "Cliente Editado",
                Contato = "(44) 98888-7777"
            };

            HttpResponseMessage response = await client.PutAsJsonAsync($"/api/cliente/{cliente.Id}", command);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            ClienteDto? body = await response.Content.ReadFromJsonAsync<ClienteDto>();

            Assert.NotNull(body);
            Assert.Equal("44988887777", body.Contato);
        }

        [Fact]
        // Input: usuario autenticado tentando renomear cliente para nome ja existente na mesma loja
        // Nao salva a alteracao para evitar duplicidade
        // Retorna: conflict
        public async Task PutClienteDeveRetornarConflictQuandoLojaJaPossuirOutroClienteComMesmoNome()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            ClienteModel cliente = await CriarClienteAsync(factory, loja.Id, "Cliente A", "44999990000");
            _ = await CriarClienteAsync(factory, loja.Id, "Cliente B", "44888880000");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            EditarClienteCommand command = new()
            {
                Nome = "Cliente B",
                Contato = "44777770000"
            };

            HttpResponseMessage response = await client.PutAsJsonAsync($"/api/cliente/{cliente.Id}", command);

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();

            ClienteModel clienteSalvo = await context.Clientes.SingleAsync(clienteAtual => clienteAtual.Id == cliente.Id);
            Assert.Equal("Cliente A", clienteSalvo.Nome);
            Assert.Equal("44999990000", clienteSalvo.Contato);
        }

        [Fact]
        // Input: usuario autenticado mantendo o mesmo nome do cliente editado
        // Permite a alteracao dos demais campos sem conflito
        // Retorna: ok
        public async Task PutClienteDeveRetornarOkQuandoClienteMantiverOProprioNome()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            ClienteModel cliente = await CriarClienteAsync(factory, loja.Id, "Cliente A", "44999990000");
            _ = await CriarClienteAsync(factory, loja.Id, "Cliente B", "44888880000");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            EditarClienteCommand command = new()
            {
                Nome = "Cliente A",
                Contato = "44777770000"
            };

            HttpResponseMessage response = await client.PutAsJsonAsync($"/api/cliente/{cliente.Id}", command);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            ClienteDto? body = await response.Content.ReadFromJsonAsync<ClienteDto>();

            Assert.NotNull(body);
            Assert.Equal("Cliente A", body.Nome);
            Assert.Equal(command.Contato, body.Contato);
        }

        [Fact]
        // Input: payload valido sem usuario autenticado
        // Nao salva alteracoes
        // Retorna: unauthorized
        public async Task PutClienteDeveRetornarUnauthorizedQuandoUsuarioNaoEstiverAutenticado()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            ClienteModel cliente = await CriarClienteAsync(factory, loja.Id, "Cliente A", "44999990000");

            EditarClienteCommand command = new()
            {
                Nome = "Cliente Editado",
                Contato = "44888880000"
            };

            HttpResponseMessage response = await client.PutAsJsonAsync($"/api/cliente/{cliente.Id}", command);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        // Input: usuario autenticado tentando editar cliente de loja de outro usuario
        // Nao salva alteracoes
        // Retorna: unauthorized
        public async Task PutClienteDeveRetornarUnauthorizedQuandoLojaNaoPertencerAoUsuarioAutenticado()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto donoDaLoja = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            UsuarioTokenDto outroUsuario = await CriarUsuarioAutenticadoAsync(client, "joao@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, donoDaLoja.Usuario.Id, "Loja Centro");
            ClienteModel cliente = await CriarClienteAsync(factory, loja.Id, "Cliente A", "44999990000");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", outroUsuario.Token);

            EditarClienteCommand command = new()
            {
                Nome = "Cliente Editado",
                Contato = "44888880000"
            };

            HttpResponseMessage response = await client.PutAsJsonAsync($"/api/cliente/{cliente.Id}", command);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        // Input: payload invalido para cliente existente
        // Nao salva alteracoes
        // Retorna: bad request
        public async Task PutClienteDeveRetornarBadRequestQuandoPayloadForInvalido()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            ClienteModel cliente = await CriarClienteAsync(factory, loja.Id, "Cliente A", "44999990000");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            EditarClienteCommand command = new()
            {
                Nome = string.Empty,
                Contato = string.Empty
            };

            HttpResponseMessage response = await client.PutAsJsonAsync($"/api/cliente/{cliente.Id}", command);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        // Input: cliente inexistente
        // Nao salva alteracoes
        // Retorna: not found
        public async Task PutClienteDeveRetornarNotFoundQuandoClienteNaoForEncontrado()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            EditarClienteCommand command = new()
            {
                Nome = "Cliente Editado",
                Contato = "44888880000"
            };

            HttpResponseMessage response = await client.PutAsJsonAsync("/api/cliente/999", command);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
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
