using Microsoft.EntityFrameworkCore;

using Renova.Domain.Model;
using Renova.Persistence;

namespace Renova.Service.Extensions
{
    public static class LojaAccessExtensions
    {
        private const string InMemoryProviderName = "Microsoft.EntityFrameworkCore.InMemory";

        public static bool IsInMemoryProvider(this RenovaDbContext context)
        {
            return string.Equals(context.Database.ProviderName, InMemoryProviderName, StringComparison.Ordinal);
        }

        public static IQueryable<LiberacaoUsuarioModel> ObterLiberacoesAtivas(this RenovaDbContext context, DateTime agora)
        {
            return context.LiberacoesUsuarios
                .AsNoTracking()
                .Where(liberacao => liberacao.Ativo && liberacao.LiberadoAte >= agora);
        }

        public static IQueryable<LojaModel> ObterLojasAcessiveisAoUsuario(this RenovaDbContext context, int usuarioId)
        {
            if (context.IsInMemoryProvider())
            {
                return context.Lojas
                    .AsNoTracking()
                    .Where(loja =>
                        loja.UsuarioId == usuarioId
                        || context.Funcionarios.Any(funcionario =>
                            funcionario.LojaId == loja.Id
                            && funcionario.UsuarioId == usuarioId));
            }

            DateTime agora = DateTime.UtcNow;

            return context.Lojas
                .AsNoTracking()
                .Where(loja =>
                    context.ObterLiberacoesAtivas(agora).Any(liberacao => liberacao.UsuarioId == loja.UsuarioId)
                    && (
                        loja.UsuarioId == usuarioId
                        || context.Funcionarios.Any(funcionario =>
                            funcionario.LojaId == loja.Id
                            && funcionario.UsuarioId == usuarioId)));
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

            if (!usuarioTemAcesso)
            {
                throw new UnauthorizedAccessException("Loja informada nao pertence ao usuario autenticado.");
            }

            if (context.IsInMemoryProvider())
            {
                return loja;
            }

            bool lojaLiberada = await context.ObterLiberacoesAtivas(DateTime.UtcNow)
                .AnyAsync(liberacao => liberacao.UsuarioId == loja.UsuarioId, cancellationToken);

            if (!lojaLiberada)
            {
                throw new UnauthorizedAccessException(loja.UsuarioId == usuarioId
                    ? "Sua loja esta bloqueada porque seu plano nao esta ativo."
                    : "Loja bloqueada porque o plano do dono nao esta ativo.");
            }

            return loja;
        }
    }
}
