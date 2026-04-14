using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using Microsoft.Extensions.DependencyInjection;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.Auth;
using Renova.Tests.Infrastructure;

namespace Renova.Tests.Services.Pagamento.Get
{
    public class Integracao
    {
        [Fact]
        public async Task GetPagamentosDeveRetornarListaPaginadaComMovimentacaoRelacionada()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "pagamento-get-api@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            ClienteModel clienteMovimentacao = await CriarClienteAsync(factory, loja.Id, "Cliente Venda", "44999990000");
            ClienteModel fornecedor = await CriarClienteAsync(factory, loja.Id, "Fornecedor A", "44999990001");
            ProdutoEstoqueModel produtoA = await CriarProdutoAsync(factory, loja.Id, "Produto A", clienteMovimentacao.Id);
            ProdutoEstoqueModel produtoB = await CriarProdutoAsync(factory, loja.Id, "Produto B", clienteMovimentacao.Id);
            MovimentacaoModel movimentacao = await CriarMovimentacaoAsync(factory, loja.Id, clienteMovimentacao.Id, TipoMovimentacao.Venda, new DateTime(2026, 4, 2, 12, 0, 0, DateTimeKind.Utc), produtoA.Id, produtoB.Id);
            PagamentoModel pagamento = await CriarPagamentoAsync(factory, movimentacao.Id, loja.Id, fornecedor.Id, NaturezaPagamento.Pagar, StatusPagamento.Pendente, 120m, new DateTime(2026, 4, 3, 12, 0, 0, DateTimeKind.Utc));

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.GetAsync($"/api/pagamento?lojaId={loja.Id}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            PaginacaoDto<PagamentoBuscaDto>? body = await response.Content.ReadFromJsonAsync<PaginacaoDto<PagamentoBuscaDto>>();
            Assert.NotNull(body);
            PagamentoBuscaDto item = Assert.Single(body.Itens);
            Assert.Equal(pagamento.Id, item.Id);
            Assert.Equal("Fornecedor A", item.Cliente);
            Assert.Equal(movimentacao.Id, item.Movimentacao.Id);
            Assert.Equal("Cliente Venda", item.Movimentacao.Cliente);
            Assert.Equal([produtoA.Id, produtoB.Id], item.Movimentacao.ProdutoIds);
        }

        [Fact]
        public async Task GetPagamentosDeveAplicarFiltrosOrdenacaoEPaginacao()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "pagamento-filtro-api@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            ClienteModel clienteMovimentacao = await CriarClienteAsync(factory, loja.Id, "Cliente Base", "44999990000");
            ClienteModel fornecedorAna = await CriarClienteAsync(factory, loja.Id, "Ana Fornecedor", "44999990001");
            ClienteModel fornecedorBeatriz = await CriarClienteAsync(factory, loja.Id, "Beatriz Fornecedor", "44999990002");
            ProdutoEstoqueModel produto = await CriarProdutoAsync(factory, loja.Id, "Produto Base", clienteMovimentacao.Id);

