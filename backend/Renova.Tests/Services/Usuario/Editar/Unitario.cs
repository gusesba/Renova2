using Microsoft.EntityFrameworkCore;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.Usuario;
using Renova.Service.Parameters.Usuario;
using Renova.Service.Services.Usuario;
using Renova.Tests.Infrastructure;

namespace Renova.Tests.Services.Usuario.Editar
{
    public class Unitario : InMemoryDbContextTestBase<RenovaDbContext>
    {
        protected override RenovaDbContext CriarContexto(DbContextOptions<RenovaDbContext> options)
        {
            return new RenovaDbContext(options);
        }

        [Fact]
        public async Task EditAsyncDeveAtualizarNomeDoUsuarioQuandoPayloadForValido()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = await CriarUsuarioAsync(context, "Maria Teste", "maria@renova.com");

            UsuarioService service = new(context);
            UsuarioDto resultado = await service.EditAsync(new EditarUsuarioCommand
            {
                Nome = "Maria Atualizada"
            }, new EditarUsuarioParametros
            {
                UsuarioAutenticadoId = usuario.Id,
                UsuarioId = usuario.Id
            });

            Assert.Equal(usuario.Id, resultado.Id);
            Assert.Equal("Maria Atualizada", resultado.Nome);
            Assert.Equal(usuario.Email, resultado.Email);

            UsuarioModel usuarioSalvo = await context.Usuarios.SingleAsync();
            Assert.Equal("Maria Atualizada", usuarioSalvo.Nome);
        }

        [Fact]
        public async Task EditAsyncDeveFalharQuandoUsuarioAutenticadoNaoExistir()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = await CriarUsuarioAsync(context, "Maria Teste", "maria@renova.com");
            UsuarioService service = new(context);

            _ = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.EditAsync(
                new EditarUsuarioCommand
                {
                    Nome = "Novo Nome"
                },
                new EditarUsuarioParametros
                {
                    UsuarioAutenticadoId = 999,
                    UsuarioId = usuario.Id
                }));
        }

        [Fact]
        public async Task EditAsyncDeveFalharQuandoUsuarioTentarEditarOutroUsuario()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = await CriarUsuarioAsync(context, "Maria Teste", "maria@renova.com");
            UsuarioModel outroUsuario = await CriarUsuarioAsync(context, "Joao Teste", "joao@renova.com");
            UsuarioService service = new(context);

            _ = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.EditAsync(
                new EditarUsuarioCommand
                {
                    Nome = "Novo Nome"
                },
                new EditarUsuarioParametros
                {
                    UsuarioAutenticadoId = usuario.Id,
                    UsuarioId = outroUsuario.Id
                }));
        }

        [Fact]
        public async Task EditAsyncDeveFalharQuandoUsuarioNaoForEncontrado()
        {
            await using RenovaDbContext context = CriarContextoEmMemoria();

            UsuarioModel usuario = await CriarUsuarioAsync(context, "Maria Teste", "maria@renova.com");
            UsuarioService service = new(context);

            _ = await Assert.ThrowsAsync<KeyNotFoundException>(() => service.EditAsync(
                new EditarUsuarioCommand
                {
                    Nome = "Novo Nome"
                },
                new EditarUsuarioParametros
                {
                    UsuarioAutenticadoId = usuario.Id,
                    UsuarioId = 999
                }));
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
