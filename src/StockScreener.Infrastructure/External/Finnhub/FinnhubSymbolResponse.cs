using System.Text.Json.Serialization;

namespace StockScreener.Infrastructure.External.Finnhub;

/// <summary>Maps to a single item from Finnhub GET /stock/symbol response.</summary>
public sealed class FinnhubSymbolResponse
{
    [JsonPropertyName("symbol")]
    public string Symbol { get; init; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; init; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; init; } = string.Empty;
}
