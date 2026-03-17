using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Renova.Api.Common.Controllers;
using Renova.Api.Common.Responses;
using Renova.Services.Features.Stores.Abstractions;
using Renova.Services.Features.Stores.Contracts;

namespace Renova.Api.Features.Stores.V1;

// Controller HTTP do modulo de lojas e configuracao operacional.
[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/stores")]
public sealed class StoresController : RenovaControllerBase
{
    private readonly IStoreService _storeService;

    /// <summary>
    /// Inicializa o controller com os servicos do modulo de lojas.
    /// </summary>
    public StoresController(IStoreService storeService)
    {
        _storeService = storeService;
    }

    [HttpGet("accessible")]
    /// <summary>
    /// Lista apenas as lojas acessiveis ao usuario autenticado.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<IReadOnlyList<StoreResponse>>>> ListAccessible(CancellationToken cancellationToken)
    {
        var response = await _storeService.ListarAcessiveisAsync(cancellationToken);
        return OkEnvelope(response);
    }

    [HttpPost]
    /// <summary>
    /// Cria uma nova loja com sua configuracao inicial.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<StoreResponse>>> Create(
        [FromBody] CreateStoreRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _storeService.CriarAsync(request, cancellationToken);
        return OkEnvelope(response);
    }

    [HttpPut("{lojaId:guid}")]
    /// <summary>
    /// Atualiza o cadastro principal de uma loja.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<StoreResponse>>> Update(
        Guid lojaId,
        [FromBody] UpdateStoreRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _storeService.AtualizarAsync(lojaId, request, cancellationToken);
        return OkEnvelope(response);
    }

    [HttpPut("{lojaId:guid}/configuration")]
    /// <summary>
    /// Atualiza a configuracao operacional e de impressao de uma loja.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<StoreResponse>>> UpdateConfiguration(
        Guid lojaId,
        [FromBody] UpdateStoreConfigurationRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _storeService.AtualizarConfiguracaoAsync(lojaId, request, cancellationToken);
        return OkEnvelope(response);
    }
}
