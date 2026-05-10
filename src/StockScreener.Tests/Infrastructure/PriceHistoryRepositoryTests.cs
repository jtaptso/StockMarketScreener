using FluentAssertions;
using StockScreener.Domain.Entities;
using StockScreener.Domain.Enums;
using StockScreener.Infrastructure.Persistence.Repositories;

namespace StockScreener.Tests.Infrastructure;

public class PriceHistoryRepositoryTests
{
    // ── helpers ──────────────────────────────────────────────────────────────

    private static Stock MakeStock(int id, string symbol = "AAPL")
        => new()
        {
            Id = id,
            Symbol = symbol,
            CompanyName = $"{symbol} Inc.",
            Exchange = Exchange.NASDAQ,
            LastUpdated = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

    private static PriceHistory MakeRow(long id, int stockId, DateOnly date, decimal close = 150m)
        => new()
        {
            Id = id,
            StockId = stockId,
            TradeDate = date,
            OpenPrice = close - 2m,
            HighPrice = close + 2m,
            LowPrice = close - 3m,
            ClosePrice = close,
            Volume = 1_000_000
        };

    // ── AddAsync / GetByIdAsync ───────────────────────────────────────────────

    [Fact]
    public async Task AddAsync_ThenGetByIdAsync_ReturnsPersisted()
    {
        await using var db = InMemoryDbContextFactory.Create();
        var stockRepo = new StockRepository(db);
        var repo = new PriceHistoryRepository(db);

        await stockRepo.AddAsync(MakeStock(1));
        var row = MakeRow(1, 1, new DateOnly(2025, 1, 2));
        await repo.AddAsync(row);

        var result = await repo.GetByIdAsync(1);
        result.Should().NotBeNull();
        result!.ClosePrice.Should().Be(150m);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        await using var db = InMemoryDbContextFactory.Create();
        var repo = new PriceHistoryRepository(db);

        var result = await repo.GetByIdAsync(99);

        result.Should().BeNull();
    }

    // ── GetByStockIdAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetByStockIdAsync_ReturnsRowsOrderedDescendingByDate()
    {
        await using var db = InMemoryDbContextFactory.Create();
        var stockRepo = new StockRepository(db);
        var repo = new PriceHistoryRepository(db);

        await stockRepo.AddAsync(MakeStock(1));
        await repo.AddRangeAsync([
            MakeRow(1, 1, new DateOnly(2025, 1, 1)),
            MakeRow(2, 1, new DateOnly(2025, 1, 3)),
            MakeRow(3, 1, new DateOnly(2025, 1, 2))
        ]);

        var results = (await repo.GetByStockIdAsync(1)).ToList();

        results.Should().HaveCount(3);
        results[0].TradeDate.Should().Be(new DateOnly(2025, 1, 3));
        results[1].TradeDate.Should().Be(new DateOnly(2025, 1, 2));
        results[2].TradeDate.Should().Be(new DateOnly(2025, 1, 1));
    }

    // ── GetByStockIdAndDateRangeAsync ─────────────────────────────────────────

    [Fact]
    public async Task GetByStockIdAndDateRangeAsync_ReturnsOnlyRowsInRange_Ascending()
    {
        await using var db = InMemoryDbContextFactory.Create();
        var stockRepo = new StockRepository(db);
        var repo = new PriceHistoryRepository(db);

        await stockRepo.AddAsync(MakeStock(1));
        await repo.AddRangeAsync([
            MakeRow(1, 1, new DateOnly(2025, 1, 1)),
            MakeRow(2, 1, new DateOnly(2025, 1, 5)),
            MakeRow(3, 1, new DateOnly(2025, 1, 10))
        ]);

        var results = (await repo.GetByStockIdAndDateRangeAsync(
            1,
            new DateOnly(2025, 1, 2),
            new DateOnly(2025, 1, 9))).ToList();

        results.Should().ContainSingle()
            .Which.TradeDate.Should().Be(new DateOnly(2025, 1, 5));
    }

    [Fact]
    public async Task GetByStockIdAndDateRangeAsync_IncludesBoundaryDates()
    {
        await using var db = InMemoryDbContextFactory.Create();
        var stockRepo = new StockRepository(db);
        var repo = new PriceHistoryRepository(db);

        await stockRepo.AddAsync(MakeStock(1));
        await repo.AddRangeAsync([
            MakeRow(1, 1, new DateOnly(2025, 1, 1)),
            MakeRow(2, 1, new DateOnly(2025, 1, 5)),
            MakeRow(3, 1, new DateOnly(2025, 1, 10))
        ]);

        var results = (await repo.GetByStockIdAndDateRangeAsync(
            1,
            new DateOnly(2025, 1, 1),
            new DateOnly(2025, 1, 10))).ToList();

        results.Should().HaveCount(3);
        results[0].TradeDate.Should().Be(new DateOnly(2025, 1, 1)); // ascending
    }

    // ── GetLatestByStockIdAsync ───────────────────────────────────────────────

    [Fact]
    public async Task GetLatestByStockIdAsync_ReturnsMostRecentRow()
    {
        await using var db = InMemoryDbContextFactory.Create();
        var stockRepo = new StockRepository(db);
        var repo = new PriceHistoryRepository(db);

        await stockRepo.AddAsync(MakeStock(1));
        await repo.AddRangeAsync([
            MakeRow(1, 1, new DateOnly(2025, 1, 1), 100m),
            MakeRow(2, 1, new DateOnly(2025, 1, 5), 150m),
            MakeRow(3, 1, new DateOnly(2025, 1, 3), 120m)
        ]);

        var latest = await repo.GetLatestByStockIdAsync(1);

        latest.Should().NotBeNull();
        latest!.TradeDate.Should().Be(new DateOnly(2025, 1, 5));
        latest.ClosePrice.Should().Be(150m);
    }

    [Fact]
    public async Task GetLatestByStockIdAsync_NoRows_ReturnsNull()
    {
        await using var db = InMemoryDbContextFactory.Create();
        var repo = new PriceHistoryRepository(db);

        var result = await repo.GetLatestByStockIdAsync(999);

        result.Should().BeNull();
    }

    // ── AddRangeAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task AddRangeAsync_PersistsAllRows()
    {
        await using var db = InMemoryDbContextFactory.Create();
        var stockRepo = new StockRepository(db);
        var repo = new PriceHistoryRepository(db);

        await stockRepo.AddAsync(MakeStock(1));
        await repo.AddRangeAsync([
            MakeRow(1, 1, new DateOnly(2025, 1, 1)),
            MakeRow(2, 1, new DateOnly(2025, 1, 2)),
            MakeRow(3, 1, new DateOnly(2025, 1, 3))
        ]);

        var results = await repo.GetByStockIdAsync(1);

        results.Should().HaveCount(3);
    }

    // ── DeleteByStockIdAndDateAsync ───────────────────────────────────────────

    [Fact]
    public async Task DeleteByStockIdAndDateAsync_ExistingRow_IsRemoved()
    {
        await using var db = InMemoryDbContextFactory.Create();
        var stockRepo = new StockRepository(db);
        var repo = new PriceHistoryRepository(db);

        await stockRepo.AddAsync(MakeStock(1));
        await repo.AddAsync(MakeRow(1, 1, new DateOnly(2025, 1, 1)));

        await repo.DeleteByStockIdAndDateAsync(1, new DateOnly(2025, 1, 1));

        var remaining = await repo.GetByStockIdAsync(1);
        remaining.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteByStockIdAndDateAsync_NonExistentRow_DoesNotThrow()
    {
        await using var db = InMemoryDbContextFactory.Create();
        var repo = new PriceHistoryRepository(db);

        Func<Task> act = () => repo.DeleteByStockIdAndDateAsync(1, new DateOnly(2025, 1, 1));

        await act.Should().NotThrowAsync();
    }
}
