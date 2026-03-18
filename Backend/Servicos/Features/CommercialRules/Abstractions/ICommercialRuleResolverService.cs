using Renova.Services.Features.CommercialRules.Contracts;

namespace Renova.Services.Features.CommercialRules.Abstractions;

// Resolve a regra comercial efetiva a partir da ordem de prioridade do negocio.
public interface ICommercialRuleResolverService
{
    /// <summary>
    /// Resolve a regra efetiva pela ordem manual, fornecedor e loja.
    /// </summary>
    Task<EffectiveCommercialRuleResponse> ResolverAsync(
        Guid lojaId,
        Guid? pessoaLojaId,
        ManualCommercialRuleInput? regraManual = null,
        CancellationToken cancellationToken = default);
}
