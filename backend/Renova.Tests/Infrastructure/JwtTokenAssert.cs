using Microsoft.IdentityModel.Tokens;
using Renova.Domain.Settings;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Renova.Tests.Infrastructure;

public static class JwtTokenAssert
{
    public static JwtSettings CreateTestingSettings()
    {
        return new JwtSettings
        {
            SecretKey = "renova-test-secret-key-com-tamanho-minimo-32",
            Issuer = "Renova.Tests",
            Audience = "Renova.Tests.Client",
            ExpirationMinutes = 60
        };
    }

    public static ClaimsPrincipal Validate(string token, JwtSettings settings)
    {
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = settings.Issuer,
            ValidAudience = settings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SecretKey)),
            ClockSkew = TimeSpan.Zero
        };

        var handler = new JwtSecurityTokenHandler();
        return handler.ValidateToken(token, validationParameters, out _);
    }

    public static JwtSecurityToken Read(string token)
    {
        return new JwtSecurityTokenHandler().ReadJwtToken(token);
    }
}
