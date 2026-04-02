using Microsoft.EntityFrameworkCore;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Parameters.Loja;
using Renova.Service.Services.Loja;
using Renova.Tests.Infrastructure;

namespace Renova.Tests.Services.Loja.Get
{
    public class Unitario : InMemoryDbContextTestBase<RenovaDbContext>
    {
        protected override RenovaDbContext CriarContexto(DbContextOptions<RenovaDbContext> options)
        {
            return new RenovaDbContext(options);
        }

        [Fact]
        //Input: usuario autenticado com lojas cadastradas
        //Retorna apenas as lojas vinculadas ao usuario informado
        //Retorna: lista de lojas em ordem consistente
        public async Task GetAllAsyncDeveRetornarApenasLojasDoUsuarioInformado()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = new()
            {
                Nome = "Maria da Silva",
                Email = "maria@renova.com",
                SenhaHash = "hash"
            };

            UsuarioModel outroUsuario = new()
            {
                Nome = "Joao Souza",
                Email = "joao@renova.com",
                SenhaHash = "hash"
            };

            _ = context.Usuarios.Add(usuario);
            _ = context.Usuarios.Add(outroUsuario);
            _ = await context.SaveChangesAsync();

            context.Lojas.AddRange(
                new LojaModel { Nome = "Loja Centro", UsuarioId = usuario.Id },
                new LojaModel { Nome = "Loja Bairro", UsuarioId = usuario.Id },
                new LojaModel { Nome = "Loja Externa", UsuarioId = outroUsuario.Id });
            _ = await context.SaveChangesAsync();

            LojaService service = new(context);
            IReadOnlyList<LojaDto> resultado = await service.GetAllAsync(new ObterLojasParametros
            {
                UsuarioId = usuario.Id
            });

            Assert.Equal(2, resultado.Count);
            Assert.Collection(resultado,
                loja => Assert.Equal("Loja Bairro", loja.Nome),
                loja => Assert.Equal("Loja Centro", loja.Nome));
        }

        [Fact]
        //Input: usuario autenticado sem lojas cadastradas
        //Nao retorna lojas de outros usuarios
        //Retorna: lista vazia
        public async Task GetAllAsyncDeveRetornarListaVaziaQuandoUsuarioNaoPossuirLojas()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = new()
            {
                Nome = "Maria da Silva",
                Email = "maria@renova.com",
                SenhaHash = "hash"
            };

            UsuarioModel outroUsuario = new()
            {
                Nome = "Joao Souza",
                Email = "joao@renova.com",
                SenhaHash = "hash"
            };

            _ = context.Usuarios.Add(usuario);
            _ = context.Usuarios.Add(outroUsuario);
            _ = await context.SaveChangesAsync();

            _ = context.Lojas.Add(new LojaModel
            {
                Nome = "Loja Externa",
                UsuarioId = outroUsuario.Id
            });
            _ = await context.SaveChangesAsync();

            LojaService service = new(context);
            IReadOnlyList<LojaDto> resultado = await service.GetAllAsync(new ObterLojasParametros
            {
                UsuarioId = usuario.Id
            });

            Assert.Empty(resultado);
        }

        [Fact]
        //Input: usuario autenticado inexistente
        //Nao consulta lojas para usuario invalido
        //Retorna: erro de autenticacao/regra
        public async Task GetAllAsyncDeveFalharQuandoUsuarioAutenticadoNaoExistir()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaService service = new(context);

            _ = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.GetAllAsync(new ObterLojasParametros
            {
                UsuarioId = 999
            }));
        }
    }
}