namespace StockScreener.Domain.Entities;

public class PriceHistory
{
    public long Id { get; init; }
    public int StockId { get; init; }
    public DateOnly TradeDate { get; init; }
    public decimal OpenPrice { get; init; }
    public decimal HighPrice { get; init; }
    public decimal LowPrice { get; init; }
    public decimal ClosePrice { get; init; }
    public long Volume { get; init; }

    // Navigation property
    public Stock Stock { get; init; } = null!;

    // Computed
    public decimal PriceChange => ClosePrice - OpenPrice;
    public decimal PriceChangePercent => OpenPrice != 0
        ? (PriceChange / OpenPrice) * 100
        : 0;
    public decimal Range => HighPrice - LowPrice;
}
