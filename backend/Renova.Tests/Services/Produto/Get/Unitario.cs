using Microsoft.EntityFrameworkCore;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Parameters.Produto;
using Renova.Service.Queries.Produto;
using Renova.Service.Services.Produto;
using Renova.Tests.Infrastructure;

namespace Renova.Tests.Services.Produto.Get
{
    public class Unitario : InMemoryDbContextTestBase<RenovaDbContext>
    {
        protected override RenovaDbContext CriarContexto(DbContextOptions<RenovaDbContext> options)
        {
            return new RenovaDbContext(options);
        }

        [Fact]
        public async Task GetAllAsyncDeveRetornarApenasProdutosDaLojaInformada()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = await CriarUsuarioAsync(context, "maria@renova.com");
            UsuarioModel outroUsuario = await CriarUsuarioAsync(context, "joao@renova.com");
            LojaModel loja = await CriarLojaAsync(context, usuario.Id, "Loja Centro");
            LojaModel outraLoja = await CriarLojaAsync(context, usuario.Id, "Loja Bairro");
            LojaModel lojaExterna = await CriarLojaAsync(context, outroUsuario.Id, "Loja Externa");

            ProdutoEstoqueModel vestidoAzul = await CriarProdutoCompletoAsync(context, loja.Id, "Vestido", "Farm", "M", "Azul", "Fornecedor Alpha", "Vestido azul");
            ProdutoEstoqueModel blazerPreto = await CriarProdutoCompletoAsync(context, loja.Id, "Blazer", "Animale", "G", "Preto", "Fornecedor Beta", "Blazer preto");
            _ = await CriarProdutoCompletoAsync(context, outraLoja.Id, "Saia", "Shoulder", "P", "Verde", "Fornecedor Loja Bairro", "Saia verde");
            _ = await CriarProdutoCompletoAsync(context, lojaExterna.Id, "Calca", "Forum", "38", "Off White", "Fornecedor Externo", "Calca externa");

            ProdutoService service = new(context);
            PaginacaoDto<ProdutoBuscaDto> resultado = await service.GetAllAsync(
                new ObterProdutosQuery { LojaId = loja.Id },
                new ObterProdutosParametros { UsuarioId = usuario.Id });

            Assert.Equal(2, resultado.TotalItens);
            Assert.Equal(1, resultado.Pagina);
            Assert.Equal(10, resultado.TamanhoPagina);
            Assert.Equal(1, resultado.TotalPaginas);
            Assert.Collection(resultado.Itens,
                produto =>
                {
                    Assert.Equal(blazerPreto.Id, produto.Id);
                    Assert.Equal("Blazer", produto.Produto);
                    Assert.Equal("Animale", produto.Marca);
                    Assert.Equal("G", produto.Tamanho);
                    Assert.Equal("Preto", produto.Cor);
                    Assert.Equal("Fornecedor Beta", produto.Fornecedor);
                },
                produto =>
                {
                    Assert.Equal(vestidoAzul.Id, produto.Id);
                    Assert.Equal("Vestido", produto.Produto);
                    Assert.Equal("Farm", produto.Marca);
                    Assert.Equal("M", produto.Tamanho);
                    Assert.Equal("Azul", produto.Cor);
                    Assert.Equal("Fornecedor Alpha", produto.Fornecedor);
                });
        }

