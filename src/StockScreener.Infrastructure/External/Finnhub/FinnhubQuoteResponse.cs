using System.Text.Json.Serialization;

namespace StockScreener.Infrastructure.External.Finnhub;

/// <summary>Maps to Finnhub GET /quote response.</summary>
public sealed class FinnhubQuoteResponse
{
    /// <summary>Current price.</summary>
    [JsonPropertyName("c")]
    public decimal C { get; init; }

    /// <summary>Change.</summary>
    [JsonPropertyName("d")]
    public decimal? D { get; init; }

    /// <summary>Percent change.</summary>
    [JsonPropertyName("dp")]
    public decimal? Dp { get; init; }

    /// <summary>High price of the day.</summary>
    [JsonPropertyName("h")]
    public decimal H { get; init; }

    /// <summary>Low price of the day.</summary>
    [JsonPropertyName("l")]
    public decimal L { get; init; }

    /// <summary>Open price of the day.</summary>
    [JsonPropertyName("o")]
    public decimal O { get; init; }

    /// <summary>Previous close price.</summary>
    [JsonPropertyName("pc")]
    public decimal Pc { get; init; }

    /// <summary>Unix timestamp of the quote.</summary>
    [JsonPropertyName("t")]
    public long T { get; init; }
}
