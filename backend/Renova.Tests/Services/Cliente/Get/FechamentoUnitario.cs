using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;

using Renova.Domain.Model;
using Renova.Persistence;
using Renova.Service.Parameters.Cliente;
using Renova.Service.Queries.Cliente;
using Renova.Service.Services.Cliente;
using Renova.Tests.Infrastructure;

namespace Renova.Tests.Services.Cliente.Get
{
    public class FechamentoUnitario : InMemoryDbContextTestBase<RenovaDbContext>
    {
        protected override RenovaDbContext CriarContexto(DbContextOptions<RenovaDbContext> options)
        {
            return new RenovaDbContext(options);
        }

        [Fact]
        public async Task ExportProductClosingAsyncDeveGerarUmaAbaPorClienteElegivelComProdutosDoPeriodo()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = await CriarUsuarioAsync(context, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(context, usuario.Id, "Loja Centro");
            ClienteModel fornecedor = await CriarClienteAsync(context, loja.Id, "Ana Fornecedora", "44999990000");

            _ = await CriarConfigLojaAsync(context, loja.Id, 40m, 60m);
            ProdutoEstoqueModel produto = await CriarProdutoAsync(
                context,
                loja.Id,
                fornecedor.Id,
                "Vestido",
                100m,
                new DateTime(2026, 3, 5, 10, 0, 0, DateTimeKind.Utc));

            _ = await CriarPagamentoAsync(
                context,
                loja.Id,
                fornecedor.Id,
                NaturezaPagamento.Receber,
                20m,
                new DateTime(2026, 2, 20, 12, 0, 0, DateTimeKind.Utc));

            _ = await CriarVendaAsync(
                context,
                loja.Id,
                fornecedor.Id,
                produto,
                new DateTime(2026, 3, 12, 14, 0, 0, DateTimeKind.Utc),
                10m);

            ClienteService service = new(context);
            byte[] arquivo = await service.ExportProductClosingAsync(
                new ExportarFechamentoClientesQuery
                {
                    LojaId = loja.Id,
                    DataInicial = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
                    DataFinal = new DateTime(2026, 3, 31, 23, 59, 59, DateTimeKind.Utc)
                },
                new ObterClientesParametros
                {
                    UsuarioId = usuario.Id
                });

            using MemoryStream stream = new(arquivo);
            using XLWorkbook workbook = new(stream);

            IXLWorksheet worksheet = Assert.Single(workbook.Worksheets);
            Assert.Equal("Ana Fornecedora", worksheet.Name);
            Assert.Contains("LOJA CENTRO", worksheet.Cell("A1").GetString(), StringComparison.Ordinal);
            Assert.Contains("Ana Fornecedora", worksheet.Cell("A7").GetString(), StringComparison.Ordinal);
            Assert.Equal("Vestido", worksheet.Cell("C14").GetString());
            Assert.Equal("Vendido", worksheet.Cell("I14").GetString());
            Assert.Equal("Sim", worksheet.Cell("J14").GetString());
        }

