using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Renova.Api.Common.Controllers;
using Renova.Api.Common.Responses;
using Renova.Api.Infrastructure.Security;
using Renova.Services.Features.Access;
using Renova.Services.Features.Access.Abstractions;
using Renova.Services.Features.Access.Contracts;

namespace Renova.Api.Features.Access.V1;

// Representa o controlador HTTP de manutencao de usuarios do sistema.
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/access/users")]
public sealed class UsersController : RenovaControllerBase
{
    private readonly IAccessUserService _accessUserService;

    /// <summary>
    /// Inicializa o controller com os servicos de usuarios.
    /// </summary>
    public UsersController(IAccessUserService accessUserService)
    {
        _accessUserService = accessUserService;
    }

    [HttpGet]
    [RequirePermission(AccessPermissionCodes.UsuariosVisualizar)]
    /// <summary>
    /// Lista os usuarios com o resumo do vinculo na loja ativa.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<IReadOnlyList<UserSummaryResponse>>>> List(CancellationToken cancellationToken)
    {
        var response = await _accessUserService.ListarAsync(cancellationToken);
        return OkEnvelope(response);
    }

    [HttpPost]
    [RequirePermission(AccessPermissionCodes.UsuariosGerenciar)]
    /// <summary>
    /// Cria um novo usuario do sistema.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<UserSummaryResponse>>> Create([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        var response = await _accessUserService.CriarAsync(request, cancellationToken);
        return OkEnvelope(response);
    }

    [HttpPut("{usuarioId:guid}")]
    [RequirePermission(AccessPermissionCodes.UsuariosGerenciar)]
    /// <summary>
    /// Atualiza os dados cadastrais de um usuario.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<UserSummaryResponse>>> Update(
        Guid usuarioId,
        [FromBody] UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _accessUserService.AtualizarAsync(usuarioId, request, cancellationToken);
        return OkEnvelope(response);
    }

    [HttpPost("{usuarioId:guid}/status")]
    [RequirePermission(AccessPermissionCodes.UsuariosGerenciar)]
    /// <summary>
    /// Altera o status operacional de um usuario.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<UserSummaryResponse>>> ChangeStatus(
        Guid usuarioId,
        [FromBody] ChangeUserStatusRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _accessUserService.AlterarStatusAsync(usuarioId, request, cancellationToken);
        return OkEnvelope(response);
    }
}
