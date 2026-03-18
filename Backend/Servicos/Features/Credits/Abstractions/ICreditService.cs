using Renova.Services.Features.Credits.Contracts;

namespace Renova.Services.Features.Credits.Abstractions;

// Define os casos de uso do modulo 10 para API e consumidores futuros.
public interface ICreditService
{
    /// <summary>
    /// Carrega a visao consolidada de contas, pessoas e filtros do modulo.
    /// </summary>
    Task<CreditsWorkspaceResponse> ObterWorkspaceAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Carrega o saldo e o extrato detalhado de uma pessoa na loja ativa.
    /// </summary>
    Task<CreditAccountDetailResponse> ObterDetalhePorPessoaAsync(Guid pessoaId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Garante a existencia de uma conta de credito unica para a pessoa na loja ativa.
    /// </summary>
    Task<CreditAccountDetailResponse> GarantirContaAsync(EnsureCreditAccountRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registra um credito manual com justificativa obrigatoria.
    /// </summary>
    Task<CreditAccountDetailResponse> RegistrarCreditoManualAsync(ManualCreditRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registra um credito de repasse para uso posterior pelo fluxo de pagamentos.
    /// </summary>
    Task<CreditAccountDetailResponse> RegistrarCreditoRepasseAsync(SupplierPassThroughCreditRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza o status operacional da conta de credito.
    /// </summary>
    Task<CreditAccountDetailResponse> AtualizarStatusContaAsync(Guid contaId, UpdateCreditAccountStatusRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Consulta a conta de credito da pessoa vinculada ao usuario autenticado.
    /// </summary>
    Task<CreditAccountDetailResponse> ObterMinhaContaAsync(CancellationToken cancellationToken = default);
}
