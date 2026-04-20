using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using Microsoft.Extensions.DependencyInjection;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.Auth;
using Renova.Service.Commands.Solicitacao;
using Renova.Tests.Infrastructure;

namespace Renova.Tests.Services.Solicitacao
{
    public class Integracao
    {
        [Fact]
        public async Task PostSolicitacaoDeveRetornarProdutosCompativeisQuandoHouverMatch()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "solicitacao@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            ClienteModel cliente = await CriarClienteAsync(factory, loja.Id, "Cliente A", "44999990000");
            ProdutoReferenciaModel produto = await CriarProdutoReferenciaAsync(factory, loja.Id, "Vestido");
            MarcaModel marca = await CriarMarcaAsync(factory, loja.Id, "Farm");
            TamanhoModel tamanho = await CriarTamanhoAsync(factory, loja.Id, "M");
            CorModel cor = await CriarCorAsync(factory, loja.Id, "Azul");
            _ = await CriarProdutoAsync(factory, loja.Id, produto.Id, marca.Id, tamanho.Id, cor.Id, cliente.Id);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.PostAsJsonAsync("/api/solicitacao", new CriarSolicitacaoCommand
            {
                ProdutoId = produto.Id,
                MarcaId = marca.Id,
                TamanhoId = tamanho.Id,
                CorId = cor.Id,
                ClienteId = cliente.Id,
                Descricao = "Procura vestido azul",
                PrecoMaximo = 200m,
                LojaId = loja.Id
            });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            SolicitacaoDto? body = await response.Content.ReadFromJsonAsync<SolicitacaoDto>();
            Assert.NotNull(body);
            ProdutoCompativelDto produtoCompativel = Assert.Single(body.ProdutosCompativeis);
            Assert.Equal("Vestido azul", produtoCompativel.Descricao);
        }

        [Fact]
        public async Task PostSolicitacaoDevePermitirCamposOpcionaisEConsiderarWildcardNoMatch()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "solicitacao-opcional@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            ClienteModel cliente = await CriarClienteAsync(factory, loja.Id, "Cliente A", "44999990000");
            ProdutoReferenciaModel produto = await CriarProdutoReferenciaAsync(factory, loja.Id, "Vestido");
            MarcaModel marca = await CriarMarcaAsync(factory, loja.Id, "Farm");
            TamanhoModel tamanho = await CriarTamanhoAsync(factory, loja.Id, "M");
            CorModel cor = await CriarCorAsync(factory, loja.Id, "Azul");
            _ = await CriarProdutoAsync(factory, loja.Id, produto.Id, marca.Id, tamanho.Id, cor.Id, cliente.Id);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.PostAsJsonAsync("/api/solicitacao", new CriarSolicitacaoCommand
            {
                Descricao = string.Empty,
                LojaId = loja.Id
            });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            SolicitacaoDto? body = await response.Content.ReadFromJsonAsync<SolicitacaoDto>();
            Assert.NotNull(body);
            Assert.Null(body.ProdutoId);
            Assert.Null(body.PrecoMaximo);
            Assert.Single(body.ProdutosCompativeis);
        }

        [Fact]
        public async Task GetSolicitacoesDeveRetornarTabelaExpansivelComProdutosCompativeis()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "solicitacao-lista@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            ClienteModel cliente = await CriarClienteAsync(factory, loja.Id, "Cliente A", "44999990000");
            ProdutoReferenciaModel produto = await CriarProdutoReferenciaAsync(factory, loja.Id, "Vestido");
            MarcaModel marca = await CriarMarcaAsync(factory, loja.Id, "Farm");
            TamanhoModel tamanho = await CriarTamanhoAsync(factory, loja.Id, "M");
            CorModel cor = await CriarCorAsync(factory, loja.Id, "Azul");

            await CriarSolicitacaoAsync(factory, loja.Id, produto.Id, marca.Id, tamanho.Id, cor.Id, cliente.Id);
            _ = await CriarProdutoAsync(factory, loja.Id, produto.Id, marca.Id, tamanho.Id, cor.Id, cliente.Id);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.GetAsync($"/api/solicitacao?lojaId={loja.Id}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            PaginacaoDto<SolicitacaoBuscaDto>? body = await response.Content.ReadFromJsonAsync<PaginacaoDto<SolicitacaoBuscaDto>>();
            Assert.NotNull(body);
            SolicitacaoBuscaDto solicitacao = Assert.Single(body.Itens);
            Assert.Equal("Cliente A", solicitacao.Cliente);
            Assert.Single(solicitacao.ProdutosCompativeis);
        }

        [Fact]
        public async Task DeleteSolicitacaoDeveRemoverRegistro()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "solicitacao-delete@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            ClienteModel cliente = await CriarClienteAsync(factory, loja.Id, "Cliente A", "44999990000");
            ProdutoReferenciaModel produto = await CriarProdutoReferenciaAsync(factory, loja.Id, "Vestido");
            MarcaModel marca = await CriarMarcaAsync(factory, loja.Id, "Farm");
            TamanhoModel tamanho = await CriarTamanhoAsync(factory, loja.Id, "M");
            CorModel cor = await CriarCorAsync(factory, loja.Id, "Azul");
            int solicitacaoId = await CriarSolicitacaoAsync(
                factory,
                loja.Id,
                produto.Id,
                marca.Id,
                tamanho.Id,
                cor.Id,
                cliente.Id);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.DeleteAsync($"/api/solicitacao/{solicitacaoId}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();
            SolicitacaoModel? solicitacao = await context.Solicitacoes.FindAsync(solicitacaoId);
            Assert.Null(solicitacao);
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

        private static async Task<int> CriarSolicitacaoAsync(
            RenovaApiFactory factory,
            int lojaId,
            int produtoId,
            int marcaId,
            int tamanhoId,
            int corId,
            int clienteId)
        {
            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();
            SolicitacaoModel solicitacao = new()
            {
                ProdutoId = produtoId,
                MarcaId = marcaId,
                TamanhoId = tamanhoId,
                CorId = corId,
                ClienteId = clienteId,
                Descricao = "Procura vestido azul",
                PrecoMinimo = 100m,
                PrecoMaximo = 200m,
                LojaId = lojaId
            };
            _ = context.Solicitacoes.Add(solicitacao);
            _ = await context.SaveChangesAsync();
            return solicitacao.Id;
        }

        private static async Task<ProdutoEstoqueModel> CriarProdutoAsync(
            RenovaApiFactory factory,
            int lojaId,
            int produtoId,
            int marcaId,
            int tamanhoId,
            int corId,
            int fornecedorId)
        {
            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();
            ProdutoEstoqueModel produto = new()
            {
                Preco = 149.90m,
                ProdutoId = produtoId,
                MarcaId = marcaId,
                TamanhoId = tamanhoId,
                CorId = corId,
                FornecedorId = fornecedorId,
                Descricao = "Vestido azul",
                Entrada = new DateTime(2026, 4, 7, 12, 0, 0, DateTimeKind.Utc),
                LojaId = lojaId,
                Situacao = SituacaoProduto.Estoque,
                Consignado = false
            };

            _ = context.ProdutosEstoque.Add(produto);
            _ = await context.SaveChangesAsync();
            return produto;
        }
    }
}
