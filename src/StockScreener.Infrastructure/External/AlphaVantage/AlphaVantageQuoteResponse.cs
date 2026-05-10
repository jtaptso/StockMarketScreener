using System.Text.Json.Serialization;

namespace StockScreener.Infrastructure.External.AlphaVantage;

/// <summary>Maps to AlphaVantage GLOBAL_QUOTE function response.</summary>
public sealed class AlphaVantageQuoteResponse
{
    [JsonPropertyName("Global Quote")]
    public AlphaVantageGlobalQuote? GlobalQuote { get; init; }
}

public sealed class AlphaVantageGlobalQuote
{
    [JsonPropertyName("01. symbol")]
    public string Symbol { get; init; } = string.Empty;

    [JsonPropertyName("02. open")]
    public string Open { get; init; } = string.Empty;

    [JsonPropertyName("03. high")]
    public string High { get; init; } = string.Empty;

    [JsonPropertyName("04. low")]
    public string Low { get; init; } = string.Empty;

    [JsonPropertyName("05. price")]
    public string Price { get; init; } = string.Empty;

    [JsonPropertyName("06. volume")]
    public string Volume { get; init; } = string.Empty;

    [JsonPropertyName("07. latest trading day")]
    public string LatestTradingDay { get; init; } = string.Empty;

    [JsonPropertyName("08. previous close")]
    public string PreviousClose { get; init; } = string.Empty;

    [JsonPropertyName("09. change")]
    public string Change { get; init; } = string.Empty;

    [JsonPropertyName("10. change percent")]
    public string ChangePercent { get; init; } = string.Empty;
}
