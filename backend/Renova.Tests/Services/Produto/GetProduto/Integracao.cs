using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using Microsoft.Extensions.DependencyInjection;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.Auth;
using Renova.Tests.Infrastructure;

namespace Renova.Tests.Services.Produto.GetProduto
{
    public class Integracao
    {
        [Fact]
        public async Task GetProdutoDeveRetornarOkQuandoIdForValido()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            ProdutoEstoqueModel produto = await CriarProdutoCompletoAsync(factory, loja.Id, "Vestido", "Farm", "M", "Azul", "Fornecedor Alpha", "Vestido azul");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.GetAsync($"/api/produto/{produto.Id}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            ProdutoBuscaDto? body = await response.Content.ReadFromJsonAsync<ProdutoBuscaDto>();
            Assert.NotNull(body);
            Assert.Equal(produto.Id, body.Id);
            Assert.Equal("Vestido", body.Produto);
            Assert.Equal("Farm", body.Marca);
            Assert.Equal("Fornecedor Alpha", body.Fornecedor);
            Assert.Equal(TipoMovimentacao.DevolucaoDono, body.TipoSugerido);
        }

        [Fact]
        public async Task GetProdutoDeveRetornarTipoSugeridoComoDoacaoQuandoFornecedorPermitir()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            ProdutoEstoqueModel produto = await CriarProdutoCompletoAsync(
                factory,
                loja.Id,
                "Vestido",
                "Farm",
                "M",
                "Azul",
                "Fornecedor Alpha",
                "Vestido azul",
                fornecedorDoacao: true);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.GetAsync($"/api/produto/{produto.Id}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            ProdutoBuscaDto? body = await response.Content.ReadFromJsonAsync<ProdutoBuscaDto>();
            Assert.NotNull(body);
            Assert.Equal(TipoMovimentacao.Doacao, body.TipoSugerido);
        }

        [Fact]
        public async Task GetProdutoDeveRetornarUnauthorizedQuandoUsuarioNaoEstiverAutenticado()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            HttpResponseMessage response = await client.GetAsync("/api/produto/1");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetProdutoDeveRetornarNotFoundQuandoProdutoNaoExistir()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.GetAsync("/api/produto/999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetProdutoDeveRetornarUnauthorizedQuandoProdutoNaoPertencerAoUsuarioAutenticado()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto donoDaLoja = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            UsuarioTokenDto outroUsuario = await CriarUsuarioAutenticadoAsync(client, "joao@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, donoDaLoja.Usuario.Id, "Loja Centro");
            ProdutoEstoqueModel produto = await CriarProdutoCompletoAsync(factory, loja.Id, "Vestido", "Farm", "M", "Azul", "Fornecedor Alpha", "Vestido azul");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", outroUsuario.Token);

            HttpResponseMessage response = await client.GetAsync($"/api/produto/{produto.Id}");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
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

        private static async Task<ProdutoEstoqueModel> CriarProdutoCompletoAsync(
            RenovaApiFactory factory,
            int lojaId,
            string produto,
            string marca,
            string tamanho,
            string cor,
            string fornecedor,
            string descricao,
            bool fornecedorDoacao = false)
        {
            ProdutoReferenciaModel produtoReferencia = await CriarProdutoReferenciaAsync(factory, lojaId, produto);
            MarcaModel marcaModel = await CriarMarcaAsync(factory, lojaId, marca);
            TamanhoModel tamanhoModel = await CriarTamanhoAsync(factory, lojaId, tamanho);
            CorModel corModel = await CriarCorAsync(factory, lojaId, cor);
            ClienteModel fornecedorModel = await CriarClienteAsync(
                factory,
                lojaId,
                fornecedor,
                $"{Guid.NewGuid():N}"[..11],
                fornecedorDoacao);

            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();

            ProdutoEstoqueModel entity = new()
            {
                Preco = 149.90m,
                ProdutoId = produtoReferencia.Id,
                MarcaId = marcaModel.Id,
                TamanhoId = tamanhoModel.Id,
                CorId = corModel.Id,
                FornecedorId = fornecedorModel.Id,
                Descricao = descricao,
                Entrada = new DateTime(2026, 4, 7, 12, 0, 0, DateTimeKind.Utc),
                LojaId = lojaId,
                Situacao = SituacaoProduto.Estoque,
                Consignado = false
            };

            _ = context.ProdutosEstoque.Add(entity);
            _ = await context.SaveChangesAsync();
            return entity;
        }

        private static async Task<ClienteModel> CriarClienteAsync(
            RenovaApiFactory factory,
            int lojaId,
            string nome,
            string contato,
            bool doacao = false)
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