            MovimentacaoModel mov1 = await CriarMovimentacaoAsync(factory, loja.Id, clienteMovimentacao.Id, TipoMovimentacao.Venda, new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc), produto.Id);
            MovimentacaoModel mov2 = await CriarMovimentacaoAsync(factory, loja.Id, clienteMovimentacao.Id, TipoMovimentacao.Venda, new DateTime(2026, 4, 2, 10, 0, 0, DateTimeKind.Utc), produto.Id);
            _ = await CriarPagamentoAsync(factory, mov1.Id, loja.Id, fornecedorAna.Id, NaturezaPagamento.Pagar, StatusPagamento.Pendente, 40m, new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc));
            _ = await CriarPagamentoAsync(factory, mov2.Id, loja.Id, fornecedorBeatriz.Id, NaturezaPagamento.Pagar, StatusPagamento.Pendente, 50m, new DateTime(2026, 4, 2, 12, 0, 0, DateTimeKind.Utc));

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.GetAsync($"/api/pagamento?lojaId={loja.Id}&cliente=fornecedor&natureza=Pagar&status=Pendente&ordenarPor=cliente&direcao=desc&pagina=2&tamanhoPagina=1");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            PaginacaoDto<PagamentoBuscaDto>? body = await response.Content.ReadFromJsonAsync<PaginacaoDto<PagamentoBuscaDto>>();
            Assert.NotNull(body);
            Assert.Equal(2, body.TotalItens);
            Assert.Equal(2, body.TotalPaginas);
            Assert.Equal(2, body.Pagina);
            PagamentoBuscaDto item = Assert.Single(body.Itens);
            Assert.Equal("Ana Fornecedor", item.Cliente);
            Assert.Equal(mov1.Id, item.MovimentacaoId);
        }

        [Fact]
        public async Task GetPagamentosDeveRetornarPagamentoManualSemMovimentacao()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "pagamento-manual-get-api@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            ClienteModel cliente = await CriarClienteAsync(factory, loja.Id, "Cliente Manual", "44999990005");
            _ = await CriarPagamentoAsync(factory, null, loja.Id, cliente.Id, NaturezaPagamento.Pagar, StatusPagamento.Pago, 25m, new DateTime(2026, 4, 5, 12, 0, 0, DateTimeKind.Utc), "Pagamento manual");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.GetAsync($"/api/pagamento?lojaId={loja.Id}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            PaginacaoDto<PagamentoBuscaDto>? body = await response.Content.ReadFromJsonAsync<PaginacaoDto<PagamentoBuscaDto>>();
            body = Assert.IsType<PaginacaoDto<PagamentoBuscaDto>>(body);
            PagamentoBuscaDto item = Assert.Single(body.Itens);
            Assert.Null(item.MovimentacaoId);
            Assert.Null(item.Movimentacao);
            Assert.Equal("Pagamento manual", item.Descricao);
        }

        [Fact]
        public async Task GetFechamentoLojaDeveRetornarResumoMensalEHistorico()
        {
            await using RenovaApiFactory factory = new();
            HttpClient client = factory.CreateClient();

            UsuarioTokenDto autenticacao = await CriarUsuarioAutenticadoAsync(client, "pagamento-fechamento-api@renova.com");
            LojaModel loja = await CriarLojaAsync(factory, autenticacao.Usuario.Id, "Loja Centro");
            ClienteModel clienteVenda = await CriarClienteAsync(factory, loja.Id, "Cliente Venda", "44999990000");
            ClienteModel fornecedor = await CriarClienteAsync(factory, loja.Id, "Fornecedor A", "44999990001");
            ProdutoEstoqueModel produtoA = await CriarProdutoAsync(factory, loja.Id, "Produto A", fornecedor.Id);
            ProdutoEstoqueModel produtoB = await CriarProdutoAsync(factory, loja.Id, "Produto B", fornecedor.Id);

            MovimentacaoModel venda = await CriarMovimentacaoAsync(
                factory,
                loja.Id,
                clienteVenda.Id,
                TipoMovimentacao.Venda,
                new DateTime(2026, 3, 4, 12, 0, 0, DateTimeKind.Utc),
                produtoA.Id,
                produtoB.Id);

            _ = await CriarPagamentoAsync(
                factory,
                venda.Id,
                loja.Id,
                clienteVenda.Id,
                NaturezaPagamento.Receber,
                StatusPagamento.Pago,
                240m,
                new DateTime(2026, 3, 4, 12, 0, 0, DateTimeKind.Utc));

            _ = await CriarPagamentoAsync(
                factory,
                venda.Id,
                loja.Id,
                fornecedor.Id,
                NaturezaPagamento.Pagar,
                StatusPagamento.Pendente,
                84m,
                new DateTime(2026, 3, 4, 12, 0, 0, DateTimeKind.Utc));

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", autenticacao.Token);

            HttpResponseMessage response = await client.GetAsync(
                $"/api/pagamento/fechamento?lojaId={loja.Id}&dataReferencia=2026-03-01T00:00:00");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            FechamentoLojaDto? body = await response.Content.ReadFromJsonAsync<FechamentoLojaDto>();
            body = Assert.IsType<FechamentoLojaDto>(body);
            Assert.Equal(2, body.QuantidadePecasVendidas);
            Assert.Equal(240m, body.ValorRecebidoClientes);
            Assert.Equal(84m, body.ValorPagoFornecedores);
            Assert.Equal(156m, body.Total);
            Assert.Equal(12, body.Historico.Count);
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

        private static async Task<ProdutoEstoqueModel> CriarProdutoAsync(RenovaApiFactory factory, int lojaId, string descricao, int fornecedorId)
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

        private static async Task<PagamentoModel> CriarPagamentoAsync(
            RenovaApiFactory factory,
            int? movimentacaoId,
            int lojaId,
            int clienteId,
            NaturezaPagamento natureza,
            StatusPagamento status,
            decimal valor,
            DateTime data,
            string? descricao = null)
        {
            using IServiceScope scope = factory.Services.CreateScope();
            RenovaDbContext context = scope.ServiceProvider.GetRequiredService<RenovaDbContext>();

            PagamentoModel pagamento = new()
            {
                MovimentacaoId = movimentacaoId,
                LojaId = lojaId,
                ClienteId = clienteId,
                Natureza = natureza,
                Status = status,
                Descricao = descricao,
                Valor = valor,
                Data = data
            };

            _ = context.Pagamentos.Add(pagamento);
            _ = await context.SaveChangesAsync();
            return pagamento;
        }
    }
}
