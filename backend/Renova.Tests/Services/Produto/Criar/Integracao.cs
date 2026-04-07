using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using Microsoft.Extensions.DependencyInjection;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.Auth;
using Renova.Service.Commands.Produto;
using Renova.Tests.Infrastructure;

namespace Renova.Tests.Services.Produto.Criar
{
    public class Integracao
    {
        [Fact]
        public async Task PostProdutoDeveRetornarCreatedQuandoUsuarioAutenticadoEnviarPayloadValido()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            ProdutoReferenciaModel produto = await CriarProdutoReferenciaAsync(factory, loja.Id, "Vestido");
            MarcaModel marca = await CriarMarcaAsync(factory, loja.Id, "Farm");
            TamanhoModel tamanho = await CriarTamanhoAsync(factory, loja.Id, "M");
            CorModel cor = await CriarCorAsync(factory, loja.Id, "Azul");
            ClienteModel fornecedor = await CriarClienteAsync(factory, loja.Id, "Fornecedor A", "44999990000");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.PostAsJsonAsync("/api/produto", new CriarProdutoCommand
            {
                Preco = 199.90m,
                ProdutoId = produto.Id,
                MarcaId = marca.Id,
                TamanhoId = tamanho.Id,
                CorId = cor.Id,
                FornecedorId = fornecedor.Id,
                Descricao = "Vestido azul midi",
                Entrada = new DateTime(2026, 4, 7, 12, 0, 0, DateTimeKind.Utc),
                LojaId = loja.Id,
                Situacao = SituacaoProduto.Estoque,
                Consignado = true
            });

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            ProdutoDto? body = await response.Content.ReadFromJsonAsync<ProdutoDto>();
            Assert.NotNull(body);
            Assert.True(body.Id > 0);
            Assert.True(body.Consignado);

            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();
            _ = Assert.Single(context.ProdutosEstoque);
        }

        [Fact]
        public async Task PostProdutoDeveRetornarUnauthorizedQuandoUsuarioNaoEstiverAutenticado()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            ProdutoReferenciaModel produto = await CriarProdutoReferenciaAsync(factory, loja.Id, "Vestido");
            MarcaModel marca = await CriarMarcaAsync(factory, loja.Id, "Farm");
            TamanhoModel tamanho = await CriarTamanhoAsync(factory, loja.Id, "M");
            CorModel cor = await CriarCorAsync(factory, loja.Id, "Azul");
            ClienteModel fornecedor = await CriarClienteAsync(factory, loja.Id, "Fornecedor A", "44999990000");

