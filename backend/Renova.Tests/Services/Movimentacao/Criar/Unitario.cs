using Microsoft.EntityFrameworkCore;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.Movimentacao;
using Renova.Service.Parameters.Movimentacao;
using Renova.Service.Services.Movimentacao;
using Renova.Service.Services.Pagamento;
using Renova.Tests.Infrastructure;

namespace Renova.Tests.Services.Movimentacao.Criar
{
    public class Unitario : InMemoryDbContextTestBase<RenovaDbContext>
    {
        protected override RenovaDbContext CriarContexto(DbContextOptions<RenovaDbContext> options)
        {
            return new RenovaDbContext(options);
        }

        [Fact]
        public async Task CreateAsyncDeveCriarMovimentacaoQuandoPayloadForValido()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");
            ClienteModel cliente = await CriarClienteAsync(context, loja.Id, "Cliente A", "44999990000");
            ProdutoEstoqueModel produtoA = await CriarProdutoAsync(context, loja.Id, "Produto A", "44999990001");
            ProdutoEstoqueModel produtoB = await CriarProdutoAsync(context, loja.Id, "Produto B", "44999990002");

            CriarMovimentacaoCommand command = new()
            {
                Tipo = TipoMovimentacao.Venda,
                Data = new DateTime(2026, 4, 8, 12, 0, 0, DateTimeKind.Utc),
                ClienteId = cliente.Id,
                LojaId = loja.Id,
                ProdutoIds = [produtoA.Id, produtoB.Id]
            };

            MovimentacaoService service = new(context);
            MovimentacaoDto resultado = await service.CreateAsync(command, new CriarMovimentacaoParametros { UsuarioId = loja.UsuarioId });

            Assert.True(resultado.Id > 0);
            Assert.Equal(command.Tipo, resultado.Tipo);
            Assert.Equal(command.Data, resultado.Data);
            Assert.Equal(command.ClienteId, resultado.ClienteId);
            Assert.Equal(command.LojaId, resultado.LojaId);
            Assert.Equal(2, resultado.ProdutoIds.Count);
            Assert.Contains(produtoA.Id, resultado.ProdutoIds);
            Assert.Contains(produtoB.Id, resultado.ProdutoIds);

            MovimentacaoModel movimentacaoSalva = await context.Movimentacoes.SingleAsync();
            Assert.Equal(resultado.Id, movimentacaoSalva.Id);
            Assert.Equal(2, await context.MovimentacoesProdutos.CountAsync());
            Assert.All(await context.ProdutosEstoque.OrderBy(item => item.Id).ToListAsync(), item => Assert.Equal(SituacaoProduto.Vendido, item.Situacao));
        }

        [Theory]
        [InlineData(TipoMovimentacao.Venda)]
        [InlineData(TipoMovimentacao.Emprestimo)]
        [InlineData(TipoMovimentacao.Doacao)]
        [InlineData(TipoMovimentacao.DevolucaoDono)]
        [InlineData(TipoMovimentacao.DevolucaoVenda)]
        [InlineData(TipoMovimentacao.DevolucaoEmprestimo)]
        public async Task CreateAsyncDevePersistirTipoMovimentacaoComValorDoEnumEsperado(TipoMovimentacao tipo)
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");
            ClienteModel cliente = await CriarClienteAsync(context, loja.Id, "Cliente A", "44999990000");
            SituacaoProduto situacaoInicial = tipo switch
            {
                TipoMovimentacao.DevolucaoVenda => SituacaoProduto.Vendido,
                TipoMovimentacao.DevolucaoEmprestimo => SituacaoProduto.Emprestado,
                _ => SituacaoProduto.Estoque
            };
            ProdutoEstoqueModel produto = await CriarProdutoAsync(context, loja.Id, "Produto A", "44999990001", situacaoInicial);

            MovimentacaoService service = new(context);
            MovimentacaoDto resultado = await service.CreateAsync(new CriarMovimentacaoCommand
            {
                Tipo = tipo,
                Data = new DateTime(2026, 4, 8, 12, 0, 0, DateTimeKind.Utc),
                ClienteId = cliente.Id,
                LojaId = loja.Id,
                ProdutoIds = [produto.Id]
            }, new CriarMovimentacaoParametros { UsuarioId = loja.UsuarioId });

