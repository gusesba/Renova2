using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.Auth;
using Renova.Service.Commands.Movimentacao;
using Renova.Tests.Infrastructure;

namespace Renova.Tests.Services.Movimentacao.Destinacao
{
    public class Integracao
    {
        [Fact]
        public async Task GetDestinacaoDeveRetornarProdutosElegiveis()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria-destinacao-get@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            ClienteModel fornecedor = await CriarClienteAsync(factory, loja.Id, "Fornecedor A", "44999990001", true);
            _ = await CriarConfigLojaAsync(factory, loja.Id, 3);
            ProdutoEstoqueModel produtoElegivel = await CriarProdutoAsync(factory, loja.Id, fornecedor.Id, "Produto Elegivel", new DateTime(2025, 10, 1, 0, 0, 0, DateTimeKind.Utc), SituacaoProduto.Estoque);
            _ = await CriarProdutoAsync(factory, loja.Id, fornecedor.Id, "Produto Recente", DateTime.UtcNow.AddDays(-10), SituacaoProduto.Estoque);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.GetAsync($"/api/movimentacao/doacao-devolucao?lojaId={loja.Id}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            MovimentacaoDestinacaoSugestaoDto? body = await response.Content.ReadFromJsonAsync<MovimentacaoDestinacaoSugestaoDto>();
            Assert.NotNull(body);
            Assert.Single(body.Produtos);
            Assert.Equal(produtoElegivel.Id, body.Produtos[0].Id);
            Assert.Equal(TipoMovimentacao.Doacao, body.Produtos[0].TipoSugerido);
        }

        [Fact]
        public async Task PostDestinacaoDeveCriarMovimentosAgrupadosPorFornecedorETipo()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria-destinacao-post@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            ClienteModel fornecedorA = await CriarClienteAsync(factory, loja.Id, "Fornecedor A", "44999990001", false);
            ClienteModel fornecedorB = await CriarClienteAsync(factory, loja.Id, "Fornecedor B", "44999990002", true);
            ProdutoEstoqueModel produtoA1 = await CriarProdutoAsync(factory, loja.Id, fornecedorA.Id, "Produto A1", new DateTime(2025, 10, 1, 0, 0, 0, DateTimeKind.Utc), SituacaoProduto.Estoque);
            ProdutoEstoqueModel produtoA2 = await CriarProdutoAsync(factory, loja.Id, fornecedorA.Id, "Produto A2", DateTime.UtcNow.AddDays(-5), SituacaoProduto.Estoque);
            ProdutoEstoqueModel produtoB1 = await CriarProdutoAsync(factory, loja.Id, fornecedorB.Id, "Produto B1", new DateTime(2025, 10, 1, 0, 0, 0, DateTimeKind.Utc), SituacaoProduto.Estoque);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.PostAsJsonAsync("/api/movimentacao/doacao-devolucao", new CriarMovimentacaoDestinacaoCommand
            {
                Data = new DateTime(2026, 4, 9, 12, 0, 0, DateTimeKind.Utc),
                LojaId = loja.Id,
                Itens =
                [
                    new CriarMovimentacaoDestinacaoItemCommand { ProdutoId = produtoA1.Id, Tipo = TipoMovimentacao.DevolucaoDono },
                    new CriarMovimentacaoDestinacaoItemCommand { ProdutoId = produtoA2.Id, Tipo = TipoMovimentacao.Doacao },
                    new CriarMovimentacaoDestinacaoItemCommand { ProdutoId = produtoB1.Id, Tipo = TipoMovimentacao.Doacao }
                ]
            });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            List<MovimentacaoDto>? body = await response.Content.ReadFromJsonAsync<List<MovimentacaoDto>>();
            Assert.NotNull(body);
            Assert.Equal(3, body.Count);

            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();
            Assert.Equal(3, await context.Movimentacoes.CountAsync());
            Assert.Equal(SituacaoProduto.Devolvido, await context.ProdutosEstoque.Where(item => item.Id == produtoA1.Id).Select(item => item.Situacao).SingleAsync());
            Assert.Equal(SituacaoProduto.Doado, await context.ProdutosEstoque.Where(item => item.Id == produtoA2.Id).Select(item => item.Situacao).SingleAsync());
            Assert.Equal(SituacaoProduto.Doado, await context.ProdutosEstoque.Where(item => item.Id == produtoB1.Id).Select(item => item.Situacao).SingleAsync());
        }

        private static async Task<UsuarioTokenDto> CriarUsuarioAutenticadoAsync(HttpClient client, string email)
        {
            HttpResponseMessage response = await client.PostAsJsonAsync("/api/auth/cadastro", new CadastroCommand
            {
                Nome = "Usuario de Teste",
                Email = email,
                Senha = "Senha@123"
            });

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

        private static async Task<ClienteModel> CriarClienteAsync(RenovaApiFactory factory, int lojaId, string nome, string contato, bool doacao)
        {
            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();

            ClienteModel cliente = new()
            {
                Nome = nome,
                Contato = contato,
                LojaId = lojaId,
                Doacao = doacao
            };

            _ = context.Clientes.Add(cliente);
            _ = await context.SaveChangesAsync();
            return cliente;
        }

        private static async Task<ConfigLojaModel> CriarConfigLojaAsync(RenovaApiFactory factory, int lojaId, int tempoPermanenciaProdutoMeses)
        {
            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();

            ConfigLojaModel config = new()
            {
                LojaId = lojaId,
                PercentualRepasseFornecedor = 45m,
                PercentualRepasseVendedorCredito = 45m,
                TempoPermanenciaProdutoMeses = tempoPermanenciaProdutoMeses
            };

            _ = context.ConfiguracoesLoja.Add(config);
            _ = await context.SaveChangesAsync();
            return config;
        }

        private static async Task<ProdutoEstoqueModel> CriarProdutoAsync(
            RenovaApiFactory factory,
            int lojaId,
            int fornecedorId,
            string descricao,
            DateTime entrada,
            SituacaoProduto situacao)
        {
            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();

            ProdutoReferenciaModel produto = new()
            {
                Valor = $"{descricao} Referencia",
                LojaId = lojaId
            };

            MarcaModel marca = new()
            {
                Valor = $"{descricao} Marca",
                LojaId = lojaId
            };

            TamanhoModel tamanho = new()
            {
                Valor = "M",
                LojaId = lojaId
            };

            CorModel cor = new()
            {
                Valor = "Azul",
                LojaId = lojaId
            };

            _ = context.ProdutosReferencia.Add(produto);
            _ = context.Marcas.Add(marca);
            _ = context.Tamanhos.Add(tamanho);
            _ = context.Cores.Add(cor);
            _ = await context.SaveChangesAsync();

            ProdutoEstoqueModel item = new()
            {
                Preco = 149.90m,
                ProdutoId = produto.Id,
                MarcaId = marca.Id,
                TamanhoId = tamanho.Id,
                CorId = cor.Id,
                FornecedorId = fornecedorId,
                Descricao = descricao,
                Entrada = entrada,
                LojaId = lojaId,
                Situacao = situacao,
                Consignado = false
            };

            _ = context.ProdutosEstoque.Add(item);
            _ = await context.SaveChangesAsync();
            return item;
        }
    }
}
