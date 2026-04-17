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

namespace Renova.Tests.Services.Produto.Excluir
{
    public class Integracao
    {
        [Fact]
        public async Task DeleteProdutoDeveRetornarNoContentQuandoUsuarioAutenticadoExcluirProdutoDaPropriaLoja()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            ProdutoReferenciaModel produtoRef = await CriarProdutoReferenciaAsync(factory, loja.Id, "Vestido");
            MarcaModel marca = await CriarMarcaAsync(factory, loja.Id, "Farm");
            TamanhoModel tamanho = await CriarTamanhoAsync(factory, loja.Id, "M");
            CorModel cor = await CriarCorAsync(factory, loja.Id, "Azul");
            ClienteModel fornecedor = await CriarClienteAsync(factory, loja.Id, "Fornecedor A", "44999990000");
            ProdutoEstoqueModel produto = await CriarProdutoAsync(factory, loja.Id, produtoRef.Id, marca.Id, tamanho.Id, cor.Id, fornecedor.Id);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.DeleteAsync($"/api/produto/{produto.Id}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();
            Assert.Empty(context.ProdutosEstoque);
        }

        [Fact]
        public async Task DeleteProdutoDeveRetornarUnauthorizedQuandoUsuarioNaoEstiverAutenticado()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            ProdutoReferenciaModel produtoRef = await CriarProdutoReferenciaAsync(factory, loja.Id, "Vestido");
            MarcaModel marca = await CriarMarcaAsync(factory, loja.Id, "Farm");
            TamanhoModel tamanho = await CriarTamanhoAsync(factory, loja.Id, "M");
            CorModel cor = await CriarCorAsync(factory, loja.Id, "Azul");
            ClienteModel fornecedor = await CriarClienteAsync(factory, loja.Id, "Fornecedor A", "44999990000");
            ProdutoEstoqueModel produto = await CriarProdutoAsync(factory, loja.Id, produtoRef.Id, marca.Id, tamanho.Id, cor.Id, fornecedor.Id);

            HttpResponseMessage response = await client.DeleteAsync($"/api/produto/{produto.Id}");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();
            _ = Assert.Single(context.ProdutosEstoque);
        }

        [Fact]
        public async Task DeleteProdutoDeveRetornarUnauthorizedQuandoLojaNaoPertencerAoUsuarioAutenticado()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto donoDaLoja = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            UsuarioTokenDto outroUsuario = await CriarUsuarioAutenticadoAsync(client, "joao@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, donoDaLoja.Usuario.Id, "Loja Centro");
            ProdutoReferenciaModel produtoRef = await CriarProdutoReferenciaAsync(factory, loja.Id, "Vestido");
            MarcaModel marca = await CriarMarcaAsync(factory, loja.Id, "Farm");
            TamanhoModel tamanho = await CriarTamanhoAsync(factory, loja.Id, "M");
            CorModel cor = await CriarCorAsync(factory, loja.Id, "Azul");
            ClienteModel fornecedor = await CriarClienteAsync(factory, loja.Id, "Fornecedor A", "44999990000");
            ProdutoEstoqueModel produto = await CriarProdutoAsync(factory, loja.Id, produtoRef.Id, marca.Id, tamanho.Id, cor.Id, fornecedor.Id);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", outroUsuario.Token);

            HttpResponseMessage response = await client.DeleteAsync($"/api/produto/{produto.Id}");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();
            _ = Assert.Single(context.ProdutosEstoque);
        }

