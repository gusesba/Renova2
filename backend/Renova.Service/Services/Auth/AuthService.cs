using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

using Renova.Domain.Model;
using Renova.Domain.Model.Dto;
using Renova.Persistence;
using Renova.Service.Commands.Auth;

namespace Renova.Service.Services.Auth
{
    public class AuthService(RenovaDbContext context, IJwtTokenService jwtTokenService) : IAuthService
    {
        private readonly RenovaDbContext _context = context;
        private readonly IJwtTokenService _jwtTokenService = jwtTokenService;

        public async Task<UsuarioTokenDto> CreateAsync(CadastroCommand request, CancellationToken cancellationToken = default)
        {
            bool emailJaCadastrado = await _context.Usuarios
                .AnyAsync(usuario => usuario.Email == request.Email, cancellationToken);

            if (emailJaCadastrado)
            {
                throw new InvalidOperationException("Email ja cadastrado.");
            }

            var usuario = new UsuarioModel
            {
                Nome = request.Nome,
                Email = request.Email,
                SenhaHash = BCrypt.Net.BCrypt.HashPassword(request.Senha)
            };

            EntityEntry<UsuarioModel> resultado = await _context.Usuarios.AddAsync(usuario, cancellationToken);
            _ = await _context.SaveChangesAsync(cancellationToken);

            return new UsuarioTokenDto
            {
                Usuario = new UsuarioDto
                {
                    Id = resultado.Entity.Id,
                    Nome = resultado.Entity.Nome,
                    Email = resultado.Entity.Email
                },
                Token = _jwtTokenService.GenerateToken(resultado.Entity)
            };
        }

        public Task<UsuarioTokenDto> LoginAsync(LoginCommand request, CancellationToken cancellationToken = default)
        {
            _ = request;
            _ = cancellationToken;

            throw new NotImplementedException();
        }
    }
}