using System.Text.Json;
using Renova.Services.Features.CommercialRules.Contracts;

namespace Renova.Services.Features.CommercialRules;

// Converte a politica de desconto entre estrutura tipada e JSON persistido.
internal static class CommercialRulePolicySerializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Serializa as faixas de desconto para a coluna JSON.
    /// </summary>
    public static string Serialize(IReadOnlyList<CommercialDiscountBandResponse> bands)
    {
        return JsonSerializer.Serialize(bands, SerializerOptions);
    }

    /// <summary>
    /// Converte o JSON persistido para a lista de faixas do dominio.
    /// </summary>
    public static IReadOnlyList<CommercialDiscountBandResponse> Deserialize(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Array.Empty<CommercialDiscountBandResponse>();
        }

        var bands = JsonSerializer.Deserialize<List<CommercialDiscountBandResponse>>(json, SerializerOptions);
        return bands is null ? Array.Empty<CommercialDiscountBandResponse>() : bands;
    }
}
