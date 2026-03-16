using System.Security.Claims;
using Renova.Services.Features.Access.Abstractions;

namespace Renova.Api.Infrastructure.Security;

// Representa a adaptacao do HttpContext para o contexto atual da requisicao.
public sealed class HttpCurrentRequestContext : ICurrentRequestContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Inicializa o adaptador com acesso ao contexto HTTP atual.
    /// </summary>
    public HttpCurrentRequestContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UsuarioId => ReadGuidClaim(SessionTokenAuthenticationHandler.UserIdClaimType);

    public Guid? SessaoId => ReadGuidClaim(SessionTokenAuthenticationHandler.SessionIdClaimType);

    public Guid? LojaAtivaId => ReadGuidClaim(SessionTokenAuthenticationHandler.ActiveStoreIdClaimType);

    public string Ip => _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "desconhecido";

    public string UserAgent => _httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString() ?? "desconhecido";

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;

    /// <summary>
    /// Le um claim Guid do usuario autenticado.
    /// </summary>
    private Guid? ReadGuidClaim(string claimType)
    {
        var value = _httpContextAccessor.HttpContext?.User.FindFirstValue(claimType);
        return Guid.TryParse(value, out var guid) ? guid : null;
    }
}