        [Fact]
        public async Task GetAllAsyncDeveAplicarPaginacaoQuandoPaginaETamanhoForemInformados()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = await CriarUsuarioAsync(context, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(context, usuario.Id, "Loja Centro");

            _ = await CriarProdutoCompletoAsync(context, loja.Id, "Vestido", "Farm", "M", "Azul", "Fornecedor Alpha", "Ana");
            _ = await CriarProdutoCompletoAsync(context, loja.Id, "Blazer", "Animale", "G", "Preto", "Fornecedor Beta", "Bruno");
            _ = await CriarProdutoCompletoAsync(context, loja.Id, "Saia", "Shoulder", "P", "Verde", "Fornecedor Gamma", "Carla");

            ProdutoService service = new(context);
            PaginacaoDto<ProdutoBuscaDto> resultado = await service.GetAllAsync(
                new ObterProdutosQuery
                {
                    LojaId = loja.Id,
                    Pagina = 2,
                    TamanhoPagina = 1
                },
                new ObterProdutosParametros { UsuarioId = usuario.Id });

            Assert.Equal(3, resultado.TotalItens);
            Assert.Equal(3, resultado.TotalPaginas);
            Assert.Equal(2, resultado.Pagina);
            Assert.Equal(1, resultado.TamanhoPagina);
            ProdutoBuscaDto item = Assert.Single(resultado.Itens);
            Assert.Equal("Bruno", item.Descricao);
        }

        [Fact]
        public async Task GetAllAsyncDeveOrdenarPorDescricaoQuandoOrdenacaoForInformada()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = await CriarUsuarioAsync(context, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(context, usuario.Id, "Loja Centro");

            _ = await CriarProdutoCompletoAsync(context, loja.Id, "Vestido", "Farm", "M", "Azul", "Fornecedor Alpha", "Carlos");
            _ = await CriarProdutoCompletoAsync(context, loja.Id, "Blazer", "Animale", "G", "Preto", "Fornecedor Beta", "Ana");
            _ = await CriarProdutoCompletoAsync(context, loja.Id, "Saia", "Shoulder", "P", "Verde", "Fornecedor Gamma", "Bruno");

            ProdutoService service = new(context);
            PaginacaoDto<ProdutoBuscaDto> resultado = await service.GetAllAsync(
                new ObterProdutosQuery
                {
                    LojaId = loja.Id,
                    OrdenarPor = "descricao",
                    Direcao = "desc"
                },
                new ObterProdutosParametros { UsuarioId = usuario.Id });

            Assert.Collection(resultado.Itens,
                produto => Assert.Equal("Carlos", produto.Descricao),
                produto => Assert.Equal("Bruno", produto.Descricao),
                produto => Assert.Equal("Ana", produto.Descricao));
        }

        [Fact]
        public async Task GetAllAsyncDeveFiltrarPorDescricaoQuandoFiltroForInformado()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = await CriarUsuarioAsync(context, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(context, usuario.Id, "Loja Centro");

            _ = await CriarProdutoCompletoAsync(context, loja.Id, "Vestido", "Farm", "M", "Azul", "Fornecedor Alpha", "Vestido azul midi");
            _ = await CriarProdutoCompletoAsync(context, loja.Id, "Blazer", "Animale", "G", "Preto", "Fornecedor Beta", "Blazer azul marinho");
            _ = await CriarProdutoCompletoAsync(context, loja.Id, "Saia", "Shoulder", "P", "Verde", "Fornecedor Gamma", "Saia verde");

            ProdutoService service = new(context);
            PaginacaoDto<ProdutoBuscaDto> resultado = await service.GetAllAsync(
                new ObterProdutosQuery
                {
                    LojaId = loja.Id,
                    Descricao = "azul"
                },
                new ObterProdutosParametros { UsuarioId = usuario.Id });

            Assert.Equal(2, resultado.TotalItens);
            Assert.Collection(resultado.Itens,
                produto => Assert.Equal("Blazer azul marinho", produto.Descricao),
                produto => Assert.Equal("Vestido azul midi", produto.Descricao));
        }

