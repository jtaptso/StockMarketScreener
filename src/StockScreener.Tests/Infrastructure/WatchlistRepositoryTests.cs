using FluentAssertions;
using StockScreener.Domain.Entities;
using StockScreener.Domain.Enums;
using StockScreener.Infrastructure.Persistence.Repositories;

namespace StockScreener.Tests.Infrastructure;

public class WatchlistRepositoryTests
{
    // ── helpers ──────────────────────────────────────────────────────────────

    private static Stock MakeStock(int id, string symbol)
        => new()
        {
            Id = id,
            Symbol = symbol,
            CompanyName = $"{symbol} Inc.",
            Exchange = Exchange.NASDAQ,
            LastUpdated = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

    private static Watchlist MakeWatchlist(Guid? id = null, string userId = "user-1", string name = "My List")
        => new()
        {
            Id = id ?? Guid.NewGuid(),
            UserId = userId,
            Name = name,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

    // ── AddAsync / GetByIdAsync ───────────────────────────────────────────────

    [Fact]
    public async Task AddAsync_ThenGetByIdAsync_ReturnsPersisted()
    {
        await using var db = InMemoryDbContextFactory.Create();
        var repo = new WatchlistRepository(db);
        var wl = MakeWatchlist(name: "Tech Picks");

        await repo.AddAsync(wl);
        var result = await repo.GetByIdAsync(wl.Id);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Tech Picks");
        result.UserId.Should().Be("user-1");
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        await using var db = InMemoryDbContextFactory.Create();
        var repo = new WatchlistRepository(db);

        var result = await repo.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_EagerLoads_ItemsAndStocks()
    {
        await using var db = InMemoryDbContextFactory.Create();
        var stockRepo = new StockRepository(db);
        var repo = new WatchlistRepository(db);

        await stockRepo.AddAsync(MakeStock(1, "AAPL"));

        var wl = MakeWatchlist();
        await repo.AddAsync(wl);
        await repo.AddItemAsync(new WatchlistItem
        {
            Id = 1,
            WatchlistId = wl.Id,
            StockId = 1,
            AddedAt = DateTime.UtcNow
        });

        var result = await repo.GetByIdAsync(wl.Id);

        result!.Items.Should().ContainSingle();
        result.Items.First().Stock.Symbol.Should().Be("AAPL");
    }

    // ── GetByUserIdAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetByUserIdAsync_ReturnsOnlyWatchlistsBelongingToUser()
    {
        await using var db = InMemoryDbContextFactory.Create();
        var repo = new WatchlistRepository(db);

        await repo.AddAsync(MakeWatchlist(userId: "user-1", name: "List A"));
        await repo.AddAsync(MakeWatchlist(userId: "user-1", name: "List B"));
        await repo.AddAsync(MakeWatchlist(userId: "user-2", name: "Other"));

        var results = (await repo.GetByUserIdAsync("user-1")).ToList();

        results.Should().HaveCount(2)
            .And.OnlyContain(w => w.UserId == "user-1");
    }

    [Fact]
    public async Task GetByUserIdAsync_ReturnsWatchlistsOrderedByName()
    {
        await using var db = InMemoryDbContextFactory.Create();
        var repo = new WatchlistRepository(db);

        await repo.AddAsync(MakeWatchlist(userId: "user-1", name: "Zebra"));
        await repo.AddAsync(MakeWatchlist(userId: "user-1", name: "Alpha"));
        await repo.AddAsync(MakeWatchlist(userId: "user-1", name: "Mid"));

        var results = (await repo.GetByUserIdAsync("user-1")).ToList();

        results.Select(w => w.Name).Should().BeInAscendingOrder();
    }

    // ── UpdateAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_ChangedName_IsReflectedOnSubsequentGet()
    {
        await using var db = InMemoryDbContextFactory.Create();
        var repo = new WatchlistRepository(db);
        var wl = MakeWatchlist(name: "Old Name");
        await repo.AddAsync(wl);

        wl.Name = "New Name";
        await repo.UpdateAsync(wl);

        var result = await repo.GetByIdAsync(wl.Id);
        result!.Name.Should().Be("New Name");
    }

    // ── DeleteAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_ExistingWatchlist_IsRemoved()
    {
        await using var db = InMemoryDbContextFactory.Create();
        var repo = new WatchlistRepository(db);
        var wl = MakeWatchlist();
        await repo.AddAsync(wl);

        await repo.DeleteAsync(wl.Id);

        var result = await repo.GetByIdAsync(wl.Id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_NonExistentId_DoesNotThrow()
    {
        await using var db = InMemoryDbContextFactory.Create();
        var repo = new WatchlistRepository(db);

        Func<Task> act = () => repo.DeleteAsync(Guid.NewGuid());

        await act.Should().NotThrowAsync();
    }

    // ── AddItemAsync / ItemExistsAsync ────────────────────────────────────────

    [Fact]
    public async Task AddItemAsync_ThenItemExistsAsync_ReturnsTrue()
    {
        await using var db = InMemoryDbContextFactory.Create();
        var stockRepo = new StockRepository(db);
        var repo = new WatchlistRepository(db);

        await stockRepo.AddAsync(MakeStock(1, "NVDA"));
        var wl = MakeWatchlist();
        await repo.AddAsync(wl);

        await repo.AddItemAsync(new WatchlistItem
        {
            Id = 1,
            WatchlistId = wl.Id,
            StockId = 1,
            AddedAt = DateTime.UtcNow
        });

        var exists = await repo.ItemExistsAsync(wl.Id, 1);
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ItemExistsAsync_ItemNotAdded_ReturnsFalse()
    {
        await using var db = InMemoryDbContextFactory.Create();
        var repo = new WatchlistRepository(db);

        var exists = await repo.ItemExistsAsync(Guid.NewGuid(), 999);

        exists.Should().BeFalse();
    }

    // ── RemoveItemAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task RemoveItemAsync_ExistingItem_IsRemoved()
    {
        await using var db = InMemoryDbContextFactory.Create();
        var stockRepo = new StockRepository(db);
        var repo = new WatchlistRepository(db);

        await stockRepo.AddAsync(MakeStock(1, "TSLA"));
        var wl = MakeWatchlist();
        await repo.AddAsync(wl);
        await repo.AddItemAsync(new WatchlistItem
        {
            Id = 1,
            WatchlistId = wl.Id,
            StockId = 1,
            AddedAt = DateTime.UtcNow
        });

        await repo.RemoveItemAsync(wl.Id, 1);

        var exists = await repo.ItemExistsAsync(wl.Id, 1);
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task RemoveItemAsync_NonExistentItem_DoesNotThrow()
    {
        await using var db = InMemoryDbContextFactory.Create();
        var repo = new WatchlistRepository(db);

        Func<Task> act = () => repo.RemoveItemAsync(Guid.NewGuid(), 999);

        await act.Should().NotThrowAsync();
    }
}
