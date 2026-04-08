using Microsoft.EntityFrameworkCore;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Parameters.Movimentacao;
using Renova.Service.Queries.Movimentacao;
using Renova.Service.Services.Movimentacao;
using Renova.Tests.Infrastructure;

namespace Renova.Tests.Services.Movimentacao.Get
{
    public class Unitario : InMemoryDbContextTestBase<RenovaDbContext>
    {
        protected override RenovaDbContext CriarContexto(DbContextOptions<RenovaDbContext> options)
        {
            return new RenovaDbContext(options);
        }

        [Fact]
        public async Task GetAllAsyncDeveRetornarMovimentacoesPaginadasComClienteProdutosEQuantidade()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = await CriarUsuarioAsync(context, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(context, usuario.Id, "Loja Centro");
            LojaModel outraLoja = await CriarLojaAsync(context, usuario.Id, "Loja Bairro");
            ClienteModel cliente = await CriarClienteAsync(context, loja.Id, "Ana Paula", "44999990000");
            ClienteModel clienteOutraLoja = await CriarClienteAsync(context, outraLoja.Id, "Cliente Externo", "44999990009");
            ProdutoEstoqueModel produtoA = await CriarProdutoAsync(context, loja.Id, "Vestido Azul", "44999990001");
            ProdutoEstoqueModel produtoB = await CriarProdutoAsync(context, loja.Id, "Blazer Preto", "44999990002");
            ProdutoEstoqueModel produtoC = await CriarProdutoAsync(context, outraLoja.Id, "Produto Outra Loja", "44999990003");

            MovimentacaoModel movimento = await CriarMovimentacaoAsync(context, loja.Id, cliente.Id, TipoMovimentacao.Venda, new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc), produtoA.Id, produtoB.Id);
            _ = await CriarMovimentacaoAsync(context, outraLoja.Id, clienteOutraLoja.Id, TipoMovimentacao.Emprestimo, new DateTime(2026, 4, 2, 12, 0, 0, DateTimeKind.Utc), produtoC.Id);

            MovimentacaoService service = new(context);
            PaginacaoDto<MovimentacaoBuscaDto> resultado = await service.GetAllAsync(
                new ObterMovimentacoesQuery { LojaId = loja.Id },
                new ObterMovimentacoesParametros { UsuarioId = usuario.Id });

            Assert.Equal(1, resultado.TotalItens);
            Assert.Equal(1, resultado.Pagina);
            Assert.Equal(10, resultado.TamanhoPagina);
            Assert.Equal(1, resultado.TotalPaginas);

            MovimentacaoBuscaDto item = Assert.Single(resultado.Itens);
            Assert.Equal(movimento.Id, item.Id);
            Assert.Equal(TipoMovimentacao.Venda, item.Tipo);
            Assert.Equal(cliente.Id, item.ClienteId);
            Assert.Equal("Ana Paula", item.Cliente);
            Assert.Equal(2, item.QuantidadeProdutos);
            Assert.Collection(item.Produtos,
                produto =>
                {
                    Assert.Equal(produtoA.Id, produto.Id);
                    Assert.Equal("Vestido Azul", produto.Descricao);
                    Assert.Equal("Vestido Azul Referencia", produto.Produto);
                    Assert.Equal("Vestido Azul Marca", produto.Marca);
                    Assert.Equal("M", produto.Tamanho);
                    Assert.Equal("Azul", produto.Cor);
                    Assert.Equal("Vestido Azul Fornecedor", produto.Fornecedor);
                },
                produto =>
                {
                    Assert.Equal(produtoB.Id, produto.Id);
                    Assert.Equal("Blazer Preto", produto.Descricao);
                    Assert.Equal("Blazer Preto Referencia", produto.Produto);
                    Assert.Equal("Blazer Preto Marca", produto.Marca);
                    Assert.Equal("M", produto.Tamanho);
                    Assert.Equal("Azul", produto.Cor);
                    Assert.Equal("Blazer Preto Fornecedor", produto.Fornecedor);
                });
        }

