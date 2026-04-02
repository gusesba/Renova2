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

            UsuarioModel usuario = new()
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

        public async Task<UsuarioTokenDto> LoginAsync(LoginCommand request, CancellationToken cancellationToken = default)
        {
            UsuarioModel? usuario = await _context.Usuarios
                .SingleOrDefaultAsync(usuario => usuario.Email == request.Email, cancellationToken) ?? throw new UnauthorizedAccessException("Credenciais invalidas.");
            bool senhaValida = BCrypt.Net.BCrypt.Verify(request.Senha, usuario.SenhaHash);

            return !senhaValida
                ? throw new UnauthorizedAccessException("Credenciais invalidas.")
                : new UsuarioTokenDto
                {
                    Usuario = new UsuarioDto
                    {
                        Id = usuario.Id,
                        Nome = usuario.Nome,
                        Email = usuario.Email
                    },
                    Token = _jwtTokenService.GenerateToken(usuario)
                };
        }
    }
}