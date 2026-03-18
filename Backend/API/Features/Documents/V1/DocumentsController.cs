using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Renova.Api.Common.Controllers;
using Renova.Api.Common.Responses;
using Renova.Services.Features.Documents.Abstractions;
using Renova.Services.Features.Documents.Contracts;

namespace Renova.Api.Features.Documents.V1;

// Controller HTTP do modulo 16 com busca e impressao de documentos.
[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/documents")]
public sealed class DocumentsController : RenovaControllerBase
{
    private readonly IDocumentService _documentService;

    /// <summary>
    /// Inicializa o controller com o service principal do modulo.
    /// </summary>
    public DocumentsController(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    [HttpGet("workspace")]
    /// <summary>
    /// Carrega a loja ativa e os tipos de documento disponiveis.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<DocumentWorkspaceResponse>>> GetWorkspace(CancellationToken cancellationToken)
    {
        var response = await _documentService.ObterWorkspaceAsync(cancellationToken);
        return OkEnvelope(response);
    }

    [HttpGet("labels")]
    /// <summary>
    /// Busca peças elegiveis para impressão de etiqueta.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<IReadOnlyList<DocumentSearchItemResponse>>>> SearchLabels(
        [FromQuery] DocumentSearchQueryRequest query,
        CancellationToken cancellationToken)
    {
        var response = await _documentService.BuscarEtiquetasAsync(query, cancellationToken);
        return OkEnvelope(response);
    }

    [HttpGet("sales")]
    /// <summary>
    /// Busca vendas para impressão de recibo.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<IReadOnlyList<DocumentSearchItemResponse>>>> SearchSales(
        [FromQuery] DocumentSearchQueryRequest query,
        CancellationToken cancellationToken)
    {
        var response = await _documentService.BuscarRecibosVendaAsync(query, cancellationToken);
        return OkEnvelope(response);
    }

    [HttpGet("supplier-payments")]
    /// <summary>
    /// Busca obrigações liquidadas para comprovante ao fornecedor.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<IReadOnlyList<DocumentSearchItemResponse>>>> SearchSupplierPayments(
        [FromQuery] DocumentSearchQueryRequest query,
        CancellationToken cancellationToken)
    {
        var response = await _documentService.BuscarComprovantesFornecedorAsync(query, cancellationToken);
        return OkEnvelope(response);
    }

    [HttpGet("consignments")]
    /// <summary>
    /// Busca devoluções e doações para impressão do comprovante.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<IReadOnlyList<DocumentSearchItemResponse>>>> SearchConsignments(
        [FromQuery] DocumentSearchQueryRequest query,
        CancellationToken cancellationToken)
    {
        var response = await _documentService.BuscarComprovantesConsignacaoAsync(query, cancellationToken);
        return OkEnvelope(response);
    }

    [HttpGet("labels/{pecaId:guid}")]
    /// <summary>
    /// Gera o HTML imprimível da etiqueta da peça.
    /// </summary>
    public async Task<IActionResult> PrintLabel(Guid pecaId, CancellationToken cancellationToken)
    {
        var response = await _documentService.ImprimirEtiquetaAsync(pecaId, cancellationToken);
        return File(response.Content, response.ContentType, response.FileName);
    }

    [HttpGet("sales/{vendaId:guid}")]
    /// <summary>
    /// Gera o HTML imprimível do recibo de venda.
    /// </summary>
    public async Task<IActionResult> PrintSaleReceipt(Guid vendaId, CancellationToken cancellationToken)
    {
        var response = await _documentService.ImprimirReciboVendaAsync(vendaId, cancellationToken);
        return File(response.Content, response.ContentType, response.FileName);
    }

    [HttpGet("supplier-payments/{obrigacaoId:guid}")]
    /// <summary>
    /// Gera o HTML imprimível do comprovante de pagamento ao fornecedor.
    /// </summary>
    public async Task<IActionResult> PrintSupplierPaymentReceipt(Guid obrigacaoId, CancellationToken cancellationToken)
    {
        var response = await _documentService.ImprimirComprovanteFornecedorAsync(obrigacaoId, cancellationToken);
        return File(response.Content, response.ContentType, response.FileName);
    }

    [HttpGet("consignments/{pecaId:guid}")]
    /// <summary>
    /// Gera o HTML imprimível do comprovante de devolução ou doação.
    /// </summary>
    public async Task<IActionResult> PrintConsignmentReceipt(Guid pecaId, CancellationToken cancellationToken)
    {
        var response = await _documentService.ImprimirComprovanteConsignacaoAsync(pecaId, cancellationToken);
        return File(response.Content, response.ContentType, response.FileName);
    }
}
