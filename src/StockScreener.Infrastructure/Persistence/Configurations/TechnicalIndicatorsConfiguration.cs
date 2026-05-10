using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockScreener.Domain.Entities;

namespace StockScreener.Infrastructure.Persistence.Configurations;

public class TechnicalIndicatorsConfiguration : IEntityTypeConfiguration<TechnicalIndicators>
{
    public void Configure(EntityTypeBuilder<TechnicalIndicators> builder)
    {
        builder.ToTable("TechnicalIndicators");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.RSI_14).HasColumnType("decimal(10,4)");
        builder.Property(t => t.SMA_20).HasColumnType("decimal(18,4)");
        builder.Property(t => t.SMA_50).HasColumnType("decimal(18,4)");
        builder.Property(t => t.SMA_200).HasColumnType("decimal(18,4)");
        builder.Property(t => t.MACD).HasColumnType("decimal(18,4)");
        builder.Property(t => t.MACD_Signal).HasColumnType("decimal(18,4)");
        builder.Property(t => t.MACD_Histogram).HasColumnType("decimal(18,4)");
        builder.Property(t => t.ATR_14).HasColumnType("decimal(18,4)");
        builder.Property(t => t.BB_Upper).HasColumnType("decimal(18,4)");
        builder.Property(t => t.BB_Middle).HasColumnType("decimal(18,4)");
        builder.Property(t => t.BB_Lower).HasColumnType("decimal(18,4)");

        // Composite index: stock + date queries
        builder.HasIndex(t => new { t.StockId, t.TradeDate })
            .HasDatabaseName("IX_TechnicalIndicators_StockId_Date");
    }
}