        [Fact]
        public async Task GetAllAsyncDeveFiltrarPorValorDasTabelasAuxiliaresQuandoFiltroForInformado()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = await CriarUsuarioAsync(context, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(context, usuario.Id, "Loja Centro");

            _ = await CriarProdutoCompletoAsync(context, loja.Id, "Vestido", "Farm", "M", "Azul", "Fornecedor Alpha", "Item 1");
            _ = await CriarProdutoCompletoAsync(context, loja.Id, "Blazer", "Animale", "G", "Preto", "Fornecedor Beta", "Item 2");
            _ = await CriarProdutoCompletoAsync(context, loja.Id, "Vestido Longo", "Shoulder", "P", "Vermelho", "Fornecedor Gamma", "Item 3");

            ProdutoService service = new(context);

            PaginacaoDto<ProdutoBuscaDto> filtradoPorProduto = await service.GetAllAsync(
                new ObterProdutosQuery { LojaId = loja.Id, Produto = "vestido" },
                new ObterProdutosParametros { UsuarioId = usuario.Id });

            Assert.Equal(2, filtradoPorProduto.TotalItens);

            PaginacaoDto<ProdutoBuscaDto> filtradoPorMarca = await service.GetAllAsync(
                new ObterProdutosQuery { LojaId = loja.Id, Marca = "animal" },
                new ObterProdutosParametros { UsuarioId = usuario.Id });

            Assert.Single(filtradoPorMarca.Itens);
            Assert.Equal("Animale", filtradoPorMarca.Itens[0].Marca);

            PaginacaoDto<ProdutoBuscaDto> filtradoPorTamanho = await service.GetAllAsync(
                new ObterProdutosQuery { LojaId = loja.Id, Tamanho = "m" },
                new ObterProdutosParametros { UsuarioId = usuario.Id });

            Assert.Single(filtradoPorTamanho.Itens);
            Assert.Equal("M", filtradoPorTamanho.Itens[0].Tamanho);

            PaginacaoDto<ProdutoBuscaDto> filtradoPorCor = await service.GetAllAsync(
                new ObterProdutosQuery { LojaId = loja.Id, Cor = "verm" },
                new ObterProdutosParametros { UsuarioId = usuario.Id });

            Assert.Single(filtradoPorCor.Itens);
            Assert.Equal("Vermelho", filtradoPorCor.Itens[0].Cor);
        }

        [Fact]
        public async Task GetAllAsyncDeveFiltrarPorNomeDoFornecedorQuandoFiltroForInformado()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = await CriarUsuarioAsync(context, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(context, usuario.Id, "Loja Centro");

            _ = await CriarProdutoCompletoAsync(context, loja.Id, "Vestido", "Farm", "M", "Azul", "Fornecedor Alpha", "Item 1");
            _ = await CriarProdutoCompletoAsync(context, loja.Id, "Blazer", "Animale", "G", "Preto", "Distribuidora Beta", "Item 2");

            ProdutoService service = new(context);
            PaginacaoDto<ProdutoBuscaDto> resultado = await service.GetAllAsync(
                new ObterProdutosQuery
                {
                    LojaId = loja.Id,
                    Fornecedor = "beta"
                },
                new ObterProdutosParametros { UsuarioId = usuario.Id });

            ProdutoBuscaDto item = Assert.Single(resultado.Itens);
            Assert.Equal("Distribuidora Beta", item.Fornecedor);
        }

        [Fact]
        public async Task GetAllAsyncDeveFiltrarPorPrecoQuandoIntervaloForInformado()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = await CriarUsuarioAsync(context, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(context, usuario.Id, "Loja Centro");

            _ = await CriarProdutoCompletoAsync(context, loja.Id, "Vestido", "Farm", "M", "Azul", "Fornecedor Alpha", "Item 1", 100m);
            _ = await CriarProdutoCompletoAsync(context, loja.Id, "Blazer", "Animale", "G", "Preto", "Fornecedor Beta", "Item 2", 200m);
            _ = await CriarProdutoCompletoAsync(context, loja.Id, "Saia", "Shoulder", "P", "Verde", "Fornecedor Gamma", "Item 3", 300m);

            ProdutoService service = new(context);

            PaginacaoDto<ProdutoBuscaDto> maiorOuIgual = await service.GetAllAsync(
                new ObterProdutosQuery { LojaId = loja.Id, PrecoInicial = 200m },
                new ObterProdutosParametros { UsuarioId = usuario.Id });

            Assert.Equal(2, maiorOuIgual.TotalItens);

            PaginacaoDto<ProdutoBuscaDto> menorOuIgual = await service.GetAllAsync(
                new ObterProdutosQuery { LojaId = loja.Id, PrecoFinal = 200m },
                new ObterProdutosParametros { UsuarioId = usuario.Id });

            Assert.Equal(2, menorOuIgual.TotalItens);

            PaginacaoDto<ProdutoBuscaDto> intervalo = await service.GetAllAsync(
                new ObterProdutosQuery { LojaId = loja.Id, PrecoInicial = 150m, PrecoFinal = 250m },
                new ObterProdutosParametros { UsuarioId = usuario.Id });

            ProdutoBuscaDto item = Assert.Single(intervalo.Itens);
            Assert.Equal(200m, item.Preco);
        }

