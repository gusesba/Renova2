using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Encodings.Web;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Renova.Tests.Infrastructure
{
    public class TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            string? authorization = Request.Headers.Authorization;

            if (string.IsNullOrWhiteSpace(authorization) || !authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            string token = authorization["Bearer ".Length..].Trim();

            if (string.IsNullOrWhiteSpace(token))
            {
                return Task.FromResult(AuthenticateResult.Fail("Token nao informado."));
            }

            List<Claim> claims = [];

            if (token.Count(character => character == '.') == 2)
            {
                JwtSecurityToken jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
                claims.AddRange(jwt.Claims);
            }
            else
            {
                claims.Add(new Claim(JwtRegisteredClaimNames.Sub, token));
                claims.Add(new Claim(ClaimTypes.NameIdentifier, token));
            }

            ClaimsIdentity identity = new(claims, Scheme.Name);
            ClaimsPrincipal principal = new(identity);
            AuthenticationTicket ticket = new(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}