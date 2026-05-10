using FluentAssertions;
using Moq;
using StockScreener.Application.Services;
using StockScreener.Domain.Entities;
using StockScreener.Domain.Enums;
using StockScreener.Domain.Interfaces.Repositories;

namespace StockScreener.Tests.Application;

public class WatchlistServiceTests
{
    private readonly Mock<IWatchlistRepository> _watchlistRepo = new();
    private readonly Mock<IStockRepository> _stockRepo = new();
    private readonly WatchlistService _sut;

    public WatchlistServiceTests()
    {
        _sut = new WatchlistService(_watchlistRepo.Object, _stockRepo.Object);
    }

    // ── GetByUserIdAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetByUserIdAsync_ReturnsMappedWatchlists()
    {
        var watchlists = new[]
        {
            BuildWatchlist("Portfolio A"),
            BuildWatchlist("Portfolio B")
        };
        _watchlistRepo.Setup(r => r.GetByUserIdAsync("user1", default)).ReturnsAsync(watchlists);

        var result = (await _sut.GetByUserIdAsync("user1")).ToList();

        result.Should().HaveCount(2);
        result.Select(w => w.Name).Should().BeEquivalentTo(["Portfolio A", "Portfolio B"]);
    }

    // ── GetByIdAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_KnownId_ReturnsMappedDto()
    {
        var watchlist = BuildWatchlist("My Watchlist");
        _watchlistRepo.Setup(r => r.GetByIdAsync(watchlist.Id, default)).ReturnsAsync(watchlist);

        var result = await _sut.GetByIdAsync(watchlist.Id);

        result.Should().NotBeNull();
        result!.Name.Should().Be("My Watchlist");
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        _watchlistRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Watchlist?)null);

        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    // ── CreateAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_AddsWatchlistAndReturnsMappedDto()
    {
        Watchlist? saved = null;
        _watchlistRepo.Setup(r => r.AddAsync(It.IsAny<Watchlist>(), default))
            .Callback<Watchlist, CancellationToken>((w, _) => saved = w);

        var result = await _sut.CreateAsync("user1", "New List", "desc");

        saved.Should().NotBeNull();
        saved!.UserId.Should().Be("user1");
        saved.Name.Should().Be("New List");
        result.Name.Should().Be("New List");
        result.UserId.Should().Be("user1");
    }

    // ── UpdateAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_UpdatesNameAndDescription()
    {
        var watchlist = BuildWatchlist("Old Name");
        _watchlistRepo.Setup(r => r.GetByIdAsync(watchlist.Id, default)).ReturnsAsync(watchlist);

        await _sut.UpdateAsync(watchlist.Id, "New Name", "New Desc");

        watchlist.Name.Should().Be("New Name");
        watchlist.Description.Should().Be("New Desc");
        _watchlistRepo.Verify(r => r.UpdateAsync(watchlist, default), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_UnknownId_ThrowsInvalidOperationException()
    {
        _watchlistRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Watchlist?)null);

        var act = () => _sut.UpdateAsync(Guid.NewGuid(), "Name", null);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    // ── DeleteAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_CallsRepositoryDelete()
    {
        var id = Guid.NewGuid();
        await _sut.DeleteAsync(id);
        _watchlistRepo.Verify(r => r.DeleteAsync(id, default), Times.Once);
    }

    // ── AddStockAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task AddStockAsync_NewItem_AddsToWatchlist()
    {
        var watchlistId = Guid.NewGuid();
        var stock = BuildStock(99, "AAPL");
        _stockRepo.Setup(r => r.GetBySymbolAsync("AAPL", default)).ReturnsAsync(stock);
        _watchlistRepo.Setup(r => r.ItemExistsAsync(watchlistId, 99, default)).ReturnsAsync(false);

        await _sut.AddStockAsync(watchlistId, "AAPL", sharesOwned: 5m, costBasis: 150m);

        _watchlistRepo.Verify(r => r.AddItemAsync(
            It.Is<WatchlistItem>(i => i.StockId == 99 && i.SharesOwned == 5m && i.CostBasis == 150m),
            default), Times.Once);
    }

    [Fact]
    public async Task AddStockAsync_DuplicateItem_DoesNotAddAgain()
    {
        var watchlistId = Guid.NewGuid();
        var stock = BuildStock(99, "AAPL");
        _stockRepo.Setup(r => r.GetBySymbolAsync("AAPL", default)).ReturnsAsync(stock);
        _watchlistRepo.Setup(r => r.ItemExistsAsync(watchlistId, 99, default)).ReturnsAsync(true);

        await _sut.AddStockAsync(watchlistId, "AAPL");

        _watchlistRepo.Verify(r => r.AddItemAsync(It.IsAny<WatchlistItem>(), default), Times.Never);
    }

    [Fact]
    public async Task AddStockAsync_UnknownSymbol_ThrowsInvalidOperationException()
    {
        _stockRepo.Setup(r => r.GetBySymbolAsync("FAKE", default)).ReturnsAsync((Stock?)null);

        var act = () => _sut.AddStockAsync(Guid.NewGuid(), "FAKE");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*FAKE*");
    }

    // ── RemoveStockAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task RemoveStockAsync_KnownSymbol_CallsRepositoryRemove()
    {
        var watchlistId = Guid.NewGuid();
        var stock = BuildStock(99, "GOOG");
        _stockRepo.Setup(r => r.GetBySymbolAsync("GOOG", default)).ReturnsAsync(stock);

        await _sut.RemoveStockAsync(watchlistId, "GOOG");

        _watchlistRepo.Verify(r => r.RemoveItemAsync(watchlistId, 99, default), Times.Once);
    }

    [Fact]
    public async Task RemoveStockAsync_UnknownSymbol_ThrowsInvalidOperationException()
    {
        _stockRepo.Setup(r => r.GetBySymbolAsync("ZZZ", default)).ReturnsAsync((Stock?)null);

        var act = () => _sut.RemoveStockAsync(Guid.NewGuid(), "ZZZ");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*ZZZ*");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static Watchlist BuildWatchlist(string name) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        UserId = "user1",
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    private static Stock BuildStock(int id, string symbol) => new()
    {
        Id = id,
        Symbol = symbol,
        CompanyName = $"Company {symbol}",
        Exchange = Exchange.NYSE,
        LastUpdated = DateTime.UtcNow
    };
}
