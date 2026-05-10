using FluentAssertions;
using Moq;
using StockScreener.Application.Services;
using StockScreener.Domain.Entities;
using StockScreener.Domain.Enums;
using StockScreener.Domain.Interfaces.Repositories;

namespace StockScreener.Tests.Application;

public class StockServiceTests
{
    private readonly Mock<IStockRepository> _stockRepo = new();
    private readonly Mock<IPriceHistoryRepository> _priceHistoryRepo = new();
    private readonly Mock<IFundamentalsRepository> _fundamentalsRepo = new();
    private readonly StockService _sut;

    public StockServiceTests()
    {
        _sut = new StockService(_stockRepo.Object, _priceHistoryRepo.Object, _fundamentalsRepo.Object);
    }

    // ── GetBySymbolAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetBySymbolAsync_KnownSymbol_ReturnsMappedDto()
    {
        var stock = BuildStock(1, "MSFT");
        _stockRepo.Setup(r => r.GetBySymbolAsync("MSFT", default)).ReturnsAsync(stock);

        var result = await _sut.GetBySymbolAsync("MSFT");

        result.Should().NotBeNull();
        result!.Symbol.Should().Be("MSFT");
        result.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetBySymbolAsync_UnknownSymbol_ReturnsNull()
    {
        _stockRepo.Setup(r => r.GetBySymbolAsync("XYZ", default)).ReturnsAsync((Stock?)null);

        var result = await _sut.GetBySymbolAsync("XYZ");

        result.Should().BeNull();
    }

    // ── GetPriceHistoryAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task GetPriceHistoryAsync_KnownSymbol_ReturnsMappedHistory()
    {
        var stock = BuildStock(10, "AAPL");
        var from = new DateOnly(2026, 1, 1);
        var to = new DateOnly(2026, 1, 31);
        var history = new[]
        {
            BuildPriceHistory(stockId: 10, tradeDate: from),
            BuildPriceHistory(stockId: 10, tradeDate: to)
        };

        _stockRepo.Setup(r => r.GetBySymbolAsync("AAPL", default)).ReturnsAsync(stock);
        _priceHistoryRepo.Setup(r => r.GetByStockIdAndDateRangeAsync(10, from, to, default))
            .ReturnsAsync(history);

        var result = (await _sut.GetPriceHistoryAsync("AAPL", from, to)).ToList();

        result.Should().HaveCount(2);
        result[0].TradeDate.Should().Be(from);
    }

    [Fact]
    public async Task GetPriceHistoryAsync_UnknownSymbol_ReturnsEmpty()
    {
        _stockRepo.Setup(r => r.GetBySymbolAsync("ZZZ", default)).ReturnsAsync((Stock?)null);

        var result = await _sut.GetPriceHistoryAsync("ZZZ", DateOnly.MinValue, DateOnly.MaxValue);

        result.Should().BeEmpty();
        _priceHistoryRepo.Verify(
            r => r.GetByStockIdAndDateRangeAsync(It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), default),
            Times.Never);
    }

    // ── GetFundamentalsAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task GetFundamentalsAsync_KnownSymbol_ReturnsMappedDto()
    {
        var stock = BuildStock(5, "GOOG");
        var fundamentals = new Fundamentals { Id = 1, StockId = 5, PE_Ratio = 25m };

        _stockRepo.Setup(r => r.GetBySymbolAsync("GOOG", default)).ReturnsAsync(stock);
        _fundamentalsRepo.Setup(r => r.GetByStockIdAsync(5, default)).ReturnsAsync(fundamentals);

        var result = await _sut.GetFundamentalsAsync("GOOG");

        result.Should().NotBeNull();
        result!.PE_Ratio.Should().Be(25m);
    }

    [Fact]
    public async Task GetFundamentalsAsync_UnknownSymbol_ReturnsNull()
    {
        _stockRepo.Setup(r => r.GetBySymbolAsync("XXX", default)).ReturnsAsync((Stock?)null);

        var result = await _sut.GetFundamentalsAsync("XXX");

        result.Should().BeNull();
        _fundamentalsRepo.Verify(r => r.GetByStockIdAsync(It.IsAny<int>(), default), Times.Never);
    }

    [Fact]
    public async Task GetFundamentalsAsync_NoFundamentalsOnRecord_ReturnsNull()
    {
        var stock = BuildStock(7, "AMZN");
        _stockRepo.Setup(r => r.GetBySymbolAsync("AMZN", default)).ReturnsAsync(stock);
        _fundamentalsRepo.Setup(r => r.GetByStockIdAsync(7, default)).ReturnsAsync((Fundamentals?)null);

        var result = await _sut.GetFundamentalsAsync("AMZN");

        result.Should().BeNull();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static Stock BuildStock(int id, string symbol) => new()
    {
        Id = id,
        Symbol = symbol,
        CompanyName = $"Company {symbol}",
        Exchange = Exchange.NYSE,
        LastUpdated = DateTime.UtcNow
    };

    private static PriceHistory BuildPriceHistory(int stockId, DateOnly tradeDate) => new()
    {
        Id = 1,
        StockId = stockId,
        TradeDate = tradeDate,
        OpenPrice = 100m,
        HighPrice = 110m,
        LowPrice = 95m,
        ClosePrice = 105m,
        Volume = 1_000_000
    };
}
