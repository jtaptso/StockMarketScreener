using Microsoft.EntityFrameworkCore;
using StockScreener.Infrastructure.Persistence;

namespace StockScreener.Tests.Infrastructure;

/// <summary>
/// Creates a fresh <see cref="AppDbContext"/> backed by an isolated InMemory database.
/// Pass a unique name (or omit for auto-generated) so each test gets its own store.
/// </summary>
internal static class InMemoryDbContextFactory
{
    internal static AppDbContext Create(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
