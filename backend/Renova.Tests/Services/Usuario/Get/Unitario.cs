using Microsoft.EntityFrameworkCore;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Queries.Usuario;
using Renova.Service.Services.Usuario;
using Renova.Tests.Infrastructure;

namespace Renova.Tests.Services.Usuario.Get
{
    public class Unitario : InMemoryDbContextTestBase<RenovaDbContext>
    {
        protected override RenovaDbContext CriarContexto(DbContextOptions<RenovaDbContext> options)
        {
            return new RenovaDbContext(options);
        }

        [Fact]
        public async Task GetAllAsyncDeveFiltrarPorNomeOuEmailQuandoBuscaForInformada()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel autenticado = await CriarUsuarioAsync(context, "Usuario Autenticado", "auth@renova.com");
            await CriarUsuarioAsync(context, "Ana Paula", "ana@renova.com");
            await CriarUsuarioAsync(context, "Bruno Costa", "bruno@renova.com");
            await CriarUsuarioAsync(context, "Carlos Lima", "contato.carlos@dominio.com");

            UsuarioService service = new(context);

            PaginacaoDto<UsuarioDto> porNome = await service.GetAllAsync(
                new ObterUsuariosQuery { Busca = "ana" },
                autenticado.Id);

            UsuarioDto usuarioPorNome = Assert.Single(porNome.Itens);
            Assert.Equal(1, porNome.TotalItens);
            Assert.Equal("Ana Paula", usuarioPorNome.Nome);

            PaginacaoDto<UsuarioDto> porEmail = await service.GetAllAsync(
                new ObterUsuariosQuery { Busca = "dominio.com" },
                autenticado.Id);

            UsuarioDto item = Assert.Single(porEmail.Itens);
            Assert.Equal("Carlos Lima", item.Nome);
        }

        [Fact]
        public async Task GetAllAsyncDeveAplicarPaginacaoEOrdenacaoQuandoInformadas()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel autenticado = await CriarUsuarioAsync(context, "Usuario Autenticado", "auth@renova.com");
            await CriarUsuarioAsync(context, "Carlos", "carlos@renova.com");
            await CriarUsuarioAsync(context, "Ana", "ana@renova.com");
            await CriarUsuarioAsync(context, "Bruno", "bruno@renova.com");

            UsuarioService service = new(context);

            PaginacaoDto<UsuarioDto> resultado = await service.GetAllAsync(
                new ObterUsuariosQuery
                {
                    OrdenarPor = "nome",
                    Direcao = "asc",
                    Pagina = 2,
                    TamanhoPagina = 2
                },
                autenticado.Id);

            Assert.Equal(4, resultado.TotalItens);
            Assert.Equal(2, resultado.TotalPaginas);
            Assert.Equal(2, resultado.Pagina);
            Assert.Equal(2, resultado.TamanhoPagina);
            Assert.Collection(resultado.Itens,
                usuario => Assert.Equal("Carlos", usuario.Nome),
                usuario => Assert.Equal("Usuario Autenticado", usuario.Nome));
        }

        [Fact]
        public async Task GetAllAsyncDeveFalharQuandoUsuarioAutenticadoNaoExistir()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            await CriarUsuarioAsync(context, "Ana Paula", "ana@renova.com");
            UsuarioService service = new(context);

            _ = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.GetAllAsync(
                new ObterUsuariosQuery(),
                999));
        }

        private static async Task<UsuarioModel> CriarUsuarioAsync(RenovaDbContext context, string nome, string email)
        {
            UsuarioModel usuario = new()
            {
                Nome = nome,
                Email = email,
                SenhaHash = "hash"
            };

            _ = context.Usuarios.Add(usuario);
            _ = await context.SaveChangesAsync();

            return usuario;
        }
    }
}