        [Fact]
        public async Task GetAllAsyncDeveFiltrarPorDataQuandoIntervaloForInformado()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = await CriarUsuarioAsync(context, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(context, usuario.Id, "Loja Centro");

            _ = await CriarProdutoCompletoAsync(context, loja.Id, "Vestido", "Farm", "M", "Azul", "Fornecedor Alpha", "Item 1", 100m, new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc));
            _ = await CriarProdutoCompletoAsync(context, loja.Id, "Blazer", "Animale", "G", "Preto", "Fornecedor Beta", "Item 2", 200m, new DateTime(2026, 4, 10, 0, 0, 0, DateTimeKind.Utc));
            _ = await CriarProdutoCompletoAsync(context, loja.Id, "Saia", "Shoulder", "P", "Verde", "Fornecedor Gamma", "Item 3", 300m, new DateTime(2026, 4, 20, 0, 0, 0, DateTimeKind.Utc));

            ProdutoService service = new(context);

            PaginacaoDto<ProdutoBuscaDto> maiorOuIgual = await service.GetAllAsync(
                new ObterProdutosQuery { LojaId = loja.Id, DataInicial = new DateTime(2026, 4, 10, 0, 0, 0, DateTimeKind.Utc) },
                new ObterProdutosParametros { UsuarioId = usuario.Id });

            Assert.Equal(2, maiorOuIgual.TotalItens);

            PaginacaoDto<ProdutoBuscaDto> menorOuIgual = await service.GetAllAsync(
                new ObterProdutosQuery { LojaId = loja.Id, DataFinal = new DateTime(2026, 4, 10, 0, 0, 0, DateTimeKind.Utc) },
                new ObterProdutosParametros { UsuarioId = usuario.Id });

            Assert.Equal(2, menorOuIgual.TotalItens);

            PaginacaoDto<ProdutoBuscaDto> intervalo = await service.GetAllAsync(
                new ObterProdutosQuery
                {
                    LojaId = loja.Id,
                    DataInicial = new DateTime(2026, 4, 5, 0, 0, 0, DateTimeKind.Utc),
                    DataFinal = new DateTime(2026, 4, 15, 0, 0, 0, DateTimeKind.Utc)
                },
                new ObterProdutosParametros { UsuarioId = usuario.Id });

