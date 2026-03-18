namespace Renova.Services.Features.Closings;

// Centraliza os valores fixos do modulo 13 para status, movimentos e exportacoes.
public static class ClosingValues
{
    public static class Statuses
    {
        public const string Aberto = "aberto";
        public const string Conferido = "conferido";
        public const string Liquidado = "liquidado";

        public static readonly IReadOnlyList<string> Todos =
        [
            Aberto,
            Conferido,
            Liquidado,
        ];
    }

    public static class MovementTypes
    {
        public const string Venda = "venda";
        public const string Pagamento = "pagamento";
        public const string Credito = "credito";
        public const string CompraLoja = "compra_loja";
        public const string Ajuste = "ajuste";

        public static readonly IReadOnlyList<string> Todos =
        [
            Venda,
            Pagamento,
            Credito,
            CompraLoja,
            Ajuste,
        ];
    }

    public static class ItemGroups
    {
        public const string Atual = "atual";
        public const string Vendida = "vendida";
    }

    public static class ExportTypes
    {
        public const string Pdf = "pdf";
        public const string Excel = "excel";
    }

    /// <summary>
    /// Normaliza e valida o status operacional do fechamento.
    /// </summary>
    public static string NormalizeStatus(string value)
    {
        var normalized = value.Trim().ToLowerInvariant();
        if (!Statuses.Todos.Contains(normalized))
        {
            throw new InvalidOperationException("Status de fechamento invalido.");
        }

        return normalized;
    }

    /// <summary>
    /// Traduz a colecao de status em opcoes amigaveis para filtros.
    /// </summary>
    public static IReadOnlyList<(string Codigo, string Nome)> BuildStatusOptions()
    {
        return
        [
            (Statuses.Aberto, "Aberto"),
            (Statuses.Conferido, "Conferido"),
            (Statuses.Liquidado, "Liquidado"),
        ];
    }

    /// <summary>
    /// Traduz os tipos de movimento em rotulos para a interface.
    /// </summary>
    public static IReadOnlyList<(string Codigo, string Nome)> BuildMovementTypeOptions()
    {
        return
        [
            (MovementTypes.Venda, "Venda"),
            (MovementTypes.Pagamento, "Pagamento"),
            (MovementTypes.Credito, "Credito"),
            (MovementTypes.CompraLoja, "Compra na loja"),
            (MovementTypes.Ajuste, "Ajuste"),
        ];
    }
}
