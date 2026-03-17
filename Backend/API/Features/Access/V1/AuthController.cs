using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Renova.Api.Common.Controllers;
using Renova.Api.Common.Responses;
using Renova.Services.Features.Access.Abstractions;
using Renova.Services.Features.Access.Contracts;

namespace Renova.Api.Features.Access.V1;

// Representa o controlador HTTP do fluxo de autenticacao e recuperacao de acesso.
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/access/auth")]
public sealed class AuthController : RenovaControllerBase
{
    private readonly IAccessAuthService _accessAuthService;

    /// <summary>
    /// Inicializa o controller com os servicos de autenticacao.
    /// </summary>
    public AuthController(IAccessAuthService accessAuthService)
    {
        _accessAuthService = accessAuthService;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    /// <summary>
    /// Cria publicamente uma nova conta de acesso.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<RegisterResponse>>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _accessAuthService.RegistrarAsync(request, cancellationToken);
        return OkEnvelope(response);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    /// <summary>
    /// Autentica o usuario e cria uma sessao de acesso.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<LoginResponse>>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var response = await _accessAuthService.LoginAsync(request, cancellationToken);
        return OkEnvelope(response);
    }

    [Authorize]
    [HttpPost("logout")]
    /// <summary>
    /// Revoga a sessao autenticada atual.
    /// </summary>
    public async Task<ActionResult> Logout(CancellationToken cancellationToken)
    {
        await _accessAuthService.LogoutAsync(cancellationToken);
        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    /// <summary>
    /// Retorna o contexto do usuario autenticado.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<LoginContextResponse>>> Me(CancellationToken cancellationToken)
    {
        var response = await _accessAuthService.ObterContextoAtualAsync(cancellationToken);
        return OkEnvelope(response);
    }

    [Authorize]
    [HttpPost("active-store")]
    /// <summary>
    /// Troca a loja ativa vinculada a sessao.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<LoginContextResponse>>> SetActiveStore(
        [FromBody] SwitchActiveStoreRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _accessAuthService.AlterarLojaAtivaAsync(request, cancellationToken);
        return OkEnvelope(response);
    }

    [AllowAnonymous]
    [HttpPost("password-reset/request")]
    /// <summary>
    /// Gera um token de recuperacao para o usuario informado.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<PasswordResetRequestResponse>>> RequestPasswordReset(
        [FromBody] PasswordResetRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _accessAuthService.SolicitarRecuperacaoAsync(request, cancellationToken);
        return OkEnvelope(response);
    }

    [AllowAnonymous]
    [HttpPost("password-reset/confirm")]
    /// <summary>
    /// Redefine a senha com base em um token valido.
    /// </summary>
    public async Task<ActionResult> ConfirmPasswordReset(
        [FromBody] ConfirmPasswordResetRequest request,
        CancellationToken cancellationToken)
    {
        await _accessAuthService.RedefinirSenhaAsync(request, cancellationToken);
        return NoContent();
    }

    [Authorize]
    [HttpPost("change-password")]
    /// <summary>
    /// Altera a senha do usuario autenticado usando a senha atual.
    /// </summary>
    public async Task<ActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        await _accessAuthService.AlterarSenhaAsync(request, cancellationToken);
        return NoContent();
    }
}
