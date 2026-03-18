using Renova.Services.Features.Pieces;

namespace Renova.Services.Features.Dashboards;

// Centraliza os valores fixos usados pelo modulo 14.
public static class DashboardValues
{
    public const int NearDueWindowDays = 7;
    public const int StaleStockThresholdDays = 30;

    public static class InconsistencyTypes
    {
        public const string PecaVendidaComSaldo = "peca_vendida_com_saldo";
        public const string ObrigacaoAcimaOriginal = "obrigacao_acima_original";
        public const string ContaCreditoNegativa = "conta_credito_negativa";
    }

    /// <summary>
    /// Normaliza o filtro opcional por tipo de peca.
    /// </summary>
    public static string? NormalizePieceTypeFilter(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return PieceValues.NormalizePieceType(value);
    }

    /// <summary>
    /// Traduz os tipos de peca em opcoes amigaveis para a interface.
    /// </summary>
    public static IReadOnlyList<(string Codigo, string Nome)> BuildPieceTypeOptions()
    {
        return
        [
            (PieceValues.PieceTypes.Consignada, "Consignada"),
            (PieceValues.PieceTypes.Fixa, "Fixa"),
            (PieceValues.PieceTypes.Lote, "Lote"),
        ];
    }
}
