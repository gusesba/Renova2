using Microsoft.EntityFrameworkCore;
using Renova.Persistence;
using Renova.Services.Features.CommercialRules.Abstractions;
using Renova.Services.Features.CommercialRules.Contracts;

namespace Renova.Services.Features.CommercialRules.Services;

// Resolve a regra comercial efetiva respeitando a ordem de prioridade documentada.
public sealed class CommercialRuleResolverService : ICommercialRuleResolverService
{
    private readonly RenovaDbContext _dbContext;

    /// <summary>
    /// Inicializa o resolvedor com acesso de leitura ao banco.
    /// </summary>
    public CommercialRuleResolverService(RenovaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Resolve a regra efetiva da peca usando manual, fornecedor e loja.
    /// </summary>
    public async Task<EffectiveCommercialRuleResponse> ResolverAsync(
        Guid lojaId,
        Guid? pessoaLojaId,
        ManualCommercialRuleInput? regraManual = null,
        CancellationToken cancellationToken = default)
    {
        if (regraManual is not null)
        {
            return new EffectiveCommercialRuleResponse(
                "manual",
                regraManual.PercentualRepasseDinheiro,
                regraManual.PercentualRepasseCredito,
                regraManual.PermitePagamentoMisto,
                regraManual.TempoMaximoExposicaoDias,
                regraManual.PoliticaDesconto);
        }

        if (pessoaLojaId.HasValue)
        {
            var regraFornecedor = await _dbContext.FornecedorRegrasComerciais
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.PessoaLojaId == pessoaLojaId.Value && x.Ativo,
                    cancellationToken);

            if (regraFornecedor is not null)
            {
                return new EffectiveCommercialRuleResponse(
                    "fornecedor",
                    regraFornecedor.PercentualRepasseDinheiro,
                    regraFornecedor.PercentualRepasseCredito,
                    regraFornecedor.PermitePagamentoMisto,
                    regraFornecedor.TempoMaximoExposicaoDias,
                    CommercialRulePolicySerializer.Deserialize(regraFornecedor.PoliticaDescontoJson));
            }
        }

        var regraLoja = await _dbContext.LojaRegrasComerciais
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.LojaId == lojaId && x.Ativo, cancellationToken)
            ?? throw new InvalidOperationException("Cadastre a regra comercial padrao da loja para continuar.");

        return new EffectiveCommercialRuleResponse(
            "loja",
            regraLoja.PercentualRepasseDinheiro,
            regraLoja.PercentualRepasseCredito,
            regraLoja.PermitePagamentoMisto,
            regraLoja.TempoMaximoExposicaoDias,
            CommercialRulePolicySerializer.Deserialize(regraLoja.PoliticaDescontoJson));
    }
}
