namespace Renova.Services.Features.CommercialRules;

// Centraliza os valores fixos usados pelo modulo 05.
public static class CommercialRuleValues
{
    public static class PaymentMethodTypes
    {
        public const string Dinheiro = "dinheiro";
        public const string Pix = "pix";
        public const string CartaoCredito = "cartao_credito";
        public const string CartaoDebito = "cartao_debito";
        public const string Outro = "outro";

        public static readonly IReadOnlyList<string> Todos =
        [
            Dinheiro,
            Pix,
            CartaoCredito,
            CartaoDebito,
            Outro,
        ];
    }

    /// <summary>
    /// Normaliza e valida o tipo do meio de pagamento.
    /// </summary>
    public static string NormalizePaymentMethodType(string value)
    {
        var normalized = value.Trim().ToLowerInvariant();
        if (!PaymentMethodTypes.Todos.Contains(normalized))
        {
            throw new InvalidOperationException("Tipo de meio de pagamento invalido.");
        }

        return normalized;
    }
}
