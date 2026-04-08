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

namespace Renova.Tests.Services.Cliente.Excluir
{
    public class Integracao
    {
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

        [Fact]
        // Input: cliente referenciado como fornecedor em produto
        // Nao remove o cliente pela API enquanto existir produto relacionado
        // Retorna: conflict com mensagem adequada
        public async Task DeleteClienteDeveRetornarConflictQuandoClienteEstiverRelacionadoAProduto()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            ClienteModel cliente = await CriarClienteAsync(factory, loja.Id, "Cliente A", "44999990000");
            ProdutoReferenciaModel produto = await CriarProdutoReferenciaAsync(factory, loja.Id, "Vestido");
            MarcaModel marca = await CriarMarcaAsync(factory, loja.Id, "Farm");
            TamanhoModel tamanho = await CriarTamanhoAsync(factory, loja.Id, "M");
            CorModel cor = await CriarCorAsync(factory, loja.Id, "Azul");
            await CriarProdutoAsync(factory, loja.Id, cliente.Id, produto.Id, marca.Id, tamanho.Id, cor.Id);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.DeleteAsync($"/api/cliente/{cliente.Id}");
            JsonElement body = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync());

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
            Assert.Equal("Cliente possui produtos vinculados e nao pode ser excluido.", body.GetProperty("mensagem").GetString());

            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();
            _ = Assert.Single(context.Clientes);
            _ = Assert.Single(context.ProdutosEstoque.Where(produtoAtual => produtoAtual.FornecedorId == cliente.Id));
        }

        [Fact]
        // Input: cliente referenciado em movimentacao
        // Nao remove o cliente pela API enquanto existir movimentacao relacionada
        // Retorna: conflict com mensagem adequada
        public async Task DeleteClienteDeveRetornarConflictQuandoClienteEstiverRelacionadoAMovimentacao()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            ClienteModel cliente = await CriarClienteAsync(factory, loja.Id, "Cliente A", "44999990000");
            await CriarMovimentacaoAsync(factory, loja.Id, cliente.Id);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.DeleteAsync($"/api/cliente/{cliente.Id}");
            JsonElement body = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync());

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
            Assert.Equal("Cliente possui movimentacoes vinculadas e nao pode ser excluido.", body.GetProperty("mensagem").GetString());

            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();
            _ = Assert.Single(context.Clientes);
            _ = await context.Movimentacoes.SingleAsync(movimentacao => movimentacao.ClienteId == cliente.Id);
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

        private static async Task<ProdutoReferenciaModel> CriarProdutoReferenciaAsync(RenovaApiFactory factory, int lojaId, string valor)
        {
            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();

            ProdutoReferenciaModel entity = new()
            {
                Valor = valor,
                LojaId = lojaId
            };

            _ = context.ProdutosReferencia.Add(entity);
            _ = await context.SaveChangesAsync();

            return entity;
        }

        private static async Task<MarcaModel> CriarMarcaAsync(RenovaApiFactory factory, int lojaId, string valor)
        {
            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();

            MarcaModel entity = new()
            {
                Valor = valor,
                LojaId = lojaId
            };

            _ = context.Marcas.Add(entity);
            _ = await context.SaveChangesAsync();

            return entity;
        }

        private static async Task<TamanhoModel> CriarTamanhoAsync(RenovaApiFactory factory, int lojaId, string valor)
        {
            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();

            TamanhoModel entity = new()
            {
                Valor = valor,
                LojaId = lojaId
            };

            _ = context.Tamanhos.Add(entity);
            _ = await context.SaveChangesAsync();

            return entity;
        }

        private static async Task<CorModel> CriarCorAsync(RenovaApiFactory factory, int lojaId, string valor)
        {
            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();

            CorModel entity = new()
            {
                Valor = valor,
                LojaId = lojaId
            };

            _ = context.Cores.Add(entity);
            _ = await context.SaveChangesAsync();

            return entity;
        }

        private static async Task CriarProdutoAsync(RenovaApiFactory factory, int lojaId, int fornecedorId, int produtoId, int marcaId, int tamanhoId, int corId)
        {
            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();

            ProdutoEstoqueModel entity = new()
            {
                Preco = 100m,
                ProdutoId = produtoId,
                MarcaId = marcaId,
                TamanhoId = tamanhoId,
                CorId = corId,
                FornecedorId = fornecedorId,
                Descricao = "Produto vinculado",
                Entrada = DateTime.UtcNow,
                LojaId = lojaId,
                Situacao = SituacaoProduto.Estoque,
                Consignado = false
            };

            _ = context.ProdutosEstoque.Add(entity);
            _ = await context.SaveChangesAsync();
        }

        private static async Task CriarMovimentacaoAsync(RenovaApiFactory factory, int lojaId, int clienteId)
        {
            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();

            MovimentacaoModel entity = new()
            {
                Tipo = TipoMovimentacao.Venda,
                Data = DateTime.UtcNow,
                LojaId = lojaId,
                ClienteId = clienteId
            };

            _ = context.Movimentacoes.Add(entity);
            _ = await context.SaveChangesAsync();
        }
    }
}
