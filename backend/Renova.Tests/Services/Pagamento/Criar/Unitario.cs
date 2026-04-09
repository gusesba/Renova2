using Microsoft.EntityFrameworkCore;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.Pagamento;
using Renova.Service.Services.Pagamento;
using Renova.Tests.Infrastructure;

namespace Renova.Tests.Services.Pagamento.Criar
{
    public class Unitario : InMemoryDbContextTestBase<RenovaDbContext>
    {
        protected override RenovaDbContext CriarContexto(DbContextOptions<RenovaDbContext> options)
        {
            return new RenovaDbContext(options);
        }

        [Fact]
        public async Task CreateAsyncDeveCriarUmaOrdemDoClienteEUmaDoFornecedorQuandoVendaTiverUmUnicoFornecedor()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");
            ClienteModel cliente = await CriarClienteAsync(context, loja.Id, "Cliente A", "44999990000");
            ProdutoEstoqueModel produto = await CriarProdutoAsync(context, loja.Id, "Produto A", 200m, "44999990001");
            MovimentacaoModel movimentacao = await CriarMovimentacaoAsync(context, loja.Id, cliente.Id, TipoMovimentacao.Venda, [produto.Id]);
            _ = await CriarConfigLojaAsync(context, loja.Id, 45m);

            PagamentoService service = new(context);
            IReadOnlyList<PagamentoDto> resultado = await service.CreateAsync(new CriarPagamentoCommand
            {
                MovimentacaoId = movimentacao.Id,
                TipoMovimentacao = TipoMovimentacao.Venda,
                LojaId = loja.Id,
                ClienteId = cliente.Id,
                ProdutoIds = [produto.Id],
                Data = movimentacao.Data
            });

            Assert.Equal(2, resultado.Count);
            Assert.Contains(resultado, item => item.Natureza == NaturezaPagamento.Receber && item.Valor == 200m && item.ClienteId == cliente.Id);
            Assert.Contains(resultado, item => item.Natureza == NaturezaPagamento.Pagar && item.Valor == 90m && item.ClienteId == produto.FornecedorId);
            Assert.Equal(2, await context.Pagamentos.CountAsync());
        }

        [Fact]
        public async Task CreateAsyncDeveCriarUmaOrdemDoClienteEUmaDoFornecedorQuandoDevolucaoVendaTiverUmUnicoFornecedor()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");
            ClienteModel cliente = await CriarClienteAsync(context, loja.Id, "Cliente A", "44999990000");
            ProdutoEstoqueModel produto = await CriarProdutoAsync(context, loja.Id, "Produto A", 200m, "44999990001");
            MovimentacaoModel movimentacao = await CriarMovimentacaoAsync(context, loja.Id, cliente.Id, TipoMovimentacao.DevolucaoVenda, [produto.Id]);
            _ = await CriarConfigLojaAsync(context, loja.Id, 45m);

            PagamentoService service = new(context);
            IReadOnlyList<PagamentoDto> resultado = await service.CreateAsync(new CriarPagamentoCommand
            {
                MovimentacaoId = movimentacao.Id,
                TipoMovimentacao = TipoMovimentacao.DevolucaoVenda,
                LojaId = loja.Id,
                ClienteId = cliente.Id,
                ProdutoIds = [produto.Id],
                Data = movimentacao.Data
            });

