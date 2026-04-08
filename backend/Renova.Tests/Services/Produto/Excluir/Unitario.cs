using Microsoft.EntityFrameworkCore;

using Renova.Domain.Model;
using Renova.Persistence;
using Renova.Service.Parameters.Produto;
using Renova.Service.Services.Produto;
using Renova.Tests.Infrastructure;

namespace Renova.Tests.Services.Produto.Excluir
{
    public class Unitario : InMemoryDbContextTestBase<RenovaDbContext>
    {
        protected override RenovaDbContext CriarContexto(DbContextOptions<RenovaDbContext> options)
        {
            return new RenovaDbContext(options);
        }

        [Fact]
        public async Task DeleteAsyncDeveExcluirProdutoDaLojaDoUsuarioAutenticado()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarCenarioBaseAsync(context, "maria@renova.com");
            ProdutoEstoqueModel produto = await CriarProdutoAsync(context, loja.Id);

            ProdutoService service = new(context);
            await service.DeleteAsync(new ExcluirProdutoParametros
            {
                UsuarioId = loja.UsuarioId,
                ProdutoId = produto.Id
            });

            Assert.Empty(context.ProdutosEstoque);
        }

        [Fact]
        public async Task DeleteAsyncDeveImpedirExclusaoQuandoLojaNaoPertencerAoUsuarioAutenticado()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarCenarioBaseAsync(context, "maria@renova.com");
            ProdutoEstoqueModel produto = await CriarProdutoAsync(context, loja.Id);
            LojaModel outraLoja = await CriarCenarioBaseAsync(context, "joao@renova.com");

            ProdutoService service = new(context);
            _ = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.DeleteAsync(new ExcluirProdutoParametros
            {
                UsuarioId = outraLoja.UsuarioId,
                ProdutoId = produto.Id
            }));

            ProdutoEstoqueModel produtoSalvo = await context.ProdutosEstoque.SingleAsync();
            Assert.Equal(produto.Id, produtoSalvo.Id);
        }

        [Fact]
        public async Task DeleteAsyncDeveFalharQuandoProdutoNaoForEncontrado()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarCenarioBaseAsync(context, "maria@renova.com");

            ProdutoService service = new(context);
            _ = await Assert.ThrowsAsync<KeyNotFoundException>(() => service.DeleteAsync(new ExcluirProdutoParametros
            {
                UsuarioId = loja.UsuarioId,
                ProdutoId = 999
            }));
        }

        [Fact]
        public async Task DeleteAsyncDeveFalharQuandoUsuarioAutenticadoNaoForEncontrado()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarCenarioBaseAsync(context, "maria@renova.com");
            ProdutoEstoqueModel produto = await CriarProdutoAsync(context, loja.Id);

            ProdutoService service = new(context);
            _ = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.DeleteAsync(new ExcluirProdutoParametros
            {
                UsuarioId = 9999,
                ProdutoId = produto.Id
            }));
        }

        private static async Task<ProdutoEstoqueModel> CriarProdutoAsync(RenovaDbContext context, int lojaId)
        {
            ProdutoReferenciaModel produto = await context.ProdutosReferencia.SingleAsync(item => item.LojaId == lojaId);
            MarcaModel marca = await context.Marcas.SingleAsync(item => item.LojaId == lojaId);
            TamanhoModel tamanho = await context.Tamanhos.SingleAsync(item => item.LojaId == lojaId);
            CorModel cor = await context.Cores.SingleAsync(item => item.LojaId == lojaId);
            ClienteModel fornecedor = await context.Clientes.SingleAsync(item => item.LojaId == lojaId);

            ProdutoEstoqueModel entity = new()
            {
                Preco = 99.90m,
                ProdutoId = produto.Id,
                MarcaId = marca.Id,
                TamanhoId = tamanho.Id,
                CorId = cor.Id,
                FornecedorId = fornecedor.Id,
                Descricao = "Produto original",
                Entrada = new DateTime(2026, 4, 7, 12, 0, 0, DateTimeKind.Utc),
                LojaId = lojaId,
                Situacao = SituacaoProduto.Estoque,
                Consignado = true
            };

            _ = context.ProdutosEstoque.Add(entity);
            _ = await context.SaveChangesAsync();
            return entity;
        }

        private static async Task<LojaModel> CriarCenarioBaseAsync(RenovaDbContext context, string emailUsuario)
        {
            LojaModel loja = await CriarLojaAsync(context, $"Loja {emailUsuario}", emailUsuario);
            _ = await CriarProdutoReferenciaAsync(context, loja.Id, "Vestido");
            _ = await CriarMarcaAsync(context, loja.Id, "Farm");
            _ = await CriarTamanhoAsync(context, loja.Id, "M");
            _ = await CriarCorAsync(context, loja.Id, "Azul");
            _ = await CriarClienteAsync(context, loja.Id, "Fornecedor", "44999990000");
            return loja;
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

        private static async Task<LojaModel> CriarLojaAsync(RenovaDbContext context, string nomeLoja, string emailUsuario)
        {
            UsuarioModel usuario = await CriarUsuarioAsync(context, emailUsuario);
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
