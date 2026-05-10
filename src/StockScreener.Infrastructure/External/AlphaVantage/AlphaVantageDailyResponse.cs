using System.Text.Json.Serialization;

namespace StockScreener.Infrastructure.External.AlphaVantage;

/// <summary>Maps to AlphaVantage TIME_SERIES_DAILY function response.</summary>
public sealed class AlphaVantageDailyResponse
{
    [JsonPropertyName("Time Series (Daily)")]
    public Dictionary<string, AlphaVantageDailyBar>? TimeSeries { get; init; }
}

public sealed class AlphaVantageDailyBar
{
    [JsonPropertyName("1. open")]
    public string Open { get; init; } = string.Empty;

    [JsonPropertyName("2. high")]
    public string High { get; init; } = string.Empty;

    [JsonPropertyName("3. low")]
    public string Low { get; init; } = string.Empty;

    [JsonPropertyName("4. close")]
    public string Close { get; init; } = string.Empty;

    [JsonPropertyName("5. volume")]
    public string Volume { get; init; } = string.Empty;
}
