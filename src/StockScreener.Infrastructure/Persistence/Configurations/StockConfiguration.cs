using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockScreener.Domain.Entities;
using StockScreener.Domain.Enums;

namespace StockScreener.Infrastructure.Persistence.Configurations;

public class StockConfiguration : IEntityTypeConfiguration<Stock>
{
    public void Configure(EntityTypeBuilder<Stock> builder)
    {
        builder.ToTable("Stocks");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Symbol)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(s => s.CompanyName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Exchange)
            .HasConversion<string>()
            .HasMaxLength(10);

        builder.Property(s => s.Sector).HasMaxLength(100);
        builder.Property(s => s.Industry).HasMaxLength(100);

        builder.Property(s => s.MarketCap).HasColumnType("decimal(20,2)");
        builder.Property(s => s.CurrentPrice).HasColumnType("decimal(18,4)");
        builder.Property(s => s.DayHigh).HasColumnType("decimal(18,4)");
        builder.Property(s => s.DayLow).HasColumnType("decimal(18,4)");
        builder.Property(s => s.Week52High).HasColumnType("decimal(18,4)");
        builder.Property(s => s.Week52Low).HasColumnType("decimal(18,4)");
        builder.Property(s => s.Beta).HasColumnType("decimal(10,4)");

        // Unique index on Symbol
        builder.HasIndex(s => s.Symbol)
            .IsUnique()
            .HasDatabaseName("IX_Stocks_Symbol");

        // Ignore computed property
        builder.Ignore(s => s.MarketCapCategory);

        // Relationships
        builder.HasOne(s => s.Fundamentals)
            .WithOne(f => f.Stock)
            .HasForeignKey<Fundamentals>(f => f.StockId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.PriceHistory)
            .WithOne(p => p.Stock)
            .HasForeignKey(p => p.StockId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.TechnicalIndicators)
            .WithOne(t => t.Stock)
            .HasForeignKey(t => t.StockId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
