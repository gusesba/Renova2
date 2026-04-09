using Microsoft.EntityFrameworkCore;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.Movimentacao;
using Renova.Service.Parameters.Movimentacao;
using Renova.Service.Services.Movimentacao;
using Renova.Tests.Infrastructure;

namespace Renova.Tests.Services.Movimentacao.Destinacao
{
    public class Unitario : InMemoryDbContextTestBase<RenovaDbContext>
    {
        protected override RenovaDbContext CriarContexto(DbContextOptions<RenovaDbContext> options)
        {
            return new RenovaDbContext(options);
        }

        [Fact]
        public async Task GetDestinacaoAsyncDeveRetornarApenasProdutosElegiveisComTipoSugeridoPorFornecedor()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");
            ClienteModel fornecedorDoacao = await CriarClienteAsync(context, loja.Id, "Fornecedor Doacao", "44999990001", true);
            ClienteModel fornecedorDevolucao = await CriarClienteAsync(context, loja.Id, "Fornecedor Devolucao", "44999990002", false);
            _ = await CriarConfigLojaAsync(context, loja.Id, 3);
            ProdutoEstoqueModel produtoElegivelDoacao = await CriarProdutoAsync(context, loja.Id, fornecedorDoacao.Id, "Produto Doacao", new DateTime(2025, 10, 1, 0, 0, 0, DateTimeKind.Utc), SituacaoProduto.Estoque);
            ProdutoEstoqueModel produtoElegivelDevolucao = await CriarProdutoAsync(context, loja.Id, fornecedorDevolucao.Id, "Produto Devolucao", new DateTime(2025, 10, 2, 0, 0, 0, DateTimeKind.Utc), SituacaoProduto.Estoque);
            _ = await CriarProdutoAsync(context, loja.Id, fornecedorDoacao.Id, "Produto Recente", DateTime.UtcNow.AddDays(-10), SituacaoProduto.Estoque);
            _ = await CriarProdutoAsync(context, loja.Id, fornecedorDevolucao.Id, "Produto Vendido", new DateTime(2025, 10, 1, 0, 0, 0, DateTimeKind.Utc), SituacaoProduto.Vendido);

            MovimentacaoService service = new(context);
            MovimentacaoDestinacaoSugestaoDto resultado = await service.GetDestinacaoAsync(
                loja.Id,
                new ObterMovimentacoesParametros { UsuarioId = loja.UsuarioId });

            Assert.Equal(loja.Id, resultado.LojaId);
            Assert.Equal(3, resultado.TempoPermanenciaProdutoMeses);
            Assert.Equal(2, resultado.Produtos.Count);
            Assert.Contains(resultado.Produtos, item => item.Id == produtoElegivelDoacao.Id && item.TipoSugerido == TipoMovimentacao.Doacao);
            Assert.Contains(resultado.Produtos, item => item.Id == produtoElegivelDevolucao.Id && item.TipoSugerido == TipoMovimentacao.DevolucaoDono);
        }

        [Fact]
        public async Task CreateDestinacaoAsyncDeveAgruparMovimentosPorFornecedorETipo()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");
            ClienteModel fornecedorA = await CriarClienteAsync(context, loja.Id, "Fornecedor A", "44999990001", false);
            ClienteModel fornecedorB = await CriarClienteAsync(context, loja.Id, "Fornecedor B", "44999990002", true);
            ProdutoEstoqueModel produtoA1 = await CriarProdutoAsync(context, loja.Id, fornecedorA.Id, "Produto A1", new DateTime(2025, 10, 1, 0, 0, 0, DateTimeKind.Utc), SituacaoProduto.Estoque);
            ProdutoEstoqueModel produtoA2 = await CriarProdutoAsync(context, loja.Id, fornecedorA.Id, "Produto A2", DateTime.UtcNow.AddDays(-5), SituacaoProduto.Estoque);
            ProdutoEstoqueModel produtoB1 = await CriarProdutoAsync(context, loja.Id, fornecedorB.Id, "Produto B1", new DateTime(2025, 10, 1, 0, 0, 0, DateTimeKind.Utc), SituacaoProduto.Estoque);

            MovimentacaoService service = new(context);
            IReadOnlyList<MovimentacaoDto> resultado = await service.CreateDestinacaoAsync(
                new CriarMovimentacaoDestinacaoCommand
                {
                    Data = new DateTime(2026, 4, 9, 12, 0, 0, DateTimeKind.Utc),
                    LojaId = loja.Id,
                    Itens =
                    [
                        new CriarMovimentacaoDestinacaoItemCommand { ProdutoId = produtoA1.Id, Tipo = TipoMovimentacao.DevolucaoDono },
                        new CriarMovimentacaoDestinacaoItemCommand { ProdutoId = produtoA2.Id, Tipo = TipoMovimentacao.Doacao },
                        new CriarMovimentacaoDestinacaoItemCommand { ProdutoId = produtoB1.Id, Tipo = TipoMovimentacao.Doacao }
                    ]
                },
                new CriarMovimentacaoParametros { UsuarioId = loja.UsuarioId });

