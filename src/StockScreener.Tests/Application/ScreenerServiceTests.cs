using FluentAssertions;
using Moq;
using StockScreener.Application.DTOs;
using StockScreener.Application.Services;
using StockScreener.Domain.Entities;
using StockScreener.Domain.Enums;
using StockScreener.Domain.Interfaces.Repositories;
using StockScreener.Domain.Interfaces.Services;
using StockScreener.Domain.ValueObjects;

namespace StockScreener.Tests.Application;

public class ScreenerServiceTests
{
    private readonly Mock<IStockRepository> _stockRepo = new();
    private readonly Mock<IScreenerEngine> _screenerEngine = new();
    private readonly Mock<IFilterPresetRepository> _presetRepo = new();
    private readonly ScreenerService _sut;

    public ScreenerServiceTests()
    {
        _sut = new ScreenerService(_stockRepo.Object, _screenerEngine.Object, _presetRepo.Object);
    }

    // ── FilterAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task FilterAsync_ReturnsPagedResult_WithCorrectTotalCount()
    {
        var stocks = BuildStocks(5);
        _stockRepo.Setup(r => r.GetAllAsync(default)).ReturnsAsync(stocks);
        _screenerEngine.Setup(e => e.Filter(stocks, It.IsAny<ScreenerFilter>())).Returns(stocks);

        var filter = new ScreenerFilterDto { Page = 1, PageSize = 3 };
        var result = await _sut.FilterAsync(filter);

        result.PageInfo.TotalCount.Should().Be(5);
        result.PageInfo.TotalPages.Should().Be(2);
        result.Items.Should().HaveCount(3);
    }

    [Fact]
    public async Task FilterAsync_SecondPage_ReturnsRemainingItems()
    {
        var stocks = BuildStocks(5);
        _stockRepo.Setup(r => r.GetAllAsync(default)).ReturnsAsync(stocks);
        _screenerEngine.Setup(e => e.Filter(stocks, It.IsAny<ScreenerFilter>())).Returns(stocks);

        var filter = new ScreenerFilterDto { Page = 2, PageSize = 3 };
        var result = await _sut.FilterAsync(filter);

        result.Items.Should().HaveCount(2);
        result.PageInfo.Page.Should().Be(2);
    }

    [Fact]
    public async Task FilterAsync_EmptyStocks_ReturnsEmptyResult()
    {
        _stockRepo.Setup(r => r.GetAllAsync(default)).ReturnsAsync([]);
        _screenerEngine.Setup(e => e.Filter(It.IsAny<IEnumerable<Stock>>(), It.IsAny<ScreenerFilter>()))
            .Returns([]);

        var result = await _sut.FilterAsync(new ScreenerFilterDto());

        result.Items.Should().BeEmpty();
        result.PageInfo.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task FilterAsync_MapsStockFieldsCorrectly()
    {
        var stock = BuildStock(id: 42, symbol: "AAPL");
        _stockRepo.Setup(r => r.GetAllAsync(default)).ReturnsAsync([stock]);
        _screenerEngine.Setup(e => e.Filter(It.IsAny<IEnumerable<Stock>>(), It.IsAny<ScreenerFilter>()))
            .Returns([stock]);

        var result = await _sut.FilterAsync(new ScreenerFilterDto { PageSize = 50 });

        var item = result.Items.Single();
        item.Id.Should().Be(42);
        item.Symbol.Should().Be("AAPL");
    }

    // ── GetPresetsAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetPresetsAsync_ReturnsMappedPresets()
    {
        var presets = new[]
        {
            new FilterPreset { Id = Guid.NewGuid(), Name = "Tech", FilterJson = "{}", UserId = "u1" },
            new FilterPreset { Id = Guid.NewGuid(), Name = "Value", FilterJson = "{}", UserId = "u1" }
        };
        _presetRepo.Setup(r => r.GetByUserIdAsync("u1", default)).ReturnsAsync(presets);

        var result = await _sut.GetPresetsAsync("u1");

        result.Should().HaveCount(2);
        result.Select(p => p.Name).Should().BeEquivalentTo(["Tech", "Value"]);
    }

    // ── SavePresetAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task SavePresetAsync_AddsPresetAndReturnsDto()
    {
        FilterPreset? saved = null;
        _presetRepo.Setup(r => r.AddAsync(It.IsAny<FilterPreset>(), default))
            .Callback<FilterPreset, CancellationToken>((p, _) => saved = p);

        var filter = new ScreenerFilterDto { MinPrice = 10m, MaxPrice = 500m };
        var result = await _sut.SavePresetAsync("u1", "My Preset", "desc", filter);

        saved.Should().NotBeNull();
        saved!.Name.Should().Be("My Preset");
        saved.UserId.Should().Be("u1");
        result.Name.Should().Be("My Preset");
        result.Filter.MinPrice.Should().Be(10m);
    }

    // ── DeletePresetAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task DeletePresetAsync_CallsRepositoryDelete()
    {
        var id = Guid.NewGuid();
        await _sut.DeletePresetAsync(id);
        _presetRepo.Verify(r => r.DeleteAsync(id, default), Times.Once);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static List<Stock> BuildStocks(int count) =>
        Enumerable.Range(1, count).Select(i => BuildStock(i, $"SYM{i}")).ToList();

    private static Stock BuildStock(int id, string symbol) => new()
    {
        Id = id,
        Symbol = symbol,
        CompanyName = $"Company {symbol}",
        Exchange = Exchange.NASDAQ,
        LastUpdated = DateTime.UtcNow
    };
}
