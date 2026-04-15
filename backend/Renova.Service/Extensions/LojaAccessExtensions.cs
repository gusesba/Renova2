using Microsoft.EntityFrameworkCore;

using Renova.Domain.Model;
using Renova.Persistence;

namespace Renova.Service.Extensions
{
    public static class LojaAccessExtensions
    {
        public static IQueryable<LojaModel> ObterLojasAcessiveisAoUsuario(this RenovaDbContext context, int usuarioId)
        {
            return context.Lojas
                .Where(loja =>
                    loja.UsuarioId == usuarioId
                    || context.Funcionarios.Any(funcionario =>
                        funcionario.LojaId == loja.Id
                        && funcionario.UsuarioId == usuarioId));
        }

        public static async Task<LojaModel> ObterLojaAcessivelAoUsuarioAsync(
            this RenovaDbContext context,
            int lojaId,
            int usuarioId,
            CancellationToken cancellationToken = default,
            bool lancarQuandoLojaNaoExistir = false)
        {
            bool usuarioExiste = await context.Usuarios
                .AnyAsync(usuario => usuario.Id == usuarioId, cancellationToken);

            if (!usuarioExiste)
            {
                throw new UnauthorizedAccessException("Usuario autenticado nao encontrado.");
            }

            LojaModel? loja = await context.Lojas
                .SingleOrDefaultAsync(lojaAtual => lojaAtual.Id == lojaId, cancellationToken);

            if (loja is null)
            {
                throw lancarQuandoLojaNaoExistir
                    ? new KeyNotFoundException("Loja informada nao foi encontrada.")
                    : new UnauthorizedAccessException("Loja informada nao pertence ao usuario autenticado.");
            }

            bool usuarioTemAcesso = loja.UsuarioId == usuarioId
                || await context.Funcionarios.AnyAsync(
                    funcionario =>
                        funcionario.LojaId == lojaId
                        && funcionario.UsuarioId == usuarioId,
                    cancellationToken);

            return !usuarioTemAcesso
                ? throw new UnauthorizedAccessException("Loja informada nao pertence ao usuario autenticado.")
                : loja;
        }
    }
}
