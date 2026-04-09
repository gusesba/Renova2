using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using Microsoft.Extensions.DependencyInjection;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.Auth;
using Renova.Tests.Infrastructure;

namespace Renova.Tests.Services.Produto.Get
{
    public class Integracao
    {
        [Fact]
        public async Task GetProdutosDeveRetornarOkComProdutosDaLojaInformada()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            UsuarioTokenDto outroUsuario = await CriarUsuarioAutenticadoAsync(client, "joao@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            LojaModel outraLoja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Bairro");
            LojaModel lojaExterna = await CriarLojaAsync(factory, outroUsuario.Usuario.Id, "Loja Externa");

            _ = await CriarProdutoCompletoAsync(factory, loja.Id, "Vestido", "Farm", "M", "Azul", "Fornecedor Alpha", "Vestido azul");
            _ = await CriarProdutoCompletoAsync(factory, loja.Id, "Blazer", "Animale", "G", "Preto", "Fornecedor Beta", "Blazer preto");
            _ = await CriarProdutoCompletoAsync(factory, outraLoja.Id, "Saia", "Shoulder", "P", "Verde", "Fornecedor Loja Bairro", "Saia verde");
            _ = await CriarProdutoCompletoAsync(factory, lojaExterna.Id, "Calca", "Forum", "38", "Off White", "Fornecedor Externo", "Calca externa");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.GetAsync($"/api/produto?lojaId={loja.Id}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            PaginacaoDto<ProdutoBuscaDto>? body = await response.Content.ReadFromJsonAsync<PaginacaoDto<ProdutoBuscaDto>>();

            Assert.NotNull(body);
            Assert.Equal(2, body.TotalItens);
            Assert.Collection(body.Itens,
                produto =>
                {
                    Assert.Equal("Blazer", produto.Produto);
                    Assert.Equal("Animale", produto.Marca);
                    Assert.Equal("G", produto.Tamanho);
                    Assert.Equal("Preto", produto.Cor);
                    Assert.Equal("Fornecedor Beta", produto.Fornecedor);
                },
                produto =>
                {
                    Assert.Equal("Vestido", produto.Produto);
                    Assert.Equal("Farm", produto.Marca);
                    Assert.Equal("M", produto.Tamanho);
                    Assert.Equal("Azul", produto.Cor);
                    Assert.Equal("Fornecedor Alpha", produto.Fornecedor);
                });
        }

        [Fact]
        public async Task GetProdutosDeveRetornarPaginaSolicitadaQuandoPaginacaoForInformada()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");

            _ = await CriarProdutoCompletoAsync(factory, loja.Id, "Vestido", "Farm", "M", "Azul", "Fornecedor Alpha", "Ana");
            _ = await CriarProdutoCompletoAsync(factory, loja.Id, "Blazer", "Animale", "G", "Preto", "Fornecedor Beta", "Bruno");
            _ = await CriarProdutoCompletoAsync(factory, loja.Id, "Saia", "Shoulder", "P", "Verde", "Fornecedor Gamma", "Carla");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.GetAsync($"/api/produto?lojaId={loja.Id}&pagina=2&tamanhoPagina=1");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            PaginacaoDto<ProdutoBuscaDto>? body = await response.Content.ReadFromJsonAsync<PaginacaoDto<ProdutoBuscaDto>>();

            Assert.NotNull(body);
            Assert.Equal(3, body.TotalItens);
            Assert.Equal(2, body.Pagina);
            Assert.Equal(1, body.TamanhoPagina);
            Assert.Equal(3, body.TotalPaginas);
            ProdutoBuscaDto item = Assert.Single(body.Itens);
            Assert.Equal("Bruno", item.Descricao);
        }

        [Fact]
        public async Task GetProdutosDeveOrdenarPorCampoPrincipalQuandoOrdenacaoForInformada()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");

            _ = await CriarProdutoCompletoAsync(factory, loja.Id, "Vestido", "Farm", "M", "Azul", "Fornecedor Alpha", "Carlos");
            _ = await CriarProdutoCompletoAsync(factory, loja.Id, "Blazer", "Animale", "G", "Preto", "Fornecedor Beta", "Ana");
            _ = await CriarProdutoCompletoAsync(factory, loja.Id, "Saia", "Shoulder", "P", "Verde", "Fornecedor Gamma", "Bruno");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.GetAsync($"/api/produto?lojaId={loja.Id}&ordenarPor=descricao&direcao=desc");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            PaginacaoDto<ProdutoBuscaDto>? body = await response.Content.ReadFromJsonAsync<PaginacaoDto<ProdutoBuscaDto>>();

