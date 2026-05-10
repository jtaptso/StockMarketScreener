using FluentAssertions;
using StockScreener.Domain.Entities;
using StockScreener.Domain.Enums;
using StockScreener.Infrastructure.Persistence.Repositories;

namespace StockScreener.Tests.Infrastructure;

public class StockRepositoryTests
{
    // ── helpers ──────────────────────────────────────────────────────────────

    private static Stock MakeStock(int id, string symbol, Exchange exchange = Exchange.NASDAQ, string sector = "Technology")
        => new()
        {
            Id = id,
            Symbol = symbol,
            CompanyName = $"{symbol} Inc.",
            Exchange = exchange,
            Sector = sector,
            CurrentPrice = 100m,
            LastUpdated = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

    // ── AddAsync / GetByIdAsync ───────────────────────────────────────────────

    [Fact]
    public async Task AddAsync_ThenGetByIdAsync_ReturnsPersistedStock()
    {
        await using var db = InMemoryDbContextFactory.Create();
        var repo = new StockRepository(db);
        var stock = MakeStock(1, "AAPL");

        await repo.AddAsync(stock);
        var result = await repo.GetByIdAsync(1);

        result.Should().NotBeNull();
        result!.Symbol.Should().Be("AAPL");
        result.CompanyName.Should().Be("AAPL Inc.");
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        await using var db = InMemoryDbContextFactory.Create();
        var repo = new StockRepository(db);

        var result = await repo.GetByIdAsync(999);

        result.Should().BeNull();
    }

    // ── GetBySymbolAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetBySymbolAsync_ExactMatch_ReturnsStock()
    {
        await using var db = InMemoryDbContextFactory.Create();
        var repo = new StockRepository(db);
        await repo.AddAsync(MakeStock(1, "MSFT"));

        var result = await repo.GetBySymbolAsync("MSFT");

        result.Should().NotBeNull();
        result!.Symbol.Should().Be("MSFT");
    }

    [Fact]
    public async Task GetBySymbolAsync_LowercaseInput_ReturnsCaseInsensitiveMatch()
    {
        await using var db = InMemoryDbContextFactory.Create();
        var repo = new StockRepository(db);
        await repo.AddAsync(MakeStock(1, "GOOG"));

        var result = await repo.GetBySymbolAsync("goog");

        result.Should().NotBeNull();
        result!.Symbol.Should().Be("GOOG");
    }

    [Fact]
    public async Task GetBySymbolAsync_UnknownSymbol_ReturnsNull()
    {
        await using var db = InMemoryDbContextFactory.Create();
        var repo = new StockRepository(db);

        var result = await repo.GetBySymbolAsync("ZZZZ");

        result.Should().BeNull();
    }

    // ── GetAllAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_ReturnsAllPersistedStocks()
    {
        await using var db = InMemoryDbContextFactory.Create();
        var repo = new StockRepository(db);
        await repo.AddAsync(MakeStock(1, "AAPL"));
        await repo.AddAsync(MakeStock(2, "MSFT"));
        await repo.AddAsync(MakeStock(3, "GOOG"));

        var result = await repo.GetAllAsync();

        result.Should().HaveCount(3);
    }

    // ── GetByExchangeAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task GetByExchangeAsync_FiltersToCorrectExchange()
    {
        await using var db = InMemoryDbContextFactory.Create();
        var repo = new StockRepository(db);
        await repo.AddAsync(MakeStock(1, "AAPL", Exchange.NASDAQ));
        await repo.AddAsync(MakeStock(2, "IBM",  Exchange.NYSE));
        await repo.AddAsync(MakeStock(3, "MSFT", Exchange.NASDAQ));

        var result = await repo.GetByExchangeAsync(Exchange.NASDAQ);

        result.Should().HaveCount(2)
            .And.OnlyContain(s => s.Exchange == Exchange.NASDAQ);
    }

    // ── GetBySectorAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetBySectorAsync_FiltersToCorrectSector()
    {
        await using var db = InMemoryDbContextFactory.Create();
        var repo = new StockRepository(db);
        await repo.AddAsync(MakeStock(1, "AAPL", sector: "Technology"));
        await repo.AddAsync(MakeStock(2, "JPM",  sector: "Financials"));

        var result = await repo.GetBySectorAsync("Technology");

        result.Should().ContainSingle()
            .Which.Symbol.Should().Be("AAPL");
    }

    // ── ExistsAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task ExistsAsync_KnownSymbol_ReturnsTrue()
    {
        await using var db = InMemoryDbContextFactory.Create();
        var repo = new StockRepository(db);
        await repo.AddAsync(MakeStock(1, "TSLA"));

        var exists = await repo.ExistsAsync("TSLA");

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_UnknownSymbol_ReturnsFalse()
    {
        await using var db = InMemoryDbContextFactory.Create();
        var repo = new StockRepository(db);

        var exists = await repo.ExistsAsync("ZZZZ");

        exists.Should().BeFalse();
    }

    // ── UpdateAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_ChangedPrice_IsReflectedOnSubsequentGet()
    {
        await using var db = InMemoryDbContextFactory.Create();
        var repo = new StockRepository(db);
        var stock = MakeStock(1, "NVDA");
        await repo.AddAsync(stock);

        // Modify tracked entity and save via UpdateAsync
        stock.CurrentPrice = 999m;
        await repo.UpdateAsync(stock);

        var updated = await repo.GetByIdAsync(1);
        updated!.CurrentPrice.Should().Be(999m);
    }

    // ── DeleteAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_ExistingId_StockIsRemoved()
    {
        await using var db = InMemoryDbContextFactory.Create();
        var repo = new StockRepository(db);
        await repo.AddAsync(MakeStock(1, "META"));

        await repo.DeleteAsync(1);

        var result = await repo.GetByIdAsync(1);
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_NonExistentId_DoesNotThrow()
    {
        await using var db = InMemoryDbContextFactory.Create();
        var repo = new StockRepository(db);

        Func<Task> act = () => repo.DeleteAsync(999);

        await act.Should().NotThrowAsync();
    }
}
