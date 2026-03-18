using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Renova.Api.Common.Controllers;
using Renova.Api.Common.Responses;
using Renova.Api.Infrastructure.Security;
using Renova.Services.Features.Access;
using Renova.Services.Features.Credits.Abstractions;
using Renova.Services.Features.Credits.Contracts;

namespace Renova.Api.Features.Credits.V1;

// Controller HTTP do modulo 10 com saldo, extrato e lancamentos manuais.
[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/credits")]
public sealed class CreditsController : RenovaControllerBase
{
    private readonly ICreditService _creditService;

    /// <summary>
    /// Inicializa o controller com o service principal do modulo.
    /// </summary>
    public CreditsController(ICreditService creditService)
    {
        _creditService = creditService;
    }

    [HttpGet("workspace")]
    /// <summary>
    /// Carrega a visao consolidada de contas e pessoas da loja ativa.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<CreditsWorkspaceResponse>>> GetWorkspace(CancellationToken cancellationToken)
    {
        var response = await _creditService.ObterWorkspaceAsync(cancellationToken);
        return OkEnvelope(response);
    }

    [HttpGet("person/{pessoaId:guid}")]
    /// <summary>
    /// Carrega o saldo e o extrato detalhado da pessoa informada.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<CreditAccountDetailResponse>>> GetByPerson(Guid pessoaId, CancellationToken cancellationToken)
    {
        var response = await _creditService.ObterDetalhePorPessoaAsync(pessoaId, cancellationToken);
        return OkEnvelope(response);
    }

    [HttpGet("me")]
    /// <summary>
    /// Carrega o saldo e o extrato da pessoa vinculada ao usuario autenticado.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<CreditAccountDetailResponse>>> GetMyAccount(CancellationToken cancellationToken)
    {
        var response = await _creditService.ObterMinhaContaAsync(cancellationToken);
        return OkEnvelope(response);
    }

    [HttpPost("accounts")]
    [RequirePermission(AccessPermissionCodes.CreditoGerenciar)]
    /// <summary>
    /// Garante a existencia da conta unica de credito por loja e pessoa.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<CreditAccountDetailResponse>>> EnsureAccount(
        [FromBody] EnsureCreditAccountRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _creditService.GarantirContaAsync(request, cancellationToken);
        return OkEnvelope(response);
    }

    [HttpPost("manual")]
    [RequirePermission(AccessPermissionCodes.CreditoGerenciar)]
    /// <summary>
    /// Registra um credito manual com justificativa obrigatoria.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<CreditAccountDetailResponse>>> RegisterManualCredit(
        [FromBody] ManualCreditRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _creditService.RegistrarCreditoManualAsync(request, cancellationToken);
        return OkEnvelope(response);
    }

    [HttpPost("pass-through")]
    [RequirePermission(AccessPermissionCodes.CreditoGerenciar)]
    /// <summary>
    /// Registra um credito por repasse para reutilizacao no fluxo de pagamentos.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<CreditAccountDetailResponse>>> RegisterPassThroughCredit(
        [FromBody] SupplierPassThroughCreditRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _creditService.RegistrarCreditoRepasseAsync(request, cancellationToken);
        return OkEnvelope(response);
    }

    [HttpPut("accounts/{contaId:guid}/status")]
    [RequirePermission(AccessPermissionCodes.CreditoGerenciar)]
    /// <summary>
    /// Atualiza o status operacional da conta de credito da pessoa.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<CreditAccountDetailResponse>>> UpdateStatus(
        Guid contaId,
        [FromBody] UpdateCreditAccountStatusRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _creditService.AtualizarStatusContaAsync(contaId, request, cancellationToken);
        return OkEnvelope(response);
    }
}