            Assert.Equal(tipo, resultado.Tipo);
            Assert.Equal(tipo, (await context.Movimentacoes.SingleAsync()).Tipo);
        }

        [Theory]
        [InlineData(TipoMovimentacao.Venda, SituacaoProduto.Vendido)]
        [InlineData(TipoMovimentacao.Emprestimo, SituacaoProduto.Emprestado)]
        [InlineData(TipoMovimentacao.Doacao, SituacaoProduto.Doado)]
        [InlineData(TipoMovimentacao.DevolucaoDono, SituacaoProduto.Devolvido)]
        [InlineData(TipoMovimentacao.DevolucaoVenda, SituacaoProduto.Estoque)]
        [InlineData(TipoMovimentacao.DevolucaoEmprestimo, SituacaoProduto.Estoque)]
        public async Task CreateAsyncDeveAtualizarSituacaoDoProdutoAposCriacao(TipoMovimentacao tipo, SituacaoProduto situacaoEsperadaAoFinal)
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");
            ClienteModel cliente = await CriarClienteAsync(context, loja.Id, "Cliente A", "44999990000");
            SituacaoProduto situacaoInicial = tipo switch
            {
                TipoMovimentacao.DevolucaoVenda => SituacaoProduto.Vendido,
                TipoMovimentacao.DevolucaoEmprestimo => SituacaoProduto.Emprestado,
                _ => SituacaoProduto.Estoque
            };
            ProdutoEstoqueModel produto = await CriarProdutoAsync(context, loja.Id, "Produto A", "44999990001", situacaoInicial);

            MovimentacaoService service = new(context);
            _ = await service.CreateAsync(new CriarMovimentacaoCommand
            {
                Tipo = tipo,
                Data = new DateTime(2026, 4, 8, 12, 0, 0, DateTimeKind.Utc),
                ClienteId = cliente.Id,
                LojaId = loja.Id,
                ProdutoIds = [produto.Id]
            }, new CriarMovimentacaoParametros { UsuarioId = loja.UsuarioId });

            Assert.Equal(situacaoEsperadaAoFinal, (await context.ProdutosEstoque.SingleAsync()).Situacao);
        }

        [Fact]
        public async Task CreateAsyncDeveImpedirCriacaoQuandoClienteNaoPertencerALojaInformada()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");
            LojaModel outraLoja = await CriarLojaAsync(context, "Loja Bairro", "joao@renova.com");
            ClienteModel clienteOutraLoja = await CriarClienteAsync(context, outraLoja.Id, "Cliente B", "44999990000");
            ProdutoEstoqueModel produto = await CriarProdutoAsync(context, loja.Id, "Produto A", "44999990001");

            MovimentacaoService service = new(context);
            _ = await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(new CriarMovimentacaoCommand
            {
                Tipo = TipoMovimentacao.Emprestimo,
                Data = new DateTime(2026, 4, 8, 12, 0, 0, DateTimeKind.Utc),
                ClienteId = clienteOutraLoja.Id,
                LojaId = loja.Id,
                ProdutoIds = [produto.Id]
            }, new CriarMovimentacaoParametros { UsuarioId = loja.UsuarioId }));

            Assert.Empty(context.Movimentacoes);
        }

        [Fact]
        public async Task CreateAsyncDeveImpedirCriacaoQuandoProdutoNaoPertencerALojaInformada()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");
            LojaModel outraLoja = await CriarLojaAsync(context, "Loja Bairro", "joao@renova.com");
            ClienteModel cliente = await CriarClienteAsync(context, loja.Id, "Cliente A", "44999990000");
            ProdutoEstoqueModel produtoOutraLoja = await CriarProdutoAsync(context, outraLoja.Id, "Produto B", "44999990001");

            MovimentacaoService service = new(context);
            _ = await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(new CriarMovimentacaoCommand
            {
                Tipo = TipoMovimentacao.Doacao,
                Data = new DateTime(2026, 4, 8, 12, 0, 0, DateTimeKind.Utc),
                ClienteId = cliente.Id,
                LojaId = loja.Id,
                ProdutoIds = [produtoOutraLoja.Id]
            }, new CriarMovimentacaoParametros { UsuarioId = loja.UsuarioId }));

            Assert.Empty(context.Movimentacoes);
            Assert.Empty(context.MovimentacoesProdutos);
        }

        [Fact]
        public async Task CreateAsyncDeveImpedirCriacaoQuandoSituacaoDosProdutosNaoForCompativelComOTipo()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");
            ClienteModel cliente = await CriarClienteAsync(context, loja.Id, "Cliente A", "44999990000");
            ProdutoEstoqueModel produtoValido = await CriarProdutoAsync(context, loja.Id, "Produto A", "44999990001", SituacaoProduto.Estoque);
            ProdutoEstoqueModel produtoInvalido = await CriarProdutoAsync(context, loja.Id, "Produto B", "44999990002", SituacaoProduto.Vendido);

            MovimentacaoService service = new(context);
            ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(new CriarMovimentacaoCommand
            {
                Tipo = TipoMovimentacao.Emprestimo,
                Data = new DateTime(2026, 4, 8, 12, 0, 0, DateTimeKind.Utc),
                ClienteId = cliente.Id,
                LojaId = loja.Id,
                ProdutoIds = [produtoValido.Id, produtoInvalido.Id]
            }, new CriarMovimentacaoParametros { UsuarioId = loja.UsuarioId }));

            Assert.Contains(produtoInvalido.Id.ToString(), exception.Message);
            Assert.Contains(SituacaoProduto.Estoque.ToString(), exception.Message);
            Assert.Empty(context.Movimentacoes);
            Assert.Empty(context.MovimentacoesProdutos);
            Assert.Equal(SituacaoProduto.Estoque, (await context.ProdutosEstoque.SingleAsync(item => item.Id == produtoValido.Id)).Situacao);
            Assert.Equal(SituacaoProduto.Vendido, (await context.ProdutosEstoque.SingleAsync(item => item.Id == produtoInvalido.Id)).Situacao);
        }

        [Fact]
        public async Task CreateAsyncDeveImpedirCriacaoQuandoLojaNaoPertencerAoUsuarioAutenticado()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");
            ClienteModel cliente = await CriarClienteAsync(context, loja.Id, "Cliente A", "44999990000");
            ProdutoEstoqueModel produto = await CriarProdutoAsync(context, loja.Id, "Produto A", "44999990001");

            MovimentacaoService service = new(context);
            _ = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.CreateAsync(new CriarMovimentacaoCommand
            {
                Tipo = TipoMovimentacao.Venda,
                Data = new DateTime(2026, 4, 8, 12, 0, 0, DateTimeKind.Utc),
                ClienteId = cliente.Id,
                LojaId = loja.Id,
                ProdutoIds = [produto.Id]
            }, new CriarMovimentacaoParametros { UsuarioId = 9999 }));
        }

        [Fact]
        public async Task CreateAsyncDevePersistirTabelaAuxiliarComUmRegistroParaCadaProdutoInformado()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");
            ClienteModel cliente = await CriarClienteAsync(context, loja.Id, "Cliente A", "44999990000");
            ProdutoEstoqueModel produtoA = await CriarProdutoAsync(context, loja.Id, "Produto A", "44999990001", SituacaoProduto.Vendido);
            ProdutoEstoqueModel produtoB = await CriarProdutoAsync(context, loja.Id, "Produto B", "44999990002", SituacaoProduto.Vendido);
            ProdutoEstoqueModel produtoC = await CriarProdutoAsync(context, loja.Id, "Produto C", "44999990003", SituacaoProduto.Vendido);

            MovimentacaoService service = new(context);
            MovimentacaoDto resultado = await service.CreateAsync(new CriarMovimentacaoCommand
            {
                Tipo = TipoMovimentacao.DevolucaoVenda,
                Data = new DateTime(2026, 4, 8, 12, 0, 0, DateTimeKind.Utc),
                ClienteId = cliente.Id,
                LojaId = loja.Id,
                ProdutoIds = [produtoA.Id, produtoB.Id, produtoC.Id]
            }, new CriarMovimentacaoParametros { UsuarioId = loja.UsuarioId });

            List<MovimentacaoProdutoModel> relacionamentos = await context.MovimentacoesProdutos
                .Where(item => item.MovimentacaoId == resultado.Id)
                .ToListAsync();

            Assert.Equal(3, relacionamentos.Count);
            Assert.Contains(relacionamentos, item => item.ProdutoId == produtoA.Id);
            Assert.Contains(relacionamentos, item => item.ProdutoId == produtoB.Id);
            Assert.Contains(relacionamentos, item => item.ProdutoId == produtoC.Id);
        }

        [Fact]
        public async Task CreateAsyncDeveAcionarPagamentoServiceQuandoMovimentacaoForVenda()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");
            ClienteModel cliente = await CriarClienteAsync(context, loja.Id, "Cliente A", "44999990000");
            ProdutoEstoqueModel produto = await CriarProdutoAsync(context, loja.Id, "Produto A", "44999990001");

            CriarMovimentacaoCommand command = new()
            {
                Tipo = TipoMovimentacao.Venda,
                Data = new DateTime(2026, 4, 8, 12, 0, 0, DateTimeKind.Utc),
                ClienteId = cliente.Id,
                LojaId = loja.Id,
                ProdutoIds = [produto.Id]
            };

            FakePagamentoService pagamentoService = new();
            MovimentacaoService service = new(context, pagamentoService);
            _ = await service.CreateAsync(command, new CriarMovimentacaoParametros { UsuarioId = loja.UsuarioId });

            Assert.Single(pagamentoService.Commands);
            Assert.Equal(TipoMovimentacao.Venda, pagamentoService.Commands[0].TipoMovimentacao);
            Assert.Equal(cliente.Id, pagamentoService.Commands[0].ClienteId);
            Assert.Equal(loja.Id, pagamentoService.Commands[0].LojaId);
            Assert.Equal([produto.Id], pagamentoService.Commands[0].ProdutoIds);
        }

        [Fact]
        public async Task CreateAsyncDeveAcionarPagamentoServiceQuandoMovimentacaoForDevolucaoVenda()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");
            ClienteModel cliente = await CriarClienteAsync(context, loja.Id, "Cliente A", "44999990000");
            ProdutoEstoqueModel produto = await CriarProdutoAsync(context, loja.Id, "Produto A", "44999990001", SituacaoProduto.Vendido);

            CriarMovimentacaoCommand command = new()
            {
                Tipo = TipoMovimentacao.DevolucaoVenda,
                Data = new DateTime(2026, 4, 8, 12, 0, 0, DateTimeKind.Utc),
                ClienteId = cliente.Id,
                LojaId = loja.Id,
                ProdutoIds = [produto.Id]
            };

            FakePagamentoService pagamentoService = new();
            MovimentacaoService service = new(context, pagamentoService);
            _ = await service.CreateAsync(command, new CriarMovimentacaoParametros { UsuarioId = loja.UsuarioId });

            Assert.Single(pagamentoService.Commands);
            Assert.Equal(TipoMovimentacao.DevolucaoVenda, pagamentoService.Commands[0].TipoMovimentacao);
            Assert.Equal(cliente.Id, pagamentoService.Commands[0].ClienteId);
            Assert.Equal(loja.Id, pagamentoService.Commands[0].LojaId);
            Assert.Equal([produto.Id], pagamentoService.Commands[0].ProdutoIds);
        }

        [Fact]
        public async Task CreateAsyncDevePermitirVendaDeProdutoEmprestadoQuandoForParaOMesmoCliente()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");
            ClienteModel cliente = await CriarClienteAsync(context, loja.Id, "Cliente A", "44999990000");
            ProdutoEstoqueModel produto = await CriarProdutoAsync(context, loja.Id, "Produto A", "44999990001", SituacaoProduto.Emprestado);
            _ = await CriarMovimentacaoExistenteAsync(context, loja.Id, cliente.Id, TipoMovimentacao.Emprestimo, produto.Id);

            MovimentacaoService service = new(context);
            MovimentacaoDto resultado = await service.CreateAsync(new CriarMovimentacaoCommand
            {
                Tipo = TipoMovimentacao.Venda,
                Data = new DateTime(2026, 4, 9, 12, 0, 0, DateTimeKind.Utc),
                ClienteId = cliente.Id,
                LojaId = loja.Id,
                ProdutoIds = [produto.Id]
            }, new CriarMovimentacaoParametros { UsuarioId = loja.UsuarioId });

            Assert.True(resultado.Id > 0);
            Assert.Equal(TipoMovimentacao.Venda, (await context.Movimentacoes.OrderBy(item => item.Id).LastAsync()).Tipo);
            Assert.Equal(SituacaoProduto.Vendido, (await context.ProdutosEstoque.SingleAsync(item => item.Id == produto.Id)).Situacao);
        }

        [Fact]
        public async Task CreateAsyncDeveImpedirVendaDeProdutoEmprestadoQuandoForParaClienteDiferente()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");
            ClienteModel clienteEmprestimo = await CriarClienteAsync(context, loja.Id, "Cliente A", "44999990000");
            ClienteModel clienteVenda = await CriarClienteAsync(context, loja.Id, "Cliente B", "44999990003");
            ProdutoEstoqueModel produto = await CriarProdutoAsync(context, loja.Id, "Produto A", "44999990001", SituacaoProduto.Emprestado);
            _ = await CriarMovimentacaoExistenteAsync(context, loja.Id, clienteEmprestimo.Id, TipoMovimentacao.Emprestimo, produto.Id);

            MovimentacaoService service = new(context);
            ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(new CriarMovimentacaoCommand
            {
                Tipo = TipoMovimentacao.Venda,
                Data = new DateTime(2026, 4, 9, 12, 0, 0, DateTimeKind.Utc),
                ClienteId = clienteVenda.Id,
                LojaId = loja.Id,
                ProdutoIds = [produto.Id]
            }, new CriarMovimentacaoParametros { UsuarioId = loja.UsuarioId }));

            Assert.Contains(produto.Id.ToString(), exception.Message);
            Assert.Contains(SituacaoProduto.Emprestado.ToString(), exception.Message);
            Assert.Equal(SituacaoProduto.Emprestado, (await context.ProdutosEstoque.SingleAsync(item => item.Id == produto.Id)).Situacao);
        }

        [Fact]
        public async Task CreateAsyncDevePermitirDevolucaoVendaQuandoUltimaMovimentacaoForVendaDoMesmoCliente()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");
            ClienteModel cliente = await CriarClienteAsync(context, loja.Id, "Cliente A", "44999990000");
            ProdutoEstoqueModel produto = await CriarProdutoAsync(context, loja.Id, "Produto A", "44999990001", SituacaoProduto.Vendido);
            _ = await CriarMovimentacaoExistenteAsync(context, loja.Id, cliente.Id, TipoMovimentacao.Venda, produto.Id);

            MovimentacaoService service = new(context);
            MovimentacaoDto resultado = await service.CreateAsync(new CriarMovimentacaoCommand
            {
                Tipo = TipoMovimentacao.DevolucaoVenda,
                Data = new DateTime(2026, 4, 9, 12, 0, 0, DateTimeKind.Utc),
                ClienteId = cliente.Id,
                LojaId = loja.Id,
                ProdutoIds = [produto.Id]
            }, new CriarMovimentacaoParametros { UsuarioId = loja.UsuarioId });

            Assert.True(resultado.Id > 0);
            Assert.Equal(TipoMovimentacao.DevolucaoVenda, (await context.Movimentacoes.OrderBy(item => item.Id).LastAsync()).Tipo);
            Assert.Equal(SituacaoProduto.Estoque, (await context.ProdutosEstoque.SingleAsync(item => item.Id == produto.Id)).Situacao);
        }

        [Fact]
        public async Task CreateAsyncDeveImpedirDevolucaoVendaQuandoUltimaMovimentacaoForVendaDeClienteDiferente()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");
            ClienteModel clienteVenda = await CriarClienteAsync(context, loja.Id, "Cliente A", "44999990000");
            ClienteModel clienteDevolucao = await CriarClienteAsync(context, loja.Id, "Cliente B", "44999990003");
            ProdutoEstoqueModel produto = await CriarProdutoAsync(context, loja.Id, "Produto A", "44999990001", SituacaoProduto.Vendido);
            _ = await CriarMovimentacaoExistenteAsync(context, loja.Id, clienteVenda.Id, TipoMovimentacao.Venda, produto.Id);

            MovimentacaoService service = new(context);
            ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(new CriarMovimentacaoCommand
            {
                Tipo = TipoMovimentacao.DevolucaoVenda,
                Data = new DateTime(2026, 4, 9, 12, 0, 0, DateTimeKind.Utc),
                ClienteId = clienteDevolucao.Id,
                LojaId = loja.Id,
                ProdutoIds = [produto.Id]
            }, new CriarMovimentacaoParametros { UsuarioId = loja.UsuarioId }));

            Assert.Contains(produto.Id.ToString(), exception.Message);
            Assert.Contains(TipoMovimentacao.Venda.ToString(), exception.Message);
            Assert.Equal(SituacaoProduto.Vendido, (await context.ProdutosEstoque.SingleAsync(item => item.Id == produto.Id)).Situacao);
        }

        [Fact]
        public async Task CreateAsyncDevePermitirDevolucaoEmprestimoQuandoUltimaMovimentacaoForEmprestimoDoMesmoCliente()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");
            ClienteModel cliente = await CriarClienteAsync(context, loja.Id, "Cliente A", "44999990000");
            ProdutoEstoqueModel produto = await CriarProdutoAsync(context, loja.Id, "Produto A", "44999990001", SituacaoProduto.Emprestado);
            _ = await CriarMovimentacaoExistenteAsync(context, loja.Id, cliente.Id, TipoMovimentacao.Emprestimo, produto.Id);

            MovimentacaoService service = new(context);
            MovimentacaoDto resultado = await service.CreateAsync(new CriarMovimentacaoCommand
            {
                Tipo = TipoMovimentacao.DevolucaoEmprestimo,
                Data = new DateTime(2026, 4, 9, 12, 0, 0, DateTimeKind.Utc),
                ClienteId = cliente.Id,
                LojaId = loja.Id,
                ProdutoIds = [produto.Id]
            }, new CriarMovimentacaoParametros { UsuarioId = loja.UsuarioId });

            Assert.True(resultado.Id > 0);
            Assert.Equal(TipoMovimentacao.DevolucaoEmprestimo, (await context.Movimentacoes.OrderBy(item => item.Id).LastAsync()).Tipo);
            Assert.Equal(SituacaoProduto.Estoque, (await context.ProdutosEstoque.SingleAsync(item => item.Id == produto.Id)).Situacao);
        }

        [Fact]
        public async Task CreateAsyncDeveImpedirDevolucaoEmprestimoQuandoUltimaMovimentacaoForEmprestimoDeClienteDiferente()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");
            ClienteModel clienteEmprestimo = await CriarClienteAsync(context, loja.Id, "Cliente A", "44999990000");
            ClienteModel clienteDevolucao = await CriarClienteAsync(context, loja.Id, "Cliente B", "44999990003");
            ProdutoEstoqueModel produto = await CriarProdutoAsync(context, loja.Id, "Produto A", "44999990001", SituacaoProduto.Emprestado);
            _ = await CriarMovimentacaoExistenteAsync(context, loja.Id, clienteEmprestimo.Id, TipoMovimentacao.Emprestimo, produto.Id);

            MovimentacaoService service = new(context);
            ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(new CriarMovimentacaoCommand
            {
                Tipo = TipoMovimentacao.DevolucaoEmprestimo,
                Data = new DateTime(2026, 4, 9, 12, 0, 0, DateTimeKind.Utc),
                ClienteId = clienteDevolucao.Id,
                LojaId = loja.Id,
                ProdutoIds = [produto.Id]
            }, new CriarMovimentacaoParametros { UsuarioId = loja.UsuarioId }));

            Assert.Contains(produto.Id.ToString(), exception.Message);
            Assert.Contains(TipoMovimentacao.Emprestimo.ToString(), exception.Message);
            Assert.Equal(SituacaoProduto.Emprestado, (await context.ProdutosEstoque.SingleAsync(item => item.Id == produto.Id)).Situacao);
        }

        private static async Task<LojaModel> CriarLojaAsync(RenovaDbContext context, string nomeLoja, string emailUsuario)
        {
            UsuarioModel usuario = new()
            {
                Nome = "Usuario de Teste",
                Email = emailUsuario,
                SenhaHash = "hash"
            };

            _ = context.Usuarios.Add(usuario);
            _ = await context.SaveChangesAsync();

            LojaModel loja = new()
            {
                Nome = nomeLoja,
                UsuarioId = usuario.Id
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

        private static async Task<ProdutoEstoqueModel> CriarProdutoAsync(RenovaDbContext context, int lojaId, string descricao, string contatoFornecedor, SituacaoProduto situacao = SituacaoProduto.Estoque)
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
                Situacao = situacao,
                Consignado = false
            };

            _ = context.ProdutosEstoque.Add(item);
            _ = await context.SaveChangesAsync();

            return item;
        }

        private static async Task<MovimentacaoModel> CriarMovimentacaoExistenteAsync(
            RenovaDbContext context,
            int lojaId,
            int clienteId,
            TipoMovimentacao tipo,
            params int[] produtoIds)
        {
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

        private sealed class FakePagamentoService : IPagamentoService
        {
            public List<global::Renova.Service.Commands.Pagamento.CriarPagamentoCommand> Commands { get; } = [];

            public Task<IReadOnlyList<PagamentoDto>> CreateAsync(global::Renova.Service.Commands.Pagamento.CriarPagamentoCommand request, CancellationToken cancellationToken = default)
            {
                _ = cancellationToken;
                Commands.Add(new global::Renova.Service.Commands.Pagamento.CriarPagamentoCommand
                {
                    MovimentacaoId = request.MovimentacaoId,
                    TipoMovimentacao = request.TipoMovimentacao,
                    LojaId = request.LojaId,
                    ClienteId = request.ClienteId,
                    ProdutoIds = [.. request.ProdutoIds],
                    Data = request.Data
                });

                return Task.FromResult<IReadOnlyList<PagamentoDto>>([]);
            }
        }
    }
}
