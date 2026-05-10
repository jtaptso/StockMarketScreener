namespace StockScreener.Domain.Entities;

public class TechnicalIndicators
{
    public long Id { get; init; }
    public int StockId { get; init; }
    public DateOnly TradeDate { get; init; }

    // Momentum
    public decimal? RSI_14 { get; init; }

    // Moving Averages
    public decimal? SMA_20 { get; init; }
    public decimal? SMA_50 { get; init; }
    public decimal? SMA_200 { get; init; }

    // MACD
    public decimal? MACD { get; init; }
    public decimal? MACD_Signal { get; init; }
    public decimal? MACD_Histogram { get; init; }

    // Volatility
    public decimal? ATR_14 { get; init; }
    public decimal? BB_Upper { get; init; }
    public decimal? BB_Middle { get; init; }
    public decimal? BB_Lower { get; init; }

    // Volume
    public long? Volume { get; init; }
    public long? AvgVolume_20 { get; init; }

    public DateTime LastUpdated { get; init; }

    // Navigation property
    public Stock Stock { get; init; } = null!;
}
