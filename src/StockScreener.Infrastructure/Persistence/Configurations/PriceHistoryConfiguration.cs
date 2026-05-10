using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockScreener.Domain.Entities;

namespace StockScreener.Infrastructure.Persistence.Configurations;

public class PriceHistoryConfiguration : IEntityTypeConfiguration<PriceHistory>
{
    public void Configure(EntityTypeBuilder<PriceHistory> builder)
    {
        builder.ToTable("PriceHistory");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.OpenPrice).HasColumnType("decimal(18,4)");
        builder.Property(p => p.HighPrice).HasColumnType("decimal(18,4)");
        builder.Property(p => p.LowPrice).HasColumnType("decimal(18,4)");
        builder.Property(p => p.ClosePrice).HasColumnType("decimal(18,4)");

        // Composite index: stock + date queries
        builder.HasIndex(p => new { p.StockId, p.TradeDate })
            .HasDatabaseName("IX_PriceHistory_StockId_Date");

        // Ignore computed properties
        builder.Ignore(p => p.PriceChange);
        builder.Ignore(p => p.PriceChangePercent);
        builder.Ignore(p => p.Range);
    }
}