        [Fact]
        public async Task ExportMovementClosingAsyncDeveGerarVendasComprasEResumoDaContaCredito()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = await CriarUsuarioAsync(context, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(context, usuario.Id, "Loja Centro");
            ClienteModel fornecedor = await CriarClienteAsync(context, loja.Id, "Ana Fornecedora", "44999990000");
            ClienteModel comprador = await CriarClienteAsync(context, loja.Id, "Beatriz Compradora", "44999990001");
            ClienteModel outroFornecedor = await CriarClienteAsync(context, loja.Id, "Carla Fornecedora", "44999990002");

            _ = await CriarConfigLojaAsync(context, loja.Id, 40m, 60m);
            _ = await CriarCreditoAsync(context, loja.Id, fornecedor.Id, -15m);

            ProdutoEstoqueModel produtoFornecedor = await CriarProdutoAsync(
                context,
                loja.Id,
                fornecedor.Id,
                "Vestido",
                100m,
                new DateTime(2026, 3, 5, 10, 0, 0, DateTimeKind.Utc));

            ProdutoEstoqueModel produtoCompra = await CriarProdutoAsync(
                context,
                loja.Id,
                outroFornecedor.Id,
                "Blusa",
                80m,
                new DateTime(2026, 3, 6, 10, 0, 0, DateTimeKind.Utc));

            _ = await CriarVendaAsync(
                context,
                loja.Id,
                comprador.Id,
                produtoFornecedor,
                new DateTime(2026, 3, 12, 14, 0, 0, DateTimeKind.Utc),
                10m);

            _ = await CriarVendaAsync(
                context,
                loja.Id,
                fornecedor.Id,
                produtoCompra,
                new DateTime(2026, 3, 15, 14, 0, 0, DateTimeKind.Utc),
                0m);

            ClienteService service = new(context);
            byte[] arquivo = await service.ExportMovementClosingAsync(
                new ExportarFechamentoClientesQuery
                {
                    LojaId = loja.Id,
                    DataInicial = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
                    DataFinal = new DateTime(2026, 3, 31, 23, 59, 59, DateTimeKind.Utc)
                },
                new ObterClientesParametros
                {
                    UsuarioId = usuario.Id
                });

            using MemoryStream stream = new(arquivo);
            using XLWorkbook workbook = new(stream);

            Assert.Equal(3, workbook.Worksheets.Count);
            IXLWorksheet worksheet = workbook.Worksheet("Ana Fornecedora");
            Assert.Contains("R$ -15,00", worksheet.Cell("G7").GetString(), StringComparison.Ordinal);
            Assert.Contains("R$ 54,00", worksheet.Cell("J7").GetString(), StringComparison.Ordinal);
            Assert.Contains("R$ 39,00", worksheet.Cell("G11").GetString(), StringComparison.Ordinal);
            Assert.Contains("Vestido", worksheet.CellsUsed().Select(cell => cell.GetString()).Where(value => !string.IsNullOrWhiteSpace(value)));
            Assert.Contains("Blusa", worksheet.CellsUsed().Select(cell => cell.GetString()).Where(value => !string.IsNullOrWhiteSpace(value)));
            Assert.Contains("Beatriz Compradora", worksheet.CellsUsed().Select(cell => cell.GetString()).Where(value => !string.IsNullOrWhiteSpace(value)));
            Assert.Contains("Carla Fornecedora", worksheet.CellsUsed().Select(cell => cell.GetString()).Where(value => !string.IsNullOrWhiteSpace(value)));
        }

