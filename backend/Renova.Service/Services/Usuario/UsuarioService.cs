using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.Usuario;
using Renova.Service.Extensions;
using Renova.Service.Parameters.Usuario;
using Renova.Service.Queries.Usuario;

namespace Renova.Service.Services.Usuario
{
    public class UsuarioService(RenovaDbContext context) : IUsuarioService
    {
        private readonly RenovaDbContext _context = context;

        private static readonly IReadOnlyDictionary<string, LambdaExpression> CamposOrdenaveis = new Dictionary<string, LambdaExpression>
        {
            ["id"] = (Expression<Func<UsuarioModel, int>>)(usuario => usuario.Id),
            ["nome"] = (Expression<Func<UsuarioModel, string>>)(usuario => usuario.Nome),
            ["email"] = (Expression<Func<UsuarioModel, string>>)(usuario => usuario.Email)
        };

        public async Task<PaginacaoDto<UsuarioDto>> GetAllAsync(
            ObterUsuariosQuery request,
            int usuarioAutenticadoId,
            CancellationToken cancellationToken = default)
        {
            bool usuarioExiste = await _context.Usuarios
                .AnyAsync(usuario => usuario.Id == usuarioAutenticadoId, cancellationToken);

            if (!usuarioExiste)
            {
                throw new UnauthorizedAccessException("Usuario autenticado nao encontrado.");
            }

            IQueryable<UsuarioModel> query = _context.Usuarios.AsQueryable();

            if (request.LojaId.HasValue)
            {
                LojaModel loja = await _context.ObterLojaAcessivelAoUsuarioAsync(
                    request.LojaId.Value,
                    usuarioAutenticadoId,
                    cancellationToken);

                query = query.Where(usuario => usuario.Id != loja.UsuarioId);
            }

            if (!string.IsNullOrWhiteSpace(request.Busca))
            {
                string buscaNormalizada = request.Busca.Trim().ToLowerInvariant();
                query = query.Where(usuario =>
                    usuario.Nome.ToLower().Contains(buscaNormalizada) ||
                    usuario.Email.ToLower().Contains(buscaNormalizada));
            }

            IQueryable<UsuarioDto> queryProjetada = query
                .ApplyOrdering(request.OrdenarPor, request.Direcao, CamposOrdenaveis, "nome")
                .ThenBy(usuario => usuario.Id)
                .Select(usuario => new UsuarioDto
                {
                    Id = usuario.Id,
                    Nome = usuario.Nome,
                    Email = usuario.Email
                });

            return await queryProjetada.ToPagedResultAsync(request.Pagina, request.TamanhoPagina, cancellationToken);
        }

        public async Task<UsuarioDto> EditAsync(
            EditarUsuarioCommand command,
            EditarUsuarioParametros parametros,
            CancellationToken cancellationToken = default)
        {
            UsuarioModel? usuarioAutenticado = await _context.Usuarios
                .FirstOrDefaultAsync(usuario => usuario.Id == parametros.UsuarioAutenticadoId, cancellationToken);

            if (usuarioAutenticado is null)
            {
                throw new UnauthorizedAccessException("Usuario autenticado nao encontrado.");
            }

            UsuarioModel? usuario = await _context.Usuarios
                .FirstOrDefaultAsync(item => item.Id == parametros.UsuarioId, cancellationToken);

            if (usuario is null)
            {
                throw new KeyNotFoundException("Usuario nao encontrado.");
            }

            if (parametros.UsuarioAutenticadoId != parametros.UsuarioId)
            {
                throw new UnauthorizedAccessException("Voce nao tem permissao para editar este usuario.");
            }

            usuario.Nome = command.Nome.Trim();

            if (!string.IsNullOrWhiteSpace(command.NovaSenha))
            {
                if (string.IsNullOrWhiteSpace(command.SenhaAtual))
                {
                    throw new ArgumentException("Informe a senha atual para alterar a senha.");
                }

                bool senhaAtualValida = BCrypt.Net.BCrypt.Verify(command.SenhaAtual, usuario.SenhaHash);

                if (!senhaAtualValida)
                {
                    throw new UnauthorizedAccessException("Senha atual invalida.");
                }

                usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(command.NovaSenha);
            }

            _ = await _context.SaveChangesAsync(cancellationToken);

            return new UsuarioDto
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email
            };
        }
    }
}
