using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Renova.Api.Common.Controllers;
using Renova.Api.Common.Responses;
using Renova.Api.Infrastructure.Security;
using Renova.Services.Features.Access;
using Renova.Services.Features.Catalogs.Abstractions;
using Renova.Services.Features.Catalogs.Contracts;

namespace Renova.Api.Features.Catalogs.V1;

// Controller HTTP do modulo 04 com cadastros auxiliares reduzidos da loja ativa.
[ApiVersion("1.0")]
[Authorize]
[RequirePermission(AccessPermissionCodes.CatalogoGerenciar)]
[Route("api/v{version:apiVersion}/catalogs")]
public sealed class CatalogsController : RenovaControllerBase
{
    private readonly ICatalogService _catalogService;

    /// <summary>
    /// Inicializa o controller com o service do modulo.
    /// </summary>
    public CatalogsController(ICatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    [HttpGet("workspace")]
    /// <summary>
    /// Carrega os cadastros auxiliares da loja ativa.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<CatalogWorkspaceResponse>>> GetWorkspace(CancellationToken cancellationToken)
    {
        var response = await _catalogService.ObterWorkspaceAsync(cancellationToken);
        return OkEnvelope(response);
    }

    [HttpPost("product-names")]
    /// <summary>
    /// Cria um nome de produto para a loja ativa.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<ProductNameResponse>>> CreateProductName(
        [FromBody] CreateProductNameRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _catalogService.CriarProdutoNomeAsync(request, cancellationToken);
        return OkEnvelope(response);
    }

    [HttpPut("product-names/{produtoNomeId:guid}")]
    /// <summary>
    /// Atualiza um nome de produto existente.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<ProductNameResponse>>> UpdateProductName(
        Guid produtoNomeId,
        [FromBody] UpdateProductNameRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _catalogService.AtualizarProdutoNomeAsync(produtoNomeId, request, cancellationToken);
        return OkEnvelope(response);
    }

    [HttpPost("brands")]
    /// <summary>
    /// Cria uma marca para a loja ativa.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<BrandResponse>>> CreateBrand(
        [FromBody] CreateBrandRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _catalogService.CriarMarcaAsync(request, cancellationToken);
        return OkEnvelope(response);
    }

    [HttpPut("brands/{marcaId:guid}")]
    /// <summary>
    /// Atualiza uma marca existente.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<BrandResponse>>> UpdateBrand(
        Guid marcaId,
        [FromBody] UpdateBrandRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _catalogService.AtualizarMarcaAsync(marcaId, request, cancellationToken);
        return OkEnvelope(response);
    }

    [HttpPost("sizes")]
    /// <summary>
    /// Cria um tamanho para a loja ativa.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<SizeResponse>>> CreateSize(
        [FromBody] CreateSizeRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _catalogService.CriarTamanhoAsync(request, cancellationToken);
        return OkEnvelope(response);
    }

    [HttpPut("sizes/{tamanhoId:guid}")]
    /// <summary>
    /// Atualiza um tamanho existente.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<SizeResponse>>> UpdateSize(
        Guid tamanhoId,
        [FromBody] UpdateSizeRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _catalogService.AtualizarTamanhoAsync(tamanhoId, request, cancellationToken);
        return OkEnvelope(response);
    }

    [HttpPost("colors")]
    /// <summary>
    /// Cria uma cor para a loja ativa.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<ColorResponse>>> CreateColor(
        [FromBody] CreateColorRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _catalogService.CriarCorAsync(request, cancellationToken);
        return OkEnvelope(response);
    }

    [HttpPut("colors/{corId:guid}")]
    /// <summary>
    /// Atualiza uma cor existente.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<ColorResponse>>> UpdateColor(
        Guid corId,
        [FromBody] UpdateColorRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _catalogService.AtualizarCorAsync(corId, request, cancellationToken);
        return OkEnvelope(response);
    }
}