        [Fact]
        public async Task ExportProductClosingAsyncNaoDeveIncluirClienteMarcadoComoDoacao()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = await CriarUsuarioAsync(context, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(context, usuario.Id, "Loja Centro");
            ClienteModel fornecedorDoacao = await CriarClienteAsync(context, loja.Id, "Cliente Doacao", "44999990001", true);

            _ = await CriarConfigLojaAsync(context, loja.Id, 40m, 60m);
            ProdutoEstoqueModel produto = await CriarProdutoAsync(
                context,
                loja.Id,
                fornecedorDoacao.Id,
                "Blusa",
                80m,
                new DateTime(2026, 3, 4, 10, 0, 0, DateTimeKind.Utc));

            _ = await CriarVendaAsync(
                context,
                loja.Id,
                fornecedorDoacao.Id,
                produto,
                new DateTime(2026, 3, 6, 14, 0, 0, DateTimeKind.Utc),
                0m);

            ClienteService service = new(context);
            byte[] arquivo = await service.ExportProductClosingAsync(
                new ExportarFechamentoClientesQuery
                {
                    LojaId = loja.Id,
                    DataInicial = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
                    DataFinal = new DateTime(2026, 3, 31, 23, 59, 59, DateTimeKind.Utc)
                },
                new ObterClientesParametros
                {
                    UsuarioId = usuario.Id
                });

            using MemoryStream stream = new(arquivo);
            using XLWorkbook workbook = new(stream);

            IXLWorksheet worksheet = Assert.Single(workbook.Worksheets);
            Assert.Equal("Resumo", worksheet.Name);
            Assert.Contains("Nenhum cliente elegivel", worksheet.Cell("A1").GetString(), StringComparison.OrdinalIgnoreCase);
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

        private static async Task<ClienteModel> CriarClienteAsync(
            RenovaDbContext context,
            int lojaId,
            string nome,
            string contato,
            bool doacao = false)
        {
            ClienteModel cliente = new()
            {
                Nome = nome,
                Contato = contato,
                Doacao = doacao,
                LojaId = lojaId
            };

            _ = context.Clientes.Add(cliente);
            _ = await context.SaveChangesAsync();
            return cliente;
        }

        private static async Task<ConfigLojaModel> CriarConfigLojaAsync(
            RenovaDbContext context,
            int lojaId,
            decimal percentualRepasseFornecedor,
            decimal percentualRepasseVendedorCredito)
        {
            ConfigLojaModel config = new()
            {
                LojaId = lojaId,
                PercentualRepasseFornecedor = percentualRepasseFornecedor,
                PercentualRepasseVendedorCredito = percentualRepasseVendedorCredito,
                TempoPermanenciaProdutoMeses = 12
            };

            _ = context.ConfiguracoesLoja.Add(config);
            _ = await context.SaveChangesAsync();
            return config;
        }

        private static async Task<ProdutoEstoqueModel> CriarProdutoAsync(
            RenovaDbContext context,
            int lojaId,
            int fornecedorId,
            string nomeProduto,
            decimal preco,
            DateTime entrada)
        {
            ProdutoReferenciaModel produtoReferencia = new()
            {
                Valor = nomeProduto,
                LojaId = lojaId
            };

            MarcaModel marca = new()
            {
                Valor = "Marca A",
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

            _ = context.ProdutosReferencia.Add(produtoReferencia);
            _ = context.Marcas.Add(marca);
            _ = context.Tamanhos.Add(tamanho);
            _ = context.Cores.Add(cor);
            _ = await context.SaveChangesAsync();

            ProdutoEstoqueModel produto = new()
            {
                Preco = preco,
                ProdutoId = produtoReferencia.Id,
                MarcaId = marca.Id,
                TamanhoId = tamanho.Id,
                CorId = cor.Id,
                FornecedorId = fornecedorId,
                Descricao = "Obs teste",
                Entrada = entrada,
                LojaId = lojaId,
                Situacao = SituacaoProduto.Estoque,
                Consignado = true
            };

            _ = context.ProdutosEstoque.Add(produto);
            _ = await context.SaveChangesAsync();
            return produto;
        }

        private static async Task<PagamentoModel> CriarPagamentoAsync(
            RenovaDbContext context,
            int lojaId,
            int clienteId,
            NaturezaPagamento natureza,
            decimal valor,
            DateTime data)
        {
            PagamentoModel pagamento = new()
            {
                LojaId = lojaId,
                ClienteId = clienteId,
                Natureza = natureza,
                Status = StatusPagamento.Pago,
                Valor = valor,
                Data = data
            };

            _ = context.Pagamentos.Add(pagamento);
            _ = await context.SaveChangesAsync();
            return pagamento;
        }

        private static async Task<ClienteCreditoModel> CriarCreditoAsync(
            RenovaDbContext context,
            int lojaId,
            int clienteId,
            decimal valor)
        {
            ClienteCreditoModel credito = new()
            {
                LojaId = lojaId,
                ClienteId = clienteId,
                Valor = valor
            };

            _ = context.ClientesCreditos.Add(credito);
            _ = await context.SaveChangesAsync();
            return credito;
        }

        private static async Task<MovimentacaoModel> CriarVendaAsync(
            RenovaDbContext context,
            int lojaId,
            int clienteId,
            ProdutoEstoqueModel produto,
            DateTime data,
            decimal desconto)
        {
            MovimentacaoModel movimentacao = new()
            {
                LojaId = lojaId,
                ClienteId = clienteId,
                Tipo = TipoMovimentacao.Venda,
                Data = data
            };

            _ = context.Movimentacoes.Add(movimentacao);
            _ = await context.SaveChangesAsync();

            produto.Situacao = SituacaoProduto.Vendido;

            _ = context.MovimentacoesProdutos.Add(new MovimentacaoProdutoModel
            {
                MovimentacaoId = movimentacao.Id,
                ProdutoId = produto.Id,
                Desconto = desconto
            });

            _ = await context.SaveChangesAsync();
            return movimentacao;
        }
    }
}
