using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Renova.Persistence;

namespace Renova.API.Controllers
{
    public abstract class AuthenticatedControllerBase(RenovaDbContext context) : ControllerBase
    {
        private readonly RenovaDbContext _context = context;

        protected async Task<int?> ObterUsuarioIdAsync(CancellationToken cancellationToken)
        {
            string? usuarioIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(usuarioIdClaim, out int usuarioId))
            {
                return usuarioId;
            }

            string? email = User.FindFirstValue(JwtRegisteredClaimNames.Email)
                ?? User.FindFirstValue(ClaimTypes.Email);

            return string.IsNullOrWhiteSpace(email)
                ? null
                : await _context.Usuarios
                .Where(usuario => usuario.Email == email)
                .Select(usuario => (int?)usuario.Id)
                .SingleOrDefaultAsync(cancellationToken);
        }
    }
}