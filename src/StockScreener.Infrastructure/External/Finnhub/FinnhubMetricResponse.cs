using System.Text.Json.Serialization;

namespace StockScreener.Infrastructure.External.Finnhub;

/// <summary>Maps to Finnhub GET /stock/metric response (subset of fields used).</summary>
public sealed class FinnhubMetricResponse
{
    [JsonPropertyName("metric")]
    public FinnhubMetric? Metric { get; init; }
}

public sealed class FinnhubMetric
{
    [JsonPropertyName("peBasicExclExtraTTM")]
    public decimal? PeRatio { get; init; }

    [JsonPropertyName("pbQuarterly")]
    public decimal? PbRatio { get; init; }

    [JsonPropertyName("psTTM")]
    public decimal? PsRatio { get; init; }

    [JsonPropertyName("epsBasicExclExtraItemsTTM")]
    public decimal? Eps { get; init; }

    [JsonPropertyName("dividendYieldIndicatedAnnual")]
    public decimal? DividendYield { get; init; }

    [JsonPropertyName("totalDebt/totalEquityQuarterly")]
    public decimal? DebtToEquity { get; init; }

    [JsonPropertyName("currentRatioQuarterly")]
    public decimal? CurrentRatio { get; init; }

    [JsonPropertyName("roeRfy")]
    public decimal? Roe { get; init; }

    [JsonPropertyName("netProfitMarginTTM")]
    public decimal? ProfitMargin { get; init; }

    [JsonPropertyName("operatingMarginTTM")]
    public decimal? OperatingMargin { get; init; }

    [JsonPropertyName("grossMarginTTM")]
    public decimal? GrossMargin { get; init; }

    [JsonPropertyName("revenueGrowthQuarterlyYoy")]
    public decimal? RevenueGrowth { get; init; }

    [JsonPropertyName("epsGrowthTTMYoy")]
    public decimal? EpsGrowth { get; init; }

    [JsonPropertyName("revenueTTM")]
    public decimal? Revenue { get; init; }

    [JsonPropertyName("netIncomeTTM")]
    public decimal? NetIncome { get; init; }

    [JsonPropertyName("beta")]
    public decimal? Beta { get; init; }
}