        [Fact]
        public async Task GetAllAsyncDeveFiltrarPorPeriodoQuandoDatasForemInformadas()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = await CriarUsuarioAsync(context, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(context, usuario.Id, "Loja Centro");
            ClienteModel cliente = await CriarClienteAsync(context, loja.Id, "Cliente Periodo", "44999990000");
            ProdutoEstoqueModel produtoA = await CriarProdutoAsync(context, loja.Id, "Produto Inicio", "44999990001");
            ProdutoEstoqueModel produtoB = await CriarProdutoAsync(context, loja.Id, "Produto Meio", "44999990002");
            ProdutoEstoqueModel produtoC = await CriarProdutoAsync(context, loja.Id, "Produto Fim", "44999990003");

            _ = await CriarMovimentacaoAsync(context, loja.Id, cliente.Id, TipoMovimentacao.Venda, new DateTime(2026, 3, 31, 12, 0, 0, DateTimeKind.Utc), produtoA.Id);
            MovimentacaoModel esperado = await CriarMovimentacaoAsync(context, loja.Id, cliente.Id, TipoMovimentacao.Emprestimo, new DateTime(2026, 4, 2, 12, 0, 0, DateTimeKind.Utc), produtoB.Id);
            _ = await CriarMovimentacaoAsync(context, loja.Id, cliente.Id, TipoMovimentacao.Doacao, new DateTime(2026, 4, 5, 12, 0, 0, DateTimeKind.Utc), produtoC.Id);

            MovimentacaoService service = new(context);
            PaginacaoDto<MovimentacaoBuscaDto> resultado = await service.GetAllAsync(
                new ObterMovimentacoesQuery
                {
                    LojaId = loja.Id,
                    DataInicial = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc),
                    DataFinal = new DateTime(2026, 4, 3, 0, 0, 0, DateTimeKind.Utc)
                },
                new ObterMovimentacoesParametros { UsuarioId = usuario.Id });

