using Microsoft.EntityFrameworkCore;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Parameters.Pagamento;
using Renova.Service.Queries.Pagamento;
using Renova.Service.Services.Pagamento;
using Renova.Tests.Infrastructure;

namespace Renova.Tests.Services.Pagamento.Get
{
    public class Unitario : InMemoryDbContextTestBase<RenovaDbContext>
    {
        protected override RenovaDbContext CriarContexto(DbContextOptions<RenovaDbContext> options)
        {
            return new RenovaDbContext(options);
        }

        [Fact]
        public async Task GetAllAsyncDeveRetornarPagamentosPaginadosComMovimentacaoRelacionada()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = await CriarUsuarioAsync(context, "pagamentos-get@renova.com");
            LojaModel loja = await CriarLojaAsync(context, usuario.Id, "Loja Centro");
            ClienteModel clienteMovimentacao = await CriarClienteAsync(context, loja.Id, "Cliente Venda", "44999990000");
            ClienteModel fornecedorPagamento = await CriarClienteAsync(context, loja.Id, "Fornecedor A", "44999990001");
            ProdutoEstoqueModel produtoA = await CriarProdutoAsync(context, loja.Id, "Produto A", clienteMovimentacao.Id);
            ProdutoEstoqueModel produtoB = await CriarProdutoAsync(context, loja.Id, "Produto B", clienteMovimentacao.Id);
            MovimentacaoModel movimentacao = await CriarMovimentacaoAsync(context, loja.Id, clienteMovimentacao.Id, TipoMovimentacao.Venda, new DateTime(2026, 4, 2, 12, 0, 0, DateTimeKind.Utc), produtoA.Id, produtoB.Id);

            _ = context.Pagamentos.Add(new PagamentoModel
            {
                MovimentacaoId = movimentacao.Id,
                LojaId = loja.Id,
                ClienteId = fornecedorPagamento.Id,
                Natureza = NaturezaPagamento.Pagar,
                Status = StatusPagamento.Pendente,
                Valor = 120m,
                Data = new DateTime(2026, 4, 3, 12, 0, 0, DateTimeKind.Utc)
            });
            _ = await context.SaveChangesAsync();

            PagamentoService service = new(context);
            PaginacaoDto<PagamentoBuscaDto> resultado = await service.GetAllAsync(
                new ObterPagamentosQuery { LojaId = loja.Id },
                new ObterPagamentosParametros { UsuarioId = usuario.Id });

            Assert.Equal(1, resultado.TotalItens);
            PagamentoBuscaDto item = Assert.Single(resultado.Itens);
            Assert.Equal("Fornecedor A", item.Cliente);
            Assert.Equal(NaturezaPagamento.Pagar, item.Natureza);
            Assert.Equal(StatusPagamento.Pendente, item.Status);
            Assert.Equal(movimentacao.Id, item.Movimentacao.Id);
            Assert.Equal("Cliente Venda", item.Movimentacao.Cliente);
            Assert.Equal(2, item.Movimentacao.QuantidadeProdutos);
            Assert.Equal([produtoA.Id, produtoB.Id], item.Movimentacao.ProdutoIds);
        }

        [Fact]
        public async Task GetAllAsyncDeveCombinarFiltrosOrdenacaoEPaginacao()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = await CriarUsuarioAsync(context, "pagamentos-filtro@renova.com");
            LojaModel loja = await CriarLojaAsync(context, usuario.Id, "Loja Centro");
            ClienteModel clienteMovimentacao = await CriarClienteAsync(context, loja.Id, "Cliente Base", "44999990000");
            ClienteModel fornecedorAna = await CriarClienteAsync(context, loja.Id, "Ana Fornecedor", "44999990001");
            ClienteModel fornecedorBeatriz = await CriarClienteAsync(context, loja.Id, "Beatriz Fornecedor", "44999990002");
            ProdutoEstoqueModel produto = await CriarProdutoAsync(context, loja.Id, "Produto Base", clienteMovimentacao.Id);

