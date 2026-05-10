using FluentAssertions;
using StockScreener.Domain.Services;

namespace StockScreener.Tests.Domain;

public class IndicatorCalculatorTests
{
    private readonly IndicatorCalculator _sut = new();

    // ── RSI ──────────────────────────────────────────────────────────────────

    [Fact]
    public void CalculateRSI_ReturnsNull_WhenInsufficientData()
    {
        // Exactly `period` prices → only period-1 changes → null
        var prices = Enumerable.Range(1, 14).Select(i => (decimal)i).ToList();
        _sut.CalculateRSI(prices, period: 14).Should().BeNull();
    }

    [Fact]
    public void CalculateRSI_Returns100_WhenAllPricesIncreasing()
    {
        // 16 prices, 15 changes, all gains → RSI = 100
        var prices = Enumerable.Range(1, 16).Select(i => (decimal)i).ToList();
        var rsi = _sut.CalculateRSI(prices, period: 14);
        rsi.Should().Be(100m);
    }

    [Fact]
    public void CalculateRSI_Returns0_WhenAllPricesDecreasing()
    {
        // 16 prices, all losses → RSI = 0
        var prices = Enumerable.Range(1, 16).Select(i => 17m - i).ToList(); // 16,15,...,1
        var rsi = _sut.CalculateRSI(prices, period: 14);
        rsi.Should().Be(0m);
    }

    [Fact]
    public void CalculateRSI_Returns50_WhenEqualGainsAndLosses()
    {
        // 15 alternating prices → 14 changes: 7 gains of 1, 7 losses of 1
        // AvgGain = AvgLoss = 0.5 → RSI = 50
        var prices = Enumerable.Range(0, 15).Select(i => i % 2 == 0 ? 10m : 11m).ToList();
        var rsi = _sut.CalculateRSI(prices, period: 14);
        rsi.Should().BeApproximately(50m, precision: 0.01m);
    }

    // ── SMA ──────────────────────────────────────────────────────────────────

    [Fact]
    public void CalculateSMA_ReturnsNull_WhenInsufficientData()
    {
        var prices = new List<decimal> { 1, 2 };
        _sut.CalculateSMA(prices, period: 3).Should().BeNull();
    }

    [Fact]
    public void CalculateSMA_ReturnsCorrectAverage()
    {
        var prices = new List<decimal> { 1, 2, 3, 4, 5 };
        // SMA(3) uses last 3 prices: (3+4+5)/3 = 4
        _sut.CalculateSMA(prices, period: 3).Should().Be(4m);
    }

    [Fact]
    public void CalculateSMA_UsesOnlyLastNPrices()
    {
        var prices = new List<decimal> { 100, 1, 2, 3 };
        // SMA(3) = (1+2+3)/3 = 2 — the leading 100 is excluded
        _sut.CalculateSMA(prices, period: 3).Should().Be(2m);
    }

    // ── MACD ─────────────────────────────────────────────────────────────────

    [Fact]
    public void CalculateMACD_ReturnsNulls_WhenInsufficientData()
    {
        // Need at least slowPeriod + signalPeriod - 1 = 34 prices
        var prices = Enumerable.Range(1, 33).Select(i => (decimal)i).ToList();
        var (macd, signal, histogram) = _sut.CalculateMACD(prices);
        macd.Should().BeNull();
        signal.Should().BeNull();
        histogram.Should().BeNull();
    }

    [Fact]
    public void CalculateMACD_ReturnsValues_WithSufficientData()
    {
        var prices = Enumerable.Range(1, 40).Select(i => (decimal)i).ToList();
        var (macd, signal, histogram) = _sut.CalculateMACD(prices);
        macd.Should().NotBeNull();
        signal.Should().NotBeNull();
        histogram.Should().NotBeNull();
    }

    [Fact]
    public void CalculateMACD_HistogramEqualsMACD_MinusSignal()
    {
        var prices = Enumerable.Range(1, 40).Select(i => (decimal)i).ToList();
        var (macd, signal, histogram) = _sut.CalculateMACD(prices);
        histogram.Should().BeApproximately(macd!.Value - signal!.Value, precision: 0.0001m);
    }

    // ── ATR ──────────────────────────────────────────────────────────────────

    [Fact]
    public void CalculateATR_ReturnsNull_WhenInsufficientData()
    {
        // Need period + 1 bars; period = 14 → need 15
        var data = Enumerable.Range(1, 14).Select(i => (decimal)i).ToList();
        _sut.CalculateATR(data, data, data, period: 14).Should().BeNull();
    }

    [Fact]
    public void CalculateATR_ReturnsNull_WhenArrayLengthsMismatch()
    {
        var high = new List<decimal> { 10, 11, 12, 13, 14, 15 };
        var low  = new List<decimal> { 8,  9,  10, 11, 12 };  // one shorter
        var close = high;
        _sut.CalculateATR(high, low, close, period: 4).Should().BeNull();
    }

    [Fact]
    public void CalculateATR_ReturnsConstantRange_WhenCandlesAreUniform()
    {
        // High always 2 above Low, no gap between bars → TR always = 2
        int n = 16;
        var high  = Enumerable.Repeat(12m, n).ToList();
        var low   = Enumerable.Repeat(10m, n).ToList();
        var close = Enumerable.Repeat(11m, n).ToList();
        var atr = _sut.CalculateATR(high, low, close, period: 14);
        atr.Should().NotBeNull();
        atr!.Value.Should().BeApproximately(2m, precision: 0.0001m);
    }

    // ── Bollinger Bands ───────────────────────────────────────────────────────

    [Fact]
    public void CalculateBollingerBands_ReturnsNulls_WhenInsufficientData()
    {
        var prices = new List<decimal> { 1, 2, 3 };
        var (upper, middle, lower) = _sut.CalculateBollingerBands(prices, period: 5);
        upper.Should().BeNull();
        middle.Should().BeNull();
        lower.Should().BeNull();
    }

    [Fact]
    public void CalculateBollingerBands_UpperEqualsLower_WhenAllPricesIdentical()
    {
        var prices = Enumerable.Repeat(10m, 20).ToList();
        var (upper, middle, lower) = _sut.CalculateBollingerBands(prices, period: 20);
        upper.Should().Be(10m);
        middle.Should().Be(10m);
        lower.Should().Be(10m);
    }

    [Fact]
    public void CalculateBollingerBands_UpperAboveMiddleAboveLower_WithVariation()
    {
        var prices = new List<decimal> { 8, 10, 12, 10, 10 };
        var (upper, middle, lower) = _sut.CalculateBollingerBands(prices, period: 5);
        upper.Should().NotBeNull();
        middle.Should().NotBeNull();
        lower.Should().NotBeNull();
        upper!.Value.Should().BeGreaterThan(middle!.Value);
        middle.Value.Should().BeGreaterThan(lower!.Value);
        middle.Value.Should().Be(10m); // SMA = 10
    }
}
