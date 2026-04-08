using Microsoft.EntityFrameworkCore;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Parameters.Produto;
using Renova.Service.Queries.Produto;
using Renova.Service.Services.Produto;
using Renova.Tests.Infrastructure;

namespace Renova.Tests.Services.Produto.GetAuxiliar
{
    public class Unitario : InMemoryDbContextTestBase<RenovaDbContext>
    {
        protected override RenovaDbContext CriarContexto(DbContextOptions<RenovaDbContext> options)
        {
            return new RenovaDbContext(options);
        }

        [Fact]
        public async Task GetMarcaAsyncDeveRetornarApenasAuxiliaresDaLojaInformada()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = await CriarUsuarioAsync(context, "maria@renova.com");
            UsuarioModel outroUsuario = await CriarUsuarioAsync(context, "joao@renova.com");
            LojaModel loja = await CriarLojaAsync(context, usuario.Id, "Loja Centro");
            LojaModel outraLoja = await CriarLojaAsync(context, usuario.Id, "Loja Bairro");
            LojaModel lojaExterna = await CriarLojaAsync(context, outroUsuario.Id, "Loja Externa");

            _ = await CriarMarcaAsync(context, loja.Id, "Farm");
            _ = await CriarMarcaAsync(context, loja.Id, "Animale");
            _ = await CriarMarcaAsync(context, outraLoja.Id, "Shoulder");
            _ = await CriarMarcaAsync(context, lojaExterna.Id, "Forum");

            ProdutoService service = new(context);
            PaginacaoDto<ProdutoAuxiliarDto> resultado = await service.GetMarcaAsync(
                new ObterProdutoAuxiliarQuery { LojaId = loja.Id },
                new ObterProdutoAuxiliarParametros { UsuarioId = usuario.Id });

            Assert.Equal(2, resultado.TotalItens);
            Assert.Collection(resultado.Itens,
                item => Assert.Equal("Animale", item.Valor),
                item => Assert.Equal("Farm", item.Valor));
        }

        [Fact]
        public async Task GetProdutoAuxiliarAsyncDeveAplicarFiltroOrdenacaoEPaginacao()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = await CriarUsuarioAsync(context, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(context, usuario.Id, "Loja Centro");

            _ = await CriarProdutoReferenciaAsync(context, loja.Id, "Vestido curto");
            _ = await CriarProdutoReferenciaAsync(context, loja.Id, "Vestido midi");
            _ = await CriarProdutoReferenciaAsync(context, loja.Id, "Blazer");

            ProdutoService service = new(context);
            PaginacaoDto<ProdutoAuxiliarDto> resultado = await service.GetProdutoAuxiliarAsync(
                new ObterProdutoAuxiliarQuery
                {
                    LojaId = loja.Id,
                    Valor = "vestido",
                    OrdenarPor = "valor",
                    Direcao = "desc",
                    Pagina = 2,
                    TamanhoPagina = 1
                },
                new ObterProdutoAuxiliarParametros { UsuarioId = usuario.Id });

            Assert.Equal(2, resultado.TotalItens);
            Assert.Equal(2, resultado.TotalPaginas);
            ProdutoAuxiliarDto item = Assert.Single(resultado.Itens);
            Assert.Equal("Vestido curto", item.Valor);
        }

        [Fact]
        public async Task GetCorAsyncDeveFalharQuandoLojaIdNaoForInformado()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = await CriarUsuarioAsync(context, "maria@renova.com");
            ProdutoService service = new(context);

            _ = await Assert.ThrowsAsync<ArgumentException>(() => service.GetCorAsync(
                new ObterProdutoAuxiliarQuery(),
                new ObterProdutoAuxiliarParametros { UsuarioId = usuario.Id }));
        }

        [Fact]
        public async Task GetTamanhoAsyncDeveFalharQuandoUsuarioAutenticadoNaoExistir()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = await CriarUsuarioAsync(context, "maria@renova.com");
            LojaModel loja = await CriarLojaAsync(context, usuario.Id, "Loja Centro");
            _ = await CriarTamanhoAsync(context, loja.Id, "M");

            ProdutoService service = new(context);

            _ = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.GetTamanhoAsync(
                new ObterProdutoAuxiliarQuery { LojaId = loja.Id },
                new ObterProdutoAuxiliarParametros { UsuarioId = 999 }));
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
    }
}
