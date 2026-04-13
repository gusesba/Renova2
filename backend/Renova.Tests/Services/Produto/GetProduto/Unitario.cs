using Microsoft.EntityFrameworkCore;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Parameters.Produto;
using Renova.Service.Services.Produto;
using Renova.Tests.Infrastructure;

namespace Renova.Tests.Services.Produto.GetProduto
{
    public class Unitario : InMemoryDbContextTestBase<RenovaDbContext>
    {
        protected override RenovaDbContext CriarContexto(DbContextOptions<RenovaDbContext> options)
        {
            return new RenovaDbContext(options);
        }

        [Fact]
        public async Task GetByIdAsyncDeveRetornarProdutoQuandoIdForValido()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = await CriarUsuarioAsync(context, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(context, usuario.Id, "Loja Centro");
            ProdutoEstoqueModel produto = await CriarProdutoCompletoAsync(context, loja.Id, "Vestido", "Farm", "M", "Azul", "Fornecedor Alpha", "Vestido azul");

            ProdutoService service = new(context);
            ProdutoBuscaDto resultado = await service.GetByIdAsync(new ObterProdutoParametros
            {
                UsuarioId = usuario.Id,
                ProdutoId = produto.Id
            });

            Assert.Equal(produto.Id, resultado.Id);
            Assert.Equal(produto.Preco, resultado.Preco);
            Assert.Equal("Vestido", resultado.Produto);
            Assert.Equal("Farm", resultado.Marca);
            Assert.Equal("M", resultado.Tamanho);
            Assert.Equal("Azul", resultado.Cor);
            Assert.Equal("Fornecedor Alpha", resultado.Fornecedor);
            Assert.Equal("Vestido azul", resultado.Descricao);
            Assert.Equal(loja.Id, resultado.LojaId);
            Assert.Equal(TipoMovimentacao.DevolucaoDono, resultado.TipoSugerido);
        }

        [Fact]
        public async Task GetByIdAsyncDeveRetornarTipoSugeridoComoDoacaoQuandoFornecedorPermitir()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = await CriarUsuarioAsync(context, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(context, usuario.Id, "Loja Centro");
            ProdutoEstoqueModel produto = await CriarProdutoCompletoAsync(
                context,
                loja.Id,
                "Vestido",
                "Farm",
                "M",
                "Azul",
                "Fornecedor Alpha",
                "Vestido azul",
                fornecedorDoacao: true);

            ProdutoService service = new(context);
            ProdutoBuscaDto resultado = await service.GetByIdAsync(new ObterProdutoParametros
            {
                UsuarioId = usuario.Id,
                ProdutoId = produto.Id
            });

            Assert.Equal(TipoMovimentacao.Doacao, resultado.TipoSugerido);
        }

        [Fact]
        public async Task GetByIdAsyncDeveFalharQuandoProdutoNaoExistir()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = await CriarUsuarioAsync(context, "maria@renova.com");
            ProdutoService service = new(context);

            _ = await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetByIdAsync(new ObterProdutoParametros
            {
                UsuarioId = usuario.Id,
                ProdutoId = 999
            }));
        }

        [Fact]
        public async Task GetByIdAsyncDeveFalharQuandoProdutoNaoPertencerAoUsuarioAutenticado()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = await CriarUsuarioAsync(context, "maria@renova.com");
            UsuarioModel outroUsuario = await CriarUsuarioAsync(context, "joao@renova.com");
            LojaModel loja = await CriarLojaAsync(context, usuario.Id, "Loja Centro");
            ProdutoEstoqueModel produto = await CriarProdutoCompletoAsync(context, loja.Id, "Vestido", "Farm", "M", "Azul", "Fornecedor Alpha", "Vestido azul");

            ProdutoService service = new(context);

            _ = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.GetByIdAsync(new ObterProdutoParametros
            {
                UsuarioId = outroUsuario.Id,
                ProdutoId = produto.Id
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
            bool fornecedorDoacao = false)
        {
            ProdutoReferenciaModel produtoReferencia = await CriarProdutoReferenciaAsync(context, lojaId, produto);
            MarcaModel marcaModel = await CriarMarcaAsync(context, lojaId, marca);
            TamanhoModel tamanhoModel = await CriarTamanhoAsync(context, lojaId, tamanho);
            CorModel corModel = await CriarCorAsync(context, lojaId, cor);
            ClienteModel fornecedorModel = await CriarClienteAsync(
                context,
                lojaId,
                fornecedor,
                $"{Guid.NewGuid():N}"[..11],
                fornecedorDoacao);

            ProdutoEstoqueModel entity = new()
            {
                Preco = 149.90m,
                ProdutoId = produtoReferencia.Id,
                MarcaId = marcaModel.Id,
                TamanhoId = tamanhoModel.Id,
                CorId = corModel.Id,
                FornecedorId = fornecedorModel.Id,
                Descricao = descricao,
                Entrada = new DateTime(2026, 4, 7, 12, 0, 0, DateTimeKind.Utc),
                LojaId = lojaId,
                Situacao = SituacaoProduto.Estoque,
                Consignado = false
            };

            _ = context.ProdutosEstoque.Add(entity);
            _ = await context.SaveChangesAsync();
            return entity;
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
                LojaId = lojaId,
                Doacao = doacao
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
    }
}
