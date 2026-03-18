namespace Renova.Services.Features.Pieces;

// Centraliza os valores fixos usados pelo modulo 06.
public static class PieceValues
{
    public static class PieceTypes
    {
        public const string Consignada = "consignada";
        public const string Fixa = "fixa";
        public const string Lote = "lote";

        public static readonly IReadOnlyList<string> Todos =
        [
            Consignada,
            Fixa,
            Lote,
        ];
    }

    public static class PieceStatuses
    {
        public const string Disponivel = "disponivel";
        public const string Reservada = "reservada";
        public const string Vendida = "vendida";
        public const string Devolvida = "devolvida";
        public const string Doada = "doada";
        public const string Perdida = "perdida";
        public const string Descartada = "descartada";
        public const string Inativa = "inativa";

        public static readonly IReadOnlyList<string> Todos =
        [
            Disponivel,
            Reservada,
            Vendida,
            Devolvida,
            Doada,
            Perdida,
            Descartada,
            Inativa,
        ];
    }

    public static class ImageVisibility
    {
        public const string Interna = "interna";
        public const string Externa = "externa";

        public static readonly IReadOnlyList<string> Todos =
        [
            Interna,
            Externa,
        ];
    }

    /// <summary>
    /// Normaliza e valida o status operacional da peca.
    /// </summary>
    public static string NormalizePieceStatus(string value)
    {
        var normalized = value.Trim().ToLowerInvariant();
        if (!PieceStatuses.Todos.Contains(normalized))
        {
            throw new InvalidOperationException("Status de peca invalido.");
        }

        return normalized;
    }

    public static class StockMovementTypes
    {
        public const string Entrada = "entrada";
        public const string Venda = "venda";
        public const string Devolucao = "devolucao";
        public const string Doacao = "doacao";
        public const string Perda = "perda";
        public const string Descarte = "descarte";
        public const string Ajuste = "ajuste";
        public const string CancelamentoVenda = "cancelamento_venda";
    }

    public static class StockOrigins
    {
        public const string Peca = "peca";
        public const string Venda = "venda";
        public const string Consignacao = "consignacao";
        public const string AjusteManual = "ajuste_manual";
    }

    /// <summary>
    /// Normaliza e valida o tipo da peca.
    /// </summary>
    public static string NormalizePieceType(string value)
    {
        var normalized = value.Trim().ToLowerInvariant();
        if (!PieceTypes.Todos.Contains(normalized))
        {
            throw new InvalidOperationException("Tipo de peca invalido.");
        }

        return normalized;
    }

    /// <summary>
    /// Normaliza e valida a visibilidade da imagem.
    /// </summary>
    public static string NormalizeImageVisibility(string value)
    {
        var normalized = value.Trim().ToLowerInvariant();
        if (!ImageVisibility.Todos.Contains(normalized))
        {
            throw new InvalidOperationException("Tipo de visibilidade da imagem invalido.");
        }

        return normalized;
    }
}
