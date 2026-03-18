namespace Renova.Services.Features.Credits;

// Centraliza os valores fixos e opcoes exibidas pelo modulo 10.
public static class CreditValues
{
    public static class AccountStatuses
    {
        public const string Ativa = "ativa";
        public const string Bloqueada = "bloqueada";

        public static readonly IReadOnlySet<string> Todos =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                Ativa,
                Bloqueada,
            };
    }

    public static class MovementTypes
    {
        public const string CreditoManual = "credito_manual";
        public const string CreditoRepasse = "credito_repasse";
        public const string DebitoVenda = "debito_venda";
        public const string EstornoCreditoVenda = "estorno_credito_venda";

        public static readonly IReadOnlyList<string> Todos =
        [
            CreditoManual,
            CreditoRepasse,
            DebitoVenda,
            EstornoCreditoVenda,
        ];
    }

    public static class Origins
    {
        public const string AjusteManual = "ajuste_manual";
        public const string RepasseFornecedor = "repasse_fornecedor";
        public const string Venda = "venda";
    }

    /// <summary>
    /// Normaliza e valida o status cadastral da conta de credito.
    /// </summary>
    public static string NormalizeAccountStatus(string value)
    {
        var normalized = value.Trim().ToLowerInvariant();
        if (!AccountStatuses.Todos.Contains(normalized))
        {
            throw new InvalidOperationException("Status da conta de credito invalido.");
        }

        return normalized;
    }

    /// <summary>
    /// Informa se a conta pode receber novos lancamentos de credito.
    /// </summary>
    public static bool CanReceiveCredits(string accountStatus)
    {
        return NormalizeAccountStatus(accountStatus) == AccountStatuses.Ativa;
    }

    /// <summary>
    /// Traduz o tipo do movimento em direcao de entrada ou saida.
    /// </summary>
    public static string ResolveDirection(string movementType)
    {
        var normalized = movementType.Trim().ToLowerInvariant();
        return normalized switch
        {
            MovementTypes.CreditoManual => "entrada",
            MovementTypes.CreditoRepasse => "entrada",
            MovementTypes.EstornoCreditoVenda => "entrada",
            MovementTypes.DebitoVenda => "saida",
            _ => "entrada",
        };
    }

    /// <summary>
    /// Monta as opcoes de status usadas pela API e pela interface.
    /// </summary>
    public static IReadOnlyList<(string Codigo, string Nome)> BuildAccountStatusOptions()
    {
        return
        [
            (AccountStatuses.Ativa, "Ativa"),
            (AccountStatuses.Bloqueada, "Bloqueada"),
        ];
    }

    /// <summary>
    /// Monta as opcoes de tipos de movimento usadas nos filtros do modulo.
    /// </summary>
    public static IReadOnlyList<(string Codigo, string Nome)> BuildMovementTypeOptions()
    {
        return
        [
            (MovementTypes.CreditoManual, "Credito manual"),
            (MovementTypes.CreditoRepasse, "Credito por repasse"),
            (MovementTypes.DebitoVenda, "Debito em venda"),
            (MovementTypes.EstornoCreditoVenda, "Estorno de credito"),
        ];
    }
}
