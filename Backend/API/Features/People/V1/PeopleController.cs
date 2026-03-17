using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Renova.Api.Common.Controllers;
using Renova.Api.Common.Responses;
using Renova.Api.Infrastructure.Security;
using Renova.Services.Features.Access;
using Renova.Services.Features.People.Abstractions;
using Renova.Services.Features.People.Contracts;

namespace Renova.Api.Features.People.V1;

// Controller HTTP do modulo 03 de clientes e fornecedores.
[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/people")]
public sealed class PeopleController : RenovaControllerBase
{
    private readonly IPersonService _personService;

    /// <summary>
    /// Inicializa o controller com o service do modulo.
    /// </summary>
    public PeopleController(IPersonService personService)
    {
        _personService = personService;
    }

    [HttpGet]
    /// <summary>
    /// Lista as pessoas da loja ativa com resumo financeiro e operacional.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<IReadOnlyList<PersonSummaryResponse>>>> List(CancellationToken cancellationToken)
    {
        var response = await _personService.ListarAsync(cancellationToken);
        return OkEnvelope(response);
    }

    [HttpGet("{pessoaId:guid}")]
    /// <summary>
    /// Carrega o detalhe completo da pessoa no contexto da loja ativa.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<PersonDetailResponse>>> GetById(Guid pessoaId, CancellationToken cancellationToken)
    {
        var response = await _personService.ObterDetalheAsync(pessoaId, cancellationToken);
        return OkEnvelope(response);
    }

    [HttpGet("users")]
    [RequirePermission(AccessPermissionCodes.PessoasGerenciar)]
    /// <summary>
    /// Lista usuarios disponiveis para vinculacao ao cadastro da pessoa.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<IReadOnlyList<PersonUserOptionResponse>>>> ListLinkableUsers(CancellationToken cancellationToken)
    {
        var response = await _personService.ListarUsuariosVinculaveisAsync(cancellationToken);
        return OkEnvelope(response);
    }

    [HttpPost]
    [RequirePermission(AccessPermissionCodes.PessoasGerenciar)]
    /// <summary>
    /// Cria a pessoa e o vinculo dela com a loja ativa.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<PersonDetailResponse>>> Create(
        [FromBody] CreatePersonRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _personService.CriarAsync(request, cancellationToken);
        return OkEnvelope(response);
    }

    [HttpPut("{pessoaId:guid}")]
    [RequirePermission(AccessPermissionCodes.PessoasGerenciar)]
    /// <summary>
    /// Atualiza o cadastro mestre da pessoa e os dados da loja ativa.
    /// </summary>
    public async Task<ActionResult<ApiEnvelope<PersonDetailResponse>>> Update(
        Guid pessoaId,
        [FromBody] UpdatePersonRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _personService.AtualizarAsync(pessoaId, request, cancellationToken);
        return OkEnvelope(response);
    }
}
