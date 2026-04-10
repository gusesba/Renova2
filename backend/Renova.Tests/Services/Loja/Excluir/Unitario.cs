using Microsoft.EntityFrameworkCore;

using Renova.Domain.Model;
using Renova.Persistence;
using Renova.Service.Parameters.Loja;
using Renova.Service.Services.Loja;
using Renova.Tests.Infrastructure;

namespace Renova.Tests.Services.Loja.Excluir
{
    public class Unitario : InMemoryDbContextTestBase<RenovaDbContext>
    {
        protected override RenovaDbContext CriarContexto(DbContextOptions<RenovaDbContext> options)
        {
            return new RenovaDbContext(options);
        }

        [Fact]
        public async Task DeleteAsyncDeveExcluirLojaSemRegistrosAtivos()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");

            LojaService service = new(context);
            await service.DeleteAsync(new ExcluirLojaParametros
            {
                UsuarioId = loja.UsuarioId,
                LojaId = loja.Id
            });

            Assert.Empty(context.Lojas);
        }

        [Fact]
        public async Task DeleteAsyncDeveImpedirExclusaoQuandoLojaPossuirClientes()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");
            _ = await CriarClienteAsync(context, loja.Id, "Cliente A", "44999990000");

            LojaService service = new(context);
            InvalidOperationException ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteAsync(new ExcluirLojaParametros
            {
                UsuarioId = loja.UsuarioId,
                LojaId = loja.Id
            }));

            Assert.Equal("Nao e possivel excluir loja com registros ativos", ex.Message);
            _ = await context.Lojas.SingleAsync();
        }

        [Fact]
        public async Task DeleteAsyncDeveImpedirExclusaoQuandoLojaNaoPertencerAoUsuarioAutenticado()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");
            LojaModel outraLoja = await CriarLojaAsync(context, "Loja Bairro", "joao@renova.com");

            LojaService service = new(context);
            _ = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.DeleteAsync(new ExcluirLojaParametros
            {
                UsuarioId = outraLoja.UsuarioId,
                LojaId = loja.Id
            }));
        }

        [Fact]
        public async Task DeleteAsyncDeveFalharQuandoLojaNaoForEncontrada()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            LojaModel loja = await CriarLojaAsync(context, "Loja Centro", "maria@renova.com");

            LojaService service = new(context);
            _ = await Assert.ThrowsAsync<KeyNotFoundException>(() => service.DeleteAsync(new ExcluirLojaParametros
            {
                UsuarioId = loja.UsuarioId,
                LojaId = 999
            }));
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
    }
}
