using Renova.Domain.Models;
using Renova.Services.Features.CommercialRules;
using Renova.Services.Features.Pieces;

namespace Renova.Services.Features.Consignments;

// Centraliza o calculo do ciclo de consignacao sem mutar o preco persistido da peca.
internal static class ConsignmentLifecycleCalculator
{
    public static ConsignmentLifecycleSnapshot Calculate(
        Peca piece,
        PecaCondicaoComercial? commercialCondition,
        decimal basePrice,
        DateTimeOffset? referenceDate = null)
    {
        var now = referenceDate ?? DateTimeOffset.UtcNow;
        var effectiveBasePrice = RoundMoney(basePrice);

        if (commercialCondition is null || piece.TipoPeca != PieceValues.PieceTypes.Consignada)
        {
            return CreateDefaultSnapshot(piece, effectiveBasePrice);
        }

        var startDate = commercialCondition.DataInicioConsignacao ?? piece.DataEntrada;
        var endDate = commercialCondition.DataFimConsignacao ?? startDate.AddDays(commercialCondition.TempoMaximoExposicaoDias);
        var daysInStore = Math.Max(0, (int)Math.Floor((now - startDate).TotalDays));
        var daysRemaining = (int)Math.Ceiling((endDate - now).TotalDays);
        var isClosed = !ConsignmentValues.IsActivePieceStatus(piece.StatusPeca) || piece.QuantidadeAtual <= 0;
        var isExpired = !isClosed && daysRemaining <= 0;
        var isNear = !isClosed && !isExpired && daysRemaining <= ConsignmentValues.NearExpirationAlertThresholdDays;
        var status = isClosed
            ? ConsignmentValues.LifecycleStatuses.Encerrada
            : isExpired
                ? ConsignmentValues.LifecycleStatuses.Vencida
                : isNear
                    ? ConsignmentValues.LifecycleStatuses.Proxima
                    : ConsignmentValues.LifecycleStatuses.Ativa;

        var discountPolicy = CommercialRulePolicySerializer.Deserialize(commercialCondition.PoliticaDescontoJson);
        var applicableBand = discountPolicy
            .OrderBy(x => x.DiasMinimos)
            .LastOrDefault(x => x.DiasMinimos <= daysInStore);

        var expectedDiscount = isClosed ? 0m : applicableBand?.PercentualDesconto ?? 0m;
        var effectivePrice = RoundMoney(effectiveBasePrice * (1m - (expectedDiscount / 100m)));
        var effectiveDiscount = effectiveBasePrice <= 0m
            ? 0m
            : RoundPercentage(((effectiveBasePrice - effectivePrice) / effectiveBasePrice) * 100m);

        return new ConsignmentLifecycleSnapshot(
            effectiveBasePrice,
            effectivePrice,
            effectiveDiscount,
            expectedDiscount,
            !isClosed && expectedDiscount > 0m,
            startDate,
            endDate,
            daysInStore,
            isClosed ? null : daysRemaining,
            isNear,
            isExpired,
            status);
    }

    private static ConsignmentLifecycleSnapshot CreateDefaultSnapshot(Peca piece, decimal basePrice)
    {
        var effectivePrice = RoundMoney(piece.PrecoVendaAtual);
        var appliedDiscount = basePrice <= 0m
            ? 0m
            : RoundPercentage(((basePrice - effectivePrice) / basePrice) * 100m);

        return new ConsignmentLifecycleSnapshot(
            basePrice,
            effectivePrice,
            appliedDiscount,
            appliedDiscount,
            false,
            piece.DataEntrada,
            piece.DataEntrada,
            0,
            null,
            false,
            false,
            ConsignmentValues.LifecycleStatuses.Ativa);
    }

    private static decimal RoundMoney(decimal value)
    {
        return Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }

    private static decimal RoundPercentage(decimal value)
    {
        return Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }
}

internal sealed record ConsignmentLifecycleSnapshot(
    decimal PrecoBase,
    decimal PrecoEfetivoVenda,
    decimal PercentualDescontoAplicado,
    decimal PercentualDescontoEsperado,
    bool DescontoAutomaticoAtivo,
    DateTimeOffset DataInicioConsignacao,
    DateTimeOffset DataFimConsignacao,
    int DiasEmLoja,
    int? DiasRestantes,
    bool ProximaDoFim,
    bool Vencida,
    string StatusConsignacao);
