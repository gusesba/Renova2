using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Renova.Api.Common.Controllers;
using Renova.Api.Common.Responses;
using Renova.Api.Infrastructure.Security;
using Renova.Services.Features.Access;
using Renova.Services.Features.Financial.Abstractions;
using Renova.Services.Features.Financial.Contracts;

namespace Renova.Api.Features.Financial.V1;

// Controller HTTP do modulo 12 com livro razao, resumo e lancamentos avulsos.
[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/financial")]
public sealed class FinancialController : RenovaControllerBase
{
    private readonly IFinancialService _financialService;

    /// <summary>
    /// Inicializa o controller com o service principal do modulo.
    /// </summary>
    public FinancialController(IFinancialService financialService)
    {
        _financialService = financialService;
    }

    [HttpGet("workspace")]
    /// <summary>
    /// Carrega meios de pagamento e filtros disponiveis da loja ativa.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<FinancialWorkspaceResponse>>> GetWorkspace(CancellationToken cancellationToken)
    {
        var response = await _financialService.ObterWorkspaceAsync(cancellationToken);
        return OkEnvelope(response);
    }

    [HttpGet]
    /// <summary>
    /// Lista o livro razao financeiro com os filtros informados.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<IReadOnlyList<FinancialLedgerEntryResponse>>>> List(
        [FromQuery] FinancialListQueryRequest query,
        CancellationToken cancellationToken)
    {
        var response = await _financialService.ListarAsync(query, cancellationToken);
        return OkEnvelope(response);
    }

    [HttpGet("reconciliation")]
    /// <summary>
    /// Consolida totais por periodo, meio de pagamento, tipo e dia.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<FinancialReconciliationResponse>>> GetReconciliation(
        [FromQuery] FinancialListQueryRequest query,
        CancellationToken cancellationToken)
    {
        var response = await _financialService.ObterConciliacaoAsync(query, cancellationToken);
        return OkEnvelope(response);
    }

    [HttpPost("entries")]
    [RequirePermission(AccessPermissionCodes.FinanceiroConciliar)]
    /// <summary>
    /// Registra um lancamento financeiro avulso na loja ativa.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<FinancialLedgerEntryResponse>>> CreateEntry(
        [FromBody] RegisterFinancialEntryRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _financialService.RegistrarLancamentoAsync(request, cancellationToken);
        return OkEnvelope(response);
    }
}