            MovimentacaoModel mov1 = await CriarMovimentacaoAsync(context, loja.Id, clienteMovimentacao.Id, TipoMovimentacao.Venda, new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc), produto.Id);
            MovimentacaoModel mov2 = await CriarMovimentacaoAsync(context, loja.Id, clienteMovimentacao.Id, TipoMovimentacao.Venda, new DateTime(2026, 4, 2, 10, 0, 0, DateTimeKind.Utc), produto.Id);
            MovimentacaoModel mov3 = await CriarMovimentacaoAsync(context, loja.Id, clienteMovimentacao.Id, TipoMovimentacao.Venda, new DateTime(2026, 4, 3, 10, 0, 0, DateTimeKind.Utc), produto.Id);

            context.Pagamentos.AddRange(
                new PagamentoModel
                {
                    MovimentacaoId = mov1.Id,
                    LojaId = loja.Id,
                    ClienteId = fornecedorAna.Id,
                    Natureza = NaturezaPagamento.Pagar,
                    Status = StatusPagamento.Pendente,
                    Valor = 40m,
                    Data = new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc)
                },
                new PagamentoModel
                {
                    MovimentacaoId = mov2.Id,
                    LojaId = loja.Id,
                    ClienteId = fornecedorBeatriz.Id,
                    Natureza = NaturezaPagamento.Pagar,
                    Status = StatusPagamento.Pendente,
                    Valor = 50m,
                    Data = new DateTime(2026, 4, 2, 12, 0, 0, DateTimeKind.Utc)
                },
                new PagamentoModel
                {
                    MovimentacaoId = mov3.Id,
                    LojaId = loja.Id,
                    ClienteId = fornecedorAna.Id,
                    Natureza = NaturezaPagamento.Receber,
                    Status = StatusPagamento.Pago,
                    Valor = 60m,
                    Data = new DateTime(2026, 4, 3, 12, 0, 0, DateTimeKind.Utc)
                });
            _ = await context.SaveChangesAsync();

            PagamentoService service = new(context);
            PaginacaoDto<PagamentoBuscaDto> resultado = await service.GetAllAsync(
                new ObterPagamentosQuery
                {
                    LojaId = loja.Id,
                    Cliente = "fornecedor",
                    Natureza = NaturezaPagamento.Pagar,
                    Status = StatusPagamento.Pendente,
                    DataInicial = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc),
                    DataFinal = new DateTime(2026, 4, 2, 23, 59, 59, DateTimeKind.Utc),
                    OrdenarPor = "cliente",
                    Direcao = "desc",
                    Pagina = 2,
                    TamanhoPagina = 1
                },
                new ObterPagamentosParametros { UsuarioId = usuario.Id });

            Assert.Equal(2, resultado.TotalItens);
            Assert.Equal(2, resultado.TotalPaginas);
            PagamentoBuscaDto item = Assert.Single(resultado.Itens);
            Assert.Equal("Ana Fornecedor", item.Cliente);
            Assert.Equal(mov1.Id, item.MovimentacaoId);
        }

        [Fact]
        public async Task GetAllAsyncDeveRetornarPagamentoManualSemMovimentacaoComDescricao()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = await CriarUsuarioAsync(context, "pagamentos-manual-get@renova.com");
            LojaModel loja = await CriarLojaAsync(context, usuario.Id, "Loja Centro");
            ClienteModel cliente = await CriarClienteAsync(context, loja.Id, "Cliente Manual", "44999990003");

            _ = context.Pagamentos.Add(new PagamentoModel
            {
                MovimentacaoId = null,
                LojaId = loja.Id,
                ClienteId = cliente.Id,
                Natureza = NaturezaPagamento.Pagar,
                Status = StatusPagamento.Pago,
                Descricao = "Acerto sem movimentacao",
                Valor = 33m,
                Data = new DateTime(2026, 4, 4, 12, 0, 0, DateTimeKind.Utc)
            });
            _ = await context.SaveChangesAsync();

            PagamentoService service = new(context);
            PaginacaoDto<PagamentoBuscaDto> resultado = await service.GetAllAsync(
                new ObterPagamentosQuery { LojaId = loja.Id },
                new ObterPagamentosParametros { UsuarioId = usuario.Id });

            PagamentoBuscaDto item = Assert.Single(resultado.Itens);
            Assert.Null(item.MovimentacaoId);
            Assert.Null(item.Movimentacao);
            Assert.Equal("Acerto sem movimentacao", item.Descricao);
        }

        [Fact]
        public async Task GetFechamentoLojaAsyncDeveRetornarResumoDoMesEHistoricoDeDozeMeses()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = await CriarUsuarioAsync(context, "pagamentos-fechamento@renova.com");
            LojaModel loja = await CriarLojaAsync(context, usuario.Id, "Loja Centro");
            ClienteModel clienteVenda = await CriarClienteAsync(context, loja.Id, "Cliente Venda", "44999990000");
            ClienteModel fornecedorA = await CriarClienteAsync(context, loja.Id, "Fornecedor A", "44999990001");
            ClienteModel fornecedorB = await CriarClienteAsync(context, loja.Id, "Fornecedor B", "44999990002");
            ProdutoEstoqueModel produtoA = await CriarProdutoAsync(context, loja.Id, "Produto A", fornecedorA.Id);
            ProdutoEstoqueModel produtoB = await CriarProdutoAsync(context, loja.Id, "Produto B", fornecedorB.Id);
            ProdutoEstoqueModel produtoC = await CriarProdutoAsync(context, loja.Id, "Produto C", fornecedorA.Id);
            ConfigLojaModel configLoja = new()
            {
                LojaId = loja.Id,
                PercentualRepasseFornecedor = 80m,
                PercentualRepasseVendedorCredito = 100m
            };

            _ = context.ConfiguracoesLoja.Add(configLoja);
            _ = await context.SaveChangesAsync();

            ConfigLojaFormaPagamentoModel formaPagamento = new()
            {
                ConfigLojaId = configLoja.Id,
                Nome = "Pix",
                PercentualAjuste = 0m
            };

            _ = context.ConfiguracoesLojaFormasPagamento.Add(formaPagamento);
            _ = await context.SaveChangesAsync();

            MovimentacaoModel vendaMarco = await CriarMovimentacaoAsync(
                context,
                loja.Id,
                clienteVenda.Id,
                TipoMovimentacao.Venda,
                new DateTime(2026, 3, 5, 12, 0, 0, DateTimeKind.Utc),
                produtoA.Id,
                produtoB.Id);

            MovimentacaoModel devolucaoMarco = await CriarMovimentacaoAsync(
                context,
                loja.Id,
                clienteVenda.Id,
                TipoMovimentacao.DevolucaoVenda,
                new DateTime(2026, 3, 18, 12, 0, 0, DateTimeKind.Utc),
                produtoA.Id);

            MovimentacaoModel vendaFevereiro = await CriarMovimentacaoAsync(
                context,
                loja.Id,
                clienteVenda.Id,
                TipoMovimentacao.Venda,
                new DateTime(2026, 2, 11, 12, 0, 0, DateTimeKind.Utc),
                produtoC.Id);

            context.Pagamentos.AddRange(
                new PagamentoModel
                {
                    MovimentacaoId = vendaMarco.Id,
                    LojaId = loja.Id,
                    ClienteId = clienteVenda.Id,
                    Natureza = NaturezaPagamento.Receber,
                    Status = StatusPagamento.Pago,
                    Valor = 300m,
                    Data = new DateTime(2026, 3, 5, 12, 0, 0, DateTimeKind.Utc)
                },
                new PagamentoModel
                {
                    MovimentacaoId = vendaMarco.Id,
                    LojaId = loja.Id,
                    ClienteId = fornecedorA.Id,
                    Natureza = NaturezaPagamento.Pagar,
                    Status = StatusPagamento.Pendente,
                    Valor = 90m,
                    Data = new DateTime(2026, 3, 5, 12, 0, 0, DateTimeKind.Utc)
                },
                new PagamentoModel
                {
                    MovimentacaoId = vendaMarco.Id,
                    LojaId = loja.Id,
                    ClienteId = fornecedorB.Id,
                    Natureza = NaturezaPagamento.Pagar,
                    Status = StatusPagamento.Pago,
                    Valor = 60m,
                    Data = new DateTime(2026, 3, 5, 12, 0, 0, DateTimeKind.Utc)
                },
                new PagamentoModel
                {
                    MovimentacaoId = devolucaoMarco.Id,
                    LojaId = loja.Id,
                    ClienteId = clienteVenda.Id,
                    Natureza = NaturezaPagamento.Pagar,
                    Status = StatusPagamento.Pendente,
                    Valor = 80m,
                    Data = new DateTime(2026, 3, 18, 12, 0, 0, DateTimeKind.Utc)
                },
                new PagamentoModel
                {
                    MovimentacaoId = devolucaoMarco.Id,
                    LojaId = loja.Id,
                    ClienteId = fornecedorA.Id,
                    Natureza = NaturezaPagamento.Receber,
                    Status = StatusPagamento.Pendente,
                    Valor = 24m,
                    Data = new DateTime(2026, 3, 18, 12, 0, 0, DateTimeKind.Utc)
                },
                new PagamentoModel
                {
                    MovimentacaoId = vendaFevereiro.Id,
                    LojaId = loja.Id,
                    ClienteId = clienteVenda.Id,
                    Natureza = NaturezaPagamento.Receber,
                    Status = StatusPagamento.Pago,
                    Valor = 150m,
                    Data = new DateTime(2026, 2, 11, 12, 0, 0, DateTimeKind.Utc)
                },
                new PagamentoModel
                {
                    MovimentacaoId = vendaFevereiro.Id,
                    LojaId = loja.Id,
                    ClienteId = fornecedorA.Id,
                    Natureza = NaturezaPagamento.Pagar,
                    Status = StatusPagamento.Cancelado,
                    Valor = 45m,
                    Data = new DateTime(2026, 2, 11, 12, 0, 0, DateTimeKind.Utc)
                });

            context.PagamentosCredito.AddRange(
                new PagamentoCreditoModel
                {
                    LojaId = loja.Id,
                    ClienteId = clienteVenda.Id,
                    Tipo = TipoPagamentoCredito.AdicionarCredito,
                    ConfigLojaFormaPagamentoId = formaPagamento.Id,
                    ValorCredito = 300m,
                    ValorDinheiro = 300m,
                    Data = new DateTime(2026, 3, 5, 12, 0, 0, DateTimeKind.Utc)
                },
                new PagamentoCreditoModel
                {
                    LojaId = loja.Id,
                    ClienteId = fornecedorA.Id,
                    Tipo = TipoPagamentoCredito.ResgatarCredito,
                    ValorCredito = 80m,
                    ValorDinheiro = 80m,
                    Data = new DateTime(2026, 3, 18, 12, 0, 0, DateTimeKind.Utc)
                },
                new PagamentoCreditoModel
                {
                    LojaId = loja.Id,
                    ClienteId = clienteVenda.Id,
                    Tipo = TipoPagamentoCredito.AdicionarCredito,
                    ConfigLojaFormaPagamentoId = formaPagamento.Id,
                    ValorCredito = 150m,
                    ValorDinheiro = 150m,
                    Data = new DateTime(2026, 2, 11, 12, 0, 0, DateTimeKind.Utc)
                });

            context.GastosLoja.AddRange(
                new GastoLojaModel
                {
                    LojaId = loja.Id,
                    Natureza = NaturezaGastoLoja.Recebimento,
                    Valor = 40m,
                    Data = new DateTime(2026, 3, 6, 12, 0, 0, DateTimeKind.Utc),
                    Descricao = "Recebimento avulso"
                },
                new GastoLojaModel
                {
                    LojaId = loja.Id,
                    Natureza = NaturezaGastoLoja.Pagamento,
                    Valor = 20m,
                    Data = new DateTime(2026, 3, 7, 12, 0, 0, DateTimeKind.Utc),
                    Descricao = "Pagamento da loja"
                },
                new GastoLojaModel
                {
                    LojaId = loja.Id,
                    Natureza = NaturezaGastoLoja.Pagamento,
                    Valor = 10m,
                    Data = new DateTime(2026, 2, 12, 12, 0, 0, DateTimeKind.Utc),
                    Descricao = "Pagamento fevereiro"
                });

            _ = await context.SaveChangesAsync();

            PagamentoService service = new(context);
            FechamentoLojaDto resultado = await service.GetFechamentoLojaAsync(
                new ObterFechamentoLojaQuery
                {
                    LojaId = loja.Id,
                    DataReferencia = new DateTime(2026, 3, 10, 0, 0, 0, DateTimeKind.Utc)
                },
                new ObterPagamentosParametros
                {
                    UsuarioId = usuario.Id
                });

            Assert.Equal(new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc), resultado.InicioPeriodo);
            Assert.Equal(1, resultado.QuantidadePecasVendidas);
            Assert.Equal(340m, resultado.ValorRecebidoClientes);
            Assert.Equal(100m, resultado.ValorPagoFornecedores);
            Assert.Equal(240m, resultado.Total);
            Assert.Equal(12, resultado.Historico.Count);

            FechamentoLojaMesDto fevereiro = Assert.Single(resultado.Historico.Where(item => item.Ano == 2026 && item.Mes == 2));
            Assert.Equal(1, fevereiro.QuantidadePecasVendidas);
            Assert.Equal(150m, fevereiro.ValorRecebidoClientes);
            Assert.Equal(10m, fevereiro.ValorPagoFornecedores);
            Assert.Equal(140m, fevereiro.Total);
        }

        private static async Task<UsuarioModel> CriarUsuarioAsync(RenovaDbContext context, string email)
        {
            UsuarioModel usuario = new()
            {
                Nome = "Usuario de Teste",
                Email = email,
                SenhaHash = "hash"
            };

            _ = context.Usuarios.Add(usuario);
            _ = await context.SaveChangesAsync();
            return usuario;
        }

        private static async Task<LojaModel> CriarLojaAsync(RenovaDbContext context, int usuarioId, string nome)
        {
            LojaModel loja = new()
            {
                Nome = nome,
                UsuarioId = usuarioId
            };

            _ = context.Lojas.Add(loja);
            _ = await context.SaveChangesAsync();
            return loja;
        }

        private static async Task<ClienteModel> CriarClienteAsync(RenovaDbContext context, int lojaId, string nome, string contato)
        {
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

        private static async Task<ProdutoEstoqueModel> CriarProdutoAsync(RenovaDbContext context, int lojaId, string descricao, int fornecedorId)
        {
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
            RenovaDbContext context,
            int lojaId,
            int clienteId,
            TipoMovimentacao tipo,
            DateTime data,
            params int[] produtoIds)
        {
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
