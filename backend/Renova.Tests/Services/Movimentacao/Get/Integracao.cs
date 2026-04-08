using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using Microsoft.Extensions.DependencyInjection;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.Auth;
using Renova.Tests.Infrastructure;

namespace Renova.Tests.Services.Movimentacao.Get
{
    public class Integracao
    {
        [Fact]
        public async Task GetMovimentacaoDeveRetornarListaPaginadaComClienteProdutosEQuantidade()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            LojaModel outraLoja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Bairro");
            ClienteModel cliente = await CriarClienteAsync(factory, loja.Id, "Ana Paula", "44999990000");
            ClienteModel clienteOutraLoja = await CriarClienteAsync(factory, outraLoja.Id, "Cliente Externo", "44999990009");
            ProdutoEstoqueModel produtoA = await CriarProdutoAsync(factory, loja.Id, "Vestido Azul", "44999990001");
            ProdutoEstoqueModel produtoB = await CriarProdutoAsync(factory, loja.Id, "Blazer Preto", "44999990002");
            ProdutoEstoqueModel produtoC = await CriarProdutoAsync(factory, outraLoja.Id, "Produto Outra Loja", "44999990003");

            MovimentacaoModel esperado = await CriarMovimentacaoAsync(factory, loja.Id, cliente.Id, TipoMovimentacao.Venda, new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc), produtoA.Id, produtoB.Id);
            _ = await CriarMovimentacaoAsync(factory, outraLoja.Id, clienteOutraLoja.Id, TipoMovimentacao.Emprestimo, new DateTime(2026, 4, 2, 12, 0, 0, DateTimeKind.Utc), produtoC.Id);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.GetAsync($"/api/movimentacao?lojaId={loja.Id}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            PaginacaoDto<MovimentacaoBuscaDto>? body = await response.Content.ReadFromJsonAsync<PaginacaoDto<MovimentacaoBuscaDto>>();
            Assert.NotNull(body);
            Assert.Equal(1, body.TotalItens);

            MovimentacaoBuscaDto item = Assert.Single(body.Itens);
            Assert.Equal(esperado.Id, item.Id);
            Assert.Equal("Ana Paula", item.Cliente);
            Assert.Equal(2, item.QuantidadeProdutos);
            Assert.Collection(item.Produtos,
                produto =>
                {
                    Assert.Equal(produtoA.Id, produto.Id);
                    Assert.Equal("Vestido Azul", produto.Descricao);
                    Assert.Equal("Vestido Azul Referencia", produto.Produto);
                    Assert.Equal("Vestido Azul Marca", produto.Marca);
                    Assert.Equal("Vestido Azul Fornecedor", produto.Fornecedor);
                },
                produto =>
                {
                    Assert.Equal(produtoB.Id, produto.Id);
                    Assert.Equal("Blazer Preto", produto.Descricao);
                    Assert.Equal("Blazer Preto Referencia", produto.Produto);
                    Assert.Equal("Blazer Preto Marca", produto.Marca);
                    Assert.Equal("Blazer Preto Fornecedor", produto.Fornecedor);
                });
        }

        [Fact]
        public async Task GetMovimentacaoDeveFiltrarPorPeriodoQuandoDatasForemInformadas()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            ClienteModel cliente = await CriarClienteAsync(factory, loja.Id, "Cliente Periodo", "44999990000");
            ProdutoEstoqueModel produtoA = await CriarProdutoAsync(factory, loja.Id, "Produto A", "44999990001");
            ProdutoEstoqueModel produtoB = await CriarProdutoAsync(factory, loja.Id, "Produto B", "44999990002");