            MovimentacaoBuscaDto item = Assert.Single(resultado.Itens);
            Assert.Equal(esperado.Id, item.Id);
        }

        [Fact]
        public async Task GetAllAsyncDeveFiltrarPorNomeDoClienteQuandoFiltroForInformado()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = await CriarUsuarioAsync(context, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(context, usuario.Id, "Loja Centro");
            ClienteModel clienteAna = await CriarClienteAsync(context, loja.Id, "Ana Paula", "44999990000");
            ClienteModel clienteBeatriz = await CriarClienteAsync(context, loja.Id, "Beatriz Lima", "44999990001");
            ProdutoEstoqueModel produtoA = await CriarProdutoAsync(context, loja.Id, "Produto Ana", "44999990002");
            ProdutoEstoqueModel produtoB = await CriarProdutoAsync(context, loja.Id, "Produto Beatriz", "44999990003");

            MovimentacaoModel esperado = await CriarMovimentacaoAsync(context, loja.Id, clienteAna.Id, TipoMovimentacao.Venda, new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc), produtoA.Id);
            _ = await CriarMovimentacaoAsync(context, loja.Id, clienteBeatriz.Id, TipoMovimentacao.Emprestimo, new DateTime(2026, 4, 2, 12, 0, 0, DateTimeKind.Utc), produtoB.Id);

            MovimentacaoService service = new(context);
            PaginacaoDto<MovimentacaoBuscaDto> resultado = await service.GetAllAsync(
                new ObterMovimentacoesQuery
                {
                    LojaId = loja.Id,
                    Cliente = "ana"
                },
                new ObterMovimentacoesParametros { UsuarioId = usuario.Id });

            MovimentacaoBuscaDto item = Assert.Single(resultado.Itens);
            Assert.Equal(esperado.Id, item.Id);
            Assert.Equal("Ana Paula", item.Cliente);
        }

        [Fact]
        public async Task GetAllAsyncDeveFiltrarPorTipoQuandoFiltroForInformado()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = await CriarUsuarioAsync(context, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(context, usuario.Id, "Loja Centro");
            ClienteModel cliente = await CriarClienteAsync(context, loja.Id, "Cliente Tipo", "44999990000");
            ProdutoEstoqueModel produtoA = await CriarProdutoAsync(context, loja.Id, "Produto Venda", "44999990001");
            ProdutoEstoqueModel produtoB = await CriarProdutoAsync(context, loja.Id, "Produto Emprestimo", "44999990002");

            MovimentacaoModel esperado = await CriarMovimentacaoAsync(context, loja.Id, cliente.Id, TipoMovimentacao.Venda, new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc), produtoA.Id);
            _ = await CriarMovimentacaoAsync(context, loja.Id, cliente.Id, TipoMovimentacao.Emprestimo, new DateTime(2026, 4, 2, 12, 0, 0, DateTimeKind.Utc), produtoB.Id);

            MovimentacaoService service = new(context);
            PaginacaoDto<MovimentacaoBuscaDto> resultado = await service.GetAllAsync(
                new ObterMovimentacoesQuery
                {
                    LojaId = loja.Id,
                    Tipo = TipoMovimentacao.Venda
                },
                new ObterMovimentacoesParametros { UsuarioId = usuario.Id });

            MovimentacaoBuscaDto item = Assert.Single(resultado.Itens);
            Assert.Equal(esperado.Id, item.Id);
            Assert.Equal(TipoMovimentacao.Venda, item.Tipo);
        }

        [Fact]
        public async Task GetAllAsyncDeveOrdenarPelosCamposSuportadosQuandoOrdenacaoForInformada()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = await CriarUsuarioAsync(context, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(context, usuario.Id, "Loja Centro");
            ClienteModel clienteCarlos = await CriarClienteAsync(context, loja.Id, "Carlos", "44999990000");
            ClienteModel clienteAna = await CriarClienteAsync(context, loja.Id, "Ana", "44999990001");
            ClienteModel clienteBruno = await CriarClienteAsync(context, loja.Id, "Bruno", "44999990004");
            ProdutoEstoqueModel produtoA = await CriarProdutoAsync(context, loja.Id, "Produto A", "44999990002");
            ProdutoEstoqueModel produtoB = await CriarProdutoAsync(context, loja.Id, "Produto B", "44999990003");
            ProdutoEstoqueModel produtoC = await CriarProdutoAsync(context, loja.Id, "Produto C", "44999990005");

            _ = await CriarMovimentacaoAsync(context, loja.Id, clienteCarlos.Id, TipoMovimentacao.Emprestimo, new DateTime(2026, 4, 3, 12, 0, 0, DateTimeKind.Utc), produtoA.Id);
            _ = await CriarMovimentacaoAsync(context, loja.Id, clienteAna.Id, TipoMovimentacao.Venda, new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc), produtoB.Id);
            _ = await CriarMovimentacaoAsync(context, loja.Id, clienteBruno.Id, TipoMovimentacao.Doacao, new DateTime(2026, 4, 2, 12, 0, 0, DateTimeKind.Utc), produtoC.Id);

            MovimentacaoService service = new(context);

            PaginacaoDto<MovimentacaoBuscaDto> ordenadoPorData = await service.GetAllAsync(
                new ObterMovimentacoesQuery { LojaId = loja.Id, OrdenarPor = "data", Direcao = "desc" },
                new ObterMovimentacoesParametros { UsuarioId = usuario.Id });

            Assert.Collection(ordenadoPorData.Itens,
                item => Assert.Equal(new DateTime(2026, 4, 3, 12, 0, 0, DateTimeKind.Utc), item.Data),
                item => Assert.Equal(new DateTime(2026, 4, 2, 12, 0, 0, DateTimeKind.Utc), item.Data),
                item => Assert.Equal(new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc), item.Data));

            PaginacaoDto<MovimentacaoBuscaDto> ordenadoPorCliente = await service.GetAllAsync(
                new ObterMovimentacoesQuery { LojaId = loja.Id, OrdenarPor = "cliente", Direcao = "asc" },
                new ObterMovimentacoesParametros { UsuarioId = usuario.Id });

            Assert.Collection(ordenadoPorCliente.Itens,
                item => Assert.Equal("Ana", item.Cliente),
                item => Assert.Equal("Bruno", item.Cliente),
                item => Assert.Equal("Carlos", item.Cliente));

            PaginacaoDto<MovimentacaoBuscaDto> ordenadoPorTipo = await service.GetAllAsync(
                new ObterMovimentacoesQuery { LojaId = loja.Id, OrdenarPor = "tipo", Direcao = "asc" },
                new ObterMovimentacoesParametros { UsuarioId = usuario.Id });

            Assert.Collection(ordenadoPorTipo.Itens,
                item => Assert.Equal(TipoMovimentacao.Venda, item.Tipo),
                item => Assert.Equal(TipoMovimentacao.Emprestimo, item.Tipo),
                item => Assert.Equal(TipoMovimentacao.Doacao, item.Tipo));
        }

        [Fact]
        public async Task GetAllAsyncDeveCombinarFiltrosOrdenacaoEPaginacao()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = await CriarUsuarioAsync(context, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(context, usuario.Id, "Loja Centro");
            ClienteModel clienteAnaPaula = await CriarClienteAsync(context, loja.Id, "Ana Paula", "44999990000");
            ClienteModel clienteAnaClara = await CriarClienteAsync(context, loja.Id, "Ana Clara", "44999990001");
            ClienteModel clienteBeatriz = await CriarClienteAsync(context, loja.Id, "Beatriz", "44999990002");
            ProdutoEstoqueModel produtoA = await CriarProdutoAsync(context, loja.Id, "Produto A", "44999990003");
            ProdutoEstoqueModel produtoB = await CriarProdutoAsync(context, loja.Id, "Produto B", "44999990004");
            ProdutoEstoqueModel produtoC = await CriarProdutoAsync(context, loja.Id, "Produto C", "44999990005");

            _ = await CriarMovimentacaoAsync(context, loja.Id, clienteAnaPaula.Id, TipoMovimentacao.Venda, new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc), produtoA.Id);
            MovimentacaoModel esperado = await CriarMovimentacaoAsync(context, loja.Id, clienteAnaClara.Id, TipoMovimentacao.Venda, new DateTime(2026, 4, 2, 12, 0, 0, DateTimeKind.Utc), produtoB.Id);
            _ = await CriarMovimentacaoAsync(context, loja.Id, clienteBeatriz.Id, TipoMovimentacao.Emprestimo, new DateTime(2026, 4, 3, 12, 0, 0, DateTimeKind.Utc), produtoC.Id);

            MovimentacaoService service = new(context);
            PaginacaoDto<MovimentacaoBuscaDto> resultado = await service.GetAllAsync(
                new ObterMovimentacoesQuery
                {
                    LojaId = loja.Id,
                    DataInicial = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc),
                    DataFinal = new DateTime(2026, 4, 3, 0, 0, 0, DateTimeKind.Utc),
                    Cliente = "Ana",
                    Tipo = TipoMovimentacao.Venda,
                    OrdenarPor = "cliente",
                    Direcao = "desc",
                    Pagina = 2,
                    TamanhoPagina = 1
                },
                new ObterMovimentacoesParametros { UsuarioId = usuario.Id });

            Assert.Equal(2, resultado.TotalItens);
            Assert.Equal(2, resultado.TotalPaginas);
            Assert.Equal(2, resultado.Pagina);
            Assert.Equal(1, resultado.TamanhoPagina);
            MovimentacaoBuscaDto item = Assert.Single(resultado.Itens);
            Assert.Equal(esperado.Id, item.Id);
            Assert.Equal("Ana Clara", item.Cliente);
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

        private static async Task<ProdutoEstoqueModel> CriarProdutoAsync(RenovaDbContext context, int lojaId, string descricao, string contatoFornecedor)
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
