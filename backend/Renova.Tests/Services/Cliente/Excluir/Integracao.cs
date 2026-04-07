using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using Microsoft.Extensions.DependencyInjection;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.Auth;
using Renova.Tests.Infrastructure;

namespace Renova.Tests.Services.Cliente.Excluir
{
    public class Integracao
    {
        private const string RelacionamentosAtivosNaoImplementados = "Relacionamentos ativos de cliente ainda nao implementados.";

        [Fact]
        // Input: usuario autenticado e cliente existente na propria loja
        // Remove o cliente via API
        // Retorna: no content
        public async Task DeleteClienteDeveRetornarNoContentQuandoUsuarioAutenticadoExcluirClienteDaPropriaLoja()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            ClienteModel cliente = await CriarClienteAsync(factory, loja.Id, "Cliente A", "44999990000");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.DeleteAsync($"/api/cliente/{cliente.Id}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();
            Assert.Empty(context.Clientes);
        }

        [Fact]
        // Input: requisicao de exclusao sem usuario autenticado
        // Nao remove o cliente
        // Retorna: unauthorized
        public async Task DeleteClienteDeveRetornarUnauthorizedQuandoUsuarioNaoEstiverAutenticado()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            ClienteModel cliente = await CriarClienteAsync(factory, loja.Id, "Cliente A", "44999990000");

            HttpResponseMessage response = await client.DeleteAsync($"/api/cliente/{cliente.Id}");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();
            _ = Assert.Single(context.Clientes);
        }

        [Fact]
        // Input: usuario autenticado tentando excluir cliente de loja de outro usuario
        // Nao remove o cliente
        // Retorna: unauthorized
        public async Task DeleteClienteDeveRetornarUnauthorizedQuandoLojaNaoPertencerAoUsuarioAutenticado()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto donoDaLoja = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            UsuarioTokenDto outroUsuario = await CriarUsuarioAutenticadoAsync(client, "joao@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, donoDaLoja.Usuario.Id, "Loja Centro");
            ClienteModel cliente = await CriarClienteAsync(factory, loja.Id, "Cliente A", "44999990000");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", outroUsuario.Token);

            HttpResponseMessage response = await client.DeleteAsync($"/api/cliente/{cliente.Id}");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();
            _ = Assert.Single(context.Clientes);
        }

        [Fact]
        // Input: cliente inexistente
        // Nao remove registro algum
        // Retorna: not found
        public async Task DeleteClienteDeveRetornarNotFoundQuandoClienteNaoForEncontrado()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.DeleteAsync("/api/cliente/999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact(Skip = RelacionamentosAtivosNaoImplementados)]
        // Input: cliente com relacionamentos ativos
        // Nao remove o cliente pela API enquanto houver dependencias de negocio
        // Retorna: conflict com mensagem adequada
        public async Task DeleteClienteDeveRetornarConflictQuandoClientePossuirRelacionamentosAtivos()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            // Arrange
            // TODO: autenticar usuario, criar cliente e dependencias ativas nas futuras tabelas relacionadas.
            // TODO: exemplos esperados: pedidos em aberto, fiados ativos, agendamentos pendentes ou contratos vinculados.
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "token-placeholder");

            // Act
            // TODO: executar DELETE /api/cliente/{id}.

            // Assert
            // TODO: validar HttpStatusCode.Conflict.
            // TODO: validar mensagem de negocio adequada no corpo da resposta.
            // TODO: sugestao de mensagem: "Cliente possui relacionamentos ativos e nao pode ser excluido."
            // TODO: validar que o cliente permanece salvo na base.
            await Task.CompletedTask;
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