        [Fact]
        public async Task DeleteProdutoDeveRetornarNotFoundQuandoProdutoNaoForEncontrado()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.DeleteAsync("/api/produto/999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task DeleteProdutoDeveRetornarConflictQuandoProdutoEstiverRelacionadoAMovimentacao()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            ProdutoReferenciaModel produtoRef = await CriarProdutoReferenciaAsync(factory, loja.Id, "Vestido");
            MarcaModel marca = await CriarMarcaAsync(factory, loja.Id, "Farm");
            TamanhoModel tamanho = await CriarTamanhoAsync(factory, loja.Id, "M");
            CorModel cor = await CriarCorAsync(factory, loja.Id, "Azul");
            ClienteModel fornecedor = await CriarClienteAsync(factory, loja.Id, "Fornecedor A", "44999990000");
            ProdutoEstoqueModel produto = await CriarProdutoAsync(factory, loja.Id, produtoRef.Id, marca.Id, tamanho.Id, cor.Id, fornecedor.Id);
            await CriarMovimentacaoComProdutoAsync(factory, loja.Id, produto.Id);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.DeleteAsync($"/api/produto/{produto.Id}");
            JsonElement body = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync());

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
            Assert.Equal("Produto possui movimentacoes vinculadas e nao pode ser excluido.", body.GetProperty("mensagem").GetString());

            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();
            ProdutoEstoqueModel produtoSalvo = await context.ProdutosEstoque.SingleAsync();
            Assert.Equal(produto.Id, produtoSalvo.Id);
            _ = await context.MovimentacoesProdutos.SingleAsync(item => item.ProdutoId == produto.Id);
        }

        [Fact]
        public async Task DeleteMarcaAuxiliarDeveRetornarNoContentQuandoNaoHouverProdutosVinculados()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            MarcaModel marca = await CriarMarcaAsync(factory, loja.Id, "Farm");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.DeleteAsync($"/api/produto/marca/{marca.Id}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();
            Assert.Empty(context.Marcas);
        }

        [Fact]
        public async Task DeleteCorAuxiliarDeveRetornarConflictQuandoExistirProdutoVinculado()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            ProdutoReferenciaModel produtoRef = await CriarProdutoReferenciaAsync(factory, loja.Id, "Vestido");
            MarcaModel marca = await CriarMarcaAsync(factory, loja.Id, "Farm");
            TamanhoModel tamanho = await CriarTamanhoAsync(factory, loja.Id, "M");
            CorModel cor = await CriarCorAsync(factory, loja.Id, "Azul");
            ClienteModel fornecedor = await CriarClienteAsync(factory, loja.Id, "Fornecedor A", "44999990000");
            _ = await CriarProdutoAsync(factory, loja.Id, produtoRef.Id, marca.Id, tamanho.Id, cor.Id, fornecedor.Id);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.DeleteAsync($"/api/produto/cor/{cor.Id}");
            JsonElement body = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync());

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
            Assert.Equal("Cor possui produtos vinculados e nao pode ser excluido.", body.GetProperty("mensagem").GetString());

            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();
            _ = await context.Cores.SingleAsync(item => item.Id == cor.Id);
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

        private static async Task<ProdutoEstoqueModel> CriarProdutoAsync(RenovaApiFactory factory, int lojaId, int produtoId, int marcaId, int tamanhoId, int corId, int fornecedorId)
        {
            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();
            ProdutoEstoqueModel entity = new()
            {
                Preco = 199.90m,
                ProdutoId = produtoId,
                MarcaId = marcaId,
                TamanhoId = tamanhoId,
                CorId = corId,
                FornecedorId = fornecedorId,
                Descricao = "Vestido azul midi",
                Entrada = new DateTime(2026, 4, 7, 12, 0, 0, DateTimeKind.Utc),
                LojaId = lojaId,
                Situacao = SituacaoProduto.Estoque,
                Consignado = true
            };

            _ = context.ProdutosEstoque.Add(entity);
            _ = await context.SaveChangesAsync();
            return entity;
        }

        private static async Task CriarMovimentacaoComProdutoAsync(RenovaApiFactory factory, int lojaId, int produtoId)
        {
            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();

            MovimentacaoModel movimentacao = new()
            {
                Tipo = TipoMovimentacao.Venda,
                Data = new DateTime(2026, 4, 8, 12, 0, 0, DateTimeKind.Utc),
                LojaId = lojaId
            };

            _ = context.Movimentacoes.Add(movimentacao);
            _ = await context.SaveChangesAsync();

            _ = context.MovimentacoesProdutos.Add(new MovimentacaoProdutoModel
            {
                MovimentacaoId = movimentacao.Id,
                ProdutoId = produtoId
            });
            _ = await context.SaveChangesAsync();
        }
    }
}
