namespace Renova.Services.Features.Reports;

// Centraliza tipos de relatorio, exportacao e opcoes fixas do modulo 15.
public static class ReportValues
{
    public static class ReportTypes
    {
        public const string EstoqueAtual = "estoque_atual";
        public const string PecasVendidas = "pecas_vendidas";
        public const string Financeiro = "financeiro";
        public const string BaixasEstoque = "baixas_estoque";

        public static readonly IReadOnlyList<string> All =
        [
            EstoqueAtual,
            PecasVendidas,
            Financeiro,
            BaixasEstoque,
        ];
    }

    public static class ExportFormats
    {
        public const string Pdf = "pdf";
        public const string Excel = "excel";

        public static readonly IReadOnlyList<string> All =
        [
            Pdf,
            Excel,
        ];
    }

    /// <summary>
    /// Normaliza e valida o tipo de relatorio solicitado.
    /// </summary>
    public static string NormalizeReportType(string value)
    {
        var normalized = value.Trim().ToLowerInvariant();
        if (!ReportTypes.All.Contains(normalized))
        {
            throw new InvalidOperationException("Tipo de relatorio invalido.");
        }

        return normalized;
    }

    /// <summary>
    /// Normaliza e valida o formato de exportacao solicitado.
    /// </summary>
    public static string NormalizeExportFormat(string value)
    {
        var normalized = value.Trim().ToLowerInvariant();
        if (!ExportFormats.All.Contains(normalized))
        {
            throw new InvalidOperationException("Formato de exportacao invalido.");
        }

        return normalized;
    }
}
