using FluentAssertions;
using Moq;
using StockScreener.Application.Services;
using StockScreener.Domain.Entities;
using StockScreener.Domain.Enums;
using StockScreener.Domain.Interfaces.Repositories;
using StockScreener.Domain.Interfaces.Services;

namespace StockScreener.Tests.Application;

public class MarketDataServiceTests
{
    private readonly Mock<IStockRepository> _stockRepo = new();
    private readonly Mock<IPriceHistoryRepository> _priceHistoryRepo = new();
    private readonly Mock<IFundamentalsRepository> _fundamentalsRepo = new();
    private readonly Mock<IMarketDataProvider> _provider = new();
    private readonly MarketDataService _sut;

    public MarketDataServiceTests()
    {
        _sut = new MarketDataService(
            _stockRepo.Object,
            _priceHistoryRepo.Object,
            _fundamentalsRepo.Object,
            _provider.Object);
    }

    // ── SyncAllAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task SyncAllAsync_CallsSyncSymbolAsync_ForEachAvailableSymbol()
    {
        _provider.Setup(p => p.GetAvailableSymbolsAsync(default))
            .ReturnsAsync(["AAPL", "MSFT"]);

        // SyncSymbolAsync returns early when quote is null
        _provider.Setup(p => p.GetQuoteAsync(It.IsAny<string>(), default))
            .ReturnsAsync((Stock?)null);

        await _sut.SyncAllAsync();

        _provider.Verify(p => p.GetQuoteAsync("AAPL", default), Times.Once);
        _provider.Verify(p => p.GetQuoteAsync("MSFT", default), Times.Once);
    }

    [Fact]
    public async Task SyncAllAsync_EmptySymbolList_DoesNothing()
    {
        _provider.Setup(p => p.GetAvailableSymbolsAsync(default)).ReturnsAsync([]);

        await _sut.SyncAllAsync();

        _provider.Verify(p => p.GetQuoteAsync(It.IsAny<string>(), default), Times.Never);
    }

    // ── SyncSymbolAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task SyncSymbolAsync_NullQuote_DoesNothing()
    {
        _provider.Setup(p => p.GetQuoteAsync("FAKE", default)).ReturnsAsync((Stock?)null);

        await _sut.SyncSymbolAsync("FAKE");

        _stockRepo.Verify(r => r.AddAsync(It.IsAny<Stock>(), default), Times.Never);
        _stockRepo.Verify(r => r.UpdateAsync(It.IsAny<Stock>(), default), Times.Never);
    }

    [Fact]
    public async Task SyncSymbolAsync_NewStock_CallsAddAsync()
    {
        var quote = BuildStock("TSLA");
        _provider.Setup(p => p.GetQuoteAsync("TSLA", default)).ReturnsAsync(quote);
        _stockRepo.Setup(r => r.GetBySymbolAsync("TSLA", default)).ReturnsAsync((Stock?)null);
        _provider.Setup(p => p.GetFundamentalsAsync("TSLA", default)).ReturnsAsync((Fundamentals?)null);
        _provider.Setup(p => p.GetCandlesAsync("TSLA", It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), default))
            .ReturnsAsync([]);

        await _sut.SyncSymbolAsync("TSLA");

        _stockRepo.Verify(r => r.AddAsync(quote, default), Times.Once);
        _stockRepo.Verify(r => r.UpdateAsync(It.IsAny<Stock>(), default), Times.Never);
    }

    [Fact]
    public async Task SyncSymbolAsync_ExistingStock_CallsUpdateAsync()
    {
        var quote = BuildStock("TSLA");
        var existing = BuildStock("TSLA");
        _provider.Setup(p => p.GetQuoteAsync("TSLA", default)).ReturnsAsync(quote);
        _stockRepo.Setup(r => r.GetBySymbolAsync("TSLA", default)).ReturnsAsync(existing);
        _provider.Setup(p => p.GetFundamentalsAsync("TSLA", default)).ReturnsAsync((Fundamentals?)null);
        _provider.Setup(p => p.GetCandlesAsync("TSLA", It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), default))
            .ReturnsAsync([]);

        await _sut.SyncSymbolAsync("TSLA");

        _stockRepo.Verify(r => r.UpdateAsync(quote, default), Times.Once);
        _stockRepo.Verify(r => r.AddAsync(It.IsAny<Stock>(), default), Times.Never);
    }

    [Fact]
    public async Task SyncSymbolAsync_WithFundamentals_AddsWhenNotExisting()
    {
        var quote = BuildStock("NVDA");
        var fundamentals = new Fundamentals { StockId = quote.Id, PE_Ratio = 50m };
        _provider.Setup(p => p.GetQuoteAsync("NVDA", default)).ReturnsAsync(quote);
        _stockRepo.Setup(r => r.GetBySymbolAsync("NVDA", default)).ReturnsAsync((Stock?)null);
        _provider.Setup(p => p.GetFundamentalsAsync("NVDA", default)).ReturnsAsync(fundamentals);
        _fundamentalsRepo.Setup(r => r.GetByStockIdAsync(It.IsAny<int>(), default))
            .ReturnsAsync((Fundamentals?)null);
        _provider.Setup(p => p.GetCandlesAsync("NVDA", It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), default))
            .ReturnsAsync([]);

        await _sut.SyncSymbolAsync("NVDA");

        _fundamentalsRepo.Verify(r => r.AddAsync(fundamentals, default), Times.Once);
    }

    [Fact]
    public async Task SyncSymbolAsync_WithCandles_CallsAddRangeAsync()
    {
        var quote = BuildStock("AMD");
        var candles = new[] { BuildPriceHistory() };
        _provider.Setup(p => p.GetQuoteAsync("AMD", default)).ReturnsAsync(quote);
        _stockRepo.Setup(r => r.GetBySymbolAsync("AMD", default)).ReturnsAsync((Stock?)null);
        _provider.Setup(p => p.GetFundamentalsAsync("AMD", default)).ReturnsAsync((Fundamentals?)null);
        _provider.Setup(p => p.GetCandlesAsync("AMD", It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), default))
            .ReturnsAsync(candles);

        await _sut.SyncSymbolAsync("AMD");

        _priceHistoryRepo.Verify(r => r.AddRangeAsync(candles, default), Times.Once);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static Stock BuildStock(string symbol) => new()
    {
        Symbol = symbol,
        CompanyName = $"Company {symbol}",
        Exchange = Exchange.NASDAQ,
        LastUpdated = DateTime.UtcNow
    };

    private static PriceHistory BuildPriceHistory() => new()
    {
        TradeDate = DateOnly.FromDateTime(DateTime.UtcNow),
        OpenPrice = 100m,
        HighPrice = 110m,
        LowPrice = 90m,
        ClosePrice = 105m,
        Volume = 500_000
    };
}