            Assert.Equal(2, resultado.Count);
            Assert.Contains(resultado, item => item.Natureza == NaturezaPagamento.Pagar && item.Valor == 200m && item.ClienteId == cliente.Id);
            Assert.Contains(resultado, item => item.Natureza == NaturezaPagamento.Receber && item.Valor == 90m && item.ClienteId == produto.FornecedorId);
        }

        [Fact]
        public async Task CreateAsyncDeveCalcularValorDaOrdemDoFornecedorComBaseNoPercentualDaLoja()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");
            ClienteModel cliente = await CriarClienteAsync(context, loja.Id, "Cliente A", "44999990000");
            ProdutoEstoqueModel produtoA = await CriarProdutoAsync(context, loja.Id, "Produto A", 100m, "44999990001");
            ProdutoEstoqueModel produtoB = await CriarProdutoAsync(context, loja.Id, "Produto B", 300m, "44999990001", produtoA.FornecedorId);
            MovimentacaoModel movimentacao = await CriarMovimentacaoAsync(context, loja.Id, cliente.Id, TipoMovimentacao.Venda, [produtoA.Id, produtoB.Id]);
            _ = await CriarConfigLojaAsync(context, loja.Id, 45m);

            PagamentoService service = new(context);
            IReadOnlyList<PagamentoDto> resultado = await service.CreateAsync(new CriarPagamentoCommand
            {
                MovimentacaoId = movimentacao.Id,
                TipoMovimentacao = TipoMovimentacao.Venda,
                LojaId = loja.Id,
                ClienteId = cliente.Id,
                ProdutoIds = [produtoA.Id, produtoB.Id],
                Data = movimentacao.Data
            });

            PagamentoDto pagamentoFornecedor = Assert.Single(resultado, item =>
                item.ClienteId == produtoA.FornecedorId &&
                item.Natureza == NaturezaPagamento.Pagar);
            Assert.Equal(180m, pagamentoFornecedor.Valor);
        }

        [Fact]
        public async Task CreateAsyncDeveCriarUmaOrdemParaCadaFornecedorQuandoMovimentacaoTiverProdutosDeFornecedoresDiferentes()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");
            ClienteModel cliente = await CriarClienteAsync(context, loja.Id, "Cliente A", "44999990000");
            ProdutoEstoqueModel produtoA = await CriarProdutoAsync(context, loja.Id, "Produto A", 100m, "44999990001");
            ProdutoEstoqueModel produtoB = await CriarProdutoAsync(context, loja.Id, "Produto B", 300m, "44999990002");
            MovimentacaoModel movimentacao = await CriarMovimentacaoAsync(context, loja.Id, cliente.Id, TipoMovimentacao.Venda, [produtoA.Id, produtoB.Id]);
            _ = await CriarConfigLojaAsync(context, loja.Id, 45m);

            PagamentoService service = new(context);
            IReadOnlyList<PagamentoDto> resultado = await service.CreateAsync(new CriarPagamentoCommand
            {
                MovimentacaoId = movimentacao.Id,
                TipoMovimentacao = TipoMovimentacao.Venda,
                LojaId = loja.Id,
                ClienteId = cliente.Id,
                ProdutoIds = [produtoA.Id, produtoB.Id],
                Data = movimentacao.Data
            });

            Assert.Equal(3, resultado.Count);
            Assert.Contains(resultado, item => item.Natureza == NaturezaPagamento.Receber && item.Valor == 400m && item.ClienteId == cliente.Id);
            Assert.Contains(resultado, item => item.Natureza == NaturezaPagamento.Pagar && item.Valor == 45m && item.ClienteId == produtoA.FornecedorId);
            Assert.Contains(resultado, item => item.Natureza == NaturezaPagamento.Pagar && item.Valor == 135m && item.ClienteId == produtoB.FornecedorId);
            Assert.Equal(3, await context.Pagamentos.CountAsync());
        }

        [Fact]
        public async Task CreateAsyncDeveImpedirCriacaoQuandoNaoExistirConfiguracaoDeRepasseParaALoja()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");
            ClienteModel cliente = await CriarClienteAsync(context, loja.Id, "Cliente A", "44999990000");
            ProdutoEstoqueModel produto = await CriarProdutoAsync(context, loja.Id, "Produto A", 200m, "44999990001");
            MovimentacaoModel movimentacao = await CriarMovimentacaoAsync(context, loja.Id, cliente.Id, TipoMovimentacao.Venda, [produto.Id]);

            PagamentoService service = new(context);

            InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(new CriarPagamentoCommand
            {
                MovimentacaoId = movimentacao.Id,
                TipoMovimentacao = TipoMovimentacao.Venda,
                LojaId = loja.Id,
                ClienteId = cliente.Id,
                ProdutoIds = [produto.Id],
                Data = movimentacao.Data
            }));

            Assert.Contains("configuracao de repasse", exception.Message, StringComparison.OrdinalIgnoreCase);
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

        private static async Task<ConfigLojaModel> CriarConfigLojaAsync(RenovaDbContext context, int lojaId, decimal percentualRepasseFornecedor)
        {
            ConfigLojaModel config = new()
            {
                LojaId = lojaId,
                PercentualRepasseFornecedor = percentualRepasseFornecedor,
                PercentualRepasseVendedorCredito = 0m
            };

            _ = context.ConfiguracoesLoja.Add(config);
            _ = await context.SaveChangesAsync();

            return config;
        }

        private static async Task<MovimentacaoModel> CriarMovimentacaoAsync(RenovaDbContext context, int lojaId, int clienteId, TipoMovimentacao tipo, List<int> produtoIds)
        {
            MovimentacaoModel movimentacao = new()
            {
                Tipo = tipo,
                Data = new DateTime(2026, 4, 8, 12, 0, 0, DateTimeKind.Utc),
                ClienteId = clienteId,
                LojaId = lojaId,
                Produtos = produtoIds.Select(produtoId => new MovimentacaoProdutoModel
                {
                    ProdutoId = produtoId
                }).ToList()
            };

            _ = context.Movimentacoes.Add(movimentacao);
            _ = await context.SaveChangesAsync();

            return movimentacao;
        }

        private static async Task<ProdutoEstoqueModel> CriarProdutoAsync(RenovaDbContext context, int lojaId, string descricao, decimal preco, string contatoFornecedor, int? fornecedorId = null)
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

            ClienteModel? fornecedor = null;
            if (!fornecedorId.HasValue)
            {
                fornecedor = new ClienteModel
                {
                    Nome = $"{descricao} Fornecedor",
                    Contato = contatoFornecedor,
                    LojaId = lojaId
                };

                _ = context.Clientes.Add(fornecedor);
            }

            _ = context.ProdutosReferencia.Add(produto);
            _ = context.Marcas.Add(marca);
            _ = context.Tamanhos.Add(tamanho);
            _ = context.Cores.Add(cor);
            _ = await context.SaveChangesAsync();

            ProdutoEstoqueModel item = new()
            {
                Preco = preco,
                ProdutoId = produto.Id,
                MarcaId = marca.Id,
                TamanhoId = tamanho.Id,
                CorId = cor.Id,
                FornecedorId = fornecedorId ?? fornecedor!.Id,
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
    }
}