            HttpResponseMessage response = await client.PostAsJsonAsync("/api/produto", new CriarProdutoCommand
            {
                Preco = 199.90m,
                ProdutoId = produto.Id,
                MarcaId = marca.Id,
                TamanhoId = tamanho.Id,
                CorId = cor.Id,
                FornecedorId = fornecedor.Id,
                Descricao = "Vestido azul midi",
                Entrada = DateTime.UtcNow,
                LojaId = loja.Id,
                Situacao = SituacaoProduto.Estoque
            });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task PostProdutoDeveRetornarBadRequestQuandoPayloadForInvalido()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.PostAsJsonAsync("/api/produto", new CriarProdutoCommand
            {
                Preco = 0m,
                ProdutoId = 0,
                MarcaId = 0,
                TamanhoId = 0,
                CorId = 0,
                FornecedorId = 0,
                Descricao = string.Empty,
                Entrada = DateTime.UtcNow,
                LojaId = loja.Id,
                Situacao = SituacaoProduto.Estoque
            });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PostProdutoDeveRetornarErroQuandoFornecedorNaoPertencerALojaInformada()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            LojaModel outraLoja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Bairro");
            ProdutoReferenciaModel produto = await CriarProdutoReferenciaAsync(factory, loja.Id, "Vestido");
            MarcaModel marca = await CriarMarcaAsync(factory, loja.Id, "Farm");
            TamanhoModel tamanho = await CriarTamanhoAsync(factory, loja.Id, "M");
            CorModel cor = await CriarCorAsync(factory, loja.Id, "Azul");
            ClienteModel fornecedorOutraLoja = await CriarClienteAsync(factory, outraLoja.Id, "Fornecedor B", "44999990001");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.PostAsJsonAsync("/api/produto", new CriarProdutoCommand
            {
                Preco = 199.90m,
                ProdutoId = produto.Id,
                MarcaId = marca.Id,
                TamanhoId = tamanho.Id,
                CorId = cor.Id,
                FornecedorId = fornecedorOutraLoja.Id,
                Descricao = "Vestido azul midi",
                Entrada = DateTime.UtcNow,
                LojaId = loja.Id,
                Situacao = SituacaoProduto.Estoque
            });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PostProdutoDeveRetornarBadRequestQuandoTabelaAuxiliarNaoPertencerALojaInformada()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            LojaModel outraLoja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Bairro");
            ProdutoReferenciaModel produtoOutraLoja = await CriarProdutoReferenciaAsync(factory, outraLoja.Id, "Vestido");
            MarcaModel marca = await CriarMarcaAsync(factory, loja.Id, "Farm");
            TamanhoModel tamanho = await CriarTamanhoAsync(factory, loja.Id, "M");
            CorModel cor = await CriarCorAsync(factory, loja.Id, "Azul");
            ClienteModel fornecedor = await CriarClienteAsync(factory, loja.Id, "Fornecedor A", "44999990000");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.PostAsJsonAsync("/api/produto", new CriarProdutoCommand
            {
                Preco = 199.90m,
                ProdutoId = produtoOutraLoja.Id,
                MarcaId = marca.Id,
                TamanhoId = tamanho.Id,
                CorId = cor.Id,
                FornecedorId = fornecedor.Id,
                Descricao = "Vestido azul midi",
                Entrada = DateTime.UtcNow,
                LojaId = loja.Id,
                Situacao = SituacaoProduto.Estoque
            });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PostProdutoAuxiliarDeveRetornarConflictQuandoValorJaExistirNaMesmaLoja()
        {
            await ValidarConflitoDeAuxiliarAsync("/api/produto/referencia", "Vestido", async (factory, lojaId) =>
                await CriarProdutoReferenciaAsync(factory, lojaId, "Vestido"));
        }

        [Fact]
        public async Task PostMarcaAuxiliarDeveRetornarConflictQuandoValorJaExistirNaMesmaLoja()
        {
            await ValidarConflitoDeAuxiliarAsync("/api/produto/marca", "Farm", async (factory, lojaId) =>
                await CriarMarcaAsync(factory, lojaId, "Farm"));
        }

        [Fact]
        public async Task PostTamanhoAuxiliarDeveRetornarConflictQuandoValorJaExistirNaMesmaLoja()
        {
            await ValidarConflitoDeAuxiliarAsync("/api/produto/tamanho", "M", async (factory, lojaId) =>
                await CriarTamanhoAsync(factory, lojaId, "M"));
        }

        [Fact]
        public async Task PostCorAuxiliarDeveRetornarConflictQuandoValorJaExistirNaMesmaLoja()
        {
            await ValidarConflitoDeAuxiliarAsync("/api/produto/cor", "Azul", async (factory, lojaId) =>
                await CriarCorAsync(factory, lojaId, "Azul"));
        }

        private static async Task ValidarConflitoDeAuxiliarAsync(string rota, string valor, Func<RenovaApiFactory, int, Task> seed)
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, $"user-{Guid.NewGuid():N}@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, $"Loja-{Guid.NewGuid():N}");
            await seed(factory, loja.Id);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.PostAsJsonAsync(rota, new CriarProdutoAuxiliarCommand
            {
                Valor = valor,
                LojaId = loja.Id
            });

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
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
    }
}
