using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Renova.Domain.Model;
using Renova.Domain.Settings;
using Renova.Service.Services.Auth;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Renova.Service.Services;

public class JwtTokenService(IOptions<JwtSettings> jwtOptions) : IJwtTokenService
{
    private readonly JwtSettings _jwtSettings = jwtOptions.Value;

    public string GenerateToken(UsuarioModel usuario)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, usuario.Email),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
            new Claim("nome", usuario.Nome)
        };

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