            Assert.NotNull(body);
            Assert.Collection(body.Itens,
                produto => Assert.Equal("Carlos", produto.Descricao),
                produto => Assert.Equal("Bruno", produto.Descricao),
                produto => Assert.Equal("Ana", produto.Descricao));
        }

        [Fact]
        public async Task GetProdutosDeveFiltrarPorValorDasTabelasAuxiliaresQuandoFiltroForInformado()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");

            _ = await CriarProdutoCompletoAsync(factory, loja.Id, "Vestido", "Farm", "M", "Azul", "Fornecedor Alpha", "Item 1");
            _ = await CriarProdutoCompletoAsync(factory, loja.Id, "Blazer", "Animale", "G", "Preto", "Fornecedor Beta", "Item 2");
            _ = await CriarProdutoCompletoAsync(factory, loja.Id, "Saia", "Shoulder", "P", "Vermelho", "Fornecedor Gamma", "Item 3");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.GetAsync($"/api/produto?lojaId={loja.Id}&marca=animal");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            PaginacaoDto<ProdutoBuscaDto>? body = await response.Content.ReadFromJsonAsync<PaginacaoDto<ProdutoBuscaDto>>();

            Assert.NotNull(body);
            ProdutoBuscaDto item = Assert.Single(body.Itens);
            Assert.Equal("Animale", item.Marca);
        }

        [Fact]
        public async Task GetProdutosDeveFiltrarPorNomeDoFornecedorQuandoFiltroForInformado()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");

            _ = await CriarProdutoCompletoAsync(factory, loja.Id, "Vestido", "Farm", "M", "Azul", "Fornecedor Alpha", "Item 1");
            _ = await CriarProdutoCompletoAsync(factory, loja.Id, "Blazer", "Animale", "G", "Preto", "Distribuidora Beta", "Item 2");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.GetAsync($"/api/produto?lojaId={loja.Id}&fornecedor=beta");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            PaginacaoDto<ProdutoBuscaDto>? body = await response.Content.ReadFromJsonAsync<PaginacaoDto<ProdutoBuscaDto>>();

            Assert.NotNull(body);
            ProdutoBuscaDto item = Assert.Single(body.Itens);
            Assert.Equal("Distribuidora Beta", item.Fornecedor);
        }

        [Fact]
        public async Task GetProdutosDeveFiltrarPorPrecoQuandoIntervaloForInformado()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");

            _ = await CriarProdutoCompletoAsync(factory, loja.Id, "Vestido", "Farm", "M", "Azul", "Fornecedor Alpha", "Item 1", 100m);
            _ = await CriarProdutoCompletoAsync(factory, loja.Id, "Blazer", "Animale", "G", "Preto", "Fornecedor Beta", "Item 2", 200m);
            _ = await CriarProdutoCompletoAsync(factory, loja.Id, "Saia", "Shoulder", "P", "Verde", "Fornecedor Gamma", "Item 3", 300m);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.GetAsync($"/api/produto?lojaId={loja.Id}&precoInicial=150&precoFinal=250");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            PaginacaoDto<ProdutoBuscaDto>? body = await response.Content.ReadFromJsonAsync<PaginacaoDto<ProdutoBuscaDto>>();

            Assert.NotNull(body);
            ProdutoBuscaDto item = Assert.Single(body.Itens);
            Assert.Equal(200m, item.Preco);
        }

        [Fact]
        public async Task GetProdutosDeveFiltrarPorDataQuandoIntervaloForInformado()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");

            _ = await CriarProdutoCompletoAsync(factory, loja.Id, "Vestido", "Farm", "M", "Azul", "Fornecedor Alpha", "Item 1", 100m, new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc));
            _ = await CriarProdutoCompletoAsync(factory, loja.Id, "Blazer", "Animale", "G", "Preto", "Fornecedor Beta", "Item 2", 200m, new DateTime(2026, 4, 10, 0, 0, 0, DateTimeKind.Utc));
            _ = await CriarProdutoCompletoAsync(factory, loja.Id, "Saia", "Shoulder", "P", "Verde", "Fornecedor Gamma", "Item 3", 300m, new DateTime(2026, 4, 20, 0, 0, 0, DateTimeKind.Utc));

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.GetAsync($"/api/produto?lojaId={loja.Id}&dataInicial=2026-04-05T00:00:00Z&dataFinal=2026-04-15T00:00:00Z");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            PaginacaoDto<ProdutoBuscaDto>? body = await response.Content.ReadFromJsonAsync<PaginacaoDto<ProdutoBuscaDto>>();

            Assert.NotNull(body);
            ProdutoBuscaDto item = Assert.Single(body.Itens);
            Assert.Equal(new DateTime(2026, 4, 10, 0, 0, 0, DateTimeKind.Utc), item.Entrada);
        }

        [Fact]
        public async Task GetProdutosDeveOrdenarPorCamposRelacionadosQuandoOrdenacaoForInformada()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");

            _ = await CriarProdutoCompletoAsync(factory, loja.Id, "Vestido", "Zeta", "M", "Azul", "Fornecedor Carlos", "Item 1");
            _ = await CriarProdutoCompletoAsync(factory, loja.Id, "Blazer", "Alpha", "G", "Preto", "Fornecedor Ana", "Item 2");
            _ = await CriarProdutoCompletoAsync(factory, loja.Id, "Saia", "Beta", "P", "Verde", "Fornecedor Bruno", "Item 3");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.GetAsync($"/api/produto?lojaId={loja.Id}&ordenarPor=fornecedor&direcao=desc");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            PaginacaoDto<ProdutoBuscaDto>? body = await response.Content.ReadFromJsonAsync<PaginacaoDto<ProdutoBuscaDto>>();

            Assert.NotNull(body);
            Assert.Collection(body.Itens,
                produto => Assert.Equal("Fornecedor Carlos", produto.Fornecedor),
                produto => Assert.Equal("Fornecedor Bruno", produto.Fornecedor),
                produto => Assert.Equal("Fornecedor Ana", produto.Fornecedor));
        }

        [Fact]
        public async Task GetProdutosDeveCombinarFiltrosOrdenacaoEPaginacao()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");

            _ = await CriarProdutoCompletoAsync(factory, loja.Id, "Vestido", "Farm", "M", "Azul", "Fornecedor Zeta", "Vestido azul premium");
            _ = await CriarProdutoCompletoAsync(factory, loja.Id, "Vestido Midi", "Farm", "G", "Azul", "Fornecedor Alpha", "Vestido azul casual");
            _ = await CriarProdutoCompletoAsync(factory, loja.Id, "Blazer", "Animale", "P", "Preto", "Fornecedor Beta", "Blazer preto");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.GetAsync($"/api/produto?lojaId={loja.Id}&produto=Vestido&cor=Azul&ordenarPor=fornecedor&direcao=desc&pagina=2&tamanhoPagina=1");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            PaginacaoDto<ProdutoBuscaDto>? body = await response.Content.ReadFromJsonAsync<PaginacaoDto<ProdutoBuscaDto>>();

            Assert.NotNull(body);
            Assert.Equal(2, body.TotalItens);
            Assert.Equal(2, body.TotalPaginas);
            ProdutoBuscaDto item = Assert.Single(body.Itens);
            Assert.Equal("Fornecedor Alpha", item.Fornecedor);
        }

        [Fact]
        public async Task GetProdutosDeveRetornarBadRequestQuandoLojaIdNaoForInformado()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.GetAsync("/api/produto");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetProdutosDeveRetornarUnauthorizedQuandoUsuarioNaoEstiverAutenticado()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            HttpResponseMessage response = await client.GetAsync("/api/produto?lojaId=1");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetProdutosDeveRetornarOkComListaVaziaQuandoLojaNaoPossuirProdutos()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Vazia");
            LojaModel outraLoja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Com Produtos");

            _ = await CriarProdutoCompletoAsync(factory, outraLoja.Id, "Vestido", "Farm", "M", "Azul", "Fornecedor Alpha", "Vestido azul");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.GetAsync($"/api/produto?lojaId={loja.Id}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            PaginacaoDto<ProdutoBuscaDto>? body = await response.Content.ReadFromJsonAsync<PaginacaoDto<ProdutoBuscaDto>>();

            Assert.NotNull(body);
            Assert.Empty(body.Itens);
            Assert.Equal(0, body.TotalItens);
            Assert.Equal(0, body.TotalPaginas);
        }

        [Fact]
        public async Task GetProdutosDeveRetornarUnauthorizedQuandoLojaFiltradaNaoPertencerAoUsuarioAutenticado()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto donoDaLoja = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            UsuarioTokenDto outroUsuario = await CriarUsuarioAutenticadoAsync(client, "joao@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, donoDaLoja.Usuario.Id, "Loja Centro");

            _ = await CriarProdutoCompletoAsync(factory, loja.Id, "Vestido", "Farm", "M", "Azul", "Fornecedor Alpha", "Vestido azul");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", outroUsuario.Token);

            HttpResponseMessage response = await client.GetAsync($"/api/produto?lojaId={loja.Id}");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetProdutosEmprestadosDeveRetornarOkComEmprestadosDoClienteSelecionado()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria-emprestados@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            ClienteModel elaine = await CriarClienteAsync(factory, loja.Id, "Elaine", "44999990001");
            ClienteModel gustavo = await CriarClienteAsync(factory, loja.Id, "Gustavo", "44999990002");
            ProdutoEstoqueModel produtoElaine = await CriarProdutoCompletoAsync(factory, loja.Id, "Vestido", "Farm", "M", "Azul", "Fornecedor Alpha", "Vestido Elaine");
            ProdutoEstoqueModel produtoGustavo = await CriarProdutoCompletoAsync(factory, loja.Id, "Blazer", "Animale", "G", "Preto", "Fornecedor Beta", "Blazer Gustavo");
            await AtualizarSituacaoProdutoAsync(factory, produtoElaine.Id, SituacaoProduto.Emprestado);
            await AtualizarSituacaoProdutoAsync(factory, produtoGustavo.Id, SituacaoProduto.Emprestado);
            _ = await CriarMovimentacaoAsync(factory, loja.Id, elaine.Id, TipoMovimentacao.Emprestimo, produtoElaine.Id);
            _ = await CriarMovimentacaoAsync(factory, loja.Id, gustavo.Id, TipoMovimentacao.Emprestimo, produtoGustavo.Id);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.GetAsync($"/api/produto/emprestados?lojaId={loja.Id}&clienteId={elaine.Id}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            IReadOnlyList<ProdutoBuscaDto>? body = await response.Content.ReadFromJsonAsync<IReadOnlyList<ProdutoBuscaDto>>();
            ProdutoBuscaDto item = Assert.Single(body!);
            Assert.Equal(produtoElaine.Id, item.Id);
        }

        [Fact]
        public async Task GetProdutosEmprestadosDeveRetornarBadRequestQuandoClienteNaoPertencerALoja()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria-emprestados-badrequest@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            LojaModel outraLoja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Bairro");
            ClienteModel clienteOutraLoja = await CriarClienteAsync(factory, outraLoja.Id, "Elaine", "44999990001");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.GetAsync($"/api/produto/emprestados?lojaId={loja.Id}&clienteId={clienteOutraLoja.Id}");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
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
            decimal preco = 149.90m,
            DateTime? entrada = null)
        {
            ProdutoReferenciaModel produtoReferencia = await CriarProdutoReferenciaAsync(factory, lojaId, produto);
            MarcaModel marcaModel = await CriarMarcaAsync(factory, lojaId, marca);
            TamanhoModel tamanhoModel = await CriarTamanhoAsync(factory, lojaId, tamanho);
            CorModel corModel = await CriarCorAsync(factory, lojaId, cor);
            ClienteModel fornecedorModel = await CriarClienteAsync(factory, lojaId, fornecedor, $"{Guid.NewGuid():N}"[..11]);

            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();

            ProdutoEstoqueModel entity = new()
            {
                Preco = preco,
                ProdutoId = produtoReferencia.Id,
                MarcaId = marcaModel.Id,
                TamanhoId = tamanhoModel.Id,
                CorId = corModel.Id,
                FornecedorId = fornecedorModel.Id,
                Descricao = descricao,
                Entrada = entrada ?? new DateTime(2026, 4, 7, 12, 0, 0, DateTimeKind.Utc),
                LojaId = lojaId,
                Situacao = SituacaoProduto.Estoque,
                Consignado = false
            };

            _ = context.ProdutosEstoque.Add(entity);
            _ = await context.SaveChangesAsync();
            return entity;
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

        private static async Task AtualizarSituacaoProdutoAsync(RenovaApiFactory factory, int produtoId, SituacaoProduto situacao)
        {
            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();

            ProdutoEstoqueModel produto = await context.ProdutosEstoque.SingleAsync(item => item.Id == produtoId);
            produto.Situacao = situacao;
            _ = await context.SaveChangesAsync();
        }

        private static async Task<MovimentacaoModel> CriarMovimentacaoAsync(
            RenovaApiFactory factory,
            int lojaId,
            int clienteId,
            TipoMovimentacao tipo,
            params int[] produtoIds)
        {
            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();

            MovimentacaoModel movimentacao = new()
            {
                Tipo = tipo,
                Data = new DateTime(2026, 4, 8, 12, 0, 0, DateTimeKind.Utc),
                ClienteId = clienteId,
                LojaId = lojaId,
                Produtos = produtoIds
                    .Select(produtoId => new MovimentacaoProdutoModel
                    {
                        ProdutoId = produtoId
                    })
                    .ToList()
            };

            _ = context.Movimentacoes.Add(movimentacao);
            _ = await context.SaveChangesAsync();
            return movimentacao;
        }
    }
}
