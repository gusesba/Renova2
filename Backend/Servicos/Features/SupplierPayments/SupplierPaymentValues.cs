using Renova.Services.Features.CommercialRules;

namespace Renova.Services.Features.SupplierPayments;

// Centraliza os valores fixos do modulo 11 para obrigacoes e liquidacoes.
public static class SupplierPaymentValues
{
    public static class ObligationTypes
    {
        public const string RepasseVendaConsignada = "repasse_venda_consignada";
        public const string CompraPecaFixa = "compra_peca_fixa";
        public const string CompraPecaLote = "compra_peca_lote";

        public static readonly IReadOnlyList<string> Todos =
        [
            RepasseVendaConsignada,
            CompraPecaFixa,
            CompraPecaLote,
        ];
    }

    public static class ObligationStatuses
    {
        public const string Pendente = "pendente";
        public const string Parcial = "parcial";
        public const string Paga = "paga";
        public const string Cancelada = "cancelada";
        public const string Ajustada = "ajustada";

        public static readonly IReadOnlyList<string> Todos =
        [
            Pendente,
            Parcial,
            Paga,
            Cancelada,
            Ajustada,
        ];
    }

    public static class LiquidationTypes
    {
        public const string MeioPagamento = "meio_pagamento";
        public const string CreditoLoja = "credito_loja";

        public static readonly IReadOnlyList<string> Todos =
        [
            MeioPagamento,
            CreditoLoja,
        ];
    }

    public static class FinancialMovementTypes
    {
        public const string PagamentoFornecedor = "pagamento_fornecedor";
    }

    /// <summary>
    /// Normaliza e valida o tipo da obrigacao.
    /// </summary>
    public static string NormalizeObligationType(string value)
    {
        var normalized = value.Trim().ToLowerInvariant();
        if (!ObligationTypes.Todos.Contains(normalized))
        {
            throw new InvalidOperationException("Tipo de obrigacao invalido.");
        }

        return normalized;
    }

    /// <summary>
    /// Normaliza o status armazenado, incluindo legado com valor aberta.
    /// </summary>
    public static string NormalizeStoredObligationStatus(string value)
    {
        var normalized = value.Trim().ToLowerInvariant();
        if (normalized == "aberta")
        {
            return ObligationStatuses.Pendente;
        }

        if (!ObligationStatuses.Todos.Contains(normalized))
        {
            throw new InvalidOperationException("Status de obrigacao invalido.");
        }

        return normalized;
    }

    /// <summary>
    /// Normaliza e valida o tipo da liquidacao.
    /// </summary>
    public static string NormalizeLiquidationType(string value)
    {
        var normalized = value.Trim().ToLowerInvariant();
        if (!LiquidationTypes.Todos.Contains(normalized))
        {
            throw new InvalidOperationException("Tipo de liquidacao invalido.");
        }

        return normalized;
    }

    /// <summary>
    /// Constroi as opcoes de tipo de obrigacao para filtros da API.
    /// </summary>
    public static IReadOnlyList<(string Codigo, string Nome)> BuildObligationTypeOptions()
    {
        return
        [
            (ObligationTypes.RepasseVendaConsignada, "Repasse de venda consignada"),
            (ObligationTypes.CompraPecaFixa, "Compra de peca fixa"),
            (ObligationTypes.CompraPecaLote, "Compra de peca em lote"),
        ];
    }

    /// <summary>
    /// Constroi as opcoes de status para filtros da API.
    /// </summary>
    public static IReadOnlyList<(string Codigo, string Nome)> BuildObligationStatusOptions()
    {
        return
        [
            (ObligationStatuses.Pendente, "Pendente"),
            (ObligationStatuses.Parcial, "Parcial"),
            (ObligationStatuses.Paga, "Paga"),
            (ObligationStatuses.Cancelada, "Cancelada"),
            (ObligationStatuses.Ajustada, "Ajustada"),
        ];
    }

    /// <summary>
    /// Constroi as opcoes de tipo de liquidacao para o formulario.
    /// </summary>
    public static IReadOnlyList<(string Codigo, string Nome)> BuildLiquidationTypeOptions()
    {
        return
        [
            (LiquidationTypes.MeioPagamento, "Meio de pagamento"),
            (LiquidationTypes.CreditoLoja, "Credito da loja"),
        ];
    }

    /// <summary>
    /// Traduz o tipo do meio de pagamento em um rotulo amigavel.
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
