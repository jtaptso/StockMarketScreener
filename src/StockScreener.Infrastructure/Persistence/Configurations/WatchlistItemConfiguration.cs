using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockScreener.Domain.Entities;

namespace StockScreener.Infrastructure.Persistence.Configurations;

public class WatchlistItemConfiguration : IEntityTypeConfiguration<WatchlistItem>
{
    public void Configure(EntityTypeBuilder<WatchlistItem> builder)
    {
        builder.ToTable("WatchlistItems");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.SharesOwned).HasColumnType("decimal(18,4)");
        builder.Property(i => i.CostBasis).HasColumnType("decimal(18,4)");

        // Prevent duplicate stock entries per watchlist
        builder.HasIndex(i => new { i.WatchlistId, i.StockId })
            .IsUnique()
            .HasDatabaseName("IX_WatchlistItems_WatchlistId_StockId");
    }
}
