using Microsoft.EntityFrameworkCore;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.Loja;
using Renova.Service.Parameters.Loja;
using Renova.Service.Services.Loja;
using Renova.Tests.Infrastructure;

namespace Renova.Tests.Services.Loja.Editar
{
    public class Unitario : InMemoryDbContextTestBase<RenovaDbContext>
    {
        protected override RenovaDbContext CriarContexto(DbContextOptions<RenovaDbContext> options)
        {
            return new RenovaDbContext(options);
        }

        [Fact]
        public async Task EditAsyncDeveEditarLojaDoUsuarioAutenticado()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");

            LojaService service = new(context);
            LojaDto resultado = await service.EditAsync(new EditarLojaCommand
            {
                Nome = "Loja Premium"
            }, new EditarLojaParametros
            {
                UsuarioId = loja.UsuarioId,
                LojaId = loja.Id
            });

            Assert.Equal(loja.Id, resultado.Id);
            Assert.Equal("Loja Premium", resultado.Nome);

            LojaModel lojaSalva = await context.Lojas.SingleAsync();
            Assert.Equal("Loja Premium", lojaSalva.Nome);
        }

        [Fact]
        public async Task EditAsyncDeveImpedirEdicaoQuandoUsuarioJaPossuirOutraLojaComMesmoNome()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = await CriarUsuarioAsync(context, "maria@renova.com");
            LojaModel primeiraLoja = await CriarLojaAsync(context, usuario.Id, "Loja Centro");
            _ = await CriarLojaAsync(context, usuario.Id, "Loja Bairro");

            LojaService service = new(context);
            InvalidOperationException ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.EditAsync(new EditarLojaCommand
            {
                Nome = "Loja Bairro"
            }, new EditarLojaParametros
            {
                UsuarioId = primeiraLoja.UsuarioId,
                LojaId = primeiraLoja.Id
            }));

            Assert.Equal("Usuario ja possui uma loja com este nome.", ex.Message);
        }

        [Fact]
        public async Task EditAsyncDeveImpedirEdicaoQuandoLojaNaoPertencerAoUsuarioAutenticado()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");
            LojaModel outraLoja = await CriarLojaAsync(context, "Loja Bairro", "joao@renova.com");

            LojaService service = new(context);
            _ = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.EditAsync(new EditarLojaCommand
            {
                Nome = "Novo Nome"
            }, new EditarLojaParametros
            {
                UsuarioId = outraLoja.UsuarioId,
                LojaId = loja.Id
            }));
        }

        [Fact]
        public async Task EditAsyncDeveFalharQuandoLojaNaoForEncontrada()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");

            LojaService service = new(context);
            _ = await Assert.ThrowsAsync<KeyNotFoundException>(() => service.EditAsync(new EditarLojaCommand
            {
                Nome = "Novo Nome"
            }, new EditarLojaParametros
            {
                UsuarioId = loja.UsuarioId,
                LojaId = 999
            }));
        }

        private static async Task<UsuarioModel> CriarUsuarioAsync(RenovaDbContext context, string emailUsuario)
        {
            UsuarioModel usuario = new()
            {
                Nome = "Usuario de Teste",
                Email = emailUsuario,
                SenhaHash = "hash"
            };

            _ = context.Usuarios.Add(usuario);
            _ = await context.SaveChangesAsync();
            return usuario;
        }

        private static async Task<LojaModel> CriarLojaAsync(RenovaDbContext context, string nomeLoja, string emailUsuario)
        {
            UsuarioModel usuario = await CriarUsuarioAsync(context, emailUsuario);
            return await CriarLojaAsync(context, usuario.Id, nomeLoja);
        }

        private static async Task<LojaModel> CriarLojaAsync(RenovaDbContext context, int usuarioId, string nomeLoja)
        {
            LojaModel loja = new()
            {
                Nome = nomeLoja,
                UsuarioId = usuarioId
            };

            _ = context.Lojas.Add(loja);
            _ = await context.SaveChangesAsync();
            return loja;
        }
    }
}
