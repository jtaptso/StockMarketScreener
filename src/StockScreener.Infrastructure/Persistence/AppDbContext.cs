using Microsoft.EntityFrameworkCore;
using StockScreener.Domain.Entities;

namespace StockScreener.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Stock> Stocks => Set<Stock>();
    public DbSet<PriceHistory> PriceHistory => Set<PriceHistory>();
    public DbSet<Fundamentals> Fundamentals => Set<Fundamentals>();
    public DbSet<TechnicalIndicators> TechnicalIndicators => Set<TechnicalIndicators>();
    public DbSet<Watchlist> Watchlists => Set<Watchlist>();
    public DbSet<WatchlistItem> WatchlistItems => Set<WatchlistItem>();
    public DbSet<FilterPreset> FilterPresets => Set<FilterPreset>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
