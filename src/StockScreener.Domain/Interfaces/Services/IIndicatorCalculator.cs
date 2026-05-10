namespace StockScreener.Domain.Interfaces.Services;

public interface IIndicatorCalculator
{
    /// <summary>Relative Strength Index over the given period (default 14).</summary>
    decimal? CalculateRSI(IReadOnlyList<decimal> closePrices, int period = 14);

    /// <summary>Simple Moving Average over the given period.</summary>
    decimal? CalculateSMA(IReadOnlyList<decimal> closePrices, int period);

    /// <summary>
    /// MACD line, signal line, and histogram.
    /// Returns null when there is insufficient data.
    /// </summary>
    (decimal? macd, decimal? signal, decimal? histogram) CalculateMACD(
        IReadOnlyList<decimal> closePrices,
        int fastPeriod = 12,
        int slowPeriod = 26,
        int signalPeriod = 9);

    /// <summary>Average True Range over the given period (default 14).</summary>
    decimal? CalculateATR(
        IReadOnlyList<decimal> highPrices,
        IReadOnlyList<decimal> lowPrices,
        IReadOnlyList<decimal> closePrices,
        int period = 14);

    /// <summary>Bollinger Bands (upper, middle/SMA, lower) over the given period.</summary>
    (decimal? upper, decimal? middle, decimal? lower) CalculateBollingerBands(
        IReadOnlyList<decimal> closePrices,
        int period = 20,
        decimal standardDeviations = 2m);
}
