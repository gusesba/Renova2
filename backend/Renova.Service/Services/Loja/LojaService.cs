using Microsoft.EntityFrameworkCore;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.Loja;
using Renova.Service.Parameters.Loja;

namespace Renova.Service.Services.Loja
{
    public class LojaService(RenovaDbContext context) : ILojaService
    {
        private readonly RenovaDbContext _context = context;

        public async Task<LojaDto> CreateAsync(CriarLojaCommand request, CriarLojaParametros parametros, CancellationToken cancellationToken = default)
        {
            string nomeNormalizado = request.Nome.Trim();

            bool usuarioExiste = await _context.Usuarios
                .AnyAsync(usuario => usuario.Id == parametros.UsuarioId, cancellationToken);

            if (!usuarioExiste)
            {
                throw new UnauthorizedAccessException("Usuario autenticado nao encontrado.");
            }

            bool lojaJaExiste = await _context.Lojas
                .AnyAsync(loja => loja.UsuarioId == parametros.UsuarioId && loja.Nome == nomeNormalizado, cancellationToken);

            if (lojaJaExiste)
            {
                throw new InvalidOperationException("Usuario ja possui uma loja com este nome.");
            }

            LojaModel loja = new()
            {
                Nome = nomeNormalizado,
                UsuarioId = parametros.UsuarioId
            };

            _ = await _context.Lojas.AddAsync(loja, cancellationToken);
            _ = await _context.SaveChangesAsync(cancellationToken);

            return new LojaDto
            {
                Id = loja.Id,
                Nome = loja.Nome
            };
        }

        public async Task<IReadOnlyList<LojaDto>> GetAllAsync(ObterLojasParametros parametros, CancellationToken cancellationToken = default)
        {
            bool usuarioExiste = await _context.Usuarios
                .AnyAsync(usuario => usuario.Id == parametros.UsuarioId, cancellationToken);

            return !usuarioExiste
                ? throw new UnauthorizedAccessException("Usuario autenticado nao encontrado.")
                : (IReadOnlyList<LojaDto>)await _context.Lojas
                .Where(loja => loja.UsuarioId == parametros.UsuarioId)
                .OrderBy(loja => loja.Nome)
                .ThenBy(loja => loja.Id)
                .Select(loja => new LojaDto
                {
                    Id = loja.Id,
                    Nome = loja.Nome
                })
                .ToListAsync(cancellationToken);
        }
    }
}