using Renova.Services.Features.CommercialRules;

namespace Renova.Services.Features.Sales;

// Centraliza os valores fixos usados pelo modulo 09.
public static class SaleValues
{
    public static class SaleStatuses
    {
        public const string Concluida = "concluida";
        public const string Cancelada = "cancelada";
    }

    public static class PaymentTypes
    {
        public const string MeioPagamento = "meio_pagamento";
        public const string CreditoLoja = "credito_loja";

        public static readonly IReadOnlyList<string> Todos =
        [
            MeioPagamento,
            CreditoLoja,
        ];
    }

    public static class CreditMovementTypes
    {
        public const string DebitoVenda = "debito_venda";
        public const string EstornoCreditoVenda = "estorno_credito_venda";
    }

    public static class CreditOrigins
    {
        public const string Venda = "venda";
    }

    public static class SupplierObligationTypes
    {
        public const string RepasseVendaConsignada = "repasse_venda_consignada";
    }

    public static class SupplierObligationStatuses
    {
        public const string Aberta = "aberta";
        public const string Cancelada = "cancelada";
    }

    public static class FinancialDirections
    {
        public const string Entrada = "entrada";
        public const string Saida = "saida";
    }

    public static class FinancialMovementTypes
    {
        public const string Venda = "venda";
        public const string EstornoVenda = "estorno_venda";
    }

    /// <summary>
    /// Normaliza e valida o tipo de pagamento informado na venda.
    /// </summary>
    public static string NormalizePaymentType(string value)
    {
        var normalized = value.Trim().ToLowerInvariant();
        if (!PaymentTypes.Todos.Contains(normalized))
        {
            throw new InvalidOperationException("Tipo de pagamento invalido.");
        }

        return normalized;
    }

    /// <summary>
    /// Define se o tipo informado representa um pagamento financeiro.
    /// </summary>
    public static bool IsFinancialPayment(string paymentType)
    {
        return NormalizePaymentType(paymentType) == PaymentTypes.MeioPagamento;
    }

    /// <summary>
    /// Expone os tipos de pagamento suportados pelo modulo.
    /// </summary>
    public static IReadOnlyList<(string Codigo, string Nome)> BuildPaymentTypeOptions()
    {
        return
        [
            (PaymentTypes.MeioPagamento, "Meio de pagamento"),
            (PaymentTypes.CreditoLoja, "Credito da loja"),
        ];
    }

    /// <summary>
    /// Traduz o tipo do meio de pagamento para um rótulo amigável.
    /// </summary>
    public static string GetPaymentMethodTypeLabel(string type)
    {
        var normalized = CommercialRuleValues.NormalizePaymentMethodType(type);

        return normalized switch
        {
            CommercialRuleValues.PaymentMethodTypes.Dinheiro => "Dinheiro",
            CommercialRuleValues.PaymentMethodTypes.Pix => "PIX",
            CommercialRuleValues.PaymentMethodTypes.CartaoCredito => "Cartao de credito",
            CommercialRuleValues.PaymentMethodTypes.CartaoDebito => "Cartao de debito",
            _ => "Outro",
        };
    }
}
