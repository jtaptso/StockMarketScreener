using StockScreener.Domain.Enums;

namespace StockScreener.Domain.Entities;

public class Stock
{
    public int Id { get; init; }
    public string Symbol { get; init; } = string.Empty;
    public string CompanyName { get; init; } = string.Empty;
    public Exchange Exchange { get; init; }
    public string? Sector { get; init; }
    public string? Industry { get; init; }
    public decimal? MarketCap { get; init; }
    public decimal? CurrentPrice { get; set; }
    public decimal? DayHigh { get; set; }
    public decimal? DayLow { get; set; }
    public decimal? Week52High { get; set; }
    public decimal? Week52Low { get; set; }
    public long? Volume { get; set; }
    public long? AvgVolume { get; set; }
    public decimal? Beta { get; set; }
    public DateTime LastUpdated { get; set; }
    public DateTime CreatedAt { get; init; }

    // Navigation properties
    public Fundamentals? Fundamentals { get; init; }
    public ICollection<PriceHistory> PriceHistory { get; init; } = new List<PriceHistory>();
    public ICollection<TechnicalIndicators> TechnicalIndicators { get; init; } = new List<TechnicalIndicators>();

    // Computed properties
    public MarketCapCategory MarketCapCategory => MarketCap switch
    {
        < 300_000_000 => MarketCapCategory.Micro,
        < 2_000_000_000 => MarketCapCategory.Small,
        < 10_000_000_000 => MarketCapCategory.Mid,
        < 200_000_000_000 => MarketCapCategory.Large,
        _ => MarketCapCategory.Mega
    };
}