using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Renova.Persistence;
using Renova.Services.Features.Access;
using Renova.Services.Features.Access.Abstractions;

namespace Renova.Api.Infrastructure.Security;

// Representa o handler que autentica requisicoes a partir do token opaco de sessao.
public sealed class SessionTokenAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string UserIdClaimType = "renova:user_id";
    public const string SessionIdClaimType = "renova:session_id";
    public const string ActiveStoreIdClaimType = "renova:active_store_id";

    private readonly RenovaDbContext _dbContext;
    private readonly ITokenService _tokenService;

    /// <summary>
    /// Inicializa o handler com dependencias de autenticacao e persistencia.
    /// </summary>
    public SessionTokenAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        RenovaDbContext dbContext,
        ITokenService tokenService) : base(options, logger, encoder)
    {
        _dbContext = dbContext;
        _tokenService = tokenService;
    }

    /// <summary>
    /// Resolve o bearer token, valida a sessao e monta o principal autenticado.
    /// </summary>
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.Authorization.Any())
        {
            return AuthenticateResult.NoResult();
        }

        var authorizationHeader = Request.Headers.Authorization.ToString();
        if (!authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticateResult.NoResult();
        }

        var rawToken = authorizationHeader["Bearer ".Length..].Trim();
        if (string.IsNullOrWhiteSpace(rawToken))
        {
            return AuthenticateResult.Fail("Token inválido.");
        }

        var tokenHash = _tokenService.Hash(rawToken);
        var sessao = await _dbContext.UsuarioSessoes
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TokenHash == tokenHash &&
                     x.RevogadoEm == null &&
                     x.ExpiraEm >= DateTimeOffset.UtcNow,
                Context.RequestAborted);

        if (sessao is null)
        {
            return AuthenticateResult.Fail("Sessão não encontrada ou expirada.");
        }

        var usuario = await _dbContext.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == sessao.UsuarioId, Context.RequestAborted);

        if (usuario is null)
        {
            return AuthenticateResult.Fail("Usuário da sessão não encontrado.");
        }

        if (!string.Equals(usuario.StatusUsuario, AccessStatusValues.Usuario.Ativo, StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticateResult.Fail("O usuário não está ativo.");
        }

        var claims = new List<Claim>
        {
            new(UserIdClaimType, usuario.Id.ToString()),
            new(SessionIdClaimType, sessao.Id.ToString()),
            new(ClaimTypes.Name, usuario.Nome),
            new(ClaimTypes.Email, usuario.Email),
        };

        if (sessao.LojaAtivaId.HasValue)
        {
            claims.Add(new Claim(ActiveStoreIdClaimType, sessao.LojaAtivaId.Value.ToString()));
        }

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        return AuthenticateResult.Success(ticket);
    }
}