            ProdutoBuscaDto item = Assert.Single(intervalo.Itens);
            Assert.Equal(new DateTime(2026, 4, 10, 0, 0, 0, DateTimeKind.Utc), item.Entrada);
        }

        [Fact]
        public async Task GetAllAsyncDeveFiltrarPorSituacaoQuandoFiltroForInformado()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = await CriarUsuarioAsync(context, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(context, usuario.Id, "Loja Centro");

            ProdutoEstoqueModel estoque = await CriarProdutoCompletoAsync(context, loja.Id, "Vestido", "Farm", "M", "Azul", "Fornecedor Alpha", "Item estoque");
            ProdutoEstoqueModel emprestado = await CriarProdutoCompletoAsync(context, loja.Id, "Blazer", "Animale", "G", "Preto", "Fornecedor Beta", "Item emprestado");
            ProdutoEstoqueModel vendido = await CriarProdutoCompletoAsync(context, loja.Id, "Saia", "Shoulder", "P", "Verde", "Fornecedor Gamma", "Item vendido");

            estoque.Situacao = SituacaoProduto.Estoque;
            emprestado.Situacao = SituacaoProduto.Emprestado;
            vendido.Situacao = SituacaoProduto.Vendido;
            _ = await context.SaveChangesAsync();

            ProdutoService service = new(context);
            PaginacaoDto<ProdutoBuscaDto> resultado = await service.GetAllAsync(
                new ObterProdutosQuery
                {
                    LojaId = loja.Id,
                    Situacao = SituacaoProduto.Emprestado
                },
                new ObterProdutosParametros { UsuarioId = usuario.Id });

            ProdutoBuscaDto item = Assert.Single(resultado.Itens);
            Assert.Equal(emprestado.Id, item.Id);
            Assert.Equal(SituacaoProduto.Emprestado, item.Situacao);
        }

        [Fact]
        public async Task GetAllAsyncDeveOrdenarPorCamposRelacionadosQuandoOrdenacaoForInformada()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = await CriarUsuarioAsync(context, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(context, usuario.Id, "Loja Centro");

            _ = await CriarProdutoCompletoAsync(context, loja.Id, "Vestido", "Zeta", "M", "Azul", "Fornecedor Carlos", "Item 1");
            _ = await CriarProdutoCompletoAsync(context, loja.Id, "Blazer", "Alpha", "G", "Preto", "Fornecedor Ana", "Item 2");
            _ = await CriarProdutoCompletoAsync(context, loja.Id, "Saia", "Beta", "P", "Verde", "Fornecedor Bruno", "Item 3");

            ProdutoService service = new(context);

            PaginacaoDto<ProdutoBuscaDto> ordenadoPorMarca = await service.GetAllAsync(
                new ObterProdutosQuery { LojaId = loja.Id, OrdenarPor = "marca", Direcao = "asc" },
                new ObterProdutosParametros { UsuarioId = usuario.Id });

            Assert.Collection(ordenadoPorMarca.Itens,
                produto => Assert.Equal("Alpha", produto.Marca),
                produto => Assert.Equal("Beta", produto.Marca),
                produto => Assert.Equal("Zeta", produto.Marca));

            PaginacaoDto<ProdutoBuscaDto> ordenadoPorFornecedor = await service.GetAllAsync(
                new ObterProdutosQuery { LojaId = loja.Id, OrdenarPor = "fornecedor", Direcao = "desc" },
                new ObterProdutosParametros { UsuarioId = usuario.Id });

            Assert.Collection(ordenadoPorFornecedor.Itens,
                produto => Assert.Equal("Fornecedor Carlos", produto.Fornecedor),
                produto => Assert.Equal("Fornecedor Bruno", produto.Fornecedor),
                produto => Assert.Equal("Fornecedor Ana", produto.Fornecedor));
        }

        [Fact]
        public async Task GetAllAsyncDeveCombinarFiltrosOrdenacaoEPaginacao()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = await CriarUsuarioAsync(context, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(context, usuario.Id, "Loja Centro");

            _ = await CriarProdutoCompletoAsync(context, loja.Id, "Vestido", "Farm", "M", "Azul", "Fornecedor Zeta", "Vestido azul premium");
            _ = await CriarProdutoCompletoAsync(context, loja.Id, "Vestido Midi", "Farm", "G", "Azul", "Fornecedor Alpha", "Vestido azul casual");
            _ = await CriarProdutoCompletoAsync(context, loja.Id, "Blazer", "Animale", "P", "Preto", "Fornecedor Beta", "Blazer preto");

            ProdutoService service = new(context);
            PaginacaoDto<ProdutoBuscaDto> resultado = await service.GetAllAsync(
                new ObterProdutosQuery
                {
                    LojaId = loja.Id,
                    Produto = "Vestido",
                    Cor = "Azul",
                    OrdenarPor = "fornecedor",
                    Direcao = "desc",
                    Pagina = 2,
                    TamanhoPagina = 1
                },
                new ObterProdutosParametros { UsuarioId = usuario.Id });

            Assert.Equal(2, resultado.TotalItens);
            Assert.Equal(2, resultado.TotalPaginas);
            ProdutoBuscaDto item = Assert.Single(resultado.Itens);
            Assert.Equal("Fornecedor Alpha", item.Fornecedor);
        }

        [Fact]
        public async Task GetAllAsyncDeveFalharQuandoLojaIdNaoForInformado()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = await CriarUsuarioAsync(context, "maria@renova.com");
            ProdutoService service = new(context);

            _ = await Assert.ThrowsAsync<ArgumentException>(() => service.GetAllAsync(
                new ObterProdutosQuery(),
                new ObterProdutosParametros { UsuarioId = usuario.Id }));
        }

        [Fact]
        public async Task GetAllAsyncDeveFalharQuandoUsuarioAutenticadoNaoExistir()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = await CriarUsuarioAsync(context, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(context, usuario.Id, "Loja Centro");
            _ = await CriarProdutoCompletoAsync(context, loja.Id, "Vestido", "Farm", "M", "Azul", "Fornecedor Alpha", "Vestido azul");

            ProdutoService service = new(context);

            _ = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.GetAllAsync(
                new ObterProdutosQuery { LojaId = loja.Id },
                new ObterProdutosParametros { UsuarioId = 999 }));
        }

        [Fact]
        public async Task GetAllAsyncDeveRetornarListaVaziaQuandoLojaNaoPossuirProdutos()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = await CriarUsuarioAsync(context, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(context, usuario.Id, "Loja Vazia");
            LojaModel outraLoja = await CriarLojaAsync(context, usuario.Id, "Loja Com Produtos");

            _ = await CriarProdutoCompletoAsync(context, outraLoja.Id, "Vestido", "Farm", "M", "Azul", "Fornecedor Alpha", "Vestido azul");

            ProdutoService service = new(context);
            PaginacaoDto<ProdutoBuscaDto> resultado = await service.GetAllAsync(
                new ObterProdutosQuery { LojaId = loja.Id },
                new ObterProdutosParametros { UsuarioId = usuario.Id });

            Assert.Empty(resultado.Itens);
            Assert.Equal(0, resultado.TotalItens);
            Assert.Equal(0, resultado.TotalPaginas);
        }

        [Fact]
        public async Task GetEmprestadosDoClienteAsyncDeveRetornarApenasProdutosEmprestadosDoClienteInformado()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = await CriarUsuarioAsync(context, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(context, usuario.Id, "Loja Centro");
            ClienteModel elaine = await CriarClienteAsync(context, loja.Id, "Elaine", "44999990001");
            ClienteModel gustavo = await CriarClienteAsync(context, loja.Id, "Gustavo", "44999990002");
            ProdutoEstoqueModel produtoElaine = await CriarProdutoCompletoAsync(context, loja.Id, "Vestido", "Farm", "M", "Azul", "Fornecedor Alpha", "Vestido Elaine");
            ProdutoEstoqueModel produtoGustavo = await CriarProdutoCompletoAsync(context, loja.Id, "Blazer", "Animale", "G", "Preto", "Fornecedor Beta", "Blazer Gustavo");

            produtoElaine.Situacao = SituacaoProduto.Emprestado;
            produtoGustavo.Situacao = SituacaoProduto.Emprestado;
            _ = await context.SaveChangesAsync();

            _ = await CriarMovimentacaoAsync(context, loja.Id, elaine.Id, TipoMovimentacao.Emprestimo, produtoElaine.Id);
            _ = await CriarMovimentacaoAsync(context, loja.Id, gustavo.Id, TipoMovimentacao.Emprestimo, produtoGustavo.Id);

            ProdutoService service = new(context);
            IReadOnlyList<ProdutoBuscaDto> resultado = await service.GetEmprestadosDoClienteAsync(
                new ObterProdutosEmprestadosClienteParametros
                {
                    UsuarioId = usuario.Id,
                    LojaId = loja.Id,
                    ClienteId = elaine.Id
                });

            ProdutoBuscaDto item = Assert.Single(resultado);
            Assert.Equal(produtoElaine.Id, item.Id);
        }

        [Fact]
        public async Task GetEmprestadosDoClienteAsyncDeveRetornarListaVaziaQuandoClienteNaoPossuirEmprestados()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = await CriarUsuarioAsync(context, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(context, usuario.Id, "Loja Centro");
            ClienteModel elaine = await CriarClienteAsync(context, loja.Id, "Elaine", "44999990001");

            ProdutoService service = new(context);
            IReadOnlyList<ProdutoBuscaDto> resultado = await service.GetEmprestadosDoClienteAsync(
                new ObterProdutosEmprestadosClienteParametros
                {
                    UsuarioId = usuario.Id,
                    LojaId = loja.Id,
                    ClienteId = elaine.Id
                });

            Assert.Empty(resultado);
        }

        [Fact]
        public async Task GetEmprestadosDoClienteAsyncDeveFalharQuandoClienteNaoPertencerALoja()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = await CriarUsuarioAsync(context, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(context, usuario.Id, "Loja Centro");
            LojaModel outraLoja = await CriarLojaAsync(context, usuario.Id, "Loja Bairro");
            ClienteModel clienteOutraLoja = await CriarClienteAsync(context, outraLoja.Id, "Elaine", "44999990001");

            ProdutoService service = new(context);

            _ = await Assert.ThrowsAsync<ArgumentException>(() => service.GetEmprestadosDoClienteAsync(
                new ObterProdutosEmprestadosClienteParametros
                {
                    UsuarioId = usuario.Id,
                    LojaId = loja.Id,
                    ClienteId = clienteOutraLoja.Id
                }));
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

        private static async Task<ProdutoEstoqueModel> CriarProdutoCompletoAsync(
            RenovaDbContext context,
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
            ProdutoReferenciaModel produtoReferencia = await CriarProdutoReferenciaAsync(context, lojaId, produto);
            MarcaModel marcaModel = await CriarMarcaAsync(context, lojaId, marca);
            TamanhoModel tamanhoModel = await CriarTamanhoAsync(context, lojaId, tamanho);
            CorModel corModel = await CriarCorAsync(context, lojaId, cor);
            ClienteModel fornecedorModel = await CriarClienteAsync(context, lojaId, fornecedor, $"{Guid.NewGuid():N}"[..11]);

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

        private static async Task<ProdutoReferenciaModel> CriarProdutoReferenciaAsync(RenovaDbContext context, int lojaId, string valor)
        {
            ProdutoReferenciaModel entity = new()
            {
                Valor = valor,
                LojaId = lojaId
            };

            _ = context.ProdutosReferencia.Add(entity);
            _ = await context.SaveChangesAsync();
            return entity;
        }

        private static async Task<MarcaModel> CriarMarcaAsync(RenovaDbContext context, int lojaId, string valor)
        {
            MarcaModel entity = new()
            {
                Valor = valor,
                LojaId = lojaId
            };

            _ = context.Marcas.Add(entity);
            _ = await context.SaveChangesAsync();
            return entity;
        }

        private static async Task<TamanhoModel> CriarTamanhoAsync(RenovaDbContext context, int lojaId, string valor)
        {
            TamanhoModel entity = new()
            {
                Valor = valor,
                LojaId = lojaId
            };

            _ = context.Tamanhos.Add(entity);
            _ = await context.SaveChangesAsync();
            return entity;
        }

        private static async Task<CorModel> CriarCorAsync(RenovaDbContext context, int lojaId, string valor)
        {
            CorModel entity = new()
            {
                Valor = valor,
                LojaId = lojaId
            };

            _ = context.Cores.Add(entity);
            _ = await context.SaveChangesAsync();
            return entity;
        }

        private static async Task<MovimentacaoModel> CriarMovimentacaoAsync(
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
    }
}
