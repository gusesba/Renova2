using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Renova.Domain.Model.Dto;
using Renova.Domain.Model;
using Renova.Persistence;
using Renova.Service.Commands;

namespace Renova.Service.Services;

public class AuthService(RenovaDbContext context, IJwtTokenService jwtTokenService) : IAuthService
{
    private readonly RenovaDbContext _context = context;
    private readonly IJwtTokenService _jwtTokenService = jwtTokenService;

    public async Task<UsuarioTokenDto> CreateAsync(CadastroCommand request, CancellationToken cancellationToken = default)
    {
        var emailJaCadastrado = await _context.Usuarios
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

        var resultado = await _context.Usuarios.AddAsync(usuario, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

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
}
