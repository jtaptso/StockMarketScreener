using StockScreener.Domain.Interfaces.Services;

namespace StockScreener.Domain.Services;

/// <summary>
/// Pure-math indicator calculations. No I/O, no external dependencies.
/// All methods return null when insufficient data is supplied.
/// </summary>
public class IndicatorCalculator : IIndicatorCalculator
{
    // ── RSI ─────────────────────────────────────────────────────────────────

    public decimal? CalculateRSI(IReadOnlyList<decimal> closePrices, int period = 14)
    {
        // Need at least period + 1 prices to produce period changes
        if (closePrices.Count <= period)
            return null;

        // Build price-change array
        int len = closePrices.Count - 1;
        decimal[] changes = new decimal[len];
        for (int i = 0; i < len; i++)
            changes[i] = closePrices[i + 1] - closePrices[i];

        // Seed: simple average of first `period` changes
        decimal avgGain = 0m, avgLoss = 0m;
        for (int i = 0; i < period; i++)
        {
            if (changes[i] > 0) avgGain += changes[i];
            else avgLoss += Math.Abs(changes[i]);
        }
        avgGain /= period;
        avgLoss /= period;

        // Wilder's smoothing for the rest
        for (int i = period; i < len; i++)
        {
            decimal gain = changes[i] > 0 ? changes[i] : 0m;
            decimal loss = changes[i] < 0 ? Math.Abs(changes[i]) : 0m;
            avgGain = (avgGain * (period - 1) + gain) / period;
            avgLoss = (avgLoss * (period - 1) + loss) / period;
        }

        if (avgLoss == 0m) return 100m;
        decimal rs = avgGain / avgLoss;
        return Math.Round(100m - (100m / (1m + rs)), 4);
    }

    // ── SMA ─────────────────────────────────────────────────────────────────

    public decimal? CalculateSMA(IReadOnlyList<decimal> closePrices, int period)
    {
        if (closePrices.Count < period)
            return null;

        decimal sum = 0m;
        int start = closePrices.Count - period;
        for (int i = start; i < closePrices.Count; i++)
            sum += closePrices[i];

        return sum / period;
    }

    // ── MACD ────────────────────────────────────────────────────────────────

    public (decimal? macd, decimal? signal, decimal? histogram) CalculateMACD(
        IReadOnlyList<decimal> closePrices,
        int fastPeriod = 12,
        int slowPeriod = 26,
        int signalPeriod = 9)
    {
        // Minimum data = slowPeriod + signalPeriod - 1
        if (closePrices.Count < slowPeriod + signalPeriod - 1)
            return (null, null, null);

        var fastEma = EmaSequence(closePrices, fastPeriod);
        var slowEma = EmaSequence(closePrices, slowPeriod);

        if (fastEma is null || slowEma is null)
            return (null, null, null);

        // Align: fastEma is longer; offset = slowPeriod - fastPeriod
        int offset = slowPeriod - fastPeriod;
        int macdLength = slowEma.Count;
        decimal[] macdLine = new decimal[macdLength];
        for (int i = 0; i < macdLength; i++)
            macdLine[i] = fastEma[i + offset] - slowEma[i];

        var signalEma = EmaSequence(macdLine, signalPeriod);
        if (signalEma is null || signalEma.Count == 0)
            return (null, null, null);

        decimal macdVal = macdLine[^1];
        decimal signalVal = signalEma[^1];
        return (Math.Round(macdVal, 6),
                Math.Round(signalVal, 6),
                Math.Round(macdVal - signalVal, 6));
    }

    // ── ATR ─────────────────────────────────────────────────────────────────

    public decimal? CalculateATR(
        IReadOnlyList<decimal> highPrices,
        IReadOnlyList<decimal> lowPrices,
        IReadOnlyList<decimal> closePrices,
        int period = 14)
    {
        if (highPrices.Count != lowPrices.Count || highPrices.Count != closePrices.Count)
            return null;

        // Need period + 1 bars so we have period True Ranges (each TR needs a prior close)
        if (highPrices.Count <= period)
            return null;

        int trCount = highPrices.Count - 1;
        decimal[] tr = new decimal[trCount];
        for (int i = 1; i < highPrices.Count; i++)
        {
            decimal hl = highPrices[i] - lowPrices[i];
            decimal hc = Math.Abs(highPrices[i] - closePrices[i - 1]);
            decimal lc = Math.Abs(lowPrices[i] - closePrices[i - 1]);
            tr[i - 1] = Math.Max(hl, Math.Max(hc, lc));
        }

        if (trCount < period)
            return null;

        // Seed with SMA of first `period` true ranges
        decimal atr = 0m;
        for (int i = 0; i < period; i++)
            atr += tr[i];
        atr /= period;

        // Wilder's smoothing for the rest
        for (int i = period; i < trCount; i++)
            atr = (atr * (period - 1) + tr[i]) / period;

        return Math.Round(atr, 6);
    }

    // ── Bollinger Bands ──────────────────────────────────────────────────────

    public (decimal? upper, decimal? middle, decimal? lower) CalculateBollingerBands(
        IReadOnlyList<decimal> closePrices,
        int period = 20,
        decimal standardDeviations = 2m)
    {
        if (closePrices.Count < period)
            return (null, null, null);

        decimal middle = CalculateSMA(closePrices, period)!.Value;

        // Population standard deviation of last `period` prices
        int start = closePrices.Count - period;
        decimal sumSqDiff = 0m;
        for (int i = start; i < closePrices.Count; i++)
        {
            decimal diff = closePrices[i] - middle;
            sumSqDiff += diff * diff;
        }
        decimal stdDev = (decimal)Math.Sqrt((double)(sumSqDiff / period));

        return (Math.Round(middle + standardDeviations * stdDev, 6),
                Math.Round(middle, 6),
                Math.Round(middle - standardDeviations * stdDev, 6));
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns an EMA value sequence seeded by the SMA of the first <paramref name="period"/> prices.
    /// Length of result = prices.Count - period + 1.
    /// Returns null when there is insufficient data.
    /// </summary>
    private static IReadOnlyList<decimal>? EmaSequence(IReadOnlyList<decimal> prices, int period)
    {
        if (prices.Count < period)
            return null;

        decimal multiplier = 2m / (period + 1);
        var ema = new List<decimal>(prices.Count - period + 1);

        // Seed with SMA of first `period` values
        decimal seed = 0m;
        for (int i = 0; i < period; i++)
            seed += prices[i];
        ema.Add(seed / period);

        for (int i = period; i < prices.Count; i++)
            ema.Add(prices[i] * multiplier + ema[^1] * (1m - multiplier));

        return ema;
    }
}
