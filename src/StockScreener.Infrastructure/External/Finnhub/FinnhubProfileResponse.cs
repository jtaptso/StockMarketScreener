using System.Text.Json.Serialization;

namespace StockScreener.Infrastructure.External.Finnhub;

/// <summary>Maps to the Finnhub GET /stock/profile2 response (subset of fields used).</summary>
public sealed class FinnhubProfileResponse
{
    [JsonPropertyName("ticker")]
    public string Ticker { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("exchange")]
    public string Exchange { get; init; } = string.Empty;

    [JsonPropertyName("finnhubIndustry")]
    public string FinnhubIndustry { get; init; } = string.Empty;

    [JsonPropertyName("marketCapitalization")]
    public decimal MarketCapitalization { get; init; }
}
