using System.Text.Json.Serialization;

namespace StockScreener.Infrastructure.External.Finnhub;

/// <summary>Maps to Finnhub GET /stock/candle response.</summary>
public sealed class FinnhubCandleResponse
{
    /// <summary>Status: "ok" or "no_data".</summary>
    [JsonPropertyName("s")]
    public string S { get; init; } = string.Empty;

    /// <summary>Close prices.</summary>
    [JsonPropertyName("c")]
    public IReadOnlyList<decimal> C { get; init; } = [];

    /// <summary>High prices.</summary>
    [JsonPropertyName("h")]
    public IReadOnlyList<decimal> H { get; init; } = [];

    /// <summary>Low prices.</summary>
    [JsonPropertyName("l")]
    public IReadOnlyList<decimal> L { get; init; } = [];

    /// <summary>Open prices.</summary>
    [JsonPropertyName("o")]
    public IReadOnlyList<decimal> O { get; init; } = [];

    /// <summary>Unix timestamps.</summary>
    [JsonPropertyName("t")]
    public IReadOnlyList<long> T { get; init; } = [];

    /// <summary>Volumes.</summary>
    [JsonPropertyName("v")]
    public IReadOnlyList<long> V { get; init; } = [];
}
