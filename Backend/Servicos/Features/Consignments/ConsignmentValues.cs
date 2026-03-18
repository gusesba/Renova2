using Renova.Services.Features.Pieces;

namespace Renova.Services.Features.Consignments;

// Centraliza valores fixos usados pelo modulo 07.
public static class ConsignmentValues
{
    public const int NearExpirationAlertThresholdDays = 7;

    public static class LifecycleStatuses
    {
        public const string Ativa = "ativa";
        public const string Proxima = "proxima";
        public const string Vencida = "vencida";
        public const string Encerrada = "encerrada";

        public static readonly IReadOnlyList<string> Todos =
        [
            Ativa,
            Proxima,
            Vencida,
            Encerrada,
        ];
    }

    public static class CloseActions
    {
        public const string Devolver = "devolver";
        public const string Doar = "doar";
        public const string Perder = "perder";
        public const string Descartar = "descartar";

        public static readonly IReadOnlyList<string> Todos =
        [
            Devolver,
            Doar,
            Perder,
            Descartar,
        ];
    }

    public static class AlertTypes
    {
        public const string ConsignacaoProxima = "consignacao_proxima";
    }

    public static class AlertStatuses
    {
        public const string Aberto = "aberto";
        public const string Resolvido = "resolvido";
    }

    public static class AlertSeverities
    {
        public const string Media = "media";
        public const string Alta = "alta";
    }

    /// <summary>
    /// Normaliza a acao de encerramento informada pela API.
    /// </summary>
    public static string NormalizeCloseAction(string value)
    {
        var normalized = value.Trim().ToLowerInvariant();
        if (!CloseActions.Todos.Contains(normalized))
        {
            throw new InvalidOperationException("Acao de encerramento da consignacao invalida.");
        }

        return normalized;
    }

    /// <summary>
    /// Informa se o status da peca ainda permite operacao de consignacao.
    /// </summary>
    public static bool IsActivePieceStatus(string value)
    {
        return value == PieceValues.PieceStatuses.Disponivel ||
               value == PieceValues.PieceStatuses.Reservada;
    }
}