            _ = await CriarMovimentacaoAsync(factory, loja.Id, cliente.Id, TipoMovimentacao.Venda, new DateTime(2026, 3, 31, 12, 0, 0, DateTimeKind.Utc), produtoA.Id);
            MovimentacaoModel esperado = await CriarMovimentacaoAsync(factory, loja.Id, cliente.Id, TipoMovimentacao.Emprestimo, new DateTime(2026, 4, 2, 12, 0, 0, DateTimeKind.Utc), produtoB.Id);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.GetAsync($"/api/movimentacao?lojaId={loja.Id}&dataInicial=2026-04-01T00:00:00Z&dataFinal=2026-04-03T00:00:00Z");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            PaginacaoDto<MovimentacaoBuscaDto>? body = await response.Content.ReadFromJsonAsync<PaginacaoDto<MovimentacaoBuscaDto>>();
            Assert.NotNull(body);
            MovimentacaoBuscaDto item = Assert.Single(body.Itens);
            Assert.Equal(esperado.Id, item.Id);
        }

        [Fact]
        public async Task GetMovimentacaoDeveFiltrarPorNomeDoClienteQuandoFiltroForInformado()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            ClienteModel clienteAna = await CriarClienteAsync(factory, loja.Id, "Ana Paula", "44999990000");
            ClienteModel clienteBeatriz = await CriarClienteAsync(factory, loja.Id, "Beatriz Lima", "44999990001");
            ProdutoEstoqueModel produtoA = await CriarProdutoAsync(factory, loja.Id, "Produto A", "44999990002");
            ProdutoEstoqueModel produtoB = await CriarProdutoAsync(factory, loja.Id, "Produto B", "44999990003");

            MovimentacaoModel esperado = await CriarMovimentacaoAsync(factory, loja.Id, clienteAna.Id, TipoMovimentacao.Venda, new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc), produtoA.Id);
            _ = await CriarMovimentacaoAsync(factory, loja.Id, clienteBeatriz.Id, TipoMovimentacao.Emprestimo, new DateTime(2026, 4, 2, 12, 0, 0, DateTimeKind.Utc), produtoB.Id);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.GetAsync($"/api/movimentacao?lojaId={loja.Id}&cliente=ana");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            PaginacaoDto<MovimentacaoBuscaDto>? body = await response.Content.ReadFromJsonAsync<PaginacaoDto<MovimentacaoBuscaDto>>();
            Assert.NotNull(body);
            MovimentacaoBuscaDto item = Assert.Single(body.Itens);
            Assert.Equal(esperado.Id, item.Id);
            Assert.Equal("Ana Paula", item.Cliente);
        }

        [Fact]
        public async Task GetMovimentacaoDeveFiltrarPorTipoQuandoFiltroForInformado()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            ClienteModel cliente = await CriarClienteAsync(factory, loja.Id, "Cliente Tipo", "44999990000");
            ProdutoEstoqueModel produtoA = await CriarProdutoAsync(factory, loja.Id, "Produto Venda", "44999990001");
            ProdutoEstoqueModel produtoB = await CriarProdutoAsync(factory, loja.Id, "Produto Emprestimo", "44999990002");

            MovimentacaoModel esperado = await CriarMovimentacaoAsync(factory, loja.Id, cliente.Id, TipoMovimentacao.Venda, new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc), produtoA.Id);
            _ = await CriarMovimentacaoAsync(factory, loja.Id, cliente.Id, TipoMovimentacao.Emprestimo, new DateTime(2026, 4, 2, 12, 0, 0, DateTimeKind.Utc), produtoB.Id);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.GetAsync($"/api/movimentacao?lojaId={loja.Id}&tipo=Venda");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            PaginacaoDto<MovimentacaoBuscaDto>? body = await response.Content.ReadFromJsonAsync<PaginacaoDto<MovimentacaoBuscaDto>>();
            Assert.NotNull(body);
            MovimentacaoBuscaDto item = Assert.Single(body.Itens);
            Assert.Equal(esperado.Id, item.Id);
            Assert.Equal(TipoMovimentacao.Venda, item.Tipo);
        }

        [Fact]
        public async Task GetMovimentacaoDeveOrdenarPelosCamposSuportadosQuandoOrdenacaoForInformada()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            ClienteModel clienteCarlos = await CriarClienteAsync(factory, loja.Id, "Carlos", "44999990000");
            ClienteModel clienteAna = await CriarClienteAsync(factory, loja.Id, "Ana", "44999990001");
            ClienteModel clienteBruno = await CriarClienteAsync(factory, loja.Id, "Bruno", "44999990002");
            ProdutoEstoqueModel produtoA = await CriarProdutoAsync(factory, loja.Id, "Produto A", "44999990003");
            ProdutoEstoqueModel produtoB = await CriarProdutoAsync(factory, loja.Id, "Produto B", "44999990004");
            ProdutoEstoqueModel produtoC = await CriarProdutoAsync(factory, loja.Id, "Produto C", "44999990005");

            _ = await CriarMovimentacaoAsync(factory, loja.Id, clienteCarlos.Id, TipoMovimentacao.Emprestimo, new DateTime(2026, 4, 3, 12, 0, 0, DateTimeKind.Utc), produtoA.Id);
            _ = await CriarMovimentacaoAsync(factory, loja.Id, clienteAna.Id, TipoMovimentacao.Venda, new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc), produtoB.Id);
            _ = await CriarMovimentacaoAsync(factory, loja.Id, clienteBruno.Id, TipoMovimentacao.Doacao, new DateTime(2026, 4, 2, 12, 0, 0, DateTimeKind.Utc), produtoC.Id);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage responsePorCliente = await client.GetAsync($"/api/movimentacao?lojaId={loja.Id}&ordenarPor=cliente&direcao=asc");
            Assert.Equal(HttpStatusCode.OK, responsePorCliente.StatusCode);
            PaginacaoDto<MovimentacaoBuscaDto>? bodyPorCliente = await responsePorCliente.Content.ReadFromJsonAsync<PaginacaoDto<MovimentacaoBuscaDto>>();
            Assert.NotNull(bodyPorCliente);
            Assert.Collection(bodyPorCliente.Itens,
                item => Assert.Equal("Ana", item.Cliente),
                item => Assert.Equal("Bruno", item.Cliente),
                item => Assert.Equal("Carlos", item.Cliente));

            HttpResponseMessage responsePorData = await client.GetAsync($"/api/movimentacao?lojaId={loja.Id}&ordenarPor=data&direcao=desc");
            Assert.Equal(HttpStatusCode.OK, responsePorData.StatusCode);
            PaginacaoDto<MovimentacaoBuscaDto>? bodyPorData = await responsePorData.Content.ReadFromJsonAsync<PaginacaoDto<MovimentacaoBuscaDto>>();
            Assert.NotNull(bodyPorData);
            Assert.Collection(bodyPorData.Itens,
                item => Assert.Equal(new DateTime(2026, 4, 3, 12, 0, 0, DateTimeKind.Utc), item.Data),
                item => Assert.Equal(new DateTime(2026, 4, 2, 12, 0, 0, DateTimeKind.Utc), item.Data),
                item => Assert.Equal(new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc), item.Data));

            HttpResponseMessage responsePorTipo = await client.GetAsync($"/api/movimentacao?lojaId={loja.Id}&ordenarPor=tipo&direcao=asc");
            Assert.Equal(HttpStatusCode.OK, responsePorTipo.StatusCode);
            PaginacaoDto<MovimentacaoBuscaDto>? bodyPorTipo = await responsePorTipo.Content.ReadFromJsonAsync<PaginacaoDto<MovimentacaoBuscaDto>>();
            Assert.NotNull(bodyPorTipo);
            Assert.Collection(bodyPorTipo.Itens,
                item => Assert.Equal(TipoMovimentacao.Venda, item.Tipo),
                item => Assert.Equal(TipoMovimentacao.Emprestimo, item.Tipo),
                item => Assert.Equal(TipoMovimentacao.Doacao, item.Tipo));
        }

        [Fact]
        public async Task GetMovimentacaoDeveCombinarFiltrosOrdenacaoEPaginacao()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            ClienteModel clienteAnaPaula = await CriarClienteAsync(factory, loja.Id, "Ana Paula", "44999990000");
            ClienteModel clienteAnaClara = await CriarClienteAsync(factory, loja.Id, "Ana Clara", "44999990001");
            ClienteModel clienteBeatriz = await CriarClienteAsync(factory, loja.Id, "Beatriz", "44999990002");
            ProdutoEstoqueModel produtoA = await CriarProdutoAsync(factory, loja.Id, "Produto A", "44999990003");
            ProdutoEstoqueModel produtoB = await CriarProdutoAsync(factory, loja.Id, "Produto B", "44999990004");
            ProdutoEstoqueModel produtoC = await CriarProdutoAsync(factory, loja.Id, "Produto C", "44999990005");

            _ = await CriarMovimentacaoAsync(factory, loja.Id, clienteAnaPaula.Id, TipoMovimentacao.Venda, new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc), produtoA.Id);
            MovimentacaoModel esperado = await CriarMovimentacaoAsync(factory, loja.Id, clienteAnaClara.Id, TipoMovimentacao.Venda, new DateTime(2026, 4, 2, 12, 0, 0, DateTimeKind.Utc), produtoB.Id);
            _ = await CriarMovimentacaoAsync(factory, loja.Id, clienteBeatriz.Id, TipoMovimentacao.Emprestimo, new DateTime(2026, 4, 3, 12, 0, 0, DateTimeKind.Utc), produtoC.Id);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.GetAsync($"/api/movimentacao?lojaId={loja.Id}&dataInicial=2026-04-01T00:00:00Z&dataFinal=2026-04-03T00:00:00Z&cliente=Ana&tipo=Venda&ordenarPor=cliente&direcao=desc&pagina=2&tamanhoPagina=1");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            PaginacaoDto<MovimentacaoBuscaDto>? body = await response.Content.ReadFromJsonAsync<PaginacaoDto<MovimentacaoBuscaDto>>();
            Assert.NotNull(body);
            Assert.Equal(2, body.TotalItens);
            Assert.Equal(2, body.TotalPaginas);
            Assert.Equal(2, body.Pagina);
            Assert.Equal(1, body.TamanhoPagina);
            MovimentacaoBuscaDto item = Assert.Single(body.Itens);
            Assert.Equal(esperado.Id, item.Id);
            Assert.Equal("Ana Clara", item.Cliente);
        }

        [Fact]
        public async Task GetMovimentacaoDeveRetornarUnauthorizedQuandoUsuarioNaoEstiverAutenticado()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            HttpResponseMessage response = await client.GetAsync("/api/movimentacao?lojaId=1");

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

        private static async Task<ProdutoEstoqueModel> CriarProdutoAsync(RenovaApiFactory factory, int lojaId, string descricao, string contatoFornecedor)
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

            ClienteModel fornecedor = new()
            {
                Nome = $"{descricao} Fornecedor",
                Contato = contatoFornecedor,
                LojaId = lojaId
            };

            _ = context.ProdutosReferencia.Add(produto);
            _ = context.Marcas.Add(marca);
            _ = context.Tamanhos.Add(tamanho);
            _ = context.Cores.Add(cor);
            _ = context.Clientes.Add(fornecedor);
            _ = await context.SaveChangesAsync();

            ProdutoEstoqueModel item = new()
            {
                Preco = 149.90m,
                ProdutoId = produto.Id,
                MarcaId = marca.Id,
                TamanhoId = tamanho.Id,
                CorId = cor.Id,
                FornecedorId = fornecedor.Id,
                Descricao = descricao,
                Entrada = new DateTime(2026, 4, 7, 12, 0, 0, DateTimeKind.Utc),
                LojaId = lojaId,
                Situacao = SituacaoProduto.Estoque,
                Consignado = false
            };

            _ = context.ProdutosEstoque.Add(item);
            _ = await context.SaveChangesAsync();
            return item;
        }

        private static async Task<MovimentacaoModel> CriarMovimentacaoAsync(
            RenovaApiFactory factory,
            int lojaId,
            int clienteId,
            TipoMovimentacao tipo,
            DateTime data,
            params int[] produtoIds)
        {
            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();

            MovimentacaoModel entity = new()
            {
                Tipo = tipo,
                Data = data,
                ClienteId = clienteId,
                LojaId = lojaId,
                Produtos = produtoIds
                    .Select(produtoId => new MovimentacaoProdutoModel
                    {
                        ProdutoId = produtoId
                    })
                    .ToList()
            };

            _ = context.Movimentacoes.Add(entity);
            _ = await context.SaveChangesAsync();
            return entity;
        }
    }
}