            Assert.Equal(3, resultado.Count);
            Assert.Contains(resultado, item => item.ClienteId == fornecedorA.Id && item.Tipo == TipoMovimentacao.DevolucaoDono && item.ProdutoIds.SequenceEqual([produtoA1.Id]));
            Assert.Contains(resultado, item => item.ClienteId == fornecedorA.Id && item.Tipo == TipoMovimentacao.Doacao && item.ProdutoIds.SequenceEqual([produtoA2.Id]));
            Assert.Contains(resultado, item => item.ClienteId == fornecedorB.Id && item.Tipo == TipoMovimentacao.Doacao && item.ProdutoIds.SequenceEqual([produtoB1.Id]));

            Assert.Equal(SituacaoProduto.Devolvido, await context.ProdutosEstoque.Where(item => item.Id == produtoA1.Id).Select(item => item.Situacao).SingleAsync());
            Assert.Equal(SituacaoProduto.Doado, await context.ProdutosEstoque.Where(item => item.Id == produtoA2.Id).Select(item => item.Situacao).SingleAsync());
            Assert.Equal(SituacaoProduto.Doado, await context.ProdutosEstoque.Where(item => item.Id == produtoB1.Id).Select(item => item.Situacao).SingleAsync());
        }

        [Fact]
        public async Task CreateDestinacaoAsyncDevePermitirProdutoManualForaDaPermanenciaMinima()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");
            ClienteModel fornecedor = await CriarClienteAsync(context, loja.Id, "Fornecedor A", "44999990001", false);
            _ = await CriarConfigLojaAsync(context, loja.Id, 6);
            ProdutoEstoqueModel produtoRecente = await CriarProdutoAsync(context, loja.Id, fornecedor.Id, "Produto Recente", DateTime.UtcNow.AddDays(-7), SituacaoProduto.Estoque);

            MovimentacaoService service = new(context);
            IReadOnlyList<MovimentacaoDto> resultado = await service.CreateDestinacaoAsync(
                new CriarMovimentacaoDestinacaoCommand
                {
                    Data = new DateTime(2026, 4, 9, 12, 0, 0, DateTimeKind.Utc),
                    LojaId = loja.Id,
                    Itens =
                    [
                        new CriarMovimentacaoDestinacaoItemCommand { ProdutoId = produtoRecente.Id, Tipo = TipoMovimentacao.DevolucaoDono }
                    ]
                },
                new CriarMovimentacaoParametros { UsuarioId = loja.UsuarioId });

            Assert.Single(resultado);
            Assert.Equal(SituacaoProduto.Devolvido, await context.ProdutosEstoque.Where(item => item.Id == produtoRecente.Id).Select(item => item.Situacao).SingleAsync());
        }

        [Fact]
        public async Task CreateDestinacaoAsyncDeveFalharQuandoItemNaoForDoacaoOuDevolucaoAoDono()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");
            ClienteModel fornecedor = await CriarClienteAsync(context, loja.Id, "Fornecedor A", "44999990001", false);
            ProdutoEstoqueModel produto = await CriarProdutoAsync(context, loja.Id, fornecedor.Id, "Produto A", new DateTime(2025, 10, 1, 0, 0, 0, DateTimeKind.Utc), SituacaoProduto.Estoque);

            MovimentacaoService service = new(context);
            _ = await Assert.ThrowsAsync<ArgumentException>(() => service.CreateDestinacaoAsync(
                new CriarMovimentacaoDestinacaoCommand
                {
                    Data = new DateTime(2026, 4, 9, 12, 0, 0, DateTimeKind.Utc),
                    LojaId = loja.Id,
                    Itens =
                    [
                        new CriarMovimentacaoDestinacaoItemCommand { ProdutoId = produto.Id, Tipo = TipoMovimentacao.Venda }
                    ]
                },
                new CriarMovimentacaoParametros { UsuarioId = loja.UsuarioId }));
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

        private static async Task<ClienteModel> CriarClienteAsync(RenovaDbContext context, int lojaId, string nome, string contato, bool doacao)
        {
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

        private static async Task<ConfigLojaModel> CriarConfigLojaAsync(RenovaDbContext context, int lojaId, int tempoPermanenciaProdutoMeses)
        {
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
            RenovaDbContext context,
            int lojaId,
            int fornecedorId,
            string descricao,
            DateTime entrada,
            SituacaoProduto situacao)
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
